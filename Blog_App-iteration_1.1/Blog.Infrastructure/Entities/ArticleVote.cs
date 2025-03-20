using System;

namespace Blog.Infrastructure.Entities
{
    public class ArticleVote
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string UserId { get; set; }
        public bool IsUpvote { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Article Article { get; set; }
        public virtual User User { get; set; }

        public ArticleVote()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}