using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// A marker interface for classes that bypass constructor injection.
	/// </summary>
	public interface IInjectionLaunderer
	{
		// Classes that implement this interface will have a static GetInstance() method 
	}
}
