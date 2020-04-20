using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace FA2.Converters
{
    class FilePathToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var path = value.ToString();

            if(parameter != null)
            switch (parameter.ToString())
            {
                case "WithoutExtension":
                    return Path.GetFileNameWithoutExtension(path);
                case "WithExtension":
                    return Path.GetFileName(path);
            }

            return Path.GetFileNameWithoutExtension(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
