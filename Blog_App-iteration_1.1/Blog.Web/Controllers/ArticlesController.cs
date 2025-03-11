using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Entities;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Blog.Core.Services;
using Microsoft.AspNetCore.Http;
using Blog.Web.Models;
using System.IO;

namespace Blog.Web.Controllers
{
    [Route("[controller]")]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ArticlesController> _logger;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private const int MAX_TOTAL_SIZE = 10 * 1024 * 1024; // 10MB in bytes
        private const string TOTAL_SIZE_KEY = "ArticleTotalSize";

        public ArticlesController(
            ApplicationDbContext context, 
            UserManager<User> userManager,
            ILogger<ArticlesController> logger,
            IFirebaseStorageService firebaseStorageService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _firebaseStorageService = firebaseStorageService;
        }

        // GET: Articles - Allow all users to view articles
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index([FromQuery] string searchTerm, [FromQuery] string dateFilter)
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

            var articles = await query.OrderByDescending(a => a.PublishedAt).ToListAsync();
            return View(articles);
        }

        // GET: Articles/Details/5
        [HttpGet]
        [Route("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsPublished);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Articles/Create
        [HttpGet]
        [Route("Create")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Log the current directory and check for Firebase credentials file
                var currentDir = Directory.GetCurrentDirectory();
                _logger.LogInformation("Current directory: {CurrentDir}", currentDir);
                
                var credentialsFileName = "blogapp-248d7-firebase-adminsdk-fbsvc-167eb1a680.json";
                var possibleLocations = new[]
                {
                    Path.Combine(currentDir, credentialsFileName),
                    Path.Combine(currentDir, "wwwroot", credentialsFileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, credentialsFileName),
                    Path.Combine(Directory.GetParent(currentDir).FullName, "Blog.Core", credentialsFileName)
                };
                
                foreach (var location in possibleLocations)
                {
                    _logger.LogInformation("Checking for credentials at: {Location}, Exists: {Exists}", 
                        location, System.IO.File.Exists(location));
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET action");
                return View();
            }
        }

        // POST: Articles/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ArticleCreateViewModel model)
        {
            try
            {
                // Reset the total size counter when creating a new article
                HttpContext.Session.SetInt32(TOTAL_SIZE_KEY, 0);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )});
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Handle featured image upload if provided
                string featuredImageUrl = null;
                if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
                {
                    // Check file type
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(model.FeaturedImageFile.ContentType))
                    {
                        return BadRequest(new { message = "Invalid file type. Only images are allowed." });
                    }

                    // Check file size (limit to 5MB)
                    if (model.FeaturedImageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "File size exceeds the 5MB limit." });
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

                return Ok(new { id = article.Id, message = "Article created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article");
                return StatusCode(500, new { message = $"An error occurred while creating the article: {ex.Message}" });
            }
        }

        // GET: Articles/Edit/5
        [HttpGet]
        [Route("Edit/{id?}")]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || article.UserId != user.Id && !await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] ArticleCreateViewModel model)
        {
            try
            {
                var existingArticle = await _context.Articles.FindAsync(id);
                if (existingArticle == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null || existingArticle.UserId != user.Id && !await _userManager.IsInRoleAsync(user, "Administrator"))
                {
                    return BadRequest(new { message = "You don't have permission to edit this article." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )});
                }

                // Handle featured image upload if provided
                string featuredImageUrl = existingArticle.FeaturedImage;
                if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
                {
                    // Check file type
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(model.FeaturedImageFile.ContentType))
                    {
                        return BadRequest(new { message = "Invalid file type. Only images are allowed." });
                    }

                    // Check file size (limit to 5MB)
                    if (model.FeaturedImageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "File size exceeds the 5MB limit." });
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
                return Ok(new { message = "Article updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article {ArticleId}", id);
                return StatusCode(500, new { message = $"An error occurred while updating the article: {ex.Message}" });
            }
        }

        // GET: Articles/Delete/5
        [HttpGet]
        [Route("Delete/{id?}")]
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || article.UserId != user.Id && !await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost]
        [Route("Delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || article.UserId != user.Id && !await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
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
                TempData["SuccessMessage"] = "Article was successfully deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article {ArticleId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the article.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("UploadImage")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, bool isFeatured = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file was uploaded" });
                }

                // Get current total size from session
                int currentTotalSize = HttpContext.Session.GetInt32(TOTAL_SIZE_KEY) ?? 0;

                // Check if adding this file would exceed the total limit
                if (currentTotalSize + file.Length > MAX_TOTAL_SIZE)
                {
                    return BadRequest(new { 
                        message = $"Adding this image would exceed the total size limit of 10MB. Current total: {currentTotalSize / 1024 / 1024}MB",
                        currentSize = currentTotalSize,
                        maxSize = MAX_TOTAL_SIZE
                    });
                }

                // Check individual file size (5MB limit)
                const int maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = "Individual file size exceeds the maximum limit of 5MB" });
                }

                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, GIF, and WEBP images are allowed." });
                }

                // Determine subfolder based on image type
                string subfolder = isFeatured ? "featured" : "content";

                // Upload to Firebase Storage using the server-side service
                string imageUrl = await _firebaseStorageService.UploadImageAsync(file, subfolder);

                // Update total size in session
                HttpContext.Session.SetInt32(TOTAL_SIZE_KEY, currentTotalSize + (int)file.Length);

                return Ok(new { 
                    imageUrl,
                    currentSize = currentTotalSize + file.Length,
                    maxSize = MAX_TOTAL_SIZE
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = $"Error uploading image: {ex.Message}" });
            }
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}