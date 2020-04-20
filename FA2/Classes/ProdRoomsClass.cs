using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using FA2.Converters;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class ProdRoomsClass
    {
        public LockClass Locks;
        public ActionClass Actions;

        private static string _connectionString;

        private DataTable _openingTimeSheetDataTable;
        private DataTable _openingTimeDataTable;
        private MySqlDataAdapter _openingTimeAdapter;

        private DataTable _closingTimeSheetDataTable;
        private DataTable _closingTimeDataTable;
        private MySqlDataAdapter _closingTimeAdapter;

        private DataTable _weekendsResponsiblesTable;
        private DataTable _weekendResponsiblesTimeSheetTable;
        private MySqlDataAdapter _weekendsResponsiblesAdapter;

        public DataTable ResponsibleArrivesTable;
        private MySqlDataAdapter _responsibleArrivesAdapter;

        public DataTable JournalProductionsTable;
        private MySqlDataAdapter _journalProductionAdapter;

        public DataTable ConfirmDataTable;
        private MySqlDataAdapter _confirmAdapter;

        private MySqlDataAdapter _reportAdapter;
        private DataTable _reportTable;
        public DataTable ReportTable
        {
            get
            {
                if (_reportTable.Columns.Count == 0)
                    FillWorkerReports();
                return _reportTable;
            }
            set { _reportTable = value; }
        }

        private int _selectedWorkerReportYear;
        private int _selectedWorkerReportMonth;
        public int SelectedWorkerReportYear
        {
            get { return _selectedWorkerReportYear; }
        }
        public int SelectedWorkerReportMonth
        {
            get { return _selectedWorkerReportMonth; }
        }

        private static int _selectedTimeSheetYear;
        private static int _selectedTimeSheetMonth;
        public int SelectedTimeSheetYear
        {
            get { return _selectedTimeSheetYear; }
        }
        public int SelectedTimeSheetMonth
        {
            get { return _selectedTimeSheetMonth; }
        }

        private int _selectedWeekendTimeSheetYear;
        private int _selectedWeekendTimeSheetMonth;
        public int SelectedWeekendTimeSheetYear
        {
            get { return _selectedWeekendTimeSheetYear; }
        }
        public int SelectedWeekendTimeSheetMonth
        {
            get { return _selectedWeekendTimeSheetMonth; }
        }

        private int _selectedResponsibleArriveYear;
        private int _selectedResponsibleArriveMonth;
        public int SelectedResponsibleArriveYear
        {
            get { return _selectedResponsibleArriveYear; }
        }
        public int SelectedResponsibleArriveMonth
        {
            get { return _selectedResponsibleArriveMonth; }
        }

        private int _journalYear;
        private int _journalMonth;

        public ProdRoomsClass(string connectionString)
        {
            _connectionString = connectionString;
            Initialize();
        }

        private void Initialize()
        {
            Create();
        }

        private void Create()
        {
            _openingTimeDataTable = new DataTable();
            _closingTimeDataTable = new DataTable();

            _weekendsResponsiblesTable = new DataTable();
            _weekendResponsiblesTimeSheetTable = new DataTable();

            ResponsibleArrivesTable = new DataTable();

            JournalProductionsTable = new DataTable();
            ConfirmDataTable = new DataTable();
            _reportTable = new DataTable();

            Locks = new LockClass();
            Actions = new ActionClass();
        }

        public void FillTimeSheet(int year, int month)
        {
            _selectedTimeSheetYear = year;
            _selectedTimeSheetMonth = month;

            FillOpeningTimeTable();
            FillClosingTimeTable();
        }

        public void FillWeekendTimeSheet(int year, int month)
        {
            _selectedWeekendTimeSheetYear = year;
            _selectedWeekendTimeSheetMonth = month;

            FillWeekendsResponsibles(year, month);
        }



        #region Journal time sheet


        #region Opening time sheet

        private void FillOpeningTimeTable()
        {
            try
            {
                _openingTimeAdapter =
                    new MySqlDataAdapter(
                        @"SELECT OpeningTimeTableID, WorkerID, Date, EditingDate, 
                          EditingWorkerID FROM FAIIProdRooms.OpeningTimeTable 
                          WHERE year(Date) = " + _selectedTimeSheetYear + " AND month(Date) = " +
                        _selectedTimeSheetMonth,
                        _connectionString);
                new MySqlCommandBuilder(_openingTimeAdapter);
                _openingTimeDataTable.Clear();
                _openingTimeAdapter.Fill(_openingTimeDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable GetOpeningTimeSheeTable()
        {
            _openingTimeSheetDataTable = new DataTable();
            var daysCount = DateTime.DaysInMonth(_selectedTimeSheetYear, _selectedTimeSheetMonth);

            // Add worker column
            _openingTimeSheetDataTable.Columns.Add("WorkerID", typeof (string));

            // Add days of month columns
            for (var i = 1; i < daysCount + 1; i++)
            {
                _openingTimeSheetDataTable.Columns.Add(i.ToString(CultureInfo.InvariantCulture), typeof (bool));
            }

            if (!_openingTimeDataTable.AsEnumerable().Any())
            {
                _openingTimeSheetDataTable.Rows.Add(string.Empty);
                return _openingTimeSheetDataTable;
            }

            var distinctWorkers =
                (_openingTimeDataTable.AsEnumerable().Select(names =>
                    new
                    {
                        WorkerID =
                            names.Field<Int64>("WorkerID")
                    })).Distinct().ToArray();

            foreach (var worker in distinctWorkers)
            {
                var openingDR = _openingTimeSheetDataTable.NewRow();
                openingDR[0] = worker.WorkerID.ToString(CultureInfo.InvariantCulture);

                var workerId = worker.WorkerID;
                foreach (
                    var day in
                        Enumerable.Select(
                            _openingTimeDataTable.AsEnumerable()
                                .Where(oT => oT.Field<Int64>("WorkerID") == workerId),
                            row => Convert.ToDateTime(row["Date"]).Day))
                {
                    openingDR[day] = true;
                }

                _openingTimeSheetDataTable.Rows.Add(openingDR);
            }


            return _openingTimeSheetDataTable;
        }

        public void AddNewWorkerToOpeningTimeSheet(int workerID)
        {
            if (_openingTimeSheetDataTable.Rows.Count != 0)
                if ((string)_openingTimeSheetDataTable.Rows[0]["WorkerID"] == string.Empty)
                {
                    _openingTimeSheetDataTable.Rows[0]["WorkerID"] = workerID;
                    return;
                }

            if (
                _openingTimeSheetDataTable.DefaultView.Cast<DataRowView>()
                    .Any(drv => Convert.ToInt32(drv.Row[0]) == workerID))
                return;

            var newDR = _openingTimeSheetDataTable.NewRow();
            newDR["WorkerID"] = workerID;
            _openingTimeSheetDataTable.Rows.Add(newDR);
        }

        public void DeleteWorkerFromOpeningTimeSheet(int workerID)
        {
            var deletingRow = _openingTimeSheetDataTable.Select("WorkerID = " + workerID)[0];
            _openingTimeSheetDataTable.Rows.Remove(deletingRow);
        }

        public void SaveOpeningTimeSheetChanges()
        {
            if (_openingTimeSheetDataTable.Rows.Count != 0)
                if ((string) _openingTimeSheetDataTable.Rows[0]["WorkerID"] == string.Empty) return;

            var currentTime = App.BaseClass.GetDateFromSqlServer();
            var daysCount = DateTime.DaysInMonth(_selectedTimeSheetYear, _selectedTimeSheetMonth);

            foreach (var deletingDR in _openingTimeDataTable.AsEnumerable())
            {
                deletingDR.Delete();
            }

            foreach (var dataRow in _openingTimeSheetDataTable.AsEnumerable())
            {
                // First row is workerId, sow we need to start from index 1
                for (var i = 1; i < daysCount + 1; i++)
                {
                    if (dataRow[i] != DBNull.Value && Convert.ToBoolean(dataRow[i]))
                    {
                        AddNewRow(Convert.ToInt32(dataRow[0]),
                            new DateTime(_selectedTimeSheetYear, _selectedTimeSheetMonth, i),
                            AdministrationClass.CurrentWorkerId, currentTime, _openingTimeDataTable);
                    }
                }
            }

            UpdateOpeningTimeTable();
        }

        private void UpdateOpeningTimeTable()
        {
            try
            {
                _openingTimeAdapter.Update(_openingTimeDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool HasWorkerDateOpeningSchedule(long workerId, DateTime date)
        {
            var hasSchedule = _openingTimeDataTable.AsEnumerable().
                Where(r => r.Field<Int64>("WorkerID") == workerId && r.Field<DateTime>("Date").Date == date.Date).Any();
            return hasSchedule;
        }

        #endregion


        #region Closing time sheet

        private void FillClosingTimeTable()
        {
            try
            {
                _closingTimeAdapter =
                    new MySqlDataAdapter(
                        @"SELECT ClosingTimeTableID, WorkerID, Date, EditingDate, 
                          EditingWorkerID FROM FAIIProdRooms.ClosingTimeTable 
                          WHERE year(Date) = " + _selectedTimeSheetYear + " AND month(Date) = " +
                        _selectedTimeSheetMonth,
                        _connectionString);
                new MySqlCommandBuilder(_closingTimeAdapter);
                _closingTimeDataTable.Clear();
                _closingTimeAdapter.Fill(_closingTimeDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0003]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable GetClosingTimeSheeTable()
        {
            _closingTimeSheetDataTable = new DataTable();
            var daysCount = DateTime.DaysInMonth(_selectedTimeSheetYear, _selectedTimeSheetMonth);
            _closingTimeSheetDataTable.Columns.Add("WorkerID", typeof (string));

            for (var i = 1; i < daysCount + 1; i++)
            {
                _closingTimeSheetDataTable.Columns.Add(i.ToString(CultureInfo.InvariantCulture), typeof (bool));
            }

            if (!_closingTimeDataTable.AsEnumerable().Any())
            {
                _closingTimeSheetDataTable.Rows.Add(string.Empty);
                return _closingTimeSheetDataTable;
            }

            var distinctWorkers =
                (_closingTimeDataTable.AsEnumerable().Select(names =>
                    new
                    {
                        WorkerID =
                            names.Field<Int64>("WorkerID")
                    })).Distinct().ToArray();

            foreach (var worker in distinctWorkers)
            {
                var openingDR = _closingTimeSheetDataTable.NewRow();
                openingDR[0] = worker.WorkerID.ToString(CultureInfo.InvariantCulture);

                var workerId = worker.WorkerID;
                foreach (
                    var day in
                        Enumerable.Select(
                            _closingTimeDataTable.AsEnumerable()
                                .Where(oT => oT.Field<Int64>("WorkerID") == workerId),
                            row => Convert.ToDateTime(row["Date"]).Day))
                {
                    openingDR[day] = true;
                }

                _closingTimeSheetDataTable.Rows.Add(openingDR);
            }
            return _closingTimeSheetDataTable;
        }

        public void AddNewWorkerToClosingTimeSheet(int workerID)
        {
            if (_closingTimeSheetDataTable.Rows.Count != 0)
                if ((string) _closingTimeSheetDataTable.Rows[0]["WorkerID"] == string.Empty)
                {
                    _closingTimeSheetDataTable.Rows[0][0] = workerID;
                    return;
                }

            if (
                _closingTimeSheetDataTable.DefaultView.Cast<DataRowView>()
                    .Any(drv => Convert.ToInt32(drv.Row[0]) == workerID))
                return;

            var newDR = _closingTimeSheetDataTable.NewRow();
            newDR["WorkerID"] = workerID;
            _closingTimeSheetDataTable.Rows.Add(newDR);
        }

        public void DeleteWorkerFromClosingTimeSheet(int workerID)
        {
            var deletingRow = _closingTimeSheetDataTable.Select("WorkerID = " + workerID)[0];
            _closingTimeSheetDataTable.Rows.Remove(deletingRow);
        }

        public void SaveClosingTimeSheetChanges()
        {
            if (_closingTimeSheetDataTable.Rows.Count != 0)
                if ((string) _closingTimeSheetDataTable.Rows[0]["WorkerID"] == string.Empty) return;

            var currentTime = App.BaseClass.GetDateFromSqlServer();
            var daysCount = DateTime.DaysInMonth(_selectedTimeSheetYear, _selectedTimeSheetMonth);

            foreach (var deletingDR in _closingTimeDataTable.AsEnumerable())
            {
                deletingDR.Delete();
            }
            foreach (var dataRow in _closingTimeSheetDataTable.AsEnumerable())
            {
                for (var i = 1; i < daysCount + 1; i++)
                {
                    if (dataRow[i] != DBNull.Value && Convert.ToBoolean(dataRow[i]))
                    {
                        AddNewRow(Convert.ToInt32(dataRow[0]),
                            new DateTime(_selectedTimeSheetYear, _selectedTimeSheetMonth, i),
                            AdministrationClass.CurrentWorkerId, currentTime, _closingTimeDataTable);
                    }
                }
            }

            UpdateClosingTimeTable();
        }

        private void UpdateClosingTimeTable()
        {
            try
            {
                _closingTimeAdapter.Update(_closingTimeDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0004]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool HasWorkerDateClosingSchedule(long workerId, DateTime date)
        {
            var hasSchedule = _closingTimeDataTable.AsEnumerable().
                Where(r => r.Field<Int64>("WorkerID") == workerId && r.Field<DateTime>("Date").Date == date.Date).Any();
            return hasSchedule;
        }

        #endregion


        public string GetTimeSheetEditingDate()
        {
            var iConverter = new IdToNameConverter();
            if (_openingTimeDataTable.Rows.Count != 0)
                return
                    iConverter.Convert(_openingTimeDataTable.Rows[0]["EditingWorkerID"], "ShortName") + " "
                    + _openingTimeDataTable.Rows[0]["EditingDate"];

            return "--:--:--";
        }


        #endregion



        #region Weekends responsibles time sheet

        private void FillWeekendsResponsibles(int year, int month)
        {
            const string sqlCommandText = @"SELECT WeekendsResponsibleTableID, WorkerID, Date, EditingDate, EditingWorkerID 
                                            FROM FAIIProdRooms.WeekendsResponsibles 
                                            WHERE Year(Date) = @Year AND Month(Date) = @Month";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("@Year", MySqlDbType.Int32).Value = year;
            sqlCommand.Parameters.Add("@Month", MySqlDbType.Int32).Value = month;

            _weekendsResponsiblesAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_weekendsResponsiblesAdapter);
            _weekendsResponsiblesTable.Clear();

            try
            {
                _weekendsResponsiblesAdapter.Fill(_weekendsResponsiblesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0017]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataView GetWeekendsResponsiblesView()
        {
            _weekendResponsiblesTimeSheetTable = new DataTable();
            var weekendsData = _weekendsResponsiblesTable.AsEnumerable();
            var daysCount = DateTime.DaysInMonth(_selectedWeekendTimeSheetYear, _selectedWeekendTimeSheetMonth);

            // Add worker column
            _weekendResponsiblesTimeSheetTable.Columns.Add("WorkerID", typeof(int));

            // Add days of month columns
            for (var i = 1; i < daysCount + 1; i++)
            {
                _weekendResponsiblesTimeSheetTable.Columns.Add(i.ToString(CultureInfo.InvariantCulture), typeof(bool));
            }

            if (!weekendsData.Any())
            {
                //timeSheetTable.Rows.Add(string.Empty);
                _weekendResponsiblesTimeSheetTable.AcceptChanges();
                return _weekendResponsiblesTimeSheetTable.AsDataView();
            }

            var distinctWorkerIds = weekendsData.Select(r => r.Field<Int64>("WorkerID")).Distinct();

            foreach (var workerId in distinctWorkerIds)
            {
                var timeSheetRow = _weekendResponsiblesTimeSheetTable.NewRow();
                timeSheetRow["WorkerID"] = workerId;

                foreach (
                    var day in
                        Enumerable.Select(
                            weekendsData
                                .Where(oT => oT.Field<Int64>("WorkerID") == workerId),
                            row => Convert.ToDateTime(row["Date"]).Day))
                {
                    timeSheetRow[day] = true;
                }

                _weekendResponsiblesTimeSheetTable.Rows.Add(timeSheetRow);
            }

            _weekendResponsiblesTimeSheetTable.AcceptChanges();
            return _weekendResponsiblesTimeSheetTable.AsDataView();
        }

        public void AddWeekendResponsible(long workerId, DateTime date, long editingWorkerId, DateTime editingDate)
        {
            var newResponsibleRow = _weekendsResponsiblesTable.NewRow();
            newResponsibleRow["WorkerID"] = workerId;
            newResponsibleRow["Date"] = date;
            newResponsibleRow["EditingWorkerID"] = editingWorkerId;
            newResponsibleRow["EditingDate"] = editingDate;
            _weekendsResponsiblesTable.Rows.Add(newResponsibleRow);

            UpdateWeekendResponsibles();

            var weekendResponsibleId = GetWeekendResponsibleId(workerId, date);
            newResponsibleRow["WeekendsResponsibleTableID"] = weekendResponsibleId != null
                ? weekendResponsibleId.Value 
                : -1;
            newResponsibleRow.AcceptChanges();
        }

        private long? GetWeekendResponsibleId(long workerId, DateTime date)
        {
            const string sqlCommandText = @"SELECT WeekendsResponsibleTableID FROM FAIIProdRooms.WeekendsResponsibles 
                                            WHERE WorkerID = @WorkerID AND Date = @Date 
                                            ORDER BY WeekendsResponsibleTableID DESC";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = date;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        long weekendResponsibleId = -1;
                        Int64.TryParse(sqlResult.ToString(), out weekendResponsibleId);
                        return weekendResponsibleId;
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

            return null;
        }

        public void DeleteWeekendResponsible(long weekendResponsibleId)
        {
            var weekendResponsibles = _weekendsResponsiblesTable.Select(string.Format("WeekendsResponsibleTableID = {0}", weekendResponsibleId));
            if (!weekendResponsibles.Any()) return;

            var weekendResponsible = weekendResponsibles.First();
            weekendResponsible.Delete();

            UpdateWeekendResponsibles();
        }

        public void DeleteWorkerWeekendResponsibles(long workerId)
        {
            var weekendResponsibles = _weekendsResponsiblesTable.Select(string.Format("WorkerID = {0}", workerId));
            if (!weekendResponsibles.Any()) return;

            foreach (var weekendResponsible in weekendResponsibles)
            {
                weekendResponsible.Delete();
            }

            UpdateWeekendResponsibles();
        }

        public void UpdateWeekendResponsibles()
        {
            try
            {
                _weekendsResponsiblesAdapter.Update(_weekendsResponsiblesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0018]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void DeleteWorkerFromWeekendTimeSheet(long workerId)
        {
            var workers = _weekendResponsiblesTimeSheetTable.Select(string.Format("WorkerID = {0}", workerId));
            if (!workers.Any()) return;

            var worker = workers.First();
            worker.Delete();
        }

        public void AddWorkerToWeekendTimeSheet(long workerId)
        {
            var existWorker = _weekendResponsiblesTimeSheetTable.Select(string.Format("WorkerID = {0}", workerId));
            if(existWorker.Any())
            {
                MessageBox.Show("Данный работник уже добавлен в график", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newWorkerRow = _weekendResponsiblesTimeSheetTable.NewRow();
            newWorkerRow["WorkerID"] = workerId;
            _weekendResponsiblesTimeSheetTable.Rows.Add(newWorkerRow);
        }

        public void SaveWeekendTimeSheet(long editingWorkerId, DateTime editingDate)
        {
            // Delete all removed rows from table
            foreach (var deletedRow in _weekendResponsiblesTimeSheetTable.AsEnumerable().Where(r => r.RowState == DataRowState.Deleted))
            {
                var workerId = Convert.ToInt64(deletedRow["WorkerID", DataRowVersion.Original]);
                DeleteWorkerWeekendResponsibles(workerId);
            }

            // Add all new rows in table
            foreach (var addedRow in _weekendResponsiblesTimeSheetTable.AsEnumerable().Where(r => r.RowState == DataRowState.Added))
            {
                var workerId = Convert.ToInt64(addedRow["WorkerID"]);
                for(var i = 1; i < _weekendResponsiblesTimeSheetTable.Columns.Count; i++)
                {
                    if(addedRow[i] != DBNull.Value && Convert.ToBoolean(addedRow[i]))
                    {
                        var date = new DateTime(SelectedWeekendTimeSheetYear, SelectedWeekendTimeSheetMonth, i);
                        AddWeekendResponsible(workerId, date, editingWorkerId, editingDate);
                    }
                }

                addedRow.AcceptChanges();
            }

            // Save all changed rows in table
            foreach(var changedRow in _weekendResponsiblesTimeSheetTable.AsEnumerable().Where(r => r.RowState == DataRowState.Modified))
            {
                var workerId = Convert.ToInt64(changedRow["WorkerID"]);
                for (var i = 1; i < _weekendResponsiblesTimeSheetTable.Columns.Count; i++)
                {
                    var date = new DateTime(SelectedWeekendTimeSheetYear, SelectedWeekendTimeSheetMonth, i);

                    var dataBaseRows = _weekendsResponsiblesTable.AsEnumerable()
                        .Where(r => r.Field<DateTime>("Date").Date == date.Date && r.Field<Int64>("WorkerID") == workerId);
                    var existInDataBase = dataBaseRows.Any();
                    var existInTimeSheet = changedRow[i] != DBNull.Value && Convert.ToBoolean(changedRow[i]);

                    // if row was delete from table
                    if (existInDataBase && !existInTimeSheet)
                    {
                        var weekendResponsibleId = Convert.ToInt64(dataBaseRows.First()["WeekendsResponsibleTableID"]);
                        DeleteWeekendResponsible(weekendResponsibleId);
                    }

                    // if row was added to table
                    if(!existInDataBase && existInTimeSheet)
                    {
                        AddWeekendResponsible(workerId, date, editingWorkerId, editingDate);
                    }
                }

                changedRow.AcceptChanges();
            }

            _weekendResponsiblesTimeSheetTable.AcceptChanges();
        }

        public object GetWeekendEditingInfo(long workerId)
        {
            var workerRow = _weekendsResponsiblesTable.AsEnumerable().LastOrDefault(r => r.Field<Int64>("WorkerID") == workerId);
            return workerRow;
        }

        #endregion

        #region Weekend responsible arrives

        public void FillResponsibleArrives(int year, int month)
        {
            _selectedResponsibleArriveYear = year;
            _selectedResponsibleArriveMonth = month;

            const string sqlCommandText = @"SELECT ResponsibleArriveID, WorkerID, Date, AdditionalInfo 
                                            FROM FAIIProdRooms.ResponsibleArrives 
                                            WHERE Year(Date) = @Year AND Month(Date) = @Month";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("@Year", MySqlDbType.Int32).Value = year;
            sqlCommand.Parameters.Add("@Month", MySqlDbType.Int32).Value = month;

            _responsibleArrivesAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_responsibleArrivesAdapter);
            ResponsibleArrivesTable.Clear();

            try
            {
                _responsibleArrivesAdapter.Fill(ResponsibleArrivesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0019]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddResponsibleArrive(long workerId, DateTime date, string additionalInfo)
        {
            var newRow = ResponsibleArrivesTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["Date"] = date;
            if (!string.IsNullOrEmpty(additionalInfo))
                newRow["AdditionalInfo"] = additionalInfo;
            ResponsibleArrivesTable.Rows.Add(newRow);

            UpdateResponsibleArrives();

            var responsibleArriveId = GetResponsibleArriveId(workerId, date);
            newRow["ResponsibleArriveID"] = responsibleArriveId != null
                ? responsibleArriveId.Value
                : -1;
            newRow.AcceptChanges();
        }

        private long? GetResponsibleArriveId(long workerId, DateTime date)
        {
            const string sqlCommandText = @"SELECT ResponsibleArriveID FROM FAIIProdRooms.ResponsibleArrives 
                                            WHERE WorkerID = @WorkerID AND Date = @Date 
                                            ORDER BY ResponsibleArriveID DESC";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = date;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        long responsibleArriveId = -1;
                        Int64.TryParse(sqlResult.ToString(), out responsibleArriveId);
                        return responsibleArriveId;
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

            return null;
        }

        public bool HasWorkerNearlyArrive(long workerId, DateTime currentDate, TimeSpan limitedDuration)
        {
            var latestArrives = new List<DateTime>();

            const string sqlCommandText = @"SELECT Date 
                                            FROM FAIIProdRooms.ResponsibleArrives 
                                            WHERE WorkerID = @WorkerID AND Date(Date) = @Date";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = currentDate.Date;

                try
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var date = reader["Date"];
                            if (date != null && date != DBNull.Value)
                                latestArrives.Add(Convert.ToDateTime(reader["Date"]));
                        }
                    }
                }
                catch { }
                finally
                {
                    sqlConn.Close();
                }
            }

            var searchResult = latestArrives
                .Where(d => currentDate.Subtract(d) < limitedDuration);

            return searchResult.Any();
        }

        public long? GetWorkerNearlyArrive(long workerId, DateTime currentDate, TimeSpan limitedDuration)
        {
            var latestArrives = new List<DataRow>();

            const string sqlCommandText = @"SELECT ResponsibleArriveID, Date 
                                            FROM FAIIProdRooms.ResponsibleArrives 
                                            WHERE WorkerID = @WorkerID AND Date(Date) = @Date 
                                            ORDER BY Date DESC";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = currentDate.Date;

                try
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var date = reader["Date"];
                            if(date != null && date != DBNull.Value)
                            {
                                if(currentDate.Subtract(Convert.ToDateTime(date)) < limitedDuration)
                                {
                                    var responsibleArriveId = Convert.ToInt64(reader["ResponsibleArriveID"]);
                                    return responsibleArriveId;
                                }
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    sqlConn.Close();
                }
            }

            return null;
        }

        public void DeleteResponsibleArrive(long responsibleArriveId)
        {
            var responsibleArrives = ResponsibleArrivesTable.Select(string.Format("ResponsibleArriveID = {0}", responsibleArriveId));
            if (!responsibleArrives.Any()) return;

            var responsibleArrive = responsibleArrives.First();
            responsibleArrive.Delete();

            UpdateResponsibleArrives();
        }

        public void UpdateResponsibleArrives()
        {
            try
            {
                _responsibleArrivesAdapter.Update(ResponsibleArrivesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0020]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion



        #region Closing journal

        public void FillJournalProductionTable(int year, int month)
        {
            _journalYear = year;
            _journalMonth = month;

            try
            {
                _journalProductionAdapter =
                    new MySqlDataAdapter(
                        @"SELECT JournalID, Date, ClosingDate, LockID, SealNumber, WorkerID, WorkerNotes, 
                          GuardID, GuardNotes, LockStatusID, ParrentJournalID, Visible, IsClosed 
                          FROM FAIIProdRooms.JournalClOpProductionRooms WHERE (Month(Date) = '" +
                        _journalMonth + "' AND Year(Date) = '" + _journalYear + "' OR IsClosed = True) AND Visible = True",
                        _connectionString);
                new MySqlCommandBuilder(_journalProductionAdapter);
                JournalProductionsTable.Clear();
                _journalProductionAdapter.Fill(JournalProductionsTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0005]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int AddNewJournalRow(object date, object closingDate, int lockID, object sealNumber, object workerID,
            int lockStatusID, int parrentJournalID)
        {
            var newDr = JournalProductionsTable.NewRow();
            newDr["Date"] = date;
            newDr["ClosingDate"] = closingDate;
            newDr["LockID"] = lockID;
            newDr["SealNumber"] = sealNumber;
            newDr["WorkerID"] = workerID;
            newDr["LockStatusID"] = lockStatusID;
            newDr["ParrentJournalID"] = parrentJournalID;
            newDr["Visible"] = true;
            newDr["IsClosed"] = true;
            JournalProductionsTable.Rows.Add(newDr);

            UpdateJournalProductionsTable();

            var journalId = lockStatusID == 2
                ? GetJournalClosingId(Convert.ToDateTime(date), lockID, sealNumber.ToString(), (int) workerID)
                : GetJournalOpeningId(Convert.ToDateTime(closingDate), lockID);

            newDr["JournalID"] = journalId;
            newDr.AcceptChanges();

            return journalId;
        }

        public void AddInfoToOpeningRow(int journalId, object date, object sealNumber, object workerID)
        {
            var dr = JournalProductionsTable.Select("JournalID = " + journalId);
            if (dr.Length == 0) return;

            var addingDr = dr[0];
            addingDr["Date"] = date;
            addingDr["SealNumber"] = sealNumber;
            addingDr["WorkerID"] = workerID;
            addingDr["IsClosed"] = false;

            UpdateJournalProductionsTable();
        }

        public void OpenClosingRow(int journalId)
        {
            var rows = JournalProductionsTable.Select("JournalID = " + journalId);
            if (rows.Length == 0) return;

            var closingRow = rows.First();
            closingRow["IsClosed"] = false;

            UpdateJournalProductionsTable();
        }

        public void AddWorkerNote(DateTime date, string noteText)
        {
            var dr = JournalProductionsTable.Select("Date = '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            if (dr.Length == 0) return;

            var noteDR = dr[0];
            noteDR["WorkerNotes"] = noteText;

            UpdateJournalProductionsTable();
        }

        public void DeleteClosingDoor(long journalId)
        {
            var rows = JournalProductionsTable.Select(string.Format("JournalID = {0}", journalId));
            if (!rows.Any()) return;

            var deletingClosingRow = rows.First();
            var date = Convert.ToDateTime(deletingClosingRow["Date"]);
            deletingClosingRow["Visible"] = false;

            rows = JournalProductionsTable.Select("ClosingDate = '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            if (rows.Any())
            {
                var deletingOpeningRow = rows.First();
                deletingOpeningRow["Visible"] = false;
            }

            UpdateJournalProductionsTable();
        }

        public void CheckGuard(int journalID, int guardID)
        {
            var dr = JournalProductionsTable.Select("JournalID = " + journalID);
            if (dr.Length == 0) return;

            var checkDR = dr[0];
            checkDR["GuardID"] = guardID;

            UpdateJournalProductionsTable();
        }

        public void AddGuardNote(int journalID, string noteText)
        {
            var dr = JournalProductionsTable.Select("JournalID = " + journalID);
            if (dr.Length == 0) return;

            var noteDR = dr[0];
            noteDR["GuardNotes"] = noteText;

            UpdateJournalProductionsTable();
        }

        private void UpdateJournalProductionsTable()
        {
            try
            {
                _journalProductionAdapter.Update(JournalProductionsTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }


        public void FillConfirmDataTable(IEnumerable<long> journalIds)
        {
            var taskIdsString = string.Empty;
            var enumerable = journalIds as IList<long> ?? journalIds.ToList();
            taskIdsString = enumerable.Count() != 0
                ? (enumerable.Cast<object>()
                    .Aggregate(taskIdsString, (current, serviceHistoryId) => current + ", " + serviceHistoryId))
                    .Remove(0, 2)
                : "-1";

            try
            {
                _confirmAdapter =
                    new MySqlDataAdapter(
                        @"SELECT ConfirmClosingRoomID, JournalID, WorkerID, SealNumber, Date 
                          FROM FAIIProdRooms.ConfirmClosingRoom WHERE JournalID IN (" + taskIdsString + ")",
                        _connectionString);
                new MySqlCommandBuilder(_confirmAdapter);
                ConfirmDataTable.Clear();
                _confirmAdapter.Fill(ConfirmDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P0006]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddConfirmRow(int journalId, int workerId, string sealNumber, DateTime confirmDate)
        {
            var newDr = ConfirmDataTable.NewRow();
            newDr["JournalID"] = journalId;
            newDr["WorkerID"] = workerId;
            newDr["SealNumber"] = sealNumber;
            newDr["Date"] = confirmDate;
            ConfirmDataTable.Rows.Add(newDr);

            UpdateConfirm();

            var confirmClosingId = GetConfirmClosingId(journalId, workerId, confirmDate);
            newDr["ConfirmClosingRoomID"] = confirmClosingId;
            newDr.AcceptChanges();
        }

        private void UpdateConfirm()
        {
            try
            {
                _confirmAdapter.Update(ConfirmDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private static int GetJournalClosingId(DateTime date, int lockId, string sealNumber, int workerId)
        {
            var journalId = 1;
            const string sqlCommandText = @"SELECT JournalID FROM FAIIProdRooms.JournalClOpProductionRooms 
                                            WHERE Date = @Date AND LockID = @LockID AND SealNumber = @SealNumber AND WorkerID = @WorkerID";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = date;
                sqlCommand.Parameters.Add("@LockID", MySqlDbType.Int64).Value = lockId;
                sqlCommand.Parameters.Add("@SealNumber", MySqlDbType.VarChar).Value = sealNumber;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        Int32.TryParse(sqlResult.ToString(), out journalId);
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

            return journalId;
        }

        private static int GetJournalOpeningId(DateTime closingDate, int lockId)
        {
            var journalId = 1;
            const string sqlCommandText = @"SELECT JournalID FROM FAIIProdRooms.JournalClOpProductionRooms 
                                            WHERE ClosingDate = @ClosingDate AND LockID = @LockID AND LockStatusID = 1";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@ClosingDate", MySqlDbType.DateTime).Value = closingDate;
                sqlCommand.Parameters.Add("@LockID", MySqlDbType.Int64).Value = lockId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        Int32.TryParse(sqlResult.ToString(), out journalId);
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

            return journalId;
        }

        private static int GetConfirmClosingId(int journalId, int workerId, DateTime confirmDate)
        {
            var confirmClosingId = 1;
            const string sqlCommandText = @"SELECT ConfirmClosingRoomID FROM FAIIProdRooms.JournalClOpProductionRooms 
                                            WHERE JournalID = @JournalID AND WorkerID = @WorkerID AND Date = @Date";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@JournalID", MySqlDbType.Int64).Value = journalId;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = confirmDate;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        Int32.TryParse(sqlResult.ToString(), out confirmClosingId);
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

            return confirmClosingId;
        }

        public int JournalYear
        {
            get { return _journalYear; }
        }

        public int JournalMonth
        {
            get { return _journalMonth; }
        }

        #endregion


        #region Worker's reports

        private void FillWorkerReports()
        {
            const string sqlCommandText = @"SELECT WorkerLocksReportID, WorkerID, ActionID, ActionStatusID, ReportDate, AdditionalInfo, IsDoneAction 
                                            FROM FAIIProdRooms.WorkerLocksReports";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            _reportAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_reportAdapter);
            _reportTable.Clear();

            try
            {
                _reportAdapter.Fill(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P00014]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillWorkerReports(int year, int month)
        {
            _selectedWorkerReportYear = year;
            _selectedWorkerReportMonth = month;

            const string sqlCommandText = @"SELECT WorkerLocksReportID, WorkerID, ActionID, ActionStatusID, ReportDate, AdditionalInfo, IsDoneAction 
                                            FROM FAIIProdRooms.WorkerLocksReports 
                                            WHERE Year(ReportDate) = @Year AND Month(ReportDate) = @Month";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("@Year", MySqlDbType.Int32).Value = year;
            sqlCommand.Parameters.Add("@Month", MySqlDbType.Int32).Value = month;
            _reportAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_reportAdapter);
            _reportTable.Clear();

            try
            {
                _reportAdapter.Fill(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P00015]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddWorkerReport(long workerId, int actionId, int actionStatusId, DateTime reportDate, string additionalInfo, bool isDoneAction)
        {
            var newRow = ReportTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["ActionID"] = actionId;
            newRow["ActionStatusID"] = actionStatusId;
            newRow["ReportDate"] = reportDate;
            if (!string.IsNullOrEmpty(additionalInfo))
                newRow["AdditionalInfo"] = additionalInfo;
            newRow["IsDoneAction"] = isDoneAction;
            ReportTable.Rows.Add(newRow);

            UpdateWorkerReports();
        }

        private void UpdateWorkerReports()
        {
            try
            {
                _reportAdapter.Update(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [P00016]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool HasWorkerCurrentReport(long workerId, DateTime reportDate, int actionStatusId)
        {
            var hasWorkerCurrentReport = false;
            const string sqlCommandText = @"SELECT COUNT(*) 
                                            FROM FAIIProdRooms.WorkerLocksReports 
                                            WHERE WorkerID = @WorkerID AND Date(ReportDate) = @ReportDate AND ActionStatusID = @ActionStatusID";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ReportDate", MySqlDbType.DateTime).Value = reportDate.Date;
                sqlCommand.Parameters.Add("@ActionStatusID", MySqlDbType.Int64).Value = actionStatusId;

                try
                {
                    sqlConn.Open();
                    var result = sqlCommand.ExecuteScalar();
                    if (result != null && result != DBNull.Value && Convert.ToInt32(result) > 0)
                        hasWorkerCurrentReport = true;
                }
                catch { }
                finally
                {
                    sqlConn.Close();
                }
            }
                
            return hasWorkerCurrentReport;
        }

        public void DeleteWorkerCurrentReport(long workerId, DateTime reportDate, int actionStatusId)
        {
            const string sqlCommandText = @"DELETE FROM FAIIProdRooms.WorkerLocksReports 
                                            WHERE WorkerID = @WorkerID AND Date(ReportDate) = @ReportDate AND ActionStatusID = @ActionStatusID";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ReportDate", MySqlDbType.DateTime).Value = reportDate.Date;
                sqlCommand.Parameters.Add("@ActionStatusID", MySqlDbType.Int64).Value = actionStatusId;

                try
                {
                    sqlConn.Open();
                    var result = sqlCommand.ExecuteScalar();
                }
                catch { }
                finally
                {
                    sqlConn.Close();
                }
            }
        }

        #endregion



        public static IEnumerable<int> DistinctYears()
        {
            var distinctYears = new List<int>();
            const string sql = "SELECT DISTINCT Year(Date) FROM FAIIProdRooms.OpeningTimeTable";
            using (var conn = new MySqlConnection(_connectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    distinctYears.Add(Convert.ToInt32(reader[0]));
                }
                conn.Close();
            }

            distinctYears.Sort();

            var currentYear = DateTime.Now.Year;
            if (!distinctYears.Contains(currentYear))
                distinctYears.Add(currentYear);
            if (!distinctYears.Contains(currentYear + 1))
                distinctYears.Add(currentYear + 1);

            return distinctYears;
        }

        public static IEnumerable<int> DistinctWeekendResponsiblesYears()
        {
            var distinctYears = new List<int>();
            const string sql = "SELECT DISTINCT Year(Date) FROM FAIIProdRooms.WeekendsResponsibles";
            using (var conn = new MySqlConnection(_connectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    distinctYears.Add(Convert.ToInt32(reader[0]));
                }
                conn.Close();
            }

            distinctYears.Sort();

            var currentYear = DateTime.Now.Year;
            if (!distinctYears.Contains(currentYear))
                distinctYears.Add(currentYear);
            if (!distinctYears.Contains(currentYear + 1))
                distinctYears.Add(currentYear + 1);

            return distinctYears;
        }

        public static IEnumerable<int> DistinctJournalYears()
        {
            var distinctYears = new List<int>();
            const string sql =
                "SELECT DISTINCT Year(Date) FROM FAIIProdRooms.JournalClOpProductionRooms WHERE Date IS NOT NULL";
            using (var conn = new MySqlConnection(_connectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    distinctYears.Add(Convert.ToInt32(reader[0]));
                }
                conn.Close();
            }
            distinctYears.Sort();

            var currentYear = DateTime.Now.Year;
            if (!distinctYears.Contains(currentYear))
                distinctYears.Add(currentYear);

            return distinctYears;
        }

        private static void AddNewRow(int workerID, DateTime date, int editingWorkerID,
                                      DateTime editingDate, DataTable dataTable)
        {
            var newDR = dataTable.NewRow();
            newDR["WorkerID"] = workerID;
            newDR["Date"] = date;
            newDR["EditingWorkerID"] = editingWorkerID;
            newDR["EditingDate"] = editingDate;
            dataTable.Rows.Add(newDR);
        }

        public static DataTable GetClosedDoors()
        {
            var closedDoorsTable = new DataTable();

            const string sqlCommandText = @"SELECT journal.LockID, journal.Date, journal.WorkerID, journal.SealNumber, 
                                            confirm.Date AS ConfirmDate, confirm.WorkerID AS ConfirmWorkerID, 
                                            confirm.SealNumber AS ConfirmSealNumber 
                                            FROM (SELECT * FROM FAIIProdRooms.JournalClOpProductionRooms 
                                            WHERE IsClosed = TRUE AND Visible = TRUE AND LockStatusID = 2) journal 
                                            LEFT JOIN 
                                            (SELECT t2.ConfirmClosingRoomID,  t2.JournalID, t2.WorkerID, t2.SenderID, t2.SealNumber, t2.Date 
                                            FROM FAIIProdRooms.ConfirmClosingRoom AS t2
                                            JOIN (SELECT  JournalID, MAX( Date) as mDate FROM FAIIProdRooms.ConfirmClosingRoom GROUP BY JournalID) AS t1
                                            ON t1.JournalID = t2.JournalID AND t1.mDate = t2.Date) confirm 
                                            ON journal.JournalID = confirm.JournalID 
                                            ORDER BY journal.LockID, journal.Date DESC";
            var adapter = new MySqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString);

            try
            {
                adapter.Fill(closedDoorsTable);
            }
            catch (MySqlException)
            {
            }

            return closedDoorsTable;
        }

        public static DataTable GetOpendDoors()
        {
            var openedDoorsTable = new DataTable();

            const string sqlCommandText = @"SELECT t2.LockID, t2.`Date`, t2.WorkerID, t2.SealNumber, t2.IsClosed 
                                              FROM FAIIProdRooms.JournalClOpProductionRooms AS t2
                                              JOIN (SELECT  LockID, MAX( `Date`) as mDate
                                              FROM FAIIProdRooms.JournalClOpProductionRooms 
                                              WHERE Visible = TRUE AND LockStatusID = 1 
                                              GROUP BY LockID) AS t1 
                                              ON t1.LockID = t2.LockID AND t1.mDate = t2.`Date` ORDER BY LockID";
            var adapter = new MySqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString);

            try
            {
                adapter.Fill(openedDoorsTable);

                foreach (
                    var dataRow in
                        Enumerable.Where(openedDoorsTable.AsEnumerable(),
                            dataRow => Convert.ToBoolean(dataRow["IsClosed"])))
                {
                    dataRow.Delete();
                }
            }
            catch (MySqlException)
            {
            }

            return openedDoorsTable;
        }

        public static DataTable GetDoorsActualStatus()
        {
            var actualStatusTable = new DataTable();

            const string sqlCommandText = @"SELECT t2.LockID, t2.`Date`, t2.WorkerID, t2.SealNumber, t2.WorkerNotes, t2.IsClosed 
                                            FROM FAIIProdRooms.JournalClOpProductionRooms AS t2
                                            JOIN (SELECT LockID, MAX(`Date`) as mDate
                                                  FROM FAIIProdRooms.JournalClOpProductionRooms 
                                                  WHERE Visible = TRUE
                                                  GROUP BY LockID) AS t1 
                                            ON t1.LockID = t2.LockID AND t1.mDate = t2.`Date` 
                                            ORDER BY LockID";
            var adapter = new MySqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString);
            adapter.Fill(actualStatusTable);

            return actualStatusTable;
        }


        public DataView GetLocks()
        {
            var locksView = Locks.Table.AsDataView();
            locksView.RowFilter = "IsEnable = 'True'";
            return locksView;
        }


        public class LockClass
        {
            private DataTable _locksDataTable;
            private MySqlDataAdapter _locksAdapter;

            public DataTable Table
            {
                set { _locksDataTable = value; }
                get { return _locksDataTable; }
            }

            public LockClass()
            {
                Create();
                Fill();
            }

            private void Create()
            {
                _locksDataTable = new DataTable();
            }

            private void Fill()
            {
                try
                {
                    _locksAdapter =
                        new MySqlDataAdapter(
                            @"SELECT LockID, LockName, LockNotes, LockPhoto, EditingWorkerID, 
                              EditingDate, IsEnable FROM FAIIProdRooms.Locks ORDER BY LockName", _connectionString);
                    new MySqlCommandBuilder(_locksAdapter);
                    _locksAdapter.Fill(_locksDataTable);
                }
                catch (Exception exp)
                {
                    MetroMessageBox.Show(
                        exp.Message +
                        "\n\n [P0004]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void SaveChanges(int lockID, string lockName, string lockNote, object lockPhoto, int workerID,
                DateTime date)
            {
                var dataRows = _locksDataTable.Select("LockID NOT = " + lockID + " AND LockName = '" + lockName + "'");
                if (dataRows.Length != 0)
                {
                    MetroMessageBox.Show("Невозможно присвоить данное название. Такая дверь уже существует.",
                        string.Empty,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dr = _locksDataTable.Select("LockID = " + lockID);
                if (dr.Length == 0) return;

                var editRow = dr[0];

                if (editRow["LockName"].ToString() == lockName && editRow["LockNotes"].ToString() == lockNote &&
                    editRow["LockPhoto"] == lockPhoto) return;

                editRow["LockName"] = lockName;
                editRow["LockNotes"] = lockNote;
                editRow["LockPhoto"] = lockPhoto;
                editRow["EditingWorkerID"] = workerID;
                editRow["EditingDate"] = date;

                _locksAdapter.Update(_locksDataTable);
            }

            public bool AddNewLock(string lockName, int workerID, DateTime date)
            {

                var dr = _locksDataTable.Select("LockName = '" + lockName + "'");
                if (dr.Length != 0)
                {
                    MetroMessageBox.Show("Невозможно добавить дверь. Такая запись уже существует.", string.Empty,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }

                var newDR = _locksDataTable.NewRow();
                newDR["LockName"] = lockName;
                newDR["LockNotes"] = string.Empty;
                newDR["EditingWorkerID"] = workerID;
                newDR["EditingDate"] = date;
                _locksDataTable.Rows.Add(newDR);

                RefillLocksDataTable();
                return true;
            }

            public void DeleteLock(int lockID)
            {
                var rows = _locksDataTable.Select("LockID = " + lockID);
                if (rows.Length == 0) return;

                var deletingRow = rows.First();
                deletingRow["IsEnable"] = false;

                _locksAdapter.Update(_locksDataTable);
            }

            private void RefillLocksDataTable()
            {
                _locksAdapter.Update(_locksDataTable);
                _locksDataTable.Clear();
                _locksAdapter.Fill(_locksDataTable);
            }
        }


        public class ActionClass
        {
            private DataTable _actionsDataTable;
            private MySqlDataAdapter _actionsAdapter;

            private DataTable _actionStatusDataTable;
            private MySqlDataAdapter _actionStatusAdapter;

            public DataTable Table
            {
                set { _actionsDataTable = value; }
                get { return _actionsDataTable; }
            }

            public DataTable Status
            {
                set { _actionStatusDataTable = value; }
                get { return _actionStatusDataTable; }
            }

            public ActionClass()
            {
                Create();
                FillActions();
                FillActionsStatus();
            }

            private void Create()
            {
                _actionsDataTable = new DataTable();
                _actionStatusDataTable = new DataTable();
            }

            private void FillActions()
            {
                try
                {
                    _actionsAdapter =
                        new MySqlDataAdapter(
                            @"SELECT ActionID, ActionStatus, ActionNumber, ActionText, EditingWorkerID, 
                              EditingDate, Visible 
                              FROM FAIIProdRooms.Actions", _connectionString);
                    new MySqlCommandBuilder(_actionsAdapter);
                    _actionsAdapter.Fill(_actionsDataTable);
                }
                catch (Exception exp)
                {
                    MetroMessageBox.Show(
                        exp.Message +
                        "\n\n [P0005]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void FillActionsStatus()
            {
                try
                {
                    _actionStatusAdapter =
                        new MySqlDataAdapter(
                            "SELECT ActionStatusID, ActionStatusName FROM FAIIProdRooms.ActionStatus",
                            _connectionString);
                    new MySqlCommandBuilder(_actionStatusAdapter);
                    _actionStatusAdapter.Fill(_actionStatusDataTable);
                }
                catch (Exception exp)
                {
                    MetroMessageBox.Show(
                        exp.Message +
                        "\n\n [P0006]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public void SaveChanges(int actionID, int actionStatus, int actionNumber, string actionText, int workerID,
                DateTime date)
            {
                var dataRows =
                    _actionsDataTable.Select("ActionID NOT = " + actionID + " AND ActionStatus = " + actionStatus +
                                             " AND ActionNumber = " + actionNumber);
                if (dataRows.Length != 0)
                {
                    MetroMessageBox.Show("Невозможно присвоить данный номер. Такой пункт уже существует.", string.Empty,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dr = _actionsDataTable.Select("ActionID = " + actionID);
                if (dr.Length == 0) return;

                var editRow = dr[0];

                if (Convert.ToInt32(editRow["ActionStatus"]) == actionStatus &&
                    Convert.ToInt32(editRow["ActionNumber"]) == actionNumber &&
                    editRow["ActionText"].ToString() == actionText) return;

                editRow["ActionStatus"] = actionStatus;
                editRow["ActionNumber"] = actionNumber;
                editRow["ActionText"] = actionText;
                editRow["EditingWorkerID"] = workerID;
                editRow["EditingDate"] = date;

                _actionsAdapter.Update(_actionsDataTable);
            }

            public bool AddNewAction(int actionStatus, int actionNumber, string actionText, int workerID, DateTime date)
            {
                var dr =
                    _actionsDataTable.Select("ActionStatus = " + actionStatus + " AND ActionNumber = " + actionNumber);
                if (dr.Length != 0)
                {
                    MetroMessageBox.Show("Невозможно добавить действие. Данный пункт уже существует.", string.Empty,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }

                var newDR = _actionsDataTable.NewRow();
                newDR["ActionStatus"] = actionStatus;
                newDR["ActionNumber"] = actionNumber;
                newDR["ActionText"] = actionText;
                newDR["EditingWorkerID"] = workerID;
                newDR["EditingDate"] = date;
                newDR["Visible"] = true;
                _actionsDataTable.Rows.Add(newDR);

                RefillActionsDataTable();
                return true;
            }

            public void DeleteAction(int actionID)
            {
                var dr = _actionsDataTable.Select("ActionID = " + actionID);
                if (dr.Length == 0) return;

                var deletingRow = dr[0];
                deletingRow["Visible"] = false;

                _actionsAdapter.Update(_actionsDataTable);
            }

            private void RefillActionsDataTable()
            {
                _actionsAdapter.Update(_actionsDataTable);
                _actionsDataTable.Clear();
                _actionsAdapter.Fill(_actionsDataTable);
            }

            public DataView GetClosingActions()
            {
                var closingActionDV = new DataView(_actionsDataTable)
                                      {
                                          Sort = "ActionNumber",
                                          RowFilter = "ActionStatus = 1"
                                      };
                return closingActionDV;
            }

            public DataView GetOpeningActions()
            {
                var openingActionDV = new DataView(_actionsDataTable)
                                      {
                                          Sort = "ActionNumber",
                                          RowFilter = "ActionStatus = 2"
                                      };
                return openingActionDV;
            }
        }
    }
}
