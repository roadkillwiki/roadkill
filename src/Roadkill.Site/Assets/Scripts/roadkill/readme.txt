These typescript files are 'compiled' into Javascript when the web project is compiled, by the typescript 
targets in the csproj file. The JS files still need to be present for the AzureDeploy to work

All the file.js files (not the min versions) are bundled by the ASP.NET bundler.

The typescript-ref folder contains files to assist Typescript, none of the js files are included 
by the bundler or on the site.