CREATE TABLE [dbo].[roadkill_pages]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Tags] [nvarchar](255) NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[ModifiedBy] [nvarchar](255) NULL,
	[ModifiedOn] [datetime] NULL,
	PRIMARY KEY CLUSTERED (Id)
);

CREATE TABLE [dbo].[roadkill_pagecontent]
(
	[Id] [uniqueidentifier] NOT NULL,
	[EditedBy] [nvarchar](255) NOT NULL,
	[EditedOn] [datetime] NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[Text] [nvarchar](255) NULL,
	[PageId] [int] NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);

ALTER TABLE [dbo].[roadkill_pagecontent] ADD CONSTRAINT [FK_roadkill_pageid] FOREIGN KEY([pageid]) REFERENCES [dbo].[roadkill_pages] ([id]);

CREATE TABLE [dbo].[roadkill_users]
(
	[Id] [uniqueidentifier] NOT NULL,
	[ActivationKey] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Firstname] [nvarchar](255) NULL,
	[Lastname] [nvarchar](255) NULL,
	[IsEditor] [bit] NOT NULL,
	[IsAdmin] [bit] NOT NULL,
	[IsActivated] [bit] NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[PasswordResetKey] [nvarchar](255) NULL,
	[Salt] [nvarchar](255) NOT NULL,
	[Username] [nvarchar](255) NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);

CREATE TABLE [dbo].[roadkill_siteconfiguration](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [nvarchar](255) NOT NULL,
	[Xml] [nvarchar](255) NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);