using System;
using System.Text.Json.Serialization;

namespace PersideraTimeTracker.Models
{
    /// <summary>
    /// A single tracked block of work. Replaces the old WinForms TimeTrackerData.
    /// Persisted to entries.json via DataService.
    /// </summary>
    public class TimeEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Description { get; set; } = "";

        public string Category { get; set; } = "Development";

        public bool IsBillable { get; set; } = true;

        [JsonIgnore]
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>"HH:MM:SS" duration display, e.g. "07:30:00".</summary>
        [JsonIgnore]
        public string DurationDisplay
        {
            get
            {
                TimeSpan d = Duration;
                if (d < TimeSpan.Zero) d = TimeSpan.Zero;
                return $"{(int)d.TotalHours:D2}:{d.Minutes:D2}:{d.Seconds:D2}";
            }
        }

        /// <summary>"9:30 – 5:00 PM" style time range for the row.</summary>
        [JsonIgnore]
        public string TimeRangeDisplay =>
            $"{StartTime.ToString("h:mm tt")} – {EndTime.ToString("h:mm tt")}";

        /// <summary>"$" indicator, dimmed when not billable (handled in the view).</summary>
        [JsonIgnore]
        public string BillableGlyph => "$";
    }
}
