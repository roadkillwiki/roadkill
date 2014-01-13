/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class Step5
	{
		private _wizard: InstallWizard;

		constructor(wizard: InstallWizard)
		{
			this._wizard = wizard;
			this._wizard.updateNavigation(5);
		}
	}
}