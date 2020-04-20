using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    public class JournalNoteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brushConverter = new BrushConverter();
            Brush brush = Brushes.LightGray;
            var note = value.ToString();
            if (!string.IsNullOrEmpty(note))
                brush = brushConverter.ConvertFrom("#FF017BCD") as Brush; //#FFF88181
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
