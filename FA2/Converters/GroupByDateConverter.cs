using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class GroupByDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as DateTime?;
            if (v == null)
            {
                return value;
            }

            return v.Value.Date.ToString("dd.MM.yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
