using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FA2.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DBNull.Value) return null;

            try
            {
                return System.Convert.ToBoolean(value)
                    //? new BitmapImage(new Uri("pack://application:,,,/FAII_Launcher;component/Resources/alertTriangleRed.png"))
                    //: new BitmapImage(new Uri("pack://application:,,,/FAII_Launcher;component/Resources/okGreen.png"));
                    ? new BitmapImage(new Uri("pack://application:,,,/Resources/alertTriangleRed.png"))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/okGreen.png"));
            }
            catch ( Exception )
            {
                return null;
            }
        }




        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
   throw new NotImplementedException();
  }
 }
}
