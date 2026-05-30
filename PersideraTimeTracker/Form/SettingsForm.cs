using System;
using System.Drawing;
using System.Windows.Forms;

namespace PersideraTimeTracker.Form
{
    /// <summary>
    /// Settings dialog for hourly rate, billing days and Mercury invoicing
    /// configuration. The Mercury API token is written straight to Windows
    /// Credential Manager and never persisted to the JSON settings file.
    /// </summary>
    public class SettingsForm : System.Windows.Forms.Form
    {
        private readonly AppSettings _settings;

        private NumericUpDown _hourlyRate = null!;
        private NumericUpDown _billingDay1 = null!;
        private NumericUpDown _billingDay2 = null!;
        private TextBox _customerId = null!;
        private TextBox _destinationAccountId = null!;
        private CheckBox _achDebit = null!;
        private CheckBox _creditCard = null!;
        private TextBox _apiToken = null!;
        private Label _tokenStatus = null!;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            BuildUi();
            LoadValues();
        }

        private void BuildUi()
        {
            Text = "Persidera Time Tracker — Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            ClientSize = new Size(440, 360);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(12),
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 180F),
                    new ColumnStyle(SizeType.Percent, 100F)
                }
            };

            // Hourly rate
            _hourlyRate = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100000,
                Increment = 1,
                Width = 200,
                Anchor = AnchorStyles.Left
            };
            AddRow(layout, "Hourly Rate ($/hr):", _hourlyRate);

            // Billing day 1
            _billingDay1 = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 28,
                Width = 200,
                Anchor = AnchorStyles.Left
            };
            AddRow(layout, "Billing Day 1 (1-28):", _billingDay1);

            // Billing day 2
            _billingDay2 = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 28,
                Width = 200,
                Anchor = AnchorStyles.Left
            };
            AddRow(layout, "Billing Day 2 (1-28):", _billingDay2);

            // Mercury customer id
            _customerId = new TextBox { Width = 220, Anchor = AnchorStyles.Left };
            AddRow(layout, "Mercury Customer ID:", _customerId);

            // Mercury destination account id
            _destinationAccountId = new TextBox { Width = 220, Anchor = AnchorStyles.Left };
            AddRow(layout, "Mercury Destination Account ID:", _destinationAccountId);

            // ACH debit
            _achDebit = new CheckBox { Text = "Enable ACH debit", Anchor = AnchorStyles.Left, AutoSize = true };
            AddRow(layout, "Payment Options:", _achDebit);

            // Credit card
            _creditCard = new CheckBox { Text = "Enable credit card", Anchor = AnchorStyles.Left, AutoSize = true };
            AddRow(layout, "", _creditCard);

            // API token + save button
            _apiToken = new TextBox
            {
                Width = 220,
                UseSystemPasswordChar = true,
                Anchor = AnchorStyles.Left
            };
            var saveTokenButton = new Button { Text = "Save Token", AutoSize = true, Anchor = AnchorStyles.Left };
            saveTokenButton.Click += SaveTokenButton_Click;

            var tokenPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0) };
            tokenPanel.Controls.Add(_apiToken);
            tokenPanel.Controls.Add(saveTokenButton);
            AddRow(layout, "Mercury API Token:", tokenPanel);

            _tokenStatus = new Label { AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = Color.DimGray };
            AddRow(layout, "", _tokenStatus);

            Controls.Add(layout);

            // OK / Cancel
            var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
            var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            okButton.Click += OkButton_Click;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12),
                Height = 52
            };
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);
            Controls.Add(buttonPanel);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private static void AddRow(TableLayoutPanel layout, string label, Control control)
        {
            int row = layout.RowCount;
            layout.RowCount = row + 1;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 3)
            }, 0, row);
            control.Margin = new Padding(3, 5, 3, 3);
            layout.Controls.Add(control, 1, row);
        }

        private void LoadValues()
        {
            _hourlyRate.Value = ClampDecimal(_settings.HourlyRate, _hourlyRate.Minimum, _hourlyRate.Maximum);
            _billingDay1.Value = Clamp(_settings.BillingDay1, 1, 28);
            _billingDay2.Value = Clamp(_settings.BillingDay2, 1, 28);
            _customerId.Text = _settings.MercuryCustomerId;
            _destinationAccountId.Text = _settings.MercuryDestinationAccountId;
            _achDebit.Checked = _settings.AchDebitEnabled;
            _creditCard.Checked = _settings.CreditCardEnabled;
            RefreshTokenStatus();
        }

        private void RefreshTokenStatus()
        {
            bool configured;
            try
            {
                configured = CredentialManager.HasToken();
            }
            catch (Exception)
            {
                configured = false;
            }

            _tokenStatus.Text = configured
                ? "A Mercury API token is stored in Windows Credential Manager."
                : "No Mercury API token stored yet.";
            _tokenStatus.ForeColor = configured ? Color.ForestGreen : Color.DimGray;
        }

        private void SaveTokenButton_Click(object? sender, EventArgs e)
        {
            string token = _apiToken.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show(this, "Please enter a token before saving.", "Mercury API Token",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                CredentialManager.SaveToken(token);
                _apiToken.Clear();
                RefreshTokenStatus();
                MessageBox.Show(this, "Mercury API token saved to Windows Credential Manager.", "Mercury API Token",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Mercury API Token",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _settings.HourlyRate = _hourlyRate.Value;
            _settings.BillingDay1 = (int)_billingDay1.Value;
            _settings.BillingDay2 = (int)_billingDay2.Value;
            _settings.MercuryCustomerId = _customerId.Text.Trim();
            _settings.MercuryDestinationAccountId = _destinationAccountId.Text.Trim();
            _settings.AchDebitEnabled = _achDebit.Checked;
            _settings.CreditCardEnabled = _creditCard.Checked;

            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save settings: " + ex.Message, "Settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        private static decimal ClampDecimal(decimal value, decimal min, decimal max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
