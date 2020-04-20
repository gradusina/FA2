using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FA2.Converters
{
    class TimeTrackingNotesVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Hidden;

            if (value == null) return visibility;

            if ((((DataRowView)value).Row["WorkerNotes"]).ToString().Trim() != string.Empty) visibility = Visibility.Visible;

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}