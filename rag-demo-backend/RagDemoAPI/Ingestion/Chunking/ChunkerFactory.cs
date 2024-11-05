using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking
{
    public class ChunkerFactory(IEnumerable<IChunker> _chunkers) : IChunkerFactory
    {
        public IChunker Create(IngestDataRequest request, string filePath, string fileContent)
        {
            var usableChunkers = _chunkers.Where(ch => ch.IsSuitable(request, fileContent));

            return usableChunkers.Last();
        }

        public IEnumerable<string> GetChunkerNames()
        {
            var chunkerNames = _chunkers.Select(c => c.Name).ToList();

            return chunkerNames;
        }
    }
}
