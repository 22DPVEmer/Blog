using System;
using System.Threading.Tasks;
using Blog.Core.Services;
using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Blog.Core.Interfaces;
using Blog.Core.Constants;

namespace Blog.Web.Controllers
{
    [Authorize]
    [Route(CommentConstants.ApiRoutes.ControllerRoot)]
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

        [HttpPost(UserConstants.ApiRoutes.UploadProfilePicture)]
        public async Task<IActionResult> UploadProfilePicture()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.NotFound,
                        UserConstants.Messages.UserNotFound
                    );
                }

                var file = Request.Form.Files.GetFile(UserConstants.FormFieldNames.ProfilePicture);
                if (file == null)
                {
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.BadRequest,
                        UserConstants.Messages.NoFileUploaded
                    );
                }

                _logger.LogInformation(UserConstants.LogMessages.ProcessingProfilePictureUpload, user.Id);

                // Delete old profile picture if it exists
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    try
                    {
                        await _firebaseStorageService.DeleteImageAsync(user.ProfilePicture);
                        _logger.LogInformation(UserConstants.LogMessages.DeletedOldProfilePicture);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, UserConstants.LogMessages.FailedToDeleteOldProfilePicture);
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
                    return StatusCode(
                        CommentConstants.HttpStatusCodes.BadRequest,
                        UserConstants.Messages.ProfilePictureUpdateFailed
                    );
                }

                return Json(new { 
                    success = true, 
                    imageUrl = imageUrl 
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, UserConstants.LogMessages.ValidationErrorDuringUpload);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.BadRequest,
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UserConstants.LogMessages.ErrorUploadingProfilePicture);
                return StatusCode(
                    CommentConstants.HttpStatusCodes.InternalServerError,
                    UserConstants.Messages.ProfilePictureUploadError
                );
            }
        }
    }
} 