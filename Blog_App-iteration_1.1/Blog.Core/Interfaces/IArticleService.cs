using Blog.Core.Models;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;

namespace Blog.Core.Interfaces
{
public interface IArticleService
{
    Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, string dateFilter);
    Task<Article> GetArticleDetailsAsync(int id);
    Task<Article> CreateArticleAsync(ArticleCreateViewModel model, User user);
    Task<Article> UpdateArticleAsync(int id, ArticleCreateViewModel model, User user);
    Task<bool> DeleteArticleAsync(int id, User user);
    Task<string> UploadImageAsync(IFormFile file, bool isFeatured);
}
}
