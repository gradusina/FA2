using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    public class ConfirmCountConverter : IValueConverter
    {
        private ProdRoomsClass _prc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DBNull.Value) return 0;

            App.BaseClass.GetProdRoomsClass(ref _prc);
            var count = 0;
            var journalId = System.Convert.ToInt32(value);
            if (_prc != null)
            {
                var custView = new DataView(_prc.ConfirmDataTable, "", "JournalID",
                    DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(journalId);

                count = foundRows.Count();
            }
            return count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
