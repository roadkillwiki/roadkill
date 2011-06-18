=== Upgrading Roadkill from 1.0.1 to version 1.1 ===

BACKUP YOUR DATABASE BEFORE UPGRADING!!

Roadkill 1.1 now supports user signups, which means a number of new settings have been added to cater for this.  
Before upgrading, ensure your users all have email addresses set, as this field is now used for logins (unless you're using Active Directory). 
The database schema has not changed radically, however you will need to run the SQL script below.

As this change affects users, this upgrade is manual.

-- BEGIN SQL SCRIPT

ALTER TABLE [dbo].[roadkill_siteconfiguration] ADD [EnableRecaptcha] [bit] NULL
GO
ALTER TABLE [dbo].[roadkill_siteconfiguration] ADD [RecaptchaPrivateKey] [nvarchar](255) NULL
GO
ALTER TABLE [dbo].[roadkill_siteconfiguration] ADD [RecaptchaPublicKey] [nvarchar](255) NULL
GO
ALTER TABLE [dbo].[roadkill_siteconfiguration] ADD [SiteUrl] [nvarchar](255) NULL
GO

ALTER TABLE [dbo].[roadkill_users] ADD [Firstname] [nvarchar](255) NULL
GO
ALTER TABLE [dbo].[roadkill_users] ADD [Lastname] [nvarchar](255) NULL
GO
ALTER TABLE [dbo].[roadkill_users] ADD [Username] [nvarchar](255) NULL
GO
ALTER TABLE [dbo].[roadkill_users] ADD [PasswordResetKey] [nvarchar](255) NULL
GO

UPDATE roadkill_siteconfiguration SET 
	EnableRecaptcha='0',
	SiteUrl='http://www.yourdomainname.com',
	Version='1.1.0.0'

-- END SQL SCRIPT