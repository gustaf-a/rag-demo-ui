CREATE TABLE AgentTasks (
    Id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name           VARCHAR(255) NOT NULL,
    TaskPrompt     TEXT,
    Status         VARCHAR(255),
    Payload        JSONB,
    CompletedAt    TIMESTAMP WITH TIME ZONE,
    StartedAt      TIMESTAMP WITH TIME ZONE,
    UpdatedAt      TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
