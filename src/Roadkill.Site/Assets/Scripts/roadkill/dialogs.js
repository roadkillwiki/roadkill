var Roadkill;
(function (Roadkill) {
    (function (Site) {
        var Dialogs = (function () {
            function Dialogs() { }
            Dialogs.openModal = function openModal(selector, params) {
                if(typeof params !== "undefined") {
                    params.openSpeed = 150;
                    params.closeSpeed = 150;
                } else {
                    params = {
                        openSpeed: 150,
                        closeSpeed: 150
                    };
                }
                $.fancybox($(selector), params);
            };
            Dialogs.openFullScreenModal = function openFullScreenModal(selector) {
                $(selector).modal("show");
                $(selector).css("width", $(window).width() - 110);
                $(selector).css("height", $(window).height() - 110);
                $(window).on("resize", function () {
                    $(selector).css("width", $(window).width() - 110);
                    $(selector).css("height", $(window).height() - 110);
                });
            };
            Dialogs.openIFrameModal = function openIFrameModal(html) {
                $("#iframe-dialog .modal-body").html(html);
                $("#iframe-dialog").modal("show");
            };
            Dialogs.closeModal = function closeModal() {
                $.fancybox.close(true);
            };
            Dialogs.closeModal2 = function closeModal2(selector) {
                $(selector).modal("hide");
            };
            return Dialogs;
        })();
        Site.Dialogs = Dialogs;        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
