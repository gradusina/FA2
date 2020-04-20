using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    public class LockIDConverter : IValueConverter
    {
        private ProdRoomsClass _prc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lockName = Convert(value);
            return lockName;
        }

        public object Convert(object value)
        {
            var lockName = string.Empty;
            App.BaseClass.GetProdRoomsClass(ref _prc);

            int lockID;
            var sucess = Int32.TryParse(value.ToString(), out lockID);
            if (sucess)
                if (_prc != null)
                {
                    var custView = new DataView(_prc.Locks.Table, "", "LockID",
                        DataViewRowState.CurrentRows);

                    var foundRows = custView.FindRows(lockID);

                    if (foundRows.Count() != 0)
                    {
                        lockName = foundRows[0].Row["LockName"].ToString();
                    }
                }
            return lockName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
