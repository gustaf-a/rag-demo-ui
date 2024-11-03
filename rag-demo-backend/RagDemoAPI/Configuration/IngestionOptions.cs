namespace RagDemoAPI.Configuration;

public class IngestionOptions
{
    public const string Ingestion = "Ingestion";

    public bool AllowLlmBasedChunking { get; set; }
    public int FixedSizeChunkingWordsPerChunk { get; set; } = 50;
    public int SlidingWindowWordsPerChunk { get; set; } = 40;           // Smaller window size for sliding window approaches.
    public int SlidingWindowOverlapWords { get; set; } = 5;         // Overlap size that ensures contextual continuity.
    public int MaxChunkWords { get; set; } = 80;                    // Maximum chunk size to prevent overly large chunks.
    public int ContextualRetrievalWordsPerChunk { get; set; } = 15;    // Smaller window size for contextual retrieval.
}
