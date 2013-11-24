module Roadkill.Site.Installer
{
	$(document).ready(function ()
	{
		
	});

	export class Page5
	{
		private _wizard: Wizard;

		constructor(wizard: Wizard)
		{
			this._wizard = wizard;
			this._wizard.updateNavigation(5);
		}
	}
}