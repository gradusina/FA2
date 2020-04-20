using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToWorkOperationGroupConverter : IValueConverter
    {
        CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string workOperationGroupName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                if (parameter != null && parameter.ToString() == "GroupID")
                {
                    var custView1 = new DataView(_cc.OperationGroupsDataTable, "", "OperationGroupID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows1 = custView1.FindRows(value);

                    if (foundRows1.Count() != 0)
                    {
                        workOperationGroupName = foundRows1[0].Row["OperationGroupName"].ToString();
                    }
                }
                else
                {


                    var custView = new DataView(_cc.WorkOperationsDataTable, "", "WorkOperationID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(value);

                    if (foundRows.Count() != 0)
                    {
                        int operationGroupId;
                        if (int.TryParse(foundRows[0].Row["OperationGroupID"].ToString(), out operationGroupId))
                        {
                            var custView1 = new DataView(_cc.OperationGroupsDataTable, "", "OperationGroupID",
                                DataViewRowState.CurrentRows);

                            DataRowView[] foundRows1 = custView1.FindRows(operationGroupId);

                            if (foundRows1.Count() != 0)
                            {
                                workOperationGroupName = foundRows1[0].Row["OperationGroupName"].ToString();
                            }
                        }


                    }
                }
            }
            return workOperationGroupName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}