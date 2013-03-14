ALTER TABLE [dbo].[roadkill_siteconfiguration] ADD [Xml] [nvarchar](MAX) NULL;

ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [AllowedFileTypes];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [AllowedUserSignup];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [EnableRecaptcha];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [MarkupType];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [RecaptchaPrivateKey];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [RecaptchaPublicKey];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [SiteUrl];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [Theme];
ALTER TABLE [dbo].[roadkill_siteconfiguration] DROP COLUMN [Title];