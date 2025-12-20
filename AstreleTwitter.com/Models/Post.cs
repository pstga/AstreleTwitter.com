using AstreleTwitter.com.Models;
using System.ComponentModel.DataAnnotations;

namespace AstreleTwitter.com.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Postarea nu poate fi goală.")]
        public string Content { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public int? OriginalPostId { get; set; }
        public virtual Post OriginalPost { get; set; }
        public virtual ICollection<Post> Reposts { get; set; }
        public string? MediaPath { get; set; }
    }
}