using System.ComponentModel.DataAnnotations;

namespace AstreleTwitter.com.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie")]
        public string Description { get; set; }

        public string? GroupImage { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
        public virtual ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
    }
}