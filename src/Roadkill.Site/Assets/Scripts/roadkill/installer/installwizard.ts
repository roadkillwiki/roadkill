/// <reference path="../typescript-ref/installerconstants.ts" />

module Roadkill.Site.Installer
{
	export class InstallWizard 
	{
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
			bootbox.alert("<h2>" +title +"<h2><pre>" +errorMessage+"</pre>");
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