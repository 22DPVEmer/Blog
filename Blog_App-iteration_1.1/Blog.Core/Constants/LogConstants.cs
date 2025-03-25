namespace Blog.Core.Constants
{
    public static class LogConstants
    {
        public static class Articles
        {
            public const string RetrieveError = "Error retrieving articles with filter: {searchTerm}, dateFilter: {dateFilter}";
            public const string CreateError = "Error creating article";
            public const string UpdateError = "Error updating article {id}";
            public const string DeleteError = "Error deleting article {id}";
            public const string ImageUploadError = "Error uploading image";
            public const string VoteError = "Error voting for article {ArticleId}";
        }
    }
}