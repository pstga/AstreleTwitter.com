using AstreleTwitter.com.Models;

namespace AstreleTwitter.com.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}