namespace Blog.Core.Constants
{
    public static class CommentConstants
    {
        public static class ErrorMessages
        {            
            public const string InternalServerError = "An error occurred while retrieving comments.";
            public const string CommentAddError = "An error occurred while adding the comment.";
            public const string CommentUpdateError = "An error occurred while updating the comment.";
            public const string CommentDeleteError = "An error occurred while deleting the comment.";
            public const string CommentIdMismatch = "Comment ID mismatch.";
        }
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
            public const string AddCommentCalled = "AddComment called with ArticleId: {ArticleId}, Content length: {ContentLength}";
            public const string InvalidModelState = "Invalid ModelState in AddComment: {Errors}";
            public const string UserNotFoundInAddComment = "User not found in AddComment";
            public const string UserAttemptingAddComment = "User {UserId} attempting to add comment";
            public const string CommentAddedSuccessfully = "Comment added successfully with ID: {CommentId}";
            public const string SignalRNotificationSent = "SignalR notification sent for new comment";
            public const string UnauthorizedAccessInAddComment = "Unauthorized access in AddComment";
            public const string InvalidArgumentInAddComment = "Invalid argument in AddComment";
            public const string DeletingParentComment = "Deleting parent comment {CommentId} with {ReplyCount} replies";
            public const string SignalRNotificationSentForParentComment = "SignalR notification sent for parent comment {CommentId} and {ReplyCount} replies";
            public const string SignalRNotificationSentForDeletion = "SignalR notification sent for comment deletion {CommentId}";
        }
        
        public static class EventNames
        {
            public const string NewComment = "NewComment";
            public const string UpdateComment = "UpdateComment";
            public const string DeleteComment = "DeleteComment";
        }
    }
}