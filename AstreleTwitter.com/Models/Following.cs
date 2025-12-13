using AstreleTwitter.com.Models;

namespace AstreleTwitter.com.Models
{
    public class Following
    {
        public string FollowerId { get; set; }
        public virtual ApplicationUser Follower { get; set; }

        public string FollowingId { get; set; }
        public virtual ApplicationUser FollowingUser { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public string Status { get; set; }
    }
}