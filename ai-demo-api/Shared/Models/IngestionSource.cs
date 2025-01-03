﻿using Shared.Models;

namespace Shared.Models;

public class IngestionSource
{
    public string Name { get; set; }
    public string Content { get; set; }
    public EmbeddingMetaData MetaData { get; set; }
}
