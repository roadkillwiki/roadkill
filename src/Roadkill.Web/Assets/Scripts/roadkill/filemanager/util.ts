/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Web.FileManager
{
	export class Util
	{
        public static IsStringNullOrEmpty(text: string): boolean
		{
			return (text === null || text === "" || typeof text === "undefined");
		}

		public static FormatString(format: string, ...args: any[]): string
		{
			var result = format;
			for (var i = 0; i < args.length; i++)
			{
				var regex = new RegExp('\\{' + (i) + '\\}', 'gm');
				result = result.replace(regex, args[i]);
			}

			return result;
		}
	}
}