/// <reference path="../typescript-ref/references.ts" />
module Roadkill.Web.Admin
{
	export class SettingsMessages
	{
		public dbSuccessTitle: string;
		public dbFailureTitle: string;
		public attachmentsSuccess: string;
		public attachmentsFailureTitle: string;
		public unexpectedError: string;
	}

	export class Settings
	{
		private _messages: SettingsMessages;

		constructor(messages : SettingsMessages)
		{
			// Test button messages
			this._messages = messages;

			// Help popovers
			$("input[rel=popover][type!=checkbox]").popover({ container: "body", placement: "right", trigger: "hover", html: true });
			$("input[type=checkbox][rel=popover],textarea[rel=popover],select[rel=popover]").popover({ container: "body", placement: "right", trigger: "hover", html: true });

			// Make the windows auth checkbox toggle the forms-auth/windows-auth sections.
			this.ToggleUserSettings(); // initial display
			$("#UseWindowsAuth").click((e) =>
			{
				this.ToggleUserSettings();
			});

			// Button clicks
			$("#testdbconnection").click((e) =>
			{
				this.OnTestDatabaseClick();
			});
			$("#testattachments").click((e) =>
			{
				this.OnTestAttachmentsClick();
			});

			// Form validation
			var validationRules =
			{
				AllowedFileTypes: {
					required: true
				},
				AttachmentsFolder: {
					required: true
				}
			};
			var validation = new Roadkill.Web.Validation();
			validation.Configure("#settings-form", validationRules);
		}

		public OnTestDatabaseClick()
		{
			$("#db-loading").removeClass("hidden");
			$("#db-loading").show();

			var jsonData: any =
			{
				"connectionString": $("#ConnectionString").val(),
				"databaseType": $("#DatabaseName").val()
			};

			// Make sure to use a lambda, so the "this" references is kept intact
			this.makeAjaxRequest(ROADKILL_TESTDB_URL, jsonData, this._messages.unexpectedError, (data) => { this.TestDatabaseSuccess(data); });
		}

		public TestDatabaseSuccess(data : any)
		{
			$("#db-loading").hide();
			if (data.Success)
			{
				toastr.success(this._messages.dbSuccessTitle);
			}
			else
			{
				this.showFailure(this._messages.dbFailureTitle, data.ErrorMessage);
			}
		}

		public OnTestAttachmentsClick()
		{
			var jsonData: any =
			{
				"folder": $("#AttachmentsFolder").val()
			}

			// Make sure to use a lambda, so the "this" references is kept intact
			this.makeAjaxRequest(ROADKILL_TESTATTACHMENTS_URL, jsonData, this._messages.unexpectedError, (data) => { this.TestAttachmentsSuccess(data) } );
		}

		public TestAttachmentsSuccess(data: any)
		{
			if (data.Success)
			{
				toastr.success(this._messages.attachmentsSuccess);
			}
			else
			{
				this.showFailure(this._messages.attachmentsFailureTitle, data.ErrorMessage);
			}
		}

		private makeAjaxRequest(url: string, data: any, errorMessage: string, successFunction: (data: any) => void)
		{
			var request = $.ajax({
				type: "GET",
				url: url,
				data: data,
				dataType: "json"
			});

			request.done(successFunction);

			request.fail(function (jqXHR, textStatus, errorThrown: SyntaxError)
			{
				// Logged out since the call was made
				if (errorThrown.message.indexOf("unexpected character") !== -1)
				{
					window.location.href = window.location.href;
				}
				else
				{
					toastr.error(errorMessage + errorThrown);
				}
			});
		}

		public ToggleUserSettings()
		{
			if ($("#UseWindowsAuth").is(":checked"))
			{
				$("#aspnetuser-settings").hide();
				$("#ldapsettings").show();
				$("#ldapsettings").removeClass("hidden");
			}
			else
			{
				$("#ldapsettings").hide();
				$("#aspnetuser-settings").show();
				$("#aspnetuser-settings").removeClass("hidden");
			}
		}

		public showFailure(title: string, errorMessage: string)
		{
			bootbox.alert("<h2>" + title + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + errorMessage + "</pre>");
		}
	}
}