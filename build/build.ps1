# Basic msbuild script

# Add the tool paths to our path
$runtimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
$env:Path = $env:Path +";"+ $runtimeDir
Write-host "Added $runtimeDir to path"

# Build the solution
msbuild ..\roadkill.sln "/p:Configuration=Release,VisualStudioVersion=14.0" /t:Rebuild