using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Blog.Core.Entities
{
    public class Rank
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinPoints { get; set; }
        public string BadgeColor { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; }

        public Rank()
        {
            Users = new HashSet<User>();
        }
    }
} 