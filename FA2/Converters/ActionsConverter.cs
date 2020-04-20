using FA2.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FA2.Converters
{
    public class ActionsConverter : IValueConverter
    {
        private ProdRoomsClass _prc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            switch (parameter.ToString())
            {
                case "ActionNumber":
                    {
                        int actionId;
                        Int32.TryParse(value.ToString(), out actionId);
                        var actionNumber = GetActionNumber(actionId);
                        return actionNumber;
                    }
                case "ActionText":
                    {
                        int actionId;
                        Int32.TryParse(value.ToString(), out actionId);
                        var actionText = GetActionText(actionId);
                        return actionText;
                    }
            }

            return null;
        }

        public int GetActionNumber(long actionId)
        {
            if (_prc == null)
                App.BaseClass.GetProdRoomsClass(ref _prc);

            var custView = new DataView(_prc.Actions.Table, "", "ActionID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(actionId);
            if (foundRows.Any())
            {
                var actionNumber = System.Convert.ToInt32(foundRows.First()["ActionNumber"]);
                return actionNumber;
            }

            return 0;
        }

        public string GetActionText(int actionId)
        {
            if (_prc == null)
                App.BaseClass.GetProdRoomsClass(ref _prc);

            var custView = new DataView(_prc.Actions.Table, "", "ActionID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(actionId);
            if (foundRows.Any())
            {
                var actionText = foundRows.First()["AtionText"].ToString();
                return actionText;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
