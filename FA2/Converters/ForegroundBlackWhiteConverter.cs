using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace FA2.Converters
{
    public class ForegroundBlackWhiteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null)
            {
                string colorHex = value.ToString();

                string lightGrayHex = new ColorConverter().ConvertToString(Colors.LightGray);

                if (colorHex != "#00FFFFFF" && colorHex != "#FF017BCD" && colorHex != lightGrayHex)
                    return new BrushConverter().ConvertFrom("#FFFFFFFF");
            }

            return new BrushConverter().ConvertFrom("#FF444444");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
