using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using FA2.Classes;

namespace FA2.Converters
{
    internal class IdToFactoryConverter : IValueConverter
    {
        private CatalogClass _cc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string factoryName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                var custView = new DataView(_cc.FactoriesDataTable, "", "FactoryID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    factoryName = foundRows[0].Row["FactoryName"].ToString();
                }
            }

            return factoryName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
