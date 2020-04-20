using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using MessageBox = FAIIControlLibrary.MetroMessageBox;

namespace FA2.Classes
{
    public class PlannedScheduleClass
    {
        private string _connectionString = null;

        private MySqlDataAdapter PlannedScheduleDataAdapter;
        public DataTable PlannedScheduleDataTable;

        private MySqlDataAdapter WorkerPlannedScheduleDataAdapter;
        public DataTable WorkerPlannedScheduleDataTable;
        private MySqlConnection WorkerPlannedScheduleConnection;

        public DataTable tempPlannedScheduleDataTable;

        public BindingListCollectionView PlannedScheduleViewSource = null;
        private MySqlConnection PlannedScheduleConnection;

        public DateTime _currentDate;

        public PlannedScheduleClass(string tConnectionString)
        {
            _connectionString = tConnectionString;
            _currentDate = App.BaseClass.GetDateFromSqlServer();

            Initialize();
        }

        private void Initialize()
        {
            Create();
            Fill();
            Binding();
        }

        private void Create()
        {
            PlannedScheduleDataTable = new DataTable();

            WorkerPlannedScheduleDataTable = new DataTable();
        }

        private void Fill()
        {
            FillPlannedSchedule(_currentDate);
        }

        private void FillPlannedSchedule(DateTime ScheduleDate)
        {
            try
            {
                PlannedScheduleDataTable.DefaultView.RowFilter = "";
                if (PlannedScheduleViewSource != null) PlannedScheduleViewSource.CustomFilter = "";

                const string commandText = @"SELECT * FROM FAIIProdCalendar.PlannedSchedule 
                                             WHERE YEAR(ScheduleDate) = YEAR(@ScheduleDate) 
                                             AND MONTH(ScheduleDate) = MONTH(@ScheduleDate)";

                PlannedScheduleConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, PlannedScheduleConnection);

                command.Parameters.Add("@ScheduleDate", MySqlDbType.DateTime).Value = ScheduleDate;

                PlannedScheduleDataAdapter = new MySqlDataAdapter(command);
                new MySqlCommandBuilder(PlannedScheduleDataAdapter);

                PlannedScheduleDataAdapter.Fill(PlannedScheduleDataTable);

                tempPlannedScheduleDataTable = PlannedScheduleDataTable.Clone();
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [PS0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillPlannedScheduleForWorker(DateTime ScheduleDate, int workerID)
        {
            try
            {
                WorkerPlannedScheduleDataTable.Clear();
                WorkerPlannedScheduleDataTable.DefaultView.RowFilter = "";

                const string commandText = @"SELECT * FROM FAIIProdCalendar.PlannedSchedule 
                                             WHERE YEAR(ScheduleDate) = YEAR(@ScheduleDate) 
                                             AND MONTH(ScheduleDate) = MONTH(@ScheduleDate) AND WorkerID = @workerID";

                WorkerPlannedScheduleConnection = new MySqlConnection(_connectionString);

                var command = new MySqlCommand(commandText, WorkerPlannedScheduleConnection);

                command.Parameters.Add("@ScheduleDate", MySqlDbType.DateTime).Value = ScheduleDate;
                command.Parameters.Add("@workerID", MySqlDbType.Int64).Value = workerID;

                WorkerPlannedScheduleDataAdapter = new MySqlDataAdapter(command);

                WorkerPlannedScheduleDataAdapter.Fill(WorkerPlannedScheduleDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [PS0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Binding()
        {
            PlannedScheduleViewSource = new BindingListCollectionView(PlannedScheduleDataTable.DefaultView);
        }



        public void RefillPlannedSchedule(DateTime ScheduleDate)
        {
            PlannedScheduleDataTable.Clear();
            FillPlannedSchedule(ScheduleDate);
        }

        public void SavePlannedSchedule()
        {
            PlannedScheduleDataAdapter.Update(PlannedScheduleDataTable);
        }

        public void AddNewPlannedScheduleRow(DataRow newPlannedScheduleRow, int workerId, DateTime scheduleDate,
            int editingWorkerId, decimal totalHours, decimal totalNightHours,
            int totalWorkingDays)
        {
            DataRow[] daraRows = PlannedScheduleDataTable.Select("WorkerID =" + workerId);

            newPlannedScheduleRow["WorkerID"] = workerId;
            newPlannedScheduleRow["ScheduleDate"] = scheduleDate;

            newPlannedScheduleRow["EditingDate"] = App.BaseClass.GetDateFromSqlServer();
            newPlannedScheduleRow["EditingWorkerID"] = editingWorkerId;

            newPlannedScheduleRow["TotalHours"] = totalHours;
            newPlannedScheduleRow["TotalNightHours"] = totalNightHours;
            newPlannedScheduleRow["TotalWorkingDays"] = totalWorkingDays;

            if (daraRows.Length == 0)
                PlannedScheduleDataTable.Rows.Add(newPlannedScheduleRow);
            else
            {
                daraRows[0] = newPlannedScheduleRow;
            }

            SavePlannedSchedule();
            RefillPlannedSchedule(scheduleDate);
        }

        public DataRow GetPlannedScheduleRow(int workerId, DateTime scheduleDate)
        {
            FillPlannedScheduleForWorker(scheduleDate, workerId);

            return WorkerPlannedScheduleDataTable.Rows.Count != 0 ? WorkerPlannedScheduleDataTable.Rows[0] : null;
        }
    }
}
