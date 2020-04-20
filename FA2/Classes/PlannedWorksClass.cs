using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace FA2.Classes
{
    public enum PlannedWorksInitiative
    {
        ByWorkers = 1,
        ByMentors
    }

    public enum ConfirmationStatus
    {
        WaitingConfirmation = 1,
        Confirmed,
        Rejected
    }

    public class PlannedWorksClass
    {
        private string _connectionString;

        private MySqlDataAdapter _plannedWorksTypesAdapter;
        public DataTable PlannedWorksTypesTable;

        private MySqlDataAdapter _plannedWorksAdapter;
        public DataTable PlannedWorksTable;

        private MySqlDataAdapter _emptyWorkReasonsAdapter;
        public DataTable EmptyWorkReasonsTable;

        private MySqlDataAdapter _startedPlannedWorksAdapter;
        public DataTable StartedPlannedWorksTable;

        public static int SelectedEmptyWorkReasonId;

        public PlannedWorksClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;

            Create();
            Fill();
        }

        private void Create()
        {
            PlannedWorksTypesTable = new DataTable();
            PlannedWorksTable = new DataTable();
            EmptyWorkReasonsTable = new DataTable();
            StartedPlannedWorksTable = new DataTable();
        }

        private void Fill()
        {
            FillPlannedWorksTypes();
            FillPlannedWorks();
            FillEmptyWorkReasons();
            FillStartedPlanedWorks();
        }

        private void FillPlannedWorksTypes()
        {
            const string sqlCommandText = @"SELECT PlannedWorksTypeID, PlannedWorksTypeName, IsEnable FROM FAIIPlannedWorks.PlannedWorksTypes";
            _plannedWorksTypesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_plannedWorksTypesAdapter);
            try
            {
                PlannedWorksTypesTable.Clear();
                _plannedWorksTypesAdapter.Fill(PlannedWorksTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0001] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillPlannedWorks()
        {
            const string sqlCommandText = @"SELECT PlannedWorksID, GlobalID, InitiativeTypeID, PlannedWorksTypeID, CreationDate, CreatedWorkerID, PlannedWorksName, 
                                            Description, ConfirmationDate, ConfirmWorkerID, ConfirmationStatusID, IsMultiple, IsActive, IsReloadEnable, IsEnable 
                                            FROM FAIIPlannedWorks.PlannedWorks";

            _plannedWorksAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_plannedWorksAdapter);
            try
            {
                PlannedWorksTable.Clear();
                _plannedWorksAdapter.Fill(PlannedWorksTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0002] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEmptyWorkReasons()
        {
            const string sqlCommandText = @"SELECT EmptyWorkReasonID, EmptyWorkReasonName, IsEnable 
                                            FROM FAIIPlannedWorks.EmptyWorkReasons";
            _emptyWorkReasonsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_emptyWorkReasonsAdapter);
            try
            {
                EmptyWorkReasonsTable.Clear();
                _emptyWorkReasonsAdapter.Fill(EmptyWorkReasonsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0003] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStartedPlanedWorks()
        {
            const string sqlCommandText = @"SELECT StartedPlannedWorkID, PlannedWorksID, TaskID, EmptyWorkReasonID 
                                            FROM FAIIPlannedWorks.StartedPlannedWorks";
            _startedPlannedWorksAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_startedPlannedWorksAdapter);
            try
            {
                StartedPlannedWorksTable.Clear();
                _startedPlannedWorksAdapter.Fill(StartedPlannedWorksTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0004] Не удалось загрузить данные с сервера. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public DataView GetPlannedWorksTypes()
        {
            var view = PlannedWorksTypesTable.AsDataView();
            view.RowFilter = "IsEnable = 'True'";
            view.Sort = "PlannedWorksTypeName";
            return view;
        }

        public DataView GetPlannedWorks()
        {
            var view = PlannedWorksTable.AsDataView();
            view.RowFilter = "IsEnable = 'True'";
            view.Sort = "PlannedWorksName";
            return view;
        }

        public DataView GetEmptyWorkReasons()
        {
            var view = EmptyWorkReasonsTable.AsDataView();
            view.RowFilter = "IsEnable = 'True'";
            view.Sort = "EmptyWorkReasonName";
            return view;
        }



        public long AddPlannedWorks(PlannedWorksInitiative initiativeType, int plannedWorksTypeId, DateTime creationDate,
            long createdWorkerId, string plannedWorksName, string description, bool isMultiple, bool isReloadEnable)
        {
            var newPlannedWorksRow = PlannedWorksTable.NewRow();
            newPlannedWorksRow["InitiativeTypeID"] = (int)initiativeType;
            newPlannedWorksRow["PlannedWorksTypeID"] = plannedWorksTypeId;
            newPlannedWorksRow["CreationDate"] = creationDate;
            newPlannedWorksRow["CreatedWorkerID"] = createdWorkerId;
            newPlannedWorksRow["PlannedWorksName"] = plannedWorksName;

            if(!string.IsNullOrEmpty(description))
                newPlannedWorksRow["Description"] = description;

            if(initiativeType == PlannedWorksInitiative.ByMentors)
            {
                newPlannedWorksRow["ConfirmationDate"] = creationDate;
                newPlannedWorksRow["ConfirmWorkerID"] = createdWorkerId;
                newPlannedWorksRow["ConfirmationStatusID"] = (int)ConfirmationStatus.Confirmed;
            }
            else
            {
                newPlannedWorksRow["ConfirmationStatusID"] = (int)ConfirmationStatus.WaitingConfirmation;
            }

            newPlannedWorksRow["IsMultiple"] = isMultiple;
            newPlannedWorksRow["IsActive"] = true;
            newPlannedWorksRow["IsReloadEnable"] = isReloadEnable;
            newPlannedWorksRow["IsEnable"] = true;

            PlannedWorksTable.Rows.Add(newPlannedWorksRow);
            UpdatePlannedWorks();

            var plannedWorksId = GetPlannedWorksId(createdWorkerId, creationDate, plannedWorksTypeId);
            newPlannedWorksRow["PlannedWorksID"] = plannedWorksId.HasValue
                ? plannedWorksId.Value
                : -1;
            newPlannedWorksRow.AcceptChanges();

            if (plannedWorksId.HasValue)
            {
                // Create global id
                var globalId = string.Format("0{0}{1:00000}",
                    (int)TaskClass.SenderApplications.PlannedWorks, plannedWorksId.Value);
                newPlannedWorksRow["GlobalID"] = globalId;
                UpdatePlannedWorks();
            }

            return plannedWorksId.Value;
        }

        private static long? GetPlannedWorksId(long createdWorkerId, DateTime creationDate, int plannedWorksTypeId)
        {
            long? plannedWorksId = null;

            const string sqlComamndText = @"SELECT PlannedWorksID FROM FAIIPlannedWorks.PlannedWorks 
                                            WHERE CreatedWorkerID = @CreatedWorkerID AND CreationDate = @CreationDate 
                                            AND PlannedWorksTypeID = @PlannedWorksTypeID";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@CreatedWorkerID", MySqlDbType.Int64).Value = createdWorkerId;
                sqlCommand.Parameters.Add("@CreationDate", MySqlDbType.DateTime).Value = creationDate;
                sqlCommand.Parameters.Add("@PlannedWorksTypeID", MySqlDbType.Int32).Value = plannedWorksTypeId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        plannedWorksId = Convert.ToInt64(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return plannedWorksId;
        }

        public void ChangePlannedWorks(long plannedWorksId, int plannedWorksTypeId, string plannedWorksName, string description, bool isMultiple, bool isReloadEnable)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["PlannedWorksTypeID"] = plannedWorksTypeId;
            plannedWorksRow["PlannedWorksName"] = plannedWorksName;
            if (!string.IsNullOrEmpty(description))
                plannedWorksRow["Description"] = description;
            plannedWorksRow["IsMultiple"] = isMultiple;
            plannedWorksRow["IsReloadEnable"] = isReloadEnable;

            UpdatePlannedWorks();
        }

        public void DeletePlannedWorks(long plannedWorksId)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["IsEnable"] = false;

            UpdatePlannedWorks();
        }

        public void ConfirmPlannedWorks(long plannedWorksId, DateTime confirmationDate, long confirmWorkerId)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["ConfirmationDate"] = confirmationDate;
            plannedWorksRow["ConfirmWorkerID"] = confirmWorkerId;
            plannedWorksRow["ConfirmationStatusID"] = (int)ConfirmationStatus.Confirmed;

            UpdatePlannedWorks();
        }

        public void RejectPlannedWorks(long plannedWorksId, DateTime confirmationDate, long confirmWorkerId)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["ConfirmationDate"] = confirmationDate;
            plannedWorksRow["ConfirmWorkerID"] = confirmWorkerId;
            plannedWorksRow["ConfirmationStatusID"] = (int)ConfirmationStatus.Rejected;

            UpdatePlannedWorks();
        }

        public void ActivatePlannedWorks(long plannedWorksId)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["IsActive"] = true;

            UpdatePlannedWorks();
        }

        public void DeactivatePlannedWorks(long plannedWorksId)
        {
            var rows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (rows.Length == 0) return;

            var plannedWorksRow = rows.First();
            plannedWorksRow["IsActive"] = false;

            UpdatePlannedWorks();
        }

        private void UpdatePlannedWorks()
        {
            try
            {
                _plannedWorksAdapter.Update(PlannedWorksTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0005] Не удалось обновить данные на сервере. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void AddPlannedWorksType(string plannedWorksTypeName)
        {
            var newPlannedWorksTypeRow = PlannedWorksTypesTable.NewRow();
            newPlannedWorksTypeRow["PlannedWorksTypeName"] = plannedWorksTypeName;
            newPlannedWorksTypeRow["IsEnable"] = true;
            PlannedWorksTypesTable.Rows.Add(newPlannedWorksTypeRow);
            UpdatePlannedWorksTypes();

            var plannedWorksTypeId = GetPlannedWorksTypeId(plannedWorksTypeName);
            newPlannedWorksTypeRow["PlannedWorksTypeID"] = plannedWorksTypeId.HasValue
                ? plannedWorksTypeId.Value
                : -1;
            newPlannedWorksTypeRow.AcceptChanges();
        }

        private static int? GetPlannedWorksTypeId(string plannedWorksTypeName)
        {
            int? plannedWorksTypeId = null;

            const string sqlComamndText = @"SELECT PlannedWorksTypeID FROM FAIIPlannedWorks.PlannedWorksTypes 
                                            WHERE PlannedWorksTypeName = @PlannedWorksTypeName 
                                            ORDER BY PlannedWorksTypeID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@PlannedWorksTypeName", MySqlDbType.VarChar).Value = plannedWorksTypeName;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        plannedWorksTypeId = Convert.ToInt32(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return plannedWorksTypeId;
        }

        public void ChangePlannedWorksType(int plannedWorksTypeId, string plannedWorksTypeName)
        {
            var rows = PlannedWorksTypesTable.Select(string.Format("PlannedWorksTypeID = {0}", plannedWorksTypeId));
            if (rows.Length == 0) return;

            var plannedWorksType = rows.First();
            plannedWorksType["PlannedWorksTypeName"] = plannedWorksTypeName;

            UpdatePlannedWorksTypes();
        }

        public void DeletePlannedWorksType(int plannedWorksTypeId)
        {
            var rows = PlannedWorksTypesTable.Select(string.Format("PlannedWorksTypeID = {0}", plannedWorksTypeId));
            if (rows.Length == 0) return;

            var plannedWorksType = rows.First();
            plannedWorksType["IsEnable"] = false;

            UpdatePlannedWorksTypes();
        }

        private void UpdatePlannedWorksTypes()
        {
            try
            {
                _plannedWorksTypesAdapter.Update(PlannedWorksTypesTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0006] Не удалось обновить данные на сервере. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void AddEmptyWorkReason(string emptyWorkReasonName)
        {
            var emptyWorkReasonRow = EmptyWorkReasonsTable.NewRow();
            emptyWorkReasonRow["EmptyWorkReasonName"] = emptyWorkReasonName;
            emptyWorkReasonRow["IsEnable"] = true;
            EmptyWorkReasonsTable.Rows.Add(emptyWorkReasonRow);
            UpdateEmptyWorkReasons();

            var emptyWorkReasonId = GetEmptyWorkReasonId(emptyWorkReasonName);
            emptyWorkReasonRow["EmptyWorkReasonID"] = emptyWorkReasonId.HasValue
                ? emptyWorkReasonId.Value
                : -1;
            emptyWorkReasonRow.AcceptChanges();
        }

        private static int? GetEmptyWorkReasonId(string emptyWorkReasonName)
        {
            int? emptyWorkReasonId = null;

            const string sqlComamndText = @"SELECT EmptyWorkReasonID FROM FAIIPlannedWorks.EmptyWorkReasons 
                                            WHERE EmptyWorkReasonName = @EmptyWorkReasonName 
                                            ORDER BY EmptyWorkReasonID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@EmptyWorkReasonName", MySqlDbType.VarChar).Value = emptyWorkReasonName;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        emptyWorkReasonId = Convert.ToInt32(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return emptyWorkReasonId;
        }

        public void ChangeEmptyWorkReason(int emptyWorkReasonId, string emptyWorkReasonName)
        {
            var rows = EmptyWorkReasonsTable.Select(string.Format("EmptyWorkReasonID = {0}", emptyWorkReasonId));
            if (rows.Length == 0) return;

            var emptyWorkReason = rows.First();
            emptyWorkReason["EmptyWorkReasonName"] = emptyWorkReasonName;

            UpdateEmptyWorkReasons();
        }

        public void DeleteEmptyWorkReason(int emptyWorkReasonId)
        {
            var rows = EmptyWorkReasonsTable.Select(string.Format("EmptyWorkReasonID = {0}", emptyWorkReasonId));
            if (rows.Length == 0) return;

            var emptyWorkReason = rows.First();
            emptyWorkReason["IsEnable"] = false;

            UpdateEmptyWorkReasons();
        }

        private void UpdateEmptyWorkReasons()
        {
            try
            {
                _emptyWorkReasonsAdapter.Update(EmptyWorkReasonsTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0007] Не удалось обновить данные на сервере. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void AddStartedPlanedWorks(long plannedWorksId, long taskId, int emptyWorkReasonId)
        {
            var newStartedPlanedWorksRow = StartedPlannedWorksTable.NewRow();
            newStartedPlanedWorksRow["PlannedWorksID"] = plannedWorksId;
            newStartedPlanedWorksRow["TaskID"] = taskId;
            newStartedPlanedWorksRow["EmptyWorkReasonID"] = emptyWorkReasonId;
            StartedPlannedWorksTable.Rows.Add(newStartedPlanedWorksRow);
            UpdateStartedPlannedWorks();

            var startedPlannedWorkId = GetStartedPlannedWorkId(plannedWorksId, taskId, emptyWorkReasonId);
            newStartedPlanedWorksRow["StartedPlannedWorkID"] = startedPlannedWorkId.HasValue
                ? startedPlannedWorkId.Value
                : -1;
            newStartedPlanedWorksRow.AcceptChanges();
        }

        private static long? GetStartedPlannedWorkId(long plannedWorksId, long taskId, int emptyWorkReasonId)
        {
            long? startedPlannedWorkId = null;

            const string sqlComamndText = @"SELECT StartedPlannedWorkID FROM FAIIPlannedWorks.StartedPlannedWorks 
                                            WHERE PlannedWorksID = @PlannedWorksID AND TaskID = @TaskID AND EmptyWorkReasonID = @EmptyWorkReasonID 
                                            ORDER BY StartedPlannedWorkID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@PlannedWorksID", MySqlDbType.Int64).Value = plannedWorksId;
                sqlCommand.Parameters.Add("@TaskID", MySqlDbType.Int64).Value = taskId;
                sqlCommand.Parameters.Add("@EmptyWorkReasonID", MySqlDbType.Int32).Value = emptyWorkReasonId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        startedPlannedWorkId = Convert.ToInt32(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return startedPlannedWorkId;
        }

        public void FinishPlannedWorks(long taskId)
        {
            var rows = StartedPlannedWorksTable.Select(string.Format("TaskID = {0}", taskId));
            if (!rows.Any()) return;

            var startedPlannedWorks = rows.First();
            var plannedWorksId = Convert.ToInt64(startedPlannedWorks["PlannedWorksID"]);
            var plannedWorksRows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (!plannedWorksRows.Any()) return;

            var plannedWorks = plannedWorksRows.First();
            var isReloadEnable = Convert.ToBoolean(plannedWorks["IsReloadEnable"]);

            if (isReloadEnable)
                ActivatePlannedWorks(plannedWorksId);
        }

        public void DeleteStartedPlannedWorks(long taskId)
        {
            var rows = StartedPlannedWorksTable.Select(string.Format("TaskID = {0}", taskId));
            if (!rows.Any()) return;

            var startedPlannedWorks = rows.First();
            var plannedWorksId = Convert.ToInt64(startedPlannedWorks["PlannedWorksID"]);
            var plannedWorksRows = PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
            if (!plannedWorksRows.Any()) return;

            var plannedWorks = plannedWorksRows.First();
            var isReloadEnable = Convert.ToBoolean(plannedWorks["IsReloadEnable"]);

            if (isReloadEnable)
                ActivatePlannedWorks(plannedWorksId);

            startedPlannedWorks.Delete();
            UpdateStartedPlannedWorks();
        }

        private void UpdateStartedPlannedWorks()
        {
            try
            {
                _startedPlannedWorksAdapter.Update(StartedPlannedWorksTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[PLWC0008] Не удалось обновить данные на сервере. " +
                                "\nВ случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
