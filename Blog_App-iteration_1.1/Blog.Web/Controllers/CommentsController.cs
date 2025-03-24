using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blog.Core.Constants;
using Blog.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Blog.Web.Controllers
{
    [Route("[controller]")]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CommentsController> _logger;
        private readonly IHubContext<CommentHub> _commentHubContext;

        public CommentsController(
            ICommentService commentService,
            UserManager<User> userManager,
            ILogger<CommentsController> logger,
            IHubContext<CommentHub> commentHubContext)
        {
            _commentService = commentService;
            _userManager = userManager;
            _logger = logger;
            _commentHubContext = commentHubContext;
        }

        [HttpGet("Article/{articleId}")]
        public async Task<IActionResult> GetArticleComments(int articleId)
        {
            try
            {
                var comments = await _commentService.GetArticleCommentsAsync(articleId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorRetrievingComments, articleId);
                return StatusCode(500, new { message = "An error occurred while retrieving comments." });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentViewModel model)
        {
            try
            {
                _logger.LogInformation("AddComment called with ArticleId: {ArticleId}, Content length: {ContentLength}", 
                    model?.ArticleId, model?.Content?.Length);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid ModelState in AddComment: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found in AddComment");
                    return Unauthorized(new { message = PermissionConstants.Messages.UserNotFound });
                }

                _logger.LogInformation("User {UserId} attempting to add comment", user.Id);
                var comment = await _commentService.AddCommentAsync(model, user);
                _logger.LogInformation("Comment added successfully with ID: {CommentId}", comment.Id);

                // Notify connected clients through SignalR
                await _commentHubContext.Clients.Group($"article-{model.ArticleId}")
                    .SendAsync(CommentConstants.EventNames.NewComment, comment);
                _logger.LogInformation("SignalR notification sent for new comment");

                return Ok(comment);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in AddComment");
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AddComment");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorAddingComment, model?.ArticleId);
                return StatusCode(500, new { message = "An error occurred while adding the comment." });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest(new { message = "Comment ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = PermissionConstants.Messages.UserNotFound });
                }

                var comment = await _commentService.UpdateCommentAsync(model, user);

                // Get the article ID for the updated comment
                var originalComment = await _commentService.GetCommentByIdAsync(id);
                int articleId = originalComment.ArticleId;

                // Notify connected clients through SignalR
                await _commentHubContext.Clients.Group($"article-{articleId}")
                    .SendAsync(CommentConstants.EventNames.UpdateComment, comment);

                return Ok(comment);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorUpdatingComment, id);
                return StatusCode(500, new { message = "An error occurred while updating the comment." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = PermissionConstants.Messages.UserNotFound });
                }

                // Get the comment before deleting
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    return NotFound(new { message = CommentConstants.Messages.CommentNotFound });
                }

                int articleId = comment.ArticleId;
                
                // Check if this is a parent comment with replies
                bool isParentComment = comment.ParentCommentId == null;
                List<int> replyIds = new List<int>();
                
                if (isParentComment)
                {
                    // Get all replies before deleting them
                    var allComments = await _commentService.GetArticleCommentsAsync(articleId);
                    replyIds = allComments
                        .Where(c => c.ParentCommentId == id)
                        .Select(c => c.Id)
                        .ToList();
                    
                    _logger.LogInformation("Deleting parent comment {CommentId} with {ReplyCount} replies", id, replyIds.Count);
                }
                
                var success = await _commentService.DeleteCommentAsync(id, user);
                if (!success)
                {
                    return NotFound(new { message = CommentConstants.Messages.CommentNotFound });
                }

                // Notify connected clients through SignalR
                if (isParentComment && replyIds.Count > 0)
                {
                    // Use our enhanced notification for deleting parent comments with replies
                    // First notify about the parent comment deletion
                    await _commentHubContext.Clients.Group($"article-{articleId}")
                        .SendAsync(CommentConstants.EventNames.DeleteComment, id);
                    
                    // Then notify about each reply deletion
                    foreach (var replyId in replyIds)
                    {
                        await _commentHubContext.Clients.Group($"article-{articleId}")
                            .SendAsync(CommentConstants.EventNames.DeleteComment, replyId);
                    }
                    
                    _logger.LogInformation("SignalR notification sent for parent comment {CommentId} and {ReplyCount} replies", id, replyIds.Count);
                }
                else
                {
                    // Use the standard method for single comment deletion
                    await _commentHubContext.Clients.Group($"article-{articleId}")
                        .SendAsync(CommentConstants.EventNames.DeleteComment, id);
                        
                    _logger.LogInformation("SignalR notification sent for comment deletion {CommentId}", id);
                }

                return Ok(new { message = CommentConstants.Messages.CommentDeleted });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorDeletingComment, id);
                return StatusCode(500, new { message = "An error occurred while deleting the comment." });
            }
        }
    }
} 