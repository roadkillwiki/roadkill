IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_siteconfiguration]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_siteconfiguration];

CREATE TABLE [dbo].[roadkill_siteconfiguration]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](MAX) NOT NULL,
	PRIMARY KEY NONCLUSTERED (Id)
);

CREATE CLUSTERED INDEX [roadkill_siteconfiguration_idx] ON [dbo].[roadkill_siteconfiguration] ([Version]);

-- DROP the bad constraints NHibernate sql generator made
declare @constraintName varchar(max)

SELECT @constraintName=CONSTRAINT_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		WHERE CONSTRAINT_TYPE='PRIMARY KEY' AND TABLE_SCHEMA='dbo' AND TABLE_NAME='roadkill_pagecontent';
exec('ALTER TABLE [dbo].[roadkill_pagecontent] DROP CONSTRAINT ' +@constraintName);

SELECT @constraintName=CONSTRAINT_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		WHERE CONSTRAINT_TYPE='PRIMARY KEY' AND TABLE_SCHEMA='dbo' AND TABLE_NAME='roadkill_users';
exec('ALTER TABLE [dbo].[roadkill_users] DROP CONSTRAINT ' +@constraintName);


-- Create new ones, to be Azure-friendly
CREATE CLUSTERED INDEX [roadkill_pagecontent_idx] ON [dbo].[roadkill_pagecontent] (PageId, VersionNumber);
CREATE CLUSTERED INDEX [roadkill_users_idx] ON [dbo].[roadkill_users] (Email);