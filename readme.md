# Roadkill Developer Readme

Branched from v2.0, customized to use Tui Editor

### Pre-requisites

To setup Roadkill on a developer machine, you will need:

* Visual Studio 2013 (2012 should also work)
* Typescript installed - http://www.typescriptlang.org
* SQL Server Express 2012 or better, although you can configure Roadkill to work with SQL Server CE or SQLite if you prefer.
* An NUnit runner (NUnit, Resharper, Dotcover etc.) if you want to run the tests. This is required if you want to contribute.

### Fresh install

To get a 'fresh' Roadkill installation on your development machine, you will need to do the following:

* For IIS: create a new site with a .NET 4 application pool. Roadkill also works with IIS Express.
* If you're using SQL Server: create a database called "roadkill". Run the `/lib/Test-databases/roadkill-sqlserver.sql` script.
  * If you want to use SQLite or SQLServer CE, empty databases can be found `/lib/Test-databases/SqlCE` or Sqlite.
* You can also install Roadkill using the unattended url, e.g.: http://localhost/install/Unattended?datastoretype=sqlserver2008&connectionstring=database=roadkill;uid=sa;pwd=Passw0rd;server=.\SQLEXPRESS

### Connection strings/Appsettings

When starting from scratch you will need to copy and rename the `connectionStrings_toRename.config` file in `src/Roadkill.Web/` to `connectionStrings.config`. You will also need to copy and rename `appsettings.CopyToDevel.json` to `appsettings.Development.json` in `Roadkill.API/`. To ensure sensitive data doesn't get pushed, connectionStrings.config and appsettings.Development.json are ignored by git.

### npm
You may need to run an `npm install` from the `src/Roadkill.Web` directory before running.

### Build scripts

There are 4 build scripts that automate the builds:

* build.ps1 - runs msbuild with the solution file
* devbuild.ps1 - builds and copies all files required for a dev build, zips the files and then pushes the zip file to the 'RoadkillBuilds' repository on Bitbucket (https://bitbucket.org/yetanotherchris/roadkillbuilds).
* releasebuild.ps1 - The same as devbuild.ps1 but uses the `release` build configuration and only produces a zip file.
* mono.releasebuild.ps1 - Uses the the `mono` build configuration. 

