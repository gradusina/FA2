using FA2.Classes;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FA2.Converters
{
    public class LocksStatusConverter : IValueConverter
    {
        private ProdRoomsClass _prc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            switch(parameter.ToString())
            {
                case "IsLockClosed":
                    {
                        int lockId;
                        Int32.TryParse(value.ToString(), out lockId);
                        var isLockClosed = IsLockClosed(lockId);
                        return isLockClosed;
                    }
                case "LastSealNumber":
                    {
                        int lockId;
                        Int32.TryParse(value.ToString(), out lockId);
                        var lastSealNumber = GetLastSealNumber(lockId);
                        return lastSealNumber;
                    }
                case "LockPhoto":
                    {
                        int lockId;
                        Int32.TryParse(value.ToString(), out lockId);
                        var lockPhoto = GetLockPhoto(lockId);
                        return lockPhoto;
                    }
            }

            return null;
        }

        public bool IsLockClosed(int lockId)
        {
            if (_prc == null)
                App.BaseClass.GetProdRoomsClass(ref _prc);

            var custView = new DataView(_prc.JournalProductionsTable, "", "LockID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(lockId);

            if (foundRows.Count(r => r.Row.Field<Boolean>("Visible")) != 0)
            {
                var isClosed = System.Convert.ToBoolean(foundRows.Last(r => r.Row.Field<Boolean>("Visible"))["IsClosed"]);
                return isClosed;
            }

            return false;
        }

        public string GetLastSealNumber(int lockId)
        {
            if (_prc == null)
                App.BaseClass.GetProdRoomsClass(ref _prc);

            var custView = new DataView(_prc.JournalProductionsTable, "", "LockID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(lockId);

            if (foundRows.Count(r => System.Convert.ToInt32(r["LockStatusID"]) == 2 && r.Row.Field<Boolean>("Visible")) != 0)
            {
                var lastSealNumber = foundRows.Last(r => System.Convert.ToInt32(r["LockStatusID"]) == 2 && r.Row.Field<Boolean>("Visible"))["SealNumber"].ToString();
                return lastSealNumber;
            }

            return string.Empty;
        }

        public BitmapImage GetLockPhoto(int lockId)
        {
            var lockPoto = new BitmapImage();

            if (_prc == null)
                App.BaseClass.GetProdRoomsClass(ref _prc);

            var custView = new DataView(_prc.Locks.Table, "", "LockID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(lockId);

            if (foundRows.Count() != 0)
            {
                var lockPhotoData = foundRows.First()["LockPhoto"];
                if (lockPhotoData != null && lockPhotoData != DBNull.Value)
                {
                    using (var stream = new MemoryStream((byte[])lockPhotoData))
                    {
                        lockPoto.BeginInit();
                        lockPoto.StreamSource = stream;
                        lockPoto.CacheOption = BitmapCacheOption.OnLoad;
                        lockPoto.EndInit();
                    }

                    lockPoto.Freeze();
                    return lockPoto;
                }
            }

            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
