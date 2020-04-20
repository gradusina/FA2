using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FA2.Converters
{
    public class BrigadierAccentConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString() == "FontWeight")
            {
                if ((value.ToString() != string.Empty) && (Convert.ToBoolean(value)))
                    return FontWeights.Medium;

                return FontWeights.Normal;
            }

            if (parameter.ToString() == "FontSize")
            {
                if ((value.ToString() != string.Empty) && (Convert.ToBoolean(value)))
                    return 15;

                return 14;
            }

            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
