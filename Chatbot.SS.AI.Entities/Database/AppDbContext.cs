using Chatbot.SS.AI.Entities.Models;
using MongoDB.Driver;

namespace Chatbot.SS.AI.Entities.Database
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;

        public AppDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<UserToken> UserToken => _database.GetCollection<UserToken>("UserToken");

        public void EnsureIndexes()
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Role);
            var indexModel = new CreateIndexModel<User>(indexKeys);
            Users.Indexes.CreateOne(indexModel);
        }
    }
}
