using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    public class DeviationPlannedScheduleColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (App.BaseClass.DiffBetweenTwoDeviationRowsCollection.Count!=0)
            {
                return App.BaseClass.DiffBetweenTwoDeviationRowsCollection.Contains(Convert.ToInt32(value)) ? new BrushConverter().ConvertFrom("#ffe684") : Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
