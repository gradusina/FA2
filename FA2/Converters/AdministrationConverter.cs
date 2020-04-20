using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class AdministrationConverter : IValueConverter
    {
        private AdministrationClass _admc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetAdministrationClass(ref _admc);

            if(_admc != null)
            {
                switch(parameter.ToString())
                {
                    case "WorkerAccessGroup":
                        var workerId = System.Convert.ToInt32(value);
                        return ConvertToWorkerAccessGroup(workerId);
                    case "AccessGroupName":
                        var accessGroupId = System.Convert.ToInt32(value);
                        return ConvertToAccessGroupName(accessGroupId);
                }
            }

            return null;
        }

        private string ConvertToAccessGroupName(int accessGroupId)
        {
            var custView = new DataView(_admc.AccessGroupsTable, "", "AccessGroupID",
                        DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(accessGroupId);

            if (foundRows.Count() != 0)
            {
                return foundRows[0]["AccessGroupName"].ToString();
            }

            return string.Empty;
        }

        private string ConvertToWorkerAccessGroup(int workerId)
        {
            var custView = new DataView(_admc.AccessGroupStructureTable, "", "WorkerID",
                        DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (foundRows.Count() != 0)
            {
                var accessGroupId = System.Convert.ToInt32(foundRows[0]["AccessGroupID"]);
                return ConvertToAccessGroupName(accessGroupId);
            }

            var workerAccess =
                _admc.WorkersAccessTable.AsEnumerable().Where(wA => wA.Field<Int64>("WorkerID") == workerId);
            return workerAccess.Any() ? "Пользовательская настройка" : "Нет группы";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
