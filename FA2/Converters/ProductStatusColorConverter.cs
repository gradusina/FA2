using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using FA2.Classes;

namespace FA2.Converters
{
    class ProductStatusColorConverter : IValueConverter
    {
        StaffClass _sc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value) return null;

            int prodStatusId = System.Convert.ToInt32(value);

            App.BaseClass.GetStaffClass(ref _sc);

            if (_sc != null)
            {
                var custView = new DataView(_sc.ProductionStatusesDataTable, "", "ProdStatusID",
                                            DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(prodStatusId);

                if (foundRows.Any())
                {
                    var prodStatusRow = foundRows.First();

                    switch (parameter.ToString())
                    {
                        case "Color":
                            {
                                if (prodStatusRow["ProdStatusColor"] != null && prodStatusRow["ProdStatusColor"] != DBNull.Value)
                                {
                                    var convertFrom = new BrushConverter().ConvertFrom(prodStatusRow["ProdStatusColor"]);
                                    if (convertFrom != null)
                                        return (Brush)convertFrom;
                                }
                                break;
                            }
                        case "Name":
                            {
                                return prodStatusRow["ProdStatusName"].ToString();
                            }
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
