using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FA2.Converters
{
    internal class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var path = Directory.GetCurrentDirectory() + @"\" + value;
            if (!File.Exists(path)) return null;

            var uri = new Uri(path, UriKind.Absolute);

            var bitmapFrame = BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.None);

            if (parameter != null && parameter.ToString() == "SmallSize")
            {
                var thumbnail = bitmapFrame.Thumbnail;
                if (thumbnail != null) return thumbnail;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.DecodePixelHeight = 100;
                bitmapImage.DecodePixelWidth = 100;
                bitmapImage.CacheOption = BitmapCacheOption.None;
                bitmapImage.UriSource = uri;
                bitmapImage.EndInit();
                return bitmapImage;
            }

            return bitmapFrame;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private BitmapImage ResizeImage(Uri originalUri, int maxHeight, int maxWidth)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = originalUri;

            var bitmapFrame = BitmapFrame.Create(originalUri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);

            double originalWidth = bitmapFrame.PixelWidth;
            double originalHeight = bitmapFrame.PixelHeight;
            var newHeight = originalHeight < maxHeight
                ? originalHeight
                : maxHeight;
            var newWidth = newHeight * (originalWidth / originalHeight);
            newWidth = newWidth > maxWidth
                ? maxWidth
                : newWidth;
            newHeight = newWidth * (originalHeight / originalWidth);

            
            image.DecodePixelHeight = System.Convert.ToInt32(newHeight);
            image.DecodePixelWidth = System.Convert.ToInt32(newWidth);
            image.EndInit();

            return image;
        }
    }
}
