using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToActionNameConverter : IValueConverter
    {
        private AdministrationClass _adc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string actionName = null;

            App.BaseClass.GetAdministrationClass(ref _adc);


            if (_adc != null)
            {
                var custView = new DataView(_adc.ActionTypesTable, "", "ActionTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    actionName = foundRows[0].Row["ActionName"].ToString();
                }
            }

            return actionName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
