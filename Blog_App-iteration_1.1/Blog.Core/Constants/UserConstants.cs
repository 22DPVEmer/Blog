namespace Blog.Core.Constants
{
    public static class UserConstants
    {
        /// <summary>
        /// Default path for the profile image relative to wwwroot folder
        /// </summary>
        public const string DefaultProfileImagePath = "images/default-profile.jpg";

        /// <summary>
        /// Route path for default profile image
        /// </summary>
        public const string DefaultProfileImageRoutePath = "images/default-profile.jpg";

        /// <summary>
        /// Content type for JPG images
        /// </summary>
        public const string ImageJpgContentType = "image/jpg";
        
        /// <summary>
        /// Error message when default profile image is not found
        /// </summary>
        public const string DefaultProfileImageNotFoundMessage = "Default profile image not found";

        /// <summary>
        /// Root path for the application
        /// </summary>
        public const string RootPath = "wwwroot";

        public static class Messages
        {
            public const string UserNotFound = "User not found.";
            public const string NoFileUploaded = "No file uploaded.";
            public const string ProfilePictureUpdateFailed = "Failed to update profile picture.";
            public const string ProfilePictureUploadError = "An error occurred while uploading the profile picture.";
        }
        
        public static class LogMessages
        {
            public const string ProcessingProfilePictureUpload = "Processing profile picture upload for user {UserId}";
            public const string DeletedOldProfilePicture = "Deleted old profile picture";
            public const string FailedToDeleteOldProfilePicture = "Failed to delete old profile picture";
            public const string ValidationErrorDuringUpload = "Validation error during profile picture upload";
            public const string ErrorUploadingProfilePicture = "Error uploading profile picture";
        }
        
        public static class FormFieldNames
        {
            public const string ProfilePicture = "profilePicture";
        }
        
        public static class ApiRoutes
        {
            public const string UploadProfilePicture = "UploadProfilePicture";
        }
        
        public static class JsonPropertyNames
        {
            public const string Success = "success";
            public const string ImageUrl = "imageUrl";
        }
    }
}
