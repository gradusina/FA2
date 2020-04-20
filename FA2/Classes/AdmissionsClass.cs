using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System;
using System.Linq;

namespace FA2.Classes
{
    public class AdmissionsClass
    {
        public const int WorkSubsectionAdmissionId = 1;

        private readonly string _connectionString;

        private MySqlDataAdapter _admissionsAdapter;
        public DataTable AdmissionsTable;

        private MySqlDataAdapter _workerAdmissionsAdapter;
        public DataTable WorkerAdmissionsTable;

        private MySqlDataAdapter _workOperationWorkerAdmissionsAdapter;
        public DataTable WorkOperationWorkerAdmissionsTable;

        public AdmissionsClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;

            Create();

            FillAdmissions();
            FillWorkerAdmissions();
            FillWorkOperationWorkerAdmissions();
        }

        private void Create()
        {
            AdmissionsTable = new DataTable();
            WorkerAdmissionsTable = new DataTable();
            WorkOperationWorkerAdmissionsTable = new DataTable();
        }

        private void FillAdmissions()
        {
            const string sqlCommandText = @"SELECT AdmissionID, AdmissionName, AdmissionPeriodEnable, AdmissionPeriod, IsLocked, IsEnable 
                                            FROM FAIIAdmission.Admissions";
            _admissionsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_admissionsAdapter);
            try
            {
                AdmissionsTable.Clear();
                _admissionsAdapter.Fill(AdmissionsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[ADC0001] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkerAdmissions()
        {
            const string sqlCommandText = @"SELECT WorkerAdmissionID, AdmissionID, WorkerID, WorkerProfessionID, AdmissionDate 
                                            FROM FAIIAdmission.WorkerAdmissions";
            _workerAdmissionsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_workerAdmissionsAdapter);
            try
            {
                WorkerAdmissionsTable.Clear();
                _workerAdmissionsAdapter.Fill(WorkerAdmissionsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[ADC0002] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkOperationWorkerAdmissions()
        {
            const string sqlCommandText = @"SELECT WorkOperationWorkerAdmissionID, WorkerAdmissionID, WorkerID, WorkSubsectionID, WorkOperationID 
                                            FROM FAIIAdmission.WorkOperationWorkerAdmissions";
            _workOperationWorkerAdmissionsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_workOperationWorkerAdmissionsAdapter);
            try
            {
                WorkOperationWorkerAdmissionsTable.Clear();
                _workOperationWorkerAdmissionsAdapter.Fill(WorkOperationWorkerAdmissionsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[ADC0003] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public DataView GetAdmissionsView()
        {
            var admissionsView = AdmissionsTable.AsDataView();
            admissionsView.RowFilter = "IsEnable = TRUE";
            return admissionsView;
        }


        public void AddAdmission(string admissionName, bool admissionPeriodEnable, int admissionPeriod)
        {
            var admissions = AdmissionsTable.Select(string.Format("AdmissionName = '{0}'", admissionName));
            if (admissions.Any())
                throw new Exception("Допуск с таким именем уже существует.");

            var newAdmission = AdmissionsTable.NewRow();
            newAdmission["AdmissionName"] = admissionName;
            newAdmission["IsEnable"] = true;
            newAdmission["IsLocked"] = false;
            if(admissionPeriodEnable)
            {
                newAdmission["AdmissionPeriodEnable"] = true;
                newAdmission["AdmissionPeriod"] = admissionPeriod;
            }
            else
            {
                newAdmission["AdmissionPeriodEnable"] = false;
                newAdmission["AdmissionPeriod"] = null;
            }

            AdmissionsTable.Rows.Add(newAdmission);
            UpdateAdmissions();

            var admissionId = GetAdmissionId(admissionName, admissionPeriodEnable);
            newAdmission["AdmissionID"] = admissionId.HasValue
                ? admissionId.Value
                : -1;
            newAdmission.AcceptChanges();
        }

        private int? GetAdmissionId(string admissionName, bool admissionPeriodEnable)
        {
            int? admissionId = null;

            const string sqlCommandText = @"SELECT AdmissionID 
                                            FROM FAIIAdmission.Admissions 
                                            WHERE AdmissionName = @AdmissionName AND AdmissionPeriodEnable = @AdmissionPeriodEnable 
                                            ORDER BY AdmissionID DESC";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@AdmissionName", MySqlDbType.VarChar).Value = admissionName;
                sqlCommand.Parameters.Add("@AdmissionPeriodEnable", MySqlDbType.Int16).Value = admissionPeriodEnable;

                sqlConn.Open();
                var result = sqlCommand.ExecuteScalar();
                sqlConn.Close();

                if(result != null && result != DBNull.Value)
                {
                    admissionId = Convert.ToInt32(result);
                    return admissionId;
                }
                else
                {
                    return null;
                }
            }
        }

        public void ChangeAdmission(int admissionId, string admissionName, bool admissionPeriodEnable, int admissionPeriod)
        {
            var admissions = AdmissionsTable.Select(string.Format("AdmissionID <> {0} AND AdmissionName = '{1}'", admissionId, admissionName));
            if (admissions.Any())
                throw new Exception("Допуск с таким именем уже существует.");

            admissions = AdmissionsTable.Select(string.Format("AdmissionID = {0}", admissionId));
            if(!admissions.Any())
                throw new Exception("Невозможно найти данный допуск в базе данных. Возможно он был удалён с другого компьютера");

            var admission = admissions.First();
            admission["AdmissionName"] = admissionName;
            if (admissionPeriodEnable)
            {
                admission["AdmissionPeriodEnable"] = true;
                admission["AdmissionPeriod"] = admissionPeriod;
            }
            else
            {
                admission["AdmissionPeriodEnable"] = false;
                admission["AdmissionPeriod"] = admissionPeriod;
            }

            UpdateAdmissions();
        }

        public void DeleteAdmission(int admissionId)
        {
            var admissions = AdmissionsTable.Select(string.Format("AdmissionID = {0}", admissionId));
            if (!admissions.Any())
                throw new Exception("Невозможно найти данный допуск в базе данных. Возможно он был удалён с другого компьютера");

            var admission = admissions.First();
            admission["IsEnable"] = false;
            UpdateAdmissions();
        }

        private void UpdateAdmissions()
        {
            _admissionsAdapter.Update(AdmissionsTable);
        }


        public long AddWorkerAdmission(int admissionId, long workerId, long workerProfessionId, DateTime admissionDate)
        {
            var workerAdmissions = WorkerAdmissionsTable.Select(string.Format("AdmissionID = {0} AND WorkerID = {1} AND WorkerProfessionID = {2}", 
                admissionId, workerId, workerProfessionId));
            if (workerAdmissions.Any())
            {
                if (admissionId == WorkSubsectionAdmissionId)
                {
                    var workerAdmission = workerAdmissions.First();
                    return Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
                }
                else
                {
                    throw new Exception("Работнику уже выдан данный допуск на выбранную должность.");
                }
            }

            var newWorkerAdmission = WorkerAdmissionsTable.NewRow();
            newWorkerAdmission["AdmissionID"] = admissionId;
            newWorkerAdmission["WorkerID"] = workerId;
            newWorkerAdmission["WorkerProfessionID"] = workerProfessionId;
            newWorkerAdmission["AdmissionDate"] = admissionDate;

            WorkerAdmissionsTable.Rows.Add(newWorkerAdmission);
            UpdateWorkerAdmissions();

            var workerAdmissionId = GetWorkerAdmissionId(admissionId, workerId, admissionDate);
            newWorkerAdmission["WorkerAdmissionID"] = workerAdmissionId.HasValue
                ? workerAdmissionId.Value
                : -1;
            newWorkerAdmission.AcceptChanges();

            return workerAdmissionId.HasValue
                ? workerAdmissionId.Value
                : -1;
        }

        private long? GetWorkerAdmissionId(int admissionId, long workerId, DateTime admissionDate)
        {
            long? workerAdmissionId = null;

            const string sqlCommandText = @"SELECT WorkerAdmissionID 
                                            FROM FAIIAdmission.WorkerAdmissions 
                                            WHERE AdmissionID = @AdmissionID AND WorkerID = @WorkerID AND AdmissionDate = @AdmissionDate 
                                            ORDER BY WorkerAdmissionID DESC";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@AdmissionID", MySqlDbType.Int32).Value = admissionId;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@AdmissionDate", MySqlDbType.DateTime).Value = admissionDate;

                sqlConn.Open();
                var result = sqlCommand.ExecuteScalar();
                sqlConn.Close();

                if (result != null && result != DBNull.Value)
                {
                    workerAdmissionId = Convert.ToInt32(result);
                    return workerAdmissionId;
                }
                else
                {
                    return null;
                }
            }
        }

        public void ChangeWorkerAdmission(long workerAdmissionId, long workerProfessionId, DateTime admissionDate)
        {
            var workerAdmissions = WorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
            if (!workerAdmissions.Any())
                throw new Exception("Невозможно найти данный допуск в базе данных. Возможно он был удалён с другого компьютера");

            var workerAdmission = workerAdmissions.First();
            var workerId = Convert.ToInt64(workerAdmission["WorkerID"]);
            var admissionId = Convert.ToInt32(workerAdmission["AdmissionID"]);

            var anotherWorkerAdmissions = 
                WorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID <> {0} AND AdmissionID = {1} AND WorkerID = {2} AND WorkerProfessionID = {3}",
                workerAdmissionId, admissionId, workerId, workerProfessionId));
            if (anotherWorkerAdmissions.Any() && admissionId != WorkSubsectionAdmissionId)
            {
                throw new Exception("Работнику уже выдан данный допуск на выбранную должность.");
            }

            workerAdmission["WorkerProfessionID"] = workerProfessionId;
            workerAdmission["AdmissionDate"] = admissionDate;

            UpdateWorkerAdmissions();
        }

        public void DeleteWorkerAdmission(long workerAdmissionId)
        {
            var workerAdmissions = WorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
            if (!workerAdmissions.Any())
                throw new Exception("Невозможно найти данный допуск в базе данных. Возможно он был удалён с другого компьютера");

            var workerAdmission = workerAdmissions.First();
            workerAdmission.Delete();

            UpdateWorkerAdmissions();
        }

        private void UpdateWorkerAdmissions()
        {
            _workerAdmissionsAdapter.Update(WorkerAdmissionsTable);
        }


        public void AddWorkOperationWorkerAdmission(long workerAdmissionId, long workerId, long workSubsectionId, long workOperationId)
        {
            var newMachineOperationWorkerAdmission = WorkOperationWorkerAdmissionsTable.NewRow();
            newMachineOperationWorkerAdmission["WorkerAdmissionID"] = workerAdmissionId;
            newMachineOperationWorkerAdmission["WorkerID"] = workerId;
            newMachineOperationWorkerAdmission["WorkSubsectionID"] = workSubsectionId;
            newMachineOperationWorkerAdmission["WorkOperationID"] = workOperationId;

            WorkOperationWorkerAdmissionsTable.Rows.Add(newMachineOperationWorkerAdmission);
            UpdateWorkOperationWorkerAdmissions();

            var machineOperationWorkerAdmissionId = GetWorkOperationWorkerAdmissionId(workerAdmissionId, workerId, workSubsectionId, workOperationId);
            newMachineOperationWorkerAdmission["WorkOperationWorkerAdmissionID"] = machineOperationWorkerAdmissionId.HasValue
                ? machineOperationWorkerAdmissionId.Value
                : -1;
            newMachineOperationWorkerAdmission.AcceptChanges();
        }

        private long? GetWorkOperationWorkerAdmissionId(long workerAdmissionId, long workerId, long workSubsectionId, long workOperationId)
        {
            long? workOperationWorkerAdmissionId = null;

            const string sqlCommandText = @"SELECT WorkOperationWorkerAdmissionID 
                                            FROM FAIIAdmission.WorkOperationWorkerAdmissions 
                                            WHERE WorkerAdmissionID = @WorkerAdmissionID AND WorkerID = @WorkerID 
                                                  AND WorkSubsectionID = @WorkSubsectionID AND WorkOperationID = @WorkOperationID 
                                            ORDER BY WorkOperationWorkerAdmissionID DESC";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerAdmissionID", MySqlDbType.Int64).Value = workerAdmissionId;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@WorkSubsectionID", MySqlDbType.Int64).Value = workSubsectionId;
                sqlCommand.Parameters.Add("@WorkOperationID", MySqlDbType.Int64).Value = workOperationId;

                sqlConn.Open();
                var result = sqlCommand.ExecuteScalar();
                sqlConn.Close();

                if (result != null && result != DBNull.Value)
                {
                    workOperationWorkerAdmissionId = Convert.ToInt32(result);
                    return workOperationWorkerAdmissionId;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool HasWorkerAdmissionsToWorkSubsection(long workerId, long workerAdmissionId, long workSubsectionId)
        {
            var workOperationWorkerAdmissions = 
                WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerID = {0} AND WorkerAdmissionID = {1} AND WorkSubsectionID = {2}",
                workerId, workerAdmissionId, workSubsectionId));
            return workOperationWorkerAdmissions.Any();
        }

        public void DeleteWorkOperationWorkerAdmission(long workOperationWorkerAdmissionId)
        {
            var workOperationWorkerAdmissions =
                WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkOperationWorkerAdmissionID = {0}", workOperationWorkerAdmissionId));
            if (!workOperationWorkerAdmissions.Any())
                throw new Exception("Невозможно найти данный допуск к операции станка. Возможно он был удалён с другого компьютера");

            var machineOperationWorkerAdmission = workOperationWorkerAdmissions.First();
            machineOperationWorkerAdmission.Delete();

            UpdateWorkOperationWorkerAdmissions();
        }

        public void DeleteWorkOperationWorkerAdmissions(long workerAdmissionId, long workSubsectionId)
        {
            var workOperationWorkerAdmissionRows = WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0} AND WorkSubsectionID = {1}",
                workerAdmissionId, workSubsectionId));
            if (!workOperationWorkerAdmissionRows.Any()) return;

            foreach(var workOperationWorkerAdmissionId in workOperationWorkerAdmissionRows.Select(r => Convert.ToInt64(r["WorkOperationWorkerAdmissionID"])))
            {
                DeleteWorkOperationWorkerAdmission(workOperationWorkerAdmissionId);
            }
        }

        public bool HasWorkerAdmissionsToWorkOperations(long workerId)
        {
            var workOperationWorkerAdmissions =
                WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerID = {0}", workerId));
            return workOperationWorkerAdmissions.Any();
        }

        public bool HasWorkerEndedAdmissions(long workerId, DateTime dateForCheck)
        {
            var admissionsWithPeriod = AdmissionsTable.AsEnumerable().Where(r => r.Field<bool>("AdmissionPeriodEnable") && r.Field<object>("AdmissionPeriod") != null);
            var workerEndedAdmissions =
                WorkerAdmissionsTable.AsEnumerable().Where(wA => wA.Field<Int64>("WorkerID") == workerId &&
                admissionsWithPeriod.Any(a => (a.Field<Int32>("AdmissionID") == wA.Field<Int32>("AdmissionID")) &&
                    wA.Field<DateTime>("AdmissionDate") < dateForCheck.Subtract(TimeSpan.FromDays(a.Field<Int32>("AdmissionPeriod")))));
            return workerEndedAdmissions.Any();
        }

        public static DataTable GetAvailableWorkersForWorkSubsections()
        {
            var availableWorkers = new DataTable();

            const string sqlCommandText = @"SELECT DISTINCT WorkerID, WorkSubsectionID 
                                            FROM FAIIAdmission.WorkOperationWorkerAdmissions";
            var adapter = new MySqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString);
            try
            {
                adapter.Fill(availableWorkers);
            }
            catch
            {
            }

            return availableWorkers;
        }

        private void UpdateWorkOperationWorkerAdmissions()
        {
            _workOperationWorkerAdmissionsAdapter.Update(WorkOperationWorkerAdmissionsTable);
        }
    }
}
