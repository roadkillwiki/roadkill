[![Nuget.org](https://img.shields.io/nuget/v/Roadkill.svg?style=flat)](https://www.nuget.org/packages/Roadkill)
[![Appveyor](https://ci.appveyor.com/api/projects/status/37etwyx9kw7uriar/branch/master?svg=true)](https://ci.appveyor.com/project/yetanotherchris/roadkill)
[![Coverage Status](https://coveralls.io/repos/roadkillwiki/roadkill/badge.svg?branch=master&service=github)](https://coveralls.io/github/roadkillwiki/roadkill?branch=master)

### Current status

A .NET Core version of Roadkill was started in the https://github.com/roadkillwiki/roadkill_new repository.

**While this .NET Core project is 99% functionally complete on the API-side, it stopped at .NET 5. It hasn't been continued because of the large amount of work involved with integrating an OAuth2 solution, and rewriting the front-end as a SPA using React or similar.**

Forking the .NET Core repository is welcome, if you'd like to implement an OAuth solution (that is FOSS, which IdentityServer no longer is) and a SPA front end using React, VueJS or similar.

This repository - Roadkill .NET Framework - is quite old now, but fully functional should you want to use it. 

# Introduction

* [Download the latest **stable** version (2.0)](https://github.com/roadkillwiki/roadkill/releases/tag/v2.0)
* [Read the docs](https://github.com/roadkillwiki/roadkill/tree/master/docs)
* [Try a demo - *sorry this is currently unavailable until further notice*](http://demo.roadkillwiki.net/)

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
  - **[US-East-1](https://us-east-1.console.aws.amazon.com/ec2/v2/#LaunchInstanceWizard:ami=ami-0ced7b9074464e093)**: `ami-0ced7b9074464e093`.
  - **[EU-West-1](https://eu-west-1.console.aws.amazon.com/ec2/v2/#LaunchInstanceWizard:ami=ami-021b89cff5ea9314c)**: `ami-021b89cff5ea9314c`.
- **[Azure instructions](docs/azure.md)**
- **Google Cloud**: looking for help

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

No contributions are currently being taken as the project is frozen.
