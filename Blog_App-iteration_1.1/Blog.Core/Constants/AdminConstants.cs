namespace Blog.Core.Constants
{
    public static class AdminConstants
    {
        public static class Routes
        {
            public const string Index = "Index";
            public const string Home = "Home";
        }
        
        public static class SuccessMessages
        {
            public const string ReportResolved = "Report has been resolved successfully.";
            public const string ReportUnresolved = "Report has been marked as unresolved.";
        }
        
        public static class ViewBagKeys
        {
            public const string SuccessMessage = "SuccessMessage";
        }
    }
} 