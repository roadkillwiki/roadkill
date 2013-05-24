var Roadkill;
(function (Roadkill) {
    (function (Site) {
        $(document).ready(function () {
            Setup.configureBinds();
        });
        var Setup = (function () {
            function Setup() { }
            Setup.configureBinds = function configureBinds() {
                this.bindInfoButton();
                this.bindTimeAgo();
                this.bindTocLinks();
            }
            Setup.bindTimeAgo = function bindTimeAgo() {
                $("#historytable .editedon").timeago();
            }
            Setup.bindInfoButton = function bindInfoButton() {
                $("#pageinfo-button").click(function () {
                    Site.Dialogs.openModal("#pageinformation");
                });
            }
            Setup.bindTocLinks = function bindTocLinks() {
                $("a.toc-showhide").click(function () {
                    if($(this).text() == "hide") {
                        $(this).text("show");
                    } else {
                        $(this).text("hide");
                    }
                    $(this).parent().next().toggle();
                });
            }
            Setup.resizeImage = function resizeImage(img, maxWidth, maxHeight) {
                if(maxWidth < 1) {
                    maxWidth = 400;
                }
                if(maxHeight < 1) {
                    maxHeight = 400;
                }
                var ratio = 0;
                var width = $(img).width();
                var height = $(img).height();
                if(width > maxWidth) {
                    ratio = maxWidth / width;
                    width = width * ratio;
                    height = height * ratio;
                    $(img).css("width", width);
                    $(img).css("height", height);
                }
                if(height > maxHeight) {
                    ratio = maxHeight / height;
                    $(img).css("width", width * ratio);
                    $(img).css("height", height * ratio);
                }
            }
            Setup.bindConfirmDelete = function bindConfirmDelete() {
                $("a.confirm").click(function () {
                    var button;
                    var value;
                    var text;
                    button = $(this);
                    if(!button.hasClass("jqConfirm")) {
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
            }
            return Setup;
        })();
        Site.Setup = Setup;        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

