using Blog.Core.Constants;
using Blog.Core.Interfaces;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Blog.Core.Services
{
    public class ArticleVoteService : IArticleVoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArticleVoteService> _logger;

        public ArticleVoteService(
            ApplicationDbContext context,
            ILogger<ArticleVoteService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool canVote, string message)> CanUserVoteAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, PermissionConstants.Messages.UserNotFound);
            }

            if (!user.CanVoteArticles && !user.IsAdmin)
            {
                return (false, PermissionConstants.Messages.NoVotePermission);
            }

            return (true, string.Empty);
        }

        public async Task<ArticleVote> GetUserVoteAsync(int articleId, string userId)
        {
            return await _context.ArticleVotes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);
        }

        public async Task<(bool success, string message, Article article)> VoteArticleAsync(int articleId, string userId, bool isUpvote)
        {
            try
            {
                var article = await _context.Articles.FindAsync(articleId);
                if (article == null)
                {
                    return (false, VoteConstants.Messages.ArticleNotFound, null);
                }

                var existingVote = await GetUserVoteAsync(articleId, userId);
                if (existingVote != null)
                {
                    if (existingVote.IsUpvote == isUpvote)
                    {
                        await UpdateArticleVoteCountsOnRemoval(article, existingVote.IsUpvote);
                        _context.ArticleVotes.Remove(existingVote);
                    }
                    else
                    {
                        await HandleExistingVote(article, existingVote, isUpvote);
                    }
                }
                else
                {
                    await CreateNewVote(article, userId, isUpvote);
                }

                await _context.SaveChangesAsync();
                return (true, VoteConstants.Messages.VoteProcessed, article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, VoteConstants.LogMessages.ErrorProcessingVote, articleId);
                return (false, VoteConstants.Messages.ErrorProcessingVote, null);
            }
        }

        private async Task HandleExistingVote(Article article, ArticleVote existingVote, bool isUpvote)
        {
            if (existingVote.IsUpvote != isUpvote)
            {
                await UpdateArticleVoteCountsOnChange(article, isUpvote);
                existingVote.IsUpvote = isUpvote;
            }
        }

        private async Task CreateNewVote(Article article, string userId, bool isUpvote)
        {
            var vote = new ArticleVote
            {
                ArticleId = article.Id,
                UserId = userId,
                IsUpvote = isUpvote,
                CreatedAt = DateTime.UtcNow
            };

            await UpdateArticleVoteCountsOnNew(article, isUpvote);
            await _context.ArticleVotes.AddAsync(vote);
        }

        private Task UpdateArticleVoteCountsOnChange(Article article, bool newVoteIsUpvote)
        {
            if (newVoteIsUpvote)
            {
                article.DownvoteCount--;
                article.UpvoteCount++;
            }
            else
            {
                article.UpvoteCount--;
                article.DownvoteCount++;
            }
            return Task.CompletedTask;
        }

        private Task UpdateArticleVoteCountsOnNew(Article article, bool isUpvote)
        {
            if (isUpvote)
            {
                article.UpvoteCount++;
            }
            else
            {
                article.DownvoteCount++;
            }
            return Task.CompletedTask;
        }

        private Task UpdateArticleVoteCountsOnRemoval(Article article, bool wasUpvote)
        {
            if (wasUpvote)
            {
                article.UpvoteCount = Math.Max(0, article.UpvoteCount - 1);
            }
            else
            {
                article.DownvoteCount = Math.Max(0, article.DownvoteCount - 1);
            }
            return Task.CompletedTask;
        }
    }
} 