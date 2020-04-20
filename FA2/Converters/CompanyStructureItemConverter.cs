using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class CompanyStructureItemConverter : IValueConverter
    {
        private StaffClass _sc;
        private IdToWorkerGroupConverter _idToWorkerGroupConverter = null;
        private IdToFactoryConverter _idToFactoryConverter = null;
        private IdToWorkUnitConverter _idToWorkUnitConverter = null;
        private IdToWorkSectionConverter _idToWorkSectionConverter = null;
        private IdToWorkSubSectionConverter _idToWorkSubsectionConverter = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = string.Empty;
            App.BaseClass.GetStaffClass(ref _sc);

            if (_sc == null) return str;


            var drv = value as DataRowView;

            if (drv == null) return str;

            switch (parameter.ToString())
            {
                case "MainText":
                    for(int i = 10; i >=6; i--)
                    {
                        if (System.Convert.ToInt32(drv.Row[i]) != -1)
                            return StructureItemName(i, drv.Row[i]);
                    }
                    break;
                case "AdditionalText":
                {
                    bool flag = false;

                    bool firstparent = false;

                    for (int i = 10; i >= 6; i--)
                    {
                        if (System.Convert.ToInt32(drv.Row[i]) != -1)
                        {
                            if (!firstparent)
                            {
                                firstparent = true;
                                continue;
                            }

                            if(flag)
                            {
                                str = StructureItemName(i, drv.Row[i]) + " - " + str;
                            }
                            else
                            {
                                str = StructureItemName(i, drv.Row[i]);
                                flag = true;
                            }
                        }
                    }
                }
                    break;

                case "SubordinationText":
                    switch (System.Convert.ToInt32(drv.Row[11]))
                    {
                        case 1:
                            return "Прямое подчинение";
                        case 2:
                            return "Косвенное подчинение";
                    }
                    break;
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private string StructureItemName(int index, object id)
        {
            switch (index)
            {
                case 6:
                    if (_idToWorkerGroupConverter == null)
                        _idToWorkerGroupConverter = new IdToWorkerGroupConverter();
                    return _idToWorkerGroupConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU")).ToString();
                case 7:
                    if (_idToFactoryConverter == null)
                        _idToFactoryConverter = new IdToFactoryConverter();
                    return _idToFactoryConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU")).ToString();
                case 8:
                    if (_idToWorkUnitConverter == null)
                        _idToWorkUnitConverter = new IdToWorkUnitConverter();
                    return _idToWorkUnitConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU")).ToString();
                case 9:
                    if (_idToWorkSectionConverter == null)
                        _idToWorkSectionConverter = new IdToWorkSectionConverter();
                    return _idToWorkSectionConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU")).ToString();
                case 10:
                    if (_idToWorkSubsectionConverter == null)
                        _idToWorkSubsectionConverter = new IdToWorkSubSectionConverter();
                    return _idToWorkSubsectionConverter.Convert(id, typeof(string), string.Empty, new CultureInfo("ru-RU")).ToString();
            }
            return string.Empty;
        }
    }
}
