/// <reference path="../typescript-ref/installerconstants.ts" />

module Roadkill.Site.Installer
{
	export class Page1 
	{
		private _wizard: Wizard;
		private _successMessage: string;
		private _failureMessage: string;

		constructor(wizard: Wizard, successMessage: string, failureMessage: string)
		{
			this._wizard = wizard;
			this._successMessage = successMessage;
			this._failureMessage = failureMessage;

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
			// TODO-translation
			var url: string = ROADKILL_INSTALLER_TESTWEBCONFIG_URL;
			this._wizard.makeAjaxRequest(url, {}, "Woops, something went wrong", this.OnTestWebConfigSuccess);
		}

		private OnTestWebConfigSuccess(data: any)
		{
			if (data.Success)
			{
				this._wizard.showSuccess(this._successMessage);
				this._wizard.enableContinueButton();
			}
			else
			{
				this._wizard.showFailure(this._failureMessage, data.ErrorMessage);
				this._wizard.disableContinueButton();
			}
		}
	}
}