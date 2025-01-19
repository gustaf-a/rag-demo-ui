namespace Shared.Models
{
    public class PostgreSqlQueryParameters
    {
        public float[] EmbeddingQuery { get; set; }
        public IEnumerable<string> ContentMustIncludeWords { get; set; }
        public IEnumerable<string> ContentMustNotIncludeWords { get; set; }
        public IEnumerable<string> MetaDataFilterIncludeAll { get; set; }
        public IEnumerable<string> MetaDataFilterIncludeAny { get; set; }
        public IEnumerable<string> MetaDataFilterExcludeAll { get; set; }
        public IEnumerable<string> MetaDataFilterExcludeAny { get; set; }

        public int ItemsToRetrieve { get; set; }
        public int ItemsToSkip { get; set; }

        public int SemanticRankerCandidatesToRetrieve { get; set; }
    }
}
