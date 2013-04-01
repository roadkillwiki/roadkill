using System;

namespace Roadkill.Core
{
	/// <summary>
	/// A set of extension methods for dates.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Returns a DateTime for yesterday at 12am.
		/// </summary>
		/// <param name="dateTime">The DateTime to adjust</param>
		/// <returns>Returns a DateTime instance adjusted for yesterday at 12am.</returns>
		public static DateTime Yesterday(this DateTime dateTime)
		{
			return DateTime.Today.AddDays(-1);
		}

		/// <summary>
		/// Returns a DateTime adjusted to the beginning of the week.
		/// </summary>
		/// <param name="dateTime">The DateTime to adjust</param>
		/// <returns>A DateTime instance adjusted to the beginning of the current week</returns>
		/// <remarks>the beginning of the week is controlled by the current Culture</remarks>
		public static DateTime StartOfWeek(this DateTime dateTime)
		{
			//Thanks to: http://stackoverflow.com/questions/38039/how-can-i-get-the-datetime-for-the-start-of-the-week/38064#38064
			System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
			DayOfWeek startOfWeek = ci.DateTimeFormat.FirstDayOfWeek;
			DayOfWeek today = DateTime.Now.DayOfWeek;

			int diff = today - startOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}

			return dateTime.AddDays(-1 * diff).Date;
		}

		/// <summary>
		/// Returns a DateTime for the first day of last week.
		/// </summary>
		/// <param name="dateTime">The DateTime to adjust</param>
		/// <returns>Returns a DateTime instance adjusted for the first day of last week</returns>
		public static DateTime LastWeek(this DateTime dateTime)
		{
			return DateTime.Today.StartOfWeek().AddDays(-7);
		}

		/// <summary>
		/// Returns a DateTime for the first day of this month.
		/// </summary>
		/// <param name="dateTime">The DateTime to adjust</param>
		/// <returns>Returns a DateTime instance adjusted for the first day of this month.</returns>
		public static DateTime StartOfThisMonth(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
		}

		/// <summary>
		/// Returns a DateTime for the first day of last month.
		/// </summary>
		/// <param name="dateTime">The DateTime to adjust</param>
		/// <returns>Returns a DateTime instance adjusted for the first day of last month.</returns>
		public static DateTime StartOfLastMonth(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0).AddMonths(-1);
		}
	}
}