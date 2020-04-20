using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToResponsibleTypeConverter : IValueConverter
    {
        private WorkshopMapClass _wmc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string responsibleTypeName = null;

            App.BaseClass.GetWorkshopMapClass(ref _wmc);


            if (_wmc != null)
            {
                DataView custView = new DataView(_wmc.ResponsibleTypesDataTable, "", "ResponsibleTypeID",
                                                 DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    responsibleTypeName = foundRows[0].Row["ResponsibleName"].ToString();
                }
            }

            return responsibleTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
