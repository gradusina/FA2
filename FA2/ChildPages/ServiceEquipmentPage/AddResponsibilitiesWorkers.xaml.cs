using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FA2.XamlFiles;

namespace FA2.ChildPages.ServiceEquipmentPage
{
    /// <summary>
    /// Логика взаимодействия для AddResponsibilitiesWorkers.xaml
    /// </summary>
    public partial class AddResponsibilitiesWorkers
    {
        private readonly int _serviceJournalId;
        private DataRow _serviceJournalRow;

        private ServiceEquipmentClass _sec;
        private StaffClass _sc;

        public AddResponsibilitiesWorkers(int serviceJournalId)
        {
            _serviceJournalId = serviceJournalId;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);
            App.BaseClass.GetStaffClass(ref _sc);

            _serviceJournalRow = null;
            var serviceRows = _sec.ServiceJournal.Table.AsEnumerable().
                Where(r => r.Field<Int64>("ServiceJournalID") == _serviceJournalId);
            if (serviceRows.Count() != 0)
                _serviceJournalRow = serviceRows.First();

            DataContext = _serviceJournalRow;

            SetBindings();
        }

        private void SetBindings()
        {
            FactoryList.ItemsSource = _sc.GetFactories();
            if (FactoryList.HasItems)
                FactoryList.SelectedIndex = 0;

            BindingResponsibilities();
        }

        private void BindingResponsibilities()
        {
            if (_serviceJournalRow != null)
            {
                var nextDate = Convert.ToDateTime(_serviceJournalRow["NextDate"]);
                var historyRows = _sec.ServiceHistory.Table.AsEnumerable().
                    Where(r => r.Field<DateTime>("NeededConfirmationDate") == nextDate &&
                        r.Field<Int64>("ServiceJournalID") == _serviceJournalId);

                var serviceHistoryId = -1;
                if (historyRows.Count() != 0)
                {
                    var historyRow = historyRows.First();
                    serviceHistoryId = Convert.ToInt32(historyRow["ServiceHistoryID"]);
                }

                ResponsibilitiesList.ItemsSource = _sec.ServiceResponsibilities.Table.AsEnumerable().
                    Where(r => Convert.ToInt32(r["ServiceHistoryID"]) == serviceHistoryId).AsDataView();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.HideCatalogGrid();
        }

        private void FactoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FactoryList.SelectedItem == null)
            {
                WorkersList.ItemsSource = null;
                return;
            }

            var factoryId = Convert.ToInt32(FactoryList.SelectedValue);
            WorkersList.ItemsSource = _sc.FilterWorkers(false, -1, true, factoryId, false, -1).AsDataView();
        }

        private void AddWorker_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersList.SelectedItems.Count == 0) return;
            if (_serviceJournalRow == null) return;

            // Find current history row
            var nextDate = Convert.ToDateTime(_serviceJournalRow["NextDate"]);
            var showNotification = Convert.ToBoolean(_serviceJournalRow["ShowNotification"]);
            var historyRows = _sec.ServiceHistory.Table.AsEnumerable().
                Where(r => r.Field<DateTime>("NeededConfirmationDate") == nextDate &&
                    r.Field<Int64>("ServiceJournalID") == _serviceJournalId);

            // Variable globalId for adding new task and adding new performers
            string globalId;

            if (!historyRows.Any())
            {
                // Responsibles are not set for operation
                var dateCreated = App.BaseClass.GetDateFromSqlServer();
                var mainWorkerId = AdministrationClass.CurrentWorkerId;
                var description = _serviceJournalRow["Description"].ToString();
                _sec.ServiceHistory.Add(dateCreated, mainWorkerId, _serviceJournalId, description, nextDate);

                // Adding new task
                globalId = historyRows.First()["GlobalID"].ToString();
                var actionName = _serviceJournalRow["ActionName"].ToString();
                var machineId = Convert.ToInt32(_serviceJournalRow["MachineID"]);
                var taskName = string.Format("{0} - {1}",
                    new IdToMachineNameConverter().Convert(machineId, typeof (string), null,
                        CultureInfo.InvariantCulture), actionName);
                Tasks.AddNewTask(globalId, taskName, mainWorkerId, dateCreated, description, TaskClass.SenderApplications.ServiceJournal, true, nextDate);
            }

            // Take current history row
            var historyRow = historyRows.First();
            var serviceHistoryId = Convert.ToInt32(historyRow["ServiceHistoryID"]);
            globalId = historyRow["GlobalID"].ToString();
            // Take id of the task
            var taskId = Tasks.GetTaskID(globalId);
            

            if (!ResponsibilitiesList.HasItems)
                ResponsibilitiesList.ItemsSource = _sec.ServiceResponsibilities.Table.AsEnumerable().
                    Where(r => Convert.ToInt32(r["ServiceHistoryID"]) == serviceHistoryId).AsDataView();

            foreach (DataRowView worker in WorkersList.SelectedItems)
            {
                var workerId = Convert.ToInt32(worker["WorkerID"]);

                if (_sec.ServiceResponsibilities.Table.AsEnumerable().
                    Any(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId &&
                    r.Field<Int64>("WorkerID") == workerId)) continue;

                // Adding responsible
                _sec.ServiceResponsibilities.Add(serviceHistoryId, workerId, showNotification);

                // Adding performer for task
                if (taskId != -1)
                {
                    Performers.AddPerformer(taskId, workerId);
                    NotificationManager.AddNotification(workerId, AdministrationClass.Modules.TasksPage, taskId);
                }
                    
            }
        }

        private void DeleteResponsible_Click(object sender, RoutedEventArgs e)
        {
            var deleteButton = (Button) sender;
            if (deleteButton.DataContext == null) return;

            var serviceResponsibilityRow = (DataRowView) deleteButton.DataContext;
            var serviceResponsibilityId = Convert.ToInt32(serviceResponsibilityRow["ServiceResponsibilityID"]);
            var serviceHistoryId = Convert.ToInt32(serviceResponsibilityRow["ServiceHistoryID"]);
            var workerId = Convert.ToInt32(serviceResponsibilityRow["WorkerID"]);

            var historyRows =
                _sec.ServiceHistory.Table.AsEnumerable()
                    .Where(h => Convert.ToInt32(h["ServiceHistoryID"]) == serviceHistoryId);
            if (historyRows.Any())
            {
                var historyRow = historyRows.First();
                var globalId = historyRow["GlobalID"].ToString();
                var taskId = Tasks.GetTaskID(globalId);

                _sec.ServiceResponsibilities.Delete(serviceResponsibilityId);
                Performers.DeletePerformer(taskId, workerId);

                if ( _sec.ServiceResponsibilities.Table.AsEnumerable()
                        .All(r => r.Field<Int64>("ServiceHistoryID") != serviceHistoryId))
                {
                    // Delete ServiceHistoryRow and Task, if responsibilities are empty
                    _sec.ServiceHistory.Delete(serviceHistoryId);
                    Tasks.DeleteTask(taskId);
                }
            }
        }

        private void SearchWorker_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WorkersList.ItemsSource == null) return;

            var searchText = SearchWorker.Text.Trim().ToLower();
            var filteredView = ((DataView)WorkersList.ItemsSource).Table.AsEnumerable().
                Where(r => r.Field<string>("Name").ToLower().Contains(searchText)).AsDataView();
            filteredView.Sort = "Name";
            WorkersList.ItemsSource = filteredView;
            if (WorkersList.HasItems)
                WorkersList.SelectedIndex = 0;


            //var filterString = string.Format("(Name LIKE  '{0}*')", searchText);
            //((DataView)WorkersList.ItemsSource).RowFilter = filterString;
            //if (WorkersList.HasItems)
            //    WorkersList.SelectedIndex = 0;
        }
    }
}
