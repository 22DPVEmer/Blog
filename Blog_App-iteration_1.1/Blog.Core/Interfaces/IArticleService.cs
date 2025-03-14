using Blog.Core.Models;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Blog.Core.Constants;

namespace Blog.Core.Interfaces
{
public interface IArticleService
{
    Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, DateFilter dateFilter);
    Task<Article> GetArticleDetailsAsync(int id);
    Task<Article> CreateArticleAsync(ArticleCreateViewModel model, User user);
    Task<Article> UpdateArticleAsync(int id, ArticleCreateViewModel model, User user);
    Task<bool> DeleteArticleAsync(int id, User user);
    Task<string> UploadImageAsync(IFormFile file, bool isFeatured);
}
}
