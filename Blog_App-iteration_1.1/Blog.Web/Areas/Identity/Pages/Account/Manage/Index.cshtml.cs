// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Blog.Core.Constants;
using Microsoft.AspNetCore.Http;
using Blog.Core.Services;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IFirebaseStorageService firebaseStorageService,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _firebaseStorageService = firebaseStorageService;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProfilePicture { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(Blog.Core.Constants.IdentityConstants.Profile.MaxNameLength, ErrorMessage = Blog.Core.Constants.IdentityConstants.Profile.FirstNameLengthError)]
            [RegularExpression(Blog.Core.Constants.IdentityConstants.Profile.NameRegexPattern, ErrorMessage = Blog.Core.Constants.IdentityConstants.Profile.NameNumberError)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(Blog.Core.Constants.IdentityConstants.Profile.MaxNameLength, ErrorMessage = Blog.Core.Constants.IdentityConstants.Profile.LastNameLengthError)]
            [RegularExpression(Blog.Core.Constants.IdentityConstants.Profile.NameRegexPattern, ErrorMessage = Blog.Core.Constants.IdentityConstants.Profile.LastNameNumberError)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePictureFile { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;
            ProfilePicture = user.ProfilePicture;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            bool hasChanges = false;
            _logger.LogInformation("Starting profile update for user {UserId}", user.Id);

            // Update FirstName and LastName
            if (user.FirstName != Input.FirstName || user.LastName != Input.LastName)
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                hasChanges = true;
                _logger.LogInformation("Name updated for user {UserId}. New name: {FirstName} {LastName}", 
                    user.Id, Input.FirstName, Input.LastName);
            }

            if (hasChanges)
            {
                _logger.LogInformation("Saving changes to user profile");
                user.UpdatedAt = DateTime.UtcNow;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update user profile. Errors: {Errors}", 
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    StatusMessage = "Error: Failed to update profile.";
                    return RedirectToPage();
                }
                _logger.LogInformation("Successfully updated user profile");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
