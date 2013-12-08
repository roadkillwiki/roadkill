using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Cache
{
	/// <summary>
	/// Most of the PageViewModelCache tests are in the pageservicetests, to test
	/// the integration between the cache and the pageservice.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class PageViewModelCacheTests
	{
		[Test]
		public void RemoveAll_Should_Remove_PageViewModelCache_Keys_Only()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			cache.Add("site.blah", "xyz", new CacheItemPolicy());

			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = false };
			PageViewModelCache pageCache = new PageViewModelCache(settings, cache);
			pageCache.Add(1, 1, new PageViewModel());

			// Act
			pageCache.RemoveAll();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}
	}
}
