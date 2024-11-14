namespace RagDemoAPI.Models
{
    public class ChatOptions
    {
        public double? Temperature { get; set; } = 0.3;
        public IEnumerable<string>? PluginsToUse { get; set; } = [];
    }
}
