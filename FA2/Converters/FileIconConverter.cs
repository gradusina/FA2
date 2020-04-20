using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FA2.Converters
{
    internal class FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return null;

            var path = value.ToString();

            var smallIcon = parameter != null && string.Equals("SmallIcon", parameter.ToString());
            var isDerectory = !Path.HasExtension(path);

            var icon = GetIcon(path, smallIcon, isDerectory);
            return icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #region Icons

        public static ImageSource GetIcon(string path, bool smallIcon, bool isDirectory)
        {
            if (path == "..")
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"pack://application:,,,/Resources/Files/FolderUp.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.None;
                bitmap.EndInit();
                return bitmap;
            }

            var flags = ShgfiIcon | ShgfiUsefileattributes;
            if (smallIcon)
                flags |= ShgfiSmallicon;

            var attributes = FileAttributeNormal;
            if (isDirectory)
                attributes |= FileAttributeDirectory;

            Shfileinfo shfi;
            if (0 != SHGetFileInfo(
                path,
                attributes,
                out shfi,
                (uint) Marshal.SizeOf(typeof (Shfileinfo)),
                flags))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    shfi.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Shfileinfo
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out Shfileinfo psfi,
            uint cbFileInfo, uint flags);

        private const uint FileAttributeDirectory = 0x00000010;
        private const uint FileAttributeNormal = 0x00000080;

        private const uint ShgfiIcon = 0x000000100; // get icon
        private const uint ShgfiSmallicon = 0x000000001; // get small icon
        private const uint ShgfiUsefileattributes = 0x000000010; // use passed dwFileAttribute

        #endregion
    }
}
