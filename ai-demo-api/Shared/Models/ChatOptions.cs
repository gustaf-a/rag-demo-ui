namespace Shared.Models;

public class ChatOptions
{
    public double? Temperature { get; set; } = 0.3;
    public IEnumerable<string>? PluginsToUse { get; set; } = [];
    public bool? PluginUseRequired { get; set; } = false;
    public bool? AllowMultiplePluginCallsPerCompletion { get; set; } = true;
    public bool? PluginsAutoInvoke { get; set; } = true;
}
