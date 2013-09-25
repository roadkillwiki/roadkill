$(document).ready(function ()
{
	$("#pagecontent img").each(function()
	{
		var src = $(this).attr("src");
		$(this).wrap("<a href='" +src+ "' target='_new'></a>");
	});
});