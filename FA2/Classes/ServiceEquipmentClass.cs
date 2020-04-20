using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public struct CrashMachineInfo
    {
        public int MachineId;
        public DateTime RequestDate;
        public string CrashReason;
    }

    public class ServiceEquipmentClass
    {
        public enum RequestType
        {
            Crash = 1, Truble
        }

        private static string _connectionString;

        private DataTable _table;
        private MySqlDataAdapter _adapter;

        private DateTime _dateFrom;
        private DateTime _dateTo;

        public DateTime DateFrom
        {
            get { return _dateFrom; }
        }

        public DateTime DateTo
        {
            get { return _dateTo; }
        }


        private MySqlConnection _serviceEquipmentSqlConnection;

        public DataTable Table
        {
            set { _table = value; }
            get 
            { 
                if(_table == null || _table.Columns.Count == 0)
                {
                    var currentTime = App.BaseClass.GetDateFromSqlServer();
                    Fill(currentTime.Subtract(new TimeSpan(31, 0, 0, 0)), currentTime);
                }

                return _table; 
            }
        }

        public readonly RequestTypesClass RequestTypes;
        public readonly DamageTypesClass DamageTypes;
        public readonly ServiceJournalClass ServiceJournal;
        public readonly TimeMeasuresClass TimeMeasures;
        public readonly ServiceHistoryClass ServiceHistory;
        public readonly ServiceResponsibilitiesClass ServiceResponsibilities;

        public ServiceEquipmentClass(string connectionString)
        {
            _connectionString = connectionString;
            RequestTypes = new RequestTypesClass();
            DamageTypes = new DamageTypesClass();
            ServiceJournal = new ServiceJournalClass(_connectionString);
            TimeMeasures = new TimeMeasuresClass(_connectionString);
            ServiceHistory = new ServiceHistoryClass(_connectionString);
            ServiceResponsibilities = new ServiceResponsibilitiesClass(_connectionString);
            Create();
        }

        private void Create()
        {
            _table = new DataTable();
        }

        public void Fill(DateTime dateFrom, DateTime dateTo)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            Fill();
        }

        private void Fill()
        {
            try
            {
                _serviceEquipmentSqlConnection = new MySqlConnection(_connectionString);

                const string commandText =
                    @"SELECT CrashMachineID, GlobalID, WorkerGroupID, FactoryID, WorkUnitID, WorkSectionID, 
                      WorkSubSectionID, RequestDate, RequestWorkerID, RequestNotes, 
                      ReceivedDate, ReceivedWorkerID, ReceivedNotes, CompletionDate, 
                      CompletionWorkerID, CompletionNotes, LaunchDate, LaunchWorkerID, 
                      LaunchNotes, DamageTypeID, CrashReason, PlannedLaunchDate, 
                      CompletionPercent, RequestClose, RequestTypeID, EditingDate, EditingWorkerID 
                      FROM FAIIServiceEquipment.CrashMachines 
                      WHERE (RequestDate >= @DateFrom AND RequestDate < @DateTo) 
                      OR (LaunchDate >= @DateFrom AND LaunchDate < @DateTo) 
                      ORDER BY RequestClose, RequestDate";

                var command = new MySqlCommand(commandText, _serviceEquipmentSqlConnection);

                command.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = _dateFrom;
                command.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = _dateTo.AddDays(1);

                _adapter = new MySqlDataAdapter(command);
                new MySqlCommandBuilder(_adapter);

                _table.Clear();
                _adapter.Fill(_table);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SE0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SE0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable GetServiceEquipmentDurringTheMonth(int year, int month)
        {
            var table = new DataTable();
            const string commandText =
                    @"SELECT CrashMachineID, GlobalID, WorkerGroupID, FactoryID, WorkUnitID, WorkSectionID, 
                      WorkSubSectionID, RequestDate, RequestWorkerID, RequestNotes, 
                      ReceivedDate, ReceivedWorkerID, ReceivedNotes, CompletionDate, 
                      CompletionWorkerID, CompletionNotes, LaunchDate, LaunchWorkerID, 
                      LaunchNotes, DamageTypeID, CrashReason, PlannedLaunchDate, 
                      CompletionPercent, RequestClose, RequestTypeID, EditingDate, EditingWorkerID 
                      FROM FAIIServiceEquipment.CrashMachines 
                      WHERE (Year(RequestDate) = @Year AND Month(RequestDate) = @Month) OR
                      (Year(LaunchDate) = @Year AND Month(LaunchDate) = @Month)";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@Year", MySqlDbType.Int32).Value = year;
                command.Parameters.Add("@Month", MySqlDbType.Int32).Value = month;

                using (var sqlDataAdapter = new MySqlDataAdapter(command))
                {
                    sqlDataAdapter.Fill(table);
                }
            }

            return table;
        }

        public int AddNewRequest(int workerGroupId, object factoryId, int workUnitId, int workSectionId, int workSubSectionId,
                                  DateTime requestDate, int workerId, string requestNote, int requestTypeId)
        {
            var id = 0;
            var newDr = Table.NewRow();
            newDr["GlobalID"] = "0100000";
            newDr["WorkerGroupID"] = workerGroupId;
            newDr["FactoryID"] = factoryId;
            newDr["WorkUnitID"] = workUnitId;
            newDr["WorkSectionID"] = workSectionId;
            newDr["WorkSubSectionID"] = workSubSectionId;
            newDr["RequestDate"] = requestDate;
            newDr["RequestWorkerID"] = workerId;
            newDr["RequestNotes"] = requestNote;
            newDr["RequestTypeID"] = requestTypeId;
            newDr["CompletionPercent"] = 0;
            newDr["RequestClose"] = false;
            Table.Rows.Add(newDr);

            Update();
            Fill();

            var dr = Table.Select("RequestDate = " + GetFilter(requestDate));
            if (dr.Length != 0)
            {
                dr[0]["GlobalID"] = "01" + Convert.ToInt32(dr[0]["CrashMachineID"]).ToString("00000");
                id = Convert.ToInt32(dr[0]["CrashMachineID"]);
            }
            Update();
            return id;
        }

        public void FillReceivedInfo(int crashMachineId, DateTime receivedDate, int receivedWorkerId, string receivedNote)
        {
            var receivedRow = Table.Select("CrashMachineID = " + crashMachineId)[0];
            receivedRow["ReceivedDate"] = receivedDate;
            receivedRow["ReceivedWorkerID"] = receivedWorkerId;
            receivedRow["ReceivedNotes"] = receivedNote;
            Update();
        }

        public void FillCompletionInfo(string globalId, DateTime completionDate, int completionWorkerId, string completionNote)
        {
            var dataRows = Table.Select("GlobalID = " + globalId);
            if (!dataRows.Any())
            {
                CompleteRequest(globalId, completionDate, completionWorkerId, completionNote);
                return;
            }

            var completionRow = dataRows.First();
            completionRow["CompletionDate"] = completionDate;
            completionRow["CompletionWorkerID"] = completionWorkerId;
            completionRow["CompletionNotes"] = completionNote;
            Update();
        }

        private static void CompleteRequest(string globalId, DateTime completionDate, int completionWorkerId,
            string completionNote)
        {
            const string sqlCommandText = @"UPDATE FAIIServiceEquipment.CrashMachines 
                                            SET CompletionDate = @CompletionDate, CompletionWorkerID = @CompletionWorkerID 
                                            WHERE GlobalID = @GlobalID";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@CompletionDate", MySqlDbType.DateTime).Value = completionDate;
                sqlCommand.Parameters.Add("@CompletionWorkerID", MySqlDbType.Int64).Value = completionWorkerId;
                //sqlCommand.Parameters.Add("@CompletionNotes", SqlDbType.Text).Value = completionNote;
                sqlCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;

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

        public void FillLaunchInfo(int crashMachineId, DateTime launchDate, int launchWorkerId, string launchNote)
        {
            var launchRow = Table.Select("CrashMachineID = " + crashMachineId)[0];
            launchRow["LaunchDate"] = launchDate;
            launchRow["LaunchWorkerID"] = launchWorkerId;
            launchRow["LaunchNotes"] = launchNote;
            launchRow["CompletionPercent"] = 100;
            launchRow["RequestClose"] = true;
            Update();
        }

        public void FillAdditionalInfo(int crashMachineId, object damageTypeId, object crashReason, object plannedLaunchDate,
                                      object completionPercent, DateTime editingDate, int editingWorkerId)
        {
            var additionalRow = Table.Select("CrashMachineID = " + crashMachineId)[0];
            additionalRow["DamageTypeID"] = damageTypeId;
            additionalRow["CrashReason"] = crashReason;
            additionalRow["PlannedLaunchDate"] = plannedLaunchDate;
            additionalRow["CompletionPercent"] = completionPercent;
            additionalRow["EditingDate"] = editingDate;
            additionalRow["EditingWorkerID"] = editingWorkerId;
            Update();
        }

        public void DeleteCrashRow(int crashMachineId)
        {
            var deletingRow = Table.Select("CrashMachineID = " + crashMachineId)[0];
            deletingRow.Delete();
            Update();
        }

        private static string GetFilter(DateTime requestDate)
        {
            var filter = "'" + requestDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            return filter;
        }

        /// <summary>
        /// Возвращает список станков находящихся на ремонте.
        /// </summary>
        /// <param name="type">Тип, определяющий вид заявки.</param>
        /// <returns>Уникальный список идентификаторов станков.</returns>
        public static List<CrashMachineInfo> GetCrashesMachines(RequestType type)
        {
            var crashesMachinesList = new List<CrashMachineInfo>();

            const string sqlCommandText = @"SELECT Machines.MachineID, 
                                          CrashSubSections.RequestDate, 
                                          CrashSubSections.RequestNotes, 
                                          Machines.MachineName, 
                                          Machines.WorkSubSectionID, 
                                          Machines.IsVisible 
                                          FROM FAIICatalog.Machines Machines INNER JOIN 
                                          (SELECT SubSections.WorkSubsectionID, 
                                          SubSections.Visible, 
                                          Crash.RequestDate, 
                                          Crash.RequestNotes 
                                          FROM FAIICatalog.WorkSubsections SubSections INNER JOIN 
                                          (SELECT * FROM FAIIServiceEquipment.CrashMachines 
                                          WHERE RequestClose = False AND RequestTypeID = @RequestTypeID)Crash 
                                          ON SubSections.WorkSubsectionID = Crash.WorkSubSectionID)CrashSubSections 
                                          ON Machines.WorkSubSectionID = CrashSubSections.WorkSubsectionID";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                    sqlCommand.Parameters.Add("@RequestTypeID", MySqlDbType.Int64).Value = type;

                    // Get list of machines, that have some damage
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var machineId = Convert.ToInt32(reader[0]);
                            var requestDate = Convert.ToDateTime(reader[1]);
                            var requestNotes = reader[2].ToString();
                            var crashMachineInfo = new CrashMachineInfo
                                                   {
                                                       MachineId = machineId,
                                                       RequestDate = requestDate,
                                                       CrashReason = requestNotes
                                                   };
                            crashesMachinesList.Add(crashMachineInfo);
                        }

                        reader.Close();
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            return crashesMachinesList.Distinct().ToList();
        }



        public void FillServiceHistory(DateTime dateFrom, DateTime dateTo)
        {
            ServiceHistory.Fill(dateFrom, dateTo);
            var idsArray = ServiceHistory.GetServiceHistoryIds();
            ServiceResponsibilities.Fill(idsArray);
        }

        public void ChangeReceivedNotes(int crashMachineId, string receivedNotes)
        {
            var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("CrashMachineID") == crashMachineId);
            if(!rows.Any()) return;

            var crashRow = rows.First();
            crashRow["ReceivedNotes"] = string.IsNullOrEmpty(receivedNotes) ? (object) DBNull.Value : receivedNotes;
            Update();
        }

        public void ChangeCompletionNotes(int crashMachineId, string completionNotes)
        {
            var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("CrashMachineID") == crashMachineId);
            if (!rows.Any()) return;

            var crashRow = rows.First();
            crashRow["CompletionNotes"] = string.IsNullOrEmpty(completionNotes) ? (object)DBNull.Value : completionNotes;
            Update();
        }


        public class RequestTypesClass
        {
            private DataTable _table;
            private MySqlDataAdapter _adapter;
            private MySqlCommandBuilder _builder;
            public DataTable Table
            {
                set { _table = value; }
                get { return _table; }
            }

            public RequestTypesClass()
            {
                Initialize();
            }

            private void Initialize()
            {
                Create();
                Fill();
            }

            private void Create()
            {
                _table = new DataTable();
                _adapter = new MySqlDataAdapter(@"SELECT RequestTypeID, RequestTypeName 
                                                  FROM FAIIServiceEquipment.RequestTypes", _connectionString);
                _builder = new MySqlCommandBuilder(_adapter);
            }

            private void Fill()
            {
                try
                {
                    _adapter.Fill(_table);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0002] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public class DamageTypesClass
        {
            private DataTable _table;
            private MySqlDataAdapter _adapter;
            private MySqlCommandBuilder _builder;
            public DataTable Table
            {
                set { _table = value; }
                get { return _table; }
            }

            public DamageTypesClass()
            {
                Initialize();
            }

            private void Initialize()
            {
                Create();
                Fill();
            }

            private void Create()
            {
                _table = new DataTable();
                _adapter = new MySqlDataAdapter(@"SELECT DamageTypeID, DamageTypeName 
                                                  FROM FAIIServiceEquipment.DamageTypes", _connectionString);
                _builder = new MySqlCommandBuilder(_adapter);
            }

            private void Fill()
            {
                try
                {
                    _adapter.Fill(_table);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0003] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public class ServiceJournalClass
        {
            private string _connectionString;

            private DataTable _table;
            public DataTable Table
            {
                get { return _table; }
                set { _table = value; }
            }

            private MySqlDataAdapter _adapter;

            public ServiceJournalClass(string connectionString)
            {
                _connectionString = connectionString;

                Create();
                Fill();
            }

            private void Create()
            {
                Table = new DataTable();
            }

            private void Fill()
            {
                _adapter = new MySqlDataAdapter(@"SELECT ServiceJournalID, ActionName, TimeInterval, TimeMeasureID, 
                    LastDate, NextDate, Description, FactoryID, MachineID, EditingWorkerID, EditingDate, 
                    NotificationInterval, NotificationTimeMeasureID, ShowNotification, IsEnabled 
                    FROM FAIIServiceEquipment.ServiceJournal ORDER BY NextDate", _connectionString);
                try
                {
                    _table.Clear();
                    if (_table.Columns.Contains("IsOverdue"))
                    {
                        _table.Columns.Remove("IsOverdue");
                    }

                    new MySqlCommandBuilder(_adapter);
                    _adapter.Fill(_table);
                    AddOverdueField();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void AddOverdueField()
            {
                var overdueColumn = new DataColumn("IsOverdue", typeof(bool))
                {
                    AllowDBNull = false,
                    DefaultValue = false
                };
                Table.Columns.Add(overdueColumn);
                var currentDate = App.BaseClass.GetDateFromSqlServer();
                foreach (var row in Table.AsEnumerable().Where(r => r.Field<DateTime>("NextDate") < currentDate))
                {
                    row["IsOverdue"] = true;
                    //var lastDate = Convert.ToDateTime(row["LastDate"]);
                    //var nextDate = Convert.ToDateTime(row["NextDate"]);
                    //var interval = nextDate.Subtract(lastDate);
                    //while(nextDate < currentDate)
                    //{
                    //    row["LastDate"] = nextDate;
                    //    nextDate = nextDate.Add(interval);
                    //    row["NextDate"] = nextDate;
                    //}
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
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void Add(string actionName, int timeInterval, int measureId, DateTime lastDate, DateTime nextDate,
                string description, int factoryId, int machineId, int editingWorkerId, DateTime editingDate,
                int notificationInterval = 0, int notificationMeasureId = 0, bool showNotification = false)
            {
                var newRow = Table.NewRow();
                newRow["ActionName"] = actionName;
                newRow["TimeInterval"] = timeInterval;
                newRow["TimeMeasureID"] = measureId;
                newRow["LastDate"] = lastDate;
                newRow["NextDate"] = nextDate;
                newRow["Description"] = description;
                newRow["FactoryID"] = factoryId;
                newRow["MachineID"] = machineId;
                newRow["EditingWorkerID"] = editingWorkerId;
                newRow["EditingDate"] = editingDate;

                if (showNotification)
                {
                    newRow["NotificationInterval"] = notificationInterval;
                    newRow["NotificationTimeMeasureID"] = notificationMeasureId;
                    newRow["ShowNotification"] = true;
                }

                Table.Rows.Add(newRow);
                Update();
                Fill();
            }

            public void Change(int serviceJournalId, string actionName, int timeInterval, int measureId,
                DateTime lastDate, DateTime nextDate, string description, int factoryId, int machineId,
                int editingWorkerId, DateTime editingDate, int notificationInterval = 0,
                int notificationMeasureId = 0, bool showNotification = false)
            {
                if (Table.AsEnumerable().All(r => r.Field<Int64>("ServiceJournalID") != serviceJournalId)) return;

                var changedRow = Table.AsEnumerable().First(r => r.Field<Int64>("ServiceJournalID") == serviceJournalId);
                changedRow["ActionName"] = actionName;
                changedRow["TimeInterval"] = timeInterval;
                changedRow["TimeMeasureID"] = measureId;
                changedRow["LastDate"] = lastDate;
                changedRow["NextDate"] = nextDate;
                changedRow["Description"] = description;
                changedRow["FactoryID"] = factoryId;
                changedRow["MachineID"] = machineId;
                changedRow["EditingWorkerID"] = editingWorkerId;
                changedRow["EditingDate"] = editingDate;

                if (showNotification)
                {
                    changedRow["NotificationInterval"] = notificationInterval;
                    changedRow["NotificationTimeMeasureID"] = notificationMeasureId;
                    changedRow["ShowNotification"] = true;
                }
                else
                {
                    changedRow["NotificationInterval"] = DBNull.Value;
                    changedRow["NotificationTimeMeasureID"] = DBNull.Value;
                    changedRow["ShowNotification"] = false;
                }

                var currentDate = App.BaseClass.GetDateFromSqlServer();
                changedRow["IsOverdue"] = nextDate < currentDate;

                Update();
            }

            public void ChangeLastDate(int serviceJournalId, DateTime lastDate)
            {
                var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("ServiceJournalID") == serviceJournalId);
                if (!rows.Any()) return;

                var changeRow = rows.First();
                var timeInterval = Convert.ToInt32(changeRow["TimeInterval"]);
                var measureId = Convert.ToInt32(changeRow["TimeMeasureID"]);
                var nextDate = CalculateNextDate(lastDate, timeInterval, measureId);

                changeRow["LastDate"] = lastDate;
                changeRow["NextDate"] = nextDate;

                Update();
            }

            public void Delete(int serviceJournalId)
            {
                if (Table.AsEnumerable().All(r => r.Field<Int64>("ServiceJournalID") != serviceJournalId)) return;

                var deletingRow = Table.AsEnumerable().First(r => r.Field<Int64>("ServiceJournalID") == serviceJournalId);
                deletingRow["IsEnabled"] = false;

                Update();
            }

            public static DateTime CalculateNextDate(DateTime lastDate, int timeInterval, int timeMeasureId)
            {
                var nextDate = lastDate;

                switch (timeMeasureId)
                {
                    case 1:
                        nextDate = lastDate.AddHours(timeInterval);
                        break;
                    case 2:
                        nextDate = lastDate.AddDays(timeInterval);
                        break;
                    case 3:
                        nextDate = lastDate.AddMonths(timeInterval);
                        break;
                    case 4:
                        nextDate = lastDate.AddYears(timeInterval);
                        break;
                }

                return nextDate;
            }
        }


        public class TimeMeasuresClass
        {
            private string _connectionString;
            private DataTable _table;
            public DataTable Table
            {
                get { return _table; }
                set { _table = value; }
            }

            private MySqlDataAdapter _adapter;

            public TimeMeasuresClass(string connectionString)
            {
                _connectionString = connectionString;

                Create();
                Fill();
            }

            private void Create()
            {
                Table = new DataTable();
            }

            private void Fill()
            {
                _adapter = new MySqlDataAdapter(@"SELECT TimeMeasureID, TimeMeasureName 
                                                  FROM FAIIServiceEquipment.TimeMeasures", _connectionString);
                try
                {
                    Table.Clear();
                    _adapter.Fill(_table);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public class ServiceHistoryClass
        {
            private string _connectionString;
            private DataTable _table;
            public DataTable Table
            {
                get
                {
                    if (_table.Columns.Count == 0)
                    {
                        var currentDate = App.BaseClass.GetDateFromSqlServer();
                        var dateFrom = currentDate.Subtract(new TimeSpan(30, 0, 0, 0));
                        Fill(dateFrom, currentDate);
                    }
                    return _table;
                }
                set { _table = value; }
            }
            private MySqlDataAdapter _adapter;

            private DateTime _dateFrom;
            private DateTime _dateTo;

            public ServiceHistoryClass(string connectionString)
            {
                _connectionString = connectionString;
                Create();
            }

            private void Create()
            {
                Table = new DataTable();
            }

            public void Fill(DateTime dateFrom, DateTime dateTo)
            {
                _dateFrom = dateFrom;
                _dateTo = dateTo;

                var sqlConn = new MySqlConnection(_connectionString);
                const string commandText =
                    @"SELECT ServiceHistoryID, GlobalID, DateCreate, MainWorkerID, ServiceJournalID, 
                      Description, DateConfirm, ConfirmWorkerID, NeededConfirmationDate, IsOverdue, IsClosing
                      FROM FAIIServiceEquipment.ServiceHistory
                      WHERE DateConfirm >= @DateFrom AND DateConfirm < @DateTo OR IsClosing = False 
                      ORDER BY IsClosing, NeededConfirmationDate";
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                command.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo;

                _adapter = new MySqlDataAdapter(command);
                new MySqlCommandBuilder(_adapter);
                try
                {
                    _table.Clear();
                    _adapter.Fill(_table);
                    FindOverdueRows();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void FindOverdueRows()
            {
                var currentTime = App.BaseClass.GetDateFromSqlServer();

                foreach (var row in _table.AsEnumerable().
                    Where(r => !r.Field<bool>("IsOverdue") && !r.Field<bool>("IsClosing") &&
                        r.Field<DateTime>("NeededConfirmationDate") < currentTime))
                {
                    row["IsOverdue"] = true;
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
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0007] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void Add(DateTime dateCreated, int mainWorkerId, int serviceJournalId, string description,
                DateTime neededConfirmationDate,
                bool isOverdue = false, bool isClosed = false)
            {
                var newRow = Table.NewRow();
                newRow["DateCreate"] = dateCreated;
                newRow["MainWorkerID"] = mainWorkerId;
                newRow["ServiceJournalID"] = serviceJournalId;
                newRow["Description"] = description;
                newRow["NeededConfirmationDate"] = neededConfirmationDate;

                var currentTime = App.BaseClass.GetDateFromSqlServer();
                newRow["IsOverdue"] = neededConfirmationDate < currentTime;

                Table.Rows.Add(newRow);
                Update();

                FillGlobalId(mainWorkerId, dateCreated);

                Fill(_dateFrom, _dateTo);
            }

            private void FillGlobalId(int mainWorkerId, DateTime creationDate)
            {
                const string updateCommandText =
                    @"UPDATE FAIIServiceEquipment.ServiceHistory SET GlobalID = @GlobalID 
                      WHERE MainWorkerID = @MainWorkerID AND DateCreate = @DateCreate";

                var serviceHistoryId = GetServiceHistoryId(mainWorkerId, creationDate);

                using (var conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();

                        var globalId = string.Format("0{0}{1:00000}", (int) TaskClass.SenderApplications.ServiceJournal,
                            serviceHistoryId);

                        var updateCommand = new MySqlCommand(updateCommandText, conn);
                        updateCommand.Parameters.Add("@MainWorkerID", MySqlDbType.Int64).Value = mainWorkerId;
                        updateCommand.Parameters.Add("@DateCreate", MySqlDbType.DateTime).Value = creationDate;
                        updateCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;

                        updateCommand.ExecuteScalar();
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

            public static int GetServiceHistoryId(int mainWorkerId, DateTime creationDate)
            {
                var serviceHistoryId = -1;

                const string sqlCommandText =
                    @"SELECT ServiceHistoryID FROM FAIIServiceEquipment.ServiceHistory 
                      WHERE MainWorkerID = @MainWorkerID AND DateCreate = @DateCreate";

                using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
                {
                    try
                    {
                        conn.Open();

                        var selectCommand = new MySqlCommand(sqlCommandText, conn);
                        selectCommand.Parameters.Add("@MainWorkerID", MySqlDbType.Int64).Value = mainWorkerId;
                        selectCommand.Parameters.Add("@DateCreate", MySqlDbType.DateTime).Value = creationDate;

                        var result = selectCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            serviceHistoryId = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return serviceHistoryId;
            }

            public static int GetServiceHistoryId(string globalId)
            {
                var serviceHistoryId = -1;

                const string sqlCommandText =
                    @"SELECT ServiceHistoryID FROM FAIIServiceEquipment.ServiceHistory 
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
                            serviceHistoryId = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return serviceHistoryId;
            }

            public void ChangeRow(int serviceHistoryId, string description, DateTime neededConfirmationDate)
            {
                var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);
                if (!rows.Any()) return;

                var changeRow = rows.First();
                changeRow["Description"] = description;
                changeRow["NeededConfirmationDate"] = neededConfirmationDate;

                var currentTime = App.BaseClass.GetDateFromSqlServer();
                changeRow["IsOverdue"] = neededConfirmationDate < currentTime;

                Update();
            }

            public void Delete(int serviceHistoryId)
            {
                var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);
                if (!rows.Any()) return;

                var deletingRow = rows.First();

                deletingRow.Delete();
                Update();
            }

            public List<long> GetServiceHistoryIds()
            {
                var filePaths =
                    from serviceHistoryView in Table.AsEnumerable()
                    select serviceHistoryView.Field<Int64>("ServiceHistoryID");

                var serviceHistoryIdsList = filePaths.ToList();

                return serviceHistoryIdsList.Count == 0 ? null : serviceHistoryIdsList;
            }

            public void Confirm(int serviceHistoryId, DateTime confirmationDate, int workerId)
            {
                var rows = Table.AsEnumerable().Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);
                if (!rows.Any()) return;

                var confirmRow = rows.First();
                var neededConfirmationDate = Convert.ToDateTime(confirmRow["NeededConfirmationDate"]);
                confirmRow["DateConfirm"] = confirmationDate;
                confirmRow["ConfirmWorkerID"] = workerId;
                confirmRow["IsOverdue"] = neededConfirmationDate < confirmationDate;
                confirmRow["IsClosing"] = true;

                Update();
            }
        }


        public class ServiceResponsibilitiesClass
        {
            private List<long> _serviceHistoryIds;
            private string _connectionString;
            private DataTable _table;
            public DataTable Table
            {
                get { return _table; }
                set { _table = value; }
            }

            private MySqlDataAdapter _adapter;

            public ServiceResponsibilitiesClass(string connectionString)
            {
                _connectionString = connectionString;

                Create();
            }

            private void Create()
            {
                Table = new DataTable();
            }

            public void Fill(List<long> serviceHistoryIds)
            {
                if (serviceHistoryIds == null || serviceHistoryIds.Count == 0) return;

                _serviceHistoryIds = serviceHistoryIds;
                var commandText = GetCommandText(serviceHistoryIds);

                _adapter = new MySqlDataAdapter(commandText, _connectionString);
                new MySqlCommandBuilder(_adapter);
                try
                {
                    Table.Clear();
                    _adapter.Fill(_table);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0008] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void Update()
            {
                try
                {
                    _adapter.Update(_table);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[SE0009] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void Add(int serviceHistoryId, int workerId, bool showNotification)
            {
                var newRow = Table.NewRow();
                newRow["ServiceHistoryID"] = serviceHistoryId;
                newRow["WorkerID"] = workerId;
                Table.Rows.Add(newRow);

                if (_serviceHistoryIds.All(id => id != serviceHistoryId))
                    _serviceHistoryIds.Add(serviceHistoryId);

                Update();
                Fill(_serviceHistoryIds);
            }

            public void Change(int serviceResponsibilityId, bool showNotification, bool update = true)
            {
                var rows =
                    Table.AsEnumerable()
                        .Where(r => r.Field<Int64>("ServiceResponsibilityID") == serviceResponsibilityId);
                if (!rows.Any()) return;

                var changeRow = rows.First();
                changeRow["ShowNotification"] = showNotification;

                if (update)
                    Update();
            }

            public void Delete(int serviceResponsibilityId, bool update = true)
            {
                var rows =
                    Table.AsEnumerable()
                        .Where(r => r.Field<Int64>("ServiceResponsibilityID") == serviceResponsibilityId);
                if (!rows.Any()) return;

                var deletingRow = rows.First();

                deletingRow.Delete();

                if (update)
                    Update();
            }

            private static string GetCommandText(ICollection serviceHistoryIds)
            {
                string commandText;

                if (serviceHistoryIds.Count != 0)
                {
                    // ServiceHistory table has rows
                    var serviceHistoryIdsString = string.Empty;
                    serviceHistoryIdsString = (serviceHistoryIds.Cast<object>()
                        .Aggregate(serviceHistoryIdsString,
                            (current, serviceHistoryId) => current + ", " + serviceHistoryId))
                        .Remove(0, 2);
                    commandText = @"SELECT ServiceResponsibilityID, ServiceHistoryID, WorkerID, TimeStart, 
                                    TimeEnd, WorkerDescription, IsClosing, IsRequestAccepted, AcceptedDate, ShowNotification 
                                    FROM FAIIServiceEquipment.ServiceResponsibilities 
                                    WHERE ServiceHistoryID IN (" + serviceHistoryIdsString + ")";
                }
                else
                {
                    // ServiceHistory table is empty, 
                    // but we need to give columns view to ServiceResponsibilities table
                    commandText =
                        @"SELECT ServiceResponsibilityID, ServiceHistoryID, WorkerID, TimeStart, 
                          TimeEnd, WorkerDescription, IsClosing, IsRequestAccepted, AcceptedDate, ShowNotification] 
                          FROM FAIIServiceEquipment.ServiceResponsibilities LIMIT 0";
                }

                return commandText;
            }

            public static void AcceptAndStart(string globalId, int workerId, DateTime startDate)
            {
                var serviceHistoryId = ServiceHistoryClass.GetServiceHistoryId(globalId);

                const string sqlCommandText = @"UPDATE FAIIServiceEquipment.ServiceResponsibilities 
                                                SET TimeStart = @TimeStart, AcceptedDate = @AcceptedDate 
                                                WHERE ServiceHistoryID = @ServiceHistoryID AND WorkerID = @WorkerID";

                using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
                {
                    var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                    sqlCommand.Parameters.Add("@ServiceHistoryID", MySqlDbType.Int64).Value = serviceHistoryId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                    sqlCommand.Parameters.Add("@TimeStart", MySqlDbType.DateTime).Value = startDate;
                    sqlCommand.Parameters.Add("@AcceptedDate", MySqlDbType.DateTime).Value = startDate;

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

            public static void Complete(string globalId, int workerId, DateTime endDate)
            {
                var serviceHistoryId = ServiceHistoryClass.GetServiceHistoryId(globalId);

                const string sqlCommandText = @"UPDATE FAIIServiceEquipment.ServiceResponsibilities 
                                                SET TimeEnd = @TimeEnd, IsClosing = TRUE 
                                                WHERE ServiceHistoryID = @ServiceHistoryID AND WorkerID = @WorkerID";

                using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
                {
                    var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                    sqlCommand.Parameters.Add("@ServiceHistoryID", MySqlDbType.Int64).Value = serviceHistoryId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                    sqlCommand.Parameters.Add("@TimeEnd", MySqlDbType.DateTime).Value = endDate;

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
        }
    }
}
