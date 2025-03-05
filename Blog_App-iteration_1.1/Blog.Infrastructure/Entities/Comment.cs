using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Blog.Infrastructure.Entities
{
    public class Comment
    {   
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        public int ArticleId { get; set; }
        public virtual Article Article { get; set; }
        
        public int? ParentCommentId { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; }
        public virtual ICollection<Report> Reports { get; set; }

        public Comment()
        {
            Replies = new HashSet<Comment>();
            Reports = new HashSet<Report>();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }
    }
} 