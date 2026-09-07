namespace AstreleTwitter.com.Models
{
    public class GroupComment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int GroupMessageId { get; set; }
        public virtual GroupMessage GroupMessage { get; set; }
    }
}