using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToWorkOperationTypeConverter : IValueConverter
    {
        CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workOperationTypeName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                var custView = new DataView(_cc.WorkOperationsDataTable, "", "WorkOperationID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(value);

                if (foundRows.Count() != 0)
                {
                    int operationTypeID;
                    if (int.TryParse(foundRows[0].Row["OperationTypeID"].ToString(), out operationTypeID))
                    {
                        switch (operationTypeID)
                        {
                            case 1:
                                workOperationTypeName = "Основная";
                                break;
                            case 2:
                                workOperationTypeName = "Общая";
                                break;
                        }
                    }

                   
                }
            }
            return workOperationTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
