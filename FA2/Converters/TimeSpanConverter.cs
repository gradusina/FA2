using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            long ticks;
            if (!Int64.TryParse(value.ToString(), out ticks) || parameter == null) return null;

            switch (parameter.ToString())
            {
                case "FromTicks":
                    return TimeSpan.FromTicks(ticks);
                case "FromSeconds":
                    return TimeSpan.FromSeconds(ticks);
                case "FromMinutes":
                    return TimeSpan.FromMinutes(ticks);
                case "FromHours":
                    return TimeSpan.FromHours(ticks);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
