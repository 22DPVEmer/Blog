using System;

namespace Blog.Core.Models
{
    public class EmailQueueMessage
    {
        public required string Email { get; set; }
        public required string Subject { get; set; }
        public required string HtmlMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}