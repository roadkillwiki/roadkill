/// <reference path="filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export interface DirectorySummary
	{
		status: string;
		message: string;
		Name: string;
		UrlPath: string;
		ChildFolders: DirectorySummary[];
		Files: FileSummary[];
	}
}