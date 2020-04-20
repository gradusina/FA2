using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class GroupHederConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value + " " + parameter;
            return !string.IsNullOrEmpty(s) ? s : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
