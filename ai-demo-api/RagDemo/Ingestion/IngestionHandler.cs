using RagDemoAPI.Ingestion.Chunking;
using RagDemoAPI.Ingestion.FileReaders;
using RagDemoAPI.Ingestion.PreProcessing;
using RagDemoAPI.Models;
using RagDemoAPI.Repositories;
using RagDemoAPI.Services;

namespace RagDemoAPI.Ingestion;

public class IngestionHandler(ILogger<IngestionHandler> _logger, IPostgreSqlRepository _postgreSqlService, IPreProcessorFactory _contentPreProcessorFactory, IChunkerFactory _chunkerFactory, IEmbeddingService _embeddingService) : IIngestionHandler
{
    public IEnumerable<string> GetChunkerNames()
    {
        return _chunkerFactory.GetChunkerNames();
    }

    public async Task IngestData(IngestDataRequest request)
    {
        ArgumentNullException.ThrowIfNull(nameof(request));
        ArgumentNullException.ThrowIfNull(nameof(request.DatabaseOptions));

        if (!await _postgreSqlService.DoesTableExist(request.DatabaseOptions!))
            throw new Exception($"Invalid table name: Table does not exist.");

        if(request.FolderPath != null)
        {
            _logger.LogInformation($"Starting to ingest data from local folder: {request.FolderPath}.");

            IFileReader fileReader = new LocalFileReader(request);

            await IngestFiles(fileReader, request);
        }

        if(request.IngestFromAzureContainerOptions != null)
        {
            _logger.LogInformation($"Starting to ingest data from Azure container: {request.IngestFromAzureContainerOptions.ContainerName}.");

            IFileReader fileReader = new AzureBlobFileReader(request);

            await IngestFiles(fileReader, request);
        }
    }

    private async Task IngestFiles(IFileReader fileReader, IngestDataRequest request)
    {
        var fileCount = await fileReader.GetFileCount();

        _logger.LogInformation($"Found {fileCount} files to ingest.");

        for (int i = 0; i < fileCount; i++)
        {
            var ingestionSource = await fileReader.GetNextFileContent();

            var preprocessedContent = DoPreProcessing(ingestionSource.Name, ingestionSource.Content);
            
            var chunks = await DoChunking(request, ingestionSource.Name, preprocessedContent);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddings(chunk);

                await _postgreSqlService.InsertData(request.DatabaseOptions!, chunk, embedding, ingestionSource.MetaData);
            }
        }
    }

    private string DoPreProcessing(string file, string content)
    {
        var preProcessor = _contentPreProcessorFactory.Create(file);

        _logger.LogInformation($"Preprocessing {file}: Using {preProcessor.Name}.");

        var preprocessedContent = preProcessor.Execute(content);
        return preprocessedContent;
    }

    private async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string file, string preprocessedContent)
    {
        var chunker = _chunkerFactory.Create(request, file, preprocessedContent);

        _logger.LogInformation($"Chunking {file}: Using {chunker.Name}.");

        var chunks = await chunker.Execute(request, preprocessedContent);
        return chunks;
    }
}
