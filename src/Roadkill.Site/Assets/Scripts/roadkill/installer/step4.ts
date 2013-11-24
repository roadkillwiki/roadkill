module Roadkill.Site.Installer
{
	$(document).ready(function ()
	{
		
	});

	export class Page4
	{
		private _wizard: InstallWizard;

		constructor(wizard: InstallWizard)
		{
			this._wizard = wizard;
			this._wizard.updateNavigation(4);
		}
	}
}