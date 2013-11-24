/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site.Admin
{
	export class Settings
	{
		constructor()
		{
			// Help popovers
			$("input[rel=popover][type!=checkbox]").popover({ container: "body", placement: "right", trigger: "hover", html: true });
			$("input[type=checkbox][rel=popover],textarea[rel=popover],select[rel=popover]").popover({ container: "body", placement: "right", trigger: "hover", html: true });

			// Make the windows auth checkbox toggle the forms-auth/windows-auth sections.
			$("#UseWindowsAuth").click(function ()
			{
				this.toggleUserSettings();
			});

			// Button clicks
			$("#testdbconnection").click((e) => { this.OnTestDatabaseClick(); });
			$("#testattachments").click((e) => { this.OnTestAttachmentsClick(); });

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
			var validation = new Roadkill.Site.Validation();
			validation.Configure("#settings-form", validationRules);
		}

		public OnTestDatabaseClick()
		{
			$("#db-loading").removeClass("hidden");
			$("#db-loading").show();

			var jsonData: any =
			{
				"connectionString": $("#ConnectionString").val(),
				"databaseType": $("#DataStoreTypeName").val()
			};

			this.makeAjaxRequest(ROADKILL_TESTDB_URL, jsonData, "Something went wrong", this.TestDatabaseSuccess);
		}

		public TestDatabaseSuccess(data : any)
		{
			// TODO-translation
			$("#db-loading").hide();
			if (data.Success)
			{
				toastr.success("Database connection was successful.");
			}
			else
			{
				toastr.error("Database connection failed: <br/>" + data.ErrorMessage);
			}
		}

		public OnTestAttachmentsClick()
		{
			var jsonData: any =
			{
				"folder": $("#AttachmentsFolder").val()
			}

			this.makeAjaxRequest(ROADKILL_TESTATTACHMENTS_URL, jsonData, "Something went wrong", this.TestAttachmentsSuccess);
		}

		public TestAttachmentsSuccess(data: any)
		{
			if (data.Success)
			{
				toastr.success("Success! The directory exists and can be written to.");
			}
			else
			{
				toastr.error("Attachments directory failed: <br/>" + data.ErrorMessage);
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
					window.location = window.location;
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
	}
}