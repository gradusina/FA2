using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FA2.Ftp;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class AdministrationClass
    {
        static AdministrationClass()
        {
            AllowAnnimations = true;
        }

        public enum Modules {Login = 0, ControlTimeTracking, TimeTracking, 
            Workers, OperationCatalog, ServiceEquipment, ProductionCalendar, 
            ProductionRooms, TimeSheet, WorkersStimulation, TechnologyProblem,
            NewsFeed, PlannedWorks, Administration, ProductionSchedule, WorkshopMap,
            TasksPage, WorkerRequests}


        #region Variables

        #region DataVariables

        private readonly string _connectionString;

        private MySqlDataAdapter _journalProgramEntryAdapter;
        private DataTable _journalProgramEntryTable;
        /// <summary>
        /// Журнал вх/вых в программу.
        /// </summary>
        public DataTable JournalProgramEntryTable
        {
            get { return _journalProgramEntryTable; }
            set { _journalProgramEntryTable = value; }
        }

        private MySqlDataAdapter _journalBrowsingModulesAdapter;
        private DataTable _journalBrowsingModulesTable;
        /// <summary>
        /// Журнал вх/вых в модули программы.
        /// </summary>
        public DataTable JournalBrowsingModulesTable
        {
            get { return _journalBrowsingModulesTable; }
            set { _journalBrowsingModulesTable = value; }
        }

        private MySqlDataAdapter _journalWorkInModulesAdapter;
        private DataTable _journalWorkInModulesTable;
        /// <summary>
        /// Журнал выполненных действий в программе.
        /// </summary>
        public DataTable JournalWorkInModulesTable
        {
            get { return _journalWorkInModulesTable; }
            set { _journalWorkInModulesTable = value; }
        }

        private MySqlDataAdapter _actionTypesAdapter;
        private DataTable _actionTypesTable;
        /// <summary>
        /// Таблица выполняемых действий в программе.
        /// </summary>
        public DataTable ActionTypesTable
        {
            get { return _actionTypesTable; }
            set { _actionTypesTable = value; }
        }

        private MySqlDataAdapter _modulesAdapter;
        private DataTable _modulesTable;
        /// <summary>
        /// Таблица модулей программы.
        /// </summary>
        public DataTable ModulesTable
        {
            get { return _modulesTable; }
            set { _modulesTable = value; }
        }

        private MySqlDataAdapter _accessGroupsAdapter;
        private DataTable _accessGroupsTable;
        /// <summary>
        /// Таблица существующих групп прав доступа.
        /// </summary>
        public DataTable AccessGroupsTable
        {
            get { return _accessGroupsTable; }
            set { _accessGroupsTable = value; }
        }

        private MySqlDataAdapter _availableModulesAdapter;
        private DataTable _availableModulesTable;
        /// <summary>
        /// Таблица доступных модулей в группах доступа.
        /// </summary>
        public DataTable AvailableModulesTable
        {
            get { return _availableModulesTable; }
            set { _availableModulesTable = value; }
        }

        private MySqlDataAdapter _accessGroupStructureAdapter;
        private DataTable _accessGroupStructureTable;
        /// <summary>
        /// Таблица отображающая работников входящих в группы доступа.
        /// </summary>
        public DataTable AccessGroupStructureTable
        {
            get { return _accessGroupStructureTable; }
            set { _accessGroupStructureTable = value; }
        }

        private MySqlDataAdapter _workersAccessAdapter;
        private DataTable _workersAccessTable;
        /// <summary>
        /// Таблица доступа работников к модулям.
        /// </summary>
        public DataTable WorkersAccessTable
        {
            get
            {
                // Fill, if table is empty
                if (_workersAccessTable == null || _workersAccessTable.Columns.Count == 0)
                    FillWorkersAccess();

                return _workersAccessTable;
            }
            set { _workersAccessTable = value; }
        }

        private MySqlDataAdapter _reportAdapter;
        private DataTable _reportTable;
        public DataTable ReportTable
        {
            get
            {
                if(_reportTable.Columns.Count == 0)
                    FillEmptyProgrammReport();
                return _reportTable;
            }
            set { _reportTable = value; }
        }

        #endregion

        #region StaticVariables

        /// <summary>
        /// Returns or set Id of current worker.
        /// </summary>
        public static int CurrentWorkerId { get; set; }

        public static bool IsAdministrator;

        /// <summary>
        /// Returns or set Id of current module.
        /// </summary>
        public static Modules CurrentModuleId { get; set; }

        public static bool AllowAnnimations { get; set; }

        #endregion


        private static int _currentJournalProgramEntryId;
        private static int _currentJournalBrowsingModuleId;


        private DateTime _dateFrom;
        private DateTime _dateTo;

        public static string FavoritesModulesForWorker;

        #endregion


        public AdministrationClass(string connectonString)
        {
            _connectionString = connectonString;
            Initialize();
        }

        private void Initialize()
        {
            Create();

            WorkersAccessAdapterInit();
            Fill();
        }

        private void Create()
        {
            _journalProgramEntryTable = new DataTable();
            _journalBrowsingModulesTable = new DataTable();
            _journalWorkInModulesTable = new DataTable();
            _actionTypesTable = new DataTable();

            _modulesTable = new DataTable();
            _accessGroupsTable = new DataTable();
            _availableModulesTable = new DataTable();
            _accessGroupStructureTable = new DataTable();
            _workersAccessTable = new DataTable();
            _reportTable = new DataTable();
        }

        private void Fill()
        {
            //FillJournalProgramEntry();
            //FillJournalBrowsingModules();
            //FillJournalWorkInModules();
            FillActionTypes();
            FillModules();
            FillAccessGroups();
            FillAvailableModules();
            FillAccessGroupStructure();
        }
        
        /// <summary>
        /// Заполняет таблицы в определённом интервале времени.
        /// </summary>
        /// <param name="dateFrom">Начало периода времени.</param>
        /// <param name="dateTo">Конец периода времени.</param>
        public void Fill(DateTime dateFrom, DateTime dateTo)
        {
            _dateFrom = dateFrom;
            _dateTo = dateTo;

            FillJournalProgramEntry();
            FillJournalBrowsingModules();
            FillJournalWorkInModules();
        }


        #region Views

        /// <summary>
        /// Представление журнала вх/вых в программу.
        /// </summary>
        public DataView JournalProgramEntryView
        {
            get
            {
                return _journalProgramEntryTable != null ? 
                    _journalProgramEntryTable.AsDataView() : null;
            }
        }

        /// <summary>
        /// Представление журнала вх/вых в модули программы.
        /// </summary>
        public DataView JournalBrowsingModulesView
        {
            get
            {
                return _journalBrowsingModulesTable != null ?
                    _journalBrowsingModulesTable.AsDataView() : null;
            }
        }

        /// <summary>
        /// Представление выполненных действий в программе.
        /// </summary>
        public DataView JournalWorkInModulesView
        {
            get
            {
                return _journalWorkInModulesTable != null ?
                    _journalWorkInModulesTable.AsDataView() : null;
            }
        }

        /// <summary>
        /// Представление модулей программы.
        /// </summary>
        public DataView ModulesView
        {
            get
            {
                if(_modulesTable == null) return null;
                var view = _modulesTable.AsDataView();
                view.Sort = "ModuleName";
                view.RowFilter = "ShowInFileStorage = 'TRUE'";
                return view;
            }
        }

        /// <summary>
        /// Представление групп доступа программы.
        /// </summary>
        public DataView AccessGroupsView
        {
            get
            {
                return _accessGroupsTable != null ?
                    _accessGroupsTable.AsDataView() : null;
            }
        }

        public DataView ActionsTypesView
        {
            get
            {
                return _actionTypesTable != null ?
                    _actionTypesTable.AsDataView() : null;
            }
        }

        /// <summary>
        /// Представление доступных модулей работникам программы.
        /// </summary>
        public DataView WorkersAccessView
        {
            get { return WorkersAccessTable.AsDataView(); }
        }

        #endregion


        #region AdapterInitialize

        private void WorkersAccessAdapterInit()
        {
            const string sqlCommandText =
                "SELECT WorkerAccessID, WorkerID, ModuleID, FullAccess, Access " +
                "FROM FAIIAdministration.WorkersAccess";
            _workersAccessAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_workersAccessAdapter);
        }

        #endregion


        #region Fillings

        private void FillJournalProgramEntry()
        {
            try
            {
                const string commandText = @"SELECT JPEID, WorkerID, EntryDate, ExitDate 
                                             FROM FAIIAdministration.JournalProgramEntry
                                             WHERE EntryDate  BETWEEN @DateFrom AND @DateTo";
                var sqlCommand = new MySqlCommand(commandText, new MySqlConnection(_connectionString));
                sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = _dateFrom;
                sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = _dateTo;
                _journalProgramEntryAdapter = new MySqlDataAdapter(sqlCommand);
                new MySqlCommandBuilder(_journalProgramEntryAdapter);
                _journalProgramEntryTable.Clear();
                _journalProgramEntryAdapter.Fill(_journalProgramEntryTable);
            }
            catch(Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillJournalBrowsingModules()
        {
            try
            {
                const string commandText = @"SELECT JBMID, JPEID, ModuleID, EntryDate, ExitDate 
                                             FROM FAIIAdministration.JournalBrowsingModules
                                             WHERE EntryDate BETWEEN @DateFrom AND @DateTo";
                var sqlCommand = new MySqlCommand(commandText, new MySqlConnection(_connectionString));
                sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = _dateFrom;
                sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = _dateTo;
                _journalBrowsingModulesAdapter = new MySqlDataAdapter(sqlCommand);
                new MySqlCommandBuilder(_journalBrowsingModulesAdapter);
                _journalBrowsingModulesTable.Clear();
                _journalBrowsingModulesAdapter.Fill(_journalBrowsingModulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0002] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillJournalWorkInModules()
        {
            try
            {
                const string commandText = @"SELECT JWMID, JBMID, ActionTypeID, ActionDate
                                             FROM FAIIAdministration.JournalWorkInModules 
                                             WHERE ActionDate BETWEEN @DateFrom AND @DateTo";
                var sqlCommand = new MySqlCommand(commandText, new MySqlConnection(_connectionString));
                sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = _dateFrom;
                sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = _dateTo;
                _journalWorkInModulesAdapter = new MySqlDataAdapter(sqlCommand);
                new MySqlCommandBuilder(_journalWorkInModulesAdapter);
                _journalWorkInModulesTable.Clear();
                _journalWorkInModulesAdapter.Fill(_journalWorkInModulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0003] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillActionTypes()
        {
            try
            {
                _actionTypesAdapter = new MySqlDataAdapter("SELECT ActionTypeID, ModuleID, ActionName " + 
                    "FROM FAIIAdministration.ActionTypes", _connectionString);
                new MySqlCommandBuilder(_actionTypesAdapter);
                _actionTypesTable.Clear();
                _actionTypesAdapter.Fill(_actionTypesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillModules()
        {
            try
            {
                _modulesAdapter = new MySqlDataAdapter(@"SELECT ModuleID, ModuleName, ModuleDescription, 
                                                         ModuleIcon, ModuleColor, ModulesGroupsID,  
                                                         ShowInFileStorage, IsEnabled, IsSwitchOff 
                                                         FROM FAIIAdministration.Modules", _connectionString);
                new MySqlCommandBuilder(_modulesAdapter);
                _modulesTable.Clear();
                _modulesAdapter.Fill(_modulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillAccessGroups()
        {
            try
            {
                _accessGroupsAdapter = new MySqlDataAdapter(@"SELECT AccessGroupID, AccessGroupName, CheckSum 
                                                              FROM FAIIAdministration.AccessGroups", _connectionString);
                new MySqlCommandBuilder(_accessGroupsAdapter);
                _accessGroupsTable.Clear();
                _accessGroupsAdapter.Fill(_accessGroupsTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillAvailableModules()
        {
            try
            {
                _availableModulesAdapter = new MySqlDataAdapter(@"SELECT AvailableModuleID, AccessGroupID, ModuleID, FullAccess 
                                                                  FROM FAIIAdministration.AvailableModules", _connectionString);
                new MySqlCommandBuilder(_availableModulesAdapter);
                _availableModulesTable.Clear();
                _availableModulesAdapter.Fill(_availableModulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0007] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillAccessGroupStructure()
        {
            try
            {
                _accessGroupStructureAdapter = new MySqlDataAdapter(@"SELECT AccessGroupStructureID, WorkerID, AccessGroupID 
                                                                      FROM FAIIAdministration.AccessGroupStructure", _connectionString);
                new MySqlCommandBuilder(_accessGroupStructureAdapter);
                _accessGroupStructureTable.Clear();
                _accessGroupStructureAdapter.Fill(_accessGroupStructureTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0008] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkersAccess()
        {
            try
            {
                _workersAccessTable.Clear();
                _workersAccessAdapter.Fill(_workersAccessTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0009] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillProgrammReport(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText = @"SELECT ReportID, Date, UserName, MachineName, OSVersion, 
                                         Message, Source, TargetSite, StackTrace, IsCorrected
                                         FROM FAIIAdministration.Report
                                         WHERE Date BETWEEN @DateFrom AND @DateTo";

            try
            {
                var sqlCommand = new MySqlCommand(commandText, new MySqlConnection(_connectionString));
                sqlCommand.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                sqlCommand.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo;
                _reportAdapter = new MySqlDataAdapter(sqlCommand);
                new MySqlCommandBuilder(_reportAdapter);
                _reportTable.Clear();
                _reportAdapter.Fill(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0010] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEmptyProgrammReport()
        {
            const string commandText = @"SELECT ReportID, Date, UserName, MachineName, OSVersion, 
                                         Message, Source, TargetSite, StackTrace, IsCorrected
                                         FROM FAIIAdministration.Report";
            try
            {
                _reportAdapter = new MySqlDataAdapter(commandText, _connectionString);
                new MySqlCommandBuilder(_reportAdapter);
                _reportTable.Clear();
                _reportAdapter.Fill(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0011] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region Static functions & methods

        public static int GetWorkerIdByComputerName()
        {
            var workerId = 0;

            using (var cnz = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCmnd =
                    new MySqlCommand(@"SELECT WorkerID FROM FAIIAdministration.JournalProgramEntry 
                                       WHERE ComputerName = @ComputerName ORDER BY EntryDate DESC LIMIT 1 ",
                        cnz);

                sqlCmnd.Parameters.Add("@ComputerName", MySqlDbType.LongText).Value =
                    Environment.MachineName.ToString(CultureInfo.InvariantCulture);

                sqlCmnd.CommandType = CommandType.Text;
                try
                {
                    cnz.Open();

                    var sqlCmndRes = sqlCmnd.ExecuteScalar();

                    if (sqlCmndRes != null)
                    {
                        workerId = Convert.ToInt32(sqlCmndRes);
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    cnz.Close();
                }
            }

            return workerId;
        }


        private static void CloseProgramEntry(int jpeId, MySqlConnection sqlConn)
        {
            const string closeEntryCommandText = @"UPDATE FAIIAdministration.JournalProgramEntry 
                                                   SET ExitDate = @ExitDate WHERE JPEID = @JPEID";

            // Get current datetime and send to database.
            var exitDate = App.BaseClass.GetDateFromSqlServer();

            using (var command = new MySqlCommand(closeEntryCommandText, sqlConn))
            {
                command.Parameters.Add("@ExitDate", MySqlDbType.DateTime).Value = exitDate;
                command.Parameters.Add("@JPEID", MySqlDbType.Int64).Value = jpeId;
                command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Opens new programm entry for choosen worker.
        /// </summary>
        /// <param name="workerId">Current worker</param>
        public static void OpenNewProgramEntry(int workerId)
        {
            CurrentWorkerId = workerId;

            IsAdministrator = GetIsAdministrator(CurrentWorkerId);

            if (CurrentWorkerId == 0) return;

            // Geting last id where exit date is null command text.
            const string exitDateCommandText = @"SELECT JPEID , ExitDate FROM FAIIAdministration.JournalProgramEntry 
                                                 WHERE WorkerID = @WorkerID AND ExitDate IS NULL ORDER BY JPEID DESC LIMIT 1";
            // Insert new row command text.
            const string commandText = @"INSERT INTO FAIIAdministration.JournalProgramEntry 
                                         (WorkerID, EntryDate, ComputerName) VALUES (@WorkerID, @EntryDate, @ComputerName)";
            // Geting last id command text.
            const string getIdCommandText = @"SELECT JPEID FROM FAIIAdministration.JournalProgramEntry 
                                              WHERE WorkerID = @WorkerID AND EntryDate = @EntryDate 
                                              ORDER BY JPEID DESC LIMIT 1";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    object sqlResult = null;
                    var exitDateCommand = new MySqlCommand(exitDateCommandText, sqlConn);
                    exitDateCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;

                    // Get last row from database for current worker.
                    using (var reader = exitDateCommand.ExecuteReader())
                    {
                        reader.Read();
                        // Check if field 'ExitDate' is NULL.
                        if (reader.HasRows && reader[1] == DBNull.Value)
                        {
                            sqlResult = reader[0];
                        }
                        reader.Close();
                    }

                    if(sqlResult != null)
                    {
                        var jpeId = Convert.ToInt32(sqlResult);
                        // Close last entry if not closed.
                        CloseProgramEntry(jpeId, sqlConn);
                    }

                    // Get current datetime.
                    var entryDate = App.BaseClass.GetDateFromSqlServer();

                    var command = new MySqlCommand(commandText, sqlConn);
                    // Set parameters for sqlcommand.
                    command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                    command.Parameters.Add("@EntryDate", MySqlDbType.DateTime).Value = entryDate;
                    command.Parameters.Add("@ComputerName", MySqlDbType.VarChar).Value = 
                        Environment.MachineName.ToString(CultureInfo.InvariantCulture);
                    // Insert new row to database.
                    command.ExecuteScalar();

                    var getIdCommand = new MySqlCommand(getIdCommandText, sqlConn);
                    // Set parameters for sqlcommand.
                    getIdCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                    getIdCommand.Parameters.Add("@EntryDate", MySqlDbType.DateTime).Value = entryDate;
                    // Get last added id of program entry for current worker.
                    var id = getIdCommand.ExecuteScalar();
                    if (id != null && id != DBNull.Value)
                    {
                        _currentJournalProgramEntryId = Convert.ToInt32(id);
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }
        }

        private static bool GetIsAdministrator(long workerId)
        {
            var result = false;

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    const string commandText = @"SELECT COUNT(AccessGroupStructureID) 
                                                 FROM FAIIAdministration.AccessGroupStructure 
                                                 WHERE WorkerID = @WorkerID AND AccessGroupID = 4";

                    var command = new MySqlCommand(commandText, sqlConn);
                    command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                    var sqlResult = command.ExecuteScalar();

                    if(sqlResult != null)
                    {
                        var count = Convert.ToInt32(sqlResult);
                        
                        if (count > 0) result = true;
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

            return result;
        }


        /// <summary>
        /// Closes program entry for current worker.
        /// </summary>
        public static void CloseProgramEntry()
        {
            IsAdministrator = false;
            if (_currentJournalProgramEntryId == 0) return;

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    // Close current program entry.
                    CloseProgramEntry(_currentJournalProgramEntryId, sqlConn);

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            _currentJournalProgramEntryId = 0;
            CurrentWorkerId = 0;
        }

        private static void CloseModuleEntry(int jbmId, MySqlConnection sqlConn)
        {
            const string closeEntryCommandText = @"UPDATE FAIIAdministration.JournalBrowsingModules 
                                                   SET ExitDate = @ExitDate WHERE JBMID = @JBMID";

            // Get current datetime and send to database.
            var exitDate = App.BaseClass.GetDateFromSqlServer();
            var command = new MySqlCommand(closeEntryCommandText, sqlConn);
            command.Parameters.Add("@ExitDate", MySqlDbType.DateTime).Value = exitDate;
            command.Parameters.Add("@JBMID", MySqlDbType.Int64).Value = jbmId;
            command.ExecuteScalar();
        }

        /// <summary>
        /// Opens new module entry for current worker.
        /// </summary>
        /// <param name="module">Opened module</param>
        public static void OpenNewModuleEntry(Modules module)
        {
            CloseModuleEntry();

            CurrentModuleId = module;

            if (CurrentModuleId == 0 || _currentJournalProgramEntryId == 0) return;

            // Geting last id where exit date is null command text.
            const string exitDateCommandText = @"SELECT JBMID, ExitDate FROM FAIIAdministration.JournalBrowsingModules 
                                        WHERE JPEID = @JPEID AND ModuleID = @ModuleID AND ExitDate IS NULL ORDER BY JBMID DESC LIMIT 1";
            // Insert new row command text.
            const string insertNewRowCommandText = @"INSERT INTO FAIIAdministration.JournalBrowsingModules 
                                            (JPEID, ModuleID, EntryDate) VALUES (@JPEID, @ModuleID, @EntryDate)";
            // Geting last id command text.
            const string getIdCommandText = @"SELECT JBMID FROM FAIIAdministration.JournalBrowsingModules 
                                     WHERE JPEID = @JPEID AND ModuleID = @ModuleID AND EntryDate = @EntryDate ORDER BY JBMID DESC LIMIT 1";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    object sqlResult = null;
                    var exitDateCommand = new MySqlCommand(exitDateCommandText, sqlConn);
                    exitDateCommand.Parameters.Add("@JPEID", MySqlDbType.Int64).Value = _currentJournalProgramEntryId;
                    exitDateCommand.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = CurrentModuleId;

                    // Get last row from database for current entry.
                    using (var reader = exitDateCommand.ExecuteReader())
                    {
                        reader.Read();
                        // Check if field 'ExitDate' is NULL.
                        if (reader.HasRows && reader[1] == DBNull.Value)
                        {
                            sqlResult = reader[0];
                        }
                        reader.Close();
                    }

                    if (sqlResult != null)
                    {
                        var jbmId = Convert.ToInt32(sqlResult);

                        // Close last entry if not closed.
                        CloseModuleEntry(jbmId, sqlConn);
                    }

                    // Get current datetime.
                    var entryDate = App.BaseClass.GetDateFromSqlServer();

                    var insertCommand = new MySqlCommand(insertNewRowCommandText, sqlConn);
                    // Set parameters for sqlcommand.
                    insertCommand.Parameters.Add("@JPEID", MySqlDbType.Int64).Value = _currentJournalProgramEntryId;
                    insertCommand.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = CurrentModuleId;
                    insertCommand.Parameters.Add("@EntryDate", MySqlDbType.DateTime).Value = entryDate;

                    insertCommand.ExecuteScalar();

                    var getIdCommand = new MySqlCommand(getIdCommandText, sqlConn);
                    // Set parameters for sqlcommand.
                    getIdCommand.Parameters.Add("@JPEID", MySqlDbType.Int64).Value = _currentJournalProgramEntryId;
                    getIdCommand.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = CurrentModuleId;
                    getIdCommand.Parameters.Add("@EntryDate", MySqlDbType.DateTime).Value = entryDate;
                    // Get last added id of module entry for current worker.
                    var id = getIdCommand.ExecuteScalar();
                    if (id != null && id != DBNull.Value)
                    {
                        _currentJournalBrowsingModuleId = Convert.ToInt32(id);
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }
        }

        /// <summary>
        /// Closes module entry for current worker.
        /// </summary>
        public static void CloseModuleEntry()
        {
            if (_currentJournalBrowsingModuleId == 0) return;

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    // Close current module entry.
                    CloseModuleEntry(_currentJournalBrowsingModuleId, sqlConn);

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            _currentJournalBrowsingModuleId = 0;
        }

        /// <summary>
        /// Ads new action to journal for current worker.
        /// </summary>
        /// <param name="actionTypeId">Id of an action</param>
        public static void AddNewAction(int actionTypeId)
        {
            if (_currentJournalBrowsingModuleId == 0) return;

            const string commandText = @"INSERT INTO FAIIAdministration.JournalWorkInModules (JBMID, ActionTypeID, ActionDate) 
                                VALUES (@JBMID, @ActionTypeID, @ActionDate)";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    // Get datetime of the action.
                    var actionDate = App.BaseClass.GetDateFromSqlServer();

                    var insertCommand = new MySqlCommand(commandText, sqlConn);
                    insertCommand.Parameters.Add("@JBMID", MySqlDbType.Int64).Value = _currentJournalBrowsingModuleId;
                    insertCommand.Parameters.Add("@ActionTypeID", MySqlDbType.Int64).Value = actionTypeId;
                    insertCommand.Parameters.Add("@ActionDate", MySqlDbType.DateTime).Value = actionDate;

                    sqlConn.Open();
                    // Insert new row to database.
                    insertCommand.ExecuteScalar();
                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }
        }

        /// <summary>
        /// Return modules, that are available for current worker. 
        /// Warning! Returns null, if current worker is not checked.
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAvailableModulesForWorker()
        {
            if (CurrentWorkerId == 0) return null;

            var additionalFilter = IsAdministrator
                ? string.Empty
                : "AND modules.IsSwitchOff = FALSE ";

            var commandText = @"SELECT modules.ModuleID, 
                                modules.ModuleName, 
                                modules.ModuleDescription, 
                                modules.ModuleIcon, 
                                modules.ModuleColor, 
                                modules.ModulesGroupsID 
                                FROM FAIIAdministration.Modules modules INNER JOIN 
                                FAIIAdministration.WorkersAccess workerAccess 
                                ON modules.ModuleID = workerAccess.ModuleID 
                                WHERE workerAccess.WorkerID = @WorkerID 
                                AND modules.IsEnabled = TRUE " + additionalFilter +
                                "ORDER BY modules.ModuleID";
            var table = new DataTable();
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                var adapter = new MySqlDataAdapter(command);
                try
                {
                    sqlConn.Open();
                    adapter.Fill(table);
                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            return table;
        }

        public static String GetFavoritesModulesIdsForWorker()
        {
            if (CurrentWorkerId == 0) return null;

            var dateFrom = DateTime.MinValue;
            var dateTo = App.BaseClass.GetDateFromSqlServer();
            if (dateTo > DateTime.MinValue)
                dateFrom = dateTo.Subtract(TimeSpan.FromDays(180));

            const string commandText = @"SELECT ModuleID FROM 
                                        (SELECT COUNT(ModuleID) AS Module_count, ModuleID 
                                         FROM (SELECT JBM.ModuleID, JPE.WorkerID 
                                               FROM (SELECT * FROM FAIIAdministration.JournalBrowsingModules
                                                     WHERE @DateFrom < EntryDate AND EntryDate < @DateTo) AS JBM 
                                               INNER JOIN (SELECT JPEID AS ID, WorkerID 
                                                           FROM FAIIAdministration.JournalProgramEntry 
                                                           WHERE WorkerID = @WorkerID AND
                                                                 @DateFrom < EntryDate AND EntryDate < @DateTo) AS JPE
                                               ON JBM.JPEID = JPE.ID 
                                               WHERE (JBM.ModuleID IN 
                                                     (SELECT Modules.ModuleID 
                                                      FROM FAIIAdministration.Modules AS Modules 
                                                      INNER JOIN (SELECT ModuleID
															      FROM FAIIAdministration.WorkersAccess
																  WHERE WorkerID = @WorkerID AND Access = true) as availableModules
                                                      ON Modules.ModuleID = availableModules.ModuleID))) AS ResTable 
                        GROUP BY ModuleID 
                        ORDER BY Module_count DESC 
                        LIMIT 5) AS Favorites";

            var table = new DataTable();
            //string s = null;

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);

                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                command.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                command.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo;

                var adapter = new MySqlDataAdapter(command);
                try
                {
                    sqlConn.Open();

                    adapter.Fill(table);
                }
                catch
                {
                }
                finally
                {
                    sqlConn.Close();
                }
            }

            if (table.Rows.Count != 0)
            {
                var ids = table.DefaultView.Cast<DataRowView>()
                    .Aggregate<DataRowView, string>(null, (current, drv) => current + drv["ModuleID"] + ", ");
                FavoritesModulesForWorker = ids.Remove(ids.Length - 2, 2);
            }
            else
            {
                FavoritesModulesForWorker = "-1";
            }

            return FavoritesModulesForWorker;
        }


        /// <summary>
        /// Check if current worker has full access for current module.
        /// Returns true if has, and false if not.
        /// </summary>
        /// <returns></returns>
        public static bool HasFullAccess(Modules module)
        {
            if (CurrentWorkerId == 0) return false;

            var fullAccess = false;
            const string commandText = @"SELECT FullAccess 
                                         FROM FAIIAdministration.WorkersAccess 
                                         WHERE WorkerID = @WorkerID AND ModuleID = @ModuleID AND Access = TRUE";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                command.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = (int)module;
                try
                {
                    sqlConn.Open();
                    var sqlResult = command.ExecuteScalar();
                    sqlConn.Close();

                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        fullAccess = Convert.ToBoolean(sqlResult);
                    }
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            return fullAccess;
        }


        public static DateTime LastModuleExit(Modules module)
        {
            var lastExit = DateTime.MinValue;
            if (CurrentWorkerId == 0 || module == 0) return lastExit;


            const string commandText = @"SELECT m.ExitDate FROM FAIIAdministration.JournalProgramEntry p 
                                JOIN FAIIAdministration.JournalBrowsingModules m 
                                ON p.JPEID = m.JPEID WHERE m.ExitDate 
                                IS NOT NULL AND p.WorkerID = @WorkerID 
                                AND m.ModuleID = @ModuleID ORDER BY m.EntryDate DESC LIMIT 1";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = CurrentWorkerId;
                command.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = (int)module;
                try
                {
                    sqlConn.Open();
                    var sqlResult = command.ExecuteScalar();
                    sqlConn.Close();

                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        lastExit = Convert.ToDateTime(sqlResult);
                    }
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            return lastExit;
        }


  	    /// <summary>
        /// Добавляет работника в указанную группу. Работник не должен состоять в одной из групп, 
        /// т.к. функция расчитана на добавление совершенно нового работника.
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="accessGroupId"></param>
        public static void AddNewWorkerToGroupBySql(Int64 workerId, int accessGroupId)
        {
            if (workerId < 1) return;

            // Get available modules for access group
            const string availableModulesForAccessGroupCommandText =
                                                        @"SELECT ModuleID, FullAccess 
                                                          FROM FAIIAdministration.AvailableModules 
                                                          WHERE AccessGroupID = @AccessGroupID";

            // Insert new rows to worker access table
            const string insertWorkerAccessCommandText = @"INSERT INTO FAIIAdministration.WorkersAccess 
                                                           (WorkerID, ModuleID, FullAccess, Access) 
                                                           VALUES (@WorkerID, @ModuleID, @FullAccess, TRUE)";

            // Insert new rows to access strucure table
            const string commandText = @"INSERT INTO FAIIAdministration.AccessGroupStructure 
                                         (WorkerID, AccessGroupID) VALUES (@WorkerID, @AccessGroupID)";


            #region Get availables modules for access group

            var availableModulesTable = new DataTable();
  	        using (
  	            var sqlAdapter = new MySqlDataAdapter(availableModulesForAccessGroupCommandText,
  	                App.ConnectionInfo.ConnectionString))
  	        {
  	            try
  	            {
  	                sqlAdapter.SelectCommand.Parameters.Add("@AccessGroupID", MySqlDbType.Int64).Value = accessGroupId;
  	                sqlAdapter.Fill(availableModulesTable);
  	            }
  	            catch (MySqlException)
  	            {
  	            }
  	        }

  	        #endregion

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                command.Parameters.Add("@AccessGroupID", MySqlDbType.Int64).Value = accessGroupId;

                try
                {
                    sqlConn.Open();

                    foreach (var dataRow in availableModulesTable.AsEnumerable())
                    {
                        var moduleId = Convert.ToInt32(dataRow["ModuleID"]);
                        var fullAccess = Convert.ToBoolean(dataRow["FullAccess"]);
                        using (var insertCommand = new MySqlCommand(insertWorkerAccessCommandText, sqlConn))
                        {
                            insertCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                            insertCommand.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = moduleId;
                            insertCommand.Parameters.Add("@FullAccess", MySqlDbType.Bit).Value = fullAccess;

                            insertCommand.ExecuteScalar();
                        }
                    }

                    command.ExecuteScalar();
                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }
        }


        public static byte[] BitmapImageToByte(BitmapImage bitmapImage)
        {
            byte[] data;

            using (var outStream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(outStream);
                data = outStream.ToArray();
            }

            return data;
        }

        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage;

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        public static BitmapImage ObjectToBitmapImage(object data)
        {
            var bitmap = new BitmapImage();

            if (data == null || data == DBNull.Value)
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Resources/nophoto.jpg", UriKind.Absolute);
                bitmap.EndInit();
            }
            else
            {
                using (var stream = new MemoryStream((byte[])data))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }
            }

            bitmap.Freeze();
            return bitmap;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception e)
            {
                if (e is IOException || e is UnauthorizedAccessException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    return true;
                }
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static void CleatTempFolder()
        {
            if (!Directory.Exists(App.TempFolder)) return;

            var direcoryInfo = new DirectoryInfo(App.TempFolder);
            foreach (var fileInfo in direcoryInfo.GetFiles())
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception)
                {
                }
            }
            foreach (var directoryInfo in direcoryInfo.GetDirectories())
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception)
                {
                }
            }
        }

        public static void ClearFtpTempFolder()
        {
            var tempDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Temp/";

            var ftpClient = new FtpClient(tempDirectory, "fa2app", "Franc1961");
            var clearingTimeInterval = TimeSpan.FromMinutes(10);


            if (!ftpClient.DirectoryExist(tempDirectory)) return;

            var fileList = ftpClient.ListDirectoryDetails();
            var list = fileList as IList<FtpFileDirectoryInfo> ?? fileList.ToList();
            if(!list.Any()) return;

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            foreach (var ftpFileDirectoryInfo in list)
            {
                DateTime fileDate;
                if (!DateTime.TryParse(ftpFileDirectoryInfo.Date, out fileDate)) continue;

                TimeSpan fileTime;
                if (!TimeSpan.TryParse(ftpFileDirectoryInfo.Time, out fileTime)) continue;

                var date = fileDate.Add(fileTime);
                if (clearingTimeInterval < currentDate.Subtract(date))
                    ftpClient.DeleteFile(ftpFileDirectoryInfo.Adress);
            }
        }

        public static void SendMessageToServer(string userName, string machineName, string osVersion,
            string message, string source, string targetSite, string stackTrace)
        {
            const string commandText =
                @"INSERT INTO FAIIAdministration.Report 
                 (UserName, MachineName, OSVersion, Message, Source, TargetSite, StackTrace, Date) 
                  VALUES 
                 (@UserName, @MachineName, @OSVersion, @Message, @Source, @TargetSite, @StackTrace, @Date)";

            var currentDate = DateTime.Now;

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(commandText, sqlConn);
                sqlCommand.Parameters.Add("@UserName", MySqlDbType.VarChar).Value = userName;
                sqlCommand.Parameters.Add("@MachineName", MySqlDbType.VarChar).Value = machineName;
                sqlCommand.Parameters.Add("@OSVersion", MySqlDbType.VarChar).Value = osVersion;
                sqlCommand.Parameters.Add("@Message", MySqlDbType.LongText).Value = message;
                sqlCommand.Parameters.Add("@Source", MySqlDbType.VarChar).Value = source;
                sqlCommand.Parameters.Add("@TargetSite", MySqlDbType.MediumText).Value = targetSite;
                sqlCommand.Parameters.Add("@StackTrace", MySqlDbType.LongText).Value = stackTrace;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.DateTime).Value = currentDate;

                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteScalar();
                }
                catch (Exception)
                {
                }
                finally
                {
                    sqlConn.Close();
                }
            }
        }

        public static void SendMessageToReport(string userName, string machineName, string osVersion,
            string message, string source, string targetSite, string stackTrace)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "report");
            var reportFile = new FileInfo(filePath);

            if (!reportFile.Exists)
            {
                var sw = reportFile.CreateText();
                sw.Close();
            }

            var streamWriter = new StreamWriter("report", true);
            streamWriter.Write("\r\n####################################################################");
            streamWriter.Write("\r\n" + DateTime.Now + " Пользователь: " + userName +
                " Компьютер: " + machineName + " ОС: " + osVersion);

            streamWriter.Write("\r\n --------------------------------------------------------------------");
            streamWriter.Write("\r\nОшибка: " + message);
            streamWriter.Write("\r\n --------------------------------------------------------------------");
            streamWriter.Write("\r\n" + "Имя приложения или объекта, вызывавшего ошибку: " + source);
            //streamWriter.Write("\r\n" + e.Exception.Source);
            streamWriter.Write("\r\n --------------------------------------------------------------------");
            streamWriter.Write("\r\n" + "Метод, создавший текущее исключение:");
            streamWriter.Write("\r\n" + targetSite);
            streamWriter.Write("\r\n --------------------------------------------------------------------");
            streamWriter.Write("\r\n" + "Строковое представление непосредственных кадров в стеке вызова:");
            streamWriter.Write("\r\n" + stackTrace);
            streamWriter.Write("\r\n####################################################################");
            streamWriter.Write("\r\n ");
            streamWriter.Close();
        }

        #endregion




        #region AccessGroups

        public void AddNewAccessGroup(string accessGroupName, int[] modulesIds, bool[] fullAccessArray)
        {
            var newRow = _accessGroupsTable.NewRow();
            newRow["AccessGroupName"] = accessGroupName;
            _accessGroupsTable.Rows.Add(newRow);

            // Refill access groups table
            UpdateAccessGroups();
            FillAccessGroups();

            // Get id of added row
            var accessGroup = GetAccessGroupByName(accessGroupName);
            if (accessGroup == null) return;

            // Set availables for this access group
            var accessGroupId = Convert.ToInt32(((DataRow)accessGroup)["AccessGroupID"]);

            var checkSum = 0;
            for (var i = 0; i < modulesIds.Length; i++)
            {
                AddNewAvailableModule(accessGroupId, modulesIds[i], fullAccessArray[i]);

                checkSum += modulesIds[i] * 10;
                if (fullAccessArray[i])
                    checkSum += 1;
            }

            SetCheckSum(accessGroupId, checkSum);
            RefillAvailableModules();
        }

        private object GetAccessGroupByName(string accessGroupName)
        {
            var rows = _accessGroupsTable.Select(string.Format("AccessGroupName = '{0}'", accessGroupName));
            return rows.Length != 0 ? rows[0] : null;
        }

        public void ChangeAccessGroupName(int accessGroupId, string accessGroupName)
        {
            var accessGroups = _accessGroupsTable.Select(string.Format("AccessGroupID = {0}", accessGroupId));
            if (accessGroups.Length == 0) return;

            var accessGroup = accessGroups[0];
            accessGroup["AccessGroupName"] = accessGroupName;
            UpdateAccessGroups();
        }

        public void SetCheckSum(int accessGroupId, int checkSum)
        {
            var accessGroups = _accessGroupsTable.Select(string.Format("AccessGroupID = {0}", accessGroupId));
            if (accessGroups.Length == 0) return;

            var accessGroup = accessGroups.First();
            accessGroup["CheckSum"] = checkSum;
            UpdateAccessGroups();
        }

        public void DeleteAccessGroup(int accessGroupId)
        {
            var groups = _accessGroupsTable.Select(string.Format("AccessGroupID = {0}", accessGroupId));
            if (groups.Length == 0) return;

            // Delete availables for deleting access group
            foreach (var available in _availableModulesTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId))
            {
                available.Delete();
            }
            UpdateAvailableModules();

            // Delete workers from deleting access group
            foreach (var groupStructure in _accessGroupStructureTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId))
            {
                groupStructure.Delete();
            }
            UpdateAccessGroupStructure();

            // Delete access group
            var group = groups[0];
            group.Delete();
            UpdateAccessGroups();
        }

        private void UpdateAccessGroups()
        {
            try
            {
                _accessGroupsAdapter.Update(_accessGroupsTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0009] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DataTable GetAvailableModulesForAccessGroup(int accessGroupId)
        {
            //Result table
            var table = new DataTable();
            table.Columns.Add("ModuleID", typeof(Int64));
            table.Columns.Add("ModuleName", typeof(string));
            table.Columns.Add("ModuleDescription", typeof(string));
            table.Columns.Add("ModuleIcon", typeof(byte[]));
            table.Columns.Add("ModuleColor", typeof(string));
            table.Columns.Add("FullAccess", typeof(bool));

            var availables = _availableModulesTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId);
            if (!availables.Any()) return table;
            var availableModules = availables.Join(_modulesTable.AsEnumerable(), a => a["ModuleID"], m => m["ModuleID"],
                (a, m) =>
                {
                    var newRow = table.NewRow();
                    newRow["ModuleID"] = m["ModuleID"];
                    newRow["ModuleName"] = m["ModuleName"];
                    newRow["ModuleDescription"] = m["ModuleDescription"];
                    newRow["ModuleIcon"] = m["ModuleIcon"];
                    newRow["ModuleColor"] = m["ModuleColor"];
                    newRow["FullAccess"] = a["FullAccess"];
                    return newRow;
                });

            return availableModules.CopyToDataTable();

            //_modulesTable.AsEnumerable().
            //Where(m => availables.Any(a => a.Field<Int64>("ModuleID") == m.Field<Int64>("ModuleID"))).CopyToDataTable();
        }

        #endregion


        #region AvailableModules

        public void AddNewAvailableModule(int accessGroupId, int moduleId, bool fullAccess)
        {
            var newRow = _availableModulesTable.NewRow();
            newRow["AccessGroupID"] = accessGroupId;
            newRow["ModuleID"] = moduleId;
            newRow["FullAccess"] = fullAccess;
            _availableModulesTable.Rows.Add(newRow);
        }

        public void DeleteAvailableModule(int accessGroupId, int moduleId)
        {
            var rows =
                AvailableModulesTable.AsEnumerable()
                    .Where(
                        r => r.Field<Int64>("AccessGroupID") == accessGroupId && r.Field<Int64>("ModuleID") == moduleId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            UpdateAvailableModules();
        }

        public void SetModuleFullAccessForAccessGroup(int accessGroupId, int moduleId, bool fullAccess)
        {
            var rows =
                AvailableModulesTable.AsEnumerable()
                    .Where(
                        r => r.Field<Int64>("AccessGroupID") == accessGroupId && r.Field<Int64>("ModuleID") == moduleId);
            if (!rows.Any()) return;

            var changingRow = rows.First();
            changingRow["FullAccess"] = fullAccess;

            UpdateAvailableModules();
        }

        private void UpdateAvailableModules()
        {
            try
            {
                _availableModulesAdapter.Update(_availableModulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0010] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefillAvailableModules()
        {
            UpdateAvailableModules();
            FillAvailableModules();
        }

        #endregion


        #region Modules

        public void AddNewModule(string moduleName, string description, byte[] icon, Color color,
            bool showInFileStorage, bool isSwitchOff)
        {
            var newRow = _modulesTable.NewRow();
            newRow["ModuleName"] = moduleName;
            newRow["ModuleDescription"] = description;
            newRow["ModuleIcon"] = icon;
            newRow["ModuleColor"] = color;
            newRow["ModulesGroupsID"] = 0;
            newRow["ShowInFileStorage"] = showInFileStorage;
            newRow["IsSwitchOff"] = isSwitchOff;
            _modulesTable.Rows.Add(newRow);

            // Refill modules
            UpdateModules();
            FillModules();
        }

        public void ChangeModule(int moduleId, string moduleName, string description, byte[] icon,
            Color color, bool showInFileStorage, bool isSwitchOff)
        {
            var modules = _modulesTable.Select(string.Format("ModuleID = {0}", moduleId));
            if (modules.Length == 0) return;

            var module = modules[0];
            module["ModuleName"] = moduleName;
            module["ModuleDescription"] = description;
            module["ModuleIcon"] = icon;
            module["ModuleColor"] = color;
            module["ShowInFileStorage"] = showInFileStorage;
            module["IsSwitchOff"] = isSwitchOff;

            UpdateModules();
        }

        public void DeleteModule(int moduleId)
        {
            var modules = _modulesTable.Select(string.Format("ModuleID = {0}", moduleId));
            if (modules.Length != 0)
            {
                var module = modules[0];
                module["IsEnabled"] = false;
                UpdateModules();
            }
        }

        private void UpdateModules()
        {
            try
            {
                _modulesAdapter.Update(_modulesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0011] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region AccessGroupStructure

        public void AddWorkersToGroup(IEnumerable<long> workers, int accessGroupId)
        {
            if (workers == null) return;

            // Delete all selected workers from access group structure
            var workersList = workers as IList<long> ?? workers.ToList();
            foreach (var accessGroupStructure in from workerId in workersList
                                                 select _accessGroupStructureTable.AsEnumerable().Where(w => w.Field<Int64>("WorkerID") == workerId)
                                                     into rows
                                                     where rows.Any()
                                                     select rows.First())
            {
                accessGroupStructure.Delete();
            }

            // Add workers to access gorup
            foreach (var worker in workersList)
            {
                AddWorkerToGroup(worker, accessGroupId);
            }

            // Refill access group structure
            UpdateAccessGroupStructure();
            FillAccessGroupStructure();
        }

        private void AddWorkerToGroup(Int64 workerId, int accessGroupId)
        {
            var newRow = _accessGroupStructureTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["AccessGroupID"] = accessGroupId;
            _accessGroupStructureTable.Rows.Add(newRow);
        }

        public void DeleteWorkersFromGroup(IEnumerable<long> workerIds)
        {
            if (workerIds == null) return;

            foreach (var workerId in workerIds)
            {
                var groups =
                    _accessGroupStructureTable.Select(string.Format("WorkerID = {0}",
                        workerId));
                if (groups.Length != 0)
                {
                    var group = groups[0];
                    group.Delete();
                }
            }

            UpdateAccessGroupStructure();
        }

        private void UpdateAccessGroupStructure()
        {
            try
            {
                _accessGroupStructureAdapter.Update(_accessGroupStructureTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0012] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region ActionTypes

        public void AddNewAction(int moduleId, string actionName)
        {
            var newRow = _actionTypesTable.NewRow();
            newRow["ModuleID"] = moduleId;
            newRow["ActionName"] = actionName;
            _actionTypesTable.Rows.Add(newRow);

            // Refill action types
            UpdateActionTypes();
            FillActionTypes();
        }

        public void ChangeAction(int actionTypeId, int moduleId, string actionName)
        {
            var actions = _actionTypesTable.Select(string.Format("ActionTypeID = {0}", actionTypeId));
            if (actions.Length == 0) return;

            var action = actions[0];
            action["ModuleID"] = moduleId;
            action["ActionName"] = actionName;

            UpdateActionTypes();
        }

        public void DeleteAction(int actionTypeId)
        {
            var actions = _actionTypesTable.Select(string.Format("ActionTypeID = {0}", actionTypeId));
            if (actions.Length == 0) return;

            var action = actions[0];
            action.Delete();
            UpdateActionTypes();
        }

        private void UpdateActionTypes()
        {
            try
            {
                _actionTypesAdapter.Update(_actionTypesTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0013] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region WorkersAccess

        public void SetFullAccessForWorker(int workerId, int moduleId, bool fullAccess)
        {
            var rows = WorkersAccessTable.Select(string.Format("WorkerID = {0} AND ModuleID = {1}", workerId, moduleId));
            if (!rows.Any()) return;

            var changingRow = rows.First();
            changingRow["FullAccess"] = fullAccess;

            UpdateWorkersAccess();
        }

        public void AddWorkerAccess(int workerId, int moduleId, bool fullAccess)
        {
            var newRow = WorkersAccessTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["ModuleID"] = moduleId;
            newRow["FullAccess"] = fullAccess;
            newRow["Access"] = true;
            WorkersAccessTable.Rows.Add(newRow);
        }

        public void DeleteWorkerAccess(int workerId, int moduleId)
        {
            var rows = WorkersAccessTable.Select(string.Format("WorkerID = {0} AND ModuleID = {1}", workerId, moduleId));
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            UpdateWorkersAccess();
        }

        public void RefillWorkerAccess()
        {
            UpdateWorkersAccess();
            FillWorkersAccess();
        }

        private void UpdateWorkersAccess()
        {
            try
            {
                _workersAccessAdapter.Update(WorkersAccessTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(exp.Message);
            }
        }

        #endregion


        #region ProgrammReport

        public void CorrectProblem(int reportId)
        {
            var problems = ReportTable.AsEnumerable().Where(r => r.Field<Int64>("ReportID") == reportId);
            if (!problems.Any()) return;

            var problem = problems.First();
            problem["IsCorrected"] = true;

            UpdateProgrammReport();
        }

        private void UpdateProgrammReport()
        {
            try
            {
                _reportAdapter.Update(_reportTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[ADMC0015] Попробуйте перезапустить приложение. " +
                    "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        public static string GetMainDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FA2");
        }
    }
}
