using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Attributes
{
	public interface IAuthorizationAttribute
	{
		IAuthorizationProvider AuthorizationProvider { get; set; }
	}
}
