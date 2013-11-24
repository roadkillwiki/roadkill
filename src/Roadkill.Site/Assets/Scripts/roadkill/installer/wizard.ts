/// <reference path="../typescript-ref/installerconstants.ts" />

module Roadkill.Site.Installer
{
	export class Wizard 
	{
		public updateNavigation(pageNumber: number)
		{
			$("#trail li:nth-child(" + pageNumber + ")").addClass("selected");
		}

		public showSuccess(message: string)
		{
			toastr.success(message);
		}

		public showFailure(message: string, errorMessage: string)
		{	
			toastr.failure(message + "<br/>" +errorMessage);
		}

		public enableContinueButton()
		{
			$(".continue").show();
		}

		public disableContinueButton()
		{
			$(".continue").hide();
		}

		public makeAjaxRequest(url: string, data: any, errorMessage: string, successFunction: (data: any) => void)
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
				toastr.error(errorMessage + errorThrown);
			});
		}
	}
}