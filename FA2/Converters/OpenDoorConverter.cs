using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class OpenDoorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string date = value.ToString();

            return "Дата закрытия: " + date;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
