using Shared.Models;
using System.Text;

namespace Shared.Models;

public class ChatPrompt
{
    public ChatPrompt(IEnumerable<ChatMessage> chatMessages)
    {
        foreach (var chatMessage in chatMessages)
        {
            Messages.Add(chatMessage);
        }
    }

    /// <summary>
    /// A list of chat messages.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// Adds a new message to the chat.
    /// </summary>
    /// <param name="message">The chat message to add.</param>
    public void AddMessage(ChatMessage message)
    {
        Messages.Add(message);
    }

    /// <summary>
    /// Converts the list of messages into the specified string format.
    /// </summary>
    /// <returns>A formatted string representing the chat prompt.</returns>
    public string ToFormattedString()
    {
        var sb = new StringBuilder();

        foreach (var message in Messages)
        {
            sb.AppendLine($@"<message role=""{EscapeXml(message.Role)}"">{EscapeXml(message.Content)}</message>");
        }

        return sb.ToString().TrimEnd(); // Remove the trailing newline
    }

    /// <summary>
    /// Escapes XML special characters in a string.
    /// </summary>
    /// <param name="input">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return System.Security.SecurityElement.Escape(input);
    }
}
