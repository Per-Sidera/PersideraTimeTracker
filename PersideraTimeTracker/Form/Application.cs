using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PersideraTimeTracker.Model;
using PersideraTimeTracker.Properties;

namespace PersideraTimeTracker.Form
{
    /// <summary>
    /// Main application form. Rebuilt with a Persidera-branded dark theme:
    /// a large live timer, an ember "Start Tracking" call-to-action, a dark
    /// entry grid, and a status bar showing the current billing period value.
    /// </summary>
    public partial class Application : System.Windows.Forms.Form
    {
        const String FILE_EXT = "timetracker";
        const String FILE_NAME = "table";
        const int CATEGORY_MAXLENGTH = 255;

        private BindingList<TimeTrackerData> Data;
        private TrackingService TrackingService;
        private System.Windows.Forms.Timer RefreshTimer;
        private ToolTip toolTip = new ToolTip();
        private FileInfo file;
        private static readonly CultureInfo defaultCulture = CultureInfo.CurrentCulture;
        private bool isSaved = true;
        private AppSettings appSettings = AppSettings.Load();

        public Application()
        {
            Data = new BindingList<TimeTrackerData>();
            TrackingService = new TrackingService();

            RefreshTimer = new System.Windows.Forms.Timer { Interval = 100 };
            RefreshTimer.Tick += new System.EventHandler(RefreshTrackingInfo);

            InitializeComponent();

            // Load the window icon from the embedded clock.ico resource.
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using var iconStream = asm.GetManifestResourceStream("PersideraTimeTracker.Images.clock.ico");
                if (iconStream != null)
                {
                    this.Icon = new System.Drawing.Icon(iconStream);
                    this.notifyIcon.Icon = this.Icon;
                }
            }
            catch { /* icon is non-essential */ }

            this.dataGridViewMain.DataSource = Data;
            this.categoryComboBox.MaxLength = CATEGORY_MAXLENGTH;

            this.Refresh();
            Data.ListChanged += new ListChangedEventHandler(DataListChanged);
            RefreshTitle();
            RefreshTrackingButtons();
            RefreshEditButtons();
            RefreshStatistics();
            RefreshBillingStatus();
            LoadSettings();

            BuildLanguageSelection();

            this.Shown += Application_Shown;
        }

        /// <summary>
        /// After the form is shown, prompt to connect Mercury invoicing if it
        /// has not been configured yet (first-run wizard).
        /// </summary>
        private void Application_Shown(object sender, EventArgs e)
        {
            string token;
            try { token = CredentialManager.GetToken(); }
            catch { token = null; }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(appSettings.MercuryCustomerId))
            {
                using var setup = new MercurySetupForm(appSettings);
                setup.ShowDialog(this);
                appSettings = AppSettings.Load();
                RefreshBillingStatus();
            }
        }

        private void BuildLanguageSelection()
        {
            Dictionary<string, string> items = new Dictionary<string, string>(){
                {"none", Resources.Application_language_default },
                {"en",  "English" },
                {"cs",  "Čeština" },
            };

            foreach (var item in items)
            {
                ResourceManager resourceManager = new ResourceManager(typeof(Resources));
                string localizedLanguageName = resourceManager.GetString("Application_language_" + item.Key);
                var menuItem = new ToolStripMenuItem
                {
                    Text = localizedLanguageName == null ? item.Value : string.Format("{0} ({1})", localizedLanguageName, item.Value),
                    Checked = Settings.Default.language == item.Key,
                    ForeColor = Theme.Bone,
                    BackColor = Theme.InkElev,
                };
                menuItem.Click += (sender, e) => LanguageItemClicked(sender, e, item.Key);
                this.languageToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void LanguageItemClicked(object sender, EventArgs e, string language)
        {
            Settings.Default.language = language;
            if (!(sender is ToolStripMenuItem)) return;

            foreach (ToolStripMenuItem item in this.languageToolStripMenuItem.DropDownItems)
            {
                item.Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;

            MessageBox.Show(this, Resources.Application_languageChangedMessageBox_Message, Resources.Application_languageChangedMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void LoadSettings()
        {
            this.alwaysOnTopToolStripMenuItem.Checked = Settings.Default.alwaysOnTop;
            this.showInTaskbarToolStripMenuItem.Checked = Settings.Default.showInTaskbar;
            this.showInNotificationAreaToolStripMenuItem.Checked = Settings.Default.showInNotificationArea;

            this.alwaysOnTopToolStripMenuItem_CheckedChanged(null, null);
            this.showInTaskbarToolStripMenuItem_CheckedChanged(null, null);
            this.showInNotificationAreaToolStripMenuItem_CheckedChanged(null, null);
        }

        private void SaveSettings()
        {
            Settings.Default.alwaysOnTop = this.alwaysOnTopToolStripMenuItem.Checked;
            Settings.Default.showInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
            Settings.Default.showInNotificationArea = this.showInNotificationAreaToolStripMenuItem.Checked;
            Settings.Default.Save();
        }

        private void RefreshTitle()
        {
            var text = "Persidera Time Tracker";
            if (file != null && file.Exists && file.Name.Length > 0)
            {
                var modifier = isSaved ? "" : "*";
                text = String.Format("{1}{2} — {0}", text, file.Name, modifier);
            }
            this.Text = text;
            if (notifyIcon != null) notifyIcon.Text = text;
        }

        private void RefreshFileButtons()
        {
            bool saveAvailable = SaveAvailable();
            this.saveToolStripMenuItem.Enabled = saveAvailable;
            this.closeToolStripMenuItem.Enabled = file != null;
            this.saveAsToolStripMenuItem.Enabled = Data.Count > 0;
        }

        private void RefreshStatistics()
        {
            TimeSpan statTotal = Data.Sum(value => value.GetTimeElapsed());
            this.statsTotalText.Text = String.Format(Properties.Resources.Application_statsTotal_Text, statTotal.Format());

            DataGridView grid = this.dataGridViewMain;

            if (grid.SelectedRows.Count > 1)
            {
                TimeSpan statSelection = new TimeSpan();
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    var data = (TimeTrackerData)row.DataBoundItem;
                    statSelection = statSelection.Add(data.GetTimeElapsed());
                }
                this.statsSelectedText.Text = String.Format(Properties.Resources.Application_statsSelected_Text, grid.SelectedRows.Count, statSelection.Format());
                this.statsSelectedText.Visible = true;
            }
            else
            {
                this.statsSelectedText.Visible = false;
            }

            if (grid.SelectedRows.Count == 1)
            {
                var selected = (TimeTrackerData)grid.SelectedRows[0].DataBoundItem;
                var category = selected.Category;
                TimeSpan statCategory = Data.Where(value => category == null ? value.Category == null : value.Category != null && value.Category.Equals(category)).Sum(value => value.GetTimeElapsed());
                this.statsCategoryText.Text = String.Format(Properties.Resources.Application_statsCategory_Text, category == null ? "" : category.Name, statCategory.Format());
                this.statsCategoryText.Visible = true;
            }
            else
            {
                this.statsCategoryText.Visible = false;
            }
        }

        private void DataListChanged(object sender, ListChangedEventArgs e)
        {
            isSaved = false;
            RefreshFileButtons();
            RefreshTitle();
            RefreshStatistics();
            RefreshBillingStatus();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void dataGridViewMain_SelectionChanged(object sender, EventArgs e)
        {
            RefreshEditButtons();
            RefreshStatistics();
        }

        private void RefreshEditButtons()
        {
            var count = this.dataGridViewMain.SelectedRows.Count;
            this.editEntryContextMenuItem.Enabled = count == 1;
            this.deleteEntryContextMenuItem.Enabled = count >= 1;
        }

        #region Tracking

        private void trackButton_Click(object sender, EventArgs e)
        {
            if (TrackingService.Tracking)
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
            TrackingService.Start();
            RefreshTimer.Start();
            RefreshTrackingButtons();
            this.timerLabel.Text = TrackingService.Elapsed;
        }

        private void StopTracking()
        {
            RefreshTimer.Stop();
            TimeTrackerData item = TrackingService.Stop();
            if (categoryComboBox.Text.Length > 0)
            {
                item.Category = new TrackedDataCategory(categoryComboBox.Text.Trim(' '));
            }
            Data.Add(item);
            RefreshTrackingButtons();
            RefreshCategoryPicker();
            this.timerLabel.Text = "00:00:00";
        }

        private void RefreshTrackingButtons()
        {
            bool tracking = TrackingService.Tracking;
            if (tracking)
            {
                this.trackButton.Text = "■  STOP TRACKING";
                this.trackButton.BackColor = Theme.Star;
                this.trackButton.ForeColor = Theme.Ink;
                this.trackButton.FlatAppearance.MouseOverBackColor = Theme.Star;
                this.timerLabel.ForeColor = Theme.Star;
            }
            else
            {
                this.trackButton.Text = "▶  START TRACKING";
                this.trackButton.BackColor = Theme.Ember;
                this.trackButton.ForeColor = Theme.Bone;
                this.trackButton.FlatAppearance.MouseOverBackColor = Theme.EmberHover;
                this.timerLabel.ForeColor = Theme.BoneMute;
            }
        }

        private void RefreshTrackingInfo(object sender, EventArgs e)
        {
            this.timerLabel.Text = TrackingService.Elapsed;
        }

        #endregion

        #region File operations

        private void Save(bool forceOpenSaveWindow = false)
        {
            if (forceOpenSaveWindow || file == null)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    DefaultExt = FILE_EXT,
                    FileName = "table",
                    Filter = String.Format("TimeTracker files (*.{0})|*.{0}|All files (*.*)|*.*", FILE_EXT),
                    InitialDirectory = file == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : file.DirectoryName
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    file = new FileInfo(dialog.FileName);
                }
            }

            if (file != null)
            {
                StreamWriter fs = null;
                try
                {
                    file.Delete();
                    fs = file.AppendText();
                    fs.Write(DataSerializer.Serialize(Data));
                }
                catch (Exception)
                {
                    MessageBox.Show(this, Properties.Resources.Application_fileErrorMessageBox_Message,
                        Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                finally
                {
                    fs?.Close();
                }
                isSaved = true;
            }

            RefreshFileButtons();
            RefreshTitle();
        }

        private void Open()
        {
            SaveIfNecessary();

            OpenFileDialog dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                DefaultExt = FILE_EXT,
                FileName = FILE_NAME,
                Filter = String.Format("TimeTracker files (*.{0})|*.{0}|All files (*.*)|*.*", FILE_EXT),
                InitialDirectory = file == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : file.DirectoryName
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                file = new FileInfo(dialog.FileName);
            }

            if (file != null)
            {
                if (!file.Exists)
                {
                    MessageBox.Show(this, Resources.Application_nonexistentFileMessageBox_Message,
                        Resources.Application_nonexistentFileMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StreamReader fs = null;
                try
                {
                    Data.Clear();
                    fs = file.OpenText();
                    string line;
                    while ((line = fs.ReadLine()) != null)
                    {
                        TimeTrackerData value = DataSerializer.DeserializeValue(line, CATEGORY_MAXLENGTH);
                        Data.Add(value);
                    }
                }
                catch (DeserializationException)
                {
                    MessageBox.Show(this, Resources.Application_fileErrorMessageBox_Message,
                        Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    file = null;
                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show(this, Resources.Application_fileErrorMessageBox_Message,
                        Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                finally
                {
                    fs?.Close();
                }
                isSaved = true;
            }

            RefreshFileButtons();
            RefreshTitle();
            RefreshCategoryPicker();
        }

        private DialogResult SaveIfNecessary()
        {
            if (SaveAvailable())
            {
                var result = MessageBox.Show(this, Properties.Resources.Application_unsavedMessageBox_Message,
                    Resources.Application_unsavedMessageBox_Caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    Save();
                }
                return result;
            }
            return DialogResult.Abort;
        }

        private bool SaveAvailable()
        {
            return !isSaved && Data.Count > 0;
        }

        private void Application_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SaveIfNecessary() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                SaveSettings();
            }
        }

        #endregion

        #region Categories

        private HashSet<TrackedDataCategory> GetUsedCategories()
        {
            var result = new HashSet<TrackedDataCategory>();
            foreach (var value in Data)
            {
                if (value.Category == null) continue;
                result.Add(value.Category);
            }
            return result;
        }

        private void RefreshCategoryPicker()
        {
            var current = this.categoryComboBox.Text;
            var items = this.categoryComboBox.Items;
            items.Clear();
            items.AddRange(GetUsedCategories().ToArray());
            this.categoryComboBox.Text = current;
        }

        private void addCategoryButton_Click(object sender, EventArgs e)
        {
            string name = this.categoryComboBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(this, "Type a category name in the box first, then click Add.",
                    "Add Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var cat = new TrackedDataCategory(name);
            if (!this.categoryComboBox.Items.Contains(cat))
            {
                this.categoryComboBox.Items.Add(cat);
            }
            this.categoryComboBox.Text = name;
        }

        private void categoryComboBox_TextUpdate(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            var text = box.Text;
            var original = text;

            Regex regex = new Regex("[^-_: \\w]");
            text = regex.Replace(text, "");

            if (original != text)
            {
                toolTip.Hide(this.categoryComboBox);
                toolTip.Show(String.Format(Resources.Application_categoryToolTip_Text, "-_: "), this.categoryComboBox, 5000);
            }

            if (text.Length > CATEGORY_MAXLENGTH)
            {
                text = text.Substring(0, CATEGORY_MAXLENGTH);
            }

            if (box.Text != text)
            {
                box.Text = text;
                box.SelectionStart = text.Length;
            }
        }

        #endregion

        #region Entry editing (double-click / context menu / delete key)

        private void dataGridViewMain_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EditEntry(this.dataGridViewMain.Rows[e.RowIndex]);
        }

        private void dataGridViewMain_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                var row = this.dataGridViewMain.Rows[e.RowIndex];
                if (!row.Selected)
                {
                    this.dataGridViewMain.ClearSelection();
                    row.Selected = true;
                }
            }
        }

        private void dataGridViewMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && this.dataGridViewMain.SelectedRows.Count > 0)
            {
                DeleteSelectedEntries();
                e.Handled = true;
            }
        }

        private void editEntryContextMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dataGridViewMain.SelectedRows.Count == 1)
            {
                EditEntry(this.dataGridViewMain.SelectedRows[0]);
            }
        }

        private void deleteEntryContextMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedEntries();
        }

        private void EditEntry(DataGridViewRow row)
        {
            if (row?.DataBoundItem is not TimeTrackerData entry) return;

            using var dialog = new EntryEditForm(entry);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                int index = Data.IndexOf(entry);
                if (index < 0) return;
                Data[index] = new TimeTrackerData(dialog.ResultStart, dialog.ResultEnd, dialog.ResultCategory);
                RefreshCategoryPicker();
                RefreshStatistics();
            }
        }

        private void DeleteSelectedEntries()
        {
            DataGridView grid = this.dataGridViewMain;
            var count = grid.SelectedRows.Count;
            if (count < 1) return;

            var result = MessageBox.Show(this, String.Format(Properties.Resources.Application_deleteMessageBox_Message, count),
                Resources.Application_deleteMessageBox_Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow selectedRow in grid.SelectedRows)
                {
                    if (selectedRow.DataBoundItem is TimeTrackerData item)
                    {
                        Data.Remove(item);
                    }
                }
                RefreshCategoryPicker();
            }
        }

        #endregion

        #region File menu handlers

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveIfNecessary();
            Data.Clear();
            file = null;
            isSaved = true;
            RefreshFileButtons();
            RefreshEditButtons();
            RefreshTitle();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeToolStripMenuItem_Click(sender, e);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(true);
        }

        #endregion

        #region Empty-grid placeholder

        private void dataGridViewMain_Paint(object sender, PaintEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.Rows.Count == 0)
            {
                var font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
                System.Drawing.SizeF labelSize = e.Graphics.MeasureString(Resources.Application_noDataLabel, font);
                float vertPos = (grid.Width - labelSize.Width) / 2;
                float horizPos = (grid.Height + grid.ColumnHeadersHeight - labelSize.Height) / 2;
                using var brush = new System.Drawing.SolidBrush(Theme.BoneDim);
                e.Graphics.DrawString(Resources.Application_noDataLabel, font, brush,
                    new System.Drawing.PointF(vertPos < 0 ? 0 : vertPos, horizPos < grid.ColumnHeadersHeight ? grid.ColumnHeadersHeight : horizPos));
            }
        }

        private void dataGridViewMain_Resize(object sender, EventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.Rows.Count == 0)
            {
                grid.Invalidate();
            }
        }

        #endregion

        #region Options menu handlers

        private void alwaysOnTopToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        private void showInTaskbarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.ShowInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
        }

        private void showInNotificationAreaToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = this.showInNotificationAreaToolStripMenuItem.Checked;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        #endregion

        #region Mercury invoicing & billing periods

        private void RefreshBillingStatus()
        {
            try
            {
                var (start, end) = BillingPeriodService.GetCurrentPeriod(appSettings);
                var periodEntries = BillingPeriodService.GetEntriesForPeriod(Data, start, end);
                double hours = periodEntries.Sum(en => en.GetTimeElapsed().TotalHours);
                decimal value = MercuryInvoicingService.CalculateAmount(periodEntries, appSettings.HourlyRate);

                this.billingPeriodText.Text = string.Format("Period: {0:MMM d} – {1:MMM d}", start, end);
                this.statsTotalText.Text = string.Format("{0:0.00}h this period · ", hours)
                    + string.Format(Properties.Resources.Application_statsTotal_Text,
                        Data.Sum(en => en.GetTimeElapsed()).Format());
                this.statsValueText.Text = string.Format("{0:C}", value);
            }
            catch (Exception)
            {
                this.billingPeriodText.Text = "";
                this.statsValueText.Text = "";
            }
        }

        private void toolsButton_Click(object sender, EventArgs e)
        {
            this.toolsToolStripMenuItem.ShowDropDown();
        }

        private void viewPeriodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var (start, end) = BillingPeriodService.GetCurrentPeriod(appSettings);
            var periodEntries = new HashSet<TimeTrackerData>(BillingPeriodService.GetEntriesForPeriod(Data, start, end));

            this.dataGridViewMain.ClearSelection();
            int firstMatch = -1;
            foreach (DataGridViewRow row in this.dataGridViewMain.Rows)
            {
                if (row.DataBoundItem is TimeTrackerData item && periodEntries.Contains(item))
                {
                    row.Selected = true;
                    if (firstMatch < 0) firstMatch = row.Index;
                }
            }

            if (firstMatch >= 0)
            {
                this.dataGridViewMain.FirstDisplayedScrollingRowIndex = firstMatch;
            }
            else
            {
                MessageBox.Show(this,
                    string.Format("No entries in the current billing period ({0:yyyy-MM-dd} – {1:yyyy-MM-dd}).", start, end),
                    "Current Period", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new SettingsForm(appSettings))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    appSettings = AppSettings.Load();
                    RefreshBillingStatus();
                }
            }
        }

        private async void createInvoiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var (start, end) = BillingPeriodService.GetCurrentPeriod(appSettings);
            var periodEntries = BillingPeriodService.GetEntriesForPeriod(Data, start, end);

            if (periodEntries.Count == 0)
            {
                MessageBox.Show(this,
                    string.Format("There are no tracked entries in the current billing period ({0:yyyy-MM-dd} – {1:yyyy-MM-dd}).", start, end),
                    "Create Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool tokenConfigured;
            try { tokenConfigured = CredentialManager.HasToken(); }
            catch { tokenConfigured = false; }

            bool idsConfigured = !string.IsNullOrWhiteSpace(appSettings.MercuryCustomerId)
                && !string.IsNullOrWhiteSpace(appSettings.MercuryDestinationAccountId);

            if (!tokenConfigured || !idsConfigured)
            {
                MessageBox.Show(this,
                    "Mercury invoicing is not configured.\r\n\r\n" +
                    "Open Tools > Settings and provide the Mercury API token, customer ID and destination account ID.",
                    "Create Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double hours = periodEntries.Sum(en => en.GetTimeElapsed().TotalHours);
            decimal total = MercuryInvoicingService.CalculateAmount(periodEntries, appSettings.HourlyRate);

            var confirm = MessageBox.Show(this,
                string.Format(
                    "Create invoice for {0:0.00}h @ {1:C}/hr = {2:C} for period {3:yyyy-MM-dd} to {4:yyyy-MM-dd}?",
                    hours, appSettings.HourlyRate, total, start, end),
                "Create Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            this.createInvoiceToolStripMenuItem.Enabled = false;
            this.createInvoiceButton.Enabled = false;
            this.UseWaitCursor = true;
            try
            {
                var service = new MercuryInvoicingService(appSettings);
                DateTime invoiceDate = DateTime.Today;
                DateTime dueDate = invoiceDate.AddDays(14);
                string response = await service.CreateInvoiceAsync(periodEntries, invoiceDate, dueDate);

                MessageBox.Show(this,
                    string.Format("Invoice created successfully in Mercury.\r\n\r\nTotal: {0:C}", total),
                    "Create Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MercuryAccessDeniedException ex)
            {
                MessageBox.Show(this,
                    "Mercury declined the request (403 Forbidden).\r\n\r\n" +
                    "Invoicing requires an eligible Mercury subscription tier and a token with invoicing permissions.\r\n\r\n" +
                    ex.Message,
                    "Create Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to create the invoice:\r\n\r\n" + ex.Message,
                    "Create Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.UseWaitCursor = false;
                this.createInvoiceToolStripMenuItem.Enabled = true;
                this.createInvoiceButton.Enabled = true;
            }
        }

        #endregion
    }
}
