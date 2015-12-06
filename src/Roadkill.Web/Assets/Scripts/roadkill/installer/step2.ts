/// <reference path="../typescript-ref/installerconstants.ts" />
module Roadkill.Web.Installer
{
	export class Step2Messages
	{
		public dbSuccessTitle: string;
		public dbSuccessMessage: string;
		public dbFailureTitle: string;
	}

	export class Step2
	{
		private _wizard: InstallWizard;
		private _messages: Step2Messages;

		constructor(wizard: InstallWizard, messages: Step2Messages)
		{
			this._wizard = wizard;
			this._messages = messages;

			// Set the page number in the header
			this._wizard.updateNavigation(2);

			// Form validation
			var validationRules =
				{
					SiteName: {
						required: true
					},
					SiteUrl: {
						required: true
					},
					ConnectionString: {
						required: true
					}
				};
			var validation = new Roadkill.Web.Validation();
			validation.Configure("#step2-form", validationRules);
		}

		public bindButtons()
		{
			$("td.example").click((e) =>
			{
				this.OnDbExampleClick(e);
			});

			$("#testdbconnection").click((e) =>
			{
				this.OnTestDatabaseClick(e);
			});
		}

		private OnDbExampleClick(e: any)
		{
			// e is a jQuery.Event
			$("#ConnectionString").val($(e.target).text());
			var dbtype = $(e.target).data("dbtype");
			$("#DataStoreTypeName").val(dbtype);
		}

		private OnTestDatabaseClick(e: any)
		{
			var url: string = ROADKILL_INSTALLER_TESTDATABASE_URL;
			var jsonData: any =
			{
				"connectionString": $("#ConnectionString").val(),
				"databaseType": $("#DataStoreTypeName").val()
			};

			this._wizard.makeAjaxRequest(url, jsonData, (data: any) => { this.OnTestDatabaseSuccess(data); });
		}

		private OnTestDatabaseSuccess(data: any)
		{
			if (data.Success)
			{
				this._wizard.showSuccess(this._messages.dbSuccessTitle, this._messages.dbSuccessMessage);
			}
			else
			{
				this._wizard.showFailure(this._messages.dbFailureTitle, data.ErrorMessage);
			}
		}
	}
}