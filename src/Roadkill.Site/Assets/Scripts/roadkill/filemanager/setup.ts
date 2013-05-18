/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
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

			$("table#files tr[data-itemtype=file]")
				.live("mouseenter", function (e)
				{
					var fileType: string;
					fileType = $("td.filetype", this).text();
					if (fileType.search(/^(jpg|png|gif)$/i) == -1)
						return;

					var imgUrl: string;
					imgUrl = (ROADKILL_ATTACHMENTSPATH + TableEvents.getCurrentPath() + "/");
					imgUrl = imgUrl.replace("//", "/") + $("td.file", this).text();

					$("body").append("<p id='image-preview'><img src='" + imgUrl + "' alt='Image Preview' /></p>");
					$("#image-preview")
						.css("top", (e.pageY - xOffset) + "px")
						.css("left", (e.pageX + yOffset) + "px")
						.fadeIn("fast");
				})
				.live("mouseleave", function ()
				{
					$("#image-preview").remove();
				})
				.live("mousemove", function (e)
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
					var percentage = (data.loaded / data.total * 100) + "";
					var progress = parseInt(percentage, 10);
					$("#progress .bar").css("width", progress + "%");
				},
				done: function (e, data)
				{
					if (data.result.status == "error")
					{
						alert(data.result.message);
						return;
					}
					else
					{
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