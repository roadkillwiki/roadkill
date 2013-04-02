/**********************************************************************/
/* UninstallTrace.SQL                                                 */
/*                                                                    */
/* Installs the tables, triggers and stored procedures necessary for  */
/* supporting the SqlDatabaseTraceListener of Essential.Diagnostics   */
/*
** Copyright Stephen Gryphon
*/
/**********************************************************************/

PRINT '-------------------------------------------'
PRINT 'Starting execution of UninstallTrace.SQL'
PRINT '-------------------------------------------'
GO

SET QUOTED_IDENTIFIER OFF
SET ANSI_NULLS ON         -- We don't want (NULL = NULL) == TRUE
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULL_DFLT_ON ON
GO

USE [diagnosticsdb]
GO

/*************************************************************/

--
--Remove items
--

IF (EXISTS ( SELECT name
                  FROM sysusers
                  WHERE issqlrole = 1
                  AND name = N'diagnotics_Trace_Writer'  ) )
EXEC sp_droprole N'diagnostics_Trace_Writer'

IF (EXISTS ( SELECT name
                  FROM sysusers
                  WHERE issqlrole = 1
                  AND name = N'diagnotics_Trace_Reader'  ) )
EXEC sp_droprole N'diagnostics_Trace_Reader'
GO

IF (EXISTS (SELECT name
              FROM sysobjects
             WHERE (name = N'diagnostics_Trace_AddEntry')
               AND (type = 'P')))
DROP PROCEDURE dbo.diagnostics_Trace_AddEntry
GO

IF (EXISTS (SELECT name
                FROM sysobjects
                WHERE (name = N'diagnostics_Trace')
                  AND (type = 'U')))
DROP TABLE dbo.diagnostics_Trace
GO

PRINT '--------------------------------------------'
PRINT 'Completed execution of InstallTrace.SQL'
PRINT '--------------------------------------------'
