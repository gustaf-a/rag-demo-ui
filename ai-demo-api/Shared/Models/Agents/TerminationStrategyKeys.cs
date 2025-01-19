namespace Shared.Models.Agents;

public static class TerminationStrategyKeys
{
    public const string Agents = nameof(Agents);
    public const string MaxIterations = nameof(MaxIterations);
    
    public const string ReduceHistoryTruncateToMostRecentNMessages = nameof(ReduceHistoryTruncateToMostRecentNMessages);
    public const string ReduceHistoryBySummary = nameof(ReduceHistoryTruncateToMostRecentNMessages);
    public const string ReduceHistoryBySummarization = nameof(ReduceHistoryTruncateToMostRecentNMessages);
}