using System.Text.RegularExpressions;
using Shared.Extensions;
using Shared.Models;

namespace Shared.Extensions;

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

        return SplitIntoWords(text, 0).Count();
    }

    private static readonly IEnumerable<string> _paragraphSeparators = ["\r\n\r\n", "\n\n"];

    public static IEnumerable<SplitText> SplitIntoParagraphs(this string content, int startingIndex)
    {
        return SplitIntoTextBySeparator(content, _paragraphSeparators, startingIndex);
    }

    private static IEnumerable<SplitText> SplitIntoTextBySeparator(string content, IEnumerable<string> paragraphSeparators, int startingIndex)
    {
        var result = new List<SplitText>();
        int currentIndex = 0;

        while (currentIndex < content.Length)
        {
            // Find the next occurrence of any known paragraph separator.
            int nextSeparatorIndex = FindNextSeparator(paragraphSeparators, content, currentIndex, out string foundSeparator);

            if (nextSeparatorIndex == -1)
            {
                // No more separators, so the remaining text is the last paragraph.
                string lastText = content.Substring(currentIndex);
                result.Add(new SplitText
                {
                    Text = lastText,
                    StartIndex = startingIndex + currentIndex,
                    EndIndex = startingIndex + currentIndex + lastText.Length - 1
                });
                break;
            }
            else
            {
                // Extract the text between currentIndex and the separator start.
                int textLength = nextSeparatorIndex - currentIndex;
                string text = content.Substring(currentIndex, textLength);

                result.Add(new SplitText
                {
                    Text = text,
                    StartIndex = startingIndex + currentIndex,
                    EndIndex = startingIndex + currentIndex + textLength
                });

                // Move currentIndex to the character after the separator.
                currentIndex = nextSeparatorIndex + foundSeparator.Length;
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the earliest next occurrence of any separator starting from startIndex.
    /// Returns the index of the separator in the content, or -1 if none is found.
    /// Also returns which separator was found via the out parameter.
    /// </summary>
    private static int FindNextSeparator(IEnumerable<string> separators, string content, int startIndex, out string foundSeparator)
    {
        int earliestIndex = -1;
        foundSeparator = null;

        foreach (var sep in separators)
        {
            int index = content.IndexOf(sep, startIndex, StringComparison.Ordinal);
            if (index != -1 && (earliestIndex == -1 || index < earliestIndex))
            {
                earliestIndex = index;
                foundSeparator = sep;
            }
        }

        return earliestIndex;
    }

    private static readonly IEnumerable<string> _newLineSeparators = ["\r\n", "\n"];

    public static IEnumerable<SplitText> SplitIntoLines(this string content, int startingIndex)
    {
        return SplitIntoTextBySeparator(content, _newLineSeparators, startingIndex);
    }

    private static readonly IEnumerable<string> _sentenceSeparators = ["?", "!", "."];

    public static IEnumerable<SplitText> SplitIntoSentences(this string content, int startingIndex)
    {
        return SplitIntoTextBySeparator(content, _sentenceSeparators, startingIndex);
    }

    private static readonly IEnumerable<string> _betweenWordsSeparator = [" "];

    public static IEnumerable<SplitText> SplitIntoWords(this string content, int startingIndex)
    {
        return SplitIntoTextBySeparator(content, _betweenWordsSeparator, startingIndex);
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
        if (literals.IsNullOrEmpty())
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