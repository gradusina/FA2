using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class NewsTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Convert(value, parameter);
            return result;
        }

        public static string Convert(object value, object patameter)
        {
            var newsText = value.ToString();
            var editInfoText = string.Empty;
            const string editMarker = "[Edit]";

            if (newsText.Contains(editMarker))
            {
                var newsEnd = newsText.IndexOf("[Edit]", 0, StringComparison.Ordinal);
                var editInfoStart = newsEnd + editMarker.Length;
                // Get edit info text
                editInfoText = newsText.Substring(editInfoStart, newsText.Length - editInfoStart);
                // Get news text without info data
                newsText = newsText.Substring(0, newsEnd);
            }

            switch (patameter.ToString())
            {
                case "JustText":
                    return newsText;
                case "EditInfoText":
                    return editInfoText;
            }

            return newsText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
