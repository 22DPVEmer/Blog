namespace Blog.Core.Constants
{
    public static class LogConstants
    {
        public static class Articles
        {
            public const string RetrieveError = "Error retrieving articles. SearchTerm: {SearchTerm}, DateFilter: {DateFilter}";
            public const string CreateError = "Error creating article";
            public const string UpdateError = "Error updating article {ArticleId}";
            public const string DeleteError = "Error deleting article {ArticleId}";
            public const string ImageUploadError = "Error uploading image";
        }
    }
}