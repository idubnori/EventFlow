﻿CREATE TABLE IF NOT EXISTS "ReadModel-ThingyAggregate"(
    PingsReceived int NOT NULL,
    DomainErrorAfterFirstReceived Boolean NOT NULL,

    -- -------------------------------------------------
    Id BigSerial,
    AggregateId Varchar(64) NOT NULL,
    CreateTime Timestamp WITH TIME ZONE NOT NULL,
    UpdatedTime Timestamp WITH TIME ZONE NOT NULL,
    LastAggregateSequenceNumber int NOT NULL,
    CONSTRAINT "PK_ReadModel-ThingyAggregate" PRIMARY KEY 
    (
        Id
    )
);

CREATE INDEX  IF NOT EXISTS "IX_ReadModel-ThingyAggregate_AggregateId" ON "ReadModel-ThingyAggregate"
(
    AggregateId
);