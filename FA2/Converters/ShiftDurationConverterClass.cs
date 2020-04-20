using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class ShiftDurationConverterClass : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string result = "--:--";

            if (value == null) return result;

            DataRow currentRow = ((DataRowView) value).Row;

            if (System.Convert.ToBoolean(currentRow["DayEnd"]))
            {
                TimeSpan? dinnerTime = null;

                if (currentRow["DinnerTimeStart"] != DBNull.Value)
                {
                    if (currentRow["DinnerTimeEnd"] != DBNull.Value)
                    {
                        dinnerTime =
                            System.Convert.ToDateTime(currentRow["DinnerTimeEnd"]).Subtract(
                                System.Convert.ToDateTime(currentRow["DinnerTimeStart"]));
                    }
                }

                if (dinnerTime != null)
                {
                    TimeSpan resultS =
                        (System.Convert.ToDateTime(currentRow["WorkDayTimeEnd"]).Subtract(
                            System.Convert.ToDateTime(currentRow["WorkDayTimeStart"]))).
                            Subtract((TimeSpan)dinnerTime);

                    result = string.Format("{0:0}:{1:00}", Math.Truncate(resultS.TotalHours), resultS.Minutes) + " (" + System.Convert.ToDecimal(resultS.TotalHours).ToString("#.00") + ")";
                }
                else
                {
                    TimeSpan resultS =
                        System.Convert.ToDateTime(currentRow["WorkDayTimeEnd"]).Subtract(
                            System.Convert.ToDateTime(currentRow["WorkDayTimeStart"]));

                    result = string.Format("{0:0}:{1:00}", Math.Truncate(resultS.TotalHours), resultS.Minutes) + " (" + System.Convert.ToDecimal(resultS.TotalHours).ToString("0.00") + ")";
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.ParseExact((string)value, parameter.ToString(), culture);
        }

    }
}
