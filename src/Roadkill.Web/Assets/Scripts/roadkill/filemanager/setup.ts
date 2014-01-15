/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Web.FileManager
{
	/**
	 Event bindings and handlers for the file manager.
	*/
	export class Setup
	{
		public static configure()
		{
			this.initializeImagePreview();
			this.initializeFileUpload();

			var buttonEvents = new ButtonEvents();
			buttonEvents.bind();

			var tableEvents = new TableEvents();
			tableEvents.bind();

			TableEvents.update("/");
		}

		private static initializeImagePreview()
		{
			var xOffset: number = 20;
			var yOffset: number = 20;
			var rowSelector = "table#files tr[data-itemtype=file]"; // see http://stackoverflow.com/a/12571166/21574

			$("#folder-container")
				.on("mouseenter", rowSelector,function (e)
				{
					var fileType: string;
					fileType = $("td.filetype", this).text();
					if (fileType.search(/^(jpg|png|gif)$/i) == -1)
						return;

					var imgUrl: string;
					imgUrl = ROADKILL_ATTACHMENTSPATH + TableEvents.getCurrentPath() + "/";
					imgUrl = imgUrl.replace("//", "/") + $("td.file", this).text();

					$("body").append("<p id='image-preview'><img src='" + imgUrl + "' alt='Image Preview' /></p>");
					$("#image-preview")
						.css("top", (e.pageY - xOffset) + "px")
						.css("left", (e.pageX + yOffset) + "px")
						.fadeIn("fast");
				})
				.on("mouseleave", rowSelector, function ()
				{
					$("#image-preview").remove();
				})
				.on("mousemove", rowSelector, function (e)
				{
					$("#preview")
						.css("top", (e.pageY - xOffset) + "px")
						.css("left", (e.pageX + yOffset) + "px");
				});
		}

		private static initializeFileUpload()
		{
			$("#fileupload").fileupload({
				dropZone: $("#folder-container"),
				pasteZone: $("body"),
				dataType: "json",
				progressall: function (e, data)
				{
					$("#progress").show();

					var percentage = (data.loaded / data.total * 100) + "";
					var progress = parseInt(percentage, 10);
					$("#progress .bar").css("width", progress + "%");
				},
				done: function (e, data)
				{
					$("#progress").hide();

					if (data.result.status == "error")
					{
						toastr.error(data.result.message);
						return;
					}
					else
					{
						toastr.success(data.result.filename + " uploaded successfully.");

						TableEvents.update("", false);
						setTimeout(function () { $("#progress div.bar").css("width", "0%"); }, 2000);
					}
				}
			})
			.bind("fileuploaddrop", function (e, data)
			{
				TableEvents.update("", false);
			});
		}
	}
}