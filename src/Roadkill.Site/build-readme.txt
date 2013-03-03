==BUILD README==
These are the steps to create a new download version:

Firstly for the version being released: commit to Hg, *and then use hg tag v1.5.2* the version.

1) Update the version in AssemblyInfo.cs in Core and Site
2) Compile using the 'Download' configuration
3) Publish the site to a folder
4) Copy the following files from the /TextFiles folder to the publish folder
	- /TextFiles/install.txt
	- /TextFiles/license.txt
	- /TextFiles/upgradeXXX.txt (if required)
5) Remove the comments at the top from the web.config, and the "To log NHibernate SQL:" section
6) Copy /lib/System.Data.SqlServerCe.dll to the publish /bin folder (publish leaves it out for some reason)
7) Copy /lib/Microsoft.Web.Administration.dll to the publish /bin folder 
8) Copy /lib/Empty-databases/roadkill.sqlite to the publish /App_Data folder
9) Copy /lib/Empty-databases/roadkill.sdf to the publish /App_Data folder
10) Copy /lib/Empty-databases/roadkill.mdf to the publish /App_Data folder
11) Zip up using the name 'Roadkill_v{number}.zip' e.g. Roadkill_v1.3.zip, add to the downloads on bitbucket/codeplex.


==Testing Windows Authentication==
This can be done by creating a new Windows 2008 server and running into inside VirtualBox.
Install ActiveDirectory, call your domain Contoso.com.
Roadkill settings are then:
-	LDAP://contoso.com
-	administrator
-	Passw0rd

The two user groups are RoadkillEditors, RoadkillAdmins. I then setup two users to belong to each:
BobAdmin - RoadkillAdmins
EricEditor - RoadkillEditors