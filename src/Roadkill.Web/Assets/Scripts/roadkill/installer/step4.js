var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Step4Messages = (function () {
                function Step4Messages() {
                }
                return Step4Messages;
            })();
            Installer.Step4Messages = Step4Messages;

            var Step4 = (function () {
                function Step4(wizard, messages) {
                    this._wizard = wizard;
                    this._messages = messages;

                    this._wizard.updateNavigation(4);
                }
                Step4.prototype.bindButtons = function () {
                    var _this = this;
                    $("#testattachments").click(function (e) {
                        _this.OnTestAttachmentsClick(e);
                    });

                    // Prevent a double click when submitting
                    $("form").submit(function () {
                        $("#next-button").attr("disabled", "disabled");
                    });
                };

                Step4.prototype.configureValidation = function () {
                    // Form validation
                    var validationRules = {
                        AttachmentsFolder: {
                            required: true
                        },
                        AllowedFileTypes: {
                            required: true
                        }
                    };

                    var validation = new Roadkill.Web.Validation();
                    validation.Configure("#step4-form", validationRules);
                };

                Step4.prototype.OnTestAttachmentsClick = function (e) {
                    var _this = this;
                    var jsonData = {
                        "folder": $("#AttachmentsFolder").val()
                    };

                    var url = ROADKILL_INSTALLER_TESTATTACHMENTS_URL;
                    this._wizard.makeAjaxRequest(url, jsonData, function (data) {
                        _this.OnTestAttachmentsSuccess(data);
                    });
                };

                Step4.prototype.OnTestAttachmentsSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._messages.successTitle, this._messages.successMessage);
                        this._wizard.enableContinueButton();
                    } else {
                        this._wizard.showFailure(this._messages.failureTitle, this._messages.failureMessage + "\n" + data.ErrorMessage);
                        this._wizard.disableContinueButton();
                    }
                };
                return Step4;
            })();
            Installer.Step4 = Step4;
        })(Web.Installer || (Web.Installer = {}));
        var Installer = Web.Installer;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step4.js.map
