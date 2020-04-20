using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToModuleNameConverter : IValueConverter
    {
        private AdministrationClass _adc;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            string moduleName = string.Empty;

            App.BaseClass.GetAdministrationClass(ref _adc);


            if (_adc != null)
            {
                var custView = new DataView(_adc.ModulesTable, "", "ModuleID",
                                                 DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    moduleName = foundRows[0].Row["ModuleName"].ToString();
                }
            }

            return moduleName;

        }

        public object Convert(object value)
        {
            if (value == null) return string.Empty;

            string moduleName = string.Empty;

            App.BaseClass.GetAdministrationClass(ref _adc);


            if (_adc != null)
            {
                var custView = new DataView(_adc.ModulesTable, "", "ModuleID",
                    DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    moduleName = foundRows[0].Row["ModuleName"].ToString();
                }
            }

            return moduleName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
