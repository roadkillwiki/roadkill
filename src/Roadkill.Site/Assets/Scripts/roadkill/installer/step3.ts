module Roadkill.Site.Installer
{
	export class Step3Messages
	{
		public successTitle: string;
		public successMessage: string;
		public failureTitle: string;
		public failureMessage: string;
	}

	export class Step3
	{
		private _wizard: InstallWizard;
		private _messages: Step3Messages;

		constructor(wizard: InstallWizard, messages: Step3Messages)
		{
			this._wizard = wizard;
			this._messages = messages;

			// Set the page number in the header
			this._wizard.updateNavigation(3);
		}

		public configureWindowsAuthValidation()
		{
			// Form validation
			var validationRules =
			{
				LdapConnectionString: {
					required: true
				},
				LdapUsername: {
					required: true
				},
				LdapPassword: {
					required: true
				}
			};

			var validation = new Roadkill.Site.Validation();
			validation.Configure("#step2-form", validationRules);
		}

		public bindWindowsAuthButtons()
		{
			$("#testldap").click((e) =>
			{
				this.testActiveDirectory("");
			});

			$("#testeditor").click((e) =>
			{
				this.testActiveDirectory($("#EditorRoleName").val());
			});

			$("#testadmin").click((e) =>
			{
				this.testActiveDirectory($("#AdminRoleName").val());
			});
		}

		private testActiveDirectory(groupName)
		{
			var url: string = ROADKILL_INSTALLER_TESTLDAP_URL;
			var jsonData: any =
			{
				"connectionstring": $("#LdapConnectionString").val(),
				"username": $("#LdapUsername").val(),
				"password": $("#LdapPassword").val(),
				"groupName": groupName
			};

			this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnTestLdapSuccess(data); });
		}

		private OnTestLdapSuccess(data: any)
		{
			if (data.Success)
			{
				this._wizard.showSuccess(this._messages.successTitle, this._messages.successMessage);
			}
			else
			{
				this._wizard.showFailure(this._messages.failureTitle, this._messages.failureMessage + "\n" + data.ErrorMessage);
			}
		}
	}
}