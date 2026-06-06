using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PersideraTimeTracker.Views
{
    /// <summary>
    /// Maps a category name to a stable color (deterministic hash) for the
    /// category dot in each entry row.
    /// </summary>
    public class CategoryColorConverter : IValueConverter
    {
        private static readonly Color[] Palette =
        {
            (Color)ColorConverter.ConvertFromString("#FF6A1F"), // ember
            (Color)ColorConverter.ConvertFromString("#F2DC14"), // star
            (Color)ColorConverter.ConvertFromString("#4FB6FF"),
            (Color)ColorConverter.ConvertFromString("#7BD88F"),
            (Color)ColorConverter.ConvertFromString("#C792EA"),
            (Color)ColorConverter.ConvertFromString("#FF8AC2"),
            (Color)ColorConverter.ConvertFromString("#5CC7C0"),
            (Color)ColorConverter.ConvertFromString("#E0A458"),
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = value as string ?? "";
            int hash = 0;
            foreach (char c in name)
            {
                hash = unchecked(hash * 31 + c);
            }
            int idx = Math.Abs(hash) % Palette.Length;
            return new SolidColorBrush(Palette[idx]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// True -> Star (yellow), False -> BoneDim (grey). Used for the "$"
    /// billable indicator.
    /// </summary>
    public class BillableBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Billable =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2DC14"));
        private static readonly SolidColorBrush NotBillable =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A6A6A"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Billable : NotBillable;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
