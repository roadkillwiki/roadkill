var Roadkill;
(function (Roadkill) {
    /// <reference path="../typescript-ref/references.ts" />
    (function (Web) {
        var EditPage = (function () {
            function EditPage(tags) {
                this._timeout = null;
                this._tagBlackList = [
                    "#", ",", ";", "/", "?", ":", "@", "&", "=", "{", "}", "|", "\\", "^", "[", "]", "`"
                ];
                // Setup tagmanager
                this.initializeTagManager(tags);

                // Bind all the button events
                var editor = new Web.WysiwygEditor();
                editor.bindEvents();

                // Setup the help popovers for the buttons
                $("#wysiwyg-toolbar button").popover({ trigger: "hover", html: false, delay: { show: 250, hide: 100 } });

                // Set the preview pane to auto-update
                this.bindPreview();

                // Set the preview pane toggle button
                this.bindPreviewToggleButton();

                // Form validation
                var validationRules = {
                    Title: {
                        required: true
                    }
                };
                var validation = new Roadkill.Web.Validation();
                validation.Configure("#editpage-form", validationRules);
            }
            /**
            Sets up the Bootstrap tag manager
            */
            EditPage.prototype.initializeTagManager = function (tags) {
                var _this = this;
                // Use jQuery UI autocomplete, as typeahead is currently broken for BS3
                $("#TagsEntry").autocomplete({
                    source: tags
                });

                $("#TagsEntry").tagsManager({
                    tagClass: "tm-tag-success",
                    blinkBGColor_1: "#FFFF9C",
                    blinkBGColor_2: "#CDE69C",
                    delimeters: [44, 186, 32, 9],
                    output: "#RawTags",
                    preventSubmitOnEnter: false,
                    validator: function (input) {
                        var isValid = _this.isValidTag(input);
                        if (isValid === false) {
                            toastr.error("The following characters are not valid for tags: <br/>" + _this._tagBlackList.join(" "));
                        }

                        return isValid;
                    }
                });

                $("#TagsEntry").keydown(function (e) {
                    // Tab adds the tag, but then focuses the toolbar (the next tab index)
                    var code = e.keyCode || e.which;
                    if (code == "9") {
                        var tag = $("#TagsEntry").val();
                        if (_this.isValidTag(tag)) {
                            if ($("#IsLocked").length == 0)
                                $(".wysiwyg-bold").focus();
                            else
                                $("#IsLocked").focus();
                        }
                        return false;
                    }

                    return true;
                });

                $("#TagsEntry").blur(function (e) {
                    // Push the tag when focus is lost, e.g. Save is pressed
                    $("#TagsEntry").tagsManager("pushTag", $("#TagsEntry").val());

                    // Fix the tag's styles from being blank
                    $(".tm-tag-remove").each(function () {
                        $(this).html("&times;");
                    });
                    $(".tm-tag").each(function () {
                        $(this).addClass("tm-tag-success");
                        $(this).addClass("tm-success");
                    });
                });
            };

            /**
            Returns false if the tag contains any characters that are blacklisted.
            */
            EditPage.prototype.isValidTag = function (tag) {
                for (var i = 0; i < tag.length; i++) {
                    if ($.inArray(tag[i], this._tagBlackList) > -1) {
                        return false;
                    }
                }

                return true;
            };

            EditPage.prototype.bindPreview = function () {
                var _this = this;
                this.resizePreviewPane();
                EditPage.updatePreviewPane();

                $(document).on("resize", function () {
                    _this.resizePreviewPane();
                });

                // Keydown fires the preview after 1/100th second, but each keypress resets this.
                $("#Content").on("keydown", function () {
                    if (_this._timeout !== null) {
                        clearTimeout(_this._timeout);
                        _this._timeout = null;
                    }

                    _this._timeout = setTimeout(EditPage.updatePreviewPane, 100);
                });
            };

            EditPage.prototype.bindPreviewToggleButton = function () {
                $("#preview-toggle").click(function () {
                    // Switch the bootstrap classes so the form area fills or collapses
                    var panelContainer = $("#previewpanel-container");

                    if (panelContainer.is(":visible")) {
                        // Hide the preview
                        $("#preview-toggle span").removeClass("glyphicon-chevron-right").addClass("glyphicon-chevron-left");

                        $("#editpage-form-container").removeClass("col-lg-6").addClass("col-lg-12");

                        $("#previewpanel-container").removeClass("col-lg-6");
                    } else {
                        // Show the preview
                        $("#preview-toggle span").removeClass("glyphicon-chevron-left").addClass("glyphicon-chevron-right");

                        $("#editpage-form-container").removeClass("col-lg-12").addClass("col-lg-6");

                        $("#previewpanel-container").addClass("col-lg-6");
                    }

                    panelContainer.toggle();
                    return false;
                });
            };

            EditPage.prototype.resizePreviewPane = function () {
                // Height fix for CSS heights sucking
                $("#Content").height($("#container").height());

                var previewTitleHeight = $("#preview-heading").outerHeight(true);
                var buttonsHeight = $("#editpage-button-container").outerHeight(true);
                var scrollbarHeight = 36;
                var formHeight = $("#editpage-form-container").outerHeight(true) - (buttonsHeight + scrollbarHeight + previewTitleHeight);

                $("#preview-wrapper").height(formHeight);
            };

            /**
            Grabs a preview from the server for the wiki markup, and displays it in the preview pane.
            */
            EditPage.updatePreviewPane = function () {
                $("#previewLoading").show();
                var text = $("#Content").val();

                var request = $.ajax({
                    type: "POST",
                    url: ROADKILL_PREVIEWURL,
                    data: { "id": text },
                    cache: false,
                    dataType: "text"
                });

                request.done(function (htmlResult) {
                    $("#preview").html(htmlResult);
                });

                request.fail(function (jqXHR, textStatus, errorThrown) {
                    $("#preview").html("<span style='color:red'>An error occurred with the preview: " + errorThrown + "</span>");
                });

                request.always(function () {
                    $("#previewLoading").show();
                    $("#previewLoading").hide();
                });
            };
            return EditPage;
        })();
        Web.EditPage = EditPage;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=editpage.js.map
