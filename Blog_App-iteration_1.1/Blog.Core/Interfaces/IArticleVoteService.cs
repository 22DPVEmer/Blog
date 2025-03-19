using Blog.Infrastructure.Entities;
using System.Threading.Tasks;

namespace Blog.Core.Interfaces
{
    public interface IArticleVoteService
    {
        /// <summary>
        /// Adds or updates a vote for an article
        /// </summary>
        Task<(bool success, string message, Article article)> VoteArticleAsync(int articleId, string userId, bool isUpvote);

        /// <summary>
        /// Removes a vote from an article
        /// </summary>
        Task<(bool success, string message, Article article)> RemoveVoteAsync(int articleId, string userId);

        /// <summary>
        /// Gets a user's vote for an article
        /// </summary>
        Task<ArticleVote> GetUserVoteAsync(int articleId, string userId);

        /// <summary>
        /// Checks if a user has permission to vote
        /// </summary>
        Task<(bool canVote, string message)> CanUserVoteAsync(string userId);
    }
} 