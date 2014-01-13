$(document).ready(function ()
{
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
});