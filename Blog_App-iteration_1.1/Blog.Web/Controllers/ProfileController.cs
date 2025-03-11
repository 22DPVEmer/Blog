using System;
using System.Threading.Tasks;
using Blog.Core.Services;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Blog.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<User> userManager,
            IFirebaseStorageService firebaseStorageService,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _firebaseStorageService = firebaseStorageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var file = Request.Form.Files.GetFile("profilePicture");
                if (file == null)
                {
                    return BadRequest("No file uploaded.");
                }

                _logger.LogInformation("Processing profile picture upload for user {UserId}", user.Id);

                // Delete old profile picture if it exists
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    try
                    {
                        await _firebaseStorageService.DeleteImageAsync(user.ProfilePicture);
                        _logger.LogInformation("Deleted old profile picture");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old profile picture");
                    }
                }

                // Upload new profile picture
                string imageUrl = await _firebaseStorageService.UploadProfilePictureAsync(file);
                
                // Update user profile
                user.ProfilePicture = imageUrl;
                user.UpdatedAt = DateTime.UtcNow;
                
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest("Failed to update profile picture.");
                }

                return Json(new { success = true, imageUrl });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during profile picture upload");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return StatusCode(500, "An error occurred while uploading the profile picture.");
            }
        }
    }
} 