using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersideraTimeTracker.Models;
using PersideraTimeTracker.Services;
using PersideraTimeTracker.Views;

namespace PersideraTimeTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AppSettings _settings;
        private readonly DataService _dataService = new();
        private readonly List<TimeEntry> _entries;
        private readonly DispatcherTimer _timer;

        private DateTime _trackingStart;

        public ObservableCollection<TimeEntryGroup> GroupedEntries { get; } = new();

        public ObservableCollection<string> Categories { get; } = new()
        {
            "Development", "Design", "Art", "Research", "Marketing",
            "Meetings", "Admin", "Support"
        };

        [ObservableProperty]
        private string _newEntryDescription = "";

        [ObservableProperty]
        private string _selectedCategory = "Development";

        [ObservableProperty]
        private bool _newEntryBillable = true;

        [ObservableProperty]
        private bool _isTracking;

        [ObservableProperty]
        private string _runningTimerDisplay = "00:00:00";

        [ObservableProperty]
        private string _currentPeriodDisplay = "";

        [ObservableProperty]
        private string _totalHoursDisplay = "0.0h tracked";

        [ObservableProperty]
        private string _totalValueDisplay = "$0.00";

        public MainViewModel(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _entries = _dataService.LoadEntries();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => UpdateRunningTimer();

            RebuildGroups();
            UpdateStatusBar();
        }

        // ---------- Commands ----------

        [RelayCommand]
        private void AddEntry()
        {
            if (IsTracking)
            {
                StopTracking();
            }
            else
            {
                StartTracking();
            }
        }

        private void StartTracking()
        {
            _trackingStart = DateTime.Now;
            IsTracking = true;
            RunningTimerDisplay = "00:00:00";
            _timer.Start();
        }

        private void StopTracking()
        {
            _timer.Stop();
            IsTracking = false;

            var entry = new TimeEntry
            {
                StartTime = _trackingStart,
                EndTime = DateTime.Now,
                Description = string.IsNullOrWhiteSpace(NewEntryDescription)
                    ? "(no description)" : NewEntryDescription.Trim(),
                Category = SelectedCategory,
                IsBillable = NewEntryBillable
            };

            _entries.Add(entry);
            Persist();
            NewEntryDescription = "";
            RunningTimerDisplay = "00:00:00";
            RebuildGroups();
            UpdateStatusBar();
        }

        [RelayCommand]
        private void DeleteEntry(TimeEntry? entry)
        {
            if (entry == null) return;
            _entries.RemoveAll(e => e.Id == entry.Id);
            Persist();
            RebuildGroups();
            UpdateStatusBar();
        }

        [RelayCommand]
        private void RestartTimer(TimeEntry? entry)
        {
            if (entry == null) return;
            if (IsTracking) StopTracking();
            NewEntryDescription = entry.Description;
            SelectedCategory = entry.Category;
            NewEntryBillable = entry.IsBillable;
            StartTracking();
        }

        [RelayCommand]
        private void EditEntry(TimeEntry? entry)
        {
            if (entry == null) return;
            var win = new Views.EditEntryWindow(entry, Categories)
            {
                Owner = Application.Current.MainWindow
            };
            if (win.ShowDialog() == true)
            {
                Persist();
                RebuildGroups();
                UpdateStatusBar();
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task CreateInvoice()
        {
            try
            {
                if (CredentialManager.GetToken() is null ||
                    string.IsNullOrEmpty(_settings.MercuryCustomerId) ||
                    string.IsNullOrEmpty(_settings.MercuryDestinationAccountId))
                {
                    MessageBox.Show(
                        "Mercury invoicing is not set up. Open Settings to add your API token and account IDs.",
                        "Create Invoice", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var (start, end) = BillingPeriodService.GetCurrentPeriod(_settings);
                var periodEntries = BillingPeriodService.GetEntriesForPeriod(_entries, start, end);
                if (periodEntries.Count == 0)
                {
                    MessageBox.Show("There are no tracked entries in the current billing period.",
                        "Create Invoice", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                decimal amount = MercuryInvoicingService.CalculateAmount(periodEntries, _settings.HourlyRate);
                var confirm = MessageBox.Show(
                    $"Create a Mercury invoice for {periodEntries.Count} entries totaling " +
                    $"{amount.ToString("C", CultureInfo.GetCultureInfo("en-US"))}?",
                    "Create Invoice", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                var service = new MercuryInvoicingService(_settings);
                DateTime due = end.Date.AddDays(14);
                await service.CreateInvoiceAsync(periodEntries, DateTime.Now.Date, due);

                MessageBox.Show("Invoice created successfully in Mercury.",
                    "Create Invoice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (MercuryAccessDeniedException ex)
            {
                MessageBox.Show(ex.Message, "Create Invoice", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create invoice:\n\n" + ex.Message,
                    "Create Invoice", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            var win = new Views.SettingsWindow(_settings)
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
            UpdateStatusBar();
        }

        // ---------- Helpers ----------

        private void UpdateRunningTimer()
        {
            TimeSpan elapsed = DateTime.Now - _trackingStart;
            RunningTimerDisplay = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }

        private void Persist() => _dataService.SaveEntries(_entries);

        private void RebuildGroups()
        {
            GroupedEntries.Clear();
            var groups = _entries
                .GroupBy(e => e.StartTime.Date)
                .OrderByDescending(g => g.Key);
            foreach (var g in groups)
            {
                GroupedEntries.Add(new TimeEntryGroup(g.Key, g));
            }
        }

        private void UpdateStatusBar()
        {
            var (start, end) = BillingPeriodService.GetCurrentPeriod(_settings);
            CurrentPeriodDisplay = $"Period: {start:MMM d} – {end:MMM d}";

            var periodEntries = BillingPeriodService.GetEntriesForPeriod(_entries, start, end);
            double hours = periodEntries.Sum(e =>
            {
                var d = e.Duration;
                return d < TimeSpan.Zero ? 0 : d.TotalHours;
            });
            TotalHoursDisplay = $"{hours:0.0}h tracked";

            decimal value = MercuryInvoicingService.CalculateAmount(periodEntries, _settings.HourlyRate);
            TotalValueDisplay = value.ToString("C", CultureInfo.GetCultureInfo("en-US"));
        }
    }
}
