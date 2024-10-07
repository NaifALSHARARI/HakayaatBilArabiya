using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HakayaatBilArabiya.Models
{
    public class Story
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public List<string> Options { get; set; } = new List<string>();
    }
}
