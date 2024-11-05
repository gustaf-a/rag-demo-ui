using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking
{
    public class ChunkingHandlerFactory(IEnumerable<IChunkingHandler> _chunkingHandlers) : IChunkingHandlerFactory
    {
        public IChunkingHandler Create(IngestDataRequest request, string filePath, string fileContent)
        {
            var usableChunkingHandlers = _chunkingHandlers.Where(ch => ch.IsSuitable(request, fileContent));

            return usableChunkingHandlers.Last();
        }

        public IEnumerable<string> GetChunkingHandlerNames()
        {
            var chunkingHandlerNames = _chunkingHandlers.Select(c => c.Name).ToList();

            return chunkingHandlerNames;
        }
    }
}
