=====BUILD README=====
The steps below are also in the releasebuild.ps1 file.
These are the steps to create a new download version:

Firstly for the version being released: 
	Commit to Hg 
	Use hg tag v1.x.x for the version.

1) Update the version in AssemblyInfo.cs in Core and Site
2) Compile using the 'Download' configuration
3) Publish the site to a folder
4) Copy the following files from the /TextFiles folder to the publish folder
	- /TextFiles/install.txt - update the version number
	- /TextFiles/license.txt
	- /TextFiles/upgradeXXX.txt (if required)
5) Copy /lib/System.Data.SqlServerCe.dll to the publish /bin folder (publish leaves it out for some reason)
6) Copy /lib/Microsoft.Web.Administration.dll to the publish /bin folder (publish leaves it out for some reason)
7) Copy /lib/Empty-databases/roadkill.sqlite to the publish /App_Data folder
8) Copy /lib/Empty-databases/roadkill.sdf to the publish /App_Data folder
9) Copy /lib/Empty-databases/roadkill.mdf to the publish /App_Data folder
10) Zip up using the name 'Roadkill_v{number}.zip' e.g. Roadkill_v1.3.zip, add to the downloads on bitbucket/codeplex.

=====DEV WEB.CONFIG====
The web.config for the site is unlikely to work when you pull the latest from Bitbucket, it could
be in any state from the previous commit. Copy the web.dev.config from the \lib\Configs directory 
and overwrite the web.config in the site folder with this.

Don't worry about committing your web.config, the Web.Download.config file is used for release packages.

=====TESTING WINDOWS AUTH=====

This can be done by creating a new Windows 2012 server and running into inside VirtualBox.

Install IIS (including application support)
Install Active Directory Domain Services, call your domain Contoso.com.
The two user groups are RoadkillEditors, RoadkillAdmins. I then setup two users to belong to each:
BobAdmin - RoadkillAdmins
EricEditor - RoadkillEditors

Logout and login as both of these users to test they add pages and administer the site. If using a remote server, add
these two users to the Administrators group to enable remote desktop access.

The Roadkill ldap settings are then:
-	ldapConnectionString: LDAP://contoso.com
-	ldapUsername: administrator
-	ldapPassword: Passw0rd

Use SQLite for the database.