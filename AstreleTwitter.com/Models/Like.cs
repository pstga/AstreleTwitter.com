using AstreleTwitter.com.Models;

namespace AstreleTwitter.com.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}