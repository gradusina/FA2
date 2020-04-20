using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    internal class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is string)
            {
                int intValue;
                var result = Int32.TryParse(value.ToString(), out intValue);
                if (result)
                {
                    return string.Format("{0} кБ", (intValue/1024).ToString(culture));
                }
            }
            else if (value is int)
            {
                return string.Format("{0} кБ", ((int) value/1024).ToString(culture));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
