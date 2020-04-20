using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    public class ActionGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var actionStatusID = System.Convert.ToInt32(value);
            return actionStatusID == 1
                ? "Инструкция ответственному лицу по открытию производственных помещений"
                : "Инструкция ответственному лицу по сдаче производственных помещений под охрану";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
