namespace AiDemos.Api.Models
{
    public class SearchOptions
    {
        public string EmbeddingsTableName { get; set; }

        public int ItemsToRetrieve { get; set; } = 3;
        public int ItemsToSkip { get; set; } = 0;

        public IEnumerable<string>? ContentMustIncludeWords { get; set; } = null;
        public IEnumerable<string>? ContentMustNotIncludeWords { get; set; } = null;

        public Dictionary<string, IEnumerable<string>> MetaDataInclude { get; set; } = [];
        public Dictionary<string, IEnumerable<string>> MetaDataExclude { get; set; } = [];

        public string? SemanticSearchContent { get; set; } = null;

        public bool UseSemanticReRanker { get; set; } = false;
        public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
        public bool UseSemanticCaptions { get; set; } = false;
        public int SemanticSearchGenerateSummaryOfNMessages { get; set; } = 0;
    }
}
