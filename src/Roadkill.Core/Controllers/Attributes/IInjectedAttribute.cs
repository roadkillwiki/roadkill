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
		[SetterProperty]
		IConfigurationContainer Configuration { get; set; }

		[SetterProperty]
		IRoadkillContext Context { get; set; }

		[SetterProperty]
		UserManager UserManager { get; set; }

		[SetterProperty]
		PageManager PageManager { get; set; }
	}
}
