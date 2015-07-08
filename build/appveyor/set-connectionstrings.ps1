$connectionStrings = Get-Content lib\Configs\connectionStrings.dev.config;
$hardcodedCsharp = Get-Content src\Roadkill.Tests\Setup\SqlExpressSetup.cs;

$originalDbConnection = "Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill"
$appveyorDbConnection = "Server=(local)\SQL2012SP1;Database=master;User ID=sa;Password=Password12!"

Get-Content $connectionStrings.replace($originalDbConnection, $appveyorDbConnection) | Out-File lib\Configs\connectionStrings.dev.config;
Get-Content $hardcodedCsharp.replace($originalDbConnection, $appveyorDbConnection) | Out-File src\Roadkill.Tests\Setup\SqlExpressSetup.cs