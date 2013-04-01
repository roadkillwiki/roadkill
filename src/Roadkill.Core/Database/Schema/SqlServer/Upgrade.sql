IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_siteconfiguration]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_siteconfiguration];

CREATE TABLE [dbo].[roadkill_siteconfiguration]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](MAX) NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);