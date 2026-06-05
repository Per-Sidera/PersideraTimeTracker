using System;
using System.Drawing;
using System.Windows.Forms;
using PersideraTimeTracker.Model;

namespace PersideraTimeTracker.Form
{
    /// <summary>
    /// Modal dark-themed editor for a single tracked time entry. Lets the user
    /// adjust the start/end date-time and the category. Because
    /// <see cref="TimeTrackerData"/> is immutable for its times, the caller
    /// reads <see cref="ResultStart"/>, <see cref="ResultEnd"/> and
    /// <see cref="ResultCategory"/> and constructs a replacement entry.
    /// </summary>
    public class EntryEditForm : System.Windows.Forms.Form
    {
        private DateTimePicker _startDate = null!;
        private DateTimePicker _startTime = null!;
        private DateTimePicker _endDate = null!;
        private DateTimePicker _endTime = null!;
        private TextBox _category = null!;

        public DateTimeOffset ResultStart { get; private set; }
        public DateTimeOffset ResultEnd { get; private set; }
        public TrackedDataCategory? ResultCategory { get; private set; }

        public EntryEditForm(TimeTrackerData entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            BuildUi();
            _startDate.Value = entry.StartTime.LocalDateTime;
            _startTime.Value = entry.StartTime.LocalDateTime;
            _endDate.Value = entry.EndTime.LocalDateTime;
            _endTime.Value = entry.EndTime.LocalDateTime;
            _category.Text = entry.Category?.Name ?? "";
        }

        private void BuildUi()
        {
            Text = "Edit Entry";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            ClientSize = new Size(380, 220);
            BackColor = Theme.Ink;
            ForeColor = Theme.Bone;
            Font = Theme.FontBase;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(14, 14, 14, 6),
                BackColor = Theme.Ink,
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            _startDate = MakeDatePicker(DateTimePickerFormat.Short);
            _startTime = MakeDatePicker(DateTimePickerFormat.Time);
            _startTime.ShowUpDown = true;
            AddRow(layout, "Start", _startDate, _startTime);

            _endDate = MakeDatePicker(DateTimePickerFormat.Short);
            _endTime = MakeDatePicker(DateTimePickerFormat.Time);
            _endTime.ShowUpDown = true;
            AddRow(layout, "End", _endDate, _endTime);

            _category = new TextBox
            {
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
            };
            var catLabel = new Label { Text = "Category", ForeColor = Theme.BoneMute, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
            int row = layout.RowCount;
            layout.RowCount = row + 1;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(catLabel, 0, row);
            layout.Controls.Add(_category, 1, row);
            layout.SetColumnSpan(_category, 2);

            Controls.Add(layout);

            var ok = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Width = 90,
                BackColor = Theme.Ember,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
            };
            ok.FlatAppearance.BorderSize = 0;
            ok.FlatAppearance.MouseOverBackColor = Theme.EmberHover;
            ok.Click += Ok_Click;

            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 90,
                BackColor = Theme.InkElev,
                ForeColor = Theme.Bone,
                FlatStyle = FlatStyle.Flat,
            };
            cancel.FlatAppearance.BorderColor = Theme.InkLine;

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(14),
                Height = 56,
                BackColor = Theme.Ink,
            };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            Controls.Add(buttons);

            AcceptButton = ok;
            CancelButton = cancel;
        }

        private static DateTimePicker MakeDatePicker(DateTimePickerFormat format)
        {
            return new DateTimePicker
            {
                Format = format,
                CalendarMonthBackground = Theme.InkElev,
                CalendarForeColor = Theme.Bone,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 120,
            };
        }

        private static void AddRow(TableLayoutPanel layout, string label, Control c1, Control c2)
        {
            int row = layout.RowCount;
            layout.RowCount = row + 1;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label
            {
                Text = label,
                ForeColor = Theme.BoneMute,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 3),
            }, 0, row);
            c1.Margin = new Padding(3, 5, 3, 3);
            c2.Margin = new Padding(3, 5, 3, 3);
            c1.Dock = DockStyle.Fill;
            c2.Dock = DockStyle.Fill;
            layout.Controls.Add(c1, 1, row);
            layout.Controls.Add(c2, 2, row);
        }

        private void Ok_Click(object? sender, EventArgs e)
        {
            DateTime start = _startDate.Value.Date + _startTime.Value.TimeOfDay;
            DateTime end = _endDate.Value.Date + _endTime.Value.TimeOfDay;

            if (end < start)
            {
                MessageBox.Show(this, "The end time must be on or after the start time.",
                    "Edit Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            ResultStart = new DateTimeOffset(start);
            ResultEnd = new DateTimeOffset(end);
            string cat = _category.Text.Trim();
            ResultCategory = string.IsNullOrEmpty(cat) ? null : new TrackedDataCategory(cat);
        }
    }
}
