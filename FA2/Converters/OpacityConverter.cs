using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, CultureInfo culture)
        {
            int sliderValue = System.Convert.ToInt32(value);

            return (100 - sliderValue) / 100d;
        }

        public object ConvertBack(object value, Type targetType,
               object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
