namespace Shared.Models;

public class ChatMessage(string role, string content)
{
    /// <summary>
    /// The role of the message sender, e.g., "system", "user", "assistant".
    /// </summary>
    public string Role { get; set; } = role;

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Content { get; set; } = content;
}

