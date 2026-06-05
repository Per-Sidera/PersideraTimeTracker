using System.Drawing;
using System.Windows.Forms;

namespace PersideraTimeTracker.Form
{
    partial class Application
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Controls referenced by Application.cs

        // Menu
        private MenuStrip mainMenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem1;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem1;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem languageToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private ToolStripMenuItem showInTaskbarToolStripMenuItem;
        private ToolStripMenuItem showInNotificationAreaToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem createInvoiceToolStripMenuItem;
        private ToolStripMenuItem viewPeriodToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem1;
        private ToolStripMenuItem aboutToolStripMenuItem1;

        // Timer panel
        private Panel timerPanel;
        private Label timerLabel;
        private Button trackButton;

        // Category row
        private ComboBox categoryComboBox;
        private Button addCategoryButton;

        // Grid
        private DataGridView dataGridViewMain;
        private DataGridViewTextBoxColumn DateStart;
        private DataGridViewTextBoxColumn EndDate;
        private DataGridViewTextBoxColumn TimeSpan;
        private DataGridViewTextBoxColumn CategoryName;
        private ContextMenuStrip gridContextMenu;
        private ToolStripMenuItem editEntryContextMenuItem;
        private ToolStripMenuItem deleteEntryContextMenuItem;

        // Status bar
        private Panel statusBar;
        private Label billingPeriodText;
        private Label statsTotalText;
        private Label statsValueText;
        private Label statsSelectedText;
        private Label statsCategoryText;
        private Button toolsButton;
        private Button createInvoiceButton;

        private NotifyIcon notifyIcon;

        #endregion

        #region Code-first themed layout

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ----- Form -----
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(680, 560);
            this.MinimumSize = new Size(560, 480);
            this.Text = "PERSIDERA TIME TRACKER";
            this.BackColor = Theme.Ink;
            this.ForeColor = Theme.Bone;
            this.Font = Theme.FontBase;
            this.Padding = new Padding(14, 0, 14, 0);

            BuildMenu();
            BuildTimerPanel();
            BuildCategoryRow();
            BuildGrid();
            BuildStatusBar();

            // notify icon
            this.notifyIcon = new NotifyIcon(this.components);
            this.notifyIcon.Text = "Persidera Time Tracker";
            this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);

            // Add in reverse z-order: docked controls fill remaining space last-added-first.
            this.Controls.Add(this.dataGridViewMain);     // Fill
            this.Controls.Add(this.categoryRow);          // Top (below timer)
            this.Controls.Add(this.timerPanel);           // Top
            this.Controls.Add(this.menuSpacer);           // Top (gap under menu)
            this.Controls.Add(this.statusBar);            // Bottom
            this.Controls.Add(this.mainMenuStrip);        // Top-most menu

            this.MainMenuStrip = this.mainMenuStrip;
            this.FormClosing += new FormClosingEventHandler(this.Application_FormClosing);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Holds the category combo + add button so it can dock as one strip.
        private Panel categoryRow;
        // Small spacer so content isn't flush against the menu bar.
        private Panel menuSpacer;

        private void BuildMenu()
        {
            this.mainMenuStrip = new MenuStrip
            {
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                Renderer = new DarkMenuRenderer(),
                Padding = new Padding(6, 2, 0, 2),
            };

            // File
            this.fileToolStripMenuItem1 = NewMenu("&File");
            this.newToolStripMenuItem = NewMenu("&New");
            this.openToolStripMenuItem = NewMenu("&Open...");
            this.saveToolStripMenuItem = NewMenu("&Save");
            this.saveAsToolStripMenuItem = NewMenu("Save &As...");
            this.closeToolStripMenuItem = NewMenu("&Close");
            this.exitToolStripMenuItem1 = NewMenu("E&xit");
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] {
                this.newToolStripMenuItem,
                this.openToolStripMenuItem,
                new ToolStripSeparator(),
                this.saveToolStripMenuItem,
                this.saveAsToolStripMenuItem,
                this.closeToolStripMenuItem,
                new ToolStripSeparator(),
                this.exitToolStripMenuItem1,
            });

            // Options
            this.optionsToolStripMenuItem = NewMenu("&Options");
            this.languageToolStripMenuItem = NewMenu("&Language");
            this.alwaysOnTopToolStripMenuItem = NewMenu("Always on &Top");
            this.showInTaskbarToolStripMenuItem = NewMenu("Show in &Taskbar");
            this.showInNotificationAreaToolStripMenuItem = NewMenu("Show in &Notification Area");
            this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
            this.showInTaskbarToolStripMenuItem.CheckOnClick = true;
            this.showInTaskbarToolStripMenuItem.Checked = true;
            this.showInNotificationAreaToolStripMenuItem.CheckOnClick = true;
            this.alwaysOnTopToolStripMenuItem.CheckedChanged += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_CheckedChanged);
            this.showInTaskbarToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showInTaskbarToolStripMenuItem_CheckedChanged);
            this.showInNotificationAreaToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showInNotificationAreaToolStripMenuItem_CheckedChanged);
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.languageToolStripMenuItem,
                new ToolStripSeparator(),
                this.alwaysOnTopToolStripMenuItem,
                this.showInTaskbarToolStripMenuItem,
                this.showInNotificationAreaToolStripMenuItem,
            });

            // Tools
            this.toolsToolStripMenuItem = NewMenu("&Tools");
            this.settingsToolStripMenuItem = NewMenu("&Settings...");
            this.createInvoiceToolStripMenuItem = NewMenu("Create &Invoice for Current Period...");
            this.viewPeriodToolStripMenuItem = NewMenu("&View Current Period Entries");
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            this.createInvoiceToolStripMenuItem.Click += new System.EventHandler(this.createInvoiceToolStripMenuItem_Click);
            this.viewPeriodToolStripMenuItem.Click += new System.EventHandler(this.viewPeriodToolStripMenuItem_Click);
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.settingsToolStripMenuItem,
                new ToolStripSeparator(),
                this.viewPeriodToolStripMenuItem,
                this.createInvoiceToolStripMenuItem,
            });

            // Help
            this.helpToolStripMenuItem1 = NewMenu("&Help");
            this.aboutToolStripMenuItem1 = NewMenu("&About");
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            this.helpToolStripMenuItem1.DropDownItems.Add(this.aboutToolStripMenuItem1);

            this.mainMenuStrip.Items.AddRange(new ToolStripItem[] {
                this.fileToolStripMenuItem1,
                this.optionsToolStripMenuItem,
                this.toolsToolStripMenuItem,
                this.helpToolStripMenuItem1,
            });

            this.menuSpacer = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.Ink };
        }

        private static ToolStripMenuItem NewMenu(string text)
        {
            return new ToolStripMenuItem(text)
            {
                ForeColor = Theme.Bone,
                BackColor = Theme.InkElev,
            };
        }

        private void BuildTimerPanel()
        {
            this.timerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Theme.InkElev,
                Padding = new Padding(0, 14, 0, 14),
            };
            this.timerPanel.Paint += PanelBorderPaint;

            this.timerLabel = new Label
            {
                Text = "00:00:00",
                Font = Theme.FontTimer,
                ForeColor = Theme.BoneMute,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Theme.InkElev,
            };

            this.trackButton = new Button
            {
                Text = "▶  START TRACKING",
                Font = Theme.FontMed,
                Size = new Size(220, 44),
                BackColor = Theme.Ember,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.None,
            };
            this.trackButton.FlatAppearance.BorderSize = 0;
            this.trackButton.FlatAppearance.MouseOverBackColor = Theme.EmberHover;
            this.trackButton.Click += new System.EventHandler(this.trackButton_Click);

            // Center the button horizontally inside a docked holder.
            var btnHolder = new Panel { Dock = DockStyle.Fill, BackColor = Theme.InkElev };
            btnHolder.Controls.Add(this.trackButton);
            btnHolder.Resize += (s, e) =>
            {
                this.trackButton.Left = (btnHolder.Width - this.trackButton.Width) / 2;
                this.trackButton.Top = (btnHolder.Height - this.trackButton.Height) / 2;
            };

            this.timerPanel.Controls.Add(btnHolder);
            this.timerPanel.Controls.Add(this.timerLabel);
        }

        private void BuildCategoryRow()
        {
            this.categoryRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Theme.Ink,
                Padding = new Padding(0, 12, 0, 8),
            };

            var lbl = new Label
            {
                Text = "Category",
                ForeColor = Theme.BoneMute,
                Font = Theme.FontSmall,
                AutoSize = true,
                Location = new Point(2, 0),
            };

            this.categoryComboBox = new ComboBox
            {
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontBase,
                Location = new Point(2, 18),
                Width = 360,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Sorted = true,
            };
            this.categoryComboBox.TextUpdate += new System.EventHandler(this.categoryComboBox_TextUpdate);

            this.addCategoryButton = MakeFlatButton("+  Add", 80);
            this.addCategoryButton.Location = new Point(370, 16);
            this.addCategoryButton.Click += new System.EventHandler(this.addCategoryButton_Click);

            this.categoryRow.Controls.Add(lbl);
            this.categoryRow.Controls.Add(this.categoryComboBox);
            this.categoryRow.Controls.Add(this.addCategoryButton);
        }

        private void BuildGrid()
        {
            this.dataGridViewMain = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Theme.Ink,
                GridColor = Theme.InkLine,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = true,
                RowHeadersVisible = false,
                ReadOnly = true,
                EditMode = DataGridViewEditMode.EditProgrammatically,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 28 },
            };

            this.dataGridViewMain.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                SelectionBackColor = Theme.InkLine,
                SelectionForeColor = Theme.Star,
                Font = Theme.FontBase,
                Padding = new Padding(6, 0, 6, 0),
            };
            this.dataGridViewMain.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.Ink,
                ForeColor = Theme.Bone,
                SelectionBackColor = Theme.InkLine,
                SelectionForeColor = Theme.Star,
            };
            this.dataGridViewMain.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.InkLine,
                ForeColor = Theme.BoneMute,
                SelectionBackColor = Theme.InkLine,
                SelectionForeColor = Theme.BoneMute,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 6, 0),
            };

            this.DateStart = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartTime",
                HeaderText = "DATE / START",
                Name = "DateStart",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd  HH:mm" },
            };
            this.EndDate = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndTime",
                HeaderText = "END",
                Name = "EndDate",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd  HH:mm" },
            };
            this.TimeSpan = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TimeElapsed",
                HeaderText = "DURATION",
                Name = "TimeSpan",
                ReadOnly = true,
                FillWeight = 70,
            };
            this.CategoryName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Category",
                HeaderText = "CATEGORY",
                Name = "CategoryName",
                ReadOnly = true,
            };
            this.dataGridViewMain.Columns.AddRange(new DataGridViewColumn[] {
                this.DateStart, this.EndDate, this.TimeSpan, this.CategoryName });

            this.dataGridViewMain.SelectionChanged += new System.EventHandler(this.dataGridViewMain_SelectionChanged);
            this.dataGridViewMain.Paint += new PaintEventHandler(this.dataGridViewMain_Paint);
            this.dataGridViewMain.Resize += new System.EventHandler(this.dataGridViewMain_Resize);
            this.dataGridViewMain.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridViewMain_CellDoubleClick);
            this.dataGridViewMain.KeyDown += new KeyEventHandler(this.dataGridViewMain_KeyDown);

            // Right-click context menu
            this.gridContextMenu = new ContextMenuStrip
            {
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                Renderer = new DarkMenuRenderer(),
            };
            this.editEntryContextMenuItem = NewMenu("Edit...");
            this.deleteEntryContextMenuItem = NewMenu("Delete");
            this.editEntryContextMenuItem.Click += new System.EventHandler(this.editEntryContextMenuItem_Click);
            this.deleteEntryContextMenuItem.Click += new System.EventHandler(this.deleteEntryContextMenuItem_Click);
            this.gridContextMenu.Items.AddRange(new ToolStripItem[] {
                this.editEntryContextMenuItem, this.deleteEntryContextMenuItem });
            this.dataGridViewMain.CellMouseDown += new DataGridViewCellMouseEventHandler(this.dataGridViewMain_CellMouseDown);
            this.dataGridViewMain.ContextMenuStrip = this.gridContextMenu;
        }

        private void BuildStatusBar()
        {
            this.statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 76,
                BackColor = Theme.InkElev,
                Padding = new Padding(2, 8, 2, 8),
            };
            this.statusBar.Paint += PanelBorderPaint;

            this.billingPeriodText = new Label
            {
                ForeColor = Theme.BoneMute,
                Font = Theme.FontSmall,
                AutoSize = true,
                Location = new Point(8, 8),
            };
            this.statsTotalText = new Label
            {
                ForeColor = Theme.Bone,
                Font = Theme.FontBase,
                AutoSize = true,
                Location = new Point(8, 26),
            };
            this.statsValueText = new Label
            {
                ForeColor = Theme.Star,
                Font = Theme.FontMed,
                AutoSize = true,
                Location = new Point(180, 24),
            };
            this.statsSelectedText = new Label
            {
                ForeColor = Theme.BoneMute,
                Font = Theme.FontSmall,
                AutoSize = true,
                Location = new Point(8, 46),
                Visible = false,
            };
            this.statsCategoryText = new Label
            {
                ForeColor = Theme.BoneMute,
                Font = Theme.FontSmall,
                AutoSize = true,
                Location = new Point(180, 46),
                Visible = false,
            };

            this.createInvoiceButton = new Button
            {
                Text = "Create Invoice",
                Font = Theme.FontBase,
                Size = new Size(130, 34),
                BackColor = Theme.Ember,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            this.createInvoiceButton.FlatAppearance.BorderSize = 0;
            this.createInvoiceButton.FlatAppearance.MouseOverBackColor = Theme.EmberHover;
            this.createInvoiceButton.Click += new System.EventHandler(this.createInvoiceToolStripMenuItem_Click);

            this.toolsButton = MakeFlatButton("Tools ▾", 90);
            this.toolsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.toolsButton.Click += new System.EventHandler(this.toolsButton_Click);

            this.statusBar.Controls.Add(this.billingPeriodText);
            this.statusBar.Controls.Add(this.statsTotalText);
            this.statusBar.Controls.Add(this.statsValueText);
            this.statusBar.Controls.Add(this.statsSelectedText);
            this.statusBar.Controls.Add(this.statsCategoryText);
            this.statusBar.Controls.Add(this.createInvoiceButton);
            this.statusBar.Controls.Add(this.toolsButton);

            this.statusBar.Resize += (s, e) =>
            {
                this.createInvoiceButton.Top = 20;
                this.createInvoiceButton.Left = this.statusBar.Width - this.createInvoiceButton.Width - 8;
                this.toolsButton.Top = 20;
                this.toolsButton.Left = this.createInvoiceButton.Left - this.toolsButton.Width - 8;
            };
        }

        /// <summary>Creates a flat dark button with an ink-line border.</summary>
        private static Button MakeFlatButton(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Font = Theme.FontBase,
                Size = new Size(width, 34),
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            b.FlatAppearance.BorderColor = Theme.InkLine;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Theme.InkHover;
            return b;
        }

        /// <summary>Draws a 1px top border line on elevated panels.</summary>
        private void PanelBorderPaint(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            using var pen = new Pen(Theme.InkLine);
            e.Graphics.DrawLine(pen, 0, 0, p.Width, 0);
        }

        #endregion
    }
}
