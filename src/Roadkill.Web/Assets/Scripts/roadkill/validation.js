/// <reference path="typescript-ref/references.ts" />
var Roadkill;
(function (Roadkill) {
    var Web;
    (function (Web) {
        var Validation = /** @class */ (function () {
            function Validation() {
            }
            /* "rules" should be in the format:
                Configure({
                    fieldName :
                        { required: true },
                    fieldName2  :
                        { required: true }
                });
            */
            Validation.prototype.Configure = function (formSelector, rules) {
                $.validator.messages = {
                    required: ROADKILL_REQUIRED_FIELD
                };
                $(formSelector).validate({
                    "rules": rules,
                    highlight: function (element) {
                        $(element).closest('.form-group').addClass('has-error');
                    },
                    unhighlight: function (element) {
                        $(element).closest('.form-group').removeClass('has-error');
                    },
                    errorElement: 'span',
                    errorClass: 'help-block',
                    errorPlacement: function (error, element) {
                        if (element.parent('.input-group').length) {
                            error.insertAfter(element.parent());
                        }
                        else {
                            error.insertAfter(element);
                        }
                    }
                });
            };
            return Validation;
        }());
        Web.Validation = Validation;
    })(Web = Roadkill.Web || (Roadkill.Web = {}));
})(Roadkill || (Roadkill = {}));
//# sourceMappingURL=validation.js.map