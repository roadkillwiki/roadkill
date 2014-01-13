// *** These Javascript tests are experimental and not used by the build server ***

// 
// Guide to setting up Chutzpah: http://joeriks.com/2012/10/06/testing-typescript-with-chutzpah/
// When running in the Visual Studio Web Essentials context menu plugin, use the "open in browser" 
// option as the run JS tests doesn't appear to work.
// 

// --- These 4 references are needed by the Roadkill scripts to run correctly
/// <Chutzpah_reference path="../../Roadkill.Web/Assets/Scripts/jquery/jquery-1.8.0.js" />
/// <Chutzpah_reference path="../../Roadkill.Web/Assets/Scripts/jquery/jquery.timeago.js" />
/// <Chutzpah_reference path="../../Roadkill.Web/Assets/Scripts/toastr.js" />
/// <Chutzpah_reference path="../../Roadkill.Web/Assets/Scripts/roadkill/editpage.js" />

// --- These are for intellisense only. QUnit.ts is copied from the (nuget) packages folder
/// <reference path="qunit.d.ts" />
/// <reference path="../../Roadkill.Web/Assets/Scripts/roadkill/editpage/editpage.ts" />

//test("Commas (,) are invalid in tags", () =>
//{
//	var isValid3: Boolean = Roadkill.Web.EditPage.isValidTag(",l,l,");
//	equal(isValid3, false);
//});

//test("Hashes (#) are invalid in tags", () =>
//{
//	var isValid2: Boolean = Roadkill.Web.EditPage.isValidTag("###");
//	equal(isValid2, false);
//});

//test("Semi-colons (;) are invalid in tags", () =>
//{
//	var isValid: Boolean = Roadkill.Web.EditPage.isValidTag(";;;");
//	equal(isValid, false);
//});