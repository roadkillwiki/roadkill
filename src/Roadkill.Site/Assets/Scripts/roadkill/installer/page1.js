var Roadkill;
(function (Roadkill) {
    (function (Site) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Page1 = (function () {
                function Page1(wizard, successMessage, failureMessage) {
                    this._wizard = wizard;
                    this._successMessage = successMessage;
                    this._failureMessage = failureMessage;

                    this._wizard.updateNavigation(1);
                }
                Page1.prototype.bindButtons = function () {
                    var _this = this;
                    $("#testwebconfig").click(function (e) {
                        _this.OnTestWebConfigClick(e);
                    });
                };

                Page1.prototype.OnTestWebConfigClick = function (e) {
                    // TODO-translation
                    var url = ROADKILL_INSTALLER_TESTWEBCONFIG_URL;
                    this._wizard.makeAjaxRequest(url, {}, "Woops, something went wrong", this.OnTestWebConfigSuccess);
                };

                Page1.prototype.OnTestWebConfigSuccess = function (data) {
                    if (data.Success) {
                        this._wizard.showSuccess(this._successMessage);
                        this._wizard.enableContinueButton();
                    } else {
                        this._wizard.showFailure(this._failureMessage, data.ErrorMessage);
                        this._wizard.disableContinueButton();
                    }
                };
                return Page1;
            })();
            Installer.Page1 = Page1;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
