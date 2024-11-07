using Microsoft.SemanticKernel;
using RagDemoAPI.Configuration;
using RagDemoAPI.Generation;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Ingestion;
using RagDemoAPI.Ingestion.Chunking;
using RagDemoAPI.Ingestion.MetaDataCreation;
using RagDemoAPI.Ingestion.PreProcessing;
using RagDemoAPI.Repositories;
using RagDemoAPI.Retrieval;
using RagDemoAPI.Retrieval.Search;
using RagDemoAPI.Services;

namespace RagDemoAPI
{
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
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient();

            SetupAzure(builder);
            SetupSemanticKernel(builder);

            SetupServices(builder);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

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
            builder.Services.AddScoped<IIngestionHandler, IngestionHandler>();

            builder.Services.AddScoped<IPreProcessorFactory, PreProcessorFactory>();
            builder.Services.AddScoped<IPreProcessor, MarkDownPreProcessor>();
            builder.Services.AddScoped<IPreProcessor, DoNothingPreProcessor>();

            builder.Services.AddScoped<IMetaDataCreatorFactory, MetaDataCreatorFactory>();
            builder.Services.AddScoped<IMetaDataCreator, BasicMetaDataCreator>();
            
            builder.Services.AddScoped<IChunkerFactory, ChunkerFactory>();
            builder.Services.AddScoped<IChunker, DoNothingChunker>();
            builder.Services.AddScoped<IChunker, SlidingWindowChunker>();
            builder.Services.AddScoped<IChunker, ContextualChunker>();

            builder.Services.AddScoped<IGenerationHandler, GenerationHandler>();
            builder.Services.AddScoped<ILlmServiceFactory, LlmServiceFactory>();
            builder.Services.AddScoped<ILlmService, LlmServiceAzure>();

            builder.Services.AddScoped<IRetrievalHandler, RetrievalHandler>();
            builder.Services.AddScoped<ISearchServiceFactory, SearchServiceFactory>();
            builder.Services.AddScoped<ISearchService, SearchServiceAzure>();
            builder.Services.AddScoped<ISearchService, SearchServicePostgreSql>();
            
            builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
            builder.Services.AddSingleton<IPostgreSqlRepository, PostgreSqlRepository>();
        }
    }
}
