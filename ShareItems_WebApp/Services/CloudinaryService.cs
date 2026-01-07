using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using ShareItems_WebApp.Settings;

namespace ShareItems_WebApp.Services
{
    /// <summary>
    /// Implementation of Cloudinary file storage service
    /// </summary>
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private const string FolderPrefix = "codesafe";

        public CloudinaryService(
            IOptions<CloudinarySettings> cloudinarySettings,
            ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var settings = cloudinarySettings.Value;

            _logger.LogInformation(
                "Initializing Cloudinary service. CloudName: {CloudName}, ApiKey: {ApiKeyPrefix}...",
                settings.CloudName, 
                string.IsNullOrEmpty(settings.ApiKey) ? "empty" : settings.ApiKey.Substring(0, Math.Min(5, settings.ApiKey.Length)));

            if (!settings.IsValid())
            {
                var errorMsg = "Cloudinary settings are not properly configured. " +
                    "Please check CloudName, ApiKey, and ApiSecret in appsettings.json";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                var account = new Account(
                    settings.CloudName,
                    settings.ApiKey,
                    settings.ApiSecret
                );

                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true; // Always use HTTPS
                
                // Increase timeout for large file uploads (5 minutes)
                _cloudinary.Api.Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

                _logger.LogInformation("Cloudinary service initialized successfully with timeout: {Timeout} seconds", 
                    _cloudinary.Api.Timeout / 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Cloudinary service");
                throw;
            }
        }

        public async Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, string code, string fileType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be empty", nameof(code));
            }

            if (string.IsNullOrWhiteSpace(fileType))
            {
                throw new ArgumentException("FileType cannot be empty", nameof(fileType));
            }

            _logger.LogInformation(
                "Starting Cloudinary upload. FileName: {FileName}, FileType: {FileType}, Size: {Size} bytes, Code: {Code}",
                file.FileName, fileType, file.Length, code);

            try
            {
                // Generate folder path: codesafe/{code}/{fileType}
                var folder = $"{FolderPrefix}/{code}/{fileType}";

                // Generate unique file name (GUID-based)
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}";

                _logger.LogInformation(
                    "Upload parameters - Folder: {Folder}, UniqueFileName: {UniqueFileName}",
                    folder, uniqueFileName);

                // Upload using in-memory stream
                using var stream = file.OpenReadStream();

                // Determine upload parameters based on file type and upload
                CloudinaryUploadResult result;
                
                switch (fileType.ToLower())
                {
                    case "image":
                        var imageResult = await UploadImageAsync(stream, folder, uniqueFileName);
                        if (imageResult == null || imageResult.SecureUrl == null)
                        {
                            throw new Exception($"Cloudinary image upload failed: {imageResult?.Error?.Message ?? "Unknown error"}");
                        }
                        result = new CloudinaryUploadResult
                        {
                            SecureUrl = imageResult.SecureUrl.ToString(),
                            PublicId = imageResult.PublicId,
                            ResourceType = "image",
                            Format = imageResult.Format,
                            Bytes = imageResult.Bytes
                        };
                        break;

                    case "video":
                        var videoResult = await UploadVideoAsync(stream, folder, uniqueFileName);
                        if (videoResult == null || videoResult.SecureUrl == null)
                        {
                            throw new Exception($"Cloudinary video upload failed: {videoResult?.Error?.Message ?? "Unknown error"}");
                        }
                        result = new CloudinaryUploadResult
                        {
                            SecureUrl = videoResult.SecureUrl.ToString(),
                            PublicId = videoResult.PublicId,
                            ResourceType = "video",
                            Format = videoResult.Format,
                            Bytes = videoResult.Bytes
                        };
                        break;

                    default:
                        var rawResult = await UploadRawAsync(stream, folder, uniqueFileName);
                        if (rawResult == null || rawResult.SecureUrl == null)
                        {
                            throw new Exception($"Cloudinary raw upload failed: {rawResult?.Error?.Message ?? "Unknown error"}");
                        }
                        result = new CloudinaryUploadResult
                        {
                            SecureUrl = rawResult.SecureUrl.ToString(),
                            PublicId = rawResult.PublicId,
                            ResourceType = "raw",
                            Format = rawResult.Format,
                            Bytes = rawResult.Bytes
                        };
                        break;
                }

                _logger.LogInformation(
                    "Successfully uploaded file to Cloudinary. PublicId: {PublicId}, SecureUrl: {SecureUrl}",
                    result.PublicId, result.SecureUrl);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error uploading file to Cloudinary. FileName: {FileName}, Code: {Code}, FileType: {FileType}, ErrorMessage: {ErrorMessage}",
                    file.FileName, code, fileType, ex.Message);
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string publicId, string resourceType)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new ArgumentException("PublicId cannot be empty", nameof(publicId));
            }

            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = MapToCloudinaryResourceType(resourceType)
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok" || result.Result == "not found")
                {
                    _logger.LogInformation("Successfully deleted file from Cloudinary. PublicId: {PublicId}", publicId);
                    return true;
                }

                _logger.LogWarning("Failed to delete file from Cloudinary. PublicId: {PublicId}, Result: {Result}", 
                    publicId, result.Result);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Cloudinary. PublicId: {PublicId}", publicId);
                return false; // Don't throw - deletion failures shouldn't block DB operations
            }
        }

        public async Task<bool> DeleteFolderAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be empty", nameof(code));
            }

            try
            {
                var folderPath = $"{FolderPrefix}/{code}";

                // Delete all resources in the folder
                var deletionParams = new DelResParams
                {
                    Prefix = folderPath,
                    All = true
                };

                var result = await _cloudinary.DeleteResourcesAsync(deletionParams);

                _logger.LogInformation("Deleted folder from Cloudinary. Folder: {Folder}, Deleted count: {Count}",
                    folderPath, result.Deleted?.Count ?? 0);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder from Cloudinary. Code: {Code}", code);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<ImageUploadResult> UploadImageAsync(Stream stream, string folder, string fileName)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                _logger.LogInformation("Uploading image to Cloudinary. Folder: {Folder}, FileName: {FileName}", folder, fileName);
                var result = await _cloudinary.UploadAsync(uploadParams);
                
                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary image upload error: {ErrorMessage}", result.Error.Message);
                }
                
                return result;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Cloudinary upload timeout - file may be too large or connection too slow");
                throw new Exception("Upload timeout - please try with a smaller file or check your internet connection", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during image upload to Cloudinary");
                throw;
            }
        }

        private async Task<VideoUploadResult> UploadVideoAsync(Stream stream, string folder, string fileName)
        {
            try
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                _logger.LogInformation("Uploading video to Cloudinary. Folder: {Folder}, FileName: {FileName}", folder, fileName);
                var result = await _cloudinary.UploadAsync(uploadParams);
                
                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary video upload error: {ErrorMessage}", result.Error.Message);
                }
                
                return result;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Cloudinary upload timeout - file may be too large or connection too slow");
                throw new Exception("Upload timeout - please try with a smaller file or check your internet connection", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during video upload to Cloudinary");
                throw;
            }
        }

        private async Task<RawUploadResult> UploadRawAsync(Stream stream, string folder, string fileName)
        {
            try
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                _logger.LogInformation("Uploading raw file to Cloudinary. Folder: {Folder}, FileName: {FileName}", folder, fileName);
                var result = await _cloudinary.UploadAsync(uploadParams);
                
                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary raw upload error: {ErrorMessage}", result.Error.Message);
                }
                
                return result;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Cloudinary upload timeout - file may be too large or connection too slow");
                throw new Exception("Upload timeout - please try with a smaller file or check your internet connection", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during raw file upload to Cloudinary");
                throw;
            }
        }

        private static ResourceType MapToCloudinaryResourceType(string resourceType)
        {
            return resourceType?.ToLower() switch
            {
                "image" => ResourceType.Image,
                "video" => ResourceType.Video,
                "raw" => ResourceType.Raw,
                _ => ResourceType.Raw
            };
        }

        #endregion
    }
}
