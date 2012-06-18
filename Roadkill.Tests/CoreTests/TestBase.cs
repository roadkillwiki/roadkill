using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roadkill.Tests.Core
{
	[TestClass]
	public class TestBase
	{
		/// <summary>
		/// This method ensures that Roadkill isn't using the Http Request items to store its context
		/// (IsWeb = false), and is all in-memory. It also recreated the SQL lite database each time.
		/// </summary>
		[TestInitialize()]
		public void Initialize()
		{
			RoadkillContext.IsWeb = false;

			SettingsSummary summary = new SettingsSummary();
			summary.ConnectionString = RoadkillSettings.ConnectionString;
			SettingsManager.CreateTables(summary);

			RoadkillApplication app = new RoadkillApplication();
			app.SetupNHibernate();
		}
	}
}
