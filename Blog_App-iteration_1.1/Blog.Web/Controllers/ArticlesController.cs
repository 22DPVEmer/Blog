using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using Blog.Core.Constants;

namespace Blog.Web.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticleService articleService,
            UserManager<User> userManager,
            ILogger<ArticlesController> logger)
        {
            _articleService = articleService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Articles - Allow all users to view articles
        public async Task<IActionResult> Index([FromQuery] string searchTerm, [FromQuery] string dateFilter)
        {
            try
            {
                var articles = await _articleService.GetPublishedArticlesAsync(searchTerm, dateFilter);
                return View(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.RetrieveError, searchTerm, dateFilter);
                TempData["ErrorMessage"] = "An error occurred while retrieving articles.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleDetailsAsync(id.Value);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Articles/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ArticleCreateViewModel model)
        {
            try
            {
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
                    return Unauthorized(new { message = ArticleConstants.Messages.UserNotFound });
                }

                var article = await _articleService.CreateArticleAsync(model, user);
                return Ok(new { id = article.Id, message = ArticleConstants.Messages.ArticleCreated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.CreateError);
                return StatusCode(500, new { message = $"An error occurred while creating the article: {ex.Message}" });
            }
        }

        // GET: Articles/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleDetailsAsync(id.Value);
            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || article.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] ArticleCreateViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var article = await _articleService.UpdateArticleAsync(id, model, user);
                return Ok(new { message = "Article updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.UpdateError, id);
                return StatusCode(500, new { message = $"An error occurred while updating the article: {ex.Message}" });
            }
        }

        // GET: Articles/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleDetailsAsync(id.Value);
            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || article.UserId != user.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var success = await _articleService.DeleteArticleAsync(id, user);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Article was successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.DeleteError, id);
                TempData["ErrorMessage"] = "An error occurred while deleting the article.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, bool isFeatured = false)
        {
            try
            {
                var result = await _articleService.UploadImageAsync(file, isFeatured);
                
                // Since the image is now processed in the background, we return a success message
                // with a placeholder or temporary URL
                return Ok(new { 
                    message = "Image upload has been queued for processing",
                    status = "processing",
                    // You might want to generate a temporary URL or use a placeholder image
                    imageUrl = "/images/placeholder-processing.jpg" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.ImageUploadError);
                return StatusCode(500, new { message = $"{ArticleConstants.Messages.ImageUploadError}: {ex.Message}" });
            }
        }
    }
}