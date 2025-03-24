using System.ComponentModel.DataAnnotations;

namespace Blog.Core.Models
{
    public class PermissionRequestViewModel
    {
        [Required]
        public string Type { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Reason { get; set; }
    }
} 