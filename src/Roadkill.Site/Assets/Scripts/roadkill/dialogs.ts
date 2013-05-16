/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	export class Dialogs
	{
		public static openModal(selector:string, params?:any)
		{
			if (typeof params !== "undefined")
			{
				params.openSpeed = 150;
				params.closeSpeed = 150;
			}
			else
			{
				params = { openSpeed: 150, closeSpeed: 150 };
			}

			$.fancybox($(selector), params);
		}

		public static openIFrameModal(html:string)
		{
			$.fancybox(html, { openSpeed: "fast", openEffect: "none" });
		}

		public static closeModal()
		{
			$.fancybox.close(true);
		}
	}
}