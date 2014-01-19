# ====================================================================================================
# ROADKILL release build script - upgrade version.
#  
# See releasebuild.ps1 for what the script does.
# This upgrade script removes files from the package that will already exist in a Roadkill installation.
# ====================================================================================================

$ErrorActionPreference = "Stop"
$zipFileName = "Roadkill_v2.0.upgrade.zip"

# ---- Up to the root directory
cd ..

# ---- Add the tool paths to our path
$runtimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
$env:Path = $env:Path + ";" +$runtimeDir
$env:Path = $env:Path + ";C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"
$env:Path = $env:Path + ";C:\Program Files\7-Zip"

# ---- Make sure the roadkill.config,connectionstrings.config files are the download template one
copy -Force lib\Configs\roadkill.download.config src\Roadkill.Web\roadkill.config
copy -Force lib\Configs\connectionStrings.config src\Roadkill.Web\connectionStrings.config

# ---- Build the solution using the Download target
msbuild roadkill.sln "/p:Configuration=Download;DeployOnBuild=True;PackageAsSingleFile=False;AutoParameterizationWebConfigConnectionStrings=false;outdir=deploytemp\;OutputPath=bin\debug"

# ---- Use msdeploy to publish the website to disk
$currentDir = $(get-location).toString()
$packageSource = $currentDir +"\src\Roadkill.Web\obj\download\Package\PackageTmp\"
$packageDest = $currentDir + "\_WEBSITE"
msdeploy -verb:sync -source:contentPath=$packageSource -dest:contentPath=$packageDest

# ---- Copy licence + text files
copy -Force textfiles\licence.txt _WEBSITE\
copy -Force textfiles\readme.txt _WEBSITE\

# ---- Copy missing DLL dependencies that the publish doesn't add
copy -Force lib\Microsoft.Web.Administration.dll _WEBSITE\bin
copy -Force lib\System.Data.SqlServerCe.dll _WEBSITE\bin

# ---- Remove files that should already exist on an existing Roadkill install
del _WEBSITE/App_Data/customvariables.xml
del _WEBSITE/App_Data/Internal/htmlwhitelist.xml

# ---- DON'T copy the blank databases

# ---- Zip up the folder (requires 7zip)
CD _WEBSITE
7z a $zipFileName
copy $zipFileName ..\$zipFileName
CD ..

# ---- Clean up the temporary deploy folders
Remove-Item -Force -Recurse _WEBSITE
Remove-Item -Force -Recurse src\Roadkill.Core\deploytemp
Remove-Item -Force -Recurse src\Roadkill.Web\deploytemp
Remove-Item -Force -Recurse src\Roadkill.Tests\deploytemp

"Release build Complete."