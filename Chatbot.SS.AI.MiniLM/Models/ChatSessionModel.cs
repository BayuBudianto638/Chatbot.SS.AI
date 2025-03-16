using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatbot.SS.AI.MiniLM.Models
{
    public class ChatSessionModel
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("UserId")]
        public ObjectId UserId { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; } = "USER";

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ChatHistory")]
        public List<ChatHistoryItem> ChatHistory { get; set; } = new();
    }
}
