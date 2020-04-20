using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    public class IdToLayerGroupNameConverter : IValueConverter
    {

        private WorkshopMapClass _wmc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Общие";

            string layerGroupName = "Общие";

            int layerGroupId;

            bool sucess = Int32.TryParse(value.ToString(), out layerGroupId);

            if (sucess)
            {
                App.BaseClass.GetWorkshopMapClass(ref _wmc);

                if (_wmc != null)
                {
                    var custView = new DataView(_wmc.LayerGroupsDataTable, "", "LayerGroupID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(layerGroupId);

                    if (foundRows.Count() != 0)
                    {
                        layerGroupName = foundRows[0].Row["LayerGroupName"].ToString();
                    }
                }
            }

            return layerGroupName.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
