using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class IdToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int statusID;

            if (!Int32.TryParse(value.ToString(), out statusID)) return null;


            if (parameter != null && parameter.ToString() == "FullName")
                switch (statusID)
                {
                    case 1:
                        return "Рабочий";
                    case 2:
                        return "Наставник";
                    case 3:
                        return "Ученик";
                    case 4:
                        return "Инноватор";
                }

            switch (statusID)
            {
                case 1:
                    return "Р";
                case 2:
                    return "Н";
                case 3:
                    return "У";
                case 4:
                    return "И";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
