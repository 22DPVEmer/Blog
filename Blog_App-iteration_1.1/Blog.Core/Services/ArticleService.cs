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

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync(string searchTerm, DateFilter dateFilter, string sortBy = null)
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

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case VoteConstants.SortOptions.Popular:
                        query = query.OrderByDescending(a => a.UpvoteCount - a.DownvoteCount);
                        break;
                    case VoteConstants.SortOptions.MostUpvoted:
                        query = query.OrderByDescending(a => a.UpvoteCount);
                        break;
                    case VoteConstants.SortOptions.MostDownvoted:
                        query = query.OrderByDescending(a => a.DownvoteCount);
                        break;
                    case VoteConstants.SortOptions.Newest:
                    default:
                        query = query.OrderByDescending(a => a.PublishedAt);
                        break;
                }
            }
            else
            {
                // Default sort by publish date
                query = query.OrderByDescending(a => a.PublishedAt);
            }

            return await query.ToListAsync();
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

            // Handle featured image upload if provided and not null
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
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

            if (existingArticle == null)
            {
                throw new ArgumentException(ArticleConstants.Messages.ArticleNotFound);
            }

            if (existingArticle.UserId != user.Id)
            {
                throw new UnauthorizedAccessException(ArticleConstants.Messages.UnauthorizedEdit);
            }

            // Handle featured image upload if provided and not null
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
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
                            _logger.LogWarning(ex, ArticleConstants.LoggerMessages.FailedToDeleteFeaturedImage, id);
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
            
            if (article == null)
            {
                return false;
            }

            // Check if user has permission to delete the article
            if (article.UserId != user.Id)
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
                    _logger.LogWarning(ex, ArticleConstants.LoggerMessages.FailedToDeleteStorageImage, id);
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
                throw new ArgumentException(ArticleConstants.Messages.ImageUploadError);
            }
            
            ValidateImageFile(file);

            // Determine subfolder based on image type
            string subfolder = isFeatured ? ArticleConstants.ImageType.Featured : ArticleConstants.ImageType.Content;

            // Queue the image upload and wait for the URL
            return await _imageProcessingService.QueueImageUploadAsync(file, subfolder, isFeatured);
        }

        private void ValidateImageFile(IFormFile file)
        {
            // Return early if file is null - no validation needed
            if (file == null)
            {
                return;
            }
            
            // Check if file is empty
            if (file.Length == 0)
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
                throw new ArgumentException(string.Format(
                    ArticleConstants.Messages.InvalidFileType, 
                    string.Join(", ", _fileSettings.AllowedImageTypes)));
            }
        }

        // Implement the voting methods
        public async Task<bool> VoteArticleAsync(int articleId, string userId, bool isUpvote)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException(PermissionConstants.Messages.UserNotFound);
            }

            if (!user.CanVoteArticles && !user.IsAdmin)
            {
                throw new UnauthorizedAccessException(PermissionConstants.Messages.UnauthorizedVote);
            }

            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new ArgumentException(VoteConstants.Messages.ArticleNotFound);
            }

            // Check if user already voted for this article
            var existingVote = await _context.ArticleVotes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);

            if (existingVote != null)
            {
                // If same vote type, remove the vote (toggle off)
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Update article vote counts
                    if (existingVote.IsUpvote)
                    {
                        article.UpvoteCount = Math.Max(0, article.UpvoteCount - 1);
                    }
                    else
                    {
                        article.DownvoteCount = Math.Max(0, article.DownvoteCount - 1);
                    }

                    _context.ArticleVotes.Remove(existingVote);
                    await _context.SaveChangesAsync();
                    return true;
                }
                
                // If vote type changed, update counts
                if (existingVote.IsUpvote != isUpvote)
                {
                    if (isUpvote)
                    {
                        // Changed from downvote to upvote
                        article.DownvoteCount--;
                        article.UpvoteCount++;
                    }
                    else
                    {
                        // Changed from upvote to downvote
                        article.UpvoteCount--;
                        article.DownvoteCount++;
                    }
                    existingVote.IsUpvote = isUpvote;
                }
            }
            else
            {
                // Create new vote
                var vote = new ArticleVote
                {
                    ArticleId = articleId,
                    UserId = userId,
                    IsUpvote = isUpvote,
                    CreatedAt = DateTime.UtcNow
                };

                // Update article vote counts
                if (isUpvote)
                {
                    article.UpvoteCount++;
                }
                else
                {
                    article.DownvoteCount++;
                }

                await _context.ArticleVotes.AddAsync(vote);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ArticleVote> GetUserVoteAsync(int articleId, string userId)
        {
            return await _context.ArticleVotes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);
        }
    }
}

