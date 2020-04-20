using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class PercentWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetCanvasWidth(System.Convert.ToDouble(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static double GetCanvasWidth(double value)
        {
            if (Math.Abs(value) < 1)
                return 10;

            return value*375/100;
        }
    }
}
