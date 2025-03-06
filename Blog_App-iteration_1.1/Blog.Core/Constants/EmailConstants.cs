namespace Blog.Core.Constants
{
    public static class EmailConstants
    {
        // Email Subjects
        public const string ResetPasswordSubject = "Reset Password";

        // Email Templates
        public const string ResetPasswordTemplate = "Please reset your password by <a href='{0}'>clicking here</a>.";

        // Status Messages
        public const string PasswordResetLinkSent = "Password reset link has been sent. Please check your email.";

        // URL Constants
        public const string ResetPasswordPage = "/Account/ResetPassword";
        public const string IdentityArea = "Identity";
    }
}