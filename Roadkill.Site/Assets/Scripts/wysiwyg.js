/*
* jQuery plugin: fieldSelection - v0.1.0 - last change: 2006-12-16
* (c) 2006 Alex Brem <alex@0xab.cd> - http://blog.0xab.cd
*/

(function () {
	var fieldSelection = {
		getSelection: function () {
			var e = this.jquery ? this[0] : this;

			return (
			/* mozilla / dom 3.0 */
                ('selectionStart' in e && function () {
                	var l = e.selectionEnd - e.selectionStart;
                	return {
                		start: e.selectionStart,
                		end: e.selectionEnd,
                		length: l,
                		text: e.value.substr(e.selectionStart, l)
                	};
                })

			/* exploder */
                || (document.selection && function () {
                	e.focus();

                	var r = document.selection.createRange();
                	if (r == null) {
                		return {
                			start: 0,
                			end: e.value.length,
                			length: 0
                		};
                	}

                	var re = e.createTextRange();
                	var rc = re.duplicate();
                	re.moveToBookmark(r.getBookmark());
                	rc.setEndPoint('EndToStart', re);

                	// IE bug - it counts newline as 2 symbols when getting selection coordinates,
                	//  but counts it as one symbol when setting selection
                	var rcLen = rc.text.length,
                        i,
                        rcLenOut = rcLen;
                	for (i = 0; i < rcLen; i++) {
                		if (rc.text.charCodeAt(i) == 13) rcLenOut--;
                	}
                	var rLen = r.text.length,
                        rLenOut = rLen;
                	for (i = 0; i < rLen; i++) {
                		if (r.text.charCodeAt(i) == 13) rLenOut--;
                	}

                	return {
                		start: rcLenOut,
                		end: rcLenOut + rLenOut,
                		length: rLenOut,
                		text: r.text
                	};
                })

			/* browser not supported */
                || function () {
                	return {
                		start: 0,
                		end: e.value.length,
                		length: 0
                	};
                }

            )();

		},

		setSelection: function (start, end) {
			var e = document.getElementById($(this).attr('id')); // I don't know why... but $(this) don't want to work today :-/
			if (!e) {
				return $(this);
			} else if (e.setSelectionRange) { /* WebKit */
				e.focus(); e.setSelectionRange(start, end);
			} else if (e.createTextRange) { /* IE */
				var range = e.createTextRange();
				range.collapse(true);
				range.moveEnd('character', end);
				range.moveStart('character', start);
				range.select();
			} else if (e.selectionStart) { /* Others */
				e.selectionStart = start;
				e.selectionEnd = end;
			}

			return $(this);
		},

		replaceSelection: function () {
			var e = this.jquery ? this[0] : this;
			var text = arguments[0] || '';

			return (
			/* mozilla / dom 3.0 */
                ('selectionStart' in e && function () {
                	e.value = e.value.substr(0, e.selectionStart) + text + e.value.substr(e.selectionEnd, e.value.length);
                	return this;
                })

			/* exploder */
                || (document.selection && function () {
                	e.focus();
                	document.selection.createRange().text = text;
                	return this;
                })

			/* browser not supported */
                || function () {
                	e.value += text;
                	return this;
                }
            )();
		}
	};

	jQuery.each(fieldSelection, function (i) { jQuery.fn[i] = this; });

})();

var _tags;

$(document).ready(function () {
	setTimeout("loadTags();", 2000);
	initTagIt();
	bindToolbar();

	$("#Content").keyup(function () {
		$("#previewContainer").hide();
	});
	$(".previewButton").click(function () {
		showPreview();
	});
	$(".cancelButton").click(function () {
		history.go(-1);
	});
});

function loadTags()
{
	$.get(_tagAjaxUrl, function (data)
	{
		_tags = eval(data);
		initTagIt();
	});
}

function initTagIt() {
	$("#mytags").tagit({
		availableTags: _tags,
		singleField: true,
		singleFieldNode: $("#Tags"),
		singleFieldDelimiter: ";"
	});
}

function bindToolbar() {
	// TODO: take these stylings from a settings file:
	// Make a JS file that is a url that writes them

	$(".wysiwyg-bold").click(function () {
		addStyling("**");
	});
	$(".wysiwyg-italic").click(function () {
		addStyling("//");
	});
	$(".wysiwyg-underline").click(function () {
		addStyling("__");
	});
	$(".wysiwyg-h1").click(function () {
		addHeading("=");
	});
	$(".wysiwyg-h2").click(function () {
		addHeading("==");
	});
	$(".wysiwyg-h3").click(function () {
		addHeading("===");
	});
	$(".wysiwyg-h4").click(function () {
		addHeading("====");
	});
	$(".wysiwyg-h5").click(function () {
		addHeading("=====");
	});
	$(".wysiwyg-bullets").click(function () {
		addListItem("#");
	});
	$(".wysiwyg-numbers").click(function () {
		addListItem("*");
	});
}

function showPreview()
{
	$("#previewLoading").show();
	var text = $("#Content").val();

	$.ajax({
		type: "POST",
		url: "/Roadkill.Site/Page/GetPreview",
		data: {"id": text },
		success: function (htmlResult) {
			$("#preview").html(htmlResult);
			$("#previewContainer").show();			
			$("#previewLoading").hide();
		}
	});
}

function addStyling(styleCode) 
{
	var range  = $("#Content").getSelection();

	if (range !== null)
	{
		var text = $("#Content").val();
		if (text.substr(range.start - 2, 2) !== styleCode && range.text.substr(0, 2) !== styleCode)
		{
			$("#Content").replaceSelection(styleCode + range.text + styleCode);
			$("#Content").setSelection(range.end + 2, range.end + 2);
		}
		else
		{
			$("#Content").setSelection(range.end,range.end);
		}
		
	}
}

function addHeading(styleCode) {
	var range = $("#Content").getSelection();

	if (range !== null) 
	{
		var text = range.text;
		if (range.text === "")
			text = "Your heading";

		$("#Content").replaceSelection("\n" + styleCode + text + styleCode + "\n");
		$("#Content").setSelection(range.end, range.end);
	}
}

function addListItem(styleCode) 
{
	var range = $("#Content").getSelection();

	if (range !== null) 
	{
		var val = $("#Content").val();
		var start = range.start;
		if (start > 0)
			start -= 1;

		var lastChar = val.substr(start, 1);
		var nextChar = val.substr(range.start, 1);

		if (nextChar === styleCode)
		{
			$("#Content").setSelection(range.end + 2, range.end + 2);
			return;
		}

		if (lastChar == "\n" || lastChar == "")
		{
			$("#Content").replaceSelection(range.text + styleCode +" ");
			$("#Content").setSelection(range.end + 2, range.end + 2);
		}
		else
		{
			$("#Content").replaceSelection(range.text + "\n" +styleCode+ " ");
			$("#Content").setSelection(range.end + 3, range.end + 3);
		}
	}
}

