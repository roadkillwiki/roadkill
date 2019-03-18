[![Nuget.org](https://img.shields.io/nuget/v/Roadkill.svg?style=flat)](https://www.nuget.org/packages/Roadkill)
[![Appveyor](https://ci.appveyor.com/api/projects/status/37etwyx9kw7uriar/branch/master?svg=true)](https://ci.appveyor.com/project/yetanotherchris/roadkill)
[![Coverage Status](https://coveralls.io/repos/roadkillwiki/roadkill/badge.svg?branch=master&service=github)](https://coveralls.io/github/roadkillwiki/roadkill?branch=master)
[![Join the chat at Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/roadkillwiki/general)

# Introduction

* [Download the latest **stable** version (2.0)](https://github.com/roadkillwiki/roadkill/releases/tag/v2.0)
* [Read the docs](https://github.com/roadkillwiki/roadkill/tree/master/docs)
* [Try a demo](http://demo.roadkillwiki.net/)

Roadkill .NET is a lightweight but powerful Wiki platform built on the following foundations:

* .NET 4.5
* jQuery
* ASP.NET MVC 5 with Razor
* Lucene.net search engine
* Creole, Media Wiki and Markdown syntax support
* Bootstrap 3 based UI.
* Supports SQL Server, SQL Server CE, SQL Azure (v1.6+), Sqlite, MySQL, Postgres, MongoDB
* It's themeable and extendable, has documentation, supports Active Directory authentication and is (I hope) extremely easy to use. It's Free Open Source (FOSS)

Roadkill is licensed under the [MS-PL license](LICENCE.md) which means it's free to use commercially or privately, but requires you to retain the copyright, trademark and attribution if you intend to distribute it (typically for commercial gain).

* [Please see the Roadkill wiki for information on installing](docs/installing.md)
* Please use issues for any discussions, bug reports, enhancements.

## Quick start: Azure and AWS

- **AWS** t2.micro is generally big enough for a small site. *Note: this AMI is Windows 2016 July 2018. It may need updating after launch via RDP*.
  - **[US-East](https://us-east-1.console.aws.amazon.com/ec2/v2/#LaunchInstanceWizard:ami=ami-ami-00e1e97f)**: `ami-00e1e97f`.
  - **[EU-West1](https://eu-west-1.console.aws.amazon.com/ec2/v2/#LaunchInstanceWizard:ami=ami-5550b5b8)**: `ami-5550b5b8`.
- **[Azure instructions](docs/azure.md)**
- **Google Cloud**: looking for help


## Version 3.0 Roadmap

*July 2018*

The next version of Roadkill will be Roadkill version 3. Because of the instability of .NET Core over the past 1.5 years, it's been a long time coming. It's currently making good progress now we have the stability of .NET Core 2.0.

It's a major version number because the following major changes are planned:

#### ASP.NET Core

The next version will be ASP.NET Core only, aimed at Linux hosting for cost and scability.

#### Enhanced security

Through its ASP.NET Core identity integration, version 3 will support everything that `Microsoft.AspNetCore.Identity` [supports](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x)

#### Postgres only

Version 3 will be Postgres only, using [Marten](http://jasperfx.github.io/marten/) as its NoSQL document store. Postgres can be run as Docker container, or is available a service by services such as AWS RDS.

#### API-first

The new Roadkill will be powered by its RESTful API, rather than version's 2 after-thought approach. This enables far easier plugin and extensibility to exist for Roadkill.

#### Docker

Roadkill 3 will be a docker image you run, on Linux Docker. With Docker comes built in scalibility, easier versioning, 

#### Elasticsearch for searching

The next version will use ElasticSearch for its search engine, rather than Lucene, removing a lot of complexity and past problems from Roadkill. You can run Elasticsearch as a Docker image, or use a hosted service search as AWS's Elasticsearch.

#### No more Creole support
Sorry Creole fans, but supporting 3 different markdown formats is too labour intensive, and CommonMark has come a long and pretty much made Creole redundant, and Mediawiki syntax has zero support for .NET. Looking at commercial wiki engines like Confluence, it ultimately doesn't make a lot of difference what markdown format you support, providing there is good documentation for the syntax.

Version 3 will only support Markdown, using the [CommonMark](http://commonmark.org/) standard via [Markdig](https://github.com/lunet-io/markdig). CommonMark is a well thought-out and documentated extension of Markdown and has a large community behind it.

#### Improved editing experience
Because Roadkill is moving to CommonMark, the editor can now be improved to be more user friendly, and have a faster client-side preview. The TUI editor is currently being considered for this: https://github.com/roadkillwiki/roadkill/issues/57

#### A new theme
A new material-design based theme.

#### Re-designed file manager
While this may not make the initial v3 release, the plan is to redesign the file manager to emulate the Wordpress 4 file manager.

#### Better page dialog for adding links
Instead of having to memorize page names, adding links will be similar to Wordpress in finding pages that exist on the site.


As Roadkill is not a commercial project, nor backed commercially, there are no time-frames that can be relied upon. It is essentially me creating it in my spare time.

## For Developers


### Pre-requisites

**Make sure you use the `version-2` branch, master is not currently stable**

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

### Contributing

No contributions are currently being taken until Version 3 is released.

## Support on Beerpay
Hey dude! Help me out for a couple of :beers:!

[![Beerpay](https://beerpay.io/roadkillwiki/roadkill/badge.svg?style=beer-square)](https://beerpay.io/roadkillwiki/roadkill)  [![Beerpay](https://beerpay.io/roadkillwiki/roadkill/make-wish.svg?style=flat-square)](https://beerpay.io/roadkillwiki/roadkill?focus=wish)