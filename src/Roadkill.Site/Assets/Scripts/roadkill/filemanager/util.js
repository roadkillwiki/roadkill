var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (FileManager) {
            var Util = (function () {
                function Util() { }
                Util.IsStringNullOrEmpty = function IsStringNullOrEmpty(text) {
                    return (text === null || text === "" || typeof text === "undefined");
                };
                Util.FormatString = function FormatString(format) {
                    var args = [];
                    for (var _i = 0; _i < (arguments.length - 1); _i++) {
                        args[_i] = arguments[_i + 1];
                    }
                    var result = format;
                    for(var i = 0; i < args.length; i++) {
                        var regex = new RegExp('\\{' + (i) + '\\}', 'gm');
                        result = result.replace(regex, args[i]);
                    }
                    return result;
                };
                return Util;
            })();
            FileManager.Util = Util;            
        })(Site.FileManager || (Site.FileManager = {}));
        var FileManager = Site.FileManager;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
