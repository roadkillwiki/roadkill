var Roadkill;
(function (Roadkill) {
    (function (Site) {
        var Dialogs = (function () {
            function Dialogs() {
            }
            Dialogs.alert = function (message) {
                bootbox.animate(false);
                bootbox.alert(message);
            };

            Dialogs.confirm = function (title, resultFunction) {
                bootbox.animate(false);
                bootbox.confirm("<b>" + title + "</b>", resultFunction);
            };

            Dialogs.openModal = function (selector) {
                $(selector).modal("show");
            };

            Dialogs.openFullScreenModal = function (selector) {
                $(selector).modal("show");
                $(selector).css("width", $(window).width() - 110);
                $(selector).css("height", $(window).height() - 110);
                $(window).on("resize", function () {
                    $(selector).css("width", $(window).width() - 110);
                    $(selector).css("height", $(window).height() - 110);
                });
            };

            Dialogs.openMarkupHelpModal = function (html) {
                $("#markup-help-dialog .modal-body-container").html(html);
                $("#markup-help-dialog").modal("show");
            };

            Dialogs.openImageChooserModal = function (html) {
                $("#choose-image-dialog .modal-body-container").html(html);
                $("#choose-image-dialog").modal("show");
            };

            Dialogs.closeImageChooserModal = function () {
                $("#choose-image-dialog").modal("hide");
            };

            Dialogs.closeModal = function (selector) {
                $(selector).modal("hide");
            };
            return Dialogs;
        })();
        Site.Dialogs = Dialogs;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
