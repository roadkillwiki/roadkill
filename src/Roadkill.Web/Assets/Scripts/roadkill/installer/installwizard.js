var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/installerconstants.ts" />
        (function (Installer) {
            var InstallWizard = (function () {
                function InstallWizard() {
                    // Set the bottom submit button to submit the form above it
                    $("#bottom-buttons button[type=submit]").click(function () {
                        $("form").submit();
                    });
                }
                InstallWizard.prototype.updateNavigation = function (pageNumber) {
                    $("#trail li:nth-child(" + pageNumber + ")").addClass("selected");
                };

                InstallWizard.prototype.showSuccess = function (title, message) {
                    toastr.success(message, title);
                };

                InstallWizard.prototype.showFailure = function (title, errorMessage) {
                    bootbox.alert("<h2>" + title + "<h2><pre style='max-height:500px;overflow-y:scroll;'>" + errorMessage + "</pre>");
                };

                InstallWizard.prototype.enableContinueButton = function () {
                    $(".continue").removeClass("hidden");
                    $(".continue").show();
                };

                InstallWizard.prototype.disableContinueButton = function () {
                    $(".continue").addClass("hidden");
                    $(".continue").hide();
                };

                InstallWizard.prototype.makeAjaxRequest = function (url, data, successFunction) {
                    var request = $.ajax({
                        type: "GET",
                        url: url,
                        data: data,
                        dataType: "json"
                    });

                    request.done(successFunction);

                    request.fail(function (jqXHR, textStatus, errorThrown) {
                        toastr.error(ROADKILL_INSTALLER_WOOPS + errorThrown);
                    });
                };
                return InstallWizard;
            })();
            Installer.InstallWizard = InstallWizard;
        })(Web.Installer || (Web.Installer = {}));
        var Installer = Web.Installer;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=installwizard.js.map
