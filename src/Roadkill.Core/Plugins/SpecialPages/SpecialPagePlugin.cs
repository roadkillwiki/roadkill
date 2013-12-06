using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Plugins
{
	public abstract class SpecialPagePlugin : ISetterInjected
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		[SetterProperty]
		public IPageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		/// <summary>
		/// The name of the special page, used in the url /Special:{name}
		/// </summary>
		public abstract string Name { get; }

		public abstract ActionResult GetResult(SpecialPagesController controller);
	}
}
