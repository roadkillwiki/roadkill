using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mindscape.LightSpeed.Logging;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Database.LightSpeed
{
	public class DatabaseLogger : ILogger
	{
		public void LogDebug(object text)
		{
			Log.Debug(text.ToString());
		}

		public void LogSql(object sql)
		{
			Log.Debug(sql.ToString());
		}
	}
}
