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

namespace Blog.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PermissionsController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Permissions
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Set ViewBag properties before any conditional logic
            ViewBag.IsAdmin = user.IsAdmin;
            ViewBag.CanWriteArticles = user.CanWriteArticles;

            // Check if user is admin using IsAdmin property
            if (user.IsAdmin)
            {
                // Get all permission requests for admin view
                var requests = await _context.PermissionRequests
                    .Include(pr => pr.User)
                    .Include(pr => pr.ProcessedByUser)
                    .OrderByDescending(pr => pr.RequestedAt)
                    .ToListAsync();

                return View(requests);
            }
            else
            {
                if (user.CanWriteArticles)
                {
                    TempData["InfoMessage"] = "You already have permission to write articles.";
                }

                // Get user's permission requests for regular user view
                var requests = await _context.PermissionRequests
                    .Include(pr => pr.ProcessedByUser)
                    .Where(pr => pr.UserId == user.Id)
                    .OrderByDescending(pr => pr.RequestedAt)
                    .ToListAsync();

                return View(requests);
            }
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

            // Check if user already has permission
            if (user.CanWriteArticles)
            {
                TempData["InfoMessage"] = "You already have permission to write articles.";
                return RedirectToAction(nameof(Index));
            }

            // Check if there's already a pending request
            var pendingRequest = await _context.PermissionRequests
                .AnyAsync(pr => pr.UserId == user.Id && pr.Status == PermissionRequestStatus.Pending);

            if (pendingRequest)
            {
                ModelState.AddModelError("", "You already have a pending request.");
                return View();
            }

            var request = new PermissionRequest
            {
                UserId = user.Id,
                Reason = reason
            };

            _context.PermissionRequests.Add(request);
            await _context.SaveChangesAsync();

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

            var request = await _context.PermissionRequests
                .Include(pr => pr.User)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedByUserId = admin.Id;
            request.Status = approved ? PermissionRequestStatus.Approved : PermissionRequestStatus.Rejected;

            if (!approved && !string.IsNullOrWhiteSpace(rejectionReason))
            {
                request.RejectionReason = rejectionReason;
            }

            if (approved)
            {
                request.User.CanWriteArticles = true;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Request has been {(approved ? "approved" : "rejected")} successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
} 