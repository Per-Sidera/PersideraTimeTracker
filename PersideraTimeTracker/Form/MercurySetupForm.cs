using System;
using System.Drawing;
using System.Windows.Forms;

namespace PersideraTimeTracker.Form
{
    /// <summary>
    /// First-run wizard prompting the user to connect Mercury invoicing. Shown
    /// at startup when the API token or Mercury IDs are not yet configured.
    /// Saves the hourly rate / IDs / payment options into <see cref="AppSettings"/>
    /// and the API token into Windows Credential Manager.
    /// </summary>
    public class MercurySetupForm : System.Windows.Forms.Form
    {
        private readonly AppSettings _settings;

        private NumericUpDown _hourlyRate = null!;
        private TextBox _apiToken = null!;
        private Button _toggleToken = null!;
        private TextBox _customerId = null!;
        private TextBox _destinationAccountId = null!;
        private CheckBox _achDebit = null!;
        private CheckBox _creditCard = null!;

        public MercurySetupForm(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            BuildUi();
            LoadValues();
        }

        private void BuildUi()
        {
            Text = "Connect Mercury Invoicing";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            ClientSize = new Size(480, 470);
            BackColor = Theme.Ink;
            ForeColor = Theme.Bone;
            Font = Theme.FontBase;

            var header = new Label
            {
                Text = "Connect Mercury Invoicing",
                Font = Theme.FontLarge,
                ForeColor = Theme.Bone,
                AutoSize = true,
                Location = new Point(20, 18),
            };
            var subtitle = new Label
            {
                Text = "Set up once to auto-create invoices at billing periods.",
                Font = Theme.FontSmall,
                ForeColor = Theme.BoneMute,
                AutoSize = true,
                Location = new Point(22, 56),
            };
            Controls.Add(header);
            Controls.Add(subtitle);

            int y = 92;
            const int labelX = 22;
            const int fieldX = 22;
            const int fieldW = 436;

            // Hourly rate
            AddLabel("Hourly Rate ($/hr)", labelX, ref y);
            _hourlyRate = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100000,
                Value = 35,
                Location = new Point(fieldX, y),
                Width = 120,
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                BorderStyle = BorderStyle.FixedSingle,
            };
            Controls.Add(_hourlyRate);
            y += 38;

            // API token + eye toggle
            AddLabel("Mercury API Token", labelX, ref y);
            _apiToken = new TextBox
            {
                Location = new Point(fieldX, y),
                Width = fieldW - 44,
                UseSystemPasswordChar = true,
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _toggleToken = new Button
            {
                Text = "👁",
                Location = new Point(fieldX + fieldW - 40, y - 1),
                Size = new Size(40, 24),
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            _toggleToken.FlatAppearance.BorderColor = Theme.InkLine;
            _toggleToken.Click += (s, e) => _apiToken.UseSystemPasswordChar = !_apiToken.UseSystemPasswordChar;
            Controls.Add(_apiToken);
            Controls.Add(_toggleToken);
            y += 38;

            // Customer ID
            AddLabel("Mercury Customer ID", labelX, ref y);
            _customerId = MakePlaceholderBox("Paste from Mercury dashboard", fieldX, y, fieldW);
            Controls.Add(_customerId);
            y += 38;

            // Destination account ID
            AddLabel("Mercury Destination Account ID", labelX, ref y);
            _destinationAccountId = MakePlaceholderBox("Paste from Mercury dashboard", fieldX, y, fieldW);
            Controls.Add(_destinationAccountId);
            y += 42;

            // Payment options
            _achDebit = new CheckBox
            {
                Text = "Enable ACH debit on invoices",
                Checked = true,
                Location = new Point(fieldX, y),
                AutoSize = true,
                ForeColor = Theme.Bone,
            };
            Controls.Add(_achDebit);
            y += 26;
            _creditCard = new CheckBox
            {
                Text = "Enable credit card on invoices",
                Checked = false,
                Location = new Point(fieldX, y),
                AutoSize = true,
                ForeColor = Theme.Bone,
            };
            Controls.Add(_creditCard);

            // Buttons
            var save = new Button
            {
                Text = "Save & Continue",
                Size = new Size(140, 36),
                BackColor = Theme.Ember,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 158, ClientSize.Height - 50),
            };
            save.FlatAppearance.BorderSize = 0;
            save.FlatAppearance.MouseOverBackColor = Theme.EmberHover;
            save.Click += Save_Click;

            var skip = new Button
            {
                Text = "Skip for now",
                Size = new Size(110, 36),
                BackColor = Theme.InkElev,
                ForeColor = Theme.BoneMute,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 158 - 120, ClientSize.Height - 50),
            };
            skip.FlatAppearance.BorderColor = Theme.InkLine;
            skip.DialogResult = DialogResult.Cancel;

            Controls.Add(save);
            Controls.Add(skip);
            CancelButton = skip;
        }

        private void AddLabel(string text, int x, ref int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = Theme.FontSmall,
                ForeColor = Theme.BoneMute,
                AutoSize = true,
                Location = new Point(x, y),
            });
            y += 18;
        }

        private TextBox MakePlaceholderBox(string placeholder, int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                PlaceholderText = placeholder,
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                BorderStyle = BorderStyle.FixedSingle,
            };
        }

        private void LoadValues()
        {
            if (_settings.HourlyRate >= _hourlyRate.Minimum && _settings.HourlyRate <= _hourlyRate.Maximum)
            {
                _hourlyRate.Value = _settings.HourlyRate;
            }
            _customerId.Text = _settings.MercuryCustomerId;
            _destinationAccountId.Text = _settings.MercuryDestinationAccountId;
            _achDebit.Checked = _settings.AchDebitEnabled;
            _creditCard.Checked = _settings.CreditCardEnabled;
        }

        private void Save_Click(object? sender, EventArgs e)
        {
            string token = _apiToken.Text.Trim();
            string customer = _customerId.Text.Trim();
            string dest = _destinationAccountId.Text.Trim();

            // If a token was supplied, require the IDs too so invoicing actually works.
            if (!string.IsNullOrEmpty(token) &&
                (string.IsNullOrEmpty(customer) || string.IsNullOrEmpty(dest)))
            {
                MessageBox.Show(this,
                    "Please fill in the Mercury Customer ID and Destination Account ID, " +
                    "or leave the token blank and use \"Skip for now\".",
                    "Connect Mercury Invoicing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.HourlyRate = _hourlyRate.Value;
            _settings.MercuryCustomerId = customer;
            _settings.MercuryDestinationAccountId = dest;
            _settings.AchDebitEnabled = _achDebit.Checked;
            _settings.CreditCardEnabled = _creditCard.Checked;

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
                MessageBox.Show(this, "Failed to save Mercury setup:\r\n\r\n" + ex.Message,
                    "Connect Mercury Invoicing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
