using Blog.Core.Models;
namespace Blog.Core.Interfaces
{
      public interface ISharedEmailQueueService
    {
        void QueueEmail(string email, string subject, string htmlMessage);
        bool TryDequeueEmail(out EmailQueueMessage emailMessage);
    }  
};
