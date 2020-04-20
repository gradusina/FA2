using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class ServiceHistoryConverter : IValueConverter
    {
        private ServiceEquipmentClass _sec;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);

            if (_sec != null)
            {
                var custView = new DataView(_sec.ServiceJournal.Table, "", "ServiceJournalID",
                                                 DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0 && parameter != null)
                {
                    switch (parameter.ToString())
                    {
                        case "ActionName":
                            return foundRows[0]["ActionName"];
                        case "FactoryName":
                            var factoryId = foundRows[0]["FactoryID"];
                            return new IdToFactoryConverter().Convert(factoryId, typeof(string), null, new CultureInfo("ru-RU"));
                        case "MachineName":
                            var machineId = foundRows[0]["MachineID"];
                            return new IdToMachineNameConverter().Convert(machineId, typeof(string), null, new CultureInfo("ru-RU"));
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
