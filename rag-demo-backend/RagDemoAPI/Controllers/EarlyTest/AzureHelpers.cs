using Azure.AI.OpenAI.Chat;
using RagDemoAPI.Configuration;

namespace RagDemoAPI.Controllers.EarlyTest;

public static class AzureHelpers
{
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static AzureSearchChatDataSource CreateAzureSearchChatDataSource(AzureOptions azureOptions)
    {
        return new AzureSearchChatDataSource()
        {
            Endpoint = new Uri(azureOptions.SearchService.Endpoint),
            IndexName = azureOptions.SearchService.Name,
            Authentication = DataSourceAuthentication.FromApiKey(azureOptions.SearchService.ApiKey),
        };
    }
}
