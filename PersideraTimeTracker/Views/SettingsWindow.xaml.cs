using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PersideraTimeTracker.Models;
using PersideraTimeTracker.Services;

namespace PersideraTimeTracker.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;

        public SettingsWindow(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeComponent();
            LoadValues();
        }

        private void LoadValues()
        {
            HourlyRate.Text = _settings.HourlyRate.ToString(CultureInfo.InvariantCulture);
            BillingDay1.Text = _settings.BillingDay1.ToString();
            BillingDay2.Text = _settings.BillingDay2.ToString();
            CustomerId.Text = _settings.MercuryCustomerId;
            DestinationAccountId.Text = _settings.MercuryDestinationAccountId;
            AchDebit.IsChecked = _settings.AchDebitEnabled;
            CreditCard.IsChecked = _settings.CreditCardEnabled;
            RefreshTokenStatus();
        }

        private void RefreshTokenStatus()
        {
            bool configured;
            try { configured = CredentialManager.HasToken(); }
            catch { configured = false; }

            TokenStatus.Text = configured
                ? "A Mercury API token is stored in Windows Credential Manager."
                : "No Mercury API token stored yet.";
            TokenStatus.Foreground = configured
                ? new SolidColorBrush(Color.FromRgb(0x7B, 0xD8, 0x8F))
                : new SolidColorBrush(Color.FromRgb(0x6A, 0x6A, 0x6A));
        }

        private void SaveToken_Click(object sender, RoutedEventArgs e)
        {
            string token = ApiToken.Password.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show(this, "Please enter a token before saving.", "Mercury API Token",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                CredentialManager.SaveToken(token);
                ApiToken.Clear();
                RefreshTokenStatus();
                MessageBox.Show(this, "Mercury API token saved to Windows Credential Manager.",
                    "Mercury API Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Mercury API Token",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(HourlyRate.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate)
                || rate < 0)
            {
                MessageBox.Show(this, "Please enter a valid hourly rate.", "Settings",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settings.HourlyRate = rate;
            _settings.BillingDay1 = ClampDay(BillingDay1.Text, _settings.BillingDay1);
            _settings.BillingDay2 = ClampDay(BillingDay2.Text, _settings.BillingDay2);
            _settings.MercuryCustomerId = CustomerId.Text.Trim();
            _settings.MercuryDestinationAccountId = DestinationAccountId.Text.Trim();
            _settings.AchDebitEnabled = AchDebit.IsChecked == true;
            _settings.CreditCardEnabled = CreditCard.IsChecked == true;

            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save settings: " + ex.Message, "Settings",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private static int ClampDay(string text, int fallback)
        {
            if (int.TryParse(text, out int v))
            {
                return Math.Min(Math.Max(v, 1), 28);
            }
            return fallback;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
