using Blog.Core.Interfaces;
using Blog.Core.Models;
using Blog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace Blog.Core.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ILogger<ArticleService> _logger;
        private const int MAX_TOTAL_SIZE = 10 * 1024 * 1024; // 10MB in bytes

        public ArticleService(ApplicationDbContext context, IFirebaseStorageService firebaseStorageService, ILogger<ArticleService> logger)
        {
            _context = context;
            _firebaseStorageService = firebaseStorageService;
            _logger = logger;
        }

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, string dateFilter)
        {
            var query = _context.Articles
                .Include(a => a.User)
                .Where(a => a.IsPublished)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    a.Title.ToLower().Contains(searchTerm) || 
                    a.Content.ToLower().Contains(searchTerm) ||
                    a.User.UserName.ToLower().Contains(searchTerm));
            }

            // Apply date filter
            var today = DateTime.UtcNow.Date;
            switch (dateFilter)
            {
                case "today":
                    query = query.Where(a => a.PublishedAt.Value.Date == today);
                    break;
                case "week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfWeek);
                    break;
                case "month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfMonth);
                    break;
                case "year":
                    var startOfYear = new DateTime(today.Year, 1, 1);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfYear);
                    break;
            }

            return await query.OrderByDescending(a => a.PublishedAt).ToListAsync();
        }

        public async Task<Article> GetArticleDetailsAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsPublished);
        }

        public async Task<Article> CreateArticleAsync(ArticleCreateViewModel model, User user)
        {
            // Handle featured image upload if provided
            string featuredImageUrl = null;
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(model.FeaturedImageFile.ContentType))
                {
                    throw new InvalidOperationException("Invalid file type. Only images are allowed.");
                }

                // Check file size (limit to 5MB)
                if (model.FeaturedImageFile.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("File size exceeds the 5MB limit.");
                }

                // Upload to Firebase Storage
                featuredImageUrl = await _firebaseStorageService.UploadImageAsync(model.FeaturedImageFile, "featured");
            }

            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                Intro = model.Intro,
                FeaturedImage = featuredImageUrl,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return article;
        }

        public async Task<Article> UpdateArticleAsync(int id, ArticleCreateViewModel model, User user)
        {
            var existingArticle = await _context.Articles.FindAsync(id);
            if (existingArticle == null)
            {
                throw new KeyNotFoundException("Article not found.");
            }

            if (existingArticle.UserId != user.Id)
            {
                throw new UnauthorizedAccessException("You don't have permission to edit this article.");
            }

            // Handle featured image upload if provided
            string featuredImageUrl = existingArticle.FeaturedImage;
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(model.FeaturedImageFile.ContentType))
                {
                    throw new InvalidOperationException("Invalid file type. Only images are allowed.");
                }

                // Check file size (limit to 5MB)
                if (model.FeaturedImageFile.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("File size exceeds the 5MB limit.");
                }

                // Delete old featured image if it exists
                if (!string.IsNullOrEmpty(existingArticle.FeaturedImage))
                {
                    try
                    {
                        await _firebaseStorageService.DeleteImageAsync(existingArticle.FeaturedImage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old featured image for article {ArticleId}", id);
                        // Continue with upload even if deletion fails
                    }
                }

                // Upload new image to Firebase Storage
                featuredImageUrl = await _firebaseStorageService.UploadImageAsync(model.FeaturedImageFile, "featured");
            }

            // Update article properties
            existingArticle.Title = model.Title;
            existingArticle.Content = model.Content;
            existingArticle.Intro = model.Intro;
            existingArticle.FeaturedImage = featuredImageUrl;
            existingArticle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingArticle;
        }

        public async Task<bool> DeleteArticleAsync(int id, User user)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }

            if (article.UserId != user.Id)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this article.");
            }

            // Delete the article's featured image from Firebase Storage if it exists
            if (!string.IsNullOrEmpty(article.FeaturedImage))
            {
                try
                {
                    await _firebaseStorageService.DeleteImageAsync(article.FeaturedImage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete featured image from storage for article {ArticleId}", id);
                    // Continue with article deletion even if image deletion fails
                }
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> UploadImageAsync(IFormFile file, bool isFeatured)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("No file was uploaded.");
            }

            // Check individual file size (5MB limit)
            const int maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException("Individual file size exceeds the maximum limit of 5MB.");
            }

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                throw new InvalidOperationException("Invalid file type. Only JPG, PNG, GIF, and WEBP images are allowed.");
            }

            // Determine subfolder based on image type
            string subfolder = isFeatured ? "featured" : "content";

            // Upload to Firebase Storage using the server-side service
            return await _firebaseStorageService.UploadImageAsync(file, subfolder);
        }
    }
}

