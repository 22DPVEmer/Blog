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
                TempData["InfoMessage"] = "You already have permission to write articles.";
            }
            else if (!user.IsAdmin && user.CanVoteArticles && !user.CanWriteArticles)
            {
                TempData["InfoMessage"] = "You already have permission to vote on articles.";
            }
            else if (!user.IsAdmin && user.CanWriteArticles && user.CanVoteArticles)
            {
                TempData["InfoMessage"] = "You have permission to write and vote on articles.";
            }

            return View(requests);
        }

        // GET: Permissions/Request
        [HttpGet("Request")]
        public async Task<IActionResult> Request(bool isVoteRequest = false)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (isVoteRequest)
            {
                if (user.CanVoteArticles)
                {
                    TempData["InfoMessage"] = "You already have permission to vote on articles.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.IsVoteRequest = true;
                ViewBag.Title = "Request Voting Permission";
                ViewBag.Description = "Why would you like to vote on articles?";
            }
            else
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = "You already have permission to write articles.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.IsVoteRequest = false;
                ViewBag.Title = "Request Writing Permission";
                ViewBag.Description = "Why would you like to write articles?";
            }
            
            return View();
        }

        // POST: Permissions/Request
        [HttpPost("Request")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request([FromForm] string reason, [FromForm] bool isVoteRequest = false)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError("", "Please provide a reason for your request.");
                
                // Set ViewBag values for the view
                if (isVoteRequest)
                {
                    ViewBag.IsVoteRequest = true;
                    ViewBag.Title = "Request Voting Permission";
                    ViewBag.Description = "Why would you like to vote on articles?";
                }
                else
                {
                    ViewBag.IsVoteRequest = false;
                    ViewBag.Title = "Request Writing Permission";
                    ViewBag.Description = "Why would you like to write articles?";
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
                    TempData["InfoMessage"] = "You already have permission to vote on articles.";
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingVoteRequest = await _permissionService.HasPendingVoteRequestAsync(user.Id);
                if (hasPendingVoteRequest)
                {
                    ModelState.AddModelError("", "You already have a pending vote permission request.");
                    ViewBag.IsVoteRequest = true;
                    ViewBag.Title = "Request Voting Permission";
                    ViewBag.Description = "Why would you like to vote on articles?";
                    return View();
                }
            }
            else
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = "You already have permission to write articles.";
                    return RedirectToAction(nameof(Index));
                }

                var hasPendingRequest = await _permissionService.HasPendingRequestAsync(user.Id);
                if (hasPendingRequest)
                {
                    ModelState.AddModelError("", "You already have a pending write permission request.");
                    ViewBag.IsVoteRequest = false;
                    ViewBag.Title = "Request Writing Permission";
                    ViewBag.Description = "Why would you like to write articles?";
                    return View();
                }
            }

            await _permissionService.CreatePermissionRequestAsync(user.Id, reason, isVoteRequest);

            TempData["SuccessMessage"] = $"Your {(isVoteRequest ? "voting" : "writing")} permission request has been submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Permissions/Process
        [HttpPost("Process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id, bool approved, string rejectionReason = null)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null || !admin.IsAdmin)
            {
                return Forbid();
            }

            await _permissionService.ProcessPermissionRequestAsync(id, admin.Id, approved, rejectionReason);

            TempData["SuccessMessage"] = $"Request has been {(approved ? "approved" : "rejected")} successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}