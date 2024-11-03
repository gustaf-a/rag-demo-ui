namespace RagDemoAPI.Models
{
    public class RetrievalOptions
    {
        public int ItemsToRetrieve { get; set; } = 3;
        public int ItemsToSkip { get; set; } = 0;

        public IEnumerable<string> CategoryInclude { get; set; } = [];
        public IEnumerable<string> CategoryExclude { get; set; } = [];

        public bool UseSemanticRanker { get; set; } = false;
        public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
        public bool UseSemanticCaptions { get; set; } = false;
    }
}
