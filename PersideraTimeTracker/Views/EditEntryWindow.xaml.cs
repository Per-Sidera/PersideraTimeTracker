using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using PersideraTimeTracker.Models;

namespace PersideraTimeTracker.Views
{
    public partial class EditEntryWindow : Window
    {
        private readonly TimeEntry _entry;

        public EditEntryWindow(TimeEntry entry, IEnumerable<string> categories)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            InitializeComponent();

            foreach (var c in categories)
            {
                Category.Items.Add(c);
            }
            if (!Category.Items.Contains(_entry.Category))
            {
                Category.Items.Add(_entry.Category);
            }

            Description.Text = _entry.Description;
            Category.SelectedItem = _entry.Category;
            StartDate.SelectedDate = _entry.StartTime.Date;
            StartTime.Text = _entry.StartTime.ToString("HH:mm");
            EndDate.SelectedDate = _entry.EndTime.Date;
            EndTime.Text = _entry.EndTime.ToString("HH:mm");
            Billable.IsChecked = _entry.IsBillable;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildDateTime(StartDate.SelectedDate, StartTime.Text, out DateTime start))
            {
                Warn("Please enter a valid start date and time (HH:mm).");
                return;
            }
            if (!TryBuildDateTime(EndDate.SelectedDate, EndTime.Text, out DateTime end))
            {
                Warn("Please enter a valid end date and time (HH:mm).");
                return;
            }
            if (end < start)
            {
                Warn("End time must be after the start time.");
                return;
            }

            _entry.Description = string.IsNullOrWhiteSpace(Description.Text)
                ? "(no description)" : Description.Text.Trim();
            _entry.Category = Category.SelectedItem?.ToString() ?? _entry.Category;
            _entry.StartTime = start;
            _entry.EndTime = end;
            _entry.IsBillable = Billable.IsChecked == true;

            DialogResult = true;
            Close();
        }

        private static bool TryBuildDateTime(DateTime? date, string time, out DateTime result)
        {
            result = default;
            if (date is null) return false;
            if (!TimeSpan.TryParseExact(time?.Trim(), "h\\:mm", CultureInfo.InvariantCulture, out TimeSpan ts)
                && !TimeSpan.TryParse(time?.Trim(), CultureInfo.InvariantCulture, out ts))
            {
                return false;
            }
            result = date.Value.Date + ts;
            return true;
        }

        private void Warn(string message)
            => MessageBox.Show(this, message, "Edit Entry", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
