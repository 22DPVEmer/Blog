using System;

namespace Blog.Web.Models
{
    public class CommentReportViewModel
    {
        public int CommentId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }
} 