using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Blog.Core.Interfaces;
using Blog.EmailWorkerService.Constants;
using System.Globalization;
namespace Blog.EmailWorkerService
{
    public class Worker(ILogger<Worker> logger, ISharedEmailQueueService emailQueueService, IEmailSender emailSender) : BackgroundService
    {
  

        private readonly ILogger<Worker> _logger = logger;
        private readonly ISharedEmailQueueService _emailQueueService = emailQueueService;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly HashSet<string> _processedEmails = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(WorkerConstants.ServiceStartedMessage, DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug(WorkerConstants.CycleStartMessage, DateTimeOffset.Now);
                await ProcessEmailQueue(stoppingToken);
                _logger.LogDebug(WorkerConstants.CycleCompleteMessage, DateTimeOffset.Now);
                
                await Task.Delay(WorkerConstants.QueueCheckDelay, stoppingToken);
            }
        }

        private async Task ProcessEmailQueue(CancellationToken stoppingToken)
        {
            try
            {
                int processedCount = 0;
                int failedCount = 0;
                
                _logger.LogDebug(WorkerConstants.QueueProcessingStartMessage, DateTimeOffset.Now);
                
                while (_emailQueueService.TryDequeueEmail(out var emailRequest) && !stoppingToken.IsCancellationRequested)
                {
                    string emailKey = $"{emailRequest.Email}:{emailRequest.Subject}:{emailRequest.CreatedAt:yyyy-MM-dd HH:mm:ss}";
                    
                    if (_processedEmails.Contains(emailKey))
                    {
                        continue;
                    }
                    
                    _logger.LogDebug(WorkerConstants.ProcessingEmailMessage, 
                        emailRequest.Email, emailRequest.Subject);
                        
                    try
                    {
                        await _emailSender.SendEmailAsync(
                            emailRequest.Email,
                            emailRequest.Subject,
                            emailRequest.HtmlMessage);
                        
                        processedCount++;
                        _processedEmails.Add(emailKey);
                        _logger.LogInformation(WorkerConstants.EmailSentSuccessMessage, 
                            emailRequest.Email, emailRequest.Subject);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, WorkerConstants.EmailSendFailureMessage, 
                            emailRequest.Email, emailRequest.Subject, ex.Message);
                    }
                }

                var cutoffTime = DateTime.UtcNow.Subtract(WorkerConstants.EmailRetentionPeriod);
                _processedEmails.RemoveWhere(key => 
                    DateTime.TryParseExact(key.Split(WorkerConstants.EmailKeyDateSeparator)[WorkerConstants.EmailKeyDateIndex], WorkerConstants.EmailDateFormats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) && 
                    parsedDate < cutoffTime);

                _logger.LogInformation(
                    WorkerConstants.QueueProcessingCompleteMessage, 
                    processedCount, failedCount, processedCount + failedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, WorkerConstants.CriticalErrorMessage, ex.Message);
            }
        }
    }
}