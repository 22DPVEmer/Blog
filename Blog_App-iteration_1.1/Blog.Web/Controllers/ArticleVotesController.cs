using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blog.Core.Constants;

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
                    return Unauthorized(new { success = false, message = PermissionConstants.Messages.UserNotFound });
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
                _logger.LogError(ex, VoteConstants.LogMessages.ErrorProcessingVote, articleId);
                return StatusCode(500, new { success = false, message = VoteConstants.Messages.ErrorProcessingVote });
            }
        }
    }
}