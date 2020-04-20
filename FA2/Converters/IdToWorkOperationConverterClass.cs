using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    internal class IdToWorkOperationConverter : IValueConverter
    {
        CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workOperationName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                var custView = new DataView(_cc.WorkOperationsDataTable, "", "WorkOperationID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    workOperationName = foundRows[0].Row["WorkOperationName"].ToString();
                }
            }
            return workOperationName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
