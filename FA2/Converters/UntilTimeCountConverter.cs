using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class UntilTimeCountConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateUntil = System.Convert.ToDateTime(value);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var timeSpan = dateUntil.Subtract(currentDate);

            if (parameter != null)
                switch (parameter.ToString())
                {
                    case "Days":
                        return timeSpan.Days;
                    case "Hours":
                        return timeSpan.Hours;
                }

            return timeSpan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
