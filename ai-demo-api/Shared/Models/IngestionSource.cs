﻿namespace Shared.Models;

public class IngestionSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Content { get; set; }
    public EmbeddingMetaData MetaData { get; set; }
}
