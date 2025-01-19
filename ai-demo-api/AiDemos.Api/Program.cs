
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using AiDemos.Api.Generation;
using AiDemos.Api.Ingestion;
using AiDemos.Api.Ingestion.Chunking;
using AiDemos.Api.Ingestion.PreProcessing;
using Shared.Plugins;
using AiDemos.Api.Retrieval;
using System.Reflection;
using Shared.Repositories;
using Shared.Services.Search;
using ProcessDemo.Processes;
using Shared.Search;
using ProcessDemo.Steps;
using ProcessDemo.Steps.StepServices;
using Shared.Configuration;
using Shared.Generation.LlmServices;
using Shared.Services;
using AgentDemo;
using AgentDemo.Agents;
using AgentDemo.TerminationStrategies;
using Shared.LlmServices;

namespace AiDemos.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));

        builder.Services.AddControllers();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy.WithOrigins("http://localhost:8080")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Demo API", Version = "v1" });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        builder.Services.AddHttpClient();

        SetupServices(builder);

        SetupAzure(builder);

        SetupSemanticKernel(builder);

        var app = builder.Build();

        //if (app.Environment.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void SetupAzure(WebApplicationBuilder builder)
    {
        var azureSettings = builder.Configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>();

        builder.Services.AddAzureOpenAIChatCompletion(
                deploymentName: azureSettings.ChatService.Name,
                endpoint: azureSettings.ChatService.Endpoint,
                apiKey: azureSettings.ChatService.ApiKey);
    }

    private static void SetupSemanticKernel(WebApplicationBuilder builder)
    {
        var azureSettings = builder.Configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>();

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddAzureOpenAITextEmbeddingGeneration(
            azureSettings.EmbeddingService.Name,
            azureSettings.EmbeddingService.Endpoint,
            azureSettings.EmbeddingService.ApiKey
            );

        builder.Services.AddTransient((serviceProvider) =>
        {
            return new Kernel(serviceProvider);
        });
    }

    private static void SetupServices(WebApplicationBuilder builder)
    {
        #region RAG
        builder.Services.AddScoped<IIngestionHandler, IngestionHandler>();

        builder.Services.AddScoped<IPreProcessorFactory, PreProcessorFactory>();
        builder.Services.AddScoped<IPreProcessor, MarkDownPreProcessor>();
        builder.Services.AddScoped<IPreProcessor, DoNothingPreProcessor>();

        builder.Services.AddScoped<IChunkerFactory, ChunkerFactory>();
        builder.Services.AddScoped<IChunker, DoNothingChunker>();
        builder.Services.AddScoped<IChunker, SlidingWindowChunker>();
        builder.Services.AddScoped<IChunker, ContextualChunker>();

        builder.Services.AddScoped<IGenerationHandler, GenerationHandler>();
        builder.Services.AddScoped<ILlmServiceFactory, LlmServiceFactory>();
        builder.Services.AddScoped<ILlmService, LlmServiceAzure>();
        builder.Services.AddScoped<ILlmService, LlmServiceSemanticKernel>();

        builder.Services.AddScoped<IPluginHandler, PluginHandler>();
        builder.Services.AddScoped<IPlugin, DatePlugin>();
        builder.Services.AddScoped<IPlugin, SearchDatabasePlugin>();
        builder.Services.AddScoped<IPlugin, TimePlugin>();

        builder.Services.AddScoped<IRetrievalHandler, RetrievalHandler>();
        builder.Services.AddScoped<ISearchServiceFactory, SearchServiceFactory>();
        builder.Services.AddScoped<ISearchService, SearchServiceAzure>();
        builder.Services.AddScoped<ISearchService, SearchServicePostgreSql>();

        builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();

        builder.Services.AddSingleton<IRagRepository, RagRepository>();
        #endregion


        #region Processes

        builder.Services.AddScoped<IProcessHandler, ProcessHandler>();
        builder.Services.AddScoped<IProcessExecutor, ProcessExecutor>();
        builder.Services.AddScoped<IProcessRepository, ProcessRepository>();

        builder.Services.AddScoped<IStepClassFactory, StepClassFactory>();
        #endregion

        #region Agents
        builder.Services.AddScoped<IAgentFactory, AgentFactory>();
        builder.Services.AddScoped<ITerminationStrategyFactory, TerminationStrategyFactory>();
        builder.Services.AddScoped<IAgentHandler, AgentHandler>();
        builder.Services.AddScoped<IAgentRepository, AgentRepository>();
        builder.Services.AddScoped<IAgentTaskExecutor, AgentTaskExecutor>();
        builder.Services.AddScoped<IKernelCreator, KernelCreator>();

        #endregion


        builder.Services.AddScoped<IFileService, FileService>();
    }
}
