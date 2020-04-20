using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using FA2.Classes.WorkerRequestsEnums;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    namespace WorkerRequestsEnums
    {
        public enum RequestType
        {
            Vacation = 1,
            ExtraWork
        }

        public enum SalarySaveType
        {
            With = 1,
            Without
        }

        public enum InitiativeType
        {
            Personal = 1,
            ByLeader,
            ByDirecory
        }

        public enum IntervalType
        {
            DurringSomeHours = 1,
            DurringWorkingDay,
            DurringSomeDays
        }

        public enum WorkingOffType
        {
            Without = 1,
            With
        }
    }




    public class WorkerRequestsClass
    {
        private readonly string _connectionString;

        private MySqlDataAdapter _workerRequestsAdapter;
        private DataTable _workerRequestsTable;
        public DataTable WorkerRequestsTable
        {
            get
            {
                if (_workerRequestsTable.Columns.Count == 0)
                    FillWorkerRequests();
                return _workerRequestsTable;
            }
            set { _workerRequestsTable = value; }
        }

        private MySqlDataAdapter _requestTypesAdapter;
        public DataTable RequestTypesTable;

        private MySqlDataAdapter _salatySaveTypesAdapter;
        public DataTable SalatySaveTypesTable;

        private MySqlDataAdapter _initiativeTypesAdapter;
        public DataTable InitiativeTypesTable;

        private MySqlDataAdapter _intervalTypesAdapter;
        public DataTable IntervalTypesTable;

        private MySqlDataAdapter _workingOffTypesAdapter;
        public DataTable WorkingOffTypesTable;

        private MySqlDataAdapter _defaultRequestReasonsAdapter;
        public DataTable DefaultRequestReasonsTable;


        private DateTime _dateFrom;
        public DateTime? DateFrom
        {
            get { return _dateFrom; }
        }

        private DateTime _dateTo;
        public DateTime? DateTo
        {
            get { return _dateTo; }
        }



        public WorkerRequestsClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;

            Create();

            FillRequestTypes();
            FillSalatySaveTypes();
            FillInitiativeTypes();
            FillIntervalTypes();
            FillWorkingOffTypes();
            FillDefaultRequestReasons();
        }

        private void Create()
        {
            _workerRequestsTable = new DataTable();
            RequestTypesTable = new DataTable();
            SalatySaveTypesTable = new DataTable();
            InitiativeTypesTable = new DataTable();
            IntervalTypesTable = new DataTable();
            WorkingOffTypesTable = new DataTable();
            DefaultRequestReasonsTable = new DataTable();
        }




        private void FillWorkerRequests()
        {
            const string sqlCommandText = @"SELECT WorkerRequestID, GlobalID, RequestTypeID, WorkerID, RequestDate, RequestFinishDate,
                                                   SalarySaveTypeID, InitiativeTypeID, IntervalTypeID, WorkingOffTypeID,
                                                   CreationDate, RequestCreatedWorkerID, RequestNotes, MainWorkerID, MainWorkerNotes,
                                                   IsConfirmed, ConfirmationDate
                                            FROM FAIIVacations.WorkerRequests LIMIT 0";
            _workerRequestsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_workerRequestsAdapter);
            try
            {
                _workerRequestsTable.Clear();
                _workerRequestsAdapter.Fill(_workerRequestsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0001] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillWorkerRequests(DateTime dateFrom, DateTime dateTo)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            const string sqlCommandText = @"SELECT WorkerRequestID, GlobalID, RequestTypeID, WorkerID, RequestDate, RequestFinishDate,
                                                   SalarySaveTypeID, InitiativeTypeID, IntervalTypeID, WorkingOffTypeID,
                                                   CreationDate, RequestCreatedWorkerID, RequestNotes, MainWorkerID, MainWorkerNotes,
                                                   IsConfirmed, ConfirmationDate
                                            FROM FAIIVacations.WorkerRequests
                                            WHERE ((@DateFrom <= Date(CreationDate) AND Date(CreationDate) <= @DateTo) OR
                                                   (@DateFrom <= Date(RequestDate) AND Date(RequestDate) <= @DateTo) OR
                                                   (@DateFrom <= Date(RequestFinishDate) AND Date(RequestFinishDate) <= @DateTo) OR
                                                   (Date(RequestDate) < @DateFrom AND 
                                                        (@DateTo < Date(RequestFinishDate) OR IsConfirmed IS NULL)))";
            var sqlConnection = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConnection);
            sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
            sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo;

            _workerRequestsAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_workerRequestsAdapter);

            var tempTable = _workerRequestsTable.Copy();

            try
            {
                _workerRequestsTable.Clear();
                _workerRequestsAdapter.Fill(_workerRequestsTable);
            }
            catch (MySqlException exp)
            {
                _workerRequestsTable = tempTable.Copy();

                MessageBox.Show(exp.Message +
                                "\n\n[WRC0002] Не удалось загрузить данные с сервера. " +
                                "\nДанные восстановлены в предыдущем состояние. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                tempTable.Dispose();
            }
        }


        public long AddWorkerRequest(RequestType requestType, long workerId, DateTime requestDate,
            DateTime requestFinishDate,
            SalarySaveType salarySaveType, InitiativeType initiativeType, IntervalType intervalType,
            WorkingOffType workingOffType,
            DateTime creationDate, long requestCreatedWorkerId, string requestNotes, long mainWorkerId)
        {
            var newRequestRow = WorkerRequestsTable.NewRow();
            newRequestRow["RequestTypeID"] = (int)requestType;
            newRequestRow["WorkerID"] = workerId;
            newRequestRow["RequestDate"] = requestDate;
            newRequestRow["RequestFinishDate"] = requestFinishDate;
            newRequestRow["SalarySaveTypeID"] = (int)salarySaveType;
            newRequestRow["InitiativeTypeID"] = (int)initiativeType;
            newRequestRow["IntervalTypeID"] = (int)intervalType;
            newRequestRow["WorkingOffTypeID"] = (int)workingOffType;
            newRequestRow["CreationDate"] = creationDate;
            newRequestRow["RequestCreatedWorkerID"] = requestCreatedWorkerId;
            newRequestRow["RequestNotes"] = requestNotes;
            newRequestRow["MainWorkerID"] = mainWorkerId;

            WorkerRequestsTable.Rows.Add(newRequestRow);
            UpdateWorkerRequests();

            var workerRequestId = GetWorkerRequestId(workerId, creationDate);
            newRequestRow["WorkerRequestID"] = workerRequestId.HasValue
                ? workerRequestId.Value
                : 0;
            newRequestRow.AcceptChanges();

            if (workerRequestId.HasValue)
            {
                // Create global id
                var globalId = string.Format("0{0}{1:00000}",
                    (int)TaskClass.SenderApplications.WorkerRequests, workerRequestId.Value);
                newRequestRow["GlobalID"] = globalId;
                UpdateWorkerRequests();
            }

            return workerRequestId.Value;
        }

        private static long? GetWorkerRequestId(long workerId, DateTime creationDate)
        {
            long? workerRequestId = null;

            const string sqlComamndText = @"SELECT WorkerRequestID FROM FAIIVacations.WorkerRequests 
                                            WHERE WorkerID = @WorkerID AND CreationDate = @CreationDate";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@CreationDate", MySqlDbType.DateTime).Value = creationDate;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        workerRequestId = Convert.ToInt64(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return workerRequestId;
        }

        public void SetConfirmationInfo(long workerRequestId, string mainWorkerNotes, bool isConfirmed,
            DateTime confirmationDate)
        {
            var rows = WorkerRequestsTable.Select(string.Format("WorkerRequestID = {0}", workerRequestId));
            if (!rows.Any()) return;

            var workerRequestRow = rows.First();
            workerRequestRow["MainWorkerNotes"] = mainWorkerNotes;
            workerRequestRow["IsConfirmed"] = isConfirmed;
            workerRequestRow["ConfirmationDate"] = confirmationDate;

            UpdateWorkerRequests();
        }

        public void DeleteWorkerRequest(long workerRequestId)
        {
            var rows = WorkerRequestsTable.Select(string.Format("WorkerRequestID = {0}", workerRequestId));
            if (!rows.Any()) return;

            var workerRequestRow = rows.First();
            workerRequestRow.Delete();

            UpdateWorkerRequests();
        }



        private void UpdateWorkerRequests()
        {
            try
            {
                _workerRequestsAdapter.Update(_workerRequestsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0003] Не удалось обновить данные на сервере. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void FillRequestTypes()
        {
            const string sqlCommandText = @"SELECT RequestTypeID, RequestTypeName 
                                            FROM FAIIVacations.RequestTypes";
            _requestTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                RequestTypesTable.Clear();
                _requestTypesAdapter.Fill(RequestTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0004] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillSalatySaveTypes()
        {
            const string sqlCommandText = @"SELECT SalarySaveTypeID, SalarySaveTypeName 
                                            FROM FAIIVacations.SalarySaveTypes";
            _salatySaveTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                SalatySaveTypesTable.Clear();
                _salatySaveTypesAdapter.Fill(SalatySaveTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0005] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillInitiativeTypes()
        {
            const string sqlCommandText = @"SELECT InitiativeTypeID, InitiativeTypeName 
                                            FROM FAIIVacations.InitiativeTypes";
            _initiativeTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                InitiativeTypesTable.Clear();
                _initiativeTypesAdapter.Fill(InitiativeTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0006] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillIntervalTypes()
        {
            const string sqlCommandText = @"SELECT IntervalTypeID, IntervalTypeName 
                                            FROM FAIIVacations.IntervalTypes";
            _intervalTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                IntervalTypesTable.Clear();
                _intervalTypesAdapter.Fill(IntervalTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0007] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkingOffTypes()
        {
            const string sqlCommandText = @"SELECT WorkingOffTypeID, WorkingOffTypeName 
                                            FROM FAIIVacations.WorkingOffTypes";
            _workingOffTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                WorkingOffTypesTable.Clear();
                _workingOffTypesAdapter.Fill(WorkingOffTypesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0008] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillDefaultRequestReasons()
        {
            const string sqlCommandText = @"SELECT DefaultRequestReasonID, DefaultRequestReasonString
                                            FROM FAIIVacations.DefaultRequestReasons";
            _defaultRequestReasonsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            try
            {
                DefaultRequestReasonsTable.Clear();
                _defaultRequestReasonsAdapter.Fill(DefaultRequestReasonsTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[WRC0009] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
