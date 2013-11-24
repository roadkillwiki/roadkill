var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (Installer) {
            $(document).ready(function () {
            });

            var Page5 = (function () {
                function Page5(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(5);
                }
                return Page5;
            })();
            Installer.Page5 = Page5;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=step5.js.map
