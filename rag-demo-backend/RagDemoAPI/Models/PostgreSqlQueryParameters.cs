namespace RagDemoAPI.Models
{
    public class PostgreSqlQueryParameters
    {
        public string TextQuery { get; set; }
        public string MetaDataSearchQuery { get; set; }
        public float[] EmbeddingQuery { get; set; }
        public IEnumerable<string> MetaDataFilterInclude { get; set; }
        public IEnumerable<string> MetaDataFilterExclude { get; set; }
        public int ItemsToRetrieve { get; set; }
        public int ItemsToSkip { get; set; }
        public bool UseSemanticCaptions { get; set; }
        public int SemanticRankerCandidatesToRetrieve { get; set; }
    }
}
