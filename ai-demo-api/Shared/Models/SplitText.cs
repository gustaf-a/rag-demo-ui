namespace Shared.Models;

public class SplitText
{
    public string Text { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; internal set; }
}
