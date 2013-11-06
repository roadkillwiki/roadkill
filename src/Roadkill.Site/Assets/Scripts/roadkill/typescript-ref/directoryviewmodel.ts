/// <reference path="filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export interface DirectoryViewModel
	{
		status: string;
		message: string;
		Name: string;
		UrlPath: string;
		ChildFolders: DirectoryViewModel[];
		Files: FileModel[];
	}
}