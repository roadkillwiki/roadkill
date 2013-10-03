using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class PluginSettingsSummary
	{
		public string Id { get; set; }
		public Guid DatabaseId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsEnabled { get; set; }
	}
}
