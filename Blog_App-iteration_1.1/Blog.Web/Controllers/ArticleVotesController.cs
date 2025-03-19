using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ArticleVotesController : Controller
    {
        private readonly IArticleVoteService _articleVoteService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ArticleVotesController> _logger;

        public ArticleVotesController(
            IArticleVoteService articleVoteService,
            UserManager<User> userManager,
            ILogger<ArticleVotesController> logger)
        {
            _articleVoteService = articleVoteService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Vote/{articleId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int articleId, bool isUpvote)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                // Check if user has permission to vote
                var (canVote, message) = await _articleVoteService.CanUserVoteAsync(user.Id);
                if (!canVote)
                {
                    return Forbid();
                }

                // Process the vote
                var (success, voteMessage, article) = await _articleVoteService.VoteArticleAsync(articleId, user.Id, isUpvote);
                
                if (!success)
                {
                    return NotFound(new { success = false, message = voteMessage });
                }

                return Json(new { 
                    success = true, 
                    upvotes = article.UpvoteCount, 
                    downvotes = article.DownvoteCount,
                    score = article.UpvoteCount - article.DownvoteCount
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing vote for article {ArticleId}", articleId);
                return StatusCode(500, new { success = false, message = "An error occurred while processing your vote." });
            }
        }

        [HttpPost("RemoveVote/{articleId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVote(int articleId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                var (success, message, article) = await _articleVoteService.RemoveVoteAsync(articleId, user.Id);
                
                if (!success)
                {
                    return NotFound(new { success = false, message = message });
                }

                return Json(new { 
                    success = true, 
                    upvotes = article.UpvoteCount, 
                    downvotes = article.DownvoteCount,
                    score = article.UpvoteCount - article.DownvoteCount
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error removing vote for article {ArticleId}", articleId);
                return StatusCode(500, new { success = false, message = "An error occurred while removing your vote." });
            }
        }
    }
}