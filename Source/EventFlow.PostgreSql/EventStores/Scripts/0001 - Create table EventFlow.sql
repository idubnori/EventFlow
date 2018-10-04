﻿
CREATE TABLE IF NOT EXISTS EventFlow(
	GlobalSequenceNumber BigSerial NOT NULL,
	BatchId uuid NOT NULL,
	AggregateId varchar(255) NOT NULL,
	AggregateName varchar(255) NOT NULL,
	Data Text NOT NULL,
	Metadata Text NOT NULL,
	AggregateSequenceNumber int NOT NULL,
	CONSTRAINT "PK_EventFlow" PRIMARY KEY
	(
		GlobalSequenceNumber
	)
);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventFlow_AggregateId_AggregateSequenceNumber" ON EventFlow
(
	AggregateId,
	AggregateSequenceNumber
);

