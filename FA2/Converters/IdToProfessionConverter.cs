using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToProfessionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string professionName = null;
            StaffClass sc = null;

            App.BaseClass.GetStaffClass(ref sc);

            if(sc != null)
            {
                var custView = new DataView(sc.ProfessionsDataTable, "", "ProfessionID",
                                                 DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    professionName = foundRows[0].Row["ProfessionName"].ToString();
                }
            }
            return professionName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
