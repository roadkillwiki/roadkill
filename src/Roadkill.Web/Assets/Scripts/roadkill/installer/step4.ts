/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class Step4Messages
	{
		public successMessage: string;
		public failureMessage: string;
	}

	export class Step4
	{
		private _wizard: InstallWizard;
		private _messages: Step1Messages;

		constructor(wizard: InstallWizard, messages: Step1Messages)
		{
			this._wizard = wizard;
			this._messages = messages;

			this._wizard.updateNavigation(4);
		}

		public bindButtons()
		{
			$("#testattachments").click((e) =>
			{
				this.OnTestAttachmentsClick(e);
			});

			// Prevent a double click when submitting
			$("form").submit(() =>
			{
				$("#next-button").attr("disabled", "disabled");
			});
		}

		public configureValidation()
		{
			// Form validation
			var validationRules =
				{
					AttachmentsFolder: {
						required: true
					},
					AllowedFileTypes: {
						required: true
					}
				};

			var validation = new Roadkill.Web.Validation();
			validation.Configure("#step4-form", validationRules);
		}

		private OnTestAttachmentsClick(e: any)
		{
			var jsonData: any =
			{
				"folder": $("#AttachmentsFolder").val()
			};

			var url: string = ROADKILL_INSTALLER_TESTATTACHMENTS_URL;
			this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnTestAttachmentsSuccess(data); });
		}

		private OnTestAttachmentsSuccess(data: any)
		{
			if (data.Success)
			{
				this._wizard.showSuccess(this._messages.successTitle, this._messages.successMessage);
				this._wizard.enableContinueButton();
			}
			else
			{
				this._wizard.showFailure(this._messages.failureTitle, this._messages.failureMessage + "\n" + data.ErrorMessage);
				this._wizard.disableContinueButton();
			}
		}
	}
}