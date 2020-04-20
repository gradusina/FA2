using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class WorkingDaysCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int daysCount1 = values[0] != null ? System.Convert.ToInt32(values[0]) : 0;
            int daysCount2 = values[1] != null ? System.Convert.ToInt32(values[1]) : 0;

            if (daysCount1 == 0 || daysCount2 == 0)
            {
                int summ = System.Convert.ToInt32(values[0]) + System.Convert.ToInt32(values[1]);

                return summ.ToString(CultureInfo.InvariantCulture);
            }

            return (daysCount1 + daysCount2) + " (" + daysCount1 + "+" +
                   daysCount2 + ")";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}