using ShareItems_WebApp.Entities;

namespace ShareItems_WebApp.Services
{
    public interface IFileStorageService
    {
        Task<NoteFile> SaveFileAsync(int noteId, IFormFile file, string fileType, string code);
        Task<IEnumerable<NoteFile>> GetFilesByNoteIdAsync(int noteId);
        Task<IEnumerable<NoteFile>> GetFilesByNoteIdAndTypeAsync(int noteId, string fileType);
        Task<NoteFile?> GetFileByIdAsync(int fileId);
        Task<bool> DeleteFileAsync(int fileId);
        Task<bool> DeleteAllNoteFilesAsync(int noteId);
        string GetPhysicalPath(string filePath);
    }
}
