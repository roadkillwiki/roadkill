var Roadkill;
(function (Roadkill) {
    (function (Site) {
        $(document).ready(function () {
            var editor = new Site.WysiwygEditor();
            editor.bindEvents();
            EditPage.bindPreviewButton();
        });
        var EditPage = (function () {
            function EditPage() { }
            EditPage._tagBlackList = [
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
            EditPage.initializeTagManager = function initializeTagManager(tags) {
                $("#TagsEntry").typeahead({
                });
                $("#TagsEntry").tagsManager({
                    tagClass: "tm-tag-success",
                    prefilled: tags,
                    preventSubmitOnEnter: true,
                    typeahead: true,
                    typeaheadAjaxMethod: "POST",
                    typeaheadAjaxSource: ROADKILL_TAGAJAXURL,
                    blinkBGColor_1: "#FFFF9C",
                    blinkBGColor_2: "#CDE69C",
                    delimeters: [
                        44, 
                        186, 
                        32
                    ],
                    hiddenTagListName: "RawTags",
                    tagCloseIcon: "×",
                    preventSubmitOnEnter: false,
                    validator: function (input) {
                        for(var i = 0; i < input.length; i++) {
                            if($.inArray(input[i], EditPage._tagBlackList) > -1) {
                                toastr.error("The following characters are not valid for tags: <br/>" + EditPage._tagBlackList.join(" "));
                                return false;
                            }
                        }
                        return true;
                    }
                });
            }
            EditPage.bindPreviewButton = function bindPreviewButton() {
                $(".previewButton").click(function () {
                    EditPage.showPreview();
                });
            }
            EditPage.showPreview = function showPreview() {
                $("#previewLoading").show();
                var text = $("#Content").val();
                var request = $.ajax({
                    type: "POST",
                    url: ROADKILL_PREVIEWURL,
                    data: {
                        "id": text
                    },
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
            }
            return EditPage;
        })();
        Site.EditPage = EditPage;        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

