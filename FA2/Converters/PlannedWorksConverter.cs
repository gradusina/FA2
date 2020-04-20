using FA2.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    class PlannedWorksConverter : IValueConverter
    {
        private PlannedWorksClass _plannedWorksClass;
        private TaskClass _taskClass;
        private BrushConverter _brushConverter;

        public PlannedWorksConverter()
        {
            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);
            App.BaseClass.GetTaskClass(ref _taskClass);
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value) return null;
            if (parameter == null) return null;

            if (_plannedWorksClass == null)
                App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);

            switch (parameter.ToString())
            {
                case "PlannedWorksTypeName":
                    {
                        var plannedWorksTypeId = System.Convert.ToInt32(value);
                        var plannedWorksTypeName = GetPlannedWorksTypeName(plannedWorksTypeId);
                        return plannedWorksTypeName;
                    }
                case "ConfirmationStatus":
                    {
                        var confirmationStatusId = System.Convert.ToInt32(value);
                        var confirmationStatus = GetConfirmationStatus(confirmationStatusId);
                        return confirmationStatus;
                    }
                case "ConfirmationStatusBrush":
                    {
                        var confirmationStatusId = System.Convert.ToInt32(value);
                        var confirmationStatusBrush = GetConfirmationStatusBrush(confirmationStatusId);
                        return confirmationStatusBrush;
                    }
                case "IsPlannedWorksInProcess":
                    {
                        var globalId = value.ToString();
                        var inProcess = GetIsPlannedWorksInProcess(globalId);
                        return inProcess;
                    }
                case "EmptyWorkReasonId":
                    {
                        var taskId = System.Convert.ToInt64(value);
                        var emptyWorkReasonId = GetEmptyWorkReasonId(taskId);
                        return emptyWorkReasonId;
                    }
                case "EmptyWorkReasonName":
                    {
                        var emptyWorkReasonId = System.Convert.ToInt32(value);
                        var emptyWorkReasonName = GetEmptyWorkReasonName(emptyWorkReasonId);
                        return emptyWorkReasonName;
                    }
            }

            return null;
        }

        public string GetPlannedWorksTypeName(int plannedWorksTypeId)
        {
            var custView = new DataView(_plannedWorksClass.PlannedWorksTypesTable, "", "PlannedWorksTypeID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(plannedWorksTypeId);

            if (!foundRows.Any()) return null;

            var plannedWorksTypeName = foundRows[0].Row["PlannedWorksTypeName"].ToString();
            return plannedWorksTypeName;
        }

        public string GetConfirmationStatus(int confirmationStatusId)
        {
            switch((ConfirmationStatus)confirmationStatusId)
            {
                case ConfirmationStatus.Confirmed:
                    return "Подтверждена";
                case ConfirmationStatus.Rejected:
                    return "Отклонена";
                case ConfirmationStatus.WaitingConfirmation:
                    return "Ждёт подтверждения";
            }

            return string.Empty;
        }

        public Brush GetConfirmationStatusBrush(int confirmationStatusId)
        {
            if (_brushConverter == null)
                _brushConverter = new BrushConverter();

            switch ((ConfirmationStatus)confirmationStatusId)
            {
                case ConfirmationStatus.Confirmed:
                    return _brushConverter.ConvertFrom("#FF4CAF50") as Brush;
                case ConfirmationStatus.Rejected:
                    return _brushConverter.ConvertFrom("#B9E82121") as Brush;
                case ConfirmationStatus.WaitingConfirmation:
                    return _brushConverter.ConvertFrom("#89000000") as Brush;
            }

            return Brushes.Transparent;
        }

        public bool GetIsPlannedWorksInProcess(string globalId)
        {
            if (_taskClass == null)
                App.BaseClass.GetTaskClass(ref _taskClass);

            var tasks = _taskClass.Tasks.Table.Select(string.Format("GlobalID = '{0}'", globalId));
            var inProcess = tasks.Any(t => !t.Field<Boolean>("IsComplete"));
            return inProcess;
        }

        public int GetEmptyWorkReasonId(long taskId)
        {
            var custView = new DataView(_plannedWorksClass.StartedPlannedWorksTable, "", "TaskID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return -1;

            var emptyWorkReasonId = System.Convert.ToInt32(foundRows[0]["EmptyWorkReasonID"]);
            return emptyWorkReasonId;
        }

        public string GetEmptyWorkReasonName(int emptyWorkReasonId)
        {
            var custView = new DataView(_plannedWorksClass.EmptyWorkReasonsTable, "", "EmptyWorkReasonID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(emptyWorkReasonId);

            if (!foundRows.Any()) return null;

            var emptyWorkReasonName = foundRows[0].Row["EmptyWorkReasonName"].ToString();
            return emptyWorkReasonName;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
