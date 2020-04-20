using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FA2.Converters
{
    public class NoRecordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value != null && value.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
