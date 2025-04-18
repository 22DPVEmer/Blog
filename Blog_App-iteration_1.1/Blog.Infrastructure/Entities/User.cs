using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Blog.Infrastructure.Entities
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
        public bool CanVoteArticles { get; set; }
        public bool CanCommentArticles { get; set; }
        public bool IsActive { get; set;}

        // Navigation properties
        public virtual ICollection<Article> Articles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<ArticleVote> ArticleVotes { get; set; }
        public virtual Rank Rank { get; set; }
        public int? RankId { get; set; }

        public User()
        {
            Articles = new HashSet<Article>();
            Comments = new HashSet<Comment>();
            Reports = new HashSet<Report>();
            ArticleVotes = new HashSet<ArticleVote>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            CanWriteArticles = false;
            CanVoteArticles = false;
            CanCommentArticles = false;
            IsAdmin = false;
        }
    }
}