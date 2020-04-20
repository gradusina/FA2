using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FA2.XamlFiles;

namespace FA2.Converters
{
    public class WeekendDaysConverter : IMultiValueConverter
    {
        /// <summary>
        /// 0 - working day
        /// 1 - preholiday
        /// 2 - holiday
        /// 3 - weekend
        /// </summary>

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime day = System.Convert.ToDateTime(values[0]);

            //var tGrid = (Grid) values[1];
            var DayNumberTextBlock = values[1] as TextBlock;

            var background = (Brush)new BrushConverter().ConvertFrom("#FF017BCD");

            DayNumberTextBlock.Tag = 0;

            if (TimesheetPage._pcc != null)
            {
                DataView custView = new DataView(TimesheetPage._pcc.HolidaysDataTable, "", "Date",
                                                     DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(day.AddDays(1));
                if (foundRows.Count() != 0)
                {
                    background = (Brush)new BrushConverter().ConvertFrom("#FF0FA861");
                    DayNumberTextBlock.Tag = 1;
                }

                foundRows = custView.FindRows(day);
                if (foundRows.Count() != 0)
                {
                    background = (Brush)new BrushConverter().ConvertFrom("#FFC84545");
                    DayNumberTextBlock.Tag = 2;
                }
            }

            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                background = (Brush) new BrushConverter().ConvertFrom("#FFC84545");

                DayNumberTextBlock.Tag = 3;
            }
            return background;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
