namespace Blog.Core.Constants
{
    public static class EmailQueueConstants
    {
        // Pipe Configuration
        public const string DefaultPipeName = "BlogEmailQueuePipe";
        public const string LocalPipeServer = ".";
        
        // Timeouts and Delays (in milliseconds)
        public const int PipeConnectionTimeout = 15000;
        public const int RetryDelay = 1000;
        
        // Pipe Settings
        public const int MaxPipeInstances = 1;
        
        // Error Messages
        public const string ConnectionFailureMessage = "Failed to connect to email queue pipe: {0}. Retrying...";
        public const string RetryFailureMessage = "Retry failed: {0}. Email will not be sent.";
    }
}