using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blog.Core.Constants;
using System;
using Blog.Web.Models;

namespace Blog.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
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

        [HttpPost]
        [Route("Vote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote([FromBody] ArticleVoteModel model)
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
                var (success, voteMessage, article) = await _articleVoteService.VoteArticleAsync(model.ArticleId, user.Id, model.IsUpvote);
                
                if (!success)
                {
                    return BadRequest(new { success = false, message = voteMessage });
                }

                return Json(new { 
                    success = true, 
                    upvoteCount = article.UpvoteCount, 
                    downvoteCount = article.DownvoteCount,
                    score = article.UpvoteCount - article.DownvoteCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(CommentConstants.HttpStatusCodes.InternalServerError, new { success = false, message = VoteConstants.Messages.ErrorProcessingVote });
            }
        }
    }
}