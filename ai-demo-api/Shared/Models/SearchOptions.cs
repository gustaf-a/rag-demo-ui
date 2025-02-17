﻿namespace Shared.Models;

public class SearchOptions
{
    public string EmbeddingsTableName { get; set; }

    public int ItemsToRetrieve { get; set; } = 3;
    public int ItemsToSkip { get; set; } = 0;

    public IEnumerable<string>? ContentMustIncludeWords { get; set; } = null;
    public IEnumerable<string>? ContentMustNotIncludeWords { get; set; } = null;

    public Dictionary<string, IEnumerable<string>> MetaDataIncludeWhenContainsAll { get; set; } = [];
    public Dictionary<string, IEnumerable<string>> MetaDataIncludeWhenContainsAny { get; set; } = [];
    public Dictionary<string, IEnumerable<string>> MetaDataExcludeWhenContainsAll { get; set; } = [];
    public Dictionary<string, IEnumerable<string>> MetaDataExcludeWhenContainsAny { get; set; } = [];

    public string? SemanticSearchContent { get; set; } = null;

    public int IncludeContentChunksAfter { get; set; } = 0;
    public int IncludeContentChunksBefore { get; set; } = 0;

    public bool UseSemanticReRanker { get; set; } = false;
    public int SemanticRankerCandidatesToRetrieve { get; set; } = 30;
    public bool UseSemanticCaptions { get; set; } = false;
    public int SemanticSearchGenerateSummaryOfNMessages { get; set; } = 0;
}
