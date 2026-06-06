using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PersideraTimeTracker.Models
{
    /// <summary>
    /// A day's worth of <see cref="TimeEntry"/> rows, with a header date and a
    /// total duration. Used to drive the Clockify-style grouped timeline.
    /// </summary>
    public class TimeEntryGroup : ObservableCollection<TimeEntry>
    {
        public DateTime Date { get; }

        /// <summary>"Mon, Jun 1".</summary>
        public string DateDisplay { get; }

        /// <summary>"Total: 07:30:00".</summary>
        public string TotalDisplay
        {
            get
            {
                TimeSpan total = this.Aggregate(TimeSpan.Zero, (acc, e) =>
                {
                    TimeSpan d = e.Duration;
                    return acc + (d < TimeSpan.Zero ? TimeSpan.Zero : d);
                });
                return $"{(int)total.TotalHours:D2}:{total.Minutes:D2}:{total.Seconds:D2}";
            }
        }

        public TimeEntryGroup(DateTime date, IEnumerable<TimeEntry> entries)
            : base(entries.OrderByDescending(e => e.StartTime))
        {
            Date = date.Date;
            DateDisplay = Date.ToString("ddd, MMM d");
        }
    }
}
