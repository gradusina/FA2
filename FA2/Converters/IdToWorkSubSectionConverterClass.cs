using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    internal class IdToWorkSubSectionConverter : IValueConverter
    {
        private CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workSubSectionName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                var custView = new DataView(_cc.WorkSubsectionsDataTable, "", "WorkSubSectionID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    workSubSectionName = foundRows[0].Row["WorkSubSectionName"].ToString();
                }
            }

            return workSubSectionName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
