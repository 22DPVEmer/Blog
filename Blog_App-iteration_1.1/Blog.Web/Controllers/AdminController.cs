using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Blog.Infrastructure.Entities;
using Blog.Core.Interfaces;
using System.Linq;
using Blog.Core.Constants;
using Blog.Core.Models;
using System;
using Blog.Web.Models;

namespace Blog.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ICommentService _commentService;
        
        public AdminController(
            UserManager<User> userManager,
            ICommentService commentService)
        {
            _userManager = userManager;
            _commentService = commentService;
        }
        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction(AdminConstants.Routes.Index, AdminConstants.Routes.Home);
            }
            
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction(AdminConstants.Routes.Index, AdminConstants.Routes.Home);
            }
            
            var reportedComments = await _commentService.GetReportedCommentsAsync();
            return View(reportedComments);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport([FromBody] ResolveReportViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsAdmin)
            {
                return Forbid();
            }
            
            try
            {
                await _commentService.ResolveReportAsync(model.ReportId, model.IsResolved);
                
                string message = model.IsResolved 
                    ? AdminConstants.SuccessMessages.ReportResolved 
                    : AdminConstants.SuccessMessages.ReportUnresolved;
                
                TempData[AdminConstants.ViewBagKeys.SuccessMessage] = message;
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
} 