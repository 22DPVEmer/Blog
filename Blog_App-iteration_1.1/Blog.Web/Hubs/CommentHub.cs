using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Blog.Core.Models;
using Blog.Core.Constants;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace Blog.Web.Hubs
{
    public class CommentHub : Hub
    {
        private readonly ILogger<CommentHub> _logger;

        public CommentHub(ILogger<CommentHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug(CommentConstants.HubConstants.ClientConnected, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            _logger.LogDebug(CommentConstants.HubConstants.ClientDisconnected, Context.ConnectionId);
            if (exception != null)
            {
                _logger.LogError(exception, CommentConstants.HubConstants.ClientDisconnectedWithError, Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinArticleGroup(int articleId)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidArticleIdProvided, 
                        articleId, 
                        CommentConstants.HubConstants.Methods.JoinArticleGroup);
                    throw new ArgumentException(CommentConstants.HubConstants.InvalidArticleId);
                }

                string groupName = GetArticleGroupName(articleId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod,
                    CommentConstants.HubConstants.Methods.JoinArticleGroup,
                    articleId);
                throw;
            }
        }
        
        public async Task LeaveArticleGroup(int articleId)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidArticleIdProvided, 
                        articleId, 
                        CommentConstants.HubConstants.Methods.LeaveArticleGroup);
                    return;
                }

                string groupName = GetArticleGroupName(articleId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod, 
                    CommentConstants.HubConstants.Methods.LeaveArticleGroup, 
                    articleId);
            }
        }
        
        public async Task NotifyNewComment(int articleId, CommentViewModel comment)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId || comment == null)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidParameters, 
                        CommentConstants.HubConstants.Methods.NotifyNewComment, 
                        articleId);
                    return;
                }

                string groupName = GetArticleGroupName(articleId);
                await Clients.Group(groupName).SendAsync(CommentConstants.EventNames.NewComment, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod, 
                    CommentConstants.HubConstants.Methods.NotifyNewComment, 
                    articleId);
            }
        }
        
        public async Task NotifyUpdateComment(int articleId, CommentViewModel comment)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId || comment == null)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidParameters, 
                        CommentConstants.HubConstants.Methods.NotifyUpdateComment, 
                        articleId);
                    return;
                }

                string groupName = GetArticleGroupName(articleId);
                await Clients.Group(groupName).SendAsync(CommentConstants.EventNames.UpdateComment, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod, 
                    CommentConstants.HubConstants.Methods.NotifyUpdateComment, 
                    articleId);
            }
        }
        
        public async Task NotifyDeleteComment(int articleId, int commentId)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId || commentId < CommentConstants.HubConstants.MinValidId)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidParameters, 
                        CommentConstants.HubConstants.Methods.NotifyDeleteComment, 
                        articleId);
                    return;
                }

                string groupName = GetArticleGroupName(articleId);
                await Clients.Group(groupName).SendAsync(CommentConstants.EventNames.DeleteComment, commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod, 
                    CommentConstants.HubConstants.Methods.NotifyDeleteComment, 
                    articleId);
            }
        }
        
        public async Task NotifyDeleteCommentWithReplies(int articleId, int parentCommentId, List<int> replyIds)
        {
            try
            {
                if (articleId < CommentConstants.HubConstants.MinValidId || 
                    parentCommentId < CommentConstants.HubConstants.MinValidId || 
                    replyIds == null)
                {
                    _logger.LogWarning(
                        CommentConstants.HubConstants.InvalidParameters, 
                        CommentConstants.HubConstants.Methods.NotifyDeleteCommentWithReplies, 
                        articleId);
                    return;
                }

                _logger.LogDebug(CommentConstants.HubConstants.NotifyingAboutParentDeletion, replyIds.Count);
                
                // First notify about the parent comment deletion
                await NotifyDeleteComment(articleId, parentCommentId);
                
                // Then notify about each reply deletion
                foreach (var replyId in replyIds)
                {
                    await NotifyDeleteComment(articleId, replyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    CommentConstants.HubConstants.ErrorInMethod, 
                    CommentConstants.HubConstants.Methods.NotifyDeleteCommentWithReplies, 
                    articleId);
            }
        }
        
        private string GetArticleGroupName(int articleId)
        {
            return string.Format(CommentConstants.HubConstants.ArticleGroupNameFormat, articleId);
        }
    }
} 