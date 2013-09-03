CREATE TABLE [roadkill_users]
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