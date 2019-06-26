/// <reference path="../typescript-ref/installerconstants.ts" />
var Roadkill;
(function (Roadkill) {
    var Web;
    (function (Web) {
        var Installer;
        (function (Installer) {
            var Step1Messages = /** @class */ (function () {
                function Step1Messages() {
                }
                return Step1Messages;
            }());
            Installer.Step1Messages = Step1Messages;
            var Step1 = /** @class */ (function () {
                function Step1(wizard, messages) {
                    this._wizard = wizard;
                    this._messages = messages;
                    this._wizard.updateNavigation(1);
                }
                Step1.prototype.bindButtons = function () {
                    var _this = this;
                    $("#testwebconfig").click(function (e) {
                        _this.OnTestWebConfigClick(e);
                    });
                };
                Step1.prototype.OnTestWebConfigClick = function (e) {
                    var _this = this;
                    var url = ROADKILL_INSTALLER_TESTWEBCONFIG_URL;
                    this._wizard.makeAjaxRequest(url, {}, function (data) { _this.OnTestWebConfigSuccess(data); });
                };
                Step1.prototype.OnTestWebConfigSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._messages.successTitle, this._messages.successMessage);
                        this._wizard.enableContinueButton();
                    }
                    else {
                        this._wizard.showFailure(this._messages.failureTitle, this._messages.failureMessage + "\n" + data.ErrorMessage);
                        this._wizard.disableContinueButton();
                    }
                };
                return Step1;
            }());
            Installer.Step1 = Step1;
        })(Installer = Web.Installer || (Web.Installer = {}));
    })(Web = Roadkill.Web || (Roadkill.Web = {}));
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step1.js.map