namespace AiDemos.Api.Models
{
    public class PostgreSqlQueryParameters
    {
        public float[] EmbeddingQuery { get; set; }
        public IEnumerable<string> ContentMustIncludeWords { get; set; }
        public IEnumerable<string> ContentMustNotIncludeWords { get; set; }
        public IEnumerable<string> MetaDataFilterInclude { get; set; }
        public IEnumerable<string> MetaDataFilterExclude { get; set; }

        public int ItemsToRetrieve { get; set; }
        public int ItemsToSkip { get; set; }

        public int SemanticRankerCandidatesToRetrieve { get; set; }
    }
}
