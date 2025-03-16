using Chatbot.SS.AI.MiniLM.Helpers;
using Chatbot.SS.AI.MiniLM.Services;
using Chatbot.SS.AI.MiniLM.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.SS.AI.MiniLM.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Req_ChatMessageVM chatMessage)
        {
            if (!ChatbotHelper.IsValidMessage(chatMessage.Message))
            {
                return BadRequest("Invalid message. Please enter a valid input.");
            }

            string response = await _chatbotService.SendMessageAsync(chatMessage.Message);

            return Ok(new { Response = ChatbotHelper.FormatResponse(response) });
        }
    }
}
