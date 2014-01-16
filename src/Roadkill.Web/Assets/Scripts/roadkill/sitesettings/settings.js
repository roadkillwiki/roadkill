var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/references.ts" />
        (function (Admin) {
            var SettingsMessages = (function () {
                function SettingsMessages() {
                }
                return SettingsMessages;
            })();
            Admin.SettingsMessages = SettingsMessages;

            var Settings = (function () {
                function Settings(messages) {
                    var _this = this;
                    // Test button messages
                    this._messages = messages;

                    // Help popovers
                    $("input[rel=popover][type!=checkbox]").popover({ container: "body", placement: "right", trigger: "hover", html: true });
                    $("input[type=checkbox][rel=popover],textarea[rel=popover],select[rel=popover]").popover({ container: "body", placement: "right", trigger: "hover", html: true });

                    // Make the windows auth checkbox toggle the forms-auth/windows-auth sections.
                    this.ToggleUserSettings();
                    $("#UseWindowsAuth").click(function (e) {
                        _this.ToggleUserSettings();
                    });

                    // Button clicks
                    $("#testdbconnection").click(function (e) {
                        _this.OnTestDatabaseClick();
                    });
                    $("#testattachments").click(function (e) {
                        _this.OnTestAttachmentsClick();
                    });

                    // Form validation
                    var validationRules = {
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
                Settings.prototype.OnTestDatabaseClick = function () {
                    var _this = this;
                    $("#db-loading").removeClass("hidden");
                    $("#db-loading").show();

                    var jsonData = {
                        "connectionString": $("#ConnectionString").val(),
                        "databaseType": $("#DataStoreTypeName").val()
                    };

                    // Make sure to use a lambda, so the "this" references is kept intact
                    this.makeAjaxRequest(ROADKILL_TESTDB_URL, jsonData, this._messages.unexpectedError, function (data) {
                        _this.TestDatabaseSuccess(data);
                    });
                };

                Settings.prototype.TestDatabaseSuccess = function (data) {
                    $("#db-loading").hide();
                    if (data.Success) {
                        toastr.success(this._messages.dbSuccessTitle);
                    } else {
                        this.showFailure(this._messages.dbFailureTitle, data.ErrorMessage);
                    }
                };

                Settings.prototype.OnTestAttachmentsClick = function () {
                    var _this = this;
                    var jsonData = {
                        "folder": $("#AttachmentsFolder").val()
                    };

                    // Make sure to use a lambda, so the "this" references is kept intact
                    this.makeAjaxRequest(ROADKILL_TESTATTACHMENTS_URL, jsonData, this._messages.unexpectedError, function (data) {
                        _this.TestAttachmentsSuccess(data);
                    });
                };

                Settings.prototype.TestAttachmentsSuccess = function (data) {
                    if (data.Success) {
                        toastr.success(this._messages.attachmentsSuccess);
                    } else {
                        this.showFailure(this._messages.attachmentsFailureTitle, data.ErrorMessage);
                    }
                };

                Settings.prototype.makeAjaxRequest = function (url, data, errorMessage, successFunction) {
                    var request = $.ajax({
                        type: "GET",
                        url: url,
                        data: data,
                        dataType: "json"
                    });

                    request.done(successFunction);

                    request.fail(function (jqXHR, textStatus, errorThrown) {
                        if (errorThrown.message.indexOf("unexpected character") !== -1) {
                            window.location = window.location;
                        } else {
                            toastr.error(errorMessage + errorThrown);
                        }
                    });
                };

                Settings.prototype.ToggleUserSettings = function () {
                    if ($("#UseWindowsAuth").is(":checked")) {
                        $("#aspnetuser-settings").hide();
                        $("#ldapsettings").show();
                        $("#ldapsettings").removeClass("hidden");
                    } else {
                        $("#ldapsettings").hide();
                        $("#aspnetuser-settings").show();
                        $("#aspnetuser-settings").removeClass("hidden");
                    }
                };

                Settings.prototype.showFailure = function (title, errorMessage) {
                    bootbox.alert("<h2>" + title + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + errorMessage + "</pre>");
                };
                return Settings;
            })();
            Admin.Settings = Settings;
        })(Web.Admin || (Web.Admin = {}));
        var Admin = Web.Admin;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
