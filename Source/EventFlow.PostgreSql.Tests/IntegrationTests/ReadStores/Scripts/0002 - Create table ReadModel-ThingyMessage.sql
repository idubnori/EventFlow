﻿CREATE TABLE IF NOT EXISTS "ReadModel-ThingyMessage"(
	Id BigSerial NOT NULL,
	ThingyId varchar(64) NOT NULL,
	MessageId varchar(64) NOT NULL,
	Message text NOT NULL,
	CONSTRAINT "PK_ReadModel-ThingyMessage" PRIMARY KEY
	(
		Id
	)
);

CREATE INDEX IF NOT EXISTS "IX_ReadModel-ThingyMessage_AggregateId" ON "ReadModel-ThingyMessage"
(
	MessageId 
);

CREATE INDEX IF NOT EXISTS "IX_ReadModel-ThingyMessage_ThingyId" ON "ReadModel-ThingyMessage"
(
	ThingyId 
);