/// <reference path="filemanager.references.ts" />
module Roadkill.Web.FileManager
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