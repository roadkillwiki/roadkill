var Roadkill;
(function (Roadkill) {
    /// <reference path="typescript-ref/references.ts" />
    (function (Site) {
        $(document).ready(function () {
            // Event bindings and handlers for the edit page
            var editor = new Site.WysiwygEditor();
            editor.bindEvents();

            EditPage.bindPreviewButton();
        });

        var EditPage = (function () {
            function EditPage() {
            }
            EditPage.initializeTagManager = /**
            Sets up the Bootstrap tag manager
            */
            function (tags) {
                $("#TagsEntry").typeahead({});
                $("#TagsEntry").tagsManager({
                    tagClass: "tm-tag-success",
                    prefilled: tags,
                    preventSubmitOnEnter: true,
                    typeahead: true,
                    typeaheadAjaxMethod: "POST",
                    typeaheadAjaxSource: ROADKILL_TAGAJAXURL,
                    blinkBGColor_1: "#FFFF9C",
                    blinkBGColor_2: "#CDE69C",
                    delimeters: [44, 186, 32, 9],
                    hiddenTagListName: "RawTags",
                    tagCloseIcon: "×",
                    preventSubmitOnEnter: false,
                    validator: function (input) {
                        var isValid = EditPage.isValidTag(input);
                        if (isValid === false) {
                            toastr.error("The following characters are not valid for tags: <br/>" + EditPage._tagBlackList.join(" "));
                        }

                        return isValid;
                    }
                });

                $("#TagsEntry").keydown(function (e) {
                    // Tab adds the tag, but then focuses the textarea
                    var code = e.keyCode || e.which;
                    if (code == "9") {
                        var tag = $("#TagsEntry").val();
                        if (EditPage.isValidTag(tag)) {
                            $("#Content").focus();
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

            EditPage.isValidTag = /**
            Returns false if the tag contains any characters that are blacklisted.
            */
            function (tag) {
                for (var i = 0; i < tag.length; i++) {
                    if ($.inArray(tag[i], EditPage._tagBlackList) > -1) {
                        return false;
                    }
                }

                return true;
            };

            EditPage.bindPreviewButton = function () {
                // Preview modal preview
                $(".previewButton").click(function () {
                    EditPage.showPreview();
                });
            };

            EditPage.showPreview = /**
            Grabs a preview from the server for the wiki markup, and displays it in the
            preview modal (as an iframe)
            */
            function () {
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
                    Site.Dialogs.openFullScreenModal("#previewContainer");
                    $("#previewLoading").hide();
                });
            };
            EditPage._tagBlackList = [
                "#",
                ",",
                ";",
                "/",
                "?",
                ":",
                "@",
                "&",
                "=",
                "{",
                "}",
                "|",
                "\\",
                "^",
                "[",
                "]",
                "`"
            ];
            return EditPage;
        })();
        Site.EditPage = EditPage;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
