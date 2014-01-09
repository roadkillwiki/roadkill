using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.DI;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Represents a plugin for a specific wiki Special:pluginname page, for example Special:Random.
	/// </summary>
	public abstract class SpecialPagePlugin : ISetterInjected
	{
		/// <summary>
		/// Gets or sets the current Roadkill <see cref="ApplicationSettings"/>. This property is automatically filled by Roadkill when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		/// <summary>
		/// Gets or sets the current logged in user represnted by <see cref="IUserContext"/>. This property is automatically filled by Roadkill when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public IUserContext Context { get; set; }

		/// <summary>
		/// Gets or sets the current Roadkill <see cref="UserServiceBase"/>. This property is automatically filled by Roadkill when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		/// <summary>
		/// Gets or sets the current Roadkill <see cref="IPageService"/>. This property is automatically filled by Roadkill when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public IPageService PageService { get; set; }

		/// <summary>
		/// Gets the current Roadkill <see cref="SettingsService"/> that can be used to get the current <see cref="SiteSettings"/>. This property is automatically filled by Roadkill when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		/// <summary>
		/// The unique name of the special page, used in the url /Special:{name}
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Returns an <see cref="System.Web.Mvc.ActionResult"/> for the special page. This can include a <see cref="ViewResult"/> that 
		/// points to a view in the /Plugin/PluginId folder.
		/// </summary>
		/// <param name="controller">The <see cref="SpecialPagesController"/> that the action belongs to.</param>
		/// <returns></returns>
		public abstract ActionResult GetResult(SpecialPagesController controller);
	}
}
