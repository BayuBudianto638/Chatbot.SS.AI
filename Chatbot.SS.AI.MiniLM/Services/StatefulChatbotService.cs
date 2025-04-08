using LLama;
using LLama.Sampling;
using LLama.Common;
using Chatbot.SS.AI.MiniLM.Models;

namespace Chatbot.SS.AI.MiniLM.Services
{
    public class StatefulChatbotService : IDisposable
    {
        private readonly ChatSession _session;
        private readonly LLamaContext _context;
        private readonly ILogger<StatefulChatbotService> _logger;
        private bool _continue = false;

        private const string SystemPrompt = "Transcript of a dialog, where the User interacts with an Assistant. Assistant is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.";

        public StatefulChatbotService(IConfiguration configuration, ILogger<StatefulChatbotService> logger)
        {
            var @params = new ModelParams(configuration["ModelPath"]!)
            {
                ContextSize = 512,
            };

            // todo: share weights from a central service
            using var weights = LLamaWeights.LoadFromFile(@params);

            _logger = logger;
            _context = new LLamaContext(weights, @params);

            _session = new ChatSession(new InteractiveExecutor(_context));
            _session.History.AddMessage(AuthorRole.System, SystemPrompt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<string> Send(SendMessageInput input)
        {

            if (!_continue)
            {
                _logger.LogInformation("Prompt: {text}", SystemPrompt);
                _continue = true;
            }
            _logger.LogInformation("Input: {text}", input.Text);
            var outputs = _session.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, input.Text),
                new InferenceParams
                {
                    AntiPrompts = ["User:"],

                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        RepeatPenalty = 1.0f
                    }
                });

            var result = "";
            await foreach (var output in outputs)
            {
                _logger.LogInformation("Message: {output}", output);
                result += output;
            }

            return result;
        }

        public async IAsyncEnumerable<string> SendStream(SendMessageInput input)
        {
            if (!_continue)
            {
                _logger.LogInformation(SystemPrompt);
                _continue = true;
            }

            _logger.LogInformation(input.Text);

            var outputs = _session.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, input.Text),
                new InferenceParams
                {
                    AntiPrompts = ["User:"],

                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        RepeatPenalty = 1.0f
                    }
                });

            await foreach (var output in outputs)
            {
                _logger.LogInformation(output);
                yield return output;
            }
        }
    }
}
