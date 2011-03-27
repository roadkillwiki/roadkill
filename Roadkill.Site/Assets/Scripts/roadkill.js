/// <reference path="jquery-1.4.1-vsdoc.js" />
$(document).ready(function ()
{
	$.require("jquery.extensions.min.js");
	$.require("jquery.form-extensions.min.js");
	$.require("jquery.ae.image.resize.min.js");
	$.require("jquery-ui-1.8.core-and-interactions.min.js");
	$.require("jquery-ui-1.8.autocomplete.min.js");
	
	$.require("jquery.timeago.js");
	$.require("jquery.simplemodal.1.4.1.min.js")

	$("#historytable .editedon").timeago();

	// Info icon on each page
	$("#pageinfo-button>a").click(function ()
	{
		showPageInformation();
	});

	formatPreTags();
});


function showPageInformation()
{
	$("#pageinformation").modal();
}

/* 
 *Formats pre tags so that < > and show as encoded.
 */
function formatPreTags()
{
	$("pre").each(function (index)
	{
		var current = $(this);
		var html = current.html();
		html = html.replace(/</g, "&lt;").replace(/>/g, "&gt;");
		current.html(html);
	});
}

/*
 * Sets all links with the .confirm class so they have to click confirm or the link is cancelled.
 */
function bindConfirmDelete()
{
	$("a.confirm").click(function ()
	{
		var button;
		var value;
		var text;
		button = $(this);

		if(!button.hasClass("jqConfirm"))
		{
			value = button.val();
			text = button.text();

			button.val("Confirm");
			button.text("Confirm");
			button.addClass("jqConfirm");

			var handler = function ()
			{
				button.removeClass("jqConfirm");
				button.val(value);
				button.text(text);
				button.unbind("click.jqConfirmHandler");
				return true;
			};
			button.bind("click.jqConfirmHandler", handler);

			setTimeout(function () { handler.call(); }, 3000);

			return false;
		}
	});
}