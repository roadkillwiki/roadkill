using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Roadkill.Core
{
	public class ConfigurationException : ExceptionBase
	{
		public ConfigurationException(string message, params string[] args) : base(message, args) { }
	}
}