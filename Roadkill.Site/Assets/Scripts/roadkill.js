$(document).ready(function () {
	$("#pagecontent img").aeImageResize({ height: 400, width: 400 });
});

function setFile(path, filename) {
	$("#wmddialoginput", window.opener.document).val(path + "/" + filename);
	window.close();
}