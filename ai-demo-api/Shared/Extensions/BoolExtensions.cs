namespace AiDemos.Api.Extensions;

public static class BoolExtensions
{
    public static string ToTrueFalse(this bool value)
        => value ? "true" : "false";
}
