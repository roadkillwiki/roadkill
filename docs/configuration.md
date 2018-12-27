# Configuration
Roadkill is configured using a combination of settings stored in the Roadkill.config file and in the database. The database connection string is stored in the connectionString.config file, and along with the Roadkill.config file is reference inside the web.config file.

## Roadkill.config
This file contains all application settings for Roadkill that typically need an application restart, or are not just cosmetic preferences. The installer will configure the file for you when you run the install wizard, if you need to change the config file later each of the properties available in the `<roadkill...` section can be found in the Api Documentation

## connectionStrings.config
Your database connection string should be stored in the connectionStrings.config file in the website root. By default, this will be empty.

There is usually no reason to change the name of the connectionString in this section as it ties in with the connectionName setting inside the roadkill section.

## Changing the language of the site
If you want to force the language of your site to something other than the one installed on the server, you can do this using the `<globalization>` tag inside the `<system.web>` section.

This element is included in the Roadkill web.config, the example below shows how to force the site to use Spanish (Spain), a full list of valid locale names can be found on MSDN.

`<globalization uiCulture="es-ES" culture="es-ES" />`
The localization of the various labels on the site is based off this setting, and is configured during the installation.

## Caching
Roadkill uses basic object caching for its caching strategy, along side browser caching. This saves trips to the database and is important for text content for mid-to-high traffic websites and responsiveness. By default Roadkill uses the .NET System.Runtime.Caching's default cache, which is similar to ASP.NET's cache for its object cache. This doesn't scale across web servers, so should be turned off if you are using multiple servers.

If you wish to scale to more than one server please contact us via Bitbucket, and a new version can be released fairly quickly to support scalable caches such as Appfabric or Couchbase. The object cache is based off the plugable System.Runtime.Caching architecture making this a simple change to make.

## Email server settings
If you have setup the Roadkill installation to allow signups from users, you will want to setup a mail server that the signup and lost emails are sent via. This can be done via the system.net section which is included in the Roadkill web.config by default, but is configured so that all emails are written as files to a drop folder. Below are the default settings, full details can be found on MSDN - http://msdn.microsoft.com/en-us/library/w355a94k.aspx.

```
<!-- Change these settings for signup and lost password emails -->
<system.net>
    <mailSettings>
        <smtp deliveryMethod="SpecifiedPickupDirectory" from="signup@roadkillwiki.org">
            <specifiedPickupDirectory pickupDirectoryLocation="C:\inetpub\temp\smtp" />
        </smtp>
    </mailSettings>
</system.net>
```

## Settings that are stored in the database
The settings available through the 'Site Settings' menus (admins only) reflects all the settings that are stored in the database. These are stored as JSON in the database, so can be easily edited if needed. The full list of settings are available in the API documentation