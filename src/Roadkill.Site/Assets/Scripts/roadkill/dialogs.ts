/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	export class Dialogs
	{
		public static openModal(selector: string, params?: any)
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

		public static openFullScreenModal(selector: string)
		{
			$(selector).modal("show");
			$(selector).css("width", $(window).width() - 110);
			$(selector).css("height", $(window).height() - 110);
			$(window).on("resize", function ()
			{
				$(selector).css("width", $(window).width() - 110);
				$(selector).css("height", $(window).height() - 110);
			});
		}

		public static openIFrameModal(html: string)
		{
			$("#iframe-dialog .modal-body").html(html);
			$("#iframe-dialog").modal("show");
		}

		public static closeModal()
		{
			$.fancybox.close(true);
		}

		public static closeModal2(selector: string)
		{
			$(selector).modal("hide");
		}
	}
}