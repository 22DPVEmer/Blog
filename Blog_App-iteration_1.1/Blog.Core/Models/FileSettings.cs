namespace Blog.Core.Models
{
    public class FileSettings
    {
        public int MaxIndividualFileSize { get; set; }
        public int MaxTotalSize { get; set; }
        public string[] AllowedImageTypes { get; set; } = Array.Empty<string>();
    }
} 