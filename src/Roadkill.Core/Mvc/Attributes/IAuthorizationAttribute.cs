using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Attributes
{
	public interface IAuthorizationAttribute
	{
		IAuthorizationProvider AuthorizationProvider { get; set; }
	}
}
