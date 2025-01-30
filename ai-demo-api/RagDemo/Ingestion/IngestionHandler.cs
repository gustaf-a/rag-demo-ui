using Microsoft.Extensions.Logging;
using AiDemos.Api.Ingestion.Chunking;
using AiDemos.Api.Ingestion.FileReaders;
using AiDemos.Api.Ingestion.PreProcessing;
using Shared.Models;
using Shared.Repositories;
using Shared.Services;
using System.Text.Json;
using System.Text;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Shared.Configuration;

namespace AiDemos.Api.Ingestion;

public class IngestionHandler(IConfiguration configuration, ILogger<IngestionHandler> _logger, IRagRepository _postgreSqlService, IPreProcessorFactory _contentPreProcessorFactory, IChunkerFactory _chunkerFactory, IEmbeddingService _embeddingService) : IIngestionHandler
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    public IEnumerable<string> GetChunkerNames()
    {
        return _chunkerFactory.GetChunkerNames();
    }

    public async Task IngestData(IngestDataRequest request)
    {
        ArgumentNullException.ThrowIfNull(nameof(request));
        ArgumentNullException.ThrowIfNull(nameof(request.DatabaseOptions));

        if (!await _postgreSqlService.DoesTableExist(request.DatabaseOptions.TableName!))
            throw new Exception($"Invalid table name: Table does not exist.");

        IFileReader fileReader;

        if (request.FolderPath != null)
        {
            if (!request.IngestDataOptions.DoLocalIndexing)
                throw new NotSupportedException($"Queue based indexing not supported for local files.");

            _logger.LogInformation($"Starting to ingest data from local folder: {request.FolderPath}.");

            fileReader = new LocalFileReader(request);
        }
        else if (request.IngestFromAzureContainerOptions != null)
        {
            _logger.LogInformation($"Starting to ingest data from Azure container: {request.IngestFromAzureContainerOptions.RootContainerName}.");

            fileReader = new AzureBlobFileReader(request);
        }
        else
        {
            throw new NotSupportedException($"Failed to find valid file source.");
        }

        if (request.IngestDataOptions.DoLocalIndexing)
        {
            var fileCount = await fileReader.GetFileCount();
            _logger.LogInformation($"Found {fileCount} files to ingest locally.");

            for (int i = 0; i < fileCount; i++)
                await DoLocalIndexing(fileReader, request);

            _logger.LogInformation($"Finished ingesting {fileCount} files locally.");
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(_azureOptions.IndexingQueueInput.BlobConnectionString));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(_azureOptions.IndexingQueueInput.QueueName));

            var messages = await CreateQueueMessages(fileReader, request);

            _logger.LogInformation($"Sending {messages.Count()} files to queue based ingestion.");

            var queueClient = new QueueClient(_azureOptions.IndexingQueueInput.BlobConnectionString, _azureOptions.IndexingQueueInput.QueueName);

            foreach (var message in messages)
                await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            _logger.LogInformation($"Finished sending {messages.Count()} files for queue based ingestion.");
        }
    }

    private async Task<IEnumerable<string>> CreateQueueMessages(IFileReader fileReader, IngestDataRequest request)
    {
        var fileCount = await fileReader.GetFileCount();

        List<string> serializedMessages = [];

        for (int i = 0; i < fileCount; i++)
        {
            var ingestionSource = await fileReader.GetNextFile(includeContent: false);

            var indexFileRequestQueueMessage = new IndexFileRequestQueueMessage()
            {
                FileName = ingestionSource.Name,
                Metadata = ingestionSource.MetaData,
                IngestDataRequest = request
            };

            var messageJson = JsonSerializer.Serialize(indexFileRequestQueueMessage);
        }

        return serializedMessages;
    }

    private async Task DoLocalIndexing(IFileReader fileReader, IngestDataRequest request)
    {
        var ingestionSource = await fileReader.GetNextFile(includeContent: true);

        var preprocessedContent = DoPreProcessing(request, ingestionSource.Name, ingestionSource.Content);

        var chunks = await DoChunking(request, ingestionSource.Name, preprocessedContent);

        ingestionSource.MetaData.SourceTotalChunkNumbers = chunks.Count - 1;
        
        await _postgreSqlService.SaveIngestionSource(ingestionSource);

        ingestionSource.MetaData.Tags["source-id"] = ingestionSource.Id.ToString();

        for (int j = 0; j < chunks.Count; j++)
        {
            var chunk = chunks[j];

            var embedding = await _embeddingService.GetEmbeddings(chunk.EmbeddingContent);

            ingestionSource.MetaData.SourceChunkNumber = j;

            await _postgreSqlService.InsertData(request.DatabaseOptions!.TableName, chunk, embedding, ingestionSource.MetaData);
        }
    }

    private string DoPreProcessing(IngestDataRequest request, string file, string content)
    {
        if (!request.IngestDataOptions.DoPreProcessing)
        {
            return content;
        }

        var preProcessor = _contentPreProcessorFactory.Create(file);

        _logger.LogInformation($"Preprocessing {file}: Using {preProcessor.Name}.");

        var preprocessedContent = preProcessor.Execute(content);
        return preprocessedContent;
    }

    private async Task<List<ContentChunk>> DoChunking(IngestDataRequest request, string file, string preprocessedContent)
    {
        var chunker = _chunkerFactory.Create(request, file, preprocessedContent);

        _logger.LogInformation($"Chunking {file}: Using {chunker.Name}.");

        var chunks = await chunker.Execute(request, preprocessedContent);

        return chunks.ToList();
    }
}
