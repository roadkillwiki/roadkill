$appVeyorConfig = "lib\Configs\connectionStrings.appveyor.config";
$destConfigPath  = "lib\Configs\connectionStrings.dev.config";

Copy-Item -Path $appVeyorConfig -Destination $devConfigPath -Force

Write-Host "Contents of lib\Configs\connectionStrings.dev.config :"
Write-Host "------------------------------------------------------"
Get-Content -Path "lib\Configs\connectionStrings.dev.config"
Write-Host "------------------------------------------------------"