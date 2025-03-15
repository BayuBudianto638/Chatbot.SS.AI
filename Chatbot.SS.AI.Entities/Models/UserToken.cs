namespace Chatbot.SS.AI.Entities.Models
{
    public class UserToken
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string RefreshToken { get; set; } = null!;

        public string? Ip { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
