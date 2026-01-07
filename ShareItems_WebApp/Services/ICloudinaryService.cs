namespace ShareItems_WebApp.Services
{
    /// <summary>
    /// Result returned after uploading a file to Cloudinary
    /// </summary>
    public class CloudinaryUploadResult
    {
        /// <summary>
        /// Secure HTTPS URL to access the uploaded file
        /// </summary>
        public string SecureUrl { get; set; } = string.Empty;

        /// <summary>
        /// Cloudinary public ID used for file management
        /// </summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Resource type in Cloudinary (image, video, raw)
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// File format/extension
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Size of uploaded file in bytes
        /// </summary>
        public long Bytes { get; set; }
    }

    /// <summary>
    /// Service for managing file uploads and deletions with Cloudinary
    /// </summary>
    public interface ICloudinaryService
    {
        /// <summary>
        /// Uploads a file to Cloudinary
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="code">Note code for folder organization</param>
        /// <param name="fileType">Category: document, image, video, or others</param>
        /// <returns>Upload result with secure URL and public ID</returns>
        Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, string code, string fileType);

        /// <summary>
        /// Deletes a file from Cloudinary
        /// </summary>
        /// <param name="publicId">Cloudinary public ID of the file to delete</param>
        /// <param name="resourceType">Resource type (image, video, raw)</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteFileAsync(string publicId, string resourceType);

        /// <summary>
        /// Deletes all files in a specific folder (all files for a note code)
        /// </summary>
        /// <param name="code">Note code whose files should be deleted</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteFolderAsync(string code);
    }
}
