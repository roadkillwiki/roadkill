/// <reference path="jquery.require.js" />
/// <reference path="jquery-1.8.0-vsdoc.js" />

/**
Event bindings and handlers for all pages.
*/
$(document).ready(function ()
{
	// Friendly times for the history tables
	$("#historytable .editedon").timeago();

	// Bind the info icon on each page
	$("#pageinfo-button").click(function () {
		openModal("#pageinformation");
	});

	// The show/hide for table of contents
	$("a.toc-showhide").click(function ()
	{
		if ($(this).text() == "hide")
		{
			$(this).text("show");
		}
		else
		{
			$(this).text("hide");
		}

		$(this).parent().next().toggle();
	});

	formatPreTags();
});

/**
Sets all links with the .confirm class so they have to click confirm to delete or the link is cancelled.
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

			button.val(ROADKILL_LINK_CONFIRM);
			button.text(ROADKILL_LINK_CONFIRM);
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

/**
Formats pre tags for Creole/Mediawiki so that < > are encoded and display.
*/
function formatPreTags()
{
	if (ROADKILL_MARKUPTYPE !== "Markdown")
	{
		$("pre").each(function (index)
		{
			var current = $(this);
			var html = current.html();
			html = html.replace(/</g, "&lt;").replace(/>/g, "&gt;");
			current.html(html);
		});
	}
}

function openModal(selector, params)
{
	if (typeof params !== "undefined")
	{
		params.openSpeed = 150;
		params.closeSpeed = 150;
	}
	else
	{
		params = { openSpeed : 150, closeSpeed: 150 };
	}

	$.fancybox($(selector), params);
}

function openIframeModal(html)
{
	$.fancybox(html, { openSpeed: "fast", openEffect : "none" });
}

function closeModal()
{
	$.fancybox.close(true);
}