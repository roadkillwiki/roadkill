/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	export class Dialogs
	{
		public static alert(message: string)
		{
			bootbox.animate(false);
			bootbox.alert(message);
		}

		public static confirm(title: string, resultFunction: (result: bool) => void )
		{
			bootbox.animate(false);
			bootbox.confirm("<b>" +title+ "</b>", resultFunction);
		}

		public static openModal(selector: string)
		{
			$(selector).modal("show");
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

		public static openMarkupHelpModal(html: string)
		{
			$("#markup-help-dialog .modal-body-container").html(html);
			$("#markup-help-dialog").modal("show");
		}

		public static openImageChooserModal(html: string)
		{
			$("#choose-image-dialog .modal-body-container").html(html);
			$("#choose-image-dialog").modal("show");
		}

		public static closeImageChooserModal()
		{
			$("#choose-image-dialog").modal("hide");
		}

		public static closeModal(selector: string)
		{
			$(selector).modal("hide");
		}
	}
}