using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    public class IdToNameConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string name = null;

            int workerId;
            var sucess = Int32.TryParse(value.ToString(), out workerId);
            if (sucess)
            {
                App.BaseClass.GetStaffClass(ref _sc);

                if (_sc != null)
                {
                    var custView = new DataView(_sc.StaffPersonalInfoDataTable, "", "WorkerID",
                        DataViewRowState.CurrentRows);

                    var foundRows = custView.FindRows(workerId);

                    if (foundRows.Count() != 0)
                    {
                        if (parameter.ToString() == "FullName")
                            name = foundRows[0].Row["Name"].ToString();
                        if (parameter.ToString() == "ShortName")
                            name = GetShortName(foundRows[0].Row["Name"].ToString());
                    }
                }
            }
            return name;
        }

        public string Convert(object value, object parameter)
        {
            string name = string.Empty;

            int workerId;
            bool sucess = Int32.TryParse(value.ToString(), out workerId);
            if (sucess)
            {
                App.BaseClass.GetStaffClass(ref _sc);

                if (_sc != null)
                {
                    var custView = new DataView(_sc.StaffPersonalInfoDataTable, "", "WorkerID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(workerId);

                    if (foundRows.Count() != 0)
                    {
                        if (parameter.ToString() == "FullName")
                            name = foundRows[0].Row["Name"].ToString();
                        if (parameter.ToString() == "ShortName")
                            name = GetShortName(foundRows[0].Row["Name"].ToString());
                    }
                }
            }

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private string GetShortName(string fullName)
        {
            string shortName = string.Empty;
            string[] fio = fullName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            bool first = true;

            foreach (string s in fio.Where(s => s.Length > 1))
            {
                if (!first)
                    shortName += " " + s.Remove(1) + ".";
                else
                {
                    shortName += s;
                    first = false;
                }
            }

            return shortName;
        }
    }
}
