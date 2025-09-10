using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SpectrumAnalyzer.Converters
{
    /// <summary>
    /// A base value converter that allows direct xaml usage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T? mConverter = null; //a static single instance of this value converter

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return mConverter ?? (mConverter = new T());
        }


        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
