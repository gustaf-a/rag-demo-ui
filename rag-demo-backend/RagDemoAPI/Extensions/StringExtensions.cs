namespace RagDemoAPI.Extensions;

public static class StringExtensions
{
    public static string RemoveNewLines(this string value)
        => value.Replace('\r', ' ').Replace('\n', ' ');

    public static string EscapeODataValue(this string value)
        => value.Replace("'", "''");
}
