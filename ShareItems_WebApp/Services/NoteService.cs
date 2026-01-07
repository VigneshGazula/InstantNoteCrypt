using Microsoft.EntityFrameworkCore;
using ShareItems_WebApp.Entities;

namespace ShareItems_WebApp.Services
{
    public class NoteService : INoteService
    {
        private readonly UserContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<NoteService> _logger;

        public NoteService(
            UserContext context, 
            IEncryptionService encryptionService,
            IFileStorageService fileStorageService,
            ILogger<NoteService> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<Note?> GetNoteByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            return await _context.Notes
                .Include(n => n.Files)
                .FirstOrDefaultAsync(n => n.Code == code);
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _context.Notes
                .Include(n => n.Files)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Note> CreateNoteAsync(string code, string? content = null, string? pin = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be empty");
            }

            // Check if code already exists
            var existing = await GetNoteByCodeAsync(code);
            if (existing != null)
            {
                throw new InvalidOperationException($"Note with code '{code}' already exists");
            }

            var note = new Note
            {
                Code = code,
                Content = content,
                Pin = !string.IsNullOrWhiteSpace(pin) ? _encryptionService.EncryptData(pin) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created note with code: {code}");
            return note;
        }

        public async Task<Note> UpdateNoteContentAsync(int noteId, string content)
        {
            var note = await GetNoteByIdAsync(noteId);
            if (note == null)
            {
                throw new ArgumentException($"Note with ID {noteId} not found");
            }

            note.Content = content;
            note.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(note);
            await _context.SaveChangesAsync();

            return note;
        }

        public async Task<Note> SetNotePinAsync(int noteId, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                throw new ArgumentException("PIN cannot be empty");
            }

            var note = await GetNoteByIdAsync(noteId);
            if (note == null)
            {
                throw new ArgumentException($"Note with ID {noteId} not found");
            }

            note.Pin = _encryptionService.EncryptData(pin);
            note.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Set PIN for note ID: {noteId}");
            return note;
        }

        public async Task<Note> RemoveNotePinAsync(int noteId)
        {
            var note = await GetNoteByIdAsync(noteId);
            if (note == null)
            {
                throw new ArgumentException($"Note with ID {noteId} not found");
            }

            note.Pin = null;
            note.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Removed PIN for note ID: {noteId}");
            return note;
        }

        public async Task<bool> ValidatePinAsync(int noteId, string pin)
        {
            var note = await GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(note.Pin))
            {
                return true; // No PIN set
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                return false;
            }

            try
            {
                var decryptedPin = _encryptionService.DecryptData(note.Pin);
                return decryptedPin == pin;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoteAsync(int noteId)
        {
            var note = await GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return false;
            }

            // Delete all associated files
            await _fileStorageService.DeleteAllNoteFilesAsync(noteId);

            // Delete the note
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted note ID: {noteId}");
            return true;
        }
    }
}
