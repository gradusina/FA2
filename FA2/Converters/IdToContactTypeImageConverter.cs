using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using FA2.Classes;
using MySql.Data.MySqlClient;

namespace FA2.Converters
{
    internal class IdToContactTypeImageConverter : IValueConverter
    {
        private StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            BitmapImage contactImage = new BitmapImage();

            int contactTypeID;
            bool sucess = Int32.TryParse(value.ToString(), out contactTypeID);
            if (sucess)
            {
                App.BaseClass.GetStaffClass(ref _sc);

                if (_sc != null)
                {
                    var sqlCon = new MySqlConnection { ConnectionString = App.ConnectionInfo.ConnectionString};
                    sqlCon.Open();
                    var ds = new DataSet();
                    var sqa = new MySqlDataAdapter ("SELECT ContactImage FROM FAIIStaff.ContactTypes WHERE ContactTypeID= '" + contactTypeID + "'", sqlCon);
                    sqa.Fill(ds);
                    sqlCon.Close();

                    if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0] == DBNull.Value)
                    {
                        return contactImage;
                    }

                    using (var stream = new MemoryStream((byte[])ds.Tables[0].Rows[0][0]))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        contactImage = bitmap;
                    }
                }
            }
            return contactImage;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
