using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Roadkill.Tests.Unit.StubsAndMocks.Mvc
{
	public class HttpCachePolicyMock : HttpCachePolicyBase
	{
		public HttpCacheability HttpCacheability { get; set; }
		public DateTime Expires { get; set; }
		public TimeSpan MaxAge { get; set; }
		public DateTime LastModified { get; set; }

		public override void SetCacheability(HttpCacheability cacheability)
		{
			HttpCacheability = cacheability;
		}

		public override void SetExpires(DateTime date)
		{
			Expires = date;
		}

		public override void SetMaxAge(TimeSpan delta)
		{
			MaxAge = delta;
		}

		public override void SetLastModified(DateTime date)
		{
			LastModified = date;
		}

	}
}
