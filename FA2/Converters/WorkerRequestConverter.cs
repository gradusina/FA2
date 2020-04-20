using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using FA2.Classes;
using System.Data;

namespace FA2.Converters
{
    class WorkerRequestConverter : IValueConverter
    {
        WorkerRequestsClass _workerRequestClass;

        public WorkerRequestConverter()
        {
            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestClass);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value) return null;
            if (parameter == null) return null;

            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestClass);
            if (_workerRequestClass == null) return null;

            switch (parameter.ToString())
            {
                case "RequestTypeName":
                    {
                        var requestTypeId = System.Convert.ToInt32(value);
                        return GetRequestTypeName(requestTypeId);
                    }
                case "SalarySaveTypeName":
                    {
                        var salarySaveTypeId = System.Convert.ToInt32(value);
                        return GetSalarySaveTypeName(salarySaveTypeId);
                    }
                case "InitiativeTypeName":
                    {
                        var initiativeTypeId = System.Convert.ToInt32(value);
                        return GetInitiativeTypeName(initiativeTypeId);
                    }
                case "IntervalTypeName":
                    {
                        var intervalTypeId = System.Convert.ToInt32(value);
                        return GetIntervalTypeName(intervalTypeId);
                    }
                case "WorkingOffTypeName":
                    {
                        var workingOffTypeId = System.Convert.ToInt32(value);
                        return GetWorkingOffTypeName(workingOffTypeId);
                    }
            }

            return null;
        }


        public object GetRequestTypeName(int requestTypeId)
        {
            var custView = new DataView(_workerRequestClass.RequestTypesTable, "", "RequestTypeID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(requestTypeId);

            if (!foundRows.Any()) return null;

            var requestTypeName = foundRows[0].Row["RequestTypeName"].ToString();
            return requestTypeName;
        }

        public object GetSalarySaveTypeName(int salarySaveTypeId)
        {
            var custView = new DataView(_workerRequestClass.SalatySaveTypesTable, "", "SalarySaveTypeID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(salarySaveTypeId);

            if (!foundRows.Any()) return null;

            var salarySaveTypeName = foundRows[0].Row["SalarySaveTypeName"].ToString();
            return salarySaveTypeName;
        }

        public object GetInitiativeTypeName(int initiativeTypeId)
        {
            var custView = new DataView(_workerRequestClass.InitiativeTypesTable, "", "InitiativeTypeID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(initiativeTypeId);

            if (!foundRows.Any()) return null;

            var initiativeTypeName = foundRows[0].Row["InitiativeTypeName"].ToString();
            return initiativeTypeName;
        }

        public object GetIntervalTypeName(int intervalTypeId)
        {
            var custView = new DataView(_workerRequestClass.IntervalTypesTable, "", "IntervalTypeID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(intervalTypeId);

            if (!foundRows.Any()) return null;

            var intervalTypeName = foundRows[0].Row["IntervalTypeName"].ToString();
            return intervalTypeName;
        }

        public object GetWorkingOffTypeName(int workingOffTypeId)
        {
            var custView = new DataView(_workerRequestClass.WorkingOffTypesTable, "", "WorkingOffTypeID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workingOffTypeId);

            if (!foundRows.Any()) return null;

            var workingOffTypeName = foundRows[0].Row["WorkingOffTypeName"].ToString();
            return workingOffTypeName;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
