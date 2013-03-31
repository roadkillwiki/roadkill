using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using StructureMap.Attributes;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines an Attribute that has its property values setter injected by Structuremap.
	/// </summary>
	public interface IInjectedAttribute
	{
		ApplicationSettings ApplicationSettings { get; set; }
		IRoadkillContext Context { get; set; }
		UserManager UserManager { get; set; }
		PageManager PageManager { get; set; }
	}
}
