CREATE TABLE IngestionSources (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(255) NOT NULL,
    Content TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    Metadata JSONB
);
