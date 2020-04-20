using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToMeasureUnitNameConverter : IValueConverter
    {
        //private OperationCatalogClass _occ;

        private CatalogClass _cc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string measureUnitName = string.Empty;

            int workOperationID;
            

            var sucess = Int32.TryParse(value.ToString(), out workOperationID);
            if (!sucess) return measureUnitName.Trim();

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc == null) return measureUnitName.Trim();


            var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                DataViewRowState.CurrentRows);

            DataRowView[] foundRows = custView.FindRows(workOperationID);

            if (foundRows.Count() != 0)
            {
                int measureUnitID;
                var sucess2 = Int32.TryParse(foundRows[0]["MeasureUnitID"].ToString(), out measureUnitID);
                if (!sucess2) return measureUnitName.Trim();

                var custView2 = new DataView(_cc.MeasureUnitsDataTable, "", "MeasureUnitID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows2 = custView2.FindRows(measureUnitID);

                if (foundRows2.Count() != 0)
                {
                    measureUnitName = foundRows2[0].Row["MeasureUnitName"].ToString();
                }
            }
            return measureUnitName.Trim();
        }

        public string Convert(object value)
        {
            if (value == null) return null;

            string measureUnitName = string.Empty;

            int workOperationID;


            var sucess = Int32.TryParse(value.ToString(), out workOperationID);
            if (!sucess) return measureUnitName.Trim();

            App.BaseClass.GetCatalogClass(ref _cc);

            if (_cc == null) return measureUnitName.Trim();


            var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                DataViewRowState.CurrentRows);

            DataRowView[] foundRows = custView.FindRows(workOperationID);

            if (foundRows.Count() != 0)
            {
                int measureUnitID;
                var sucess2 = Int32.TryParse(foundRows[0]["MeasureUnitID"].ToString(), out measureUnitID);
                if (!sucess2) return measureUnitName.Trim();

                var custView2 = new DataView(_cc.MeasureUnitsDataTable, "", "MeasureUnitID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows2 = custView2.FindRows(measureUnitID);

                if (foundRows2.Count() != 0)
                {
                    measureUnitName = foundRows2[0].Row["MeasureUnitName"].ToString();
                }
            }
            return measureUnitName.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
