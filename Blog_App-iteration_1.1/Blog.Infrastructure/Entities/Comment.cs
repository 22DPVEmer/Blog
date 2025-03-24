using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Blog.Infrastructure.Entities
{
    public class Comment
    {   
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        [Required]
        public int ArticleId { get; set; }
        
        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
        
        // Parent comment ID for nested comments (optional)
        public int? ParentCommentId { get; set; }
        
        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; }
        
        public bool IsDeleted { get; set; }
        
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