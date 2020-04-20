using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using FA2.Classes;

namespace FA2.Converters
{
    class TaskConverter : IValueConverter
    {
        private TaskClass _taskClass;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value) return null;
            if (parameter == null) return null;

            App.BaseClass.GetTaskClass(ref _taskClass);
            if (_taskClass == null) return null;

            switch (parameter.ToString())
            {
                case "PerformersCollection":
                {
                    var taskId = System.Convert.ToInt32(value);
                    var performersCollection =
                        _taskClass.Performers.Table.AsEnumerable().Where(p => p.Field<Int64>("TaskID") == taskId);
                    return performersCollection.Any()
                        ? performersCollection.AsDataView()
                        : new DataView();
                }
                case "TaskStatusName":
                {
                    var taskStatusId = System.Convert.ToInt32(value);
                    return GetTaskStatusName(taskStatusId);
                }
                case "TaskStatusColor":
                {
                    var taskStatusId = System.Convert.ToInt32(value);
                    return GetTaskStatusColor(taskStatusId);
                }
                case "TaskName":
                {
                    var taskId = System.Convert.ToInt32(value);
                    return GetTaskName(taskId);
                }
                case "TaskDescription":
                {
                    var taskId = System.Convert.ToInt32(value);
                    return GetTaskDescription(taskId);
                }
                case "TaskStatusNameByTaskID":
                {
                    var taskId = System.Convert.ToInt32(value);
                    return GetTaskStatusNameByTaskId(taskId);
                }
                case "TaskStatusColorByTaskID":
                {
                    var taskId = System.Convert.ToInt32(value);
                    return GetTaskStatusColorByTaskId(taskId);
                }
                case "PerformerStatusNameByPerformerID":
                {
                    var performerId = System.Convert.ToInt32(value);
                    return GetPerformerStatusNameByPerformerId(performerId);
                }
                case "PerformerStatusColorByPerformerID":
                {
                    var performerId = System.Convert.ToInt32(value);
                    return GetPerformerStatusColorByPerformerId(performerId);
                }
                case "TaskRowByTaskID":
                {
                    var taskId = System.Convert.ToInt32(value);
                    return GetTaskRowByTaskId(taskId);
                }
            }

            return null;
        }


        public static object GetTaskStatusName(int taskStatusId)
        {
            switch ((TaskClass.TaskStatuses)taskStatusId)
            {
                case TaskClass.TaskStatuses.NotStarted:
                    return "Не начата";
                case TaskClass.TaskStatuses.IsPerformed:
                    return "Выполняется";
                case TaskClass.TaskStatuses.IsCompleted:
                    return "Завершена";
            }

            return null;
        }

        public static object GetTaskStatusColor(int taskStatusId)
        {
            switch ((TaskClass.TaskStatuses)taskStatusId)
            {
                case TaskClass.TaskStatuses.NotStarted:
                    return new BrushConverter().ConvertFrom("#FFF57C00");
                case TaskClass.TaskStatuses.IsPerformed:
                    return new BrushConverter().ConvertFrom("#FF0FA861");
                case TaskClass.TaskStatuses.IsCompleted:
                    return new BrushConverter().ConvertFrom("#FF3366CC");
            }

            return null;
        }

        public object GetTaskName(int taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return null;

            var taskName = foundRows[0].Row["TaskName"].ToString();
            return taskName;
        }

        public object GetTaskDescription(int taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return null;

            var taskName = foundRows[0].Row["Description"].ToString();
            return taskName;
        }

        public object GetTaskStatusNameByTaskId(int taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return null;

            var taskStatusId = System.Convert.ToInt32(foundRows[0].Row["TaskStatusID"]);
            return GetTaskStatusName(taskStatusId);
        }

        public object GetTaskStatusColorByTaskId(int taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return null;

            var taskStatusId = System.Convert.ToInt32(foundRows[0].Row["TaskStatusID"]);
            return GetTaskStatusColor(taskStatusId);
        }

        public object GetPerformerStatusNameByPerformerId(int performerId)
        {
            var custView = new DataView(_taskClass.Performers.Table, "", "PerformerID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(performerId);

            if (!foundRows.Any()) return null;

            var taskStatusId = System.Convert.ToInt32(foundRows[0].Row["TaskStatusID"]);
            return GetTaskStatusName(taskStatusId);
        }

        public object GetPerformerStatusColorByPerformerId(int performerId)
        {
            var custView = new DataView(_taskClass.Performers.Table, "", "PerformerID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(performerId);

            if (!foundRows.Any()) return null;

            var taskStatusId = System.Convert.ToInt32(foundRows[0].Row["TaskStatusID"]);
            return GetTaskStatusColor(taskStatusId);
        }

        public object GetTaskRowByTaskId(int taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            if (!foundRows.Any()) return null;

            return foundRows.First();
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
