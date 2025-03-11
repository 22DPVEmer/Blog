using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Blog.Infrastructure.Entities
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        
        [Required(ErrorMessage = "Intro is required")]
        [StringLength(200, ErrorMessage = "Intro cannot be longer than 200 characters")]
        public string Intro { get; set; }
        
        public string? FeaturedImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Report> Reports { get; set; }

        public Article()
        {
            Comments = new HashSet<Comment>();
            Reports = new HashSet<Report>();
            CreatedAt = DateTime.UtcNow;
            IsPublished = false;
        }
    }
} 