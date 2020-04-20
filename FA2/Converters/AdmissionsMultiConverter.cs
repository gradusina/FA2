using FA2.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FA2.Converters
{
    class AdmissionsMultiConverter : IMultiValueConverter
    {
        private AdmissionsClass _admClass;
        private CatalogClass _catalogClass;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || parameter == null) return null;
            if (_admClass == null)
                App.BaseClass.GetAdmissionsClass(ref _admClass);
            if (_catalogClass == null)
                App.BaseClass.GetCatalogClass(ref _catalogClass);

            switch (parameter.ToString())
            {
                case "HasWorkerWorkOperationAdmission":
                    {
                        long workerId = 0;
                        var result = long.TryParse(values[0].ToString(), out workerId);
                        if (!result)
                            return false;

                        long workOperationId = 0;
                        result = long.TryParse(values[1].ToString(), out workOperationId);
                        if (!result)
                            return false;

                        return HasWorkerWorkOperationAdmission(workerId, workOperationId);
                    }
            }

            return null;
        }

        public bool HasWorkerWorkOperationAdmission(long workerId, long workOperationId)
        {
            var workOperationsView = new DataView(_catalogClass.WorkOperationsDataTable, "", "WorkOperationID", DataViewRowState.CurrentRows);
            var foundOperations = workOperationsView.FindRows(workOperationId);
            if (!foundOperations.Any()) return true;

            var operationTypeId = System.Convert.ToInt32(foundOperations.First()["OperationTypeID"]);
            if (operationTypeId != 1) return true;

            var workerWorkOperationAdmissions = _admClass.WorkOperationWorkerAdmissionsTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerID") == workerId &&
                r.Field<Int64>("WorkOperationID") == workOperationId);
            return workerWorkOperationAdmissions.Any();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
