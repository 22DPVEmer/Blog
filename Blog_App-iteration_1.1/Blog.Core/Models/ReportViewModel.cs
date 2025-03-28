using System;

namespace Blog.Core.Models
{
    public class ReportViewModel
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        // User who reported
        public string ReportedByUserId { get; set; }
        public string ReportedByUserName { get; set; }
        public string ReportedByUserProfilePicture { get; set; }
        
        // Comment details if it's a comment report
        public int? CommentId { get; set; }
        public string CommentContent { get; set; }
        public string CommentAuthorName { get; set; }
        public string CommentAuthorProfilePicture { get; set; }
        public int? ArticleId { get; set; }
        public string ArticleTitle { get; set; }
        
        // Indicates if this is a parent comment (without a parent itself)
        public bool IsParent { get; set; }
        
        // Indicates if the comment is blocked
        public bool IsBlocked { get; set; }
    }
} 