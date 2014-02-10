var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/filemanager.references.ts" />
        (function (FileManager) {
            var Util = (function () {
                function Util() {
                }
                Util.IsStringNullOrEmpty = function (text) {
                    return (text === null || text === "" || typeof text === "undefined");
                };

                Util.FormatString = function (format) {
                    var args = [];
                    for (var _i = 0; _i < (arguments.length - 1); _i++) {
                        args[_i] = arguments[_i + 1];
                    }
                    var result = format;
                    for (var i = 0; i < args.length; i++) {
                        var regex = new RegExp('\\{' + (i) + '\\}', 'gm');
                        result = result.replace(regex, args[i]);
                    }

                    return result;
                };
                return Util;
            })();
            FileManager.Util = Util;
        })(Web.FileManager || (Web.FileManager = {}));
        var FileManager = Web.FileManager;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=util.js.map
