namespace Shared.Models.Agents;

public static class AgentTaskPayloadKeys
{
    public const string Result = nameof(Result);

    public const string ErrorCollection = nameof(ErrorCollection);

    public const string Agents = nameof(Agents);
    
    public const string TerminationStrategyInfo = nameof(TerminationStrategyInfo);

    public const string ChatHistoryRaw = nameof(ChatHistoryRaw);

    public const string InitialAgent = nameof(InitialAgent);
}