namespace ShareItems_WebApp.ViewModels
{
    public class FileUploadViewModel
    {
        public string Code { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
        public string FileType { get; set; } = string.Empty; // "document", "image", "video"
        public string? Pin { get; set; }
    }

    public class FileDownloadViewModel
    {
        public int FileId { get; set; }
        public string? Pin { get; set; }
    }

    public class FileListViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public string? Pin { get; set; }
    }

    public class FileDeleteViewModel
    {
        public int FileId { get; set; }
        public string? Pin { get; set; }
    }
}
