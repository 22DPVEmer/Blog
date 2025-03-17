namespace Blog.Core.Constants
{
    public static class FirebaseConstants
    {
        public const string BucketName = "blog-ed498.firebasestorage.app";
        public const string ProjectId = "blog-ed498";
        public const string CredentialsFileName = "blog-ed498-firebase-adminsdk-fbsvc-218aa95c14.json";
        
        public static class ContentTypes
        {
            public const string OctetStream = "application/octet-stream";
            public const string Jpeg = "image/jpeg";
            public const string Png = "image/png";
            public const string Gif = "image/gif";
            public const string Webp = "image/webp";
        }
        
        public static class Folders
        {
            public const string Content = "content";
            public const string ProfilePictures = "profilepictures";
        }
        
        public static class Limits
        {
            public const int ProfilePictureMaxSize = 2 * 1024 * 1024; // 2MB
        }
        
        public static class Messages
        {
            public const string CredentialsNotFound = "Firebase credentials file not found. Please ensure the file exists and is accessible.";
            public const string UploadError = "An error occurred while uploading the image: {0}";
            public const string DeleteError = "An error occurred while deleting the image: {0}";
            public const string ProfilePictureSizeExceeded = "Profile picture size must not exceed 2MB";
            public const string ProfilePictureInvalidType = "Only JPG, PNG, and WEBP images are allowed for profile pictures";
        }
        
        public static class Urls
        {
            public const string StorageBaseUrl = "https://storage.googleapis.com/{0}/{1}";
        }
    }
} 