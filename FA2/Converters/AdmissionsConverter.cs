using FA2.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FA2.Converters
{
    class AdmissionsConverter : IValueConverter
    {
        private static AdmissionsClass _admClass;
        private DateTime? _currentDate;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;
            App.BaseClass.GetAdmissionsClass(ref _admClass);

            switch (parameter.ToString())
            {
                case "AdmissionName":
                    {
                        var admissionId = System.Convert.ToInt32(value);
                        return GetAdmissionName(admissionId);
                    }
                case "HasWorkOperationAdmissions":
                    {
                        var workerAdmissionId = System.Convert.ToInt64(value);
                        return HasWorkOperationAdmissions(workerAdmissionId);
                    }
                case "WorkOperationWorkerAdmissions":
                    {
                        var workerAdmissionId = System.Convert.ToInt64(value);
                        return GetWorkOperaionWorkerAdmissions(workerAdmissionId);
                    }
                case "IsWorkerAdmissionHasEnded":
                    {
                        var workerAdmissionId = System.Convert.ToInt64(value);
                        return IsWorkerAdmissionHasEnded(workerAdmissionId);
                    }
                case "HasWorkerEndedAdmissions":
                    {
                        if (_currentDate == null)
                            _currentDate = App.BaseClass.GetDateFromSqlServer();

                        var workerId = System.Convert.ToInt64(value);

                        if (_admClass != null)
                        {
                            var hasWorkerEndedAdmissions = _admClass.HasWorkerEndedAdmissions(workerId, _currentDate.Value);
                            return hasWorkerEndedAdmissions;
                        }

                        return false;
                    }
            }

            return null;
        }


        public static string GetAdmissionName(int admissionId)
        {
            var custView = new DataView(_admClass.AdmissionsTable, "", "AdmissionID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(admissionId);

            if (!foundRows.Any()) return null;

            var admissionName = foundRows[0].Row["AdmissionName"].ToString();
            return admissionName;
        }

        public static bool HasWorkOperationAdmissions(long workerAdmissionId)
        {
            var workOperaionWorkerAdmissions = 
                _admClass.WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
            return workOperaionWorkerAdmissions.Any();
        }

        public static BindingListCollectionView GetWorkOperaionWorkerAdmissions(long workerAdmissionId)
        {
            var workOperaionWorkerAdmissions =
                _admClass.WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
            if (!workOperaionWorkerAdmissions.Any()) return null;

            var workOperaionWorkerAdmissionsView = new BindingListCollectionView(workOperaionWorkerAdmissions.CopyToDataTable().DefaultView);
            if (workOperaionWorkerAdmissionsView.GroupDescriptions != null)
                workOperaionWorkerAdmissionsView.GroupDescriptions.Add(new PropertyGroupDescription("WorkSubsectionID"));

            return workOperaionWorkerAdmissionsView;
        }

        public static bool IsWorkerAdmissionHasEnded(long workerAdmissionId)
        {
            var custView = new DataView(_admClass.WorkerAdmissionsTable, "", "WorkerAdmissionID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(workerAdmissionId);
            if (!foundRows.Any()) return true;

            var admissionId = System.Convert.ToInt32(foundRows[0].Row["AdmissionID"]);
            var admissionDate = System.Convert.ToDateTime(foundRows[0].Row["AdmissionDate"]);

            var admissionsView = new DataView(_admClass.AdmissionsTable, "", "AdmissionID", DataViewRowState.CurrentRows);
            var admissions = admissionsView.FindRows(admissionId);
            if (!admissions.Any()) return true;

            var admissionPeriodEnable = System.Convert.ToBoolean(admissions.First()["AdmissionPeriodEnable"]);
            if(admissionPeriodEnable)
            {
                var admissionPeriod = System.Convert.ToInt32(admissions.First()["AdmissionPeriod"]);
                var currentDate = App.BaseClass.GetDateFromSqlServer();

                return admissionDate < currentDate.Subtract(TimeSpan.FromDays(admissionPeriod));
            }

            return false;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
