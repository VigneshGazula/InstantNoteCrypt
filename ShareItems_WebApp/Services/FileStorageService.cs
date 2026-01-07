using Microsoft.EntityFrameworkCore;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Helpers;

namespace ShareItems_WebApp.Services
{
    /// <summary>
    /// File storage service using Cloudinary as the storage backend
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly UserContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            UserContext context,
            ICloudinaryService cloudinaryService,
            ILogger<FileStorageService> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<NoteFile> SaveFileAsync(int noteId, IFormFile file, string fileType, string code)
        {
            // Input validation
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
                throw new ArgumentException("File type cannot be empty", nameof(fileType));
            }

            if (noteId <= 0)
            {
                throw new ArgumentException("Invalid note ID", nameof(noteId));
            }

            // Validate file size
            if (!FileValidationHelper.IsValidFileSize(file.Length))
            {
                throw new InvalidOperationException(
                    $"File size exceeds maximum allowed size of {FileValidationHelper.GetFileSizeString(FileValidationHelper.MaxFileSize)}");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            
            if (string.IsNullOrEmpty(extension))
            {
                throw new InvalidOperationException("File must have an extension");
            }
            
            // Check if extension is blocked
            if (FileValidationHelper.ForbiddenExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    $"File extension '{extension}' is not allowed for security reasons");
            }

            // Validate extension matches file type
            if (!FileValidationHelper.IsValidExtension(extension, fileType))
            {
                throw new InvalidOperationException(
                    $"File extension '{extension}' is not valid for file type '{fileType}'");
            }

            _logger.LogInformation(
                "Uploading file to Cloudinary. FileName: {FileName}, FileType: {FileType}, Size: {Size} bytes, NoteId: {NoteId}",
                file.FileName, fileType, file.Length, noteId);

            // Upload to Cloudinary
            CloudinaryUploadResult? uploadResult = null;
            try
            {
                uploadResult = await _cloudinaryService.UploadFileAsync(file, code, fileType);
                
                if (uploadResult == null)
                {
                    throw new Exception("Cloudinary upload returned null result");
                }

                if (string.IsNullOrWhiteSpace(uploadResult.SecureUrl))
                {
                    throw new Exception("Cloudinary upload did not return a valid URL");
                }

                if (string.IsNullOrWhiteSpace(uploadResult.PublicId))
                {
                    throw new Exception("Cloudinary upload did not return a valid PublicId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Cloudinary. FileName: {FileName}", file.FileName);
                throw new Exception("Failed to upload file to cloud storage", ex);
            }

            // Create database record
            var noteFile = new NoteFile
            {
                NoteId = noteId,
                FileName = file.FileName,
                StoredFileName = Path.GetFileName(uploadResult.PublicId) ?? Guid.NewGuid().ToString(),
                FileType = fileType.ToLower(),
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                FileUrl = uploadResult.SecureUrl,
                PublicId = uploadResult.PublicId,
                FilePath = null, // Explicitly set to null for Cloudinary storage
                UploadedAt = DateTime.UtcNow
            };

            // Save to database with retry logic
            try
            {
                _context.NoteFiles.Add(noteFile);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "File metadata saved to database. FileId: {FileId}, PublicId: {PublicId}, FileUrl: {FileUrl}",
                    noteFile.Id, noteFile.PublicId, noteFile.FileUrl);

                return noteFile;
            }
            catch (Exception ex)
            {
                // Rollback: delete file from Cloudinary if database save fails
                _logger.LogError(ex, 
                    "Error saving file metadata to database. Attempting rollback... PublicId: {PublicId}", 
                    uploadResult.PublicId);

                try
                {
                    var resourceType = DetermineResourceType(fileType);
                    await _cloudinaryService.DeleteFileAsync(uploadResult.PublicId, resourceType);
                    _logger.LogInformation("Successfully rolled back Cloudinary upload. PublicId: {PublicId}", uploadResult.PublicId);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, 
                        "Failed to rollback Cloudinary upload. File may remain orphaned. PublicId: {PublicId}", 
                        uploadResult.PublicId);
                }

                throw new Exception("Failed to save file metadata. Upload has been rolled back.", ex);
            }
        }

        public async Task<IEnumerable<NoteFile>> GetFilesByNoteIdAsync(int noteId)
        {
            return await _context.NoteFiles
                .Where(nf => nf.NoteId == noteId)
                .OrderByDescending(nf => nf.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NoteFile>> GetFilesByNoteIdAndTypeAsync(int noteId, string fileType)
        {
            return await _context.NoteFiles
                .Where(nf => nf.NoteId == noteId && nf.FileType == fileType.ToLower())
                .OrderByDescending(nf => nf.UploadedAt)
                .ToListAsync();
        }

        public async Task<NoteFile?> GetFileByIdAsync(int fileId)
        {
            return await _context.NoteFiles
                .Include(nf => nf.Note)
                .FirstOrDefaultAsync(nf => nf.Id == fileId);
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var noteFile = await _context.NoteFiles.FindAsync(fileId);
            if (noteFile == null)
            {
                _logger.LogWarning("Attempted to delete non-existent file. FileId: {FileId}", fileId);
                return false;
            }

            // Determine resource type for Cloudinary
            var resourceType = DetermineResourceType(noteFile.FileType);

            // Delete from Cloudinary
            var cloudinaryDeleted = await _cloudinaryService.DeleteFileAsync(noteFile.PublicId, resourceType);

            if (!cloudinaryDeleted)
            {
                _logger.LogWarning(
                    "Failed to delete file from Cloudinary, but continuing with database deletion. PublicId: {PublicId}",
                    noteFile.PublicId);
            }

            // Delete from database
            _context.NoteFiles.Remove(noteFile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File deleted successfully. FileId: {FileId}, PublicId: {PublicId}",
                fileId, noteFile.PublicId);

            return true;
        }

        public async Task<bool> DeleteAllNoteFilesAsync(int noteId)
        {
            if (noteId <= 0)
            {
                throw new ArgumentException("Invalid note ID", nameof(noteId));
            }

            var files = await _context.NoteFiles
                .Where(nf => nf.NoteId == noteId)
                .ToListAsync();

            if (!files.Any())
            {
                _logger.LogInformation("No files to delete for NoteId: {NoteId}", noteId);
                return true;
            }

            _logger.LogInformation("Deleting {Count} files for NoteId: {NoteId}", files.Count, noteId);

            var deletionErrors = new List<string>();

            // Delete each file from Cloudinary
            foreach (var file in files)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(file.PublicId))
                    {
                        var resourceType = DetermineResourceType(file.FileType);
                        var deleted = await _cloudinaryService.DeleteFileAsync(file.PublicId, resourceType);
                        
                        if (!deleted)
                        {
                            _logger.LogWarning(
                                "Failed to delete file from Cloudinary. FileId: {FileId}, PublicId: {PublicId}", 
                                file.Id, file.PublicId);
                            deletionErrors.Add($"FileId {file.Id}: {file.PublicId}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "File has no PublicId, skipping Cloudinary deletion. FileId: {FileId}", 
                            file.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error deleting file from Cloudinary. FileId: {FileId}, PublicId: {PublicId}", 
                        file.Id, file.PublicId);
                    deletionErrors.Add($"FileId {file.Id}: {ex.Message}");
                }
            }

            // Delete all from database regardless of Cloudinary deletion success
            try
            {
                _context.NoteFiles.RemoveRange(files);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "All file records deleted from database for NoteId: {NoteId}. Cloudinary deletion errors: {ErrorCount}",
                    noteId, deletionErrors.Count);

                if (deletionErrors.Any())
                {
                    _logger.LogWarning(
                        "Some files could not be deleted from Cloudinary: {Errors}",
                        string.Join("; ", deletionErrors));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file records from database for NoteId: {NoteId}", noteId);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Determines Cloudinary resource type based on file category
        /// </summary>
        private static string DetermineResourceType(string fileType)
        {
            return fileType?.ToLower() switch
            {
                "image" => "image",
                "video" => "video",
                _ => "raw" // documents and others use raw type
            };
        }

        #endregion
    }
}
