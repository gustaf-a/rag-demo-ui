using RagDemoAPI.Ingestion.Chunking;
using RagDemoAPI.Ingestion.PreProcessing;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Ingestion;

public class IngestionHandler(ILogger<IngestionHandler> _logger, IPostgreSqlService _postgreSqlService, IContentPreProcessorFactory _contentPreProcessorFactory, IChunkerFactory _chunkerFactory, IEmbeddingService _embeddingService) : IIngestionHandler
{
    public IEnumerable<string> GetChunkerNames()
    {
        return _chunkerFactory.GetChunkerNames();
    }

    //TODO Make general ingest from request which uses DataImporterFactory and collects data. Website, folder, file, azure container etc

    public async Task IngestDataFromFolder(IngestDataRequest request)
    {
        _logger.LogInformation($"Starting to ingest data from {request.FolderPath}.");

        var files = Directory.GetFiles(request.FolderPath);

        _logger.LogInformation($"Found {files.Count()} files to ingest.");

        await ChunkAndIngestForLocalFiles(request, files);
    }

    private async Task ChunkAndIngestForLocalFiles(IngestDataRequest request, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);

            var preProcessor = _contentPreProcessorFactory.Create(file);

            var preprocessedContent = preProcessor.DoPreProcessing(content);

            var chunker = _chunkerFactory.Create(request, file, content);

            _logger.LogInformation($"Chunking {file}: Using {chunker.Name}.");

            var chunks = await chunker.DoChunking(request, content);

            var metaData = CreateMetaDataForFile(file, request.FolderPath);
            
            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddings(chunk);

                await _postgreSqlService.InsertData(content, embedding, metaData);
            }
        }
    }

    private static EmbeddingMetaData CreateMetaDataForFile(string file, string folderPath)
    {
        //TODO Improve metadata
        return new EmbeddingMetaData
        {
            CreatedDateTime = DateTime.UtcNow,
            Source = Path.GetFileNameWithoutExtension(file),
            Uri = folderPath,
            Category = ""
        };
    }
}
