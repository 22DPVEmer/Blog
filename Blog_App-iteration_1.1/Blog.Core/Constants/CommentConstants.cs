namespace Blog.Core.Constants
{
    public static class CommentConstants
    {
        public static class Messages
        {
            public const string CommentNotFound = "Comment not found";
            public const string CommentAdded = "Comment added successfully";
            public const string CommentUpdated = "Comment updated successfully";
            public const string CommentDeleted = "Comment deleted successfully";
            public const string UnauthorizedCommentEdit = "You don't have permission to edit this comment";
            public const string UnauthorizedCommentDelete = "You don't have permission to delete this comment";
            public const string NoCommentPermission = "You don't have permission to comment on articles";
            public const string PendingCommentRequest = "You already have a pending comment permission request";
            public const string AlreadyHasCommentPermission = "You already have permission to comment on articles";
            public const string NestedCommentLimit = "Comments can only be nested one level deep (replies to replies are not allowed)";
        }

        public static class LogMessages
        {
            public const string ErrorAddingComment = "Error adding comment for article {ArticleId}";
            public const string ErrorUpdatingComment = "Error updating comment {CommentId}";
            public const string ErrorDeletingComment = "Error deleting comment {CommentId}";
            public const string ErrorRetrievingComments = "Error retrieving comments for article {ArticleId}";
        }
        
        public static class EventNames
        {
            public const string NewComment = "NewComment";
            public const string UpdateComment = "UpdateComment";
            public const string DeleteComment = "DeleteComment";
        }
    }
} 