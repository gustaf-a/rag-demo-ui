
namespace RagDemoAPI.Models
{
    public class EmbeddingMetaData
    {
        public string Uri { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Source { get; set; }
        public Dictionary<string, string> Tags { get; internal set; }
    }
}
