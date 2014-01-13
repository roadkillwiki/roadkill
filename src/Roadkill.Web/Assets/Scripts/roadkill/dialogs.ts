/// <reference path="typescript-ref/references.ts" />
module Roadkill.Web
{
	export class Dialogs
	{
		public static alert(message: string)
		{
			bootbox.setDefaults({ animate: false });
			bootbox.alert(message);
		}

		public static confirm(title: string, resultFunction: (result: boolean) => void)
		{
			bootbox.setDefaults({ animate: false });
			bootbox.confirm("<b>" + title + "</b>", resultFunction);
		}

		public static openModal(selector: string)
		{
			$(selector).modal("show");
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