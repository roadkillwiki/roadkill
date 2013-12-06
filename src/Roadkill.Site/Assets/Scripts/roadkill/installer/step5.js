var Roadkill;
(function (Roadkill) {
    (function (Site) {
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
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step5.js.map
