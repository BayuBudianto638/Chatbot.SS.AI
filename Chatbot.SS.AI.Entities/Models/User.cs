using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatbot.SS.AI.Entities.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("lastAccess")]
        public DateTime LastAccess { get; set; }
    }

}
