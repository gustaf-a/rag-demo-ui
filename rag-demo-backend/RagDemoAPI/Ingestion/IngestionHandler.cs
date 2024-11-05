using RagDemoAPI.Ingestion.Chunking;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Ingestion;

public class IngestionHandler(ILogger<IngestionHandler> _logger, IPostgreSqlService _postgreSqlService, IChunkingHandlerFactory _chunkingHandlerFactory, IEmbeddingService _embeddingService) : IIngestionHandler
{
    public IEnumerable<string> GetChunkingHandlerNames()
    {
        return _chunkingHandlerFactory.GetChunkingHandlerNames();
    }

    //TODO Make general ingest from request which uses DataImporterFactory and collects data. Website, folder, file, azure container etc

    public async Task IngestDataFromFolderAsync(IngestDataRequest request)
    {
        _logger.LogInformation($"Starting to ingest data from {request.FolderPath}.");

        var files = Directory.GetFiles(request.FolderPath);

        _logger.LogInformation($"Found {files.Count()} files to ingest.");

        await ChunkAndIngestForLocalFilesAsync(request, files);
    }

    private async Task ChunkAndIngestForLocalFilesAsync(IngestDataRequest request, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);

            var chunkingHandler = _chunkingHandlerFactory.Create(request, file, content);

            _logger.LogInformation($"Chunking {file}: Using {chunkingHandler.Name}.");

            var chunks = await chunkingHandler.DoChunking(request, content);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddingsAsync(chunk);

                await _postgreSqlService.InsertDataAsync(content, embedding);
            }
        }
    }
}
