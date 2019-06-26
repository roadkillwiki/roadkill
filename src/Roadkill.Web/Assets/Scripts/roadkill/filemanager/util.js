/// <reference path="../typescript-ref/filemanager.references.ts" />
var Roadkill;
(function (Roadkill) {
    var Web;
    (function (Web) {
        var FileManager;
        (function (FileManager) {
            var Util = /** @class */ (function () {
                function Util() {
                }
                Util.IsStringNullOrEmpty = function (text) {
                    return (text === null || text === "" || typeof text === "undefined");
                };
                Util.FormatString = function (format) {
                    var args = [];
                    for (var _i = 1; _i < arguments.length; _i++) {
                        args[_i - 1] = arguments[_i];
                    }
                    var result = format;
                    for (var i = 0; i < args.length; i++) {
                        var regex = new RegExp('\\{' + (i) + '\\}', 'gm');
                        result = result.replace(regex, args[i]);
                    }
                    return result;
                };
                return Util;
            }());
            FileManager.Util = Util;
        })(FileManager = Web.FileManager || (Web.FileManager = {}));
    })(Web = Roadkill.Web || (Roadkill.Web = {}));
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=util.js.map