namespace AstreleTwitter.com.Models
{
    public class GroupLike
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int GroupMessageId { get; set; }
        public virtual GroupMessage GroupMessage { get; set; }
    }
}