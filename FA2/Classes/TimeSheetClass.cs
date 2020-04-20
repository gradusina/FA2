using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using MessageBox = FAIIControlLibrary.MetroMessageBox;

namespace FA2.Classes
{
    public class TimeSheetClass
    {
        #region events

        public event ProgressChangedEventHandler ProgressChanged;

        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        protected virtual void OnProgressChanged(int progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progress, null));
            }
        }

        protected virtual void OnRunWorkerCompleted()
        {
            if (RunWorkerCompleted != null)
            {
                RunWorkerCompleted(this, null);
            }
        }

        #endregion

        public DataTable TimesheetDataTable;
        private MySqlConnection _timesheetConnection;
        private MySqlDataAdapter _timesheetDataAdapter;

        public DataTable _timesheetStatETDataTable;
        private MySqlConnection _timesheetStatETConnection;
        private MySqlDataAdapter _timesheetStatETDataAdapter;
        
        private static DataTable _timesheetWorkerIdsDataTable;
        private MySqlConnection _timesheetWorkerIdsConnection;
        private MySqlDataAdapter _timesheetWorkerIdsDataAdapter;

        private DataTable _timeSpentAtWorkDataTable;
        private MySqlConnection _timeSpentAtWorkConnection;
        private MySqlDataAdapter _timeSpentAtWorkDataAdapter;

        private DataTable _timeTrackingDataTable;
        private MySqlConnection _timeTrackingConnection;
        private MySqlDataAdapter _timeTrackingDataAdapter;

        public DataTable AbsencesTypesDataTable;
        private MySqlConnection _absencesTypesConnection;
        private MySqlDataAdapter _absencesTypesDataAdapter;
        public BindingListCollectionView AbsencesTypesViewSource;

        private DataTable _insalubrityMachinesOperationsDataTable;
        private MySqlConnection _insalubrityMachinesOperationsConnection;
        private MySqlDataAdapter _insalubrityMachinesOperationsDataAdapter;

        private DataTable _prodCalendarDataTable;
        private MySqlConnection _prodCalendarConnection;
        private MySqlDataAdapter _prodCalendarDataAdapter;
        
        private readonly string _connectionString;

        private Array _workersIdsArray;

        private string _workersIdsString;

        private int _daysCount;

        private int _year;
        private int _month;

        public DataTable HolidaysDataTable = null;

        private PlannedScheduleClass PSC;

        private DateTime _currentDate;

        public TimeSheetClass(string connectionString)
        {
            _connectionString = connectionString;
            _currentDate = App.BaseClass.GetDateFromSqlServer();


            Initialize();
        }

        private void Initialize()
        {
            Create();
            Fill();
            Binding();

            App.BaseClass.GetPlannedScheduleClass(ref PSC);
        }

        private void Fill()
        {
            FillAbsencesTypes();
            FillInsalubrityMachinesOperations();
        }

        private void Create()
        {
            TimesheetDataTable = new DataTable();
            _timesheetWorkerIdsDataTable = new DataTable();

            _timeSpentAtWorkDataTable = new DataTable();
            _timeTrackingDataTable = new DataTable();

            AbsencesTypesDataTable = new DataTable();

            _insalubrityMachinesOperationsDataTable = new DataTable();

            _prodCalendarDataTable = new DataTable();

            _timesheetStatETDataTable = new DataTable();
            //TimesheetStatDataTable = new DataTable();
        }

        private void Binding()
        {
            AbsencesTypesViewSource = new BindingListCollectionView(AbsencesTypesDataTable.DefaultView);
        }

        private void FillAbsencesTypes()
        {
            try
            {
                if (AbsencesTypesDataTable != null)
                {
                    AbsencesTypesDataTable.DefaultView.RowFilter = "";
                    AbsencesTypesDataTable.Clear();
                }

                const string commandText = @"SELECT AbsencesTypeID, AbsencesName, AbsencesSymbol, AbsencesDescription, 
                                             SetInWeekend, SetInHoliday, ConsiderInResult, ConsiderNorm, Color, Locked 
                                             FROM FAIIVacations.AbsencesTypes";

                _absencesTypesConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _absencesTypesConnection);

                _absencesTypesDataAdapter = new MySqlDataAdapter(command);
// ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_absencesTypesDataAdapter);
// ReSharper restore ObjectCreationAsStatement

                _absencesTypesDataAdapter.Fill(AbsencesTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0003] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillInsalubrityMachinesOperations()
        {
            try
            {
                if (_insalubrityMachinesOperationsDataTable != null)
                {
                    _insalubrityMachinesOperationsDataTable.DefaultView.RowFilter = "";
                    _insalubrityMachinesOperationsDataTable.Clear();
                }

                const string commandText = @"SELECT WorkOperationID, Insalubrity, InsalubrityRate 
                                             FROM FAIICatalog.MachinesOperations WHERE Insalubrity = True";

                _insalubrityMachinesOperationsConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _insalubrityMachinesOperationsConnection);

                _insalubrityMachinesOperationsDataAdapter = new MySqlDataAdapter(command);

                _insalubrityMachinesOperationsDataAdapter.Fill(_insalubrityMachinesOperationsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTimesheet(int year, int month, string workersIds)
        {
            try
            {
                if (TimesheetDataTable != null)
                {
                    TimesheetDataTable.DefaultView.RowFilter = "";
                    TimesheetDataTable.Clear();
                }

                string commandText =
                    "SELECT * FROM FAIITimeTracking.Timesheet WHERE YEAR(TimesheetDate) = '" + year +
                    "' AND MONTH(TimesheetDate) = '" + month + "' AND WorkerID IN (" + workersIds + ")";//AND WorkerID = '205' ";//

                _timesheetConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _timesheetConnection);

                _timesheetDataAdapter = new MySqlDataAdapter(command);
// ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_timesheetDataAdapter);
// ReSharper restore ObjectCreationAsStatement

                _timesheetDataAdapter.Fill(TimesheetDataTable);


                if (TimesheetDataTable != null)
                    if (TimesheetDataTable.Columns.IndexOf("Deviation") == -1)
                        TimesheetDataTable.Columns.Add("Deviation", typeof (Boolean));


                if (_timesheetWorkerIdsDataTable != null)
                {
                    _timesheetWorkerIdsDataTable.DefaultView.RowFilter = "";
                    _timesheetWorkerIdsDataTable.Clear();
                }

                string commandTextIDs =
                    "SELECT WorkerID FROM FAIITimeTracking.Timesheet " +
                    "WHERE YEAR(TimesheetDate) = '" + year +
                    "' AND MONTH(TimesheetDate) = '" + month + "' AND WorkerID IN (" + workersIds + ")";

                _timesheetWorkerIdsConnection = new MySqlConnection(_connectionString);

                var commandIDs = new MySqlCommand(commandTextIDs, _timesheetWorkerIdsConnection);

                _timesheetWorkerIdsDataAdapter = new MySqlDataAdapter(commandIDs);

                _timesheetWorkerIdsDataAdapter.Fill(_timesheetWorkerIdsDataTable);

                if (_timesheetStatETDataTable != null)
                {
                    _timesheetStatETDataTable.DefaultView.RowFilter = "";
                    _timesheetStatETDataTable.Clear();
                }

                string commandText_ET =
                    "SELECT TimesheetDate, WorkerID, ExceedingTime, ExceedingAllTime " +
                    "FROM FAIITimeTracking.Timesheet " +
                    "WHERE YEAR(TimesheetDate) = '" + year +
                    "' AND WorkerID IN (" + workersIds + ")";

                _timesheetStatETConnection = new MySqlConnection(_connectionString);

                var command_ET = new MySqlCommand(commandText_ET, _timesheetStatETConnection);

                _timesheetStatETDataAdapter = new MySqlDataAdapter(command_ET);

                _timesheetStatETDataAdapter.Fill(_timesheetStatETDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                    "\n\n[TSC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillProdCalendar(int year)
        {
            try
            {
                if (_prodCalendarDataTable != null)
                {
                    _prodCalendarDataTable.DefaultView.RowFilter = "";
                    _prodCalendarDataTable.Clear();
                }

                string commandText =
                    "SELECT ProductionCalendarID, Date, Standart40Time FROM FAIIProdCalendar.ProductionCalendar WHERE YEAR(Date) = '" +
                    year + "'";

                _prodCalendarConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _prodCalendarConnection);

                _prodCalendarDataAdapter = new MySqlDataAdapter(command);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_prodCalendarDataAdapter);
                // ReSharper restore ObjectCreationAsStatement

                _prodCalendarDataAdapter.Fill(_prodCalendarDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TSC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static Array GetWorkersIds(DataView workersDataView)
        {
            var filePaths =
                from workersView in workersDataView.ToTable().AsEnumerable()
                select workersView.Field<Int64>("WorkerID");

            Array workersIdsArray = filePaths.ToArray();

            return workersIdsArray.Length == 0 ? null : workersIdsArray;
        }

        public void FillTablesForTimesheet(int year, int month, DataView workersDataView, int standartTime, int factoryID)
        {
            //workersDataView.RowFilter = "AvailableInList = 'True'";

            _daysCount = DateTime.DaysInMonth(year, month);

            _year = year;
            _month = month;

            //_standartTime = standartTime;

            _workersIdsArray = GetWorkersIds(workersDataView);

            if (_workersIdsArray == null) return;

            _workersIdsString = string.Empty;

            _workersIdsString =
                (_workersIdsArray.Cast<object>()
                    .Aggregate(_workersIdsString, (current, workerID) => current + ", " + workerID))
                    .Remove(0, 2);

            FillTimesheet(year, month, _workersIdsString);
            //FillTimesheetStat(year, month, _workersIdsString);
            FillProdCalendar(year);

            //_missingWorkersIdsArray = GetMissingWorkersIds(workersDataView);

            //ShowTimesheetForCurrentWorkers(year, month, factoryID);
        }

        private void FillTimeSpentAtWork(int year, int month, string workersIds)
        {
            try
            {
                if (_timeSpentAtWorkDataTable != null)
                {
                    _timeSpentAtWorkDataTable.DefaultView.RowFilter = "";
                    _timeSpentAtWorkDataTable.Clear();
                }

                string commandText = "SELECT TimeSpentAtWorkID, WorkerID, Date, ShiftNumber, " +
                                     "VCLP, DayEnd FROM FAIITimeTracking.TimeSpentAtWork " +
                                     "WHERE DayEnd = TRUE AND YEAR(WorkDayTimeStart) = '" + year + "' " +
                                     "AND MONTH(WorkDayTimeStart) = '" + month + "' " +
                                     "AND WorkerID IN (" + workersIds + ")";

                _timeSpentAtWorkConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _timeSpentAtWorkConnection);

                #pragma warning disable 618
                command.Parameters.Add("@WorkersIds", typeof(string)).Value = workersIds;
                #pragma warning restore 618

                _timeSpentAtWorkDataAdapter = new MySqlDataAdapter(command);

                _timeSpentAtWorkDataAdapter.Fill(_timeSpentAtWorkDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTimeTracking(int year, int month, string workersIds)
        {
            try
            {
                _timeTrackingDataTable.DefaultView.RowFilter = "";
                _timeTrackingDataTable.Clear();

                string commandText = "SELECT WorkersTimeTrackingID, TimeSpentAtWorkID, WorkDayTimeStart, " +
                                     "WorkerID, TimeStart, TimeEnd, ThirdStageVerification, " +
                                     "DeleteRecord, WorkStatusID, WorkOperationID " +
                                     "FROM FAIITimeTracking.WorkersTimeTracking " +
                                     "WHERE YEAR(WorkDayTimeStart) = @Year AND MONTH(WorkDayTimeStart) = @Month AND WorkerID IN (" +
                                     workersIds + ") AND DeleteRecord <> True AND ThirdStageVerification = True";

                _timeTrackingConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, _timeTrackingConnection);

                command.Parameters.Add("@Year", MySqlDbType.Int64).Value = year;
                command.Parameters.Add("@Month", MySqlDbType.Int64).Value = month;

                _timeTrackingDataAdapter = new MySqlDataAdapter(command);

                _timeTrackingDataAdapter.Fill(_timeTrackingDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [TSC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CalculateTimeSheet(int year, int month, int factoryID)
        {
            FillTimeSpentAtWork(year, month, _workersIdsString);
            FillTimeTracking(year, month, _workersIdsString);

            decimal onePercent = _workersIdsArray.Length != 0 ? (decimal)100 / _workersIdsArray.Length : 100;
            decimal currentPercent = 0;

           //_workersIdsArray = new[]{415};
            
            foreach (var workerID in _workersIdsArray)
            {
                DataRow tsDataRow = TimesheetDataTable.NewRow();

                tsDataRow["WorkerID"] = workerID;
                tsDataRow["FactoryID"] = factoryID;
                tsDataRow["TimesheetDate"] = new DateTime(year, month, 1);

                for (var i = 1; i < (_daysCount + 1); i++)
                {
                    var tempTime = new TimeSpan(0);
                    var tempNightTime = new TimeSpan(0);
                    var tempInsalubrityTime = new TimeSpan(0);

                    var fromDate = new DateTime(year, month, i);
                    var toDate = fromDate.AddDays(1);

                    _timeSpentAtWorkDataTable.DefaultView.RowFilter = "Date >= #" + fromDate.ToString("yyyy-M-d") +
                                                                      "# AND Date < #" + toDate.ToString("yyyy-M-d") +
                                                                      "# AND WorkerID = " + workerID;

                    if (_timeSpentAtWorkDataTable.DefaultView.Count != 0)
                    {
                        foreach (DataRowView shiftRowView in _timeSpentAtWorkDataTable.DefaultView)
                        {
                            _timeTrackingDataTable.DefaultView.RowFilter = "TimeSpentAtWorkID = " +
                                                                           shiftRowView["TimeSpentAtWorkID"];

                            foreach (
                                TimeSpan[] durations in
                                    from DataRowView timeTrackingRowView in _timeTrackingDataTable.DefaultView
                                    let startTimeStart = (TimeSpan) timeTrackingRowView["TimeStart"]
                                    let endTime = (TimeSpan) timeTrackingRowView["TimeEnd"]
                                    let workOperationID = (long) timeTrackingRowView["WorkOperationID"]
                                    select CalculateTime(startTimeStart, endTime, workOperationID))
                            {
                                tempTime = tempTime.Add(durations[0]);
                                tempNightTime = tempNightTime.Add(durations[1]);

                                tempInsalubrityTime = tempInsalubrityTime.Add(durations[2]);
                            }

                            tsDataRow["s" + i.ToString(CultureInfo.InvariantCulture)] = shiftRowView["ShiftNumber"];
                        }
                    }

                    var hours = Convert.ToDecimal(tempTime.TotalHours);
                    var nightHours = Convert.ToDecimal(tempNightTime.TotalHours);
                    var insalubrityHours = Convert.ToDecimal(tempInsalubrityTime.TotalHours);

                    if (_timeSpentAtWorkDataTable.DefaultView.Count != 0)
                    {
                        tsDataRow["d" + i.ToString(CultureInfo.InvariantCulture)] = Decimal.Round(hours, 2);
                        tsDataRow["n" + i.ToString(CultureInfo.InvariantCulture)] = Decimal.Round(nightHours, 2);

                        if (hours == 0)
                        {
                            tsDataRow["t" + i.ToString(CultureInfo.InvariantCulture)] = 1;
                        }
                        else
                        {
                            if (hours/2 > nightHours)
                                tsDataRow["t" + i.ToString(CultureInfo.InvariantCulture)] = 1;
                            else
                                tsDataRow["t" + i.ToString(CultureInfo.InvariantCulture)] = 6;
                        }

                        tsDataRow["i" + i.ToString(CultureInfo.InvariantCulture)] = Decimal.Round(insalubrityHours, 2);
                    }
                    else
                    {
                        tsDataRow["s" + i.ToString(CultureInfo.InvariantCulture)] = 0;
                        tsDataRow["d" + i.ToString(CultureInfo.InvariantCulture)] = "-";
                        tsDataRow["t" + i.ToString(CultureInfo.InvariantCulture)] = 3;
                        tsDataRow["n" + i.ToString(CultureInfo.InvariantCulture)] = 0;
                        tsDataRow["i" + i.ToString(CultureInfo.InvariantCulture)] = 0;
                    }

                    if (IsHoliday(fromDate))
                        tsDataRow["t" + i.ToString(CultureInfo.InvariantCulture)] = 2;
                }

                tsDataRow["Deviation"] = IsDeviation(tsDataRow);

                AddRefillTimesheetDataRow(tsDataRow, Convert.ToInt32(workerID));

                currentPercent = currentPercent + onePercent;
                OnProgressChanged(Convert.ToInt32(currentPercent));
                DispatcherHelper.DoEvents();
            }

            UpdateTimesheetStat();

            OnRunWorkerCompleted();
        }

        private void AddRefillTimesheetDataRow(DataRow newTimesheetDataRow, int workerID)
        {
            DataRow[] tsDataRows = TimesheetDataTable.Select("WorkerID =" + workerID);

            if (tsDataRows.Length != 0)
            {
                for (int i = 1; i < _daysCount + 1; i++)
                {
                    //tsDataRows[0]["s" + i] = newTimesheetDataRow["s" + i];
                    //tsDataRows[0]["d" + i] = newTimesheetDataRow["d" + i];

                    //if (tsDataRows[0]["t" + i].ToString() == "1")
                    //    tsDataRows[0]["t" + i] = newTimesheetDataRow["t" + i];

                    if (newTimesheetDataRow["i" + i].ToString() == "0")
                    tsDataRows[0]["i" + i] = newTimesheetDataRow["i" + i];
                    //tsDataRows[0]["n" + i] = newTimesheetDataRow["n" + i];

                    tsDataRows[0]["Deviation"] = newTimesheetDataRow["Deviation"];
                }
            }
            else
            {
                TimesheetDataTable.Rows.Add(newTimesheetDataRow);
            }
        }

        private TimeSpan[] CalculateTime(TimeSpan startTime, TimeSpan endTime, long workOperationID)
        {
            var durations = new TimeSpan[3];
            var startNight = new TimeSpan(22, 0, 0);
            var endNight = new TimeSpan(6, 0, 0);

            // Нахождение длительности
            if (endTime >= startTime)
                durations[0] = endTime.Subtract(startTime);
            else
            {
                durations[0] = new TimeSpan(24, 0, 0).Subtract(startTime.Subtract(endTime));
            }

            // Нахождение длительности ночных
            if (startTime == endTime)
            {
                durations[1] = TimeSpan.Zero;
            }

            else if (startTime < endTime)
            {
                // Start и End не входят в ночные часы
                if (Interval(startTime, endNight, true, startNight, false) && Interval(endTime, endNight, false, startNight, true))
                {
                    durations[1] = TimeSpan.Zero;
                }
                // Start и End находятся от 00:00 до 06:00 включительно или Start и End находится от 22:00 до 24:00
                else if (Interval(startTime, TimeSpan.Zero, true, endNight, false) && Interval(endTime, TimeSpan.Zero, false, endNight, true) || Interval(startTime, startNight, true, new TimeSpan(24, 0, 0), false) && Interval(endTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = endTime.Subtract(startTime);
                }
                // Start находится от 00:00 до 06:00, End от 06:00 до 22:00
                else if (Interval(startTime, TimeSpan.Zero, true, endNight, false) && Interval(endTime, endNight, false, startNight, true))
                {
                    durations[1] = endNight.Subtract(startTime);
                }
                // Start находится от 00:00 до 06:00, End находится от 22:00 до 24:00
                else if (Interval(startTime, TimeSpan.Zero, true, endNight, false) && Interval(endTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = endNight.Subtract(startTime).Add(endTime.Subtract(startNight));
                }
                // Start находится от 06:00 до 22:00, End находится от 22:00 до 24:00
                else if (Interval(startTime, endNight, true, startNight, false) && Interval(endTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = endTime.Subtract(startNight);
                }
            }

            else
            {
                // End и Start находятся от 00:00 до 06:00 или End и Start находится от 22:00 до 24:00
                if (Interval(endTime, TimeSpan.Zero, true, endNight, false) && Interval(startTime, TimeSpan.Zero, false, endNight, true) || Interval(endTime, startNight, true, new TimeSpan(24, 0, 0), false) && Interval(startTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = new TimeSpan(8, 0, 0).Subtract(startTime.Subtract(endTime));
                }
                // End находится от 00:00 до 06:00, Start находится от 06:00 до 22:00
                else if (Interval(endTime, TimeSpan.Zero, true, endNight, false) && Interval(startTime, endNight, false, startNight, true))
                {
                    durations[1] = endTime.Add(new TimeSpan(2, 0, 0));
                }
                // Start и End не входят в ночные часы
                else if (Interval(endTime, endNight, true, startNight, false) && Interval(startTime, endNight, false, startNight, true))
                {
                    durations[1] = new TimeSpan(8, 0, 0);
                }
                // End находится от 06:00 до 22:00, Start находится от 22:00 до 24:00
                else if (Interval(endTime, endNight, true, startNight, false) && Interval(startTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = new TimeSpan(24, 0, 0).Subtract(startTime).Add(new TimeSpan(6, 0, 0));
                }
                // End находится от 00:00 до 06:00, Start находится от 22:00 до 24:00
                else if (Interval(endTime, TimeSpan.Zero, true, endNight, false) && Interval(startTime, startNight, false, new TimeSpan(24, 0, 0), true))
                {
                    durations[1] = new TimeSpan(24, 0, 0).Subtract(startTime).Add(endTime);
                }
            }


            var insalubrityTime = durations[0];

            IsInsalubrity(ref insalubrityTime, workOperationID);

            durations[2] = insalubrityTime;

            return durations;
        }

        private void IsInsalubrity(ref TimeSpan timeInterval, long workOperationID)
        {
            DataRow[] dataRows = _insalubrityMachinesOperationsDataTable.Select("WorkOperationID =" + workOperationID);

            if (dataRows.Length == 0)
            {
              timeInterval = new TimeSpan(0);
              return;
            }

            if (Convert.ToDecimal(dataRows[0]["InsalubrityRate"]) != 1.2m)
            {
                timeInterval = timeInterval.Add(new TimeSpan(4, 4, 0, 0));
            }
        }

        private bool Interval(TimeSpan value, TimeSpan from, bool includeFrom, TimeSpan to, bool includeTo)
        {
            if (includeFrom && includeTo)
            {
                if (value >= from && value <= to)
                    return true;
            }
            else if (includeFrom)
            {
                if (value >= from && value < to)
                    return true;
            }
            else if (includeTo)
            {
                if (value > from && value <= to)
                    return true;
            }
            else
            {
                if (value > from && value < to)
                    return true;
            }
            return false;
        }

        private bool IsHoliday(DateTime day, bool considerWeekend = true, bool onlyWeekEnd = false)
        {
            if (onlyWeekEnd) return day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;

            var custView = new DataView(HolidaysDataTable, "", "Date", DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(day);

            if (foundRows.Count() != 0)
            {
                return true;
            }

            if (!considerWeekend)
                return false;

            return day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
        }

        public void SaveTimesheet()
        {
            _timesheetDataAdapter.Update(TimesheetDataTable);
        }

        public bool AddAbsences(string absencesName, string absencesSymbol, string absencesDescription, bool setInWeekend,
                                bool setInHoliday, bool considerInResult, object color, bool considerNorm)
        {
            var dataRows = AbsencesTypesDataTable.Select("AbsencesSymbol = '" + absencesSymbol + "'");
            if (dataRows.Length > 0) return false;

            var dataRow = AbsencesTypesDataTable.NewRow();
            dataRow["AbsencesName"] = absencesName;
            dataRow["AbsencesSymbol"] = absencesSymbol;
            dataRow["AbsencesDescription"] = absencesDescription;
            dataRow["SetInWeekend"] = setInWeekend;
            dataRow["SetInHoliday"] = setInHoliday;
            dataRow["ConsiderInResult"] = considerInResult;
            dataRow["ConsiderNorm"] = considerNorm;
            dataRow["Color"] = color;
            dataRow["Locked"] = false;
            AbsencesTypesDataTable.Rows.Add(dataRow);

            RefillAbsences();
            return true;
        }

        public bool SaveAbsences(int absencesTypeId, string absencesName, string absencesSymbol, string absencesDescription,
                                 bool setInWeekend, bool setInHoliday, bool considerInResult, object color, bool considerNorm)
        {
            var dataRows =
                AbsencesTypesDataTable.Select("AbsencesSymbol = '" + absencesSymbol +
                                               "' AND AbsencesTypeID NOT = " + absencesTypeId);
            if (dataRows.Length > 0) return false;

            var dataRow = AbsencesTypesDataTable.Select("AbsencesTypeID = " + absencesTypeId)[0];
            dataRow["AbsencesName"] = absencesName;
            dataRow["AbsencesSymbol"] = absencesSymbol;
            dataRow["AbsencesDescription"] = absencesDescription;
            dataRow["SetInWeekend"] = setInWeekend;
            dataRow["SetInHoliday"] = setInHoliday;
            dataRow["ConsiderInResult"] = considerInResult;
            dataRow["ConsiderNorm"] = considerNorm;
            dataRow["Color"] = color;
            try
            {
                _absencesTypesDataAdapter.Update(AbsencesTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }

        public void DeleteAbsences(int absencesTypeId)
        {
            var deletingRow = AbsencesTypesDataTable.Select("AbsencesTypeID = " + absencesTypeId)[0];
            deletingRow.Delete();
            try
            {
                _absencesTypesDataAdapter.Update(AbsencesTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataRow LastItemFromAbsences()
        {
            return AbsencesTypesDataTable.Select().Last();
        }

        private void RefillAbsences()
        {
            try
            {
                _absencesTypesDataAdapter.Update(AbsencesTypesDataTable);
                AbsencesTypesDataTable.Clear();
                _absencesTypesDataAdapter.Fill(AbsencesTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TSC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region TimeSheetStat

        public void UpdateTimesheetStat()
        {
            //TimesheetStatDataTable.Clear();

            foreach (DataRowView tRowView in TimesheetDataTable.DefaultView)
            {
                DataRow timesheetStatRow = TimesheetDataTable.NewRow();

                int workerID = Convert.ToInt32(tRowView["WorkerID"]);

                timesheetStatRow["TimesheetDate"] =tRowView["TimesheetDate"];
                timesheetStatRow["WorkerID"] = workerID;

                timesheetStatRow["WorkingDaysCount"] = CalculateWorkingDaysCount(tRowView);

                int nitghShiftsCount = 0;

                decimal timesheetSumm = CalculateTimesheetSumm(tRowView,ref nitghShiftsCount);
                timesheetStatRow["TimesheetSumm"] = timesheetSumm;

                timesheetStatRow["InsalubrityTime"] = CalculateInsalubrityTime(tRowView);
                timesheetStatRow["NightTime"] = CalculateNightTime(tRowView);

                decimal timeOnBasisNorms = CalculateTimeOnBasisNorms(tRowView);
                timesheetStatRow["TimeOnBasisNorms"] = timeOnBasisNorms;

                decimal sickTime = CalculateSickTime(tRowView);
                timesheetStatRow["SickTime"] = sickTime;

                timesheetStatRow["OwnExpenseTime"] = CalculateOwnExpense(tRowView);

                double exceedingTime = CalculateExceedingTime(_year, _month, timesheetSumm, timeOnBasisNorms, sickTime,
                    nitghShiftsCount);
                timesheetStatRow["ExceedingTime"] = exceedingTime;

                timesheetStatRow["ExceedingAllTime"] = CalculateExceedingAllTime(workerID, _month, exceedingTime);

                AddRefillTimesheetStatDataRow(timesheetStatRow, workerID);
            }
        }

        private void AddRefillTimesheetStatDataRow(DataRow newTimesheetStatDataRow, int workerID)
        {
            DataRow[] tsSratDataRows = TimesheetDataTable.Select("WorkerID =" + workerID);

            if (tsSratDataRows.Length == 0) return;

            tsSratDataRows[0]["WorkingDaysCount"] = newTimesheetStatDataRow["WorkingDaysCount"];
            tsSratDataRows[0]["TimesheetSumm"] = newTimesheetStatDataRow["TimesheetSumm"];
            tsSratDataRows[0]["InsalubrityTime"] = newTimesheetStatDataRow["InsalubrityTime"];
            tsSratDataRows[0]["NightTime"] = newTimesheetStatDataRow["NightTime"];
            tsSratDataRows[0]["SickTime"] = newTimesheetStatDataRow["SickTime"];
            tsSratDataRows[0]["OwnExpenseTime"] = newTimesheetStatDataRow["OwnExpenseTime"];
            tsSratDataRows[0]["TimeOnBasisNorms"] = newTimesheetStatDataRow["TimeOnBasisNorms"];
            tsSratDataRows[0]["ExceedingTime"] = newTimesheetStatDataRow["ExceedingTime"];
            tsSratDataRows[0]["ExceedingAllTime"] = newTimesheetStatDataRow["ExceedingAllTime"];
            //else
            //{
            //    TimesheetStatDataTable.Rows.Add(newTimesheetStatDataRow);
            //}
        }

        private int CalculateWorkingDaysCount(DataRowView timesheetRowView)
        {
            int workingDaysCount = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                if (Convert.ToInt32(timesheetRowView["s" + i]) != 0)
                    workingDaysCount++;
            }
            return workingDaysCount;
        }

        private decimal CalculateTimesheetSumm(DataRowView timesheetRowView, ref int nitghShiftsCount)
        {

            decimal timesheetSumm = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                if (Convert.ToInt32(timesheetRowView["s" + i]) == 0) continue;
                if (timesheetRowView["d" + i].ToString() == "-") continue;

                int absencesTypeID = Convert.ToInt32(timesheetRowView["t" + i]);

                var absencesDataRows =
                    AbsencesTypesDataTable.Select("AbsencesTypeID = " + absencesTypeID);

                if (!absencesDataRows.Any()) continue;

                var considerInResult = Convert.ToBoolean(absencesDataRows[0]["ConsiderInResult"]);
                var considerNorm = Convert.ToBoolean(absencesDataRows[0]["ConsiderNorm"]);

                if (!considerInResult || considerNorm) continue;

                Decimal dayTime = 0;
                var result = Decimal.TryParse(timesheetRowView["d" + i].ToString(), out dayTime);

                if (result)
                {
                    // night shift and working hours more then 6
                    if (absencesTypeID == 6 && dayTime > 6)
                        nitghShiftsCount++;

                    timesheetSumm = timesheetSumm + dayTime;
                }
            }
            return timesheetSumm;
        }

        private decimal CalculateInsalubrityTime(DataRowView timesheetRowView)
        {
            decimal insalubrityTime = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                if (Convert.ToInt32(timesheetRowView["s" + i]) == 0) continue;
                if (timesheetRowView["d" + i].ToString() == "-") continue;

                var absencesDataRows =
                    AbsencesTypesDataTable.Select("AbsencesTypeID = " + timesheetRowView["t" + i]);

                if (!absencesDataRows.Any()) continue;

                var considerInResult = Convert.ToBoolean(absencesDataRows[0]["ConsiderInResult"]);
                var considerNorm = Convert.ToBoolean(absencesDataRows[0]["ConsiderNorm"]);

                if (!considerInResult || considerNorm) continue;

                Decimal dayTime = 0;
                var result = Decimal.TryParse(timesheetRowView["i" + i].ToString(), out dayTime);

                if (result)
                    insalubrityTime = insalubrityTime + dayTime;
            }
            return insalubrityTime;
        }

        private decimal CalculateNightTime(DataRowView timesheetRowView)
        {
            decimal nightTime = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                if (Convert.ToInt32(timesheetRowView["s" + i]) == 0) continue;
                if (timesheetRowView["d" + i].ToString() == "-") continue;

                var absencesDataRows =
                    AbsencesTypesDataTable.Select("AbsencesTypeID = " + timesheetRowView["t" + i]);

                if (!absencesDataRows.Any()) continue;

                var considerInResult = Convert.ToBoolean(absencesDataRows[0]["ConsiderInResult"]);
                var considerNorm = Convert.ToBoolean(absencesDataRows[0]["ConsiderNorm"]);

                if (!considerInResult || considerNorm) continue;

                Decimal dayTime = 0;
                var result = Decimal.TryParse(timesheetRowView["n" + i].ToString(), out dayTime);

                if (result)
                    nightTime = nightTime + dayTime;
            }
            return nightTime;
        }

        private decimal CalculateSickTime(DataRowView timesheetRowView)
        {
            decimal sickTime = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                //if (Convert.ToInt32(timesheetRowView["s" + i]) == 0) continue;
                //if (timesheetRowView["d" + i].ToString() == "-") continue;

                if (Convert.ToInt32(timesheetRowView["t" + i]) != 4) continue;

                var day = new DateTime(_year, _month, i);
                var hours = 8;

                if (IsHoliday(day)) hours = 0;
                else if (IsHoliday(day.AddDays(1), false)) hours = 7;

                //if (IsHoliday(day))  hours = 0;
                //if (IsHoliday(day.AddDays(1))) hours = 7;

                sickTime = sickTime + hours;
            }
            return sickTime;
        }

        private decimal CalculateOwnExpense(DataRowView timesheetRowView)
        {
            decimal ownExpenseTime = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                //if (Convert.ToInt32(timesheetRowView["s" + i]) == 0) continue;
                //if (timesheetRowView["d" + i].ToString() == "-") continue;

                if (Convert.ToInt32(timesheetRowView["t" + i]) != 5) continue;

                var day = new DateTime(_year, _month, i);
                var hours = 8;

                //if (IsHoliday(day)) hours = 0;
                //if (IsHoliday(day.AddDays(1))) hours = 7;

                if (IsHoliday(day)) hours = 0;
                else if (IsHoliday(day.AddDays(1), false)) hours = 7;

                ownExpenseTime = ownExpenseTime + hours;
            }
            return ownExpenseTime;
        }

        private decimal CalculateTimeOnBasisNorms(DataRowView timesheetRowView)
        {
            decimal timeOnBasisNorms = 0;

            for (int i = 1; i < _daysCount + 1; i++)
            {
                int absencesTypeID = Convert.ToInt32(timesheetRowView["t" + i]);

                if ((absencesTypeID == 4) || (absencesTypeID == 5)) continue;

                var absencesDataRows =
                    AbsencesTypesDataTable.Select("AbsencesTypeID = " + absencesTypeID);

                if (!absencesDataRows.Any()) continue;

                var considerNorm = Convert.ToBoolean(absencesDataRows[0]["ConsiderNorm"]);

                if (!considerNorm) continue;

                var day = new DateTime(_year, _month, i);
                var hours = 8;

                if (IsHoliday(day)) hours = 0;
                else if (IsHoliday(day.AddDays(1), false)) hours = 7;

                timeOnBasisNorms = timeOnBasisNorms + hours;
            }
            return timeOnBasisNorms;
        }

        private double CalculateExceedingTime(int year, int month, decimal timesheetSumm, decimal timeOnBasisNorms, decimal sickTime, int nitghShiftsCount)
        {
            double exceedingTime = 0;

            var day = new DateTime(year, month, 1);

            string filterStr = "Date = '" + day.ToShortDateString() + "'";
            DataRow[] dataRows = _prodCalendarDataTable.Select(filterStr);

            if (!dataRows.Any()) return exceedingTime;

            int standartTime = Convert.ToInt32(dataRows[0]["Standart40Time"]);

            exceedingTime = Convert.ToDouble(timesheetSumm + timeOnBasisNorms + sickTime + nitghShiftsCount) - standartTime;

            return Math.Round(exceedingTime, 2);
        }

        private double CalculateExceedingAllTime(int workerID, int month, double exceedingTime)
        {
            double exceedingAllTime = exceedingTime;

            DataRow[] dataRowsEt = _timesheetStatETDataTable.Select("WorkerID =" + workerID);

            exceedingAllTime =
                dataRowsEt.Where(dataRowEt => Convert.ToDateTime(dataRowEt["TimesheetDate"]).Month != month)
                    .Aggregate(exceedingAllTime,
                        (current, dataRowEt) => current + Convert.ToDouble(dataRowEt["ExceedingTime"]));

            return Math.Round(exceedingAllTime, 2);
        }

        private bool IsDeviation(DataRow tsDatarow)
        {
            PSC.FillPlannedScheduleForWorker(Convert.ToDateTime(tsDatarow["TimesheetDate"]), Convert.ToInt32(tsDatarow["WorkerID"]));
            
            if (PSC.WorkerPlannedScheduleDataTable.Rows.Count == 0)
                return false;

            var timesheetDate = Convert.ToDateTime(tsDatarow["TimesheetDate"]);


            int _daysInMonth = DateTime.DaysInMonth(timesheetDate.Year, timesheetDate.Month);

            if (timesheetDate.Month != _currentDate.Month)
            {
                for (int i = 1; i <= 31; i++)
                {
                    if (i <= _daysInMonth)
                    {
                        string s1 = tsDatarow["d" + i].ToString();

                        if (s1 == "-" || s1 == "") s1 = 0.ToString(CultureInfo.InvariantCulture);

                        string s2 = PSC.WorkerPlannedScheduleDataTable.Rows[0]["d" + i].ToString();

                        decimal d1;
                        decimal d2;

                        if (Decimal.TryParse(s1, out d1) && Decimal.TryParse(s2, out d2))
                        {
                            if (d1 != d2) return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {

                for (int i = 1; i < 32; i++)
                {
                    if (i <= _currentDate.Day && i <= _daysInMonth)
                    {
                        string s1 = tsDatarow["d" + i].ToString();

                        if (s1 == "-" || s1 == "") s1 = 0.ToString(CultureInfo.InvariantCulture);

                        string s2 = PSC.WorkerPlannedScheduleDataTable.Rows[0]["d" + i].ToString();

                        decimal d1;
                        decimal d2;

                        if (Decimal.TryParse(s1, out d1) && Decimal.TryParse(s2, out d2))
                        {
                            if (d1 != d2) return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
