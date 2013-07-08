$env:Path = "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\"
cd ..\..\src\Roadkill.Core\Localization\Text
& resgen /compile "SiteStrings.txt,SiteStrings.resx"
move -Path SiteStrings.resx ..\Resx\ -Force
cd ..\..\..\..\lib\tools