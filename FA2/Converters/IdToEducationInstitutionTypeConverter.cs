using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToEducationInstitutionTypeConverter : IValueConverter
    {
        private StaffClass _sc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string institutionType = null;

            App.BaseClass.GetStaffClass(ref _sc);

            if (_sc != null)
            {
                var custView = new DataView(_sc.EducationInstitutionTypesDataTable, "", "InstitutionTypeID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    institutionType = foundRows[0].Row["InstitutionTypeName"].ToString();
                }
            }

            return institutionType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
