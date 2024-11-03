namespace RagDemoAPI.Configuration;

public class IngestionOptions
{
    public const string Ingestion = "Ingestion";

    public bool AllowLlmBasedChunking { get; set; }
    public int FixedSizeChunkingChunkSize { get; set; } = 500;       // Typical fixed size for general text chunking.
    public int SlidingWindowChunkSize { get; set; } = 300;           // Smaller window size for sliding window approaches.
    public int SlidingWindowOverlapSize { get; set; } = 100;         // Overlap size that ensures contextual continuity.
    public int MaxChunkSize { get; set; } = 1000;                    // Maximum chunk size to prevent overly large chunks.
    public int ContextualRetrievalChunkSize { get; set; } = 200;    // Smaller window size for contextual retrieval.
}
