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

            Dialogs.openModal = function (selector, params) {
                if (typeof params !== "undefined") {
                    params.openSpeed = 150;
                    params.closeSpeed = 150;
                } else {
                    params = { openSpeed: 150, closeSpeed: 150 };
                }

                $.fancybox($(selector), params);
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

            Dialogs.openIFrameModal = function (html) {
                $("#iframe-dialog .modal-body").html(html);
                $("#iframe-dialog").modal("show");
            };

            Dialogs.closeModal = function () {
                $.fancybox.close(true);
            };

            Dialogs.closeModal2 = function (selector) {
                $(selector).modal("hide");
            };
            return Dialogs;
        })();
        Site.Dialogs = Dialogs;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
