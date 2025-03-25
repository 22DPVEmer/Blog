using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Blog.Core.Constants;

namespace Blog.Web.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [Route(UserConstants.DefaultProfileImageRoutePath)]
        public IActionResult DefaultProfileImage()
        {
            // Returns a static image
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), UserConstants.RootPath);
            string imagePath = Path.Combine(webRootPath, UserConstants.DefaultProfileImagePath);
            
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, UserConstants.ImageJpgContentType);
            }
            
            // If the image doesn't exist, return a 404 Not Found
            return NotFound(UserConstants.DefaultProfileImageNotFoundMessage);
        }
    }
} 