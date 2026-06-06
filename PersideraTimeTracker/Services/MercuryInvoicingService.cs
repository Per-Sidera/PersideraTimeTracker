using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PersideraTimeTracker.Models;

namespace PersideraTimeTracker.Services
{
    /// <summary>
    /// Creates accounts-receivable invoices in Mercury from tracked time.
    /// The API token is pulled from the Windows Credential Manager, never from
    /// the JSON settings file.
    /// </summary>
    public class MercuryInvoicingService
    {
        private const string BaseUrl = "https://api.mercury.com/api/v1";

        private readonly HttpClient _http;
        private readonly AppSettings _settings;

        public MercuryInvoicingService(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            string? token = CredentialManager.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException(
                    "Mercury API token not configured. Open Settings to add it.");
            }

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Creates a Mercury invoice for the supplied entries, grouping line
        /// items by category. Returns the raw response body on success.
        /// Throws <see cref="MercuryAccessDeniedException"/> on HTTP 403 (e.g.
        /// the account's subscription tier does not include invoicing).
        /// </summary>
        public async Task<string> CreateInvoiceAsync(
            List<TimeEntry> entries,
            DateTime invoiceDate,
            DateTime dueDate)
        {
            if (entries == null || entries.Count == 0)
            {
                throw new InvalidOperationException("There are no tracked entries to invoice for this period.");
            }

            var lineItems = entries
                .Where(e => e.IsBillable)
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Category) ? "Development" : e.Category)
                .Select(g => new
                {
                    description = g.Key,
                    quantity = Math.Round(g.Sum(e => e.Duration.TotalHours), 2),
                    unitPrice = (double)_settings.HourlyRate
                })
                .ToList();

            if (lineItems.Count == 0)
            {
                throw new InvalidOperationException("There are no billable entries to invoice for this period.");
            }

            var payload = new
            {
                customerId = _settings.MercuryCustomerId,
                destinationAccountId = _settings.MercuryDestinationAccountId,
                invoiceDate = invoiceDate.ToString("yyyy-MM-dd"),
                dueDate = dueDate.ToString("yyyy-MM-dd"),
                achDebitEnabled = _settings.AchDebitEnabled,
                creditCardEnabled = _settings.CreditCardEnabled,
                useRealAccountNumber = false,
                sendEmailOption = "SendNow",
                lineItems
            };

            HttpResponseMessage response = await _http.PostAsJsonAsync($"{BaseUrl}/ar/invoices", payload);
            string body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new MercuryAccessDeniedException(
                    "Mercury returned 403 Forbidden. Invoicing requires an eligible Mercury subscription tier, " +
                    "and the API token must have invoicing permissions. " +
                    $"Response: {body}");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Mercury API error {(int)response.StatusCode} {response.StatusCode}: {body}");
            }

            return body;
        }

        /// <summary>
        /// Computes the total billable amount for a set of entries at the
        /// configured hourly rate.
        /// </summary>
        public static decimal CalculateAmount(IEnumerable<TimeEntry> entries, decimal hourlyRate)
        {
            double totalHours = entries.Where(e => e.IsBillable).Sum(e => e.Duration.TotalHours);
            return Math.Round((decimal)totalHours * hourlyRate, 2);
        }
    }

    /// <summary>
    /// Raised when Mercury returns HTTP 403, typically due to a subscription
    /// tier or token permission issue. Handled with a clear, friendly message.
    /// </summary>
    public class MercuryAccessDeniedException : Exception
    {
        public MercuryAccessDeniedException(string message) : base(message)
        {
        }
    }
}
