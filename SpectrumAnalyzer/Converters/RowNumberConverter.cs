using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpectrumAnalyzer.Converters
{
    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow? row = value as DataGridRow;
            if (row == null)
                throw new InvalidOperationException("This converter class can only be used with DataGridRow elements.");

            return row.GetIndex() + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
