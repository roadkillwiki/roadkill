In order to build:

1) Update the version in AssemblyInfo.cs in Core and Site
2) Compile using the 'Download' configuration
3) Publish the site to a folder
4) Ensure the published folder has an App_Data\Search folder
5) Copy the following files from the  /TextFiles folder to the publish folder
	- /TextFiles/install.txt
	- /TextFiles/license.txt
	- /TextFiles/README_FOR_UPGRADING_1.0_to_1.1.txt
6) Delete the bin/App_Data folder from the publish folder
7) Copy the following files from the /lib folder  to the publish /bin folder
	- SQLite.Interop.dll
	- System.Data.SQLite.dll
	- System.Data.SQLite.Linq.dll
8) Zip up using the name 'Roadkill_v{number}.zip' e.g. Roadkill_v1.11.zip, add to the downloads on bitbucket/codeplex.