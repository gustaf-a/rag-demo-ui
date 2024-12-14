namespace Shared.Models;

public static class ProcessStatus
{
    public const string StartScheduled = nameof(StartScheduled);

    public const string NotStarted = nameof(NotStarted);
    public const string InProgress = nameof(InProgress);
    public const string Completed = nameof(Completed);
    public const string Skipped = nameof(Skipped);
    
    public const string Failed = nameof(Failed);
    public const string Paused = nameof(Paused);
    public const string Canceled = nameof(Canceled);
    public const string Waiting = nameof(Waiting);
    public const string Retrying = nameof(Retrying);
    public const string Blocked = nameof(Blocked);
}

