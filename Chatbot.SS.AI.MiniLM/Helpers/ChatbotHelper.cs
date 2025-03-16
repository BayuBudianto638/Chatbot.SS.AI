namespace Chatbot.SS.AI.MiniLM.Helpers
{
    public static class ChatbotHelper
    {
        public static bool IsValidMessage(string message)
        {
            return !string.IsNullOrWhiteSpace(message) && message.Length < 500;
        }

        public static string FormatResponse(string response)
        {
            return response.Trim();
        }
    }
}
