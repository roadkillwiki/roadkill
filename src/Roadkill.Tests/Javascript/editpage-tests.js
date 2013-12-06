test("Commas (,) are invalid in tags", function () {
    var isValid3 = Roadkill.Site.EditPage.isValidTag(",l,l,");
    equal(isValid3, false);
});
test("Hashes (#) are invalid in tags", function () {
    var isValid2 = Roadkill.Site.EditPage.isValidTag("###");
    equal(isValid2, false);
});
test("Semi-colons (;) are invalid in tags", function () {
    var isValid = Roadkill.Site.EditPage.isValidTag(";;;");
    equal(isValid, false);
});
