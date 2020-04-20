using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToStaffAdressTypeConverter : IValueConverter
    {
        private StaffClass _sc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string staffAdressType = null;

            App.BaseClass.GetStaffClass(ref _sc);

            if (_sc != null)
            {
                var custView = new DataView(_sc.StaffAdressTypesDataTable, "", "StaffAdressTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    staffAdressType = foundRows[0].Row["StaffAdressTypeName"].ToString();
                }
            }

            return staffAdressType;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
