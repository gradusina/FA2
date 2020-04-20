using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class VerificationButtonConverter : IValueConverter
    {
        //private MainUserIDtoNameConverter _mainUserConverter = new MainUserIDtoNameConverter();
        private readonly IdToNameConverter _workerConverter = new IdToNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drv = value as DataRowView;

            switch (parameter.ToString())
            {

                // FirstVerification converter part
                case "FSVContent":
                    string fsvContent = "Подтвердить";
                    if (drv != null)
                    {
                        if (drv["FSVWorkerID"] != DBNull.Value && !string.IsNullOrEmpty(drv["FSVWorkerID"].ToString().Trim()))
                        {
                            fsvContent = _workerConverter.Convert(drv["FSVWorkerID"], "ShortName");
                        }
                    }
                    return fsvContent;
                    //break;

                case "FSVTag":
                    if (drv != null)
                    {
                        if (drv["FSVDate"] != DBNull.Value && !string.IsNullOrEmpty(drv["FSVDate"].ToString().Trim()))
                        {
                            return System.Convert.ToDateTime(drv["FSVDate"]).ToString("dd.MM.yy HH:mm");
                        }
                    }
                    return null;
                    //break;

                case "FSVEnable":
                    bool fsvEnable = false;
                    if (drv != null)
                    {
                        if (drv["FSVWorkerID"] == DBNull.Value || string.IsNullOrEmpty(drv["FSVWorkerID"].ToString().Trim()))
                            fsvEnable = true;
                    }
                    return fsvEnable;
                    //break;



                // SecondVerification converter part
                case "SSVContent":
                    string ssvContent = "Подтвердить";
                    if (drv != null)
                    {
                        if (drv["SSVWorkerID"] != DBNull.Value && !string.IsNullOrEmpty(drv["SSVWorkerID"].ToString().Trim()))
                        {
                            ssvContent = _workerConverter.Convert(drv["SSVWorkerID"], "ShortName");
                        }
                    }
                    return ssvContent;
                    //break;

                case "SSVTag":
                    if (drv != null)
                    {
                        if (drv["SSVDate"] != DBNull.Value && !string.IsNullOrEmpty(drv["SSVDate"].ToString().Trim()))
                        {
                            return System.Convert.ToDateTime(drv["SSVDate"]).ToString("dd.MM.yy HH:mm");
                        }
                    }
                    return null;
                    //break;

                case "SSVEnable":
                    bool ssvEnable = false;
                    if (drv != null)
                    {
                        if (drv["SSVWorkerID"] == DBNull.Value || string.IsNullOrEmpty(drv["SSVWorkerID"].ToString().Trim()))
                            ssvEnable = true;
                    }
                    return ssvEnable;
                    //break;



                // ThirdVerification converter part
                case "TSVContent":
                    string tsvContent = "Подтвердить";
                    if (drv != null)
                    {
                        if (drv["TSVWorkerID"] != DBNull.Value && !string.IsNullOrEmpty(drv["TSVWorkerID"].ToString().Trim()))
                        {
                            tsvContent = _workerConverter.Convert(drv["TSVWorkerID"], "ShortName");
                        }
                    }
                    return tsvContent;
                    //break;

                case "TSVTag":
                    if (drv != null)
                    {
                        if (drv["TSVDate"] != DBNull.Value && !string.IsNullOrEmpty(drv["TSVDate"].ToString().Trim()))
                        {
                            return System.Convert.ToDateTime(drv["TSVDate"]).ToString("dd.MM.yy HH:mm");
                        }
                    }
                    return null;
                    //break;

                case "TSVEnable":
                    bool tsvEnable = false;
                    if (drv != null)
                    {
                        if (drv["TSVWorkerID"] == DBNull.Value || string.IsNullOrEmpty(drv["TSVWorkerID"].ToString().Trim()))
                            tsvEnable = true;
                    }
                    return tsvEnable;
                    //break;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
