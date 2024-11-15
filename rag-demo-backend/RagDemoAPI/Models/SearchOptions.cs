namespace RagDemoAPI.Models
{
    public class SearchOptions
    {
        public DatabaseOptions DatabaseOptions { get; set; } = new DatabaseOptions();

        public int ItemsToRetrieve { get; set; } = 3;
        public int ItemsToSkip { get; set; } = 0;

        public IEnumerable<string>? ContentMustIncludeWords { get; set; } = null;
        public IEnumerable<string>? ContentMustNotIncludeWords { get; set; } = null;

        public Dictionary<string, IEnumerable<string>> MetaDataFiltersInclude { get; set; } = [];
        public Dictionary<string, IEnumerable<string>> MetaDataFiltersExclude { get; set; } = [];

        public string? SemanticSearchContent { get; set; } = null;

        public bool UseSemanticReRanker { get; set; } = false;
        public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
        public bool UseSemanticCaptions { get; set; } = false;
        public int SemanticSearchGenerateSummaryOfNMessages { get; set; } = 0;
    }
}
