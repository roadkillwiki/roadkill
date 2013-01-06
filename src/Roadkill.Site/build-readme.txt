Steps to create a new download version (this could be turned into a NANT/MSBuild script at some point):

1) Update the version in AssemblyInfo.cs in Core and Site
2) Compile using the 'Download' configuration
3) Publish the site to a folder
4) Copy the following files from the /TextFiles folder to the publish folder
	- /TextFiles/install.txt
	- /TextFiles/license.txt
	- /TextFiles/upgradeXXX.txt (if required)
5) Remove the fi-FI folder from the /bin folder (as it's a fake localization satellite assembly)
6) Remove the fi-FI folder from the /App_Data/EmailTemplates (as it's fake localization templates)
7) Remove the comments at the top from the web.config
8) Copy Microsoft.Web.Infrastructure from Core/Bin to the Site bin (it's missed by VS as it's not referenced, and needed on 2008 R2)
9) Remove the XML files from the publish bin folder) 
10) Copy /lib/Empty-databases/roadkill.sqlite to the publish /App_Data folder
11) Copy /lib/Empty-databases/roadkill.sdf to the publish /App_Data folder
12) Copy /lib/Empty-databases/roadkill.mdf to the publish /App_Data folder
13) Zip up using the name 'Roadkill_v{number}.zip' e.g. Roadkill_v1.3.zip, add to the downloads on bitbucket/codeplex.