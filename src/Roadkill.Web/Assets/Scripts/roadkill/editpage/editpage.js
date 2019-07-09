/// <reference path="../typescript-ref/references.ts" />
var Roadkill;
(function (Roadkill) {
    var Web;
    (function (Web) {
        var EditPage = /** @class */ (function () {
            function EditPage(tags, tuiEditor, baseURL) {
                var _this = this;
                this._timeout = null;
                this._tagBlackList = [
                    "#", ",", ";", "/", "?", ":", "@", "&", "=", "{", "}", "|", "\\", "^", "[", "]", "`"
                ];
                this._tuiEditor = null;
                this._baseURL = '';
                /**
                A Custom markdownIt renderer rule for roadkill code fences
                */
                this.rk_fence_renderer = function (tokens, idx, options, env, slf) {
                    var token = tokens[idx], info = token.info ? _this._tuiEditor.markdownitHighlight.utils.unescapeAll(token.info).trim() : '', langName = '', highlighted, i, tmpAttrs, tmpToken;
                    if (info) {
                        langName = info.split(/\s+/g)[0];
                    }
                    highlighted = _this._tuiEditor.markdownitHighlight.utils.escapeHtml(token.content);
                    if (highlighted.indexOf('<pre') === 0) {
                        return highlighted + '\n';
                    }
                    // If language exists
                    if (info) {
                        // Load the brush
                        return '<pre class="brush: ' + langName + '">'
                            + highlighted
                            + '</pre>\n';
                    }
                    // Language doesn't exist, just copy the attrs
                    return '<pre><code' + slf.renderAttrs(token) + '>'
                        + highlighted
                        + '</code></pre>\n';
                };
                /**
                Custom markdownIt image renderer rule for roadkill-style urls in img src values
                */
                this.rk_image_renderer = function (tokens, idx, options, env, slf) {
                    var token = tokens[idx];
                    token.attrs[token.attrIndex('alt')][1] =
                        slf.renderInlineAsText(token.children, options, env);
                    var src = token.attrs[token.attrIndex('src')][1];
                    token.attrs[token.attrIndex('src')][1] = src.replace(/(.+)/, _this._baseURL + 'Attachments$1');
                    return slf.renderToken(tokens, idx, options);
                };
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
                this._tuiEditor = tuiEditor;
                this._baseURL = baseURL;
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
                        $("#preview-toggle span")
                            .removeClass("glyphicon-chevron-right")
                            .addClass("glyphicon-chevron-left");
                        $("#editpage-form-container")
                            .removeClass("col-lg-6")
                            .addClass("col-lg-12");
                        $("#previewpanel-container")
                            .removeClass("col-lg-6");
                    }
                    else {
                        // Show the preview
                        $("#preview-toggle span")
                            .removeClass("glyphicon-chevron-left")
                            .addClass("glyphicon-chevron-right");
                        $("#editpage-form-container")
                            .removeClass("col-lg-12")
                            .addClass("col-lg-6");
                        $("#previewpanel-container")
                            .addClass("col-lg-6");
                    }
                    panelContainer.toggle();
                    return false;
                });
            };
            EditPage.prototype.resizePreviewPane = function () {
                // Height fix for CSS heights sucking
                $("#Content").height($("#container").height());
                var previewTitleHeight = $("#preview-heading").outerHeight(true); // true to include margin
                var buttonsHeight = $("#editpage-button-container").outerHeight(true);
                var scrollbarHeight = 36; // top and bottom scrollbars
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
            /**
            Roadkill-style code block (fence) markdownIt tokenizer rule
            */
            EditPage.prototype.rk_fence = function (state, startLine, endLine, silent) {
                var optionMarker = '[';
                var pos = state.bMarks[startLine] + state.tShift[startLine];
                var max = state.eMarks[startLine];
                var haveEndMarker = false;
                if (state.sCount[startLine] - state.blkIndent >= 4)
                    return false;
                if (pos + 3 > max)
                    return false;
                var marker = state.src.charCodeAt(pos);
                if (marker !== optionMarker.charCodeAt(0))
                    return false;
                var mem = pos;
                pos = state.skipChars(pos, marker);
                var len = pos - mem;
                if (len < 3)
                    return false;
                // KW If we're interrupting an element here, don't bother pushing the token
                if (silent) {
                    return true;
                }
                var markup = state.src.slice(mem, pos);
                var params = state.src.slice(pos, max).replace(/code.*lang=([^|]*)\|/, '$1');
                // Skip jumbotron blocks
                if (/.*jumbotron.*/.test(params)) {
                    return false;
                }
                if (params.indexOf(String.fromCharCode(marker)) >= 0)
                    return false;
                // Search for end of block
                var nextLine = startLine;
                marker = 93; /* ] */
                for (;;) {
                    nextLine++;
                    if (nextLine >= endLine)
                        break;
                    pos = mem = state.bMarks[nextLine] + state.tShift[nextLine];
                    max = state.eMarks[nextLine];
                    if (pos < max && state.sCount[nextLine] < state.blkIndent)
                        break;
                    if (state.src.charCodeAt(pos) !== marker)
                        continue;
                    if (state.sCount[nextLine] - state.blkIndent >= 4)
                        continue;
                    pos = state.skipChars(pos, marker);
                    if (pos - mem < len)
                        continue;
                    pos = state.skipSpaces(pos);
                    if (pos < max)
                        continue;
                    haveEndMarker = true;
                    break;
                }
                len = state.sCount[startLine];
                state.line = nextLine + (haveEndMarker ? 1 : 0);
                var token = state.push('fence', 'code', 0);
                token.info = params;
                token.content = state.getLines(startLine + 1, nextLine, len, true);
                token.markup = markup;
                token.map = [startLine, state.line];
                return true;
            };
            /**
            Transform CommonMark to standard markdown
            */
            EditPage.prototype.toStdMarkdown = function (commonMarkdown) {
                return commonMarkdown
                    .replace(/&#/g, '&amp;#'); // TUI entity escape
            };
            return EditPage;
        }());
        Web.EditPage = EditPage;
    })(Web = Roadkill.Web || (Roadkill.Web = {}));
})(Roadkill || (Roadkill = {}));
