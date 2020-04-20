using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FA2.Converters
{
    class TimeIntervalCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() <= 1) return null;

            TimeSpan startTime;
            if(!TimeSpan.TryParse(values[0].ToString(), out startTime)) return null;

            TimeSpan stopTime;
            if (!TimeSpan.TryParse(values[1].ToString(), out stopTime)) return null;

            var interval = CalculateTimeInterval(startTime, stopTime);
            return interval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public static TimeSpan CalculateTimeInterval(TimeSpan startTime, TimeSpan stopTime)
        {
            TimeSpan timeInterval;

            if (stopTime >= startTime)
            {
                timeInterval = new TimeSpan(stopTime.Hours - startTime.Hours,
                    stopTime.Minutes - startTime.Minutes, 0);
            }
            else
            {
                timeInterval = new TimeSpan((24 - startTime.Hours) + stopTime.Hours,
                    stopTime.Minutes - startTime.Minutes, 0);
            }

            return timeInterval;
        }
    }
}
