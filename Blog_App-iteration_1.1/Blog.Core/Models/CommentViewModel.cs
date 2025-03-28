using System;
using System.ComponentModel.DataAnnotations;

namespace Blog.Core.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, ErrorMessage = "Comment must be between {2} and {1} characters", MinimumLength = 2)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string UserProfilePicture { get; set; }
        
        public int ArticleId { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        public bool IsBlocked { get; set; }
    }
    
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, ErrorMessage = "Comment must be between {2} and {1} characters", MinimumLength = 2)]
        public string Content { get; set; }
        
        [Required]
        public int ArticleId { get; set; }
        
        public int? ParentCommentId { get; set; }
    }
    
    public class UpdateCommentViewModel
    {
        [Required]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, ErrorMessage = "Comment must be between {2} and {1} characters", MinimumLength = 2)]
        public string Content { get; set; }
    }
} 