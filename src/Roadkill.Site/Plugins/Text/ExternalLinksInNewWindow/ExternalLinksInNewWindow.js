$(document).ready(function ()
{
	$("#pagecontent a.external-link").each(function ()
	{
		$(this).attr("target","_blank");
	});
})