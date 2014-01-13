/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class InstallWizard 
	{
		constructor()
		{
			// Set the bottom submit button to submit the form above it
			$("#bottom-buttons button[type=submit]").click(function ()
			{
				$("form").submit();
			});
		}

		public updateNavigation(pageNumber: number)
		{
			$("#trail li:nth-child(" + pageNumber + ")").addClass("selected");
		}

		public showSuccess(title: string, message: string)
		{
			toastr.success(message, title);
		}

		public showFailure(title: string, errorMessage: string)
		{	
			bootbox.alert("<h2>" +title +"<h2><pre style='max-height:500px;overflow-y:scroll;'>" +errorMessage+"</pre>");
		}

		public enableContinueButton()
		{
			$(".continue").removeClass("hidden");
			$(".continue").show();
		}

		public disableContinueButton()
		{
			$(".continue").addClass("hidden");
			$(".continue").hide();
		}

		public makeAjaxRequest(url: string, data: any, successFunction: (data: any) => void)
		{
			var request = $.ajax({
				type: "GET",
				url: url,
				data: data,
				dataType: "json"
			});

			request.done(successFunction);

			request.fail(function (jqXHR, textStatus, errorThrown: SyntaxError)
			{
				toastr.error(ROADKILL_INSTALLER_WOOPS + errorThrown);
			});
		}
	}
}