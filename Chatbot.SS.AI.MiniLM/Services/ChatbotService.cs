using LLama.Common;
using LLama;
using LLama.Sampling;
using MongoDB.Driver;

namespace Chatbot.SS.AI.MiniLM.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly InteractiveExecutor _executor;
        private readonly ChatHistory _chatHistory;

        public ChatbotService(string modelPath)
        {
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 5
            };

            var model = LLamaWeights.LoadFromFile(parameters);
            var context = model.CreateContext(parameters);
            _executor = new InteractiveExecutor(context);

            _chatHistory = new ChatHistory();
            _chatHistory.AddMessage(AuthorRole.System,
                "Transcript of a dialog where the User interacts with an AI Assistant named Bob. Bob is helpful, kind, and precise.");
            _chatHistory.AddMessage(AuthorRole.User, "Hello, Bob.");
            _chatHistory.AddMessage(AuthorRole.Assistant, "Hello! How may I assist you?");
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddMessage(AuthorRole.User, userMessage);

            var inferenceParams = new InferenceParams
            {
                MaxTokens = 256,
                AntiPrompts = new List<string> { "User:" },
                SamplingPipeline = new DefaultSamplingPipeline()
            };

            string botResponse = "";
            await foreach (var text in new ChatSession(_executor, chatHistory).ChatAsync(new ChatHistory.Message(AuthorRole.User, userMessage), inferenceParams))
            {
                botResponse += text;
            }

            // Store in MongoDB
            var newMessage = new ChatHistory
            {
                MessageUser = userMessage,
                MessageAI = botResponse
            };

            var filter = Builders<ChatMessageViewModel>.Filter.Eq("UserId", userId);
            var update = Builders<ChatMessageViewModel>.Update.Push("ChatHistory", newMessage);

            await _chatCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            return botResponse;
        }
    }
}
