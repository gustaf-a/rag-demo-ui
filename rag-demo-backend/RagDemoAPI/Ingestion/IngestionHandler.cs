using RagDemoAPI.Ingestion.Chunking;
using RagDemoAPI.Ingestion.PreProcessing;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Ingestion;

public class IngestionHandler(ILogger<IngestionHandler> _logger, IPostgreSqlService _postgreSqlService, IPreProcessorFactory _contentPreProcessorFactory, IChunkerFactory _chunkerFactory, IMetaDataCreatorFactory _metaDataCreatorFactory,IEmbeddingService _embeddingService) : IIngestionHandler
{
    public IEnumerable<string> GetChunkerNames()
    {
        return _chunkerFactory.GetChunkerNames();
    }
    

    public async Task IngestDataFromFolder(IngestDataRequest request)
    {
        _logger.LogInformation($"Starting to ingest data from {request.FolderPath}.");

        //TODO Make general file-getter ingest from request which uses DataImporterFactory and collects data. Website, folder, file, azure container etc
        var files = Directory.GetFiles(request.FolderPath);

        _logger.LogInformation($"Found {files.Count()} files to ingest.");

        await IngestLocalFiles(request, files);
    }

    private async Task IngestLocalFiles(IngestDataRequest request, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            //TODO Replace with data importer
            var content = await File.ReadAllTextAsync(file);

            var preprocessedContent = DoPreProcessing(_contentPreProcessorFactory, file, content);
            
            var chunks = await DoChunking(_chunkerFactory, request, file, preprocessedContent);

            var metaData = CreateMetaData(request, file, preprocessedContent);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddings(chunk);

                await _postgreSqlService.InsertData(content, embedding, metaData);
            }
        }
    }

    private string DoPreProcessing(IPreProcessorFactory _contentPreProcessorFactory, string file, string content)
    {
        var preProcessor = _contentPreProcessorFactory.Create(file);

        _logger.LogInformation($"Preprocessing {file}: Using {preProcessor.Name}.");

        var preprocessedContent = preProcessor.Execute(content);
        return preprocessedContent;
    }

    private async Task<IEnumerable<string>> DoChunking(IChunkerFactory _chunkerFactory, IngestDataRequest request, string file, string preprocessedContent)
    {
        var chunker = _chunkerFactory.Create(request, file, preprocessedContent);

        _logger.LogInformation($"Chunking {file}: Using {chunker.Name}.");

        var chunks = await chunker.Execute(request, preprocessedContent);
        return chunks;
    }

    private EmbeddingMetaData CreateMetaData(IngestDataRequest request, string filePath, string content)
    {
        var metaDataCreator = _metaDataCreatorFactory.Create(request, filePath, content);

        _logger.LogInformation($"Creating meta data for {filePath}: Using {metaDataCreator.Name}.");

        return metaDataCreator.Execute(request, filePath, content);
    }
}
