/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class Step1Messages
	{
		public successTitle: string;
		public successMessage: string;
		public failureTitle: string;
		public failureMessage: string;
	}

	export class Step1 
	{
		private _wizard: InstallWizard;
		private _messages: Step1Messages;

		constructor(wizard: InstallWizard, messages: Step1Messages)
		{
			this._wizard = wizard;
			this._messages = messages;

			this._wizard.updateNavigation(1);
		}

		public bindButtons()
		{
			$("#testwebconfig").click((e) =>
			{
				this.OnTestWebConfigClick(e);
			});
		}

		private OnTestWebConfigClick(e : any)
		{
			var url: string = ROADKILL_INSTALLER_TESTWEBCONFIG_URL;
			this._wizard.makeAjaxRequest(url, {}, (data: any) => { this.OnTestWebConfigSuccess(data); });
		}

		private OnTestWebConfigSuccess(data: any)
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