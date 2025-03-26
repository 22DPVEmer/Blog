using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using Blog.Core.Constants;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Controllers
{
    [Route(CommentConstants.ApiRoutes.ControllerRoot)]
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
        public async Task<IActionResult> Index([FromQuery] string searchTerm, [FromQuery] string dateFilterStr = "All", [FromQuery] string sortBy = null)
        {
            try
            {
                // Parse the date filter string to enum, defaulting to All if parsing fails
                DateFilter dateFilter = DateFilter.All;
                if (!string.IsNullOrEmpty(dateFilterStr))
                {
                    if (Enum.TryParse<DateFilter>(dateFilterStr, true, out DateFilter parsedFilter))
                    {
                        dateFilter = parsedFilter;
                    }
                }

                // Get latest articles
                var latestArticles = await _articleService.GetLatestArticlesAsync();
                ViewBag.LatestArticles = latestArticles;

                // Get top ranked articles
                var topRankedArticles = await _articleService.GetTopRankedArticlesAsync();
                ViewBag.TopRankedArticles = topRankedArticles;

                // Get recently commented articles
                var recentlyCommentedArticles = await _articleService.GetRecentlyCommentedArticlesAsync();
                ViewBag.RecentlyCommentedArticles = recentlyCommentedArticles;

                // Get search results if search term is provided
                var searchResults = !string.IsNullOrEmpty(searchTerm) ?
                    await _articleService.GetPublishedArticlesAsync(searchTerm, dateFilter, sortBy) :
                    latestArticles;

                ViewBag.CurrentDateFilter = dateFilterStr; // Keep the current filter for the view
                ViewBag.CurrentSortBy = sortBy; // Keep the current sort for the view
                ViewBag.SearchTerm = searchTerm; // Keep the search term for the view

                return View(searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.RetrieveError, searchTerm, dateFilterStr);
                TempData["ErrorMessage"] = CommentConstants.ErrorMessages.ArticleRetrievalError;
                return RedirectToAction(CommentConstants.ApiRoutes.DefaultErrorAction, CommentConstants.ApiRoutes.DefaultHomeController);
            }
        }

        // GET: Articles/Details/5
        [HttpGet(CommentConstants.ApiRoutes.ArticleId)]
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

            // Get the current user's vote for this article if logged in
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserVote = await _articleService.GetUserVoteAsync(id.Value, user.Id);
                ViewBag.UserCanVoteArticles = user.CanVoteArticles || user.IsAdmin;
            }

            return View(article);
        }

        // GET: Articles/Create
        [HttpGet(CommentConstants.ApiRoutes.ArticleCreate)]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        [HttpPost(CommentConstants.ApiRoutes.ArticleCreate)]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ArticleCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.BadRequest, 
                        new { errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )}
                    );
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.Unauthorized, 
                        new { message = ArticleConstants.Messages.UserNotFound }
                    );
                }

                var article = await _articleService.CreateArticleAsync(model, user);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.Ok, 
                    new { 
                        id = article.Id, 
                        message = ArticleConstants.Messages.ArticleCreated 
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.CreateError);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.InternalServerError, 
                    new { message = ArticleConstants.Messages.CreateError }
                );
            }
        }

        // GET: Articles/Edit/5
        [HttpGet(CommentConstants.ApiRoutes.ArticleEdit + "/{id}")]
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
                TempData["ErrorMessage"] = CommentConstants.ErrorMessages.NoPermissionToEditArticle;
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost(CommentConstants.ApiRoutes.ArticleEdit + "/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromForm] ArticleCreateViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.Unauthorized, 
                        new { message = ArticleConstants.Messages.UserNotFound }
                    );
                }

                var article = await _articleService.UpdateArticleAsync(id, model, user);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.Ok, 
                    new { message = ArticleConstants.Messages.ArticleUpdated }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.UpdateError, id);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.InternalServerError, 
                    new { message = ArticleConstants.Messages.UpdateError }
                );
            }
        }

        // GET: Articles/Delete/5
        [HttpGet(CommentConstants.ApiRoutes.ArticleDelete + "/{id}")]
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
        [HttpPost(CommentConstants.ApiRoutes.ArticleDeleteConfirmed)]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.Unauthorized, 
                        new { message = ArticleConstants.Messages.UserNotFound }
                    );
                }

                var success = await _articleService.DeleteArticleAsync(id, user);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = CommentConstants.SuccessMessages.ArticleDeleted;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.DeleteError, id);
                TempData["ErrorMessage"] = CommentConstants.ErrorMessages.ArticleDeleteError;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost(CommentConstants.ApiRoutes.ArticleUploadImage)]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, bool isFeatured = false)
        {
            try
            {
                var imageUrl = await _articleService.UploadImageAsync(file, isFeatured);
                
                return StatusCode(
                    CommentConstants.HttpStatusCodes.Ok, 
                    new { 
                        message = CommentConstants.SuccessMessages.ImageUploaded,
                        status = CommentConstants.JsonPropertyNames.Success,
                        imageUrl = imageUrl
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogConstants.Articles.ImageUploadError);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.InternalServerError, 
                    new { message = ArticleConstants.Messages.ImageUploadError }
                );
            }
        }


    }
}