using Microsoft.EntityFrameworkCore;
using ShareItems_WebApp.Entities;

namespace ShareItems_WebApp.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly UserContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
        {
            { "document", new[] { ".pdf", ".doc", ".docx" } },
            { "image", new[] { ".jpg", ".jpeg", ".png", ".webp" } },
            { "video", new[] { ".mp4", ".mov" } }
        };

        private static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            { "document", new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { "image", new[] { "image/jpeg", "image/png", "image/webp" } },
            { "video", new[] { "video/mp4", "video/quicktime" } }
        };

        public FileStorageService(UserContext context, IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<NoteFile> SaveFileAsync(int noteId, IFormFile file, string fileType, string code)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
            }

            // Validate file type
            if (!AllowedFileTypes.ContainsKey(fileType.ToLower()))
            {
                throw new ArgumentException($"Invalid file type: {fileType}");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedFileTypes[fileType.ToLower()].Contains(extension))
            {
                throw new ArgumentException($"File extension {extension} is not allowed for {fileType}");
            }

            // Validate MIME type
            if (!AllowedMimeTypes[fileType.ToLower()].Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException($"MIME type {file.ContentType} is not allowed for {fileType}");
            }

            // Verify note exists
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                throw new ArgumentException($"Note with ID {noteId} not found");
            }

            // Create directory structure: wwwroot/uploads/{code}/
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", code);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsPath, storedFileName);

            // Save file to disk
            try
            {
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to disk");
                throw new Exception("Failed to save file to disk", ex);
            }

            // Create metadata record
            var noteFile = new NoteFile
            {
                NoteId = noteId,
                FileName = file.FileName,
                StoredFileName = storedFileName,
                FileType = fileType.ToLower(),
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = $"uploads/{code}/{storedFileName}",
                UploadedAt = DateTime.UtcNow
            };

            // Save to database
            try
            {
                _context.NoteFiles.Add(noteFile);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Rollback: delete file from disk if database save fails
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                _logger.LogError(ex, "Error saving file metadata to database");
                throw new Exception("Failed to save file metadata", ex);
            }

            return noteFile;
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
                return false;
            }

            // Delete from disk
            var fullPath = GetPhysicalPath(noteFile.FilePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to delete file from disk: {fullPath}");
                }
            }

            // Delete from database
            _context.NoteFiles.Remove(noteFile);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAllNoteFilesAsync(int noteId)
        {
            var files = await _context.NoteFiles
                .Where(nf => nf.NoteId == noteId)
                .ToListAsync();

            foreach (var file in files)
            {
                // Delete from disk
                var fullPath = GetPhysicalPath(file.FilePath);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete file from disk: {fullPath}");
                    }
                }
            }

            // Delete from database
            _context.NoteFiles.RemoveRange(files);
            await _context.SaveChangesAsync();

            return true;
        }

        public string GetPhysicalPath(string filePath)
        {
            return Path.Combine(_environment.WebRootPath, filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
    }
}
