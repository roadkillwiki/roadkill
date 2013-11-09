using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core.Converters;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Text;
using Roadkill.Core.Plugins;
using Roadkill.Core.Plugins.SpecialPages;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for /Special: urls
	/// </summary>
	[OptionalAuthorization]
	public class SpecialPagesController : ControllerBase
	{
		private IPluginFactory _pluginFactory;

		public SpecialPagesController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context, 
			SettingsService settingsService, IPluginFactory pluginFactory)
			: base(settings, userManager, context, settingsService) 
		{
			_pluginFactory = pluginFactory;
		}

		public ActionResult Index(string id)
		{
			SpecialPage plugin = _pluginFactory.GetSpecialPagePlugin(id);

			// Throw an HttpException so the customerrors is used and not the default asp.net 404 page
			if (plugin == null)
				throw new HttpException(404, string.Format("A plugin for the special page '{0}' was not found", id));

			return plugin.GetResult();
		}
	}
}