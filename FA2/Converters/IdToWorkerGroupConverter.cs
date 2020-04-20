using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToWorkerGroupConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workerGroupName = string.Empty;
            App.BaseClass.GetStaffClass(ref _sc);

            if (_sc != null)
            {
                var custView = new DataView(_sc.WorkerGroupsDataTable, "", "WorkerGroupID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    workerGroupName = foundRows[0].Row["WorkerGroupName"].ToString();
                }
            }

            return workerGroupName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
