using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Entities;
using System.Threading.Tasks;
using System.Linq;
using System;
using Blog.Web.Models;
using Blog.Core.Interfaces;
using Blog.Core.Constants;

namespace Blog.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly UserManager<User> _userManager;

        public PermissionsController(
            IPermissionService permissionService,
            UserManager<User> userManager)
        {
            _permissionService = permissionService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ViewBag.IsAdmin = user.IsAdmin;
            ViewBag.CanWriteArticles = user.CanWriteArticles;
            ViewBag.CanVoteArticles = user.CanVoteArticles;
            ViewBag.CanCommentArticles = user.CanCommentArticles;

            var requests = user.IsAdmin
                ? await _permissionService.GetAdminPermissionRequestsAsync()
                : await _permissionService.GetUserPermissionRequestsAsync(user.Id);

            if (!user.IsAdmin)
            {
                if (user.CanWriteArticles && !user.CanVoteArticles && !user.CanCommentArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
                }
                else if (user.CanVoteArticles && !user.CanWriteArticles && !user.CanCommentArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasVotePermission;
                }
                else if (user.CanCommentArticles && !user.CanWriteArticles && !user.CanVoteArticles)
                {
                    TempData["InfoMessage"] = CommentConstants.Messages.AlreadyHasCommentPermission;
                }
                else if (user.CanWriteArticles && user.CanVoteArticles && user.CanCommentArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.HasAllPermissions;
                }
            }

            return View(requests);
        }

        // GET: Permissions/Request
        [HttpGet("Request")]
        public new async Task<IActionResult> Request(string type = "write")
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }
            
            if (type == "vote")
            {
                if (user.CanVoteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasVotePermission;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.PermissionType = "vote";
                ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
            }
            else if (type == "comment")
            {
                if (user.CanCommentArticles)
                {
                    TempData["InfoMessage"] = CommentConstants.Messages.AlreadyHasCommentPermission;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.PermissionType = "comment";
                ViewBag.Title = PermissionConstants.ViewData.CommentRequest.Title;
                ViewBag.Description = PermissionConstants.ViewData.CommentRequest.Description;
            }
            else // default to write request
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.PermissionType = "write";
                ViewBag.Title = PermissionConstants.ViewData.WriteRequest.Title;
                ViewBag.Description = PermissionConstants.ViewData.WriteRequest.Description;
            }
            
            return View();
        }

        // POST: Permissions/Request
        [HttpPost("Request")]
        [ValidateAntiForgeryToken]
        public new async Task<IActionResult> Request([FromForm] string reason, [FromForm] string type = "write")
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError("", PermissionConstants.Messages.EmptyReason);
                
                // Set ViewBag values for the view
                if (type == "vote")
                {
                    ViewBag.PermissionType = "vote";
                    ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
                }
                else if (type == "comment")
                {
                    ViewBag.PermissionType = "comment";
                    ViewBag.Title = PermissionConstants.ViewData.CommentRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.CommentRequest.Description;
                }
                else // default to write request
                {
                    ViewBag.PermissionType = "write";
                    ViewBag.Title = PermissionConstants.ViewData.WriteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.WriteRequest.Description;
                }
                
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            bool isVoteRequest = type == "vote";
            bool isCommentRequest = type == "comment";
            
            if (isVoteRequest)
            {
                if (user.CanVoteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasVotePermission;
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingVoteRequest = await _permissionService.HasPendingVoteRequestAsync(user.Id);
                if (hasPendingVoteRequest)
                {
                    ModelState.AddModelError("", PermissionConstants.Messages.PendingVoteRequest);
                    ViewBag.PermissionType = "vote";
                    ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
                    return View();
                }
            }
            else if (isCommentRequest)
            {
                if (user.CanCommentArticles)
                {
                    TempData["InfoMessage"] = CommentConstants.Messages.AlreadyHasCommentPermission;
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingCommentRequest = await _permissionService.HasPendingCommentRequestAsync(user.Id);
                if (hasPendingCommentRequest)
                {
                    ModelState.AddModelError("", CommentConstants.Messages.PendingCommentRequest);
                    ViewBag.PermissionType = "comment";
                    ViewBag.Title = PermissionConstants.ViewData.CommentRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.CommentRequest.Description;
                    return View();
                }
            }
            else // Write request
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingWriteRequest = await _permissionService.HasPendingRequestAsync(user.Id);
                if (hasPendingWriteRequest)
                {
                    ModelState.AddModelError("", PermissionConstants.Messages.PendingWriteRequest);
                    ViewBag.PermissionType = "write";
                    ViewBag.Title = PermissionConstants.ViewData.WriteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.WriteRequest.Description;
                    return View();
                }
            }

            await _permissionService.CreatePermissionRequestAsync(user.Id, reason, isVoteRequest, isCommentRequest);

            string permissionType = isVoteRequest 
                ? PermissionConstants.PermissionType.Voting 
                : (isCommentRequest 
                    ? PermissionConstants.PermissionType.Commenting 
                    : PermissionConstants.PermissionType.Writing);
                    
            TempData["SuccessMessage"] = string.Format(
                PermissionConstants.Messages.PermissionRequestSuccess, 
                permissionType);
                
            return RedirectToAction(nameof(Index));
        }

        // POST: Permissions/Process
        [HttpPost("Process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id, bool approved, string? rejectionReason = null)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null || !admin.IsAdmin)
            {
                return Forbid();
            }

            await _permissionService.ProcessPermissionRequestAsync(id, admin.Id, approved, rejectionReason ?? string.Empty);

            TempData["SuccessMessage"] = string.Format(
                PermissionConstants.Messages.RequestProcessed, 
                approved ? PermissionConstants.ApprovalStatus.Approved : PermissionConstants.ApprovalStatus.Rejected);
            return RedirectToAction(nameof(Index));
        }
    }
}