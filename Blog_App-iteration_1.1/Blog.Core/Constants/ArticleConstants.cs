using System;

namespace Blog.Core.Constants
{
    public static class ArticleConstants
    {
        public static class FileSize
        {
            public const int MaxTotalSize = 10 * 1024 * 1024; // 10MB in bytes
            public const int MaxIndividualFileSize = 10 * 1024 * 1024; // 10MB in bytes
            public const string TotalSizeKey = "ArticleTotalSize";
        }
        
        public static class Messages
        {
            public const string ArticleCreated = "Article created successfully.";
            public const string ArticleUpdated = "Article updated successfully.";
            public const string ArticleDeleted = "Article was successfully deleted";
            public const string UnauthorizedEdit = "You don't have permission to edit this article";
            public const string UserNotFound = "User not found.";
            public const string ArticleNotFound = "Article not found";
            public const string ImageUploadError = "An error occurred while uploading the image.";
            public const string FileSizeExceeded = "File size exceeds the maximum limit of {0}MB";
            public const string TotalSizeExceeded = "Total size exceeds the maximum limit of {0}MB";
            public const string InvalidFileType = "Invalid file type. Allowed types are: {0}";
            public const string CreateError = "An error occurred while creating the article.";
            public const string UpdateError = "An error occurred while updating the article.";
        }
        
        public static class LoggerMessages
        {
            public const string FailedToDeleteFeaturedImage = "Failed to delete old featured image for article {ArticleId}";
            public const string FailedToDeleteStorageImage = "Failed to delete featured image from storage for article {ArticleId}";
        }
        
        public static class ImageStatus
        {
            public const string Processing = "processing";
        }
        
        public static class ImageType
        {
            public const string Featured = "featured";
            public const string Content = "content";
        }
    }
}