CREATE TABLE ProcessSteps (
    ProcessStepId    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProcessId        UUID NOT NULL REFERENCES Processes(ProcessId) ON DELETE CASCADE,
    Name              VARCHAR(255) NOT NULL,
    StepClassName     VARCHAR(255) NOT NULL,
    PredecessorStepIds UUID[] DEFAULT '{}',
    SuccessorStepIds   UUID[] DEFAULT '{}',
    CreatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    Payload           JSONB,
    UpdatedAt         TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
