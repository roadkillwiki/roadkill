var Roadkill;
(function (Roadkill) {
    (function (Site) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var Wizard = (function () {
                function Wizard() {
                }
                Wizard.prototype.updateNavigation = function (pageNumber) {
                    $("#trail li:nth-child(" + pageNumber + ")").addClass("selected");
                };

                Wizard.prototype.showSuccess = function (message) {
                    toastr.success(message);
                };

                Wizard.prototype.showFailure = function (message, errorMessage) {
                    toastr.failure(message + "<br/>" + errorMessage);
                };

                Wizard.prototype.enableContinueButton = function () {
                    $(".continue").show();
                };

                Wizard.prototype.disableContinueButton = function () {
                    $(".continue").hide();
                };

                Wizard.prototype.makeAjaxRequest = function (url, data, errorMessage, successFunction) {
                    var request = $.ajax({
                        type: "GET",
                        url: url,
                        data: data,
                        dataType: "json"
                    });

                    request.done(successFunction);

                    request.fail(function (jqXHR, textStatus, errorThrown) {
                        toastr.error(errorMessage + errorThrown);
                    });
                };
                return Wizard;
            })();
            Installer.Wizard = Wizard;
        })(Site.Installer || (Site.Installer = {}));
        var Installer = Site.Installer;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=wizard.js.map
