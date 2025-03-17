using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Blog.Core.Models
{
    public class ArticleCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Intro is required")]
        [StringLength(200, ErrorMessage = "Intro cannot be longer than 200 characters")]
        public string Intro { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        // No validation attribute means this is optional
        public IFormFile? FeaturedImageFile { get; set; }

        public string? UserId { get; set; }
    }
} 