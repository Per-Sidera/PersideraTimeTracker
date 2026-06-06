using System;
using System.IO;
using System.Text.Json;

namespace PersideraTimeTracker.Models
{
    /// <summary>
    /// JSON-backed application settings for billing and Mercury invoicing.
    /// Stored at %AppData%\PersideraTimeTracker\settings.json.
    /// The Mercury API token is NEVER stored here — it lives in Windows
    /// Credential Manager via <see cref="Services.CredentialManager"/>.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Hourly billing rate. Defaults to $35/hr.</summary>
        public decimal HourlyRate { get; set; } = 35m;

        /// <summary>First billing day of the month (defaults to the 1st).</summary>
        public int BillingDay1 { get; set; } = 1;

        /// <summary>Second billing day of the month (defaults to the 15th).</summary>
        public int BillingDay2 { get; set; } = 15;

        /// <summary>Mercury customer (recipient) id used when creating invoices.</summary>
        public string MercuryCustomerId { get; set; } = "";

        /// <summary>Mercury destination account id that the invoice deposits into.</summary>
        public string MercuryDestinationAccountId { get; set; } = "";

        /// <summary>Whether ACH debit is offered on generated invoices.</summary>
        public bool AchDebitEnabled { get; set; } = true;

        /// <summary>Whether credit-card payment is offered on generated invoices.</summary>
        public bool CreditCardEnabled { get; set; } = false;

        // NOTE: the Mercury API token is intentionally absent here. It is stored
        // securely in Windows Credential Manager (see CredentialManager).

        public static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PersideraTimeTracker");

        private static readonly string SettingsPath =
            Path.Combine(SettingsDirectory, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Loads settings from disk, returning defaults if the file is missing
        /// or unreadable.
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (loaded != null)
                    {
                        return loaded;
                    }
                }
            }
            catch (Exception)
            {
                // Fall through to defaults on any read/parse error.
            }

            return new AppSettings();
        }

        /// <summary>
        /// Persists the current settings to disk, creating the directory if needed.
        /// </summary>
        public void Save()
        {
            Directory.CreateDirectory(SettingsDirectory);
            string json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
