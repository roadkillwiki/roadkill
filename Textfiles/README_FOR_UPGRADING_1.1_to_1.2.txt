=== Upgrading Roadkill from 1.1 to version 1.2 ===

--INTRODUCTION--
BACKUP YOUR DATABASE BEFORE UPGRADING!!

Roadkill 1.2 now supports locking pages, so only administrators are able to edit the page. This option is
available in the edit page if you are logged in as an administrator.

If you have a 1.1 installation, please run the SQL script below inside your database manager before 
running Roadkill.


-- BEGIN SQL SCRIPT

ALTER TABLE [dbo].[roadkill_pages] ADD [IsLocked] [bit] NULL
GO


UPDATE roadkill_siteconfiguration SET 
	Version='1.2.0.0'

-- END SQL SCRIPT