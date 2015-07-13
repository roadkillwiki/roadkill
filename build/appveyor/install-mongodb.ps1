$url = https://fastdl.mongodb.org/win32/mongodb-win32-x86_64-2008plus-ssl-3.0.4-signed.msi

Write-Host "Downloading $url..."
$msiPath = "$($env:USERPROFILE)\mongodb-win32-x86_64-2008plus-ssl-3.0.4-signed.msi"
(New-Object Net.WebClient).DownloadFile($url, $msiPath)
Write-Host "Done"

Write-Host "Launching MSI installer"
cmd /c start /wait msiexec /q /i $msiPath INSTALLLOCATION=C:\mongodb ADDLOCAL="all"
del $msiPath
 
mkdir c:\mongodb\data\db | Out-Null
mkdir c:\mongodb\log | Out-Null
 
'systemLog:
    destination: file
    path: c:\mongodb\log\mongod.log
storage:
    dbPath: c:\mongodb\data\db' | Out-File C:\mongodb\mongod.cfg -Encoding utf8
 
Write-Host "Installing mongodb"
cmd /c start /wait sc create MongoDB binPath= "C:\mongodb\bin\mongod.exe --service --config=C:\mongodb\mongod.cfg" DisplayName= "MongoDB" start= "demand"
 
& c:\mongodb\bin\mongod --version

Write-Host "Starting mongodb service" 
Start-Service mongodb
Write-Host "Mongodb install complete."
Write-Host "--------------------------------------"