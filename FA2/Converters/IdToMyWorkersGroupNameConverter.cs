using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToMyWorkersGroupNameConverter : IValueConverter
    {
        private MyWorkersClass _mwc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string myWorkerGroupName = null;

            App.BaseClass.GetMyWorkersClass(ref _mwc);


            if (_mwc == null) return null;

            var custView = new DataView(_mwc.MyWorkersGroupsDataTable, "", "MyWorkersGroupID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(value);

            if (foundRows.Count() != 0)
            {
                myWorkerGroupName = foundRows[0].Row["MyWorkerGroupName"].ToString();
            }

            return myWorkerGroupName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
