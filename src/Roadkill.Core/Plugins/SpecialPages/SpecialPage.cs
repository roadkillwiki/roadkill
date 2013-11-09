using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Plugins.SpecialPages
{
	public abstract class SpecialPage : ISetterInjected
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserManager { get; set; }

		[SetterProperty]
		public PageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		/// <summary>
		/// The name of the special page, used in the url /Special:{name}
		/// </summary>
		public abstract string Name { get; }

		public abstract ActionResult GetResult();
	}

	public class WhoAmISpecialPage : SpecialPage
	{
		public override string Name
		{
			get
			{
				return "WhoAmI";
			}
		}

		public override ActionResult GetResult()
		{
			string loginName = Context.CurrentUsername;
			if (!Context.IsLoggedIn)
				loginName = "Anonymoose";

			return new ContentResult() { Content = loginName };
		}
	}


	public class SoundCloudSpecialPage : SpecialPage
	{
		public override string Name
		{
			get
			{
				return "SoundCloud";
			}
		}

		public override ActionResult GetResult()
		{
			return new ViewResult() { ViewName = "SoundCloud/SoundCloud", };
		}
	}
}
