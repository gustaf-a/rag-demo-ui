namespace RagDemoAPI.Models
{
    public class SearchOptions
    {
        public int ItemsToRetrieve { get; set; } = 3;
        public int ItemsToSkip { get; set; } = 0;

        public string TextSearchContent { get; set; }
        public string TextSearchMetaData { get; set; }

        public Dictionary<string,string> MetaDataFiltersInclude { get; set; } = [];
        public Dictionary<string, string> MetaDataFiltersExclude { get; set; } = [];

        public string SemanticSearchContent { get; set; }
        
        public bool UseSemanticRanker { get; set; } = false;
        public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
        public bool UseSemanticCaptions { get; set; } = false;
        public int SemanticSearchGenerateSummaryOfNMessages { get; set; } = 0;
    }
}
