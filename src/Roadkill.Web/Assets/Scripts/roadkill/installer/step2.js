var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Step2Messages = (function () {
                function Step2Messages() {
                }
                return Step2Messages;
            })();
            Installer.Step2Messages = Step2Messages;

            var Step2 = (function () {
                function Step2(wizard, messages) {
                    this._wizard = wizard;
                    this._messages = messages;

                    // Set the page number in the header
                    this._wizard.updateNavigation(2);

                    // Form validation
                    var validationRules = {
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
                Step2.prototype.bindButtons = function () {
                    var _this = this;
                    $("td.example").click(function (e) {
                        _this.OnDbExampleClick(e);
                    });

                    $("#testdbconnection").click(function (e) {
                        _this.OnTestDatabaseClick(e);
                    });

                    $("#sqlitecopy").click(function (e) {
                        _this.OnCopySqliteClick(e);
                    });
                };

                Step2.prototype.OnDbExampleClick = function (e) {
                    // e is a jQuery.Event
                    $("#ConnectionString").val($(e.target).text());
                    var dbtype = $(e.target).data("dbtype");
                    $("#DataStoreTypeName").val(dbtype);
                };

                Step2.prototype.OnTestDatabaseClick = function (e) {
                    var _this = this;
                    var url = ROADKILL_INSTALLER_TESTDATABASE_URL;
                    var jsonData = {
                        "connectionString": $("#ConnectionString").val(),
                        "databaseType": $("#DataStoreTypeName").val()
                    };

                    this._wizard.makeAjaxRequest(url, jsonData, function (data) {
                        _this.OnTestDatabaseSuccess(data);
                    });
                };

                Step2.prototype.OnCopySqliteClick = function (e) {
                    var _this = this;
                    var url = ROADKILL_INSTALLER_COPYSQLITE_URL;
                    this._wizard.makeAjaxRequest(url, {}, function (data) {
                        _this.OnCopySqliteSuccess(data);
                    });
                };

                Step2.prototype.OnTestDatabaseSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._messages.dbSuccessTitle, this._messages.dbSuccessMessage);
                    } else {
                        this._wizard.showFailure(this._messages.dbFailureTitle, data.ErrorMessage);
                    }
                };

                Step2.prototype.OnCopySqliteSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._messages.sqliteSuccessTitle, this._messages.sqliteSuccessMessage);
                    } else {
                        this._wizard.showFailure(this._messages.sqliteFailureTitle, this._messages.sqliteFailureMessage + "\n" + data.ErrorMessage);
                    }
                };
                return Step2;
            })();
            Installer.Step2 = Step2;
        })(Web.Installer || (Web.Installer = {}));
        var Installer = Web.Installer;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
