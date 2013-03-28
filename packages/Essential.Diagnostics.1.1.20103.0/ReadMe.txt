Essential.Diagnostics
=====================

Copyright 2010 Sly Gryphon. This library distributed under the 
Microsoft Public License (Ms-PL).

http://essentialdiagnostics.codeplex.com/

Using and extending System.Diagnostics trace logging. 

This project uses the inbuilt features of the System.Diagnostics 
namespace, and shows how logging and tracing can be integrated into a 
client application whilst taking advantage of the services exposed by 
System.Diagnostics.

The Essential.Diagnostics.dll contains extensions to the .NET Framework 
System.Diagnostics trace listeners, filters, and other utilities.

Version History
---------------

v1.1.20103 (January 2012)

* Feature #4: Add HttpContext items -- RequestUrl, RequestPath, UserHostAddress, 
  AppData.
* Issue #1: TraceFormatter.cs dependent on System.Windows.Forms.
  We only want the application name part, so use either Assembly.GetEntryAssembly() 
  directly, or for native code use kernel32 GetModuleFileName(), without checking 
  security.
* Issue #2: traceSource.TraceInformation("Information message") throws exception 
  with SqlDatabaseTraceListener. (Issue was in TraceListenerBase and affected all 
  listeners.)
* Issue #12: Allow currently open log file to be shared with another program (for 
  read access only). 

v1.1.10711 (July 2011)

* RollingFileTraceListener, with trace format templates
* RollingXmlTraceListener, rolling files compatible with Service Trace Viewer
* Added TraceFormatter parameters: LocalDateTime, DateTime (preferred name 
  for UtcDateTime), PrincipalName, WindowsIdentityName, Thread (name, if
  available, otherwise id)
* Added new parameters to ExpressionFilter
* Added new SQL parameters to SqlDatabaseTraceListener
* Added Diagnostics.Abstractions library, for better dependency injection support
* Make backwards compatible with .NET 2.0 SP1
* Updated hello logging example for new trace listeners
* Added filter examples

v1.0.1011 (October 2010)

* Release as nuget package

v1.0.1008 (October 2010)

* Initial release
* ColoredConsoleTraceListener, with trace format templates
* SqlDatabaseTraceListener and diagnostics_regsql tool
* InMemoryTraceListener
* PropertyFilter and ExpressionFilter
* ActivityScope and LogicalOperationScope
* TraceConfigurationMonitor
