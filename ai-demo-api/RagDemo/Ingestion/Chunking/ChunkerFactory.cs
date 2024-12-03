using AiDemos.Api.Extensions;
using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion.Chunking
{
    public class ChunkerFactory(IEnumerable<IChunker> _chunkers) : IChunkerFactory
    {
        public IChunker Create(IngestDataRequest request, string filePath, string fileContent)
        {
            var chunkersToUse = GetSelectedChunkers(request.IngestDataOptions);

            foreach (var chunker in chunkersToUse)
                if(chunker.IsSuitable(request, fileContent))
                    return chunker;

            throw new Exception($"No suitable chunker found for file {filePath}.");
        }

        private IEnumerable<IChunker> GetSelectedChunkers(IngestDataOptions ingestDataOptions)
        {
            if (ingestDataOptions.SelectedChunkers.IsNullOrEmpty())
                return _chunkers;

            return ingestDataOptions.SelectedChunkers
                .Select(chunkerName 
                    => _chunkers.GetByClassName(chunkerName) 
                        ?? throw new Exception($"Failed to find chunker with name {chunkerName}."));
        }

        public IEnumerable<string> GetChunkerNames()
        {
            var chunkerNames = _chunkers.Select(c => c.Name).ToList();

            return chunkerNames;
        }
    }
}
