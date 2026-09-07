using System.ComponentModel.DataAnnotations;

namespace AstreleTwitter.com.Models
{
    public class GroupMessage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mesajul nu poate fi gol.")] 
        public string Content { get; set; }

        public string? MediaPath { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<GroupLike> GroupLikes { get; set; } = new List<GroupLike>();
        public virtual ICollection<GroupComment> GroupComments { get; set; } = new List<GroupComment>();
    }
}