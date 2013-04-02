Get-ChildItem *.md | Sort-Object { [regex]::Replace($_.Name, '\d+', { $args[0].Value.PadLeft(20) }) }| gc | out-file combinedfile.txt
ren combinedfile.txt readme.md
move -Force readme.md ..\..\readme.md