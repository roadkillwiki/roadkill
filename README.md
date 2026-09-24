[![Nuget.org](https://img.shields.io/nuget/v/Roadkill.svg?style=flat)](https://www.nuget.org/packages/Roadkill)
[![CI](https://github.com/AFract/roadkill-fork/actions/workflows/ci.yml/badge.svg)](https://github.com/AFract/roadkill-fork/actions/workflows/ci.yml)

### Current status (this fork)

This fork has been migrated to **.NET 10 / ASP.NET Core MVC** (details in [MIGRATION.md](MIGRATION.md)).

**Upgrading a 2.x installation:** [English guide](docs/upgrade-v2-to-v3.md) · [Guide en français](docs/migration-v2-vers-v3.md)

#### New / changed

* Runs on .NET 10 / ASP.NET Core MVC (on IIS with the ASP.NET Core Hosting Bundle, or any other ASP.NET Core host; tested on Linux with Kestrel).
* Markdown parser replaced by Markdig, with **GitHub Flavored Markdown**: pipe tables, strikethrough, task lists, autolinks,
  footnotes, fenced code blocks. The existing Roadkill Markdown syntax (`[[[code lang=xx|...]]]`, image sizes, `#Title#`...) still works.
* **Mermaid** diagrams through a new plugin (disabled by default).
* Data access: SQL Server and Postgres through Dapper. The database schema is unchanged from 2.x.
* Settings are in `appsettings.json` (`Roadkill` section and `ConnectionStrings:Roadkill`) instead of `web.config` / `Roadkill.config`.
  `tools/ConvertWebConfig.cs` (or `.linq` for LINQPad) converts an existing `web.config`. Logging is still configured in `App_Data/NLog.config`.
* The Lucene search index format changed (Lucene.Net 4.8): the index must be rebuilt after upgrading.
* Themes: `@Html.Action(...)` (child actions) no longer exists in ASP.NET Core; custom `Theme.cshtml` files need a 3 line change (see the upgrade guide).

#### Removed (no longer supported)

* Databases: **MySQL**, **SQLite** and **SQL Server CE** (and the LightSpeed ORM they relied on).
* **Windows / Active Directory authentication** (and the matching installer step). Only the built-in forms (cookie) authentication remains.
* **reCAPTCHA v1** (Google's v1 API is gone): reCAPTCHA v2 keys are required.
* The XML configuration files (`web.config` Roadkill section, `Roadkill.config`, `connectionStrings.config`).
* Binary compatibility of 2.x plugins: custom plugins must be rebuilt for .NET 10.
* The .NET Framework build tooling: AppVeyor / Travis CI, `build/*.ps1` and Mono scripts, Web Platform Installer package (`lib/WebPI`), XML configs in `lib/Configs`.
  CI now runs on **GitHub Actions** (`.github/workflows/ci.yml`: build, unit tests, SQL Server and Postgres integration tests).

#### Kept but untested

* **MongoDB** (driver updated, integration tests skipped by default), Azure Blob attachments storage, SMTP sending, reCAPTCHA v2, hosting on IIS.

#### Build and test

* Build and run: `dotnet run --project src/Roadkill.Web` (solution file: `Roadkill.slnx`).
* Tests: `dotnet test src/Roadkill.Tests`. The integration tests use the `ROADKILL_SQLSERVER_CONNECTION_STRING` and
  `ROADKILL_POSTGRES_CONNECTION_STRING` environment variables.

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
