using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// A marker interface for classes to say "Hey, I'm not using DI right and letting consumers get to
	/// me by using the IoC service locator directly".
	/// </summary>
	public interface IInjectionLaunderer
	{
		// Classes that implement this interface will have a static GetInstance() method 
	}
}
