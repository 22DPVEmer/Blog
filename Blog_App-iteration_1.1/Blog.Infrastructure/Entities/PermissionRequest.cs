using System;

namespace Blog.Infrastructure.Entities
{
    public class PermissionRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string Reason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedByUserId { get; set; }
        public virtual User ProcessedByUser { get; set; }
        public PermissionRequestStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public PermissionRequestType RequestType { get; set; }

        public PermissionRequest()
        {
            RequestedAt = DateTime.UtcNow;
            Status = PermissionRequestStatus.Pending;
            RequestType = PermissionRequestType.WriteArticle; // Default to WriteArticle for backward compatibility
        }
    }

    public enum PermissionRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum PermissionRequestType
    {
        WriteArticle,
        VoteArticle,
        CommentArticle
    }
}