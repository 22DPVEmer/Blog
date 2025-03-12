using Blog.Core.Interfaces;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermissionRequest>> GetAdminPermissionRequestsAsync()
        {
            return await _context.PermissionRequests
                .Include(pr => pr.User)
                .Include(pr => pr.ProcessedByUser)
                .OrderByDescending(pr => pr.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<PermissionRequest>> GetUserPermissionRequestsAsync(string userId)
        {
            return await _context.PermissionRequests
                .Include(pr => pr.ProcessedByUser)
                .Where(pr => pr.UserId == userId)
                .OrderByDescending(pr => pr.RequestedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequestAsync(string userId)
        {
            return await _context.PermissionRequests
                .AnyAsync(pr => pr.UserId == userId && pr.Status == PermissionRequestStatus.Pending);
        }

        public async Task CreatePermissionRequestAsync(string userId, string reason)
        {
            var request = new PermissionRequest
            {
                UserId = userId,
                Reason = reason
            };

            _context.PermissionRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task ProcessPermissionRequestAsync(int requestId, string adminId, bool approved, string rejectionReason = null)
        {
            var request = await _context.PermissionRequests
                .Include(pr => pr.User)
                .FirstOrDefaultAsync(pr => pr.Id == requestId);

            if (request == null) return;

            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedByUserId = adminId;
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
        }
    }
}