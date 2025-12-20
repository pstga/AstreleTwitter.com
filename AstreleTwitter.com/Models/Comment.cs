using AstreleTwitter.com.Models;
using System.ComponentModel.DataAnnotations;

namespace AstreleTwitter.com.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comentariul nu poate fi gol.")] 
        public string Content { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}