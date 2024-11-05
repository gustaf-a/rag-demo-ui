using System.Text.RegularExpressions;

namespace RagDemoAPI.Ingestion.PreProcessing;

public class MarkDownPreProcessor : IPreProcessor
{
    private readonly List<string> _applicableFileExtensions = [".md"];

    public string Name => nameof(MarkDownPreProcessor);

    public bool IsSuitable(string filePath)
    {
        return _applicableFileExtensions.Contains(Path.GetExtension(filePath), StringComparer.InvariantCultureIgnoreCase);
    }

    public string Execute(string fileContent)
    {
        if (string.IsNullOrEmpty(fileContent))
            return string.Empty;

        string processedContent = fileContent;

        // Remove headers (e.g., ## Header) and replace with 2 new lines to show new paragraph
        processedContent = Regex.Replace(processedContent, @"^#{1,6}\s*", @"\n\n", RegexOptions.Multiline);

        // 2. Remove bold and italics syntax (** or __), (* or _)
        processedContent = Regex.Replace(processedContent, @"(\*\*|__)(.*?)\1", "$2");
        processedContent = Regex.Replace(processedContent, @"(\*|_)(.*?)\1", "$2");

        // 3. Convert bullet points to plain text
        // Replace unordered list markers (-, *, +) with nothing
        processedContent = Regex.Replace(processedContent, @"^\s*[-*\+]\s+", "", RegexOptions.Multiline);

        // 4. Handle nested lists by removing leading spaces or replacing them with indentation if needed
        // Here, we remove leading spaces for simplicity
        processedContent = Regex.Replace(processedContent, @"^\s{2,}", "", RegexOptions.Multiline);

        // 5. Remove any remaining markdown syntax (e.g., links)
        // Convert [text](url) to just text
        processedContent = Regex.Replace(processedContent, @"\[(.*?)\]\(.*?\)", "$1");

        // 6. Remove inline code or code blocks if present
        // Remove inline code `code`
        processedContent = Regex.Replace(processedContent, @"`{1,3}(.*?)`{1,3}", "$1");
        // Remove code blocks ```code```
        processedContent = Regex.Replace(processedContent, @"```[\s\S]*?```", "", RegexOptions.Multiline);

        // 7. Remove images or replace them with alt text if necessary
        // Convert ![alt](url) to alt
        processedContent = Regex.Replace(processedContent, @"!\[(.*?)\]\(.*?\)", "$1");

        // 8. Trim whitespace from each line
        var lines = processedContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        // 9. Join the lines back into a single string
        processedContent = string.Join(Environment.NewLine, lines);

        return processedContent;
    }
}
