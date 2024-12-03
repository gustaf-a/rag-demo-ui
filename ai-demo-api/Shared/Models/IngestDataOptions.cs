namespace AiDemos.Api.Models
{
    public class IngestDataOptions
    {
        public int MergeLineIfFewerWordsThan { get; set; } = 8;

        /// <summary>
        /// An ordered list of the chunking strategies to apply.
        /// </summary>
        public IEnumerable<string> SelectedChunkers { get; set; } = [];
    }
}
