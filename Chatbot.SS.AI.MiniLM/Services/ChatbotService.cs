using LLama.Common;
using LLama;
using LLama.Sampling;
using MongoDB.Driver;
using Chatbot.SS.AI.Entities.Models;
using Chatbot.SS.AI.Entities.Database;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Chatbot.SS.AI.AuthorizationLib.Tools;
using Chatbot.SS.AI.AuthorizationLib.Enums;

namespace Chatbot.SS.AI.MiniLM.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InteractiveExecutor _executor;
        private readonly ChatHistory _chatHistory;
        private readonly AppDbContext _appDbContext;
        private readonly AuthorizationTool _authorizationTool;
        private readonly string _modelPath = Environment.GetEnvironmentVariable("MINILM_PATH");
        public ChatbotService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            var parameters = new ModelParams(_modelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 5
            };

            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
            _authorizationTool = new AuthorizationTool(_appDbContext);

            var model = LLamaWeights.LoadFromFile(parameters);
            var context = model.CreateContext(parameters);
            _executor = new InteractiveExecutor(context);

            _chatHistory = new ChatHistory();
            _chatHistory.AddMessage(AuthorRole.System,
                "Transcript of a dialog where the User interacts with an AI Assistant named Kacrut. " +
                "Kacrut is helpful, kind, and precise. Kacrut also a Customer Service Officer that help Customer questions.");
            _chatHistory.AddMessage(AuthorRole.User, "Hello, Kacrut.");
            _chatHistory.AddMessage(AuthorRole.Assistant, "Hello! I am Kacrut. How may I assist you?");
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            var authed = await _authorizationTool.IsAuthorized(_httpContextAccessor.HttpContext.User, AuthGrantEnum.CREATE);

            if (!authed.Auth)
            {
                throw new Exception(authed.Message);
            }

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

            var newMessage = new ChatHistoryItem
            {
                MessageUser = userMessage,
                MessageAI = botResponse,
                CreatedAt = DateTime.UtcNow
            };


            var user = _httpContextAccessor.HttpContext?.User;
            string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            string sessionId = userId;
            var objectId = new ObjectId(sessionId);

            var filter = Builders<ChatHistorySession>.Filter.And(
                Builders<ChatHistorySession>.Filter.Eq(c => c.UserId, objectId),
                Builders<ChatHistorySession>.Filter.Eq(c => c.Status, true)
            );

            var data = _GetActiveChatSession(filter);

            if (data == null)
            {
                await _InsertChatSession(data.Result.Role, newMessage);
            }
            else
            {
                await _AddMessageToChatSession(filter, newMessage);
            }

            return botResponse;
        }

        public async Task<string> SendMessageNonUserAsync(string userMessage)
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

            var newMessage = new ChatHistoryItem
            {
                MessageUser = userMessage,
                MessageAI = botResponse,
                CreatedAt = DateTime.UtcNow
            };


            var user = _httpContextAccessor.HttpContext?.User;
            string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            string sessionId = userId;
            var objectId = new ObjectId(sessionId);

            var filter = Builders<ChatHistorySession>.Filter.And(
                Builders<ChatHistorySession>.Filter.Eq(c => c.UserId, objectId),
                Builders<ChatHistorySession>.Filter.Eq(c => c.Status, true)
            );

            var data = _GetActiveChatSession(filter);

            if (data == null)
            {
                await _InsertChatSession(data.Result.Role, newMessage);
            }
            else
            {
                await _AddMessageToChatSession(filter, newMessage);
            }

            return botResponse;
        }

        private async Task _InsertChatSession(string role, ChatHistoryItem chatHistoryItem)
        {
            var chatSession = new ChatHistorySession
            {
                UserId = chatHistoryItem.UserId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                ChatHistoryItem = new List<ChatHistoryItem>
                {
                    new ChatHistoryItem
                    {
                        Id = ObjectId.GenerateNewId(),
                        MessageUser = chatHistoryItem.MessageUser,
                        MessageAI = chatHistoryItem.MessageAI,
                        CreatedAt = DateTime.UtcNow.AddMinutes(1)
                    }
                }
            };

            await _appDbContext.ChatSessions.InsertOneAsync(chatSession);
        }

        private async Task _AddMessageToChatSession(FilterDefinition<ChatHistorySession> filter, ChatHistoryItem chatHistoryItem)
        {
            var newChatItem = new ChatHistoryItem
            {
                Id = ObjectId.GenerateNewId(),
                MessageUser = chatHistoryItem.MessageUser,
                MessageAI = chatHistoryItem.MessageAI,
                CreatedAt = DateTime.UtcNow.AddMinutes(1)
            };

            var update = Builders<ChatHistorySession>.Update
                .Push(c => c.ChatHistoryItem, newChatItem);

            var result = await _appDbContext.ChatSessions.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                Console.WriteLine("Message added successfully!");
            }
            else
            {
                Console.WriteLine("Session not found!");
            }
        }

        private async Task<List<ChatHistorySession>> _GetChatSessions()
        {
            return await _appDbContext.ChatSessions.Find(_ => true).ToListAsync();
        }

        private async Task<ChatHistorySession> _GetActiveChatSession(FilterDefinition<ChatHistorySession> filter)
        {
            var data = await _appDbContext.ChatSessions.Find(filter).FirstOrDefaultAsync();
            return data;
        }
    }
}
