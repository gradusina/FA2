using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using FA2.XamlFiles;

namespace FA2.Converters
{
    class AbsenceTypeConverter : IMultiValueConverter
    {
        /// <summary>
        /// parameter:
        /// Hours
        /// Color
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="values"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
 
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0].ToString() == "{DependencyProperty.UnsetValue}" ||
                values[1].ToString() == "{DependencyProperty.UnsetValue}")
                return null;

            var param = parameter.ToString();

            decimal dayHours;
            var dayHoursString = values[0].ToString();

            var abscenceTypeId = values[1].ToString() != "" ? System.Convert.ToInt32(values[1]) : 1;

            var result = Decimal.TryParse(dayHoursString, out dayHours);
            
            //if (!result) absencesSymbol = true;

            if (TimesheetPage._tsc == null) return null;

            var custView = new DataView(TimesheetPage._tsc.AbsencesTypesDataTable, "", "AbsencesTypeID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(abscenceTypeId);
                
            if (!foundRows.Any()) return null;
                
            switch (param)
            {
                case "Hours":
                {
                    var considerInResult = System.Convert.ToBoolean(foundRows[0]["ConsiderInResult"]);
                    var сonsiderNorm = System.Convert.ToBoolean(foundRows[0]["ConsiderNorm"]);

                    if (considerInResult && !сonsiderNorm)
                        return dayHours.ToString(CultureInfo.InvariantCulture);

                    return foundRows[0]["AbsencesSymbol"].ToString();
                }
            }

            var color = new BrushConverter().ConvertFrom(foundRows[0]["Color"].ToString());

            return param == "Color" ? color : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
