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
            public const string ArticleRetrievalError = "An error occurred while retrieving articles.";
            public const string NoPermissionToEditArticle = "You don't have permission to edit this article.";
            public const string NoPermissionToRankArticles = "You don't have permission to rank articles.";
            public const string ArticleVoteError = "An error occurred while voting for the article.";
            public const string ArticleDeleteError = "An error occurred while deleting the article.";
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
            public const string SignalRNotificationError = "Error sending SignalR notification for {0}";
            public const string SignalRNotificationSentForUpdate = "SignalR notification sent for updated comment";
        }
        
        public static class EventNames
        {
            public const string NewComment = "NewComment";
            public const string UpdateComment = "UpdateComment";
            public const string DeleteComment = "DeleteComment";
        }

        public static class HubConstants
        {
            public const int MinValidId = 1;
            
            public const string InvalidArticleId = "Invalid article ID provided";
            public const string ClientConnected = "Client connected to CommentHub: {ConnectionId}";
            public const string ClientDisconnected = "Client disconnected from CommentHub: {ConnectionId}";
            public const string ClientDisconnectedWithError = "Client disconnected with error: {ConnectionId}";
            public const string InvalidArticleIdProvided = "Invalid article ID {ArticleId} provided to {Method}";
            public const string ClientJoiningGroup = "Client {ConnectionId} joining article group: {GroupName}";
            public const string ClientJoinedGroup = "Client {ConnectionId} joined article group: {GroupName}";
            public const string ClientLeavingGroup = "Client {ConnectionId} leaving article group: {GroupName}";
            public const string ClientLeftGroup = "Client {ConnectionId} left article group: {GroupName}";
            public const string InvalidParameters = "Invalid parameters for {Method}. ArticleId: {ArticleId}";
            public const string NotifyingGroup = "Notifying group {GroupName} about {Action}: {CommentId}";
            public const string GroupNotified = "Group {GroupName} notified about {Action}: {CommentId}";
            public const string NotifyingAboutParentDeletion = "Notifying about parent comment deletion with {ReplyCount} replies";
            public const string ErrorInMethod = "Error in {Method} for article {ArticleId}";

            public const string ArticleGroupNameFormat = "article-{0}";
            
            public static class Actions
            {
                public const string NewComment = "new comment";
                public const string UpdatedComment = "updated comment";
                public const string DeletedComment = "deleted comment";
            }

            public static class Methods
            {
                public const string JoinArticleGroup = "JoinArticleGroup";
                public const string LeaveArticleGroup = "LeaveArticleGroup";
                public const string NotifyNewComment = "NotifyNewComment";
                public const string NotifyUpdateComment = "NotifyUpdateComment";
                public const string NotifyDeleteComment = "NotifyDeleteComment";
                public const string NotifyDeleteCommentWithReplies = "NotifyDeleteCommentWithReplies";
            }
        }

        public static class ApiRoutes
        {
            public const string ControllerRoot = "[controller]";
            public const string ArticleByIdFormat = "Article/{articleId}";
            public const string CommentById = "{id}";
            public const string Request = "Request";
            public const string Process = "Process";
            public const string Vote = "Vote/{articleId}";
            public const string CommentHubEndpoint = "/commentHub";
            public const string DefaultRoute = "{controller=Home}/{action=Index}/{id?}";
            public const string ArticleDelete = "Delete";
            public const string ArticleDeleteConfirmed = "DeleteConfirmed";
            public const string ArticleEdit = "Edit";
            public const string ArticleCreate = "Create";
            public const string ArticleDetails = "Details";
            public const string ArticleUploadImage = "UploadImage";
            public const string ArticleIndex = "Index";
            public const string ArticleId = "{id}";
            public const string DefaultErrorAction = "Error";
            public const string DefaultHomeController = "Home";
        }

        public static class SignalRNotificationTypes
        {
            public const string NewComment = "new comment";
            public const string CommentUpdate = "comment update";
            public const string CommentDeletion = "comment deletion";
        }

        public static class HttpStatusCodes
        {
            public const int InternalServerError = 500;
            public const int BadRequest = 400;
            public const int NotFound = 404;
            public const int Unauthorized = 401;
            public const int Forbidden = 403;
            public const int Ok = 200;
        }
        
        public static class SuccessMessages
        {
            public const string ArticleDeleted = "Article was successfully deleted.";
            public const string ImageUploaded = "Image uploaded successfully";
        }
        
        public static class ViewBagKeys
        {
            public const string CurrentDateFilter = "CurrentDateFilter";
            public const string CurrentSortBy = "CurrentSortBy";
            public const string UserVote = "UserVote";
            public const string UserCanVoteArticles = "UserCanVoteArticles";
            public const string IsAdmin = "IsAdmin";
            public const string ErrorMessage = "ErrorMessage";
            public const string SuccessMessage = "SuccessMessage";
        }
        
        public static class JsonPropertyNames
        {
            public const string Message = "message";
            public const string Success = "success";
            public const string Status = "status";
            public const string ImageUrl = "imageUrl";
            public const string Errors = "errors";
            public const string Id = "id";
            public const string Upvotes = "upvotes";
            public const string Downvotes = "downvotes";
            public const string Score = "score";
        }
    }
}