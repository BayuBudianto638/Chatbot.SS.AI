using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatbot.SS.AI.Entities.Models
{
    public class ChatHistorySession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        public string Role { get; set; }

        public bool Status { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatHistoryItem> ChatHistoryItem { get; set; } = new List<ChatHistoryItem>();
    }
}
