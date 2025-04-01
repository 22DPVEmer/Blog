namespace Blog.Core.Constants
{
    public static class EmailConstants
    {
        // Email Subjects
        public const string ResetPasswordSubject = "Reset Password";
        public const string EmailConfirmationSubject = "Confirm your email";

        // Email Templates
        public const string ResetPasswordTemplate = "Please reset your password by <a href='{0}'>clicking here</a>.";
        public const string EmailConfirmationTemplate = "Please confirm your account by <a href='{0}'>clicking here</a>.";

        // Status Messages
        public const string PasswordResetLinkSent = "Password reset link has been sent. Please check your email.";
        public const string EmailConfirmationSent = "Please check your email to confirm your account.";

        // URL Constants
        public const string ResetPasswordPage = "/Account/ResetPassword";
        public const string EmailConfirmationPage = "/Account/ConfirmEmail";
        public const string IdentityArea = "Identity";
    }
}