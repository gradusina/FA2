using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToTooltipConverter : IMultiValueConverter
    {
        private StaffClass _sc;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int statusID;

            if (!Int32.TryParse(values[0].ToString(), out statusID)) return null;

            switch (statusID)
            {
                case 1: return "Статус: Рабочий";
                case 2: return "Статус: Наставник";
                case 4:return "Статус: Инноватор";
                case 3:
                {
                    string name = null;

                    App.BaseClass.GetStaffClass(ref _sc);

                    var custView = new DataView(_sc.StaffPersonalInfoDataTable, "", "WorkerID", DataViewRowState.CurrentRows);

                    var foundRows = custView.FindRows(values[1]);

                    if (foundRows.Count() != 0) name = foundRows[0].Row["Name"].ToString();

                    return "Статус: Ученик\nНаставник: " + name;
                }
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
