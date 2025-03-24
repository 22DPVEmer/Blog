namespace Blog.Core.Constants
{
    public static class PermissionConstants
    {
        public static class Messages
        {
            public const string PendingVoteRequest = "You already have a pending vote permission request.";
            public const string PendingWriteRequest = "You already have a pending write permission request.";
            public const string EmptyReason = "Please provide a reason for your request.";
            public const string AlreadyHasWritePermission = "You already have permission to write articles.";
            public const string AlreadyHasVotePermission = "You already have permission to vote on articles.";
            public const string HasBothPermissions = "You already have both write and vote permissions.";
            public const string HasAllPermissions = "You have all available permissions.";
            public const string PermissionRequestSuccess = "Your {0} permission request has been submitted successfully.";
            public const string RequestProcessed = "The permission request has been {0}.";
            public const string NoVotePermission = "You don't have permission to vote on articles.";
            public const string UnauthorizedVote = "You don't have permission to vote on articles.";
            public const string UserNotFound = "User not found.";
        }

        public static class ViewData
        {
            public static class VoteRequest
            {
                public const string Title = "Request Voting Permission";
                public const string Description = "Why would you like to vote on articles?";
            }

            public static class WriteRequest
            {
                public const string Title = "Request Writing Permission";
                public const string Description = "Why would you like to write articles?";
            }
            
            public static class CommentRequest
            {
                public const string Title = "Request Comment Permission";
                public const string Description = "Why would you like to comment on articles?";
            }
        }

        public static class PermissionType
        {
            public const string Voting = "voting";
            public const string Writing = "writing";
            public const string Commenting = "commenting";
        }

        public static class ApprovalStatus
        {
            public const string Approved = "approved";
            public const string Rejected = "rejected";
        }
        
        /// <summary>
        /// Type identifiers used in controllers for permission requests
        /// </summary>
        public static class TypeIdentifiers
        {
            public const string Vote = "vote";
            public const string Comment = "comment";
            public const string Write = "write";
        }
    }
} 