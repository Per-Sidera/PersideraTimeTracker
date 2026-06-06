using System;

namespace PersideraTimeTracker.Models
{
    /// <summary>
    /// A project that time entries can be assigned to. Replaces the old
    /// free-form string categories. Persisted as part of <see cref="AppSettings"/>.
    /// </summary>
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#4A90D9"; // default blue
    }
}
