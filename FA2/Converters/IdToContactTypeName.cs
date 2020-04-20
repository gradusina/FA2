using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToContactTypeName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string contactTypeName = null;
            StaffClass sc = null;

            App.BaseClass.GetStaffClass(ref sc);

            if (sc != null)
            {
                var custView = new DataView(sc.ContactTypesDataTable, "", "ContactTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    contactTypeName = foundRows[0].Row["ContactTypeName"].ToString();
                }
            }
            return contactTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
