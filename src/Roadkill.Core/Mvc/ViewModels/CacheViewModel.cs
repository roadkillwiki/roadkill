using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class CacheViewModel
	{
		public IEnumerable<string> PageKeys { get; set; }
		public IEnumerable<string> ListKeys { get; set; }
		public IEnumerable<string> SiteKeys { get; set; }
		public bool IsCacheEnabled { get; set; }
	}
}
