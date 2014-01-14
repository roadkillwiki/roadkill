var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/filemanager.references.ts" />
        (function (FileManager) {
            /**
            Event bindings and handlers for the file manager.
            */
            var Setup = (function () {
                function Setup() {
                }
                Setup.configure = function () {
                    this.initializeImagePreview();
                    this.initializeFileUpload();

                    var buttonEvents = new FileManager.ButtonEvents();
                    buttonEvents.bind();

                    var tableEvents = new FileManager.TableEvents();
                    tableEvents.bind();

                    FileManager.TableEvents.update("/");
                };

                Setup.initializeImagePreview = function () {
                    var xOffset = 20;
                    var yOffset = 20;
                    var rowSelector = "table#files tr[data-itemtype=file]";

                    $("#folder-container").on("mouseenter", rowSelector, function (e) {
                        var fileType;
                        fileType = $("td.filetype", this).text();
                        if (fileType.search(/^(jpg|png|gif)$/i) == -1)
                            return;

                        var imgUrl;
                        imgUrl = ROADKILL_ATTACHMENTSPATH + FileManager.TableEvents.getCurrentPath() + "/";
                        imgUrl = imgUrl.replace("//", "/") + $("td.file", this).text();

                        $("body").append("<p id='image-preview'><img src='" + imgUrl + "' alt='Image Preview' /></p>");
                        $("#image-preview").css("top", (e.pageY - xOffset) + "px").css("left", (e.pageX + yOffset) + "px").fadeIn("fast");
                    }).on("mouseleave", rowSelector, function () {
                        $("#image-preview").remove();
                    }).on("mousemove", rowSelector, function (e) {
                        $("#preview").css("top", (e.pageY - xOffset) + "px").css("left", (e.pageX + yOffset) + "px");
                    });
                };

                Setup.initializeFileUpload = function () {
                    $("#fileupload").fileupload({
                        dropZone: $("#folder-container"),
                        pasteZone: $("body"),
                        dataType: "json",
                        progressall: function (e, data) {
                            $("#progress").show();

                            var percentage = (data.loaded / data.total * 100) + "";
                            var progress = parseInt(percentage, 10);
                            $("#progress .bar").css("width", progress + "%");
                        },
                        done: function (e, data) {
                            $("#progress").hide();

                            if (data.result.status == "error") {
                                toastr.error(data.result.message);
                                return;
                            } else {
                                toastr.success(data.result.filename + " uploaded successfully.");

                                FileManager.TableEvents.update("", false);
                                setTimeout(function () {
                                    $("#progress div.bar").css("width", "0%");
                                }, 2000);
                            }
                        }
                    }).bind("fileuploaddrop", function (e, data) {
                        FileManager.TableEvents.update("", false);
                    });
                };
                return Setup;
            })();
            FileManager.Setup = Setup;
        })(Web.FileManager || (Web.FileManager = {}));
        var FileManager = Web.FileManager;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=setup.js.map
