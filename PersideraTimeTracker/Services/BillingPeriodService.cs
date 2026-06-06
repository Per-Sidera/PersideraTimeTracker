using System;
using System.Collections.Generic;
using System.Linq;
using PersideraTimeTracker.Models;

namespace PersideraTimeTracker.Services
{
    /// <summary>
    /// Computes semi-monthly billing periods. Billing boundaries land on the
    /// configured days of the month (defaults: 1st and 15th), shifted to the
    /// nearest weekday when they fall on a weekend.
    /// </summary>
    public static class BillingPeriodService
    {
        /// <summary>
        /// Returns the actual billing date for a target day-of-month in a given
        /// month, adjusted to the nearest weekday (Saturday -> Friday,
        /// Sunday -> Monday). The target day is clamped to the number of days in
        /// the month.
        /// </summary>
        public static DateTime GetBillingDate(int year, int month, int targetDay)
        {
            int day = Math.Min(Math.Max(targetDay, 1), DateTime.DaysInMonth(year, month));
            var date = new DateTime(year, month, day);
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => date.AddDays(-1),
                DayOfWeek.Sunday => date.AddDays(1),
                _ => date
            };
        }

        /// <summary>
        /// Returns all billing boundary dates (sorted ascending) that surround
        /// <paramref name="reference"/>, spanning the previous, current and next
        /// month so a containing period can always be found.
        /// </summary>
        private static List<DateTime> GetBoundaries(AppSettings settings, DateTime reference)
        {
            var boundaries = new List<DateTime>();

            void AddMonth(int year, int month)
            {
                boundaries.Add(GetBillingDate(year, month, settings.BillingDay1));
                boundaries.Add(GetBillingDate(year, month, settings.BillingDay2));
            }

            DateTime prev = reference.AddMonths(-1);
            DateTime next = reference.AddMonths(1);

            AddMonth(prev.Year, prev.Month);
            AddMonth(reference.Year, reference.Month);
            AddMonth(next.Year, next.Month);

            return boundaries
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        /// <summary>
        /// Returns the (Start, End) of the billing period containing
        /// <paramref name="now"/>. Start is inclusive (midnight of the boundary
        /// day); End is the last tick before the next boundary day begins.
        /// </summary>
        public static (DateTime Start, DateTime End) GetCurrentPeriod(AppSettings settings, DateTime now)
        {
            List<DateTime> boundaries = GetBoundaries(settings, now);
            DateTime today = now.Date;

            // The period starts on the latest boundary that is on or before today.
            DateTime start = boundaries.Where(b => b.Date <= today).DefaultIfEmpty(boundaries.First()).Last();

            // It ends just before the next boundary after the start.
            DateTime? nextBoundary = boundaries.Where(b => b.Date > start.Date).Cast<DateTime?>().FirstOrDefault();
            DateTime end = nextBoundary.HasValue
                ? nextBoundary.Value.Date.AddTicks(-1)
                : start.Date.AddMonths(1).AddTicks(-1);

            return (start.Date, end);
        }

        /// <summary>
        /// Convenience overload using the current local time.
        /// </summary>
        public static (DateTime Start, DateTime End) GetCurrentPeriod(AppSettings settings)
        {
            return GetCurrentPeriod(settings, DateTime.Now);
        }

        /// <summary>
        /// Returns the entries whose start time falls within the inclusive
        /// [start, end] period.
        /// </summary>
        public static List<TimeEntry> GetEntriesForPeriod(
            IEnumerable<TimeEntry> entries, DateTime start, DateTime end)
        {
            return entries
                .Where(e => e.StartTime >= start && e.StartTime <= end)
                .ToList();
        }
    }
}
