using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class StimulationClass
    {
        public DataTable StimTypesDataTable;
        private MySqlDataAdapter _stimTypesDataAdapter;

        public DataTable StimDataTable;
        public BindingListCollectionView StimView;
        private MySqlDataAdapter _stimDataAdapter;

        public DataTable StimUnitsDataTable;
        private MySqlDataAdapter _stimUnitsDataAdapter;

        public DataTable WorkersStimDataTable;
        private MySqlDataAdapter _workersStimDataAdapter;

        private MySqlConnection _workerStimConnection;

        private readonly string _connectionString;
        public StimulationClass(string connectionString)
        {
            _connectionString = connectionString;
            Initialize();
        }

        private void Initialize()
        {
            Create();
            FillTables();
            Binding();
        }

        private void Create()
        {
            StimTypesDataTable = new DataTable();
            StimDataTable = new DataTable();
            StimUnitsDataTable = new DataTable();
            WorkersStimDataTable = new DataTable();
        }

        private void FillTables()
        {
            FillStimTypes();
            FillStimulations();
            FillStimUnits();
            FillWorkersStim(new DateTime(), new DateTime());
        }

        private void FillStimTypes()
        {
            try
            {
                _stimTypesDataAdapter = new MySqlDataAdapter(@"SELECT StimulationTypeID, StimulationTypeName 
                                                               FROM FAIIStimulating.StimulationTypes",
                    _connectionString);
                new MySqlCommandBuilder(_stimTypesDataAdapter);
                _stimTypesDataAdapter.Fill(StimTypesDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStimulations()
        {
            try
            {
                _stimDataAdapter =
                    new MySqlDataAdapter(@"SELECT StimulationID, StimulationTypeID, StimulationName, 
                                           StimulationNotes, Available FROM FAIIStimulating.Stimulations",
                        _connectionString);
                new MySqlCommandBuilder(_stimDataAdapter);
                _stimDataAdapter.Fill(StimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0002] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStimUnits()
        {
            try
            {
                _stimUnitsDataAdapter = new MySqlDataAdapter(@"SELECT StimulationUnitID, StimulationUnitName 
                                                               FROM FAIIStimulating.StimulationUnits",
                    _connectionString);
                new MySqlCommandBuilder(_stimUnitsDataAdapter);
                _stimUnitsDataAdapter.Fill(StimUnitsDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0003] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillWorkersStim(DateTime dateFrom, DateTime dateTo)
        {
            var nullDate = new DateTime();
            const string timeSpanCommandText = @"SELECT WorkerStimID, WorkerID, Date, StimulationID, 
                                                 Warning, ClosedWarning, Notes, Available, StimulationUnitID, 
                                                 StimulationSize, EditingDate, EditingWorkerID 
                                                 FROM FAIIStimulating.WorkersStimulating 
                                                 WHERE Date >= @DateFrom AND Date < @DateTo";
            const string emptyCommandText = @"SELECT WorkerStimID, WorkerID, Date, StimulationID, 
                                              Warning, ClosedWarning, Notes, Available, StimulationUnitID, 
                                              StimulationSize, EditingDate, EditingWorkerID 
                                              FROM FAIIStimulating.WorkersStimulating LIMIT 0";

            try
            {
                if (dateFrom != nullDate && dateTo != nullDate)
                {
                    _workerStimConnection = new MySqlConnection(_connectionString);

                    var command = new MySqlCommand(timeSpanCommandText, _workerStimConnection);
                    command.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                    command.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo.AddDays(1);
                    _workersStimDataAdapter = new MySqlDataAdapter(command);
                }
                else
                {
                    _workersStimDataAdapter = new MySqlDataAdapter(emptyCommandText, _connectionString);
                }

                new MySqlCommandBuilder(_workersStimDataAdapter);
                WorkersStimDataTable.Clear();
                _workersStimDataAdapter.Fill(WorkersStimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Binding()
        {
            StimView = new BindingListCollectionView(StimDataTable.DefaultView) {CustomFilter = "Available = 'TRUE'"};
            if (StimView.GroupDescriptions != null)
                StimView.GroupDescriptions.Add(new PropertyGroupDescription("StimulationTypeID"));
            StimView.SortDescriptions.Add(new SortDescription("StimulationTypeID", ListSortDirection.Ascending));
            StimView.SortDescriptions.Add(new SortDescription("StimulationName", ListSortDirection.Ascending));
        }



        #region StimulationsCatalog

        public void AddStimulation(string stimName, int stimType, string stimNotes)
        {
            var newDr = StimDataTable.NewRow();
            newDr["StimulationName"] = stimName;
            newDr["StimulationTypeID"] = stimType;
            newDr["StimulationNotes"] = stimNotes;
            StimDataTable.Rows.Add(newDr);

            UpdateStimulations();
            RefillStimulations();
        }

        public void DeleteStimulation()
        {
            if (StimView.CurrentItem == null) return;
            var deletingRow = (DataRowView) StimView.CurrentItem;
            deletingRow["Available"] = false;
            StimView.CustomFilter = "Available = 'TRUE'";
            UpdateStimulations();
        }

        public void ChangeStimulation(string stimName, int stimType, string stimNotes)
        {
            if (StimView.CurrentItem == null) return;
            var changingRow = (DataRowView) StimView.CurrentItem;
            changingRow["StimulationName"] = stimName;
            changingRow["StimulationTypeID"] = stimType;
            changingRow["StimulationNotes"] = stimNotes;
            StimView.CustomFilter = "Available = 'TRUE'";
            UpdateStimulations();
        }

        private void RefillStimulations()
        {
            try
            {
                _stimDataAdapter.Update(StimDataTable);
                StimDataTable.Clear();
                _stimDataAdapter.Fill(StimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStimulations()
        {
            try
            {
                _stimDataAdapter.Update(StimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion



        public DataView WorkerStimView(int stimTypeId)
        {
            DataView view = null;
            if (WorkersStimDataTable != null)
            {
                var stimulationsByType =
                    (StimDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("StimulationTypeID") == stimTypeId));

                var workersStimByType =
                    (WorkersStimDataTable.AsEnumerable().Where(s => stimulationsByType.AsEnumerable().Any(
                        x => x.Field<Int64>("StimulationID") == s.Field<Int64>("StimulationID"))));

                if (workersStimByType.Count() != 0)
                {
                    var filteringTable = workersStimByType.CopyToDataTable();
                    view = filteringTable.AsDataView();
                }
            }

            return view;
        }


        public void AddWorkerStimWarning(int workerId, DateTime date, int stimulationId, string notes,
            DateTime editingDate, int editingWorkerId)
        {
            var newDr = WorkersStimDataTable.NewRow();
            newDr["WorkerID"] = workerId;
            newDr["Date"] = date;
            newDr["StimulationID"] = stimulationId;
            newDr["Warning"] = true;
            newDr["ClosedWarning"] = false;
            newDr["Notes"] = notes;
            newDr["Available"] = true;
            newDr["EditingDate"] = editingDate;
            newDr["EditingWorkerID"] = editingWorkerId;
            WorkersStimDataTable.Rows.Add(newDr);

            RefillWorkerStim();
        }

        public void AddWorkerStimNotWarning(int workerId, DateTime date, int stimulationId, string notes,
                                            int stimulationUnitId, double stimulationSize, DateTime editingDate,
                                            int editingWorkerId)
        {
            var newDr = WorkersStimDataTable.NewRow();
            newDr["WorkerID"] = workerId;
            newDr["Date"] = date;
            newDr["StimulationID"] = stimulationId;
            newDr["Warning"] = false;
            newDr["ClosedWarning"] = true;
            newDr["Notes"] = notes;
            newDr["Available"] = true;
            newDr["StimulationUnitID"] = stimulationUnitId;
            newDr["StimulationSize"] = stimulationSize;
            newDr["EditingDate"] = editingDate;
            newDr["EditingWorkerID"] = editingWorkerId;
            WorkersStimDataTable.Rows.Add(newDr);

            RefillWorkerStim();
        }

        public void ChangeWorkerStimWarning(int workerStimId, DateTime date, int stimulationId, string notes,
                                            DateTime editingDate, int editingWorkerId)
        {
            var dataRows = WorkersStimDataTable.Select(string.Format("WorkerStimID = {0}", workerStimId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows[0];
            dataRow["Date"] = date;
            dataRow["StimulationID"] = stimulationId;
            dataRow["Warning"] = true;
            dataRow["ClosedWarning"] = false;
            dataRow["Notes"] = notes;
            dataRow["StimulationUnitID"] = DBNull.Value;
            dataRow["StimulationSize"] = DBNull.Value;
            dataRow["EditingDate"] = editingDate;
            dataRow["EditingWorkerID"] = editingWorkerId;

            UpdateWorkerStim();
        }

        public void ChangeWorkerStimNotWarning(int workerStimId, DateTime date, int stimulationId, string notes,
                                               int stimulationUnitId, double stimulationSize, DateTime editingDate,
                                               int editingWorkerId)
        {
            var dataRows = WorkersStimDataTable.Select(string.Format("WorkerStimID = {0}", workerStimId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows[0];
            dataRow["Date"] = date;
            dataRow["StimulationID"] = stimulationId;
            dataRow["Warning"] = false;
            dataRow["ClosedWarning"] = true;
            dataRow["Notes"] = notes;
            dataRow["StimulationUnitID"] = stimulationUnitId;
            dataRow["StimulationSize"] = stimulationSize;
            dataRow["EditingDate"] = editingDate;
            dataRow["EditingWorkerID"] = editingWorkerId;

            UpdateWorkerStim();
        }

        public void DeleteWorkerStim(int workerStimId, int editingWorkerId, DateTime editingDate)
        {
            var dataRows = WorkersStimDataTable.Select(string.Format("WorkerStimID = {0}", workerStimId));
            if (dataRows.Length == 0) return;

            var deletingRow = dataRows[0];
            deletingRow["Available"] = false;
            deletingRow["EditingDate"] = editingDate;
            deletingRow["EditingWorkerID"] = editingWorkerId;

            UpdateWorkerStim();
        }

        private void RefillWorkerStim()
        {
            try
            {
                _workersStimDataAdapter.Update(WorkersStimDataTable);
                WorkersStimDataTable.Clear();
                _workersStimDataAdapter.Fill(WorkersStimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0007] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateWorkerStim()
        {
            try
            {
                _workersStimDataAdapter.Update(WorkersStimDataTable);
            }
            catch (Exception e)
            {
                MetroMessageBox.Show(
                    e.Message +
                    "\n\n[StC0008] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
