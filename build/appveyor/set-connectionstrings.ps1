$devConfigPath = "lib\Configs\connectionStrings.dev.config";
$testsSetupPath = "src\Roadkill.Tests\Setup\SqlExpressSetup.cs";

$connectionStrings = Get-Content -Path $devConfigPath;
$hardcodedCsharp = Get-Content -Path $testsSetupPath;

$originalDbConnection = "Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill";
$appveyorDbConnection = "Server=(local)\SQL2012SP1;Database=master;User ID=sa;Password=Password12!";

$connectionStrings.replace($originalDbConnection, $appveyorDbConnection) | Out-File $devConfigPath;
$hardcodedCsharp.replace($originalDbConnection, $appveyorDbConnection) | Out-File $testsSetupPath;