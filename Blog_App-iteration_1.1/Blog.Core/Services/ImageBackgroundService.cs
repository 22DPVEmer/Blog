using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Blog.Core.Models;
using Blog.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Blog.Core.Services
{
    public class ImageProcessingBackgroundService : BackgroundService
    {
        private readonly ILogger<ImageProcessingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Channel<ImageProcessingJob> _channel;

        public ImageProcessingBackgroundService(
            ILogger<ImageProcessingBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _channel = Channel.CreateUnbounded<ImageProcessingJob>();
        }

        public async Task QueueImageUploadAsync(IFormFile file, string subFolder = "", bool isFeatured = false)
        {
            // Save to temp file
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            using (var stream = File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            await _channel.Writer.WriteAsync(new ImageProcessingJob
            {
                TempFilePath = tempPath,
                SubFolder = subFolder,
                IsFeatured = isFeatured
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _channel.Reader.ReadAsync(stoppingToken);
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

                    await firebaseStorage.UploadImageAsync(formFile, job.SubFolder);

                    // Cleanup
                    if (File.Exists(job.TempFilePath))
                    {
                        File.Delete(job.TempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image job");
                }
            }
        }
    }
}
