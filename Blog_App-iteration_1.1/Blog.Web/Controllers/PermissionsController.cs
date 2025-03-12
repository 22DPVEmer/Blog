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

            var requests = user.IsAdmin
                ? await _permissionService.GetAdminPermissionRequestsAsync()
                : await _permissionService.GetUserPermissionRequestsAsync(user.Id);

            if (!user.IsAdmin && user.CanWriteArticles)
            {
                TempData["InfoMessage"] = "You already have permission to write articles.";
            }

            return View(requests);
        }

        // GET: Permissions/Request
        [HttpGet("Request")]
        public async Task<IActionResult> Request()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.CanWriteArticles)
            {
                TempData["InfoMessage"] = "You already have permission to write articles.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Permissions/Request
        [HttpPost("Request")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request([FromForm] string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError("", "Please provide a reason for your request.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (user.CanWriteArticles)
            {
                TempData["InfoMessage"] = "You already have permission to write articles.";
                return RedirectToAction(nameof(Index));
            }

            var hasPendingRequest = await _permissionService.HasPendingRequestAsync(user.Id);
            if (hasPendingRequest)
            {
                ModelState.AddModelError("", "You already have a pending request.");
                return View();
            }

            await _permissionService.CreatePermissionRequestAsync(user.Id, reason);

            TempData["SuccessMessage"] = "Your request has been submitted successfully.";
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