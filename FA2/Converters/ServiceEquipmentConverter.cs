using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FA2.Converters
{
    class ServiceEquipmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            switch (parameter.ToString())
            {
                case "AdditionalRowVisibility":
                    return System.Convert.ToInt32(value) == 1 
                        ? Visibility.Visible 
                        : Visibility.Collapsed;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
