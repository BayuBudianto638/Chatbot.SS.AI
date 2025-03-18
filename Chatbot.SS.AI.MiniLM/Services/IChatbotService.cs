namespace Chatbot.SS.AI.MiniLM.Services
{
    public interface IChatbotService
    {
        Task<string> SendMessageAsync(string userMessage);
        Task<string> SendMessageNonUserAsync(string userMessage);
    }
}
