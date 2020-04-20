using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class WorkerProffessionIdtoInfoConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int workerProfessionID;

            if (!Int32.TryParse(value.ToString(), out workerProfessionID)) return null;

            App.BaseClass.GetStaffClass(ref _sc);

            var custView = new DataView( _sc.GetWorkerProfessions().Table, "", "WorkerProfessionID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerProfessionID);

            return !foundRows.Any() ? null : foundRows[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}