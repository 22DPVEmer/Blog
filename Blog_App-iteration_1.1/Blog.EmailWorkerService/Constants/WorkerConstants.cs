namespace Blog.EmailWorkerService.Constants{
    public class WorkerConstants{
        public static readonly TimeSpan QueueCheckDelay = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan EmailRetentionPeriod = TimeSpan.FromHours(24);
        
        public static readonly string ServiceStartedMessage = "Email Worker Service started at: {time}";
        public static readonly string CycleStartMessage = "Starting email queue processing cycle at: {time}";
        public static readonly string CycleCompleteMessage = "Completed email queue processing cycle at: {time}";
        public static readonly string QueueProcessingStartMessage = "Beginning to process email queue at: {time}";
        public static readonly string ProcessingEmailMessage = "Processing email to {email} with subject: {subject}";
        public static readonly string EmailSentSuccessMessage = "Successfully sent email to {email} with subject: {subject}";
        public static readonly string EmailSendFailureMessage = "Failed to send email to {email} with subject: {subject}. Error: {error}";
        public static readonly string QueueProcessingCompleteMessage = "Email queue processing completed. Processed: {processed}, Failed: {failed}, Total: {total}";
        public static readonly string CriticalErrorMessage = "Critical error occurred while processing email queue: {error}";
    }
}