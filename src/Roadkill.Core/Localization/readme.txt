Roadkill Localization
==================================

There are two sets of localization files in Roadkill: installation strings and site-wide strings.
These are managed in POEditor.com, and then exported and copied into the solution when any updates occur.

The default language is British English, which InstallStrings.resx and SiteStrings.resx are in. All languages that are 
missing translations keys default to this.

When a new key is added to either of these files, the resx file is exported from POEditor.com and copied into the 
solution. The file is then opened in Visual Studio and the Access Modifier is changed to public, which ensures 
the solution compiles as the designer file is generated.

You don't need to do this step for the other languages however.

POEditor.com has two projects:

Roadkill installer - https://poeditor.com/join/project?hash=91f1c5c82a1467e41e3560d65fc017b6
Roadkill site - https://poeditor.com/join/project?hash=b0ed5a645e19dfd403afa99224f887bd

These two projects contain a mixture of manual translations and automatic (Google Translate) translations.
When you export from POEditor.com, the file names of the resx files need to change to be in
the format "InstallStrings.{code}.resx" and "SiteStrings.{code}.resx.".

A full list of the codes can be found here: 
http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo(v=vs.80).aspx

Current languages/codes supported are:

	Czech      - cs
	Dutch      - nl
	German     - de
	Hindi      - hi
	Italian    - it
	Polish     - pl
	Russian    - ru
	Portuguese - pt
	Spanish    - es
	Swedish    - sv

The powershell script for performing the bulk renames:

ren Roadkill_Installer_Czech.resx InstallStrings.cs.resx
ren Roadkill_Installer_Dutch.resx InstallStrings.nl.resx
ren Roadkill_Installer_English.resx InstallStrings.resx
ren Roadkill_Installer_German.resx InstallStrings.de.resx
ren Roadkill_Installer_Hindi.resx InstallStrings.hi.resx
ren Roadkill_Installer_Italian.resx InstallStrings.it.resx
ren Roadkill_Installer_Polish.resx InstallStrings.pl.resx
ren Roadkill_Installer_Portuguese.resx InstallStrings.pt.resx
ren Roadkill_Installer_Russian.resx InstallStrings.ru.resx
ren Roadkill_Installer_Spanish.resx InstallStrings.es.resx
ren Roadkill_Installer_Swedish.resx InstallStrings.sv.resx

ren Roadkill_Site_Czech.resx SiteStrings.cs.resx
ren Roadkill_Site_Dutch.resx SiteStrings.nl.resx
ren Roadkill_Site_English.resx SiteStrings.resx
ren Roadkill_Site_German.resx SiteStrings.de.resx
ren Roadkill_Site_Hindi.resx SiteStrings.hi.resx
ren Roadkill_Site_Italian.resx SiteStrings.it.resx
ren Roadkill_Site_Polish.resx SiteStrings.pl.resx
ren Roadkill_Site_Portuguese.resx SiteStrings.pt.resx
ren Roadkill_Site_Russian.resx SiteStrings.ru.resx
ren Roadkill_Site_Spanish.resx SiteStrings.es.resx
ren Roadkill_Site_Swedish.resx SiteStrings.sv.resx