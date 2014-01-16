var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Step5 = (function () {
                function Step5(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(5);
                }
                return Step5;
            })();
            Installer.Step5 = Step5;
        })(Web.Installer || (Web.Installer = {}));
        var Installer = Web.Installer;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
