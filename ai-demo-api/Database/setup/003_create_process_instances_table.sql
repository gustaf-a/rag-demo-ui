CREATE TABLE ProcessInstances (
    ProcessInstanceId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProcessId         UUID NOT NULL REFERENCES Processes(ProcessId) ON DELETE CASCADE,
    Name              VARCHAR(255) NOT NULL,
    Status            VARCHAR(50) NOT NULL,
    StartedBy         VARCHAR(255) NOT NULL,
    StartedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CompletedAt       TIMESTAMP WITH TIME ZONE,
    CreatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    Payload           JSONB,
    UpdatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
