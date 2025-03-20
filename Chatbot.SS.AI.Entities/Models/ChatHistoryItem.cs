using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Chatbot.SS.AI.Entities.Models
{
    public class ChatHistoryItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public string MessageUser { get; set; }
        public string MessageAI { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
