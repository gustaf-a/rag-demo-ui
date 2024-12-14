CREATE TABLE Processes (
    ProcessId        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name              VARCHAR(255) NOT NULL,
    Description       TEXT,
    CreatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    Payload           JSONB,
    UpdatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
