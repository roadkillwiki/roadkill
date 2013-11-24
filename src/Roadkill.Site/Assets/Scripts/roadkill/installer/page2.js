var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (Installer) {
            $(document).ready(function () {
            });

            var Page2 = (function () {
                function Page2(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(2);
                }
                return Page2;
            })();
            Installer.Page2 = Page2;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
