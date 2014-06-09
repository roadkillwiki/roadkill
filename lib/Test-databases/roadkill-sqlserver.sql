--USE Roadkill;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'roadkill_pagecontent')
    DROP TABLE [dbo].[roadkill_pagecontent];

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'roadkill_pages')
    DROP TABLE [roadkill_pages];

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'roadkill_users')
    DROP TABLE [roadkill_users];

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'roadkill_siteconfiguration')
    DROP TABLE [roadkill_siteconfiguration];

-- SCHEMA (taken from Core/Database/Schema/SqlServer)
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
	[Text] [nvarchar](MAX) NULL,
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

CREATE TABLE [dbo].[roadkill_siteconfiguration]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](MAX) NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);

CREATE CLUSTERED INDEX [roadkill_pagecontent_idx] ON [dbo].[roadkill_pagecontent] (PageId, VersionNumber);
CREATE CLUSTERED INDEX [roadkill_users_idx] ON [dbo].[roadkill_users] (Email);
CREATE CLUSTERED INDEX [roadkill_siteconfiguration_idx] ON [dbo].[roadkill_siteconfiguration] ([Version]);


-- DATA
SET IDENTITY_INSERT roadkill_pages ON;

-- Users
INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES ('aabd5468-1c0e-4277-ae10-a0ce00d2fefc','','admin@localhost','Chris','0','1','1','Admin','C882A7933951FCC4197718B104AECC53564FC205','','J::]T!>k5LR|.{U9','admin');
INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES ('4d0bc016-1d47-4ad3-a6fe-a11a013ef9c8','3d12daea-16d0-4bd6-9e0c-347f14e0d97d','editor@localhost','','1','0','1','','7715C929E99254C117657B0937E97926443FDAF6','','fO)M`*QU:eH''Xl_%','editor');

-- Configuration
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('b960e8e5-529f-4f7c-aee4-28eb23e13dbd','2.0.0.0','{
  "AllowedFileTypes": "jpg,png,gif,zip,xml,pdf",
  "AllowUserSignup": true,
  "IsRecaptchaEnabled": false,
  "MarkupType": "Creole",
  "RecaptchaPrivateKey": "recaptcha-private-key",
  "RecaptchaPublicKey": "recaptcha-public-key",
  "SiteUrl": "http://localhost:9876",
  "SiteName": "Acceptance Tests",
  "Theme": "Responsive",
  "OverwriteExistingFiles": false,
  "HeadContent": "",
  "MenuMarkup": "* %mainpage%\r\n* %categories%\r\n* %allpages%\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n\r\n",
  "PluginLastSaveDate": "2013-12-28T16:00:54.408505Z"
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('8050978c-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "ClickableImages",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('208af9dc-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "ResizeImages",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('b35f5545-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "ExternalLinksInNewWindow",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('b20d067e-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "Jumbotron",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('b970504f-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "MathJax",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('598fdb04-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "ToC",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('92e641c4-80fb-0000-0000-000000000000','1.0','{
  "PluginId": "SyntaxHighlighter",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');
