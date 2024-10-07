using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HakayaatBilArabiya.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int StoryId { get; set; }
        public Story Story { get; set; } = null!; 
    }

}
