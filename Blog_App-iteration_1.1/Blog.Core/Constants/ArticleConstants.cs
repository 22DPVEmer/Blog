namespace Blog.Core.Constants
{
    public static class ArticleConstants
    {
        public const int MaxTotalSize = 10 * 1024 * 1024; // 10MB in bytes
        public const string TotalSizeKey = "ArticleTotalSize";
        
        public static class Messages
        {
            public const string ArticleCreated = "Article created successfully";
            public const string ArticleUpdated = "Article updated successfully";
            public const string ArticleDeleted = "Article was successfully deleted";
            public const string UnauthorizedEdit = "You don't have permission to edit this article";
            public const string UserNotFound = "User not found";
            public const string ArticleNotFound = "Article not found";
            public const string ImageUploadError = "Error uploading image";
        }
    }
}