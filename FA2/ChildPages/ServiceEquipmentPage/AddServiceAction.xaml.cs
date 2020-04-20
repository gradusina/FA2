using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ChildPages.ServiceEquipmentPage
{
    /// <summary>
    /// Логика взаимодействия для AddServiceAction.xaml
    /// </summary>
    public partial class AddServiceAction
    {
        private readonly int _serviceJournalId;
        private readonly int _factoryId;
        private readonly int _machineId;
        private readonly bool _editMode;
        private readonly bool _fullAccess;

        private ServiceEquipmentClass _sec;

        public AddServiceAction(int serviceJournalId, int factoryId, int machineId, bool fullAccess)
        {
            _serviceJournalId = serviceJournalId;
            _editMode = serviceJournalId != 0;

            _factoryId = factoryId;
            _machineId = machineId;
            _fullAccess = fullAccess;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);

            if (_sec.ServiceJournal.Table.AsEnumerable().
                Any(r => r.Field<Int64>("ServiceJournalID") == _serviceJournalId))
            {
                var contextRow = _sec.ServiceJournal.Table.AsEnumerable().
                    First(r => r.Field<Int64>("ServiceJournalID") == _serviceJournalId);
                DataContext = contextRow;
            }
            else
            {
                DataContext = null;
            }

            if (!_editMode)
                WorkerList.Height = new GridLength(0);

            if (!_fullAccess)
                Ok.Visibility = Visibility.Hidden;

            SetBindings();
        }

        private void SetBindings()
        {
            var measureView = _sec.TimeMeasures.Table.AsDataView();
            MeasureList.ItemsSource = measureView;
            NotificationMeasureList.ItemsSource = measureView;

            ActionName.DataSource = _sec.ServiceJournal.Table.AsDataView();
            ActionName.DisplayMemberPath = "ActionName";
            ActionName.SelectedValuePath = "ServiceJournalID";

            if (_editMode)
                BindingResponsibilities();
        }

        private void BindingResponsibilities()
        {
            if (_serviceJournalId != 0)
            {
                var journalRows = _sec.ServiceJournal.Table.AsEnumerable().
                    Where(r => r.Field<Int64>("ServiceJournalID") == _serviceJournalId);
                if(journalRows.Count() != 0)
                {
                    var serviceJournalRow = journalRows.First();
                    var nextDate = Convert.ToDateTime(serviceJournalRow["NextDate"]);
                    var historyRows = _sec.ServiceHistory.Table.AsEnumerable().
                        Where(r => r.Field<DateTime>("NeededConfirmationDate") == nextDate &&
                            r.Field<Int64>("ServiceJournalID") == _serviceJournalId);

                    int serviceHistoryId = -1;
                    if (historyRows.Count() != 0)
                    {
                        var historyRow = historyRows.First();
                        serviceHistoryId = Convert.ToInt32(historyRow["ServiceHistoryID"]);
                    }

                    ResponsibilitiesList.ItemsSource = _sec.ServiceResponsibilities.Table.AsEnumerable().
                        Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId).AsDataView();
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.HideCatalogGrid();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ActionName.Text) || LastTime.SelectedDate == null ||
                NextTime.SelectedDate == null || TimeInterval.Value == null || Convert.ToInt32(TimeInterval.Value) == 0 ||
                MeasureList.SelectedItem == null)
                return;

            if (Convert.ToBoolean(ShowNotification.IsChecked) &&
                (NotificationInterval.Value == null || Convert.ToInt32(NotificationInterval.Value) == 0 ||
                NotificationMeasureList.SelectedItem == null))
                return;

            if (_editMode)
                Update();
            else
                AddNew();

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var servEquipPage = mainWindow.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    servEquipPage.UpdateJournalView();
                }
            }

            Cancel_Click(null, null);
        }

        private void AddNew()
        {
            var actionName = ActionName.Text;
            var lastTime = LastTime.SelectedDate.Value;
            var nextTime = NextTime.SelectedDate.Value;
            var timeInterval = Convert.ToInt32(TimeInterval.Value);
            var measureId = Convert.ToInt32(MeasureList.SelectedValue);
            var description = Description.Text;
            var editingWorkerId = AdministrationClass.CurrentWorkerId;
            var editingDate = App.BaseClass.GetDateFromSqlServer();

            var showNotification = Convert.ToBoolean(ShowNotification.IsChecked);
            if(!showNotification)
            {
                _sec.ServiceJournal.Add(actionName, timeInterval, measureId, lastTime, nextTime,
                    description, _factoryId, _machineId, editingWorkerId, editingDate);
            }
            else
            {
                var notificationInterval = Convert.ToInt32(NotificationInterval.Value);
                var notificationMeasureId = Convert.ToInt32(NotificationMeasureList.SelectedValue);
                _sec.ServiceJournal.Add(actionName, timeInterval, measureId, lastTime, nextTime,
                    description, _factoryId, _machineId, editingWorkerId, editingDate,
                    notificationInterval, notificationMeasureId, true);
            }
        }

        private void Update()
        {
            var actionName = ActionName.Text;
            var lastTime = LastTime.SelectedDate.Value;
            var nextTime = NextTime.SelectedDate.Value;
            var timeInterval = Convert.ToInt32(TimeInterval.Value);
            var measureId = Convert.ToInt32(MeasureList.SelectedValue);
            var description = Description.Text;
            var editingWorkerId = AdministrationClass.CurrentWorkerId;
            var editingDate = App.BaseClass.GetDateFromSqlServer();

            var showNotification = Convert.ToBoolean(ShowNotification.IsChecked);
            if (!showNotification)
            {
                _sec.ServiceJournal.Change(_serviceJournalId, actionName, timeInterval, measureId, lastTime, nextTime,
                    description, _factoryId, _machineId, editingWorkerId, editingDate);
            }
            else
            {
                var notificationInterval = Convert.ToInt32(NotificationInterval.Value);
                var notificationMeasureId = Convert.ToInt32(NotificationMeasureList.SelectedValue);
                _sec.ServiceJournal.Change(_serviceJournalId, actionName, timeInterval, measureId, lastTime, nextTime,
                   description, _factoryId, _machineId, editingWorkerId, editingDate,
                   notificationInterval, notificationMeasureId, true);
            }

            var historyRows = _sec.ServiceHistory.Table.AsEnumerable().
                        Where(r => r.Field<Int64>("ServiceJournalID") == _serviceJournalId &&
                        !r.Field<bool>("IsClosing"));
            if(historyRows.Count() != 0)
            {
                var serviceHistoryId = Convert.ToInt32(historyRows.First()["ServiceHistoryID"]);
                _sec.ServiceHistory.ChangeRow(serviceHistoryId, description, nextTime);


                var rows = _sec.ServiceResponsibilities.Table.AsEnumerable().
                    Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);
                if(rows.Count() != 0)
                {
                    foreach(var responsibleRow in rows)
                    {
                        var serviceResponsibilityId = Convert.ToInt32(responsibleRow["ServiceResponsibilityID"]);
                        _sec.ServiceResponsibilities.Change(serviceResponsibilityId, showNotification, false);
                    }
                    _sec.ServiceResponsibilities.Update();
                }
            }
        }


        #region Counting next date

        private void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LastTime.SelectedDate == null || MeasureList.SelectedItem == null ||
                TimeInterval.Value == null || Convert.ToInt32(TimeInterval.Value) == 0)
            {
                NextTime.SelectedDate = null;
                return;
            }

            SetNextDate();
        }

        private void Interval_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (LastTime.SelectedDate == null || MeasureList.SelectedItem == null ||
                TimeInterval.Value == null || Convert.ToInt32(TimeInterval.Value) == 0)
            {
                NextTime.SelectedDate = null;
                return;
            }

            SetNextDate();
        }

        private void SetNextDate()
        {
            var lastTime = LastTime.SelectedDate.Value;
            var measureId = Convert.ToInt32(((DataRowView)MeasureList.SelectedItem).Row["TimeMeasureID"]);
            var timeInterval = Convert.ToInt32(TimeInterval.Value);

            DateTime nextTime = lastTime;
            try
            {
                nextTime = ServiceEquipmentClass.ServiceJournalClass.CalculateNextDate(lastTime, timeInterval, measureId);
            }
            catch(Exception exp)
            {
                FAIIControlLibrary.MetroMessageBox.Show(exp.Message);
            }

            NextTime.SelectedDate = nextTime;
        }

        #endregion
    }
}
