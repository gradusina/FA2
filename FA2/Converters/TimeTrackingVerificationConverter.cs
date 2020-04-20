using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    class TimeTrackingVerificationConverter : IValueConverter
    {
        private readonly IdToNameConverter _idToNameConverter = new IdToNameConverter();
        //private readonly MainUserIDtoNameConverter _mainUserIDtoNameConverter = new MainUserIDtoNameConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drv = value as DataRowView;
            if (drv == null) return null;

            if (parameter.ToString() == "FVColor")
            {
                if (drv["FSVWorkerID"] == DBNull.Value)
                    return new BrushConverter().ConvertFrom("#B9E82121");
                return new BrushConverter().ConvertFrom("#0fa861");
            }

            if (parameter.ToString() == "FVText")
            {
                if (drv["FSVWorkerID"] == DBNull.Value) return "Не подтвержденно";

                return _idToNameConverter.Convert(drv["FSVWorkerID"], "ShortName");
            }

            if (parameter.ToString() == "SVColor")
            {
                if (drv["SSVWorkerID"] == DBNull.Value)
                    return new BrushConverter().ConvertFrom("#B9E82121");
                return new BrushConverter().ConvertFrom("#0fa861");
            }

            if (parameter.ToString() == "SVText")
            {
                if (drv["SSVWorkerID"] == DBNull.Value) return "Не подтвержденно";

                return _idToNameConverter.Convert(drv["SSVWorkerID"], "ShortName");
            }


            if (parameter.ToString() == "TVColor")
            {
                if (drv["TSVWorkerID"] == DBNull.Value)
                    return new BrushConverter().ConvertFrom("#B9E82121");
                return new BrushConverter().ConvertFrom("#0fa861");
            }

            if (parameter.ToString() != "TVText") return null;

            if (drv["TSVWorkerID"] == DBNull.Value) return "Не подтвержденно";

            return _idToNameConverter.Convert(drv["TSVWorkerID"], "ShortName");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
