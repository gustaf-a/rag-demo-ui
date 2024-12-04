﻿namespace AiDemos.Api.Configuration;

public class AzureOptions
{
    public const string Azure = "Azure";

    public AzureServiceEndpoint ChatService { get; set; }
    public AzureServiceEndpoint SearchService { get; set; }
    public AzureServiceEndpoint EmbeddingService { get; set; }
}