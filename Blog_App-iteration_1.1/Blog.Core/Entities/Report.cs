using System;
using System.ComponentModel.DataAnnotations;
namespace Blog.Core.Entities
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public string ReportedByUserId { get; set; }
        public virtual User ReportedByUser { get; set; }

        public int? ArticleId { get; set; }
        public virtual Article Article { get; set; }

        public int? CommentId { get; set; }
        public virtual Comment Comment { get; set; }

        public Report()
        {
            CreatedAt = DateTime.UtcNow;
            IsResolved = false;
        }
    }
} 