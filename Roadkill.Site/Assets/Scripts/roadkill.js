$(document).ready(function ()
{
	$.require("jquery.extensions.min.js");
	$.require("jquery.form-extensions.min.js");
	$.require("jquery.ae.image.resize.min.js");
	$.require("jquery-ui-1.8.core-and-interactions.min.js");
	$.require("jquery-ui-1.8.autocomplete.min.js");
	$.require("tag-it.js");
	$.require("jquery.timeago.js");
	$.require("jquery.simplemodal.1.4.1.min.js")

	$("#pagecontent img").aeImageResize({ height: 400, width: 400 });
	$("#historytable .editedon").timeago();
	$("#pageinfo-button>a").click(function ()
	{
		showPageInformation();
	});
});

function setFile(path, filename) {
	$("#wmddialoginput", window.opener.document).val(path + "/" + filename);
	window.close();
}

function showPageInformation()
{
	$("#pageinformation").modal();
}


/**
* require is used for on demand loading of JavaScript
* 
* require r1 // 2008.02.05 // jQuery 1.2.2
* 
* // basic usage (just like .accordion) 
* $.require("comp1.js");
*
* @param  jsFiles string array or string holding the js file names to load
* @param  params object holding parameter like browserType, callback, cache
* @return The jQuery object
* @author Manish Shanker (altered for Roadkill)
*/
(function ($) {
	$.require = function (jsFiles, params) {

		var params = params || {};
		var bType = params.browserType === false ? false : true;

		if (!bType) {
			return $;
		}

		var cBack = params.callBack || function () { };
		var eCache = params.cache === false ? false : true;

		if (!$.require.loadedLib) $.require.loadedLib = {};

		if (!$.scriptPath) {
			$.scriptPath = ROADKILL_CORESCRIPTPATH;
		}
		if (typeof jsFiles === "string") {
			jsFiles = new Array(jsFiles);
		}
		for (var n = 0; n < jsFiles.length; n++) {
			if (!$.require.loadedLib[jsFiles[n]]) {
				$.ajax({
					type: "GET",
					url: $.scriptPath + jsFiles[n],
					success: cBack,
					dataType: "script",
					cache: eCache,
					async: false
				});
				$.require.loadedLib[jsFiles[n]] = true;
			}
		}
		//console.dir($.require.loadedLib);

		return $;
	};
})(jQuery);