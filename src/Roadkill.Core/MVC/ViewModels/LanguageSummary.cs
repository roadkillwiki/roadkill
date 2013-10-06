using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class LanguageSummary
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public LanguageSummary(string code, string name)
		{
			Code = code;
			Name = name;
		}

		public static IEnumerable<LanguageSummary> SupportedLocales()
		{
			List<LanguageSummary> languages = new List<LanguageSummary>()
			{
				new LanguageSummary("en", "English"),
				new LanguageSummary("cz", "Čeština"),
				new LanguageSummary("de", "Deutsch"),
				new LanguageSummary("es", "Español"),
				new LanguageSummary("hi", "हिंदी"),
				new LanguageSummary("it", "Italiano"),
				new LanguageSummary("pl", "Polski"),
				new LanguageSummary("ru", "Pусский"),
				new LanguageSummary("sw", "Svensk"),
			};

			return languages;
		}
	}
}
