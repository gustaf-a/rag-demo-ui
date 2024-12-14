CREATE TABLE ProcessStepInstances (
    ProcessStepInstanceId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProcessInstanceId     UUID NOT NULL REFERENCES ProcessInstances(ProcessInstanceId) ON DELETE CASCADE,
    ProcessStepId         UUID NOT NULL REFERENCES ProcessSteps(ProcessStepId) ON DELETE CASCADE,
    Name                   VARCHAR(255) NOT NULL,
    StepClassName          VARCHAR(255) NOT NULL,
    Status                 VARCHAR(50) NOT NULL,
    StartedAt              TIMESTAMP WITH TIME ZONE,
    CompletedAt            TIMESTAMP WITH TIME ZONE,
    CreatedAt              TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    Payload                JSONB,
    UpdatedAt              TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
