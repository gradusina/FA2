using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    internal class IdToWorkSectionConverter : IValueConverter
    {
        private CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workSectionName = string.Empty;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc == null) return workSectionName;

            var custView = new DataView(_cc.WorkSectionsDataTable, "", "WorkSectionID",
                DataViewRowState.CurrentRows);

            DataRowView[] foundRows = custView.FindRows(value);

            if (foundRows.Count() != 0)
            {
                workSectionName = foundRows[0].Row["WorkSectionName"].ToString();
            }

            return workSectionName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
