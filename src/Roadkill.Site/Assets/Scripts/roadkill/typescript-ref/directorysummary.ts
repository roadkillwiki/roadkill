/// <reference path="filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export interface DirectorySummary
	{
		Name: string;
		UrlPath: string;
		ChildFolders: DirectorySummary[];
		Files: FileSummary[];
	}
}