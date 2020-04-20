using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class OperationStructConverter : IValueConverter
    {
        private IdToFactoryConverter _idToFactoryConverter;
        private IdToWorkUnitConverter _idToWorkUnitConverter;
        private IdToWorkSectionConverter _idToWorkSectionConverter;
        private IdToWorkSubSectionConverter _idToWorkSubsectionConverter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = string.Empty;
            

            var drv = value as DataRowView;

            if (drv == null) return str;

            str = StructureItemName("FactoryName", drv["FactoryID"]) + " -" +
                  StructureItemName("WorkUnitName", drv["WorkUnitID"]) + " -" +
                  StructureItemName("WorkSectionName", drv["WorkSectionID"]) + " -" +
                  StructureItemName("WorkSubsectionName", drv["WorkSubsectionID"]);

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private string StructureItemName(string name, object id)
        {
            switch (name)
            {
                case "FactoryName":
                    if (_idToFactoryConverter == null)
                        _idToFactoryConverter = new IdToFactoryConverter();
                    return (string) _idToFactoryConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU"));
                case "WorkUnitName":
                    if (_idToWorkUnitConverter == null)
                        _idToWorkUnitConverter = new IdToWorkUnitConverter();
                    return (string) _idToWorkUnitConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU"));
                case "WorkSectionName":
                    if (_idToWorkSectionConverter == null)
                        _idToWorkSectionConverter = new IdToWorkSectionConverter();
                    return (string) _idToWorkSectionConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU"));
                case "WorkSubsectionName":
                    if (_idToWorkSubsectionConverter == null)
                        _idToWorkSubsectionConverter = new IdToWorkSubSectionConverter();
                    return (string) _idToWorkSubsectionConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU"));
            }
            return string.Empty;
        }
    }
}
