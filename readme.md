[![Version](https://img.shields.io/nuget/v/Roadkill.svg?style=flat)](https://www.nuget.org/packages/Roadkill)

[![Version](https://ci.appveyor.com/api/projects/status/37etwyx9kw7uriar/branch/master?svg=true)](https://ci.appveyor.com/project/yetanotherchris/roadkill)

# Introduction

* [Download the latest version (2.0)](https://github.com/roadkillwiki/roadkill/releases/tag/v2.0)
* [Read the docs](http://www.roadkillwiki.net)
* [Try a demo](http://demo.roadkillwiki.net)

Roadkill .NET is a lightweight but powerful Wiki platform built on the following foundations:

* .NET 4.5
* jQuery
* ASP.NET MVC 5 with Razor
* Lucene.net search engine
* Creole, Media Wiki and Markdown syntax support
* Bootstrap 3 based UI.
* Supports SQL Server, SQL Server CE, SQL Azure (v1.6+), Sqlite, MySQL, Postgres, MongoDB
* It's themeable and extendable, has documentation, supports Active Directory authentication and is (I hope) extremely easy to use. It's Free Open Source (FOSS)

Roadkill is licensed under the [MS-PL license](https://github.com/roadkillwiki/roadkill/blob/master/LICENCE.md) which means it's free to use commercially or privately, but requires you to retain the copyright, trademark and attribution if you intend to distribute it (typically for commercial gain).

* [Please see the Roadkill wiki for information on installing](http://www.roadkillwiki.net/wiki/2/installing)
* [Upgrading from 1.7 to 2.0](http://www.roadkillwiki.net/wiki/14/upgrading-from-version-17-to-20)
* [Mono on Ubuntu installations](http://www.roadkillwiki.net/wiki/15/installing-on-linux-ubuntu-with-mono)

## For Developers


### Pre-requisites

To setup Roadkill on a developer machine, you will need:

* Visual Studio 2015 (2013 will also work but requires Typescript installed)
* SQL Server Express 2012 or higher - `choco install sqlserver2014express` for Chocolatey users.
* Your SQL Server installation should be the default instance (not YOURMACHINE\SQLEXPRESS) for the tests to pass. They rely on the connection string being `Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill`
* An NUnit runner (NUnit, Resharper, Dotcover etc.) if you want to run the tests. This is required if you want to contribute.

### Fresh install

To get a 'fresh' Roadkill installation on your development machine, you will need to do the following:

* For IIS: create a new site with a .NET 4 application pool. Roadkill also works with IIS Express.
* If you're using SQL Server: create a database called "roadkill". Run the `/lib/Test-databases/roadkill-sqlserver.sql` script.
  * If you want to use SQLite or SQLServer CE, empty databases can be found `/lib/Test-databases/SqlCE` or Sqlite.
* You can also install Roadkill using the unattended url, e.g.: http://localhost/install/Unattended?datastoretype=sqlserver2008&connectionstring=database=roadkill;uid=sa;pwd=Passw0rd;server=.\SQLEXPRESS


### Build scripts

*Note: work is now commencing on vNext (ASP.NET 5/MVC 6/DNX), so these scripts are likely to change in master.*

There are 4 build scripts that automate the builds:

* build.ps1 - runs msbuild with the solution file
* devbuild.ps1 - builds and copies all files required for a dev build, zips the files and then pushes the zip file to the 'RoadkillBuilds' repository on Bitbucket (https://bitbucket.org/yetanotherchris/roadkillbuilds).
* releasebuild.ps1 - The same as devbuild.ps1 but uses the `release` build configuration and only produces a zip file.
* mono.releasebuild.ps1 - Uses the the `mono` build configuration. 

### Running Roadkill on Azure
Roadkill can be run using a website deploy on Windows Azure. The instructions for this can be found on the [Roadkill wiki](http://www.roadkillwiki.net/wiki/13/azure-website-deployments)

### Syncing with Codeplex

Codeplex is synced from Github before releases using `git push --all https://git01.codeplex.com/roadkill`

The Codeplex site is now only around for publicity, and its source is usually very stale.
The Bitbucket site has also been retired.

### Contributing

If you want to contribute to Roadkill, have a look at the Contributing page on Github or on the [Roadkill wiki](http://www.roadkillwiki.net/wiki/4/contributing).
