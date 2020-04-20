using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    public class CompletionPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBackgroundBrush(System.Convert.ToDouble(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Brush GetBackgroundBrush(double value)
        {
            var color = new Color {A = 255, B = 97};
            if (value < 50)
            {
                color.R = 255;
                color.G = System.Convert.ToByte(168*value/50);
            }
            if (value >= 50)
            {
                color.R = System.Convert.ToByte(((100 - value)*255)/50);
                color.G = 168;
            }
            Brush backgroundBrush = new SolidColorBrush(color);

            return backgroundBrush;
        }
    }
}
