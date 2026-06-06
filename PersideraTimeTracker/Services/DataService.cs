using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PersideraTimeTracker.Models;

namespace PersideraTimeTracker.Services
{
    /// <summary>
    /// Loads and saves time entries as JSON at
    /// %AppData%\PersideraTimeTracker\entries.json.
    /// </summary>
    public class DataService
    {
        private static readonly string EntriesPath =
            Path.Combine(AppSettings.SettingsDirectory, "entries.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Loads all stored entries, returning an empty list if the file is
        /// missing or unreadable.
        /// </summary>
        public List<TimeEntry> LoadEntries()
        {
            try
            {
                if (File.Exists(EntriesPath))
                {
                    string json = File.ReadAllText(EntriesPath);
                    List<TimeEntry>? loaded = JsonSerializer.Deserialize<List<TimeEntry>>(json, JsonOptions);
                    if (loaded != null)
                    {
                        return loaded;
                    }
                }
            }
            catch (Exception)
            {
                // Fall through to an empty list on any read/parse error.
            }

            return new List<TimeEntry>();
        }

        /// <summary>
        /// Persists the supplied entries to disk, creating the directory if needed.
        /// </summary>
        public void SaveEntries(List<TimeEntry> entries)
        {
            Directory.CreateDirectory(AppSettings.SettingsDirectory);
            string json = JsonSerializer.Serialize(entries, JsonOptions);
            File.WriteAllText(EntriesPath, json);
        }
    }
}
