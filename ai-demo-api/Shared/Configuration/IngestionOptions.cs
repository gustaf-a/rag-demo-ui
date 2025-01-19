namespace Shared.Configuration;

public class IngestionOptions
{
    public const string Ingestion = "Ingestion";

    public int SlidingWindowWordsPerChunk { get; set; } = 60;
    public int SlidingWindowOverlapWords { get; set; } = 10;         // Overlap size that ensures contextual continuity.
    public int MaxChunkWords { get; set; } = 200;                    // Maximum chunk size to prevent overly large chunks.
    public int ContextualRetrievalWordsPerChunk { get; set; } = 40;    // Smaller window size for contextual retrieval.
}
