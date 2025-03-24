using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Blog.Core.Models;
using Blog.Core.Constants;
using System.Collections.Generic;

namespace Blog.Web.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinArticleGroup(int articleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetArticleGroupName(articleId));
        }
        
        public async Task LeaveArticleGroup(int articleId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetArticleGroupName(articleId));
        }
        
        public async Task NotifyNewComment(int articleId, CommentViewModel comment)
        {
            await Clients.Group(GetArticleGroupName(articleId))
                .SendAsync(CommentConstants.EventNames.NewComment, comment);
        }
        
        public async Task NotifyUpdateComment(int articleId, CommentViewModel comment)
        {
            await Clients.Group(GetArticleGroupName(articleId))
                .SendAsync(CommentConstants.EventNames.UpdateComment, comment);
        }
        
        public async Task NotifyDeleteComment(int articleId, int commentId)
        {
            await Clients.Group(GetArticleGroupName(articleId))
                .SendAsync(CommentConstants.EventNames.DeleteComment, commentId);
        }
        
        public async Task NotifyDeleteCommentWithReplies(int articleId, int parentCommentId, List<int> replyIds)
        {
            // First notify about the parent comment deletion
            await NotifyDeleteComment(articleId, parentCommentId);
            
            // Then notify about each reply deletion
            foreach (var replyId in replyIds)
            {
                await NotifyDeleteComment(articleId, replyId);
            }
        }
        
        private string GetArticleGroupName(int articleId)
        {
            return $"article-{articleId}";
        }
    }
} 