var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (Installer) {
            $(document).ready(function () {
            });

            var Step4 = (function () {
                function Step4(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(4);
                }
                return Step4;
            })();
            Installer.Step4 = Step4;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step4.js.map
