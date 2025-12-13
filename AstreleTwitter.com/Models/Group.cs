using AstreleTwitter.com.Models;
using System.ComponentModel.DataAnnotations;

namespace AstreleTwitter.com.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string? GroupImage { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}