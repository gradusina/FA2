using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using FA2.XamlFiles;

namespace FA2.Converters
{
    public class DailyRateWorkingHoursConverter : IMultiValueConverter
    {
        /// <summary>
        /// 8 - working day
        /// 7 - preholiday
        /// 0 - holiday
        /// 0 - weekend
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) return "n";
            if (values[0] == null || values[0].ToString() == "" || values[1] == null || values[1].ToString() == "" ||
                values[2] == null || values[2].ToString() == "" || values[1].ToString() == "-1")
                return "n";

            int dayNumber = values[0].ToString() != "" ? System.Convert.ToInt32(values[0]) : 1;
            int month = values[1].ToString() != "" ? System.Convert.ToInt32(values[1]) : 1;
            int year = values[2].ToString() != "" ? System.Convert.ToInt32(values[2]) : 1;


            if (DateTime.DaysInMonth(year, month + 1) < dayNumber) return "n";


            int hours = 8;

            var day = new DateTime(year, month + 1, dayNumber);

            if (TimesheetPage._pcc != null)
            {
                var custView = new DataView(TimesheetPage._pcc.HolidaysDataTable, "", "Date",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(day.AddDays(1));

                if (foundRows.Count() != 0)
                    hours = 7;

                foundRows = custView.FindRows(day);

                if (foundRows.Count() != 0)
                {
                    if (parameter.ToString() == "Hours")
                        return "В";
                    if (parameter.ToString() == "Color")
                        return new BrushConverter().ConvertFrom("#FFDB6262");
                }
            }

            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                if (parameter.ToString() == "Hours")
                    return "В";
                if (parameter.ToString() == "Color")
                    return new BrushConverter().ConvertFrom("#FFDB6262");
            }

            if (parameter.ToString() == "Hours")
                return hours.ToString(CultureInfo.InvariantCulture);

            if (parameter.ToString() == "Color")
                return new BrushConverter().ConvertFrom("#FF001E4E");

            return null;
        }

        public object Convert(object[] values)
        {
            int dayNumber = System.Convert.ToInt32(values[0]);
            int month = System.Convert.ToInt32(values[1]);
            int year = System.Convert.ToInt32(values[2]);


            int hours = 8;

            var day = new DateTime(year, month, dayNumber);

            if (TimesheetPage._pcc != null)
            {
                var custView = new DataView(TimesheetPage._pcc.HolidaysDataTable, "", "Date",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(day.AddDays(1));
                if (foundRows.Count() != 0)
                {
                    // DayNumberTextBlock.Tag = 1;
                    hours = 7;
                }

                foundRows = custView.FindRows(day);
                if (foundRows.Count() != 0)
                {
                    return "В";
                }
            }

            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                // DayNumberTextBlock.Tag = 3;
                return "В";
            }

            return hours;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
