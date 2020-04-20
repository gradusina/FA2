using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToAccessGroupNameConverter : IValueConverter
    {
        private AdministrationClass _admc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string accessGroupName = null;

            App.BaseClass.GetAdministrationClass(ref _admc);

            if (_admc != null)
            {
                var custView = new DataView(_admc.AccessGroupsTable, "", "AccessGroupID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    accessGroupName = foundRows[0].Row["AccessGroupName"].ToString();
                }
            }

            return accessGroupName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
