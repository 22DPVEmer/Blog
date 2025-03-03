using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Blog.Core.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanWriteArticles { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<Article> Articles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual Rank Rank { get; set; }
        public int? RankId { get; set; }

        public User()
        {
            Articles = new HashSet<Article>();
            Comments = new HashSet<Comment>();
            Reports = new HashSet<Report>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            CanWriteArticles = false; // Default to false, admin can grant this permission
        }
    }
} 