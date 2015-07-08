# From: https://gist.github.com/yetanotherchris/9112300ecfc11c20931c
# Make sure this is run as administrator
$user = [Security.Principal.WindowsIdentity]::GetCurrent();
$isAdmin = (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
if ($isAdmin -eq $false)
{
	Write-Host "Please run this script as an administrator" -ForegroundColor Yellow
	exit
}
 
$version = "2.15"
$downloadUrl = "http://chromedriver.storage.googleapis.com/$version/chromedriver_win32.zip"
$destFile = "chromedriver_win32-$version.zip"
$destDir = "$env:TEMP" 
$destFullPath = "$destDir\$destFile"
$copyPath = "C:\windows" # This should be somewhere that's in the PATH environment variable
 
# Download the latest chomedriver
Write-Host "Downloading $downloadUrl"
if ("$destFullPath")
{
  del "$destFullPath" -Force
}
wget -Uri $downloadUrl -OutFile $destFullPath
 
# Unzip it - requires 7zip - choco install 7zip
Write-Host "Unzipped $destPath"
if (Test-Path "$destDir\chromedriver.exe")
{
  del "$destDir\chromedriver.exe" -Force
}
& "C:\Program Files\7-Zip\7z.exe" x $destFullPath -o"$destDir"
 
# Copy to c:\windows
Write-Host "Copying $destDir\chromedriver.exe to $copyPath\chromedriver.exe"
Copy-Item "$destDir\chromedriver.exe" "$copyPath\chromedriver.exe" -Force