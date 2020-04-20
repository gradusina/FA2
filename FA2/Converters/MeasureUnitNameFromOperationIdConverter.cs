using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class MeasureUnitNameFromOperationIdConverter : IValueConverter
    {
        //private OperationCatalogClass _occ;
        private CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string measureUnitName = null;

            //App.BaseClass.GetOperationCatalogClass(ref _occ);

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                if (parameter.ToString() == "MeasureUnitName")
                {

                    var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(value);

                    if (foundRows.Count() != 0)
                    {
                        if (foundRows[0].Row["MeasureUnitID"] != DBNull.Value)
                        {
                            var measureId = System.Convert.ToInt32(foundRows[0].Row["MeasureUnitID"]);
                            var view = new DataView(_cc.MeasureUnitsDataTable, "", "MeasureUnitID",
                                DataViewRowState.CurrentRows);
                            DataRowView[] rows = view.FindRows(measureId);

                            if (rows.Count() != 0)
                            {
                                measureUnitName = rows[0].Row["MeasureUnitName"].ToString();
                            }
                        }
                    }
                }
                else if (parameter.ToString() == "MeasureUnitName")
                {
                    var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(value);

                    if (foundRows.Count() != 0)
                    {
                        measureUnitName = foundRows[0].Row["Productivity"] != DBNull.Value
                            ? foundRows[0].Row["Productivity"].ToString()
                            : 0.ToString(CultureInfo.InvariantCulture);
                    } 
                }
            }

            return measureUnitName;
        }

        public object Convert(object value, object parameter)
        {
            string measureUnitName = null;

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc != null)
            {
                if (parameter.ToString() == "MeasureUnitName")
                {
                    var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(value);

                    if (foundRows.Count() != 0)
                    {
                        if (foundRows[0].Row["MeasureUnitID"] != DBNull.Value)
                        {
                            var measureId = System.Convert.ToInt32(foundRows[0].Row["MeasureUnitID"]);
                            var view = new DataView(_cc.MeasureUnitsDataTable, "", "MeasureUnitID",
                                DataViewRowState.CurrentRows);
                            DataRowView[] rows = view.FindRows(measureId);

                            if (rows.Count() != 0)
                            {
                                measureUnitName = rows[0].Row["MeasureUnitName"].ToString();
                            }
                        }
                    }
                }
                else if (parameter.ToString() == "Productivity")
                {
                    var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                        DataViewRowState.CurrentRows);

                    DataRowView[] foundRows = custView.FindRows(value);

                    if (foundRows.Count() != 0)
                    {
                        measureUnitName = foundRows[0].Row["Productivity"] != DBNull.Value
                            ? foundRows[0].Row["Productivity"].ToString()
                            : 0.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            return measureUnitName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
