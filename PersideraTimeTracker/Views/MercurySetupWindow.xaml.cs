using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using PersideraTimeTracker.Models;
using PersideraTimeTracker.Services;

namespace PersideraTimeTracker.Views
{
    /// <summary>
    /// First-run wizard prompting the user to connect Mercury invoicing. Saves
    /// the hourly rate / IDs / payment options into <see cref="AppSettings"/>
    /// and the API token into Windows Credential Manager.
    /// </summary>
    public partial class MercurySetupWindow : Window
    {
        private readonly AppSettings _settings;

        public MercurySetupWindow(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeComponent();
            LoadValues();
        }

        private void LoadValues()
        {
            HourlyRate.Text = _settings.HourlyRate.ToString(CultureInfo.InvariantCulture);
            CustomerId.Text = _settings.MercuryCustomerId;
            DestinationAccountId.Text = _settings.MercuryDestinationAccountId;
            AchDebit.IsChecked = _settings.AchDebitEnabled;
            CreditCard.IsChecked = _settings.CreditCardEnabled;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string token = ApiToken.Password.Trim();
            string customer = CustomerId.Text.Trim();
            string dest = DestinationAccountId.Text.Trim();

            // If a token was supplied, require the IDs too so invoicing works.
            if (!string.IsNullOrEmpty(token) &&
                (string.IsNullOrEmpty(customer) || string.IsNullOrEmpty(dest)))
            {
                MessageBox.Show(this,
                    "Please fill in the Mercury Customer ID and Destination Account ID, " +
                    "or leave the token blank and use \"Skip for now\".",
                    "Connect Mercury Invoicing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(HourlyRate.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate)
                && rate >= 0)
            {
                _settings.HourlyRate = rate;
            }
            _settings.MercuryCustomerId = customer;
            _settings.MercuryDestinationAccountId = dest;
            _settings.AchDebitEnabled = AchDebit.IsChecked == true;
            _settings.CreditCardEnabled = CreditCard.IsChecked == true;

            try
            {
                _settings.Save();
                if (!string.IsNullOrEmpty(token))
                {
                    CredentialManager.SaveToken(token);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save Mercury setup:\n\n" + ex.Message,
                    "Connect Mercury Invoicing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
