using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using FA2.Converters;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class TaskClass
    {
        public readonly Tasks Tasks;
        public readonly Performers Performers;
        public readonly Observers Observers;
        public readonly TaskTimeTracking TaskTimeTracking;

        private DateTime _dateFrom;
        private DateTime _dateTo;

        
        #region StaticEnums

        /// <summary>
        /// Sets work status of task.
        /// </summary>
        public enum TaskStatuses
        {
            NotStarted = 1,
            IsPerformed,
            IsCompleted
        }

        /// <summary>
        /// Sets type of application, that created task.
        /// </summary>
        public enum SenderApplications
        {
            ServiceDamage = 1,
            TechnologyProblem,
            ServiceJournal,
            Tasks,
            WorkerRequests,
            PlannedWorks
        }

        #endregion


        public TaskClass()
        {
            _dateFrom = DateTime.MinValue;
            _dateTo = DateTime.MinValue;

            Tasks = new Tasks();
            Performers = new Performers();
            Observers = new Observers();
            TaskTimeTracking = new TaskTimeTracking();
        }

        public TaskClass(DateTime dateFrom, DateTime dateTo, int currentWorkerId)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            Tasks = new Tasks(dateFrom, dateTo, currentWorkerId);

            // Take task ids to fill Performers and TaskTimeTracking classes
            var taskIds = Tasks.GetTaskIds();
            var ids = taskIds as long[] ?? taskIds.ToArray();

            Performers = new Performers(ids);
            Observers = new Observers(ids);
            TaskTimeTracking = new TaskTimeTracking(ids);
        }

        public TaskClass(string globalId)
        {
            _dateFrom = DateTime.MinValue;
            _dateTo = DateTime.MinValue;

            Tasks = new Tasks(globalId);

            // Take task ids to fill Performers and TaskTimeTracking classes
            var taskIds = Tasks.GetTaskIds();
            var ids = taskIds as long[] ?? taskIds.ToArray();

            Performers = new Performers(ids);
            Observers = new Observers(ids);
            TaskTimeTracking = new TaskTimeTracking(ids);
        }



        #region Fillings

        public void Fill(DateTime dateFrom, DateTime dateTo, int currentWorkerId)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            Tasks.Fill(dateFrom, dateTo, currentWorkerId);

            FillPerformersObserversAndTimeTracking();
        }

        public void Fill(DateTime dateFrom, DateTime dateTo)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            Tasks.Fill(dateFrom, dateTo);
            FillPerformersObserversAndTimeTracking();
        }

        public void Fill(string globalId)
        {
            _dateFrom = DateTime.MinValue;
            _dateTo = DateTime.MinValue;

            Tasks.Fill(globalId);

            FillPerformersObserversAndTimeTracking();
        }

        private void FillPerformersObserversAndTimeTracking()
        {
            // Take task ids to fill Performers and TaskTimeTracking classes
            var taskIds = Tasks.GetTaskIds();
            var ids = taskIds as long[] ?? taskIds.ToArray();

            Performers.Fill(ids);
            Observers.Fill(ids);
            TaskTimeTracking.Fill(ids);
        }

        #endregion



        #region Addings

        public int AddNewTask(string taskName, int mainWorkerId, DateTime creationDate, 
            string description, SenderApplications senderAppId, 
            bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            var taskId = Tasks.Add(taskName, mainWorkerId, creationDate, description,
                senderAppId, isDeadLine, deadLine);
            return taskId;
        }

        public int AddNewTask(string globalId, string taskName, int mainWorkerId, 
            DateTime creationDate, string description, SenderApplications senderAppId, 
            bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            var taskId = Tasks.Add(globalId, taskName, mainWorkerId, creationDate, description, 
                senderAppId, isDeadLine, deadLine);
            return taskId;
        }

        public void AddNewPerformer(int taskId, int workerId)
        {
            Performers.Add(taskId, workerId);
        }

        public void AddNewObserver(long taskId, long workerId)
        {
            Observers.Add(taskId, workerId);
        }

        public void AddNewTaskTimeTracking(long taskId, long performerId, int timeSpentAtWorkId, DateTime date, 
            TimeSpan timeStart, TimeSpan timeEnd)
        {
            TaskTimeTracking.Add(taskId, performerId, timeSpentAtWorkId, date, timeStart, timeEnd);
        }

        #endregion



        #region Deleting

        public void DeleteTask(int taskId)
        {
            TaskTimeTracking.DeleteByTask(taskId);

            Performers.DeleteByTask(taskId);

            Observers.DeleteByTask(taskId);

            Tasks.Delete(taskId);
        }

        public void DeleteTaskByGlobalId(string globalId)
        {
            var rows = Tasks.Table.AsEnumerable().Where(r => r.Field<string>("GlobalID") == globalId);
            if(!rows.Any()) return;

            var taskId = Convert.ToInt32(rows.First()["TaskID"]);

            DeleteTask(taskId);
        }

        public void DeleteObserver(int observerId)
        {
            Observers.Delete(observerId);
        }

        public void DeletePerformer(int performerId)
        {
            // Delete all time tracking for deleting performer
            TaskTimeTracking.DeleteByPerformer(performerId);

            Performers.Delete(performerId);
        }

        public void DeleteTaskTimeTracking(int taskTimeTracking)
        {
            TaskTimeTracking.Delete(taskTimeTracking);
        }

        #endregion


        public DateTime GetDateFrom()
        {
            return _dateFrom;
        }

        public DateTime GetDateTo()
        {
            return _dateTo;
        }

        public void SaveTimeTracking()
        {
            TaskTimeTracking.Update();
        }
    }




    public class Tasks
    {
        private readonly string _connectionString = App.ConnectionInfo.ConnectionString;
        private MySqlDataAdapter _adapter;
        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if (_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }



        #region Constructors

        public Tasks()
        {
            Create();
        }

        public Tasks(DateTime dateFrom, DateTime dateTo, int currentWorkerId)
        {
            Create();
            Fill(dateFrom, dateTo, currentWorkerId);
        }

        public Tasks(string globalId)
        {
            Create();
            Fill(globalId);
        }

        #endregion


        #region Fillings

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT TaskID, TaskName, GlobalID, MainWorkerID, CreationDate, CompletionDate, 
                  Description, TaskStatusID, IsDeadLine, DeadLine, IsComplete, SenderAppID 
                  FROM FAIITasks.Tasks LIMIT 0";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0001] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(DateTime dateFrom, DateTime dateTo)
        {
            const string sqlCommandText =
                @"SELECT TaskID, TaskName, GlobalID, MainWorkerID, CreationDate, CompletionDate, 
                  Description, TaskStatusID, IsDeadLine, DeadLine, IsComplete, SenderAppID 
                  FROM FAIITasks.Tasks 
                  WHERE (Date(CreationDate) >= @DateFrom AND Date(CreationDate) <= @DateTo) 
                  OR (Date(CompletionDate) >= @DateFrom AND Date(CompletionDate) <= @DateTo)
                  OR (Date(CreationDate) < @DateFrom AND (IsComplete = FALSE OR Date(CompletionDate) > @DateTo))";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom.Date;
            sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo.Date;

            _adapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0002] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(DateTime dateFrom, DateTime dateTo, int currentWorkerId)
        {
            //Get task ids for current worker, that setisfy filtered values
            var taskIds = Performers.GetWorkerTasks(currentWorkerId, dateFrom, dateTo);
            var enumerable = taskIds as IList<long> ?? taskIds.ToList();

            var taskIdsString = string.Empty;
            //If returned task ids is empty, set filter to -1
            taskIdsString = enumerable.Count() != 0
                ? (enumerable.Cast<object>()
                    .Aggregate(taskIdsString, (current, taskId) => current + ", " + taskId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText =
                @"SELECT TaskID, TaskName, GlobalID, MainWorkerID, CreationDate, CompletionDate, 
                  Description, TaskStatusID, IsDeadLine, DeadLine, IsComplete, SenderAppID 
                  FROM FAIITasks.Tasks WHERE TaskID IN (" + taskIdsString + ")";

            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0002] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(string globalId)
        {
            const string sqlCommandText =
                @"SELECT TaskID, TaskName, GlobalID, MainWorkerID, CreationDate, CompletionDate, 
                  Description, TaskStatusID, IsDeadLine, DeadLine, IsComplete, SenderAppID 
                  FROM FAIITasks.Tasks WHERE GlobalID = @GlobalID";

            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;

            _adapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0003] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        private void Create()
        {
            _table = new DataTable();
        }

        public IEnumerable<long> GetTaskIds()
        {
            var ids = from taskView in Table.AsEnumerable()
                select taskView.Field<Int64>("TaskID");
            return ids.ToList();
        }

        public int Add(string taskName, int mainWorkerId, DateTime creationDate, string description,
            TaskClass.SenderApplications senderAppId, bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            var newRow = Table.NewRow();
            newRow["TaskName"] = taskName;
            newRow["MainWorkerID"] = mainWorkerId;
            newRow["CreationDate"] = creationDate;
            newRow["Description"] = description;
            newRow["TaskStatusID"] = TaskClass.TaskStatuses.NotStarted;
            newRow["SenderAppId"] = senderAppId;
            newRow["IsComplete"] = false;
            newRow["IsDeadLine"] = false;

            if (isDeadLine)
            {
                newRow["IsDeadLine"] = true;
                newRow["DeadLine"] = deadLine;
            }

            Table.Rows.Add(newRow);
            Update();

            //Fill globalId for new task
            var taskId = GetTaskID(mainWorkerId, creationDate);
            if (taskId != -1)
            {
                newRow["TaskID"] = taskId;
                newRow.AcceptChanges();

                // Create global id
                var globalId = string.Format("0{0}{1:00000}",
                    (int) TaskClass.SenderApplications.Tasks, taskId);
                newRow["GlobalID"] = globalId;
                Update();
            }

            return taskId;
        }

        private void Update()
        {
            try
            {
                _adapter.Update(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0003] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int Add(string globalId, string taskName, int mainWorkerId, DateTime creationDate, string description,
            TaskClass.SenderApplications senderAppId, bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            var newRow = Table.NewRow();
            newRow["GlobalID"] = globalId;
            newRow["TaskName"] = taskName;
            newRow["MainWorkerID"] = mainWorkerId;
            newRow["CreationDate"] = creationDate;
            newRow["Description"] = description;
            newRow["TaskStatusID"] = TaskClass.TaskStatuses.NotStarted;
            newRow["SenderAppId"] = senderAppId;
            newRow["IsComplete"] = false;
            newRow["IsDeadLine"] = false;

            if (isDeadLine)
            {
                newRow["IsDeadLine"] = true;
                newRow["DeadLine"] = deadLine;
            }

            Table.Rows.Add(newRow);
            Update();

            var taskId = GetTaskID(mainWorkerId, creationDate);
            newRow["TaskID"] = taskId;
            newRow.AcceptChanges();

            return taskId;
        }

        public static void AddNewTask(string globalId, string taskName, int mainWorkerId, DateTime creationDate,
            string description,
            TaskClass.SenderApplications senderAppId, bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            const string sqlCommandText = @"INSERT INTO FAIITasks.Tasks 
                                            (TaskName, GlobalID, MainWorkerID, CreationDate, Description, 
                                             TaskStatusID, IsDeadLine, DeadLine, IsComplete, SenderAppID) VALUES 
                                            (@TaskName, @GlobalID, @MainWorkerID, @CreationDate, @Description, 
                                             @TaskStatusID, @IsDeadLine, @DeadLine, @IsComplete, @SenderAppID)";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@TaskName", MySqlDbType.VarChar).Value = taskName;
                sqlCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;
                sqlCommand.Parameters.Add("@MainWorkerID", MySqlDbType.Int64).Value = mainWorkerId;
                sqlCommand.Parameters.Add("@CreationDate", MySqlDbType.DateTime).Value = creationDate;
                sqlCommand.Parameters.Add("@Description", MySqlDbType.Text).Value = description;
                sqlCommand.Parameters.Add("@TaskStatusID", MySqlDbType.Int64).Value = TaskClass.TaskStatuses.NotStarted;
                sqlCommand.Parameters.Add("@IsDeadLine", MySqlDbType.Bit).Value = isDeadLine;

                if (isDeadLine)
                    sqlCommand.Parameters.Add("@DeadLine", MySqlDbType.DateTime).Value = deadLine;
                else
                    sqlCommand.Parameters.Add("@DeadLine", MySqlDbType.DateTime).Value = DBNull.Value;

                sqlCommand.Parameters.Add("@IsComplete", MySqlDbType.Bit).Value = false;
                sqlCommand.Parameters.Add("@SenderAppID", MySqlDbType.Int64).Value = senderAppId;

                try
                {
                    conn.Open();
                    sqlCommand.ExecuteScalar();

                }
                catch (Exception exp)
                {
                    MessageBox.Show(string.Format("Не удалось создать задачу: '{0}'\n\n {1}", taskName, exp.Message));
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private static int GetTaskID(int mainWorkerId, DateTime creationDate)
        {
            var taskId = -1;
            const string sqlCommandText = @"SELECT TaskID FROM FAIITasks.Tasks 
                                            WHERE MainWorkerID = @MainWorkerID AND 
                                            CreationDate = @CreationDate";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    conn.Open();

                    var selectCommand = new MySqlCommand(sqlCommandText, conn);
                    selectCommand.Parameters.Add("@MainWorkerID", MySqlDbType.Int64).Value = mainWorkerId;
                    selectCommand.Parameters.Add("@CreationDate", MySqlDbType.DateTime).Value = creationDate;

                    var result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        taskId = Convert.ToInt32(result);
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                }

                return taskId;
            }
        }

        public static int GetTaskID(string globalId)
        {
            var taskId = -1;
            const string sqlCommandText = @"SELECT TaskID FROM FAIITasks.Tasks 
                                            WHERE GlobalID = @GlobalID";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    conn.Open();

                    var selectCommand = new MySqlCommand(sqlCommandText, conn);
                    selectCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;

                    var result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        taskId = Convert.ToInt32(result);
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                }

                return taskId;
            }
        }

        public void Change(int taskId, string taskName, string description, bool isDeadLine = false,
            DateTime deadLine = new DateTime())
        {
            var rows = Table.AsEnumerable().Where(t => t.Field<Int64>("TaskID") == taskId);
            if (!rows.Any()) return;

            var changingRow = rows.First();
            changingRow["TaskName"] = taskName;
            changingRow["Description"] = description;

            if (isDeadLine)
            {
                changingRow["IsDeadLine"] = true;
                changingRow["DeadLine"] = deadLine;
            }
            else
            {
                changingRow["IsDeadLine"] = false;
                changingRow["DeadLine"] = DBNull.Value;
            }

            Update();
        }

        public void Delete(int taskId)
        {
            var rows = Table.AsEnumerable().Where(t => t.Field<Int64>("TaskID") == taskId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            Update();
        }

        public static void DeleteTask(int taskId)
        {
            const string sqlCommandText = @"DELETE FROM FAIITasks.Tasks 
                                            WHERE TaskID = @TaskID";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;

                try
                {
                    conn.Open();
                    sqlCommand.ExecuteScalar();
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public void StartTask(long taskId)
        {
            var rows = Table.AsEnumerable().Where(t => t.Field<Int64>("TaskID") == taskId);
            if (!rows.Any()) return;

            var task = rows.First();
            task["TaskStatusID"] = (int) TaskClass.TaskStatuses.IsPerformed;

            Update();
        }

        public void EndTask(long taskId, DateTime completionDate)
        {
            var rows = Table.AsEnumerable().Where(t => t.Field<Int64>("TaskID") == taskId);
            if (!rows.Any()) return;

            var task = rows.First();
            task["CompletionDate"] = completionDate;
            task["TaskStatusID"] = (int) TaskClass.TaskStatuses.IsCompleted;
            task["IsComplete"] = true;

            Update();
        }
    }




    public class Performers
    {
        private readonly string _connectionString = App.ConnectionInfo.ConnectionString;
        private MySqlDataAdapter _adapter;
        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if (_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }



        #region Constructors

        public Performers()
        {
            Create();
        }

        public Performers(IEnumerable<long> taskIds)
        {
            Create();
            Fill(taskIds);
        }

        #endregion


        #region Fillings

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT PerformerID, TaskID, WorkerID, StartDate, CompletionDate, 
                  SpendTime, TaskStatusID, IsComplete FROM FAIITasks.Performers LIMIT 0";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0004] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(IEnumerable<long> taskIds)
        {
            var ids = taskIds as IList<long> ?? taskIds.ToList();

            var taskIdsString = string.Empty;
            //If returned task ids is empty, set filter to -1
            taskIdsString = ids.Count() != 0
                ? (ids.Cast<object>()
                    .Aggregate(taskIdsString, (current, taskId) => current + ", " + taskId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText = @"SELECT PerformerID, TaskID, WorkerID, StartDate, CompletionDate, 
                                   SpendTime, TaskStatusID, IsComplete FROM FAIITasks.Performers 
                                   WHERE TaskID IN (" + taskIdsString + ")";


            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0005] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion



        private void Create()
        {
            _table = new DataTable();
        }

        public static IEnumerable<long> GetWorkerTasks(int workerId, DateTime dateFrom, DateTime dateTo)
        {
            var ids = new List<Int64>();

            const string sqlCommandText = @"SELECT DISTINCT ot.TaskID 
                                            FROM FAIITasks.Performers p 
                                            RIGHT JOIN 
                                                (SELECT t.TaskID, t.MainWorkerID, t.CreationDate, 
		                                                t.CompletionDate, t.IsComplete, o.WorkerID 
                                                 FROM FAIITasks.Observers o
                                                 RIGHT JOIN FAIITasks.Tasks t 
                                                 ON o.TaskID = t.TaskID) ot
                                            ON p.TaskID = ot.TaskID 
                                            WHERE 
                                            ((ot.CreationDate >= @DateFrom AND ot.CreationDate <= @DateTo) 
                                              OR (ot.CompletionDate >= @DateFrom AND ot.CompletionDate <= @DateTo)
                                              OR (ot.CreationDate < @DateFrom 
                                                  AND (p.IsComplete = FALSE OR ot.IsComplete = FALSE 
                                                       OR ot.CompletionDate > @DateTo))) 
                                              AND (p.WorkerID = @WorkerID OR ot.MainWorkerID = @WorkerID OR ot.WorkerID = @WorkerID)";

            using (var sqlConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConnection);
                sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo.AddDays(1);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                try
                {
                    sqlConnection.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(Convert.ToInt64(reader[0]));
                        }
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    sqlConnection.Close();
                }
            }

            return ids;
        }

        public void Add(int taskId, int workerId)
        {
            var newRow = Table.NewRow();
            newRow["TaskID"] = taskId;
            newRow["WorkerID"] = workerId;
            newRow["TaskStatusID"] = 1;
            newRow["IsComplete"] = false;

            Table.Rows.Add(newRow);
            Update();

            var performerId = GetPerformerId(taskId, workerId);
            newRow["PerformerID"] = performerId;
            newRow.AcceptChanges();
        }

        private int GetPerformerId(int taskId, int workerId)
        {
            var performerId = -1;

            const string sqlCommandText = @"SELECT PerformerID 
                                            FROM FAIITasks.Performers 
                                            WHERE TaskID = @TaskID 
                                            AND WorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            performerId = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }

            return performerId;
        }

        public static void AddPerformer(int taskId, int workerId)
        {
            const string sqlCommandText = @"INSERT INTO FAIITasks.Performers 
                                            (TaskID, WorkerID, TaskStatusID, IsComplete) VALUES 
                                            (@TaskID, @WorkerID, @TaskStatusID, @IsComplete)";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@TaskStatusID", MySqlDbType.Int64).Value = TaskClass.TaskStatuses.NotStarted;
                sqlCommand.Parameters.Add("@IsComplete", MySqlDbType.Bit).Value = false;

                try
                {
                    conn.Open();
                    sqlCommand.ExecuteScalar();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(string.Format("Не удалось добавить задачу для: '{0}'\n\n {1}",
                        new IdToNameConverter().Convert(workerId, typeof (string), "ShortName",
                            CultureInfo.InvariantCulture), exp.Message));
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public void Delete(int performerId)
        {
            var rows = Table.Select("PerformerID = " + performerId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            Update();
        }

        public void DeleteByTask(int taskId)
        {
            foreach (var performer in Table.AsEnumerable().
                Where(r => r.Field<Int64>("TaskID") == taskId))
            {
                performer.Delete();
            }

            Update();
        }

        public static void DeletePerformer(int taskId, int workerId)
        {
            const string sqlCommandText = @"DELETE FROM FAIITasks.Performers 
                                            WHERE TaskID = @TaskID AND WorkerID = @WorkerID";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                try
                {
                    conn.Open();
                    sqlCommand.ExecuteScalar();
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void Update()
        {
            try
            {
                _adapter.Update(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0006] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartTask(long taskId, long workerId, DateTime startDate)
        {
            var rows =
                Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == taskId &&
                                p.Field<Int64>("WorkerID") == workerId);
            if (!rows.Any()) return;

            var performer = rows.First();
            performer["StartDate"] = startDate;
            performer["TaskStatusID"] = (int) TaskClass.TaskStatuses.IsPerformed;

            Update();
        }

        public void EndTask(long taskId, long workerId, DateTime completionDate)
        {
            var rows =
                Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == taskId &&
                                p.Field<Int64>("WorkerID") == workerId);
            if (!rows.Any()) return;

            var performer = rows.First();
            performer["CompletionDate"] = completionDate;
            performer["TaskStatusID"] = (int)TaskClass.TaskStatuses.IsCompleted;
            performer["IsComplete"] = true;

            Update();
        }

        public void SetTimeSpend(long performerId, TimeSpan timeSpend)
        {
            var rows = Table.Select(string.Format("PerformerID = {0}", performerId));
            if(!rows.Any()) return;

            var setingRow = rows.First();
            setingRow["SpendTime"] = Convert.ToInt64(timeSpend.TotalMinutes);

            Update();
        }
    }




    public class Observers
    {
        private readonly string _connectionString = App.ConnectionInfo.ConnectionString;
        private MySqlDataAdapter _adapter;
        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if (_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }



        #region Constructors

        public Observers()
        {
            Create();
        }

        public Observers(IEnumerable<long> taskIds)
        {
            Create();
            Fill(taskIds);
        }

        #endregion


        #region Fillings

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT ObserverID, TaskID, WorkerID FROM FAIITasks.Observers LIMIT 0";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0006] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(IEnumerable<long> taskIds)
        {
            var ids = taskIds as IList<long> ?? taskIds.ToList();

            var taskIdsString = string.Empty;
            //If returned task ids is empty, set filter to -1
            taskIdsString = ids.Count() != 0
                ? (ids.Cast<object>()
                    .Aggregate(taskIdsString, (current, taskId) => current + ", " + taskId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText = @"SELECT ObserverID, TaskID, WorkerID FROM FAIITasks.Observers 
                                   WHERE TaskID IN (" + taskIdsString + ")";


            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0007] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        private void Create()
        {
            _table = new DataTable();
        }

        public void Add(long taskId, long workerId)
        {
            var newRow = Table.NewRow();
            newRow["TaskID"] = taskId;
            newRow["WorkerID"] = workerId;

            Table.Rows.Add(newRow);
            Update();

            var observerId = GetObserverId(taskId, workerId);
            newRow["ObserverID"] = observerId;
            newRow.AcceptChanges();
        }

        private long GetObserverId(long taskId, long workerId)
        {
            long observerId = -1;

            const string sqlCommandText = @"SELECT ObserverID 
                                            FROM FAIITasks.Observers 
                                            WHERE TaskID = @TaskID 
                                            AND WorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            observerId = Convert.ToInt64(result);
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }

            return observerId;
        }

        public void Delete(int observerId)
        {
            var rows = Table.Select("ObserverID = " + observerId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            Update();
        }

        public void DeleteByTask(int taskId)
        {
            foreach (var performer in Table.AsEnumerable().
                Where(r => r.Field<Int64>("TaskID") == taskId))
            {
                performer.Delete();
            }

            Update();
        }

        private void Update()
        {
            try
            {
                _adapter.Update(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0008] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }




    public class TaskTimeTracking
    {
        private readonly string _connectionString = App.ConnectionInfo.ConnectionString;
        private MySqlDataAdapter _adapter;
        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if (_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }



        #region Constructors

        public TaskTimeTracking()
        {
            Create();
        }

        public TaskTimeTracking(IEnumerable<long> taskIds)
        {
            Create();
            Fill(taskIds);
        }

        #endregion


        #region Fillings

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT TaskTimeTrackingID, TaskID, PerformerID, TimeSpentAtWorkID, Date, TimeStart, TimeEnd, 
                  VerificationDate, VerificationWorkerID, IsVerificated 
                  FROM FAIITasks.TaskTimeTracking LIMIT 0";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0008] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Fill(IEnumerable<long> taskIds)
        {
            var ids = taskIds as IList<long> ?? taskIds.ToList();

            var taskIdsString = string.Empty;
            //If returned task ids is empty, set filter to -1
            taskIdsString = ids.Count() != 0
                ? (ids.Cast<object>()
                    .Aggregate(taskIdsString, (current, taskId) => current + ", " + taskId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText =
                @"SELECT TaskTimeTrackingID, TaskID, PerformerID, TimeSpentAtWorkID, Date, TimeStart, TimeEnd, 
                  VerificationDate, VerificationWorkerID, IsVerificated 
                  FROM FAIITasks.TaskTimeTracking 
                  WHERE TaskID IN (" + taskIdsString + ")";


            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0009] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion



        private void Create()
        {
            _table = new DataTable();
        }

        public void Add(long taskId, long performerId, int timeSpentAtWorkId, DateTime date, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var newRow = Table.NewRow();
            newRow["TaskID"] = taskId;
            newRow["PerformerID"] = performerId;
            newRow["TimeSpentAtWorkID"] = timeSpentAtWorkId;
            newRow["Date"] = date;
            newRow["TimeStart"] = timeStart;
            newRow["TimeEnd"] = timeEnd;
            newRow["IsVerificated"] = false;

            Table.Rows.Add(newRow);
            Update();

            var taskTimeTrackingId = GetTaskTimeTrackingId(performerId, date);
            newRow["TaskTimeTrackingID"] = taskTimeTrackingId;
            newRow.AcceptChanges();
        }

        private int GetTaskTimeTrackingId(long performerId, DateTime date)
        {
            var taskTimeTrackingId = -1;

            const string sqlCommandText = @"SELECT TaskTimeTrackingID 
                                            FROM FAIITasks.TaskTimeTracking 
                                            WHERE PerformerID = @PerformerID 
                                            AND Date = @Date 
                                            ORDER BY TaskTimeTrackingID DESC";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@PerformerID", MySqlDbType.Int64).Value = performerId;
                    sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = date;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            taskTimeTrackingId = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }

            return taskTimeTrackingId;
        }

        public void Update()
        {
            try
            {
                _adapter.Update(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0009] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Delete(int taskTimeTrackingId)
        {
            var rows = Table.Select(string.Format("TaskTimeTrackingID = {0}", taskTimeTrackingId));
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            Update();
        }

        public void DeleteByPerformer(int performerId)
        {
            foreach (var taskTimeTracking in 
                Table.Select(string.Format("PerformerID = {0}", performerId)))
            {
                taskTimeTracking.Delete();
            }

            Update();
        }

        public void DeleteByTask(int taskId)
        {
            foreach (var taskTimeTracking in
                Table.Select(string.Format("TaskID = {0}", taskId)))
            {
                taskTimeTracking.Delete();
            }

            Update();
        }

        public void Confirm(int taskTimeTrackingId, int verificationWorkerId, DateTime verificationDate, bool update = true)
        {
            var rows = Table.AsEnumerable().Where(t => Convert.ToInt32(t["TaskTimeTrackingID"]) == taskTimeTrackingId);
            if (!rows.Any()) return;

            var timeTrackingRow = rows.First();
            if (Convert.ToBoolean(timeTrackingRow["IsVerificated"])) return;

            timeTrackingRow["VerificationDate"] = verificationDate;
            timeTrackingRow["VerificationWorkerID"] = verificationWorkerId;
            timeTrackingRow["IsVerificated"] = true;

            if (update)
                Update();
        }

        public void SetTimeInterval(int taskTimeTrackingId, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var rows = Table.AsEnumerable().Where(t => Convert.ToInt32(t["TaskTimeTrackingID"]) == taskTimeTrackingId);
            if (!rows.Any()) return;

            var timeTrackingRow = rows.First();

            timeTrackingRow["TimeStart"] = timeStart;
            timeTrackingRow["TimeEnd"] = timeEnd;

            Update();
        }
    }




    public class TaskStatuses
    {
        private readonly string _connectionString;

        private MySqlDataAdapter _adapter;

        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if(_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }

        public TaskStatuses(string connectionString)
        {
            _connectionString = connectionString;

            Create();
        }

        private void Create()
        {
            _table = new DataTable();
        }

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT TaskStatusID, TaskStatusName FROM FAIITasks.TaskStatuses";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0010] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }




    public class SenderApplications
    {
        private readonly string _connectionString;

        private MySqlDataAdapter _adapter;

        private DataTable _table;

        public DataTable Table
        {
            set { _table = value; }
            get
            {
                if(_table.Columns.Count == 0)
                    Fill();
                return _table;
            }
        }

        public SenderApplications(string connectionString)
        {
            _connectionString = connectionString;

            Create();
        }

        private void Create()
        {
            _table = new DataTable();
        }

        private void Fill()
        {
            const string sqlCommandText =
                @"SELECT AppID, AppName, AppColor FROM FAIITasks.SenderApplications";
            _adapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_adapter);
            try
            {
                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TC0011] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
