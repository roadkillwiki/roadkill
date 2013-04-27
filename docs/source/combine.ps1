Get-ChildItem *.md | Sort-Object { [regex]::Replace($_.Name, '\d+', { $args[0].Value.PadLeft(20) }) }| gc | out-file combinedfile.txt
ren combinedfile.txt readme.md
move -Force readme.md ..\..\readme.md
"Markdown combining complete"
"....."

"Now delete the manual.html and manual.pdf files and export "
"the readme.md using Markdown Pad to manual.html"
" "
"Once this is done, open the file in Chrome and print it as a PDF to manual.pdf"