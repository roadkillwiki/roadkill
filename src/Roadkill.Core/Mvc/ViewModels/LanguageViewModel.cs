using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class LanguageViewModel
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public LanguageViewModel(string code, string name)
		{
			Code = code;
			Name = name;
		}

		public static IEnumerable<LanguageViewModel> SupportedLocales()
		{
			List<LanguageViewModel> languages = new List<LanguageViewModel>()
			{
				new LanguageViewModel("en", "English"),
				new LanguageViewModel("cs", "Čeština"),
				new LanguageViewModel("de", "Deutsch"),
				new LanguageViewModel("nl", "Dutch"),
				new LanguageViewModel("es", "Español"),
				new LanguageViewModel("hi", "हिंदी"),
				new LanguageViewModel("it", "Italiano"),
				new LanguageViewModel("pl", "Polski"),
				new LanguageViewModel("pt", "Português"),
				new LanguageViewModel("ru", "Pусский"),
				new LanguageViewModel("sv", "Svensk"),
			};

			return languages;
		}
	}
}
