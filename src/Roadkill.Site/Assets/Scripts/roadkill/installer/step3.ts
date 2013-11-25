module Roadkill.Site.Installer
{
	export class Step3WindowsAuthMessages
	{
		public successTitle: string;
		public successMessage: string;
		public failureTitle: string;
		public failureMessage: string;
	}

	export class Step3
	{
		private _wizard: InstallWizard;
		private _messages: Step3WindowsAuthMessages;

		constructor(wizard: InstallWizard, messages: Step3WindowsAuthMessages)
		{
			this._wizard = wizard;
			this._messages = messages;

			// Set the page number in the header
			this._wizard.updateNavigation(3);
		}

		public configureDatabaseAuthValidation()
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
						equalTo: ""// TODO-translation
					}
				}
			};

			var validation = new Roadkill.Site.Validation();
			validation.Configure("#step3-form", validationRules);

			var rules = $("#password2").rules();
			rules.messages.equalTo = "The passwords don't match";
			$("#password2").rules("add", rules);
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
			validation.Configure("#step3-form", validationRules);
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