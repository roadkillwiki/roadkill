$appVeyorConfig = "lib\Configs\connectionStrings.appveyor.config";
$destConfigPath  = "lib\Configs\connectionStrings.dev.config";

Copy-Item -Path $appVeyorConfig -Destination $devConfigPath -Force