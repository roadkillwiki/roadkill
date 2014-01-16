var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Step3WindowsAuthMessages = (function () {
                function Step3WindowsAuthMessages() {
                }
                return Step3WindowsAuthMessages;
            })();
            Installer.Step3WindowsAuthMessages = Step3WindowsAuthMessages;

            var step3WindowsAuth = (function () {
                function step3WindowsAuth(wizard, messages) {
                    this._wizard = wizard;
                    this._messages = messages;

                    // Set the page number in the header
                    this._wizard.updateNavigation(3);
                }
                step3WindowsAuth.prototype.configureValidation = function () {
                    // Form validation
                    var validationRules = {
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

                    var validation = new Roadkill.Web.Validation();
                    validation.Configure("#step3-form", validationRules);
                };

                step3WindowsAuth.prototype.bindButtons = function () {
                    var _this = this;
                    $("#testldap").click(function (e) {
                        _this.testActiveDirectory("");
                    });

                    $("#testeditor").click(function (e) {
                        _this.testActiveDirectory($("#EditorRoleName").val());
                    });

                    $("#testadmin").click(function (e) {
                        _this.testActiveDirectory($("#AdminRoleName").val());
                    });
                };

                step3WindowsAuth.prototype.testActiveDirectory = function (groupName) {
                    var _this = this;
                    var url = ROADKILL_INSTALLER_TESTLDAP_URL;
                    var jsonData = {
                        "connectionstring": $("#LdapConnectionString").val(),
                        "username": $("#LdapUsername").val(),
                        "password": $("#LdapPassword").val(),
                        "groupName": groupName
                    };

                    this._wizard.makeAjaxRequest(url, jsonData, function (data) {
                        _this.OnTestLdapSuccess(data);
                    });
                };

                step3WindowsAuth.prototype.OnTestLdapSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._messages.successTitle, this._messages.successMessage);
                    } else {
                        this._wizard.showFailure(this._messages.failureTitle, this._messages.failureMessage + "\n" + data.ErrorMessage);
                    }
                };
                return step3WindowsAuth;
            })();
            Installer.step3WindowsAuth = step3WindowsAuth;
        })(Web.Installer || (Web.Installer = {}));
        var Installer = Web.Installer;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
