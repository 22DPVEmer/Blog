using Blog.Core.Interfaces;
using Blog.Core.Models;
using Blog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Blog.Core.Constants;

namespace Blog.Core.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ImageProcessingBackgroundService _imageProcessingService;
        private readonly ILogger<ArticleService> _logger;
        private readonly FileSettings _fileSettings;

        public ArticleService(
            ApplicationDbContext context,
            IFirebaseStorageService firebaseStorageService,
            ImageProcessingBackgroundService imageProcessingService,
            ILogger<ArticleService> logger,
            IOptions<FileSettings> fileSettings)
        {
            _context = context;
            _firebaseStorageService = firebaseStorageService;
            _imageProcessingService = imageProcessingService;
            _logger = logger;
            _fileSettings = fileSettings.Value;
        }

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, DateFilter dateFilter)
        {
            var query = _context.Articles
                .Include(a => a.User)
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
                case DateFilter.Today:
                    query = query.Where(a => a.PublishedAt.Value.Date == today);
                    break;
                case DateFilter.Week:
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfWeek);
                    break;
                case DateFilter.Month:
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfMonth);
                    break;
                case DateFilter.Year:
                    var startOfYear = new DateTime(today.Year, 1, 1);
                    query = query.Where(a => a.PublishedAt.Value.Date >= startOfYear);
                    break;
                case DateFilter.All:
                default:
                    // No date filter applied
                    break;
            }

            return await query.OrderByDescending(a => a.PublishedAt).ToListAsync();
        }

        public async Task<Article> GetArticleDetailsAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Article> CreateArticleAsync(ArticleCreateViewModel model, User user)
        {
            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                Intro = model.Intro,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Handle featured image upload if provided
            if (model.FeaturedImageFile != null)
            {
                try
                {
                    ValidateImageFile(model.FeaturedImageFile);
                    // Queue the featured image upload and wait for the URL
                    article.FeaturedImage = await _imageProcessingService.QueueImageUploadAsync(
                        model.FeaturedImageFile, 
                        ArticleConstants.ImageType.Featured, 
                        true,
                        article.Id);
                    await _context.SaveChangesAsync();
                }
                catch (ArgumentException)
                {
                    // If validation fails, just continue without setting the image
                }
            }

            return article;
        }

        public async Task<Article> UpdateArticleAsync(int id, ArticleCreateViewModel model, User user)
        {
            var existingArticle = await _context.Articles.FindAsync(id);

            if (existingArticle != null && existingArticle.UserId != user.Id)
            {
                throw new UnauthorizedAccessException(ArticleConstants.Messages.UnauthorizedEdit);
            }



            // Handle featured image upload if provided
            if (model.FeaturedImageFile != null)
            {
                try
                {
                    ValidateImageFile(model.FeaturedImageFile);

                    // Delete old featured image if it exists
                    if (!string.IsNullOrEmpty(existingArticle.FeaturedImage) && 
                        existingArticle.FeaturedImage != ArticleConstants.ImageStatus.Processing)
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

                    // Queue the new featured image upload and wait for the URL
                    existingArticle.FeaturedImage = await _imageProcessingService.QueueImageUploadAsync(
                        model.FeaturedImageFile, 
                        ArticleConstants.ImageType.Featured, 
                        true,
                        existingArticle.Id);
                }
                catch (ArgumentException)
                {
                    // If validation fails, just continue without setting the image
                }
            }

            // Update article properties
            existingArticle.Title = model.Title;
            existingArticle.Content = model.Content;
            existingArticle.Intro = model.Intro;
            existingArticle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingArticle;
        }

        public async Task<bool> DeleteArticleAsync(int id, User user)
        {
            // Get the article
            var article = await _context.Articles.FindAsync(id);
            
            // Check if user has permission to delete the article
            if (article != null && article.UserId != user.Id)
            {
                throw new UnauthorizedAccessException(ArticleConstants.Messages.UnauthorizedEdit);
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
            ValidateImageFile(file);

            // Determine subfolder based on image type
            string subfolder = isFeatured ? ArticleConstants.ImageType.Featured : ArticleConstants.ImageType.Content;

            // Queue the image upload and wait for the URL
            return await _imageProcessingService.QueueImageUploadAsync(file, subfolder, isFeatured);
        }

        private void ValidateImageFile(IFormFile file)
        {
            // Check if file is null or empty
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(ArticleConstants.Messages.ImageUploadError);
            }

            // Check file size
            if (file.Length > ArticleConstants.FileSize.MaxIndividualFileSize)
            {
                throw new ArgumentException(string.Format(
                    ArticleConstants.Messages.FileSizeExceeded, 
                    ArticleConstants.FileSize.MaxIndividualFileSize / (1024 * 1024)));
            }

            // Check file type
            if (string.IsNullOrEmpty(file.ContentType) || !_fileSettings.AllowedImageTypes.Contains(file.ContentType))
            {
                throw new ArgumentException($"Invalid file type. Allowed types are: {string.Join(", ", _fileSettings.AllowedImageTypes)}");
            }
        }
    }
}

