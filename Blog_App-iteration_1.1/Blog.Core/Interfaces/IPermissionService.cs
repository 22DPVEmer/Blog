using Blog.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<List<PermissionRequest>> GetAdminPermissionRequestsAsync();
        Task<List<PermissionRequest>> GetUserPermissionRequestsAsync(string userId);
        Task<bool> HasPendingRequestAsync(string userId);
        Task CreatePermissionRequestAsync(string userId, string reason, bool isVotePermission = false);
        Task ProcessPermissionRequestAsync(int requestId, string adminId, bool approved, string rejectionReason = null);
        Task<bool> HasPendingVoteRequestAsync(string userId);
    }
}