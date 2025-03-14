using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Blog.Core.Models;
using Blog.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Blog.Infrastructure.Data;

namespace Blog.Core.Services
{
    public class ImageProcessingBackgroundService : BackgroundService
    {
        private readonly ILogger<ImageProcessingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Channel<ImageProcessingJob> _channel;
        private readonly Dictionary<Guid, TaskCompletionSource<string>> _pendingUploads;

        public ImageProcessingBackgroundService(
            ILogger<ImageProcessingBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _channel = Channel.CreateUnbounded<ImageProcessingJob>();
            _pendingUploads = new Dictionary<Guid, TaskCompletionSource<string>>();
        }

        public async Task<string> QueueImageUploadAsync(IFormFile file, string subFolder, bool isFeatured, int? articleId = null)
        {
            var jobId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<string>();
            _pendingUploads[jobId] = tcs;

            // Save to temp file
            var tempPath = Path.Combine(Path.GetTempPath(), $"{jobId}{Path.GetExtension(file.FileName)}");
            using (var stream = File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            var job = new ImageProcessingJob
            {
                Id = jobId,
                TempFilePath = tempPath,
                SubFolder = subFolder,
                IsFeatured = isFeatured,
                ArticleId = articleId,
                ImageType = isFeatured ? "featured" : "content",
                OnComplete = async (url, articleId) => 
                {
                    if (articleId.HasValue)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var article = await dbContext.Articles.FindAsync(articleId.Value);
                        if (article != null)
                        {
                            article.FeaturedImage = url;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            };

            await _channel.Writer.WriteAsync(job);
            
            // Return the URL once processing is complete
            return await tcs.Task;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _channel.Reader.ReadAsync(stoppingToken);
                    string uploadedUrl = null;

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var firebaseStorage = scope.ServiceProvider.GetRequiredService<IFirebaseStorageService>();

                        using var fileStream = File.OpenRead(job.TempFilePath);
                        var formFile = new FormFile(
                            fileStream,
                            0,
                            fileStream.Length,
                            "image",
                            Path.GetFileName(job.TempFilePath)
                        );

                        uploadedUrl = await firebaseStorage.UploadImageAsync(formFile, job.SubFolder);
                        
                        // Execute the callback if provided
                        job.OnComplete?.Invoke(uploadedUrl, job.ArticleId);

                        // Complete the task completion source
                        if (_pendingUploads.TryGetValue(job.Id, out var tcs))
                        {
                            tcs.SetResult(uploadedUrl);
                            _pendingUploads.Remove(job.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing image job {JobId}", job.Id);
                        if (_pendingUploads.TryGetValue(job.Id, out var tcs))
                        {
                            tcs.SetException(ex);
                            _pendingUploads.Remove(job.Id);
                        }
                    }
                    finally
                    {
                        // Cleanup
                        if (File.Exists(job.TempFilePath))
                        {
                            File.Delete(job.TempFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background service");
                }
            }
        }
    }
}
