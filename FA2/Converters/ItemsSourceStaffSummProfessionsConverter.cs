using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    internal class ItemsSourceStaffSummProfessionsConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int workerID;

            if (Int32.TryParse(value.ToString(), out workerID))
            {
                App.BaseClass.GetStaffClass(ref _sc);

                DataView workerProfessionsDataView = _sc.GetWorkerProfessions();

                workerProfessionsDataView.RowFilter = "WorkerID=" + workerID;

                return workerProfessionsDataView;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

