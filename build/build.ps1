# ===============================================================================
# 1. Performs a nuget restore
# 2. Runs msbuild.exe with octopack output pointing to $octopackTargetDir
#
# Note: This script should be run from the root directory, not from the build folder.
# ===============================================================================
$ErrorActionPreference = "Stop"

$solutionFile      = "Roadkill.sln"
$configuration     = "Release"
$platform          = "Any CPU"
$msbuild           = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

$root              = $PSScriptRoot;
$octopackTargetDir = "$root\_deploymentOutput\";

if (!(Test-Path $msbuild))
{
    $msbuild = "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
}

# Nuget restore
Write-Host "Performing Nuget restore" -ForegroundColor Green
nuget restore $solutionFile

# Build the sln file
Write-Host "Building $solutionFile." -ForegroundColor Green

& $msbuild $solutionFile /p:Configuration=$configuration /p:RunOctoPack=true /p:OctoPackPublishPackageToFileShare="$octopackTargetDir" /p:Platform=$platform /target:Build /verbosity:minimal /p:WarningLevel=1
if ($LastExitCode -ne 0)
{
    throw "Building solution failed."
}
else
{
    Write-Host "Building solution complete."-ForegroundColor Green
}