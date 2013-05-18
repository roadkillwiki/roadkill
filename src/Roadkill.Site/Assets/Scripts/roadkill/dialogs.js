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
            }
            Dialogs.openIFrameModal = function openIFrameModal(html) {
                $.fancybox(html, {
                    openSpeed: "fast",
                    openEffect: "none"
                });
            }
            Dialogs.closeModal = function closeModal() {
                $.fancybox.close(true);
            }
            return Dialogs;
        })();
        Site.Dialogs = Dialogs;        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

