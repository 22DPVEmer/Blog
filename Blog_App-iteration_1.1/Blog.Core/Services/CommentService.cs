using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Blog.Core.Constants;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ApplicationDbContext context,
            ILogger<CommentService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CommentViewModel>> GetArticleCommentsAsync(int articleId)
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.ArticleId == articleId && !c.IsBlocked)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                return comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserId = c.UserId,
                    UserName = $"{c.User.FirstName} {c.User.LastName}",
                    UserProfilePicture = c.User.ProfilePicture,
                    ArticleId = c.ArticleId,
                    ParentCommentId = c.ParentCommentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorRetrievingComments, articleId);
                throw;
            }
        }

        public async Task<CommentViewModel> GetCommentByIdAsync(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return null;
            }

            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                UserName = $"{comment.User.FirstName} {comment.User.LastName}",
                UserProfilePicture = comment.User.ProfilePicture,
                ArticleId = comment.ArticleId,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task<CommentViewModel> AddCommentAsync(CreateCommentViewModel model, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var (canComment, message) = await CanUserCommentAsync(user.Id);
            if (!canComment)
            {
                throw new UnauthorizedAccessException(message);
            }

            var article = await _context.Articles.FindAsync(model.ArticleId);
            if (article == null)
            {
                throw new ArgumentException(ArticleConstants.Messages.ArticleNotFound);
            }

            // Validate parent comment if provided
            if (model.ParentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(model.ParentCommentId.Value);
                if (parentComment == null)
                {
                    throw new ArgumentException(CommentConstants.Messages.CommentNotFound);
                }
                
                // Ensure parent comment belongs to the same article
                if (parentComment.ArticleId != model.ArticleId)
                {
                    throw new ArgumentException("Parent comment does not belong to the specified article");
                }
                
                // Prevent nesting beyond 2 levels
                if (parentComment.ParentCommentId.HasValue)
                {
                    throw new ArgumentException(CommentConstants.Messages.NestedCommentLimit);
                }
            }

            var comment = new Comment
            {
                Content = model.Content,
                ArticleId = model.ArticleId,
                UserId = user.Id,
                ParentCommentId = model.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                UserProfilePicture = user.ProfilePicture,
                ArticleId = comment.ArticleId,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task<CommentViewModel> UpdateCommentAsync(UpdateCommentViewModel model, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (comment == null)
            {
                throw new ArgumentException(CommentConstants.Messages.CommentNotFound);
            }

            // Check if user has permission to edit the comment
            if (comment.UserId != user.Id && !user.IsAdmin)
            {
                throw new UnauthorizedAccessException(CommentConstants.Messages.UnauthorizedCommentEdit);
            }

            comment.Content = model.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                UserName = $"{comment.User.FirstName} {comment.User.LastName}",
                UserProfilePicture = comment.User.ProfilePicture,
                ArticleId = comment.ArticleId,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task<bool> DeleteCommentAsync(int commentId, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return false;
            }

            // Check if user has permission to delete the comment
            if (comment.UserId != user.Id && !user.IsAdmin)
            {
                throw new UnauthorizedAccessException(CommentConstants.Messages.UnauthorizedCommentDelete);
            }

            // Check if this is a parent comment (has no ParentCommentId)
            bool isParentComment = comment.ParentCommentId == null;
            
            // If this is a parent comment, also remove all replies
            if (isParentComment)
            {
                // Find all child comments (replies) for this parent comment
                var childComments = await _context.Comments
                    .Where(c => c.ParentCommentId == commentId)
                    .ToListAsync();
                
                // Remove all child comments
                if (childComments.Any())
                {
                    _context.Comments.RemoveRange(childComments);
                }
            }

            // Hard delete the comment
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool canComment, string message)> CanUserCommentAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, PermissionConstants.Messages.UserNotFound);
            }

            if (!user.CanCommentArticles && !user.IsAdmin)
            {
                return (false, CommentConstants.Messages.NoCommentPermission);
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Reports a comment
        /// </summary>
        public async Task ReportCommentAsync(int commentId, string userId, string reason, string description)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException(CommentConstants.Messages.CommentNotFound);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException(PermissionConstants.Messages.UserNotFound);
            }

            // Don't allow users to report their own comments
            if (comment.UserId == userId)
            {
                throw new ArgumentException("Users cannot report their own comments");
            }

            // Check if this user has already reported this comment
            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.ReportedByUserId == userId);

            if (existingReport != null)
            {
                // Update existing report instead of creating a new one
                existingReport.Reason = reason;
                existingReport.Description = description;
                existingReport.IsResolved = false;
                existingReport.ResolvedAt = null;
            }
            else
            {
                // Create new report
                var report = new Report
                {
                    CommentId = commentId,
                    ReportedByUserId = userId,
                    Reason = reason,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                await _context.Reports.AddAsync(report);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Sets the block status of a comment and all its replies
        /// </summary>
        public async Task<CommentViewModel> SetCommentBlockStatusAsync(int commentId, bool isBlocked)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ArgumentException(CommentConstants.Messages.CommentNotFound);
            }

            // Update the block status of the parent comment
            comment.IsBlocked = isBlocked;
            
            // Find and update all replies to this comment if it's a parent comment
            if (comment.ParentCommentId == null)
            {
                var replies = await _context.Comments
                    .Where(c => c.ParentCommentId == commentId)
                    .ToListAsync();
                
                // Log the number of replies affected
                Console.WriteLine($"Updating block status for {replies.Count} replies to comment {commentId}");
                
                // Update each reply's block status to match the parent
                foreach (var reply in replies)
                {
                    reply.IsBlocked = isBlocked;
                }
            }
            
            await _context.SaveChangesAsync();

            // If blocking the comment, automatically resolve any reports
            if (isBlocked)
            {
                // Get reports for both the comment and its replies
                List<Report> reportsToResolve;
                
                if (comment.ParentCommentId == null)
                {
                    // If this is a parent comment, also get reports for all its replies
                    reportsToResolve = await _context.Reports
                        .Where(r => (r.CommentId == commentId || 
                                    _context.Comments
                                        .Where(c => c.ParentCommentId == commentId)
                                        .Select(c => c.Id)
                                        .Contains(r.CommentId.Value)) 
                                && !r.IsResolved)
                        .ToListAsync();
                }
                else
                {
                    // If this is a reply, just get reports for this comment
                    reportsToResolve = await _context.Reports
                        .Where(r => r.CommentId == commentId && !r.IsResolved)
                        .ToListAsync();
                }

                foreach (var report in reportsToResolve)
                {
                    report.IsResolved = true;
                    report.ResolvedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                UserName = $"{comment.User.FirstName} {comment.User.LastName}",
                UserProfilePicture = comment.User.ProfilePicture,
                ArticleId = comment.ArticleId,
                ParentCommentId = comment.ParentCommentId,
                IsBlocked = comment.IsBlocked
            };
        }

        /// <summary>
        /// Gets all reported comments for admin review
        /// </summary>
        public async Task<IEnumerable<ReportViewModel>> GetReportedCommentsAsync()
        {
            var reports = await _context.Reports
                .Include(r => r.ReportedByUser)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.User)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Article)
                .Where(r => r.CommentId.HasValue)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(r => new ReportViewModel
            {
                Id = r.Id,
                Reason = r.Reason,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                IsResolved = r.IsResolved,
                ResolvedAt = r.ResolvedAt,
                ReportedByUserId = r.ReportedByUserId,
                ReportedByUserName = $"{r.ReportedByUser.FirstName} {r.ReportedByUser.LastName}",
                ReportedByUserProfilePicture = r.ReportedByUser.ProfilePicture,
                CommentId = r.CommentId,
                CommentContent = r.Comment?.Content,
                CommentAuthorName = r.Comment != null ? $"{r.Comment.User.FirstName} {r.Comment.User.LastName}" : "",
                CommentAuthorProfilePicture = r.Comment?.User?.ProfilePicture,
                ArticleId = r.Comment?.ArticleId,
                ArticleTitle = r.Comment?.Article?.Title,
                IsParent = r.Comment?.ParentCommentId == null,
                IsBlocked = r.Comment?.IsBlocked ?? false
            });
        }
        
        /// <summary>
        /// Resolves or unresolves a report based on the isResolved parameter
        /// </summary>
        public async Task ResolveReportAsync(int reportId, bool isResolved = true)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                throw new ArgumentException("Report not found");
            }
            
            report.IsResolved = isResolved;
            
            if (isResolved)
            {
                report.ResolvedAt = DateTime.UtcNow;
            }
            else
            {
                report.ResolvedAt = null;
            }
            
            await _context.SaveChangesAsync();
        }
    }
}