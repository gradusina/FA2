using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    internal class IdToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (System.Convert.ToInt32(value) == 1)
            {
                return new BrushConverter().ConvertFrom("#3f83f8");
            }

            if (System.Convert.ToInt32(value) == 2)
            {
                return new BrushConverter().ConvertFrom("#FF981D");
            }

            if (System.Convert.ToInt32(value) == 3)
            {
                return new BrushConverter().ConvertFrom("#0fa861");
            }

            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
