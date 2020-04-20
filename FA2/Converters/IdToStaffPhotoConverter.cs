using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToStaffPhotoConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var asyncTask = new AsyncTask();
            var thread = new Thread(() =>
                                    {
                                        BitmapImage resphoto = null;

                                        int workerId;
                                        var sucess = Int32.TryParse(value.ToString(), out workerId);
                                        if (sucess)
                                        {
                                            App.BaseClass.GetStaffClass(ref _sc);

                                            if (_sc != null)
                                            {
                                                var photo = _sc.GetObjectPhotoFromDataBase(workerId);

                                                if (photo != DBNull.Value)
                                                {
                                                    resphoto = AdministrationClass.ObjectToBitmapImage(photo);
                                                }
                                                else
                                                {
                                                    var bitmapImage = new BitmapImage();
                                                    bitmapImage.BeginInit();
                                                    bitmapImage.CacheOption = BitmapCacheOption.None;
                                                    bitmapImage.UriSource = new Uri("pack://application:,,,/Resources/user.png",
                                                        UriKind.Absolute);
                                                    bitmapImage.EndInit();
                                                    bitmapImage.Freeze();
                                                    resphoto = bitmapImage;
                                                }
                                            }
                                        }

                                        asyncTask.AsyncValue = resphoto;
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return asyncTask;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public class AsyncTask : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private object _asyncValue;

            public object AsyncValue
            {
                get { return _asyncValue; }
                set
                {
                    _asyncValue = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AsyncValue"));
                }
            }
        }
    }
}
