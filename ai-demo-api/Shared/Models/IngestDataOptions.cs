namespace Shared.Models
{
    public class IngestDataOptions
    {
        public int MergeLineIfFewerWordsThan { get; set; } = 8;

        /// <summary>
        /// An ordered list of the chunking strategies to apply.
        /// Default is ContextualChunker
        /// </summary>
        public IEnumerable<string> SelectedChunkers { get; set; } = ["ContextualChunker"];
        public bool DoPreProcessing { get; set; } = true;
    }
}
