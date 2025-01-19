CREATE TABLE Agents (
    Id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name        VARCHAR(255) NOT NULL,
    Description TEXT,
    Options     JSONB
);
