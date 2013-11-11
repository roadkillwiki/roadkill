var Roadkill;
(function (Roadkill) {
    (function (Site) {
        $(document).ready(function () {
            Setup.configureBinds();

            toastr.options = {
                "debug": false,
                "positionClass": "toast-top-right",
                "onclick": null,
                "fadeIn": 300,
                "fadeOut": 1000,
                "timeOut": 5000,
                "extendedTimeOut": 1000
            };
        });

        var Setup = (function () {
            function Setup() {
            }
            Setup.configureBinds = function () {
                this.bindInfoButton();
                this.bindTimeAgo();
                this.bindTocLinks();
            };

            Setup.bindTimeAgo = function () {
                $("#historytable .editedon").timeago();
            };

            Setup.bindInfoButton = function () {
                $("#pageinfo-button").click(function () {
                    Site.Dialogs.openModal("#pageinformation");
                });
            };

            Setup.bindTocLinks = function () {
                $("a.toc-showhide").click(function () {
                    if ($(this).text() == "hide") {
                        $(this).text("show");
                    } else {
                        $(this).text("hide");
                    }

                    $(this).parent().next().toggle();
                });
            };

            Setup.bindConfirmDelete = function () {
                $("a.confirm").click(function () {
                    var button;
                    var value;
                    var text;
                    button = $(this);

                    if (!button.hasClass("jqConfirm")) {
                        value = button.val();
                        text = button.text();

                        button.val(ROADKILL_LINK_CONFIRM);
                        button.text(ROADKILL_LINK_CONFIRM);
                        button.addClass("jqConfirm");

                        var handler = function () {
                            button.removeClass("jqConfirm");
                            button.val(value);
                            button.text(text);
                            button.unbind("click.jqConfirmHandler");
                            return true;
                        };
                        button.bind("click.jqConfirmHandler", handler);

                        setTimeout(function () {
                            handler();
                        }, 3000);

                        return false;
                    }
                });
            };
            return Setup;
        })();
        Site.Setup = Setup;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
