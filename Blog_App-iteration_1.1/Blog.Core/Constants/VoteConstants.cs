namespace Blog.Core.Constants
{
    public static class VoteConstants
    {
        public static class Messages
        {
            public const string UserNotFound = "User not found";
            public const string NoVotePermission = "You don't have permission to vote on articles";
            public const string VoteNotFound = "Vote not found";
            public const string ArticleNotFound = "Article not found";
            public const string VoteRemoved = "Vote removed successfully";
            public const string VoteProcessed = "Vote processed successfully";
            public const string ErrorProcessingVote = "An error occurred while processing the vote";
            public const string ErrorRemovingVote = "An error occurred while removing the vote";
            public const string UnauthorizedVote = "You don't have permission to vote on articles";
        }

        public static class LogMessages
        {
            public const string ErrorProcessingVote = "Error processing vote for article {ArticleId}";
            public const string ErrorRemovingVote = "Error removing vote for article {ArticleId}";
        }

        public static class SortOptions
        {
            public const string Popular = "popular";
            public const string MostUpvoted = "mostupvoted";
            public const string MostDownvoted = "mostdownvoted";
            public const string Newest = "newest";
        }
    }
} 