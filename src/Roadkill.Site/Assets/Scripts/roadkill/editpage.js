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
            EditPage.initializeTagManager = function initializeTagManager(tags) {
                $("#TagsEntry").typeahead({
                });
                $("#TagsEntry").tagsManager({
                    prefilled: tags,
                    preventSubmitOnEnter: true,
                    typeahead: true,
                    typeaheadAjaxSource: ROADKILL_TAGAJAXURL,
                    blinkBGColor_1: "#FFFF9C",
                    blinkBGColor_2: "#CDE69C",
                    delimeters: [
                        59, 
                        186, 
                        32
                    ],
                    hiddenTagListName: "RawTags"
                });
            };
            EditPage.bindPreviewButton = function bindPreviewButton() {
                $(".previewButton").click(function () {
                    EditPage.showPreview();
                });
            };
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
                    $("#preview").show();
                    Site.Dialogs.openModal("#previewContainer");
                    $("#previewLoading").hide();
                });
            };
            return EditPage;
        })();
        Site.EditPage = EditPage;        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
