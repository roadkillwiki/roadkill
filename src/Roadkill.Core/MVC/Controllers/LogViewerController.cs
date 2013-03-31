using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Common;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	// Unfinished (started, then pulled)...coming in version 1.8
	[AdminRequired]
	public class LogViewerController : ControllerBase
	{
		public LogViewerController(ApplicationSettings settings, UserManager userManager, 
			IRoadkillContext context, SettingsManager siteSettingsManager) 
			: base(settings, userManager, context, siteSettingsManager)
		{
		}

		/// <summary>
		/// The default list view of events.
		/// </summary>
		public ActionResult Index(string logLevel, int? maxItems, string messageFilter, string startDate, string endDate)
		{
			IEnumerable<Log4jEvent> eventList = LogReader.LoadAll();
			IEnumerable<Log4jEvent> filteredList = eventList;

			//
			// Log level type filter
			//
			if (string.IsNullOrEmpty(logLevel))
				logLevel = "Error";

			if (logLevel != "All")
			{
				filteredList =
					from e in filteredList
					where e.Level != null && e.Level.ToLower() == logLevel.ToLower()
					select e;
			}

			//
			// Start and end date filter
			//
			DateTime startDateTime = this.ParseFriendlyDate(startDate);
			DateTime endDateTime = this.ParseFriendlyDate(endDate);
			if (startDateTime > DateTime.MinValue)
			{
				filteredList =
					from e in filteredList
					where e.Timestamp >= startDateTime
					select e;
			}

			if (endDateTime > DateTime.MinValue)
			{
				filteredList =
					from e in filteredList
					where e.Timestamp <= endDateTime
					select e;
			}

			//
			// Message filter
			//
			bool filterMessages = !string.IsNullOrEmpty(messageFilter);
			List<Log4jEvent> list = new List<Log4jEvent>();
			foreach (Log4jEvent filteredEntry in filteredList)
			{
				if (filterMessages)
				{
					string loweredMessageFilter = messageFilter.ToLower();
					string message = filteredEntry.Message;

					if (!string.IsNullOrEmpty(message))
					{ 
						message = message.ToLower();
						if (message.Contains(loweredMessageFilter))
							list.Add(filteredEntry);
					}
				}
				else
				{
					list.Add(filteredEntry);
				}
			}

			//
			// Paging
			//
			int maxItemsToTake = maxItems.GetValueOrDefault();
			if (maxItemsToTake < 1)
				maxItemsToTake = int.MaxValue;

			if (string.IsNullOrEmpty(logLevel))
				logLevel = "All";

			ViewData["logLevel"] = logLevel;
			ViewData["maxItems"] = maxItems.GetValueOrDefault();
			ViewData["messageFilter"] = messageFilter;
			ViewData["startDate"] = startDate;
			ViewData["endDate"] = endDate;
			ViewData["lastUpdate"] = DateTime.Now;

			// Sort by the time generated and only take N items
			list = (from e in list orderby e.Timestamp descending select e).Take(maxItemsToTake).ToList<Log4jEvent>();

			return View(list);
		}

		/// <summary>
		/// JSON action to retrieve a message from the cached list of aggregated events, and returns a HTML string with the message text.
		/// </summary>
		/// <param name="id">The Guid of the event</param>
		/// <returns></returns>
		public ActionResult GetMessageText(string id)
		{
			StringBuilder builder = new StringBuilder();
			long entryId;
			long.TryParse(id, out entryId);
			Log4jEvent entry = LogReader.CachedItems.FirstOrDefault(e => e.Id == entryId); // use a cached list instead of loading mbs of log files each time

			if (entry != null)
			{
				builder.Append("<br />");
				builder.Append("<pre class=\"entrymessage\">" + entry.Message.Replace("\r", "") + "</pre>");
				builder.Append("<br style=\"clear:both;\" />");
			}
			return base.Content(builder.ToString());
		}

		/// <summary>
		/// Turns constants such as 'today', 'yesterday', 'lastweek' into a DateTime.
		/// </summary>
		private DateTime ParseFriendlyDate(string startDate)
		{
			DateTime result = DateTime.MinValue;
			if (!string.IsNullOrEmpty(startDate))
			{
				startDate = startDate.ToLower();
				if (startDate == "today")
				{
					result = DateTime.Today;
				}
				else
				{
					if (startDate == "yesterday")
					{
						result = DateTime.UtcNow.Yesterday();
					}
					else
					{
						if (startDate == "thisweek")
						{
							result = DateTime.UtcNow.StartOfWeek();
						}
						else
						{
							if (startDate == "lastweek")
							{
								result = DateTime.UtcNow.StartOfWeek().AddDays(-7.0);
							}
							else
							{
								if (startDate == "thismonth")
								{
									result = DateTime.UtcNow.StartOfThisMonth();
								}
								else
								{
									if (startDate == "lastmonth")
									{
										result = DateTime.UtcNow.StartOfLastMonth();
									}
									else
									{
										result = DateTime.MinValue;
										DateTime.TryParse(startDate, out result);
									}
								}
							}
						}
					}
				}
			}

			return result;
		}
	}
}
