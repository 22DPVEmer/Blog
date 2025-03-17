using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FirebaseAdmin;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Blog.Core.Interfaces;
using Google.Apis.Storage.v1.Data;
using Blog.Core.Constants;

namespace Blog.Core.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly string _bucketName;
        private readonly GoogleCredential _credential;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(IConfiguration configuration, ILogger<FirebaseStorageService> logger)
        {
            _logger = logger;
            _bucketName = FirebaseConstants.BucketName;

            try
            {
                // Find the credentials file
                var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FirebaseConstants.CredentialsFileName);
                
                if (!File.Exists(credentialsPath))
                {
                    // Try to find it in the project directory
                    var projectDir = Directory.GetCurrentDirectory();
                    credentialsPath = Path.Combine(projectDir, FirebaseConstants.CredentialsFileName);
                    
                    if (!File.Exists(credentialsPath))
                    {
                        // Try one directory up (for web projects)
                        var parentDir = Directory.GetParent(projectDir)?.FullName;
                        credentialsPath = Path.Combine(parentDir, "Blog.Core", FirebaseConstants.CredentialsFileName);
                        
                        // Try in the wwwroot folder
                        if (!File.Exists(credentialsPath))
                        {
                            var wwwrootPath = Path.Combine(projectDir, "wwwroot");
                            credentialsPath = Path.Combine(wwwrootPath, FirebaseConstants.CredentialsFileName);
                        }
                    }
                }

                if (!File.Exists(credentialsPath))
                {
                    _logger.LogError(FirebaseConstants.Messages.CredentialsNotFound);
                    throw new FileNotFoundException(FirebaseConstants.Messages.CredentialsNotFound);
                }

                // Load the credential
                _credential = GoogleCredential.FromFile(credentialsPath);

                // Check if Firebase app is already initialized
                if (FirebaseApp.DefaultInstance == null)
                {
                    var firebaseConfig = new AppOptions()
                    {
                        Credential = _credential,
                        ProjectId = FirebaseConstants.ProjectId
                    };

                    _firebaseApp = FirebaseApp.Create(firebaseConfig);
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Storage Service");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string subFolder = "")
        {
            try
            {
                // Create storage client with explicit credentials
                var storage = StorageClient.Create(_credential);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                
                // Determine the object path based on the subfolder
                string objectName;
                if (string.IsNullOrEmpty(subFolder))
                {
                    objectName = $"{FirebaseConstants.Folders.Content}/{fileName}";
                }
                else
                {
                    // Clean the subfolder path to ensure proper formatting
                    subFolder = subFolder.Trim('/');
                    objectName = $"{subFolder}/{fileName}";
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    
                    // Set content type based on file extension
                    string contentType = FirebaseConstants.ContentTypes.OctetStream;
                    switch (Path.GetExtension(file.FileName).ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                            contentType = FirebaseConstants.ContentTypes.Jpeg;
                            break;
                        case ".png":
                            contentType = FirebaseConstants.ContentTypes.Png;
                            break;
                        case ".gif":
                            contentType = FirebaseConstants.ContentTypes.Gif;
                            break;
                        case ".webp":
                            contentType = FirebaseConstants.ContentTypes.Webp;
                            break;
                    }
                    
                    // Upload with public read access
                    var uploadOptions = new UploadObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    };
                    
                    var uploadedObject = await storage.UploadObjectAsync(
                        _bucketName, 
                        objectName, 
                        contentType, 
                        memoryStream,
                        options: uploadOptions);
                }

                // Generate the public URL
                var publicUrl = string.Format(FirebaseConstants.Urls.StorageBaseUrl, _bucketName, objectName);
                
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Firebase Storage");
                throw new Exception(string.Format(FirebaseConstants.Messages.UploadError, ex.Message), ex);
            }
        }

        public async Task<string> UploadProfilePictureAsync(IFormFile file)
        {
            try
            {
                // Validate file size (2MB limit for profile pictures)
                if (file.Length > FirebaseConstants.Limits.ProfilePictureMaxSize)
                {
                    throw new ArgumentException(FirebaseConstants.Messages.ProfilePictureSizeExceeded);
                }

                // Validate file type
                var allowedTypes = new[] { 
                    FirebaseConstants.ContentTypes.Jpeg, 
                    FirebaseConstants.ContentTypes.Png, 
                    FirebaseConstants.ContentTypes.Webp 
                };
                
                if (!allowedTypes.Contains(file.ContentType))
                {
                    throw new ArgumentException(FirebaseConstants.Messages.ProfilePictureInvalidType);
                }

                // Use the UploadImageAsync method with the profilepictures subfolder
                var result = await UploadImageAsync(file, FirebaseConstants.Folders.ProfilePictures);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture to Firebase Storage");
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                // Create storage client with explicit credentials
                var storage = StorageClient.Create(_credential);
                var uri = new Uri(imageUrl);
                var objectName = uri.LocalPath.TrimStart('/');
                
                await storage.DeleteObjectAsync(_bucketName, objectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Firebase Storage");
                throw new Exception(string.Format(FirebaseConstants.Messages.DeleteError, ex.Message), ex);
            }
        }
    }
} 