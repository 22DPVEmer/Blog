using System;

namespace Blog.Core.Models
{
    public class ImageProcessingJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TempFilePath { get; set; }
        public string SubFolder { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CallbackUrl { get; set; }
        public int? ArticleId { get; set; }
    } 
}