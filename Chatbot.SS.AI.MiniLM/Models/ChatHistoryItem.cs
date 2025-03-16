using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Chatbot.SS.AI.MiniLM.Models
{
    public class ChatHistoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("MessageUser")]
        public string MessageUser { get; set; }

        [BsonElement("MessageAI")]
        public string MessageAI { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
