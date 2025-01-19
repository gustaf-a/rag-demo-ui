using Shared.Models.Agents;

namespace Shared.Extensions;

public static class AgentTaskPayloadExtensions
{
    public static void AddError(this AgentTaskPayload payload, Exception exception)
    {
        List<Exception> errorCollection = (List<Exception>)payload.State[AgentTaskPayloadKeys.ErrorCollection] 
                                            ?? [];

        errorCollection.Add(exception);

        payload.State[AgentTaskPayloadKeys.ErrorCollection] = errorCollection;
    }
}
