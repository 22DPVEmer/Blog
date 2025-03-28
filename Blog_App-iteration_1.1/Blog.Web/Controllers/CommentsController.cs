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
using Blog.Web.Models;

namespace Blog.Web.Controllers
{
    [Route(CommentConstants.ApiRoutes.ControllerRoot)]
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

        [HttpGet(CommentConstants.ApiRoutes.ArticleByIdFormat)]
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
                return StatusCode(CommentConstants.HttpStatusCodes.InternalServerError, new { message = CommentConstants.ErrorMessages.InternalServerError });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = PermissionConstants.Messages.UserNotFound });
                }

                var comment = await _commentService.AddCommentAsync(model, user);

                // Notify connected clients through SignalR
                try {
                    await _commentHubContext.Clients.Group(string.Format(CommentConstants.HubConstants.ArticleGroupNameFormat, model.ArticleId))
                        .SendAsync(CommentConstants.EventNames.NewComment, comment);
                } catch (Exception ex) {
                    _logger.LogError(ex, CommentConstants.LogMessages.SignalRNotificationError, CommentConstants.SignalRNotificationTypes.NewComment);
                }

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
                _logger.LogError(ex, CommentConstants.LogMessages.ErrorAddingComment, model?.ArticleId);
                return StatusCode(CommentConstants.HttpStatusCodes.InternalServerError, new { message = CommentConstants.ErrorMessages.CommentAddError });
            }
        }

        [HttpPut(CommentConstants.ApiRoutes.CommentById)]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest(new { message = CommentConstants.ErrorMessages.CommentIdMismatch });
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
                try {
                    await _commentHubContext.Clients.Group(string.Format(CommentConstants.HubConstants.ArticleGroupNameFormat, articleId))
                        .SendAsync(CommentConstants.EventNames.UpdateComment, comment);
                } catch (Exception ex) {
                    _logger.LogError(ex, CommentConstants.LogMessages.SignalRNotificationError, CommentConstants.SignalRNotificationTypes.CommentUpdate);
                }

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
                return StatusCode(CommentConstants.HttpStatusCodes.InternalServerError, new { message = CommentConstants.ErrorMessages.CommentUpdateError });
            }
        }

        [HttpDelete(CommentConstants.ApiRoutes.CommentById)]
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
                }
                
                var success = await _commentService.DeleteCommentAsync(id, user);
                if (!success)
                {
                    return NotFound(new { message = CommentConstants.Messages.CommentNotFound });
                }

                // Notify connected clients through SignalR
                try {
                    string groupName = string.Format(CommentConstants.HubConstants.ArticleGroupNameFormat, articleId);
                    
                    // First notify about the parent comment
                    await _commentHubContext.Clients.Group(groupName)
                        .SendAsync(CommentConstants.EventNames.DeleteComment, id);
                    
                    // If this is a parent comment with replies, notify about each reply deletion too
                    if (isParentComment && replyIds.Count > 0) {
                        // Then notify about each reply deletion
                        foreach (var replyId in replyIds) {
                            await _commentHubContext.Clients.Group(groupName)
                                .SendAsync(CommentConstants.EventNames.DeleteComment, replyId);
                        }
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, CommentConstants.LogMessages.SignalRNotificationError, CommentConstants.SignalRNotificationTypes.CommentDeletion);
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
                return StatusCode(CommentConstants.HttpStatusCodes.InternalServerError, new { message = CommentConstants.ErrorMessages.CommentDeleteError });
            }
        }

        // Report and Block endpoints
        [HttpPost(CommentConstants.ApiRoutes.Report)]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportComment([FromBody] Models.CommentReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            
            var comment = await _commentService.GetCommentByIdAsync(model.CommentId);
            if (comment == null)
            {
                return NotFound();
            }
            
            // Don't allow users to report their own comments
            if (comment.UserId == user.Id)
            {
                return BadRequest(CommentConstants.ErrorMessages.CannotReportOwnComment);
            }
            
            await _commentService.ReportCommentAsync(model.CommentId, user.Id, model.Reason, model.Description);
            
            return Ok();
        }
        
        // POST: api/Comments/Block
        [HttpPost(CommentConstants.ApiRoutes.Block)]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<CommentViewModel>> BlockComment([FromBody] Models.CommentBlockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsAdmin)
            {
                return Forbid();
            }
            
            var comment = await _commentService.GetCommentByIdAsync(model.CommentId);
            if (comment == null)
            {
                return NotFound();
            }
            
            // Set the block status of the comment and all its replies
            var updatedComment = await _commentService.SetCommentBlockStatusAsync(model.CommentId, model.IsBlocked);
            
            // Get article ID for SignalR notifications
            int articleId = updatedComment.ArticleId;
            string groupName = string.Format(CommentConstants.HubConstants.ArticleGroupNameFormat, articleId);
            
            // Notify about the main comment update
            await _commentHubContext.Clients.Group(groupName)
                .SendAsync(CommentConstants.EventNames.UpdateComment, updatedComment);
            
            // If this is a parent comment, also get and notify about all replies
            if (updatedComment.ParentCommentId == null)
            {
                // Get all replies to this comment
                var allComments = await _commentService.GetArticleCommentsAsync(articleId);
                var replies = allComments.Where(c => c.ParentCommentId == updatedComment.Id).ToList();
                
                _logger.LogInformation(CommentConstants.LogMessages.RepliesUpdated, replies.Count, updatedComment.Id);
                
                // Notify about each reply update
                foreach (var reply in replies)
                {
                    // Ensure reply has same blocked status as parent
                    reply.IsBlocked = updatedComment.IsBlocked;
                    
                    await _commentHubContext.Clients.Group(groupName)
                        .SendAsync(CommentConstants.EventNames.UpdateComment, reply);
                }
            }
            
            return Ok(updatedComment);
        }
    }
}