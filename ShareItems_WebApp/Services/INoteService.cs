using ShareItems_WebApp.Entities;

namespace ShareItems_WebApp.Services
{
    public interface INoteService
    {
        Task<Note?> GetNoteByCodeAsync(string code);
        Task<Note?> GetNoteByIdAsync(int id);
        Task<Note> CreateNoteAsync(string code, string? content = null, string? pin = null);
        Task<Note> UpdateNoteContentAsync(int noteId, string content);
        Task<Note> SetNotePinAsync(int noteId, string pin);
        Task<Note> RemoveNotePinAsync(int noteId);
        Task<bool> ValidatePinAsync(int noteId, string pin);
        Task<bool> DeleteNoteAsync(int noteId);
    }
}
