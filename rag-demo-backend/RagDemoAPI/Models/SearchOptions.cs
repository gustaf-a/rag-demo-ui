namespace RagDemoAPI.Models
{
    public class SearchOptions
    {
        public int ItemsToRetrieve { get; set; } = 3;
        public int ItemsToSkip { get; set; } = 0;

        public string TextSearchContent { get; set; }
        public string TextSearchMetaData { get; set; }

        public IEnumerable<string> CategoriesInclude { get; set; } = [];
        public IEnumerable<string> CategoriesExclude { get; set; } = [];

        public string SearchContent { get; set; }
        
        public bool UseSemanticRanker { get; set; } = false;
        public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
        public bool UseSemanticCaptions { get; set; } = false;
        public bool SemanticSearchGenerateSummaryOfMessageHistory { get; internal set; }
    }
}
