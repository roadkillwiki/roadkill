var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (Installer) {
            $(document).ready(function () {
            });

            var Page4 = (function () {
                function Page4(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(4);
                }
                return Page4;
            })();
            Installer.Page4 = Page4;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step4.js.map
