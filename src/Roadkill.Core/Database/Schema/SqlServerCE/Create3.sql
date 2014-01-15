CREATE TABLE [roadkill_pagecontent]
(
	[Id] [uniqueidentifier] NOT NULL,
	[EditedBy] [nvarchar](255) NOT NULL,
	[EditedOn] [datetime] NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[Text] NTEXT NULL,
	[PageId] [int] NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);