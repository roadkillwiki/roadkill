using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Domain.Managers
{
	public interface IWikiImporter
	{
		void ImportFromSql(string connectionString);
		bool ConvertToCreole { get; set; }
	}
}
