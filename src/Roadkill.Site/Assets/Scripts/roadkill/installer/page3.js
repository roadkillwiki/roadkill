var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (Installer) {
            $(document).ready(function () {
            });

            var Page3 = (function () {
                function Page3(wizard) {
                    this._wizard = wizard;
                    this._wizard.updateNavigation(3);
                }
                return Page3;
            })();
            Installer.Page3 = Page3;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
