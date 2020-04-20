using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class OperationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = String.Empty;

            int type;

            int.TryParse(value.ToString(), out type);

            switch (type)
            {
                case 1:
                    result = "Основные";
                    break;
                case 2:
                    result = "Общие";
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}