[![Nuget.org](https://img.shields.io/nuget/v/Roadkill.svg?style=flat)](https://www.nuget.org/packages/Roadkill)
[![Appveyor](https://ci.appveyor.com/api/projects/status/37etwyx9kw7uriar/branch/master?svg=true)](https://ci.appveyor.com/project/yetanotherchris/roadkill)
[![Coverage Status](https://coveralls.io/repos/roadkillwiki/roadkill/badge.svg?branch=master&service=github)](https://coveralls.io/github/roadkillwiki/roadkill?branch=master)
[![Join the chat at Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/roadkillwiki/general)

# Introduction

* [Download the latest **stable** version (2.0)](https://github.com/roadkillwiki/roadkill/releases/tag/v2.0)
* [Read the docs](http://www.roadkillwiki.net)
* [Try a demo](http://demo.roadkillwiki.net)
* [Discuss on Google Groups](https://groups.google.com/forum/#!forum/roadkillwiki)

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
* [Discussions/forum](https://groups.google.com/forum/#!forum/roadkillwiki)

## Version 3.0 Roadmap

*Last updated: July 2015*

It's been over 18 months since a Roadkill release, and the number of feature requests has grown quite large on the [UserEcho site](http://roadkillwiki.userecho.com/list/27059-general/).

The next version of Roadkill will be Roadkill version 3. It's a major version number because the following major changes are planned:

#### No more Creole support
Sorry Creole fans, but supported 3 different markdown formats is too labour intensive, and CommonMark has come a long and pretty much made Creole redundant, and Mediawiki syntax has zero support for .NET. Version 3 will support Markdown, using the [CommonMark](http://commonmark.org/) standard. 

This will be done via the [CommonMark.NET](https://github.com/Knagis/CommonMark.NET) library, which in future should support additional features like tables, which are [currently under discussion](http://talk.commonmark.org/t/tables-in-pure-markdown/81/81)

There's plans to create a tool to do some primitive conversion of Creole to Markdown to help people upgrade.

#### No more SQLite support

SQlite on Windows makes the whole Roadkill installation experience painful, and was the cause of most of the issues with version 2. It also runs extremely slower after you have a 5+ pages of lots of text.

#### .NET 4.6, ASP.NET 5, MVC 6, DNX by default
As with the previous versions of Roadkill, version 3 will use the latest Microsoft technologies. This is quite involved given the changes Microsoft have just made to their web stack, but also promises that Roadkill *should* work on OS X and Linux environments.

#### Better editor
Because Roadkill is moving to CommonMark, the editor can now be improved to be more user friendly, and have a faster client-side preview and will hopefully have some WYSIWYG abilities.

#### Continuous-integration builds
To replace the archaic zip system, it's planned to combine Octopack and Appveyor's ninja abilities together to make a new tag-based release system, in other words more frequent minor releases for minor bug fixes. Self-updating will also be considered if it's viable.

#### A new theme
A new material-design based theme.

#### Re-designed file manager
The plan is to redesign the file manager to emulate the Wordpress 4 file manager.

#### Better page dialog for adding links
Instead of having to memorize page names, adding links will be similar to Wordpress in finding pages that exist on the site.

#### Page attachments
See http://roadkillwiki.userecho.com/topic/354042-page-attachments/

#### Viewer role
See http://roadkillwiki.userecho.com/topic/416051-viewer-role/

As Roadkill is not a commercial project, nor backed commercially, there are no time-frames that can be relied upon.

## For Developers


### Pre-requisites

To setup Roadkill on a developer machine, you will need:

* Visual Studio 2015, Community Edition is fine, Roadkill is written using Community Edition.
* IIS
* SQL Server. Your SQL Server installation should be the default instance (not YOURMACHINE\SQLEXPRESS) for the tests to pass. They rely on the connection string being `Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill`
* An NUnit runner (NUnit, Resharper, Dotcover etc.) if you want to run the tests. This is required if you want to contribute.

### Fresh install

To get a 'fresh' Roadkill installation on your development machine, you will need to do the following:

* For IIS: create a new site with a .NET 4 application pool.
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
