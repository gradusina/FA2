using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class TimeSpanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            TimeSpan time;
            if (TimeSpan.TryParse(value.ToString(), out time))
            {
                var timeString = ConvertToString(time);
                return timeString;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public static string ConvertToString(TimeSpan time)
        {
            int hrs = System.Convert.ToInt32(Math.Truncate(time.TotalHours));
            int min = time.Minutes;
            int sec = time.Seconds;
            int days = time.Days;

            return string.Format("{0:00}:{1:00}{2}{3}", hrs, min, sec > 0 ? string.Format(":{0:00}", sec) : string.Empty,
                days > 0 ? string.Format(" ({0}д. {1}ч.)", days, time.Hours) : string.Empty);
        }
    }
}
