using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToMachineNameConverter : IValueConverter
    {
        private CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string machineName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                var custView = new DataView(_cc.MachinesDataTable, "", "MachineID",
                                                 DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    machineName = foundRows[0].Row["MachineName"].ToString();
                }
            }

            return machineName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
