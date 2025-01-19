namespace Shared.Models.Agents;

public static class AgentTaskStatus
{
    public const string NotStarted = nameof(NotStarted);
    public const string InProgress = nameof(InProgress);
    public const string Completed = nameof(Completed);

    public const string Canceled = nameof(Canceled);
    
    public const string Paused = nameof(Paused);
    public const string Waiting = nameof(Waiting);

    public const string Blocked = nameof(Blocked);
    public const string Failed = nameof(Failed);

    public const string Retrying = nameof(Retrying);
}

