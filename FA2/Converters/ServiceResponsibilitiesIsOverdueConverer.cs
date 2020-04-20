using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class ServiceResponsibilitiesIsOverdueConverer : IValueConverter
    {
        private ServiceEquipmentClass _sec;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);
            string isOverdue = null;

            if (_sec != null)
            {
                var serviceResponsibleRow = (DataRowView)value;

                var timeEnd = serviceResponsibleRow["TimeEnd"];
                var serviceHistoryId = System.Convert.ToInt32(serviceResponsibleRow["ServiceHistoryID"]);

                var rows = _sec.ServiceHistory.Table.AsEnumerable().
                    Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);

                if (rows.Count() != 0)
                {
                    var historyRow = rows.First();
                    var overdue = System.Convert.ToBoolean(historyRow["IsOverdue"]);
                    var neededConfirmationDate = System.Convert.ToDateTime(historyRow["NeededConfirmationDate"]);
                    if(overdue)
                    {
                        if (timeEnd == DBNull.Value || System.Convert.ToDateTime(timeEnd) > neededConfirmationDate)
                            isOverdue = "Не выполнена вовремя";
                    }
                }
            }

            return isOverdue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
