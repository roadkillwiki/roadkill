# ====================================================================================================
# ROADKILL Developer/nightly build script
#  
# This build script does the following:
# 1. Builds the solution using the Download target with msbuild, and publish/deploy settings
# 2. Uses MSDEPLOY to create a package into a _WEBSITE
# 3. Adds the text files to the _WEBSITE directory
# 4. Adds the missing references to the _WEBSITE directory
# 5. Adds blank SQLite, SQL Server CE and SQL Server Express database to the _WEBSITE directory
# 6. Zips up _WEBSITE using 7zip
# 7. Cleans up the mess MSBUILD/MSDEPLOY created
# 8. Pushes the zip file to the roadkillbuilds repository
#
# This batch file assumes:
#	You have MSDeploy installed
#	You're running on x64 machine (for 7zip).
# 	You have a ..\roadkillbuilds directory (from https://bitbucket.org/mrshrinkray/roadkillbuilds)
# ====================================================================================================

$ErrorActionPreference = "Stop"
$zipFilename = "Roadkill.devbuild.zip"

# ---- Add the tool paths to our path
$runtimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
$env:Path = $env:Path + $runtimeDir
$env:Path = $env:Path + ";C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"
$env:Path = $env:Path + ";C:\Program Files\7-Zip"

# ---- Make sure the web.config file is the template one
copy -Force lib\Configs\web.dev.config src\Roadkill.site\web.config

# ---- Build the solution using the Download target
msbuild roadkill.sln "/p:Configuration=Download;DeployOnBuild=True;PackageAsSingleFile=False;AutoParameterizationWebConfigConnectionStrings=false;outdir=deploytemp\;OutputPath=bin\debug"

# ---- Use msdeploy to publish the website to disk
$currentDir = $(get-location).toString()
$packageSource = $currentDir +"\src\roadkill.site\obj\download\Package\PackageTmp\"
$packageDest = $currentDir + "\_WEBSITE"
msdeploy -verb:sync -source:contentPath=$packageSource -dest:contentPath=$packageDest

# ---- Copy licence + text files
copy -Force textfiles\devbuild.txt _WEBSITE\
copy -Force textfiles\licence.txt _WEBSITE\
copy -Force textfiles\install.txt _WEBSITE\

# ---- Copy missing DLL dependencies that the publish doesn't add
copy -Force lib\Microsoft.Web.Administration.dll _WEBSITE\bin
copy -Force lib\System.Data.SqlServerCe.dll _WEBSITE\bin

# ---- Copy blank databases
copy -Force lib\Empty-databases\roadkill.sqlite _WEBSITE\App_Data
copy -Force lib\Empty-databases\roadkill.sdf _WEBSITE\App_Data
copy -Force lib\Empty-databases\roadkill.mdf _WEBSITE\App_Data

# ---- Zip up the folder (requires 7zip)
CD _WEBSITE
7z a $zipFileName
copy -Force $zipFileName ..\..\roadkillbuilds\
CD ..

# ---- Clean up the temporary deploy folders
Remove-Item -Force -Recurse _WEBSITE
Remove-Item -Force -Recurse src\Roadkill.Core\deploytemp
Remove-Item -Force -Recurse src\Roadkill.Site\deploytemp
Remove-Item -Force -Recurse src\Roadkill.Tests\deploytemp

# ---- Commit to builds repository
CD ..\roadkillbuilds
$commitMessage = "Dev build " + [DateTime]::Now.toString()
hg addremove
hg commit -m $commitMessage
hg push

"DEV BUILD Complete."