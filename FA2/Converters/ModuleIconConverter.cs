using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FA2.Converters
{
    class ModuleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int moduleID;

            object icon = null;

            if (int.TryParse(value.ToString(), out moduleID))
            {
                var resDic = new ResourceDictionary
                {
                    Source = new Uri("/FA2;component/Themes/Icons.xaml", UriKind.RelativeOrAbsolute)
                };

                switch (moduleID)
                {
                    case 1:
                        icon = resDic["ViewWorkersTimeStatIcon"];
                        break;
                    case 2:
                        icon =  resDic["WorkersTimeStatIcon"];
                        break;
                    case 3:
                        icon = resDic["WorkersIcon"];
                        break;
                    case 4:
                        icon = resDic["OperationCatalogIcon"];
                        break;
                    case 5:
                        icon = resDic["BrokenGearIcon"];
                        break;
                   case 7:
                        icon = resDic["DoorIcon"];
                        break;
                    case 8:
                        icon = resDic["TimesheetIcon"];
                        break;
                    case 9:
                        icon = resDic["WorkerStimIcon"];
                        break;
                    case 10:
                        icon = resDic["TechnologyProblemIcon"];
                        break;
                    case 12:
                        icon = resDic["ProductionScheduleIcon"];
                        break;
                    case 13:
                        icon = resDic["AdministrationIcon"];
                        break;
                    case 14:
                        icon = resDic["ProductionScheduleIcon"];
                        break;
                    case 15:
                        icon = resDic["WorkshopMapIcon"];
                        break;
                    case 16:
                        icon = resDic["TasksIcon"];
                        break;
                    case 17:
                        icon = resDic["OrdersIcon"];
                        break;
                }
            }

            return icon;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
