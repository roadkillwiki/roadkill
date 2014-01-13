/// <reference path="typescript-ref/references.ts" />
module Roadkill.Web
{
	export class Validation
	{
		/* "rules" should be in the format:
			Configure({ 
				fieldName : 
					{ required: true },
				fieldName2  : 
					{ required: true }
			});
		*/
		public Configure(formSelector: string, rules : any)
		{
			$.validator.messages = {
				required: ROADKILL_REQUIRED_FIELD
			};
			
			$(formSelector).validate({
				"rules" : rules,
				highlight: function (element)
				{
					$(element).closest('.form-group').addClass('has-error');
				},
				unhighlight: function (element)
				{
					$(element).closest('.form-group').removeClass('has-error');
				},
				errorElement: 'span',
				errorClass: 'help-block',
				errorPlacement: function (error, element)
				{
					if (element.parent('.input-group').length)
					{
						error.insertAfter(element.parent());
					} else
					{
						error.insertAfter(element);
					}
				}
			});
		}
	}
}