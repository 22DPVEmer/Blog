using System;

namespace Blog.Core.Models
{
    public class VoteModel
    {
        public bool IsUpvote { get; set; }
        
        // This is used in the adapter method to convert to the appropriate vote type
        public string VoteType => IsUpvote ? "Upvote" : "Downvote";
    }
} 