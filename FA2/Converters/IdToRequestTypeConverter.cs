using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToRequestTypeConverter : IValueConverter
    {
        private ServiceEquipmentClass _sec;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);

            string requestTypeName = null;

            if (_sec != null)
            {
                var custView = new DataView(_sec.RequestTypes.Table, "", "RequestTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    requestTypeName = foundRows[0].Row["RequestTypeName"].ToString();
                }
            }

            return requestTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
