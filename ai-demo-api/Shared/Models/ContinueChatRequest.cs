using Shared.Models;

namespace Shared.Models
{
    public class ContinueChatRequest
    {
        public string PreviousChatHistoryJson { get; set; }
        public ChatRequest? ChatRequest { get; set; } = null;
    }
}
