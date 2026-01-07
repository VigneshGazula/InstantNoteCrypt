using Microsoft.AspNetCore.Mvc;
using ShareItems_WebApp.Services;

namespace ShareItems_WebApp.Controllers
{
    public class FileController : Controller
    {
        private readonly INoteService _noteService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileController> _logger;

        public FileController(
            INoteService noteService,
            IFileStorageService fileStorageService,
            ILogger<FileController> logger)
        {
            _noteService = noteService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a file to a note
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upload(string code, IFormFile file, string fileType, string? pin = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest("Code is required");
                }

                // Get or create note
                var note = await _noteService.GetNoteByCodeAsync(code);
                if (note == null)
                {
                    note = await _noteService.CreateNoteAsync(code);
                }

                // Validate PIN if note is protected
                if (!string.IsNullOrWhiteSpace(note.Pin))
                {
                    if (string.IsNullOrWhiteSpace(pin) || !await _noteService.ValidatePinAsync(note.Id, pin))
                    {
                        return Unauthorized("Invalid or missing PIN");
                    }
                }

                // Save file
                var noteFile = await _fileStorageService.SaveFileAsync(note.Id, file, fileType, code);

                _logger.LogInformation($"File uploaded successfully: {noteFile.FileName} for note: {code}");

                return Ok(new
                {
                    success = true,
                    fileId = noteFile.Id,
                    fileName = noteFile.FileName,
                    fileSize = noteFile.FileSize,
                    fileType = noteFile.FileType,
                    uploadedAt = noteFile.UploadedAt
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during file upload");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        /// <summary>
        /// Download a file by ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(int fileId, string? pin = null)
        {
            try
            {
                var noteFile = await _fileStorageService.GetFileByIdAsync(fileId);
                if (noteFile == null)
                {
                    return NotFound("File not found");
                }

                // Validate PIN if note is protected
                if (!string.IsNullOrWhiteSpace(noteFile.Note.Pin))
                {
                    if (string.IsNullOrWhiteSpace(pin) || !await _noteService.ValidatePinAsync(noteFile.NoteId, pin))
                    {
                        return Unauthorized("Invalid or missing PIN");
                    }
                }

                // Get physical file path
                var physicalPath = _fileStorageService.GetPhysicalPath(noteFile.FilePath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    _logger.LogError($"File not found on disk: {physicalPath}");
                    return NotFound("File not found on disk");
                }

                // Return file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                return File(fileBytes, noteFile.ContentType, noteFile.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file ID: {fileId}");
                return StatusCode(500, "An error occurred while downloading the file");
            }
        }

        /// <summary>
        /// Get all files for a note by code
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFiles(string code, string? fileType = null, string? pin = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest("Code is required");
                }

                var note = await _noteService.GetNoteByCodeAsync(code);
                if (note == null)
                {
                    return NotFound("Note not found");
                }

                // Validate PIN if note is protected
                if (!string.IsNullOrWhiteSpace(note.Pin))
                {
                    if (string.IsNullOrWhiteSpace(pin) || !await _noteService.ValidatePinAsync(note.Id, pin))
                    {
                        return Unauthorized("Invalid or missing PIN");
                    }
                }

                // Get files
                var files = string.IsNullOrWhiteSpace(fileType)
                    ? await _fileStorageService.GetFilesByNoteIdAsync(note.Id)
                    : await _fileStorageService.GetFilesByNoteIdAndTypeAsync(note.Id, fileType);

                return Ok(files.Select(f => new
                {
                    id = f.Id,
                    fileName = f.FileName,
                    fileType = f.FileType,
                    fileSize = f.FileSize,
                    contentType = f.ContentType,
                    uploadedAt = f.UploadedAt
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving files for code: {code}");
                return StatusCode(500, "An error occurred while retrieving files");
            }
        }

        /// <summary>
        /// Delete a file by ID
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int fileId, string? pin = null)
        {
            try
            {
                var noteFile = await _fileStorageService.GetFileByIdAsync(fileId);
                if (noteFile == null)
                {
                    return NotFound("File not found");
                }

                // Validate PIN if note is protected
                if (!string.IsNullOrWhiteSpace(noteFile.Note.Pin))
                {
                    if (string.IsNullOrWhiteSpace(pin) || !await _noteService.ValidatePinAsync(noteFile.NoteId, pin))
                    {
                        return Unauthorized("Invalid or missing PIN");
                    }
                }

                var success = await _fileStorageService.DeleteFileAsync(fileId);
                if (success)
                {
                    _logger.LogInformation($"File deleted successfully: {fileId}");
                    return Ok(new { success = true, message = "File deleted successfully" });
                }

                return StatusCode(500, "Failed to delete file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file ID: {fileId}");
                return StatusCode(500, "An error occurred while deleting the file");
            }
        }
    }
}
