namespace Blog.Core.Constants
{
    public static class IdentityConstants
    {
        public static class ChangePassword
        {
            public const string SuccessMessage = "Your password has been changed.";
            public const string NotFoundMessage = "Unable to load user with ID '{0}'";
            public const string PasswordValidationError = "The {0} must be at least {2} and at max {1} characters long.";
            public const string PasswordMismatchError = "The new password and confirmation password do not match.";
            public const int MaxPasswordLength = 100;
            public const int MinPasswordLength = 6;
        }

        public static class Profile
        {
            public const string SuccessMessage = "Your profile has been updated";
            public const string NotFoundMessage = "Unable to load user with ID '{0}'";
            public const string UpdateError = "Unexpected error when trying to update profile.";
            public const string FirstNameRequired = "First name is required.";
            public const string LastNameRequired = "Last name is required.";
            public const string NameRegexPattern = @"^[a-zA-Z\s]*$";
            public const string NameNumberError = "First name cannot contain numbers.";
            public const string LastNameNumberError = "Last name cannot contain numbers.";
            public const string FirstNameLengthError = "First name cannot exceed 60 characters.";
            public const string LastNameLengthError = "Last name cannot exceed 60 characters.";
            public const int MaxNameLength = 60;
        }

        public static class Registration
        {
            public const string DefaultRole = "User";
            public const string EmailConfirmationSubject = "Confirm your email";
            public const string EmailConfirmationMessage = "Please confirm your account by <a href='{0}'>clicking here</a>.";
            public const string AccountCreatedMessage = "User created a new account with password.";
            public const string EmailNotConfirmedMessage = "You must confirm your email before logging in. Please check your email inbox.";
            public const string EmailConfirmedMessage = "Your email has been confirmed. You can now log in.";
            public const int MaxPasswordLength = 100;
            public const int MinPasswordLength = 6;
            public const string PasswordLengthError = "The {0} must be at least {2} and at max {1} characters long.";
            public const string PasswordMismatchError = "The password and confirmation password do not match.";
            public const string FirstNameRequired = "First name is required.";
            public const string LastNameRequired = "Last name is required.";
            public const string NameRegexPattern = @"^[a-zA-Z\s]*$";
            public const string FirstNameNumberError = "First name cannot contain numbers.";
            public const string LastNameNumberError = "Last name cannot contain numbers.";
            public const string FirstNameLengthError = "First name cannot exceed 60 characters.";
            public const string LastNameLengthError = "Last name cannot exceed 60 characters.";
            public const int MaxNameLength = 60;
        }

        public static class ResetPassword
        {
            public const string SuccessMessage = "Your password has been reset.";
            public const string LoginMessage = "Your password has been reset. Please log in with your new password.";
            public const string CodeRequiredMessage = "A code must be supplied for password reset.";
            public const string PasswordValidationError = "The {0} must be at least {2} and at max {1} characters long.";
            public const string PasswordMismatchError = "The password and confirmation password do not match.";
            public const int MaxPasswordLength = 100;
            public const int MinPasswordLength = 6;
        }
    }
}
