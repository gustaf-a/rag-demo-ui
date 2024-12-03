using System.Text.RegularExpressions;

namespace RagDemoAPI.Extensions;

public static class StringExtensions
{
    public static string RemoveNewLines(this string value)
        => value.Replace('\r', ' ').Replace('\n', ' ');

    public static string EscapeODataValue(this string value)
        => value.Replace("'", "''");

    public static string CleanText(this string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    public static int GetWordCount(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return SplitIntoWords(text).Length;
    }

    private static readonly string[] _paragraphSeparators = ["\r\n\r\n", "\n\n"];

    public static IEnumerable<string> SplitIntoParagraphs(this string content)
    {
        return content.Split(_paragraphSeparators, StringSplitOptions.None);
    }

    private static readonly string[] _newLineSeparators = ["\r\n", "\n"];

    public static IEnumerable<string> SplitIntoLines(this string paragraph)
    {
        return paragraph.Split(_newLineSeparators, StringSplitOptions.None);
    }

    private static readonly string _regexPunctiationWithWhiteSpace = @"(?<=[.!?])\s+";

    public static IEnumerable<string> SplitIntoSentences(this string text)
    {
        return Regex.Split(text, _regexPunctiationWithWhiteSpace)
                    .Where(sentence => !string.IsNullOrWhiteSpace(sentence));
    }

    private static readonly char[] _betweenWordsSeparator = [' '];

    public static string[] SplitIntoWords(this string cleanedParagraph)
    {
        return cleanedParagraph.Split(_betweenWordsSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static IEnumerable<string> PostgreSqlEscapeSqlIdentifier(this IEnumerable<string> identifier)
    {
        if (identifier.IsNullOrEmpty())
            return Enumerable.Empty<string>();

        return identifier.Select(i => i.PostgreSqlEscapeSqlIdentifier());
    }

    public static string PostgreSqlEscapeSqlIdentifier(this string identifier)
    {
        return identifier.Replace("\"", "\"\"");
    }

    public static IEnumerable<string> PostgreSqlEscapeSqlLiteral(this IEnumerable<string> literals)
    {
        if(literals.IsNullOrEmpty()) 
            return Enumerable.Empty<string>();

        return literals.Select(l => l.PostgreSqlEscapeSqlIdentifier());
    }

    public static string PostgreSqlEscapeSqlLiteral(this string literal)
    {
        // In PostgreSQL, single quotes are escaped by doubling them
        return literal
            .Replace("\\", "\\\\")
            .Replace("\"", "\"\"")
            .Replace("'", "''");
    }
}
