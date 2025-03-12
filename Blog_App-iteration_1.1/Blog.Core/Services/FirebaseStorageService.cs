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
            _bucketName = "blog-ed498.firebasestorage.app";
            _logger.LogInformation("Initializing FirebaseStorageService with bucket: {BucketName}", _bucketName);

            try
            {
                // Find the credentials file
                var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blog-ed498-firebase-adminsdk-fbsvc-218aa95c14.json");
                _logger.LogInformation("Looking for credentials at: {CredentialsPath}", credentialsPath);
                
                if (!File.Exists(credentialsPath))
                {
                    // Try to find it in the project directory
                    var projectDir = Directory.GetCurrentDirectory();
                    _logger.LogInformation("Credentials not found. Trying project directory: {ProjectDir}", projectDir);
                    credentialsPath = Path.Combine(projectDir, "blog-ed498-firebase-adminsdk-fbsvc-218aa95c14.json");
                    
                    if (!File.Exists(credentialsPath))
                    {
                        // Try one directory up (for web projects)
                        var parentDir = Directory.GetParent(projectDir)?.FullName;
                        _logger.LogInformation("Credentials not found. Trying parent directory: {ParentDir}", parentDir);
                        credentialsPath = Path.Combine(parentDir, "Blog.Core", "blog-ed498-firebase-adminsdk-fbsvc-218aa95c14.json");
                        
                        // Try in the wwwroot folder
                        if (!File.Exists(credentialsPath))
                        {
                            var wwwrootPath = Path.Combine(projectDir, "wwwroot");
                            _logger.LogInformation("Credentials not found. Trying wwwroot directory: {WwwrootPath}", wwwrootPath);
                            credentialsPath = Path.Combine(wwwrootPath, "blog-ed498-firebase-adminsdk-fbsvc-218aa95c14.json");
                        }
                    }
                }

                if (!File.Exists(credentialsPath))
                {
                    _logger.LogError("Firebase credentials file not found after trying multiple locations");
                    throw new FileNotFoundException("Firebase credentials file not found. Please ensure the file exists and is accessible.");
                }

                _logger.LogInformation("Found credentials file at: {CredentialsPath}", credentialsPath);

                // Load the credential
                _credential = GoogleCredential.FromFile(credentialsPath);
                _logger.LogInformation("Successfully loaded Google credentials");

                // Check if Firebase app is already initialized
                if (FirebaseApp.DefaultInstance == null)
                {
                    var firebaseConfig = new AppOptions()
                    {
                        Credential = _credential,
                        ProjectId = "blog-ed498"
                    };

                    _firebaseApp = FirebaseApp.Create(firebaseConfig);
                    _logger.LogInformation("Created new Firebase app instance");
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    _logger.LogInformation("Using existing Firebase app instance");
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
                _logger.LogInformation("Starting image upload. File name: {FileName}, Size: {FileSize}, Subfolder: {Subfolder}", 
                    file.FileName, file.Length, subFolder);

                // Create storage client with explicit credentials
                var storage = StorageClient.Create(_credential);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                
                // Determine the object path based on the subfolder
                string objectName;
                if (string.IsNullOrEmpty(subFolder))
                {
                    objectName = $"content/{fileName}";
                }
                else
                {
                    // Clean the subfolder path to ensure proper formatting
                    subFolder = subFolder.Trim('/');
                    objectName = $"{subFolder}/{fileName}";
                }

                _logger.LogInformation("Uploading to path: {ObjectPath} in bucket: {BucketName}", objectName, _bucketName);

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    _logger.LogInformation("File loaded into memory. Size: {MemorySize} bytes", memoryStream.Length);
                    
                    // Set content type based on file extension
                    string contentType = "application/octet-stream";
                    switch (Path.GetExtension(file.FileName).ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                            contentType = "image/jpeg";
                            break;
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        case ".webp":
                            contentType = "image/webp";
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
                    
                    _logger.LogInformation("Upload successful. Object: {ObjectName}", uploadedObject.Name);
                }

                // Generate the public URL
                var publicUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
                _logger.LogInformation("Generated public URL: {PublicUrl}", publicUrl);
                
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Firebase Storage. File: {FileName}, Subfolder: {Subfolder}", 
                    file.FileName, subFolder);
                throw new Exception($"An error occurred while uploading the image: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadProfilePictureAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Starting profile picture upload. File name: {FileName}, Size: {FileSize}, Content Type: {ContentType}", 
                    file.FileName, file.Length, file.ContentType);

                // Validate file size (2MB limit for profile pictures)
                if (file.Length > 2 * 1024 * 1024)
                {
                    _logger.LogWarning("File size exceeds limit. Size: {FileSize}", file.Length);
                    throw new ArgumentException("Profile picture size must not exceed 2MB");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("Invalid file type: {ContentType}", file.ContentType);
                    throw new ArgumentException("Only JPG, PNG, and WEBP images are allowed for profile pictures");
                }

                _logger.LogInformation("Profile picture validation passed, proceeding with upload to profilepictures folder");

                // Use the UploadImageAsync method with the profilepictures subfolder
                var result = await UploadImageAsync(file, "profilepictures");
                
                _logger.LogInformation("Profile picture uploaded successfully to {Url}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture to Firebase Storage. File: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                _logger.LogInformation("Starting image deletion. URL: {ImageUrl}", imageUrl);

                // Create storage client with explicit credentials
                var storage = StorageClient.Create(_credential);
                var uri = new Uri(imageUrl);
                var objectName = uri.LocalPath.TrimStart('/');
                
                _logger.LogInformation("Deleting object: {ObjectName} from bucket: {BucketName}", objectName, _bucketName);
                
                await storage.DeleteObjectAsync(_bucketName, objectName);
                
                _logger.LogInformation("Successfully deleted object: {ObjectName}", objectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Firebase Storage. URL: {ImageUrl}", imageUrl);
                throw new Exception($"An error occurred while deleting the image: {ex.Message}", ex);
            }
        }
    }
} 