using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToDamageTypesConverter : IValueConverter
    {
        private ServiceEquipmentClass _sec;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string damageTypeName = null;
            App.BaseClass.GetServiceEquipmentClass(ref _sec);

            if (_sec != null)
            {
                var custView = new DataView(_sec.DamageTypes.Table, "", "DamageTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    damageTypeName = foundRows[0].Row["DamageTypeName"].ToString();
                }
            }

            return damageTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
