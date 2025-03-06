using Microsoft.AspNetCore.Identity.UI.Services;
using Blog.Core.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Blog.Web.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly ISharedEmailQueueService _emailQueueService;

        public EmailSenderService(ISharedEmailQueueService emailQueueService)
        {
            _emailQueueService = emailQueueService;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _emailQueueService.QueueEmail(email, subject, htmlMessage);
            
            // Return a completed task since we're not actually sending the email here
            return Task.CompletedTask;
        }
    }
}