using System;
using System.Globalization;
using System.Windows.Media;

namespace SpectrumAnalyzer.Converters
{
    public class ContributionPercentageToForegroundConverter : BaseValueConverter<ContributionPercentageToForegroundConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {          
            if (value is double)
            {
                //5% (0.05) = full red, 0% = black
                double redPercentage = (double)value / 0.05;

                if (redPercentage > 1)
                    redPercentage = 1;

                var c = Color.FromRgb((byte)(int)(redPercentage * 255), 0, 0);
                return new SolidColorBrush(c);
            }
            //value is not a double. Do not throw an exception
            return Brushes.Orange;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
