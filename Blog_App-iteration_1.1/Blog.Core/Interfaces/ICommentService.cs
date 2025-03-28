using Blog.Core.Models;
using Blog.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Core.Interfaces
{
    public interface ICommentService
    {
        /// <summary>
        /// Gets all comments for an article
        /// </summary>
        Task<IEnumerable<CommentViewModel>> GetArticleCommentsAsync(int articleId);
        
        /// <summary>
        /// Gets a comment by id
        /// </summary>
        Task<CommentViewModel> GetCommentByIdAsync(int commentId);
        
        /// <summary>
        /// Adds a new comment to an article
        /// </summary>
        Task<CommentViewModel> AddCommentAsync(CreateCommentViewModel model, User user);
        
        /// <summary>
        /// Updates an existing comment
        /// </summary>
        Task<CommentViewModel> UpdateCommentAsync(UpdateCommentViewModel model, User user);
        
        /// <summary>
        /// Deletes a comment
        /// </summary>
        Task<bool> DeleteCommentAsync(int commentId, User user);
        
        /// <summary>
        /// Checks if a user can comment on articles
        /// </summary>
        Task<(bool canComment, string message)> CanUserCommentAsync(string userId);
        
        /// <summary>
        /// Reports a comment
        /// </summary>
        Task ReportCommentAsync(int commentId, string userId, string reason, string description);
        
        /// <summary>
        /// Sets the block status of a comment
        /// </summary>
        Task<CommentViewModel> SetCommentBlockStatusAsync(int commentId, bool isBlocked);
        
        /// <summary>
        /// Gets all reported comments for admin review
        /// </summary>
        Task<IEnumerable<ReportViewModel>> GetReportedCommentsAsync();
        
        /// <summary>
        /// Resolves or unresolves a report based on the isResolved parameter
        /// </summary>
        Task ResolveReportAsync(int reportId, bool isResolved = true);
    }
} 