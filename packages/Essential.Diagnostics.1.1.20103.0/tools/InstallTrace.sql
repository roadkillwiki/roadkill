/**********************************************************************/
/* InstallTrace.SQL                                                  */
/*                                                                    */
/* Installs the tables, triggers and stored procedures necessary for  */
/* supporting the SqlDatabaseTraceListener of Essential.Diagnostics   */
/*
** Copyright Stephen Gryphon
*/
/**********************************************************************/

PRINT '-------------------------------------------'
PRINT 'Starting execution of InstallTrace.SQL'
PRINT '-------------------------------------------'
GO

SET QUOTED_IDENTIFIER OFF
SET ANSI_NULLS ON         -- We don't want (NULL = NULL) == TRUE
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULL_DFLT_ON ON
GO

/*************************************************************/

--
--Create database
--

DECLARE @dbname nvarchar(128)
DECLARE @dboptions nvarchar(1024)

SET @dboptions = N'/**/'
SET @dbname = N'diagnosticsdb'

IF (NOT EXISTS (SELECT name
                FROM master.dbo.sysdatabases
                WHERE name = @dbname))
BEGIN
  PRINT 'Creating the ' + @dbname + ' database...'
  DECLARE @cmd nvarchar(500)
  SET @cmd = 'CREATE DATABASE [' + @dbname + '] ' + @dboptions
  EXEC(@cmd)
END
GO

USE [diagnosticsdb]
GO

/*************************************************************/

--
--Create tables
--

IF (NOT EXISTS (SELECT name
                FROM sysobjects
                WHERE (name = N'diagnostics_Trace')
                  AND (type = 'U')))
BEGIN
  PRINT 'Creating the diagnostics_Trace table...'

  CREATE TABLE [dbo].[diagnostics_Trace](
    ApplicationName nvarchar(256) NOT NULL,
	TraceId uniqueidentifier NOT NULL PRIMARY KEY NONCLUSTERED DEFAULT NEWID(),
	[Source] nvarchar(64) NULL,
	Id int NOT NULL default 0,
	EventType nvarchar(32) NOT NULL,
	[UtcDateTime] datetime NOT NULL,
	MachineName nvarchar(32) NOT NULL,
	AppDomainFriendlyName nvarchar(512) NOT NULL,
	ProcessId int NOT NULL default 0,
	ThreadName nvarchar(512) NULL,
	[Message] nvarchar(1500) NULL,
	ActivityId uniqueidentifier NULL,
	RelatedActivityId uniqueidentifier NULL,
	LogicalOperationStack nvarchar(512) NULL,
	Data ntext NULL,
  )
  CREATE CLUSTERED INDEX diagnostics_Trace_index ON diagnostics_Trace(ApplicationName, [UtcDateTime])

END
GO

/*************************************************************/

--
--Create stored procedures
--

IF (EXISTS (SELECT name
              FROM sysobjects
             WHERE (name = N'diagnostics_Trace_AddEntry')
               AND (type = 'P')))
DROP PROCEDURE dbo.diagnostics_Trace_AddEntry
GO
PRINT 'Creating the diagnostics_Trace_AddEntry procedure...'
GO

CREATE PROCEDURE dbo.diagnostics_Trace_AddEntry
    @ApplicationName nvarchar(256),
	@Source nvarchar(64),
	@Id int,
	@EventType nvarchar(32),
	@UtcDateTime datetime,
	@MachineName nvarchar(32),
	@AppDomainFriendlyName nvarchar(512),
	@ProcessId int,
	@ThreadName nvarchar(512),
	@Message nvarchar(1500),
	@ActivityId uniqueidentifier,
	@RelatedActivityId uniqueidentifier,
	@LogicalOperationStack nvarchar(512),
	@Data ntext
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    INSERT INTO dbo.diagnostics_Trace
                ( ApplicationName,
                  [Source],
                  Id,
                  EventType,
                  [UtcDateTime],
                  MachineName,
                  AppDomainFriendlyName,
                  ProcessId,
                  ThreadName,
                  [Message],
                  ActivityId,
                  RelatedActivityId,
                  LogicalOperationStack,
                  Data )
         VALUES ( @ApplicationName,
                  @Source,
                  @Id,
                  @EventType,
                  @UtcDateTime,
                  @MachineName,
                  @AppDomainFriendlyName,
                  @ProcessId,
                  @ThreadName,
                  @Message,
                  @ActivityId,
                  @RelatedActivityId,
                  @LogicalOperationStack,
                  @Data )

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    RETURN 0

Cleanup:

    RETURN @ErrorCode

END
GO


/*************************************************************/

--
--Create roles
--

IF ( NOT EXISTS ( SELECT name
                  FROM sysusers
                  WHERE issqlrole = 1
                  AND name = N'diagnostics_Trace_Writer'  ) )
EXEC sp_addrole N'diagnostics_Trace_Writer'

IF ( NOT EXISTS ( SELECT name
                  FROM sysusers
                  WHERE issqlrole = 1
                  AND name = N'diagnostics_Trace_Reader'  ) )
EXEC sp_addrole N'diagnostics_Trace_Reader'
GO

GRANT EXECUTE ON dbo.diagnostics_Trace_AddEntry TO diagnostics_Trace_Writer
GRANT SELECT ON dbo.diagnostics_Trace TO diagnostics_Trace_Reader
GO

PRINT '--------------------------------------------'
PRINT 'Completed execution of InstallTrace.SQL'
PRINT '--------------------------------------------'
