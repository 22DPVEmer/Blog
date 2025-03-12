
using Microsoft.AspNetCore.Http;

namespace Blog.Core.Interfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string subFolder = "");
        Task<string> UploadProfilePictureAsync(IFormFile file);
        Task DeleteImageAsync(string imageUrl);
    }
}
