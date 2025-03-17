namespace Blog.Core.Constants
{
    public static class ImageProcessingConstants
    {
        public const string DefaultImageName = "image";
        
        public static class FormFields
        {
            public const string Image = "image";
        }
        
        public static class ErrorMessages
        {
            public const string ProcessingError = "Error processing image job {0}";
            public const string BackgroundServiceError = "Error in background service";
        }
    }
} 