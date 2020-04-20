using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != DBNull.Value)
            {
                try
                {
                    DateTime date = System.Convert.ToDateTime(value.ToString());

                    return date.ToString(parameter.ToString());
                }
                catch (Exception)
                {
                    return value;
                }
            }

            return string.Empty;
        }



        public object Convert(object value,  object parameter)
        {
            if (value != DBNull.Value)
            {
                try
                {
                    DateTime date = System.Convert.ToDateTime(value.ToString());

                    return date.ToString(parameter.ToString());
                }
                catch (Exception)
                {
                    return value;
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.ParseExact((string)value, parameter.ToString(), culture);
        }

    }
}
