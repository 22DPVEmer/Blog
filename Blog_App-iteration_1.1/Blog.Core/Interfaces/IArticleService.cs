using Blog.Core.Models;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Blog.Core.Constants;

namespace Blog.Core.Interfaces
{
public interface IArticleService
{
    Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, DateFilter dateFilter, string sortBy = null);
    Task<Article> GetArticleDetailsAsync(int id);
    Task<Article> CreateArticleAsync(ArticleCreateViewModel model, User user);
    Task<Article> UpdateArticleAsync(int id, ArticleCreateViewModel model, User user);
    Task<bool> DeleteArticleAsync(int id, User user);
    Task<string> UploadImageAsync(IFormFile file, bool isFeatured);
    
    // Article voting methods
    Task<bool> VoteArticleAsync(int articleId, string userId, bool isUpvote);
    Task<ArticleVote> GetUserVoteAsync(int articleId, string userId);

    // Article grouping methods
    Task<IEnumerable<Article>> GetLatestArticlesAsync(int count = 5);
    Task<IEnumerable<Article>> GetTopRankedArticlesAsync(int count = 3);
    Task<IEnumerable<Article>> GetRecentlyCommentedArticlesAsync(int count = 3);
}
}
