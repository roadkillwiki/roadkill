/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class Step3DbMessages
	{
		public passwordsDontMatch : string;
	}

	export class Step3Db
	{
		private _wizard: InstallWizard;
		private _messages: Step3DbMessages;

		constructor(wizard: InstallWizard, messages: Step3DbMessages)
		{
			this._wizard = wizard;
			this._messages = messages;

			// Set the page number in the header
			this._wizard.updateNavigation(3);
		}

		public configureValidation()
		{
			// Form validation
			var validationRules =
			{
				EditorRoleName: {
					required: true
				},
				AdminRoleName: {
					required: true
				},
				AdminEmail: {
					required: true
				},
				AdminPassword: {
					required: true
				},
				password2: {
					required: true,
					equalTo: "#AdminPassword",
					messages: {
						equalTo: this._messages.passwordsDontMatch
					}
				}
			};

			var validation = new Roadkill.Web.Validation();
			validation.Configure("#step3-form", validationRules);

			var rules = $("#password2").rules();
			rules.messages.equalTo = "The passwords don't match";
			$("#password2").rules("add", rules);
		}
	}
}