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

            var requests = user.IsAdmin
                ? await _permissionService.GetAdminPermissionRequestsAsync()
                : await _permissionService.GetUserPermissionRequestsAsync(user.Id);

            if (!user.IsAdmin && user.CanWriteArticles && !user.CanVoteArticles)
            {
                TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
            }
            else if (!user.IsAdmin && user.CanVoteArticles && !user.CanWriteArticles)
            {
                TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasVotePermission;
            }
            else if (!user.IsAdmin && user.CanWriteArticles && user.CanVoteArticles)
            {
                TempData["InfoMessage"] = PermissionConstants.Messages.HasBothPermissions;
            }

            return View(requests);
        }

        // GET: Permissions/Request
        [HttpGet("Request")]
        public new async Task<IActionResult> Request(bool isVoteRequest = false)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }
            
            if (isVoteRequest)
            {
                if (user.CanVoteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasVotePermission;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.IsVoteRequest = true;
                ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
            }
            else
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.IsVoteRequest = false;
                ViewBag.Title = PermissionConstants.ViewData.WriteRequest.Title;
                ViewBag.Description = PermissionConstants.ViewData.WriteRequest.Description;
            }
            
            return View();
        }

        // POST: Permissions/Request
        [HttpPost("Request")]
        [ValidateAntiForgeryToken]
        public new async Task<IActionResult> Request([FromForm] string reason, [FromForm] bool isVoteRequest = false)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError("", PermissionConstants.Messages.EmptyReason);
                
                // Set ViewBag values for the view
                if (isVoteRequest)
                {
                    ViewBag.IsVoteRequest = true;
                    ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
                }
                else
                {
                    ViewBag.IsVoteRequest = false;
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
                    ViewBag.IsVoteRequest = true;
                    ViewBag.Title = PermissionConstants.ViewData.VoteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.VoteRequest.Description;
                    return View();
                }
            }
            else
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = PermissionConstants.Messages.AlreadyHasWritePermission;
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingRequest = await _permissionService.HasPendingRequestAsync(user.Id);
                if (hasPendingRequest)
                {
                    ModelState.AddModelError("", PermissionConstants.Messages.PendingWriteRequest);
                    ViewBag.IsVoteRequest = false;
                    ViewBag.Title = PermissionConstants.ViewData.WriteRequest.Title;
                    ViewBag.Description = PermissionConstants.ViewData.WriteRequest.Description;
                    return View();
                }
            }

            await _permissionService.CreatePermissionRequestAsync(user.Id, reason, isVoteRequest);

            TempData["SuccessMessage"] = string.Format(
                PermissionConstants.Messages.PermissionRequestSuccess, 
                isVoteRequest ? PermissionConstants.PermissionType.Voting : PermissionConstants.PermissionType.Writing);
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