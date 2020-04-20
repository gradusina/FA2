using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FA2.ChildPages.ServiceEquipmentPage;
using FA2.ChildPages.TaskPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для ServiceEquipmentPage.xaml
    /// </summary>
    public partial class ServiceEquipmentPage
    {
        private const string ReceivedText = "\n\nЗаявка принята: {0} дата: {1}.";
        private bool _firstTimePageRun = true;
        private DateTime _currentTime;
        private readonly int _currentWorkerId;
        private ServiceEquipmentClass.RequestType _requestType = ServiceEquipmentClass.RequestType.Crash;
        private string _modeFilter;

        private bool _needToOpenPupup;
        private int _openingDuration;

        private System.Windows.Forms.Timer _timer;

        private ServiceEquipmentClass _sec;
        private CatalogClass _cc;
        private StaffClass _sc;
        private TaskClass _taskClass;


        private readonly bool _fullAccess;

        public ServiceEquipmentPage(bool fullAccess)
        {
            _currentWorkerId = AdministrationClass.CurrentWorkerId;
            _fullAccess = fullAccess;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.ServiceEquipment);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.ServiceEquipment);

            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) =>
                                           {
                                               _firstTimePageRun = false;
                                               _currentTime = App.BaseClass.GetDateFromSqlServer();
                                               FillData();
                                           };
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           BindingData();

                                                           JournalFactoryFilterEnable.IsChecked = true;

                                                           FilteringEnabled.IsChecked = true;
                                                           MachineFiltering.IsChecked = true;

                                                           if (_fullAccess)
                                                               ServiceHistorySwitch.IsChecked = true;
                                                           else
                                                               appBarCrashButton.IsChecked = true;

                                                           if (_fullAccess)
                                                           {
                                                               AddServiceRow.IsEnabled = true;
                                                               AddResponsibleWorker.IsEnabled = true;
                                                               DeleteServiceRow.IsEnabled = true;
                                                               ConfirmServiceAction.IsEnabled = true;
                                                               ExportServiceHistory.IsEnabled = true;
                                                           }

                                                           showClosedRequestCheckBox_Unchecked(null, null);
                                                           SetTimerProperties();
                                                           
                                                           var mainWindow = Application.Current.MainWindow as MainWindow;
                                                           if (mainWindow != null) mainWindow.HideWaitAnnimation();
                                                       };

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();

                RefillInfo();
            }
        }

        private void FillData()
        {
            var dateFrom = _currentTime.Subtract(new TimeSpan(60, 0, 0, 0));
            var dateTo = _currentTime;
            App.BaseClass.GetServiceEquipmentClass(ref _sec);
            _sec.Fill(dateFrom, dateTo);
            _sec.FillServiceHistory(dateFrom, dateTo);

            if (!_sec.Table.Columns.Contains("TimeOut"))
                _sec.Table.Columns.Add("TimeOut", typeof (TimeSpan));

            App.BaseClass.GetCatalogClass(ref _cc);
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetTaskClass(ref _taskClass);
        }

        #region Привязка к источникам данных

        private void BindingData()
        {
            BindingDatePickers();

            BindingMachinesComboBoxes();
            BindingFactoryComboBoxes();

            BindingDamageTypeComboBox();

            CrashMachinesListBox.ItemsSource = _sec.Table.DefaultView;
            _modeFilter = "RequestTypeID = 1";
            ((DataView) CrashMachinesListBox.ItemsSource).RowFilter = _modeFilter;

            if (CrashMachinesListBox.Items.Count != 0)
                CrashMachinesListBox.SelectedIndex = 0;
        }

        private void BindingDatePickers()
        {
            var dateFrom = _currentTime.Subtract(new TimeSpan(60, 0, 0, 0));
            var dateTo = _currentTime;

            dateFromPicker.SelectedDate = dateFrom;
            dateUntilPicker.SelectedDate = dateTo;

            ServiceHistoryDateFrom.SelectedDate = dateFrom;
            ServiceHistoryDateTo.SelectedDate = dateTo;
        }

        private void BindingDamageTypeComboBox()
        {
            damageTypeCombpBox.ItemsSource = _sec.DamageTypes.Table.DefaultView;
            damageTypeCombpBox.DisplayMemberPath = "DamageTypeName";
            damageTypeCombpBox.SelectedValuePath = "DamageTypeID";
            if (damageTypeCombpBox.Items.Count != 0)
                damageTypeCombpBox.SelectedIndex = 0;
        }

        private void BindingFactoryComboBoxes()
        {
            var factoriesView = _cc.GetFactories();

            FactoryComboBox.ItemsSource = factoriesView;

            ServiceHistoryWorkerFactoryList.ItemsSource = factoriesView;
            if (ServiceHistoryWorkerFactoryList.HasItems)
                ServiceHistoryWorkerFactoryList.SelectedIndex = 0;

            ServiceHistoryMachineFactoryList.ItemsSource = factoriesView;
            if (ServiceHistoryMachineFactoryList.HasItems)
                ServiceHistoryMachineFactoryList.SelectedIndex = 0;

            RequestFactoryFilter.ItemsSource = factoriesView;
        }

        private void BindingMachinesComboBoxes()
        {
            var machinesView = GetMachines(0);

            MachinesListBox.ItemsSource = machinesView;

            ServiceHistoryMachineList.ItemsSource = machinesView;
            if (ServiceHistoryMachineList.HasItems)
                ServiceHistoryMachineList.SelectedIndex = 0;
        }

        #endregion


        #region TimerClock

        private void SetTimeOut()
        {
            if (CrashMachinesListBox.Items.Count == 0) return;
            foreach (
                var drv in
                    CrashMachinesListBox.ItemsSource.Cast<DataRowView>().Where(drv => drv["LaunchDate"] == DBNull.Value)
                )
            {
                DateTime startTime;
                var succes = DateTime.TryParse(drv["RequestDate"].ToString(), out startTime);
                if (!succes) return;

                var span = _currentTime.Subtract(startTime);
                drv["TimeOut"] = span;
            }
        }

        private void FillTimeOutColumn()
        {
            if (CrashMachinesListBox.Items.Count == 0) return;
            foreach (
                var drv in
                    CrashMachinesListBox.Items.Cast<DataRowView>().Where(drv => drv.Row["LaunchDate"] != DBNull.Value))
            {
                DateTime startTime;
                var succes = DateTime.TryParse(drv["RequestDate"].ToString(), out startTime);
                if (!succes) return;
                DateTime stopTime;
                succes = DateTime.TryParse(drv["LaunchDate"].ToString(), out stopTime);
                if (!succes) return;

                var span = stopTime.Subtract(startTime);
                drv["TimeOut"] = span;
            }
        }

        private void SetTimerProperties()
        {
            _timer = new System.Windows.Forms.Timer {Interval = 1000};
            _timer.Tick += TimerTick;
            if (!_timer.Enabled)
                _timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));

            if (appBarCrashButton.IsChecked.HasValue && appBarCrashButton.IsChecked.Value)
            {
                SetTimeOut();
                FillTimeOutColumn();
            }

            OpenPopupForTime(5);
        }

        private void OpenPopupForTime(int duration)
        {
            if (_needToOpenPupup)
                _openingDuration++;

            if (_openingDuration == 3)
                requestClosedPopup.StaysOpen = false;

            if (_openingDuration == duration)
            {
                requestClosedPopup.IsOpen = false;
                _needToOpenPupup = false;
                _openingDuration = 0;
            }
        }

        #endregion


        #region Добавление новой заявки

        private void addRequestButton_Click(object sender, RoutedEventArgs e)
        {
            _currentTime = App.BaseClass.GetDateFromSqlServer();

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var addServEquip = new AddServiceEquipment(_requestType);
                mainWindow.ShowCatalogGrid(addServEquip, "Добавить заявку");
            }
        }

        public void SelectNewTableRow(int crashId)
        {
            var dt = ((DataView) CrashMachinesListBox.ItemsSource).ToTable();
            var dr = dt.Select("CrashMachineID = " + crashId);
            if (dr.Length != 0)
            {
                var dataRow = dr[0];
                var rowNumber = dt.Rows.IndexOf(dataRow);
                var drv = ((DataView) CrashMachinesListBox.ItemsSource)[rowNumber];
                if (drv != null)
                    CrashMachinesListBox.SelectedItem = drv;
            }
            else
            {
                if (CrashMachinesListBox.Items.Count != 0)
                    CrashMachinesListBox.SelectedIndex = 0;
            }
        }

        #endregion


        #region Выбор строк в таблице

        private void OnCrashMachinesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drv = CrashMachinesListBox.SelectedItem as DataRowView;

            MainRequestInfoGrid.DataContext = drv;

            if (drv != null)
            {
                var globalId = drv["GlobalID"].ToString();
                _taskClass.Fill(globalId);
            }

            SetAddInfoBorderEnabled(drv);
            SetButtonVisibility(drv);
            SetNotesEditVisibility(drv);
        }

        private void SetButtonVisibility(DataRowView drv)
        {
            ReceiveButton.Width = 0;
            //CompletButton.Width = 0;
            launchButton.Width = 0;
            infoButton.Width = 0;

            if (drv != null)
            {
                if (drv.Row["ReceivedDate"] == DBNull.Value)
                    ReceiveButton.Width = 170;
                else if (drv.Row["CompletionDate"] == DBNull.Value)
                    return;
                    //CompletButton.Width = 170;
                else if (drv.Row["LaunchDate"] == DBNull.Value)
                    launchButton.Width = 170;
                else if (drv.Row["LaunchDate"] != DBNull.Value)
                    infoButton.Width = 170;
            }
        }

        private void SetAddInfoBorderEnabled(DataRowView drv)
        {
            if (drv == null || drv.Row["LaunchDate"] != DBNull.Value)
            {
                saveChangesButton.IsEnabled = false;
                CrashReasonTextBox.IsReadOnly = true;
                PlannedLaunchDatePicker.IsEnabled = false;
                TimePlannedControl.IsEnabled = false;
                CompletionPercentControl.IsEnabled = false;
                damageTypeCombpBox.IsEnabled = false;
            }
            else if (drv.Row["launchDate"] == DBNull.Value)
            {
                saveChangesButton.IsEnabled = true;
                CrashReasonTextBox.IsReadOnly = false;
                PlannedLaunchDatePicker.IsEnabled = true;
                TimePlannedControl.IsEnabled = true;
                CompletionPercentControl.IsEnabled = true;
                damageTypeCombpBox.IsEnabled = true;
            }
        }

        private void SetNotesEditVisibility(DataRowView drv)
        {
            SetRecieveNotesVisibility(drv);
            SetCompletionNotesVisibility(drv);
        }

        private void SetRecieveNotesVisibility(DataRowView drv)
        {
            ReceivedNotesTextBlock.Visibility = Visibility.Visible;
            ChangeReceiveNotesTextBox.Visibility = Visibility.Collapsed;

            EditReceiveNotesPanel.Visibility = Visibility.Collapsed;
            EditReceiveNotesButton.Visibility = Visibility.Collapsed;

            if (drv != null && drv["ReceivedDate"] != DBNull.Value && !Convert.ToBoolean(drv["RequestClose"]))
            {
                if ((_fullAccess || Convert.ToInt32(drv["ReceivedWorkerID"]) == AdministrationClass.CurrentWorkerId))
                {
                    EditReceiveNotesButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetCompletionNotesVisibility(DataRowView drv)
        {
            CompletionNotesTextBlock.Visibility = Visibility.Visible;
            ChangeCompletionNotesTextBox.Visibility = Visibility.Collapsed;

            EditCompletionNotesPanel.Visibility = Visibility.Collapsed;
            EditCompletionNotesButton.Visibility = Visibility.Collapsed;

            if (drv != null && drv["CompletionDate"] != DBNull.Value && !Convert.ToBoolean(drv["RequestClose"]))
            {
                if ((_fullAccess || Convert.ToInt32(drv["ReceivedWorkerID"]) == AdministrationClass.CurrentWorkerId
                     || Convert.ToInt32(drv["CompletionWorkerID"]) == AdministrationClass.CurrentWorkerId))
                {
                    EditCompletionNotesButton.Visibility = Visibility.Visible;
                }
            }
        }

        public void RefillInfo()
        {
            if (CrashMachinesListBox.SelectedItem != null)
            {
                OnCrashMachinesListBoxSelectionChanged(null, null);
            }
        }

        #endregion


        private void showButton_Click(object sender, RoutedEventArgs e)
        {
            if (dateFromPicker.SelectedDate == null || dateUntilPicker.SelectedDate == null) return;

            _sec.Fill(dateFromPicker.SelectedDate.Value, dateUntilPicker.SelectedDate.Value);
        }

        private void showClosedRequestCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _sec.Table.DefaultView.RowFilter = _modeFilter;

            if (RequestMachineFilter.SelectedItem != null)
            {
                var machineId = Convert.ToInt32(RequestMachineFilter.SelectedValue);
                _sec.Table.DefaultView.RowFilter += string.Format(" AND WorkSubSectionID = {0}", machineId);
            }
        }

        private void showClosedRequestCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CrashMachinesListBox.ItemsSource == null) return;

            ((DataView) CrashMachinesListBox.ItemsSource).RowFilter += " AND RequestClose = 'False'";

            if (RequestMachineFilter.SelectedItem != null)
            {
                var machineId = Convert.ToInt32(RequestMachineFilter.SelectedValue);
                ((DataView) CrashMachinesListBox.ItemsSource).RowFilter += string.Format(" AND WorkSubSectionID = {0}",
                    machineId);
            }
        }


        #region Формирование бланка

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            if (CrashMachinesListBox.SelectedItem == null) return;

            var rowView = (DataRowView) CrashMachinesListBox.SelectedItem;
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var servEquipInfo = new ServiceEquipmentInfo(rowView, _currentWorkerId);
                mainWindow.ShowCatalogGrid(servEquipInfo, "Информация");
            }
        }

        public void OpenPopup(object globalId)
        {
            _needToOpenPupup = true;
            requestClosedPopup.IsOpen = true;
            requestClosedPopup.StaysOpen = true;
            requestClosedLabel.Content = globalId;
        }

        #endregion

        private void saveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (damageTypeCombpBox.SelectedValue == null && string.IsNullOrEmpty(CrashReasonTextBox.Text) &&
                PlannedLaunchDatePicker.SelectedDate == null && CompletionPercentControl.Percent < 0.001 ||
                CrashMachinesListBox.SelectedItem == null) return;

            int crashMachineId = Convert.ToInt32(((DataRowView) CrashMachinesListBox.SelectedItem).Row["CrashMachineID"]);
            object damageType = DBNull.Value;
            if (damageTypeCombpBox.SelectedValue != null)
                damageType = damageTypeCombpBox.SelectedValue;
            object crashReason = DBNull.Value;
            if (!string.IsNullOrEmpty(CrashReasonTextBox.Text))
                crashReason = CrashReasonTextBox.Text;
            object plannedLaunchDate = DBNull.Value;
            if (PlannedLaunchDatePicker.SelectedDate != null)
                plannedLaunchDate = PlannedLaunchDatePicker.SelectedDate.Value.Add(TimePlannedControl.TotalTime);
            object completionPercent = CompletionPercentControl.Percent;
            _sec.FillAdditionalInfo(crashMachineId, damageType, crashReason, plannedLaunchDate, completionPercent,
                App.BaseClass.GetDateFromSqlServer(),
                _currentWorkerId);
            AdministrationClass.AddNewAction(12);

            if (CrashMachinesListBox.SelectedItem == null) return;
        }


        #region Переход к другому режиму

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }

        private void appBarCrashButton_Checked(object sender, RoutedEventArgs e)
        {
            Requests.IsSelected = true;
            _requestType = ServiceEquipmentClass.RequestType.Crash;

            SetCrashMode();
            CloseAppBar();

            if (showClosedRequestCheckBox.IsChecked == true)
                showClosedRequestCheckBox.IsChecked = false;
            else
                showClosedRequestCheckBox_Unchecked(null, null);

            RequestMachineFilterEnable.IsChecked = false;
        }

        private void appBarNonCrashButton_Checked(object sender, RoutedEventArgs e)
        {
            Requests.IsSelected = true;
            _requestType = ServiceEquipmentClass.RequestType.Truble;

            SetNonCrashMode();
            CloseAppBar();

            if (showClosedRequestCheckBox.IsChecked == true)
                showClosedRequestCheckBox.IsChecked = false;
            else
                showClosedRequestCheckBox_Unchecked(null, null);

            RequestMachineFilterEnable.IsChecked = false;
        }

        private void SetCrashMode()
        {
            _modeFilter = "RequestTypeID = 1";
            ((DataView) CrashMachinesListBox.ItemsSource).RowFilter = _modeFilter;

            if (CrashMachinesListBox.Items.Count != 0)
                CrashMachinesListBox.SelectedIndex = 0;
        }

        private void SetNonCrashMode()
        {
            _modeFilter = "RequestTypeID = 2";
            ((DataView) CrashMachinesListBox.ItemsSource).RowFilter = _modeFilter;

            if (CrashMachinesListBox.Items.Count != 0)
                CrashMachinesListBox.SelectedIndex = 0;
        }

        #endregion


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CrashMachinesListBox.SelectedItem == null || CrashMachinesListBox.Items.Count == 0) return;

            var drv = (DataRowView) CrashMachinesListBox.SelectedItem;
            if (drv != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить запись?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    int crashMachineId = Convert.ToInt32(drv.Row["CrashMachineID"]);
                    var requestDate = Convert.ToDateTime(drv["RequestDate"]);
                    var requestWorkerId = Convert.ToInt32(drv["RequestWorkerID"]);
                    var globalId = drv["GlobalID"].ToString();
                    

                    _sec.DeleteCrashRow(crashMachineId);
                    AdministrationClass.AddNewAction(13);

                    var rows = _taskClass.Tasks.Table.AsEnumerable().Where(t => t.Field<string>("GlobalID") == globalId);
                    if (rows.Any())
                    {
                        var task = rows.First();
                        var taskId = Convert.ToInt32(task["TaskID"]);
                        _taskClass.DeleteTask(taskId);
                    }
                    NewsHelper.DeleteNews(requestDate, requestWorkerId);
                }
            }
        }

        private void RowMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (CrashMachinesListBox.SelectedItem == null || CrashMachinesListBox.Items.Count == 0) return;

            var drv = (DataRowView) CrashMachinesListBox.SelectedItem;
            if (Convert.ToBoolean(drv.Row["RequestClose"]))
            {
                e.Handled = true;
            }
        }

        private void FactoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoryComboBox.SelectedItem == null)
            {
                MachinesListBox.ItemsSource = _cc.MachinesDataTable.AsDataView();
                if (MachinesListBox.ItemsSource != null)
                    ((DataView) MachinesListBox.ItemsSource).Sort = "MachineName";
            }
            else
            {
                var factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
                MachinesListBox.ItemsSource = GetMachines(factoryId);
                if (MachinesListBox.HasItems && !JournalMachineFilterEnable.IsChecked.Value)
                    MachinesListBox.SelectedIndex = 0;
            }

            OnJournalFilteringChanged();
        }

        private DataView GetMachines(int factoryId)
        {
            var table = _cc.MachinesDataTable.AsEnumerable().Where(m => (m.Field<object>("IsVisible") != null &&
                                                                         m.Field<bool>("IsVisible")) &&
                                                                        m.Field<Int64>("FactoryID") == factoryId);
            if (!table.Any()) return null;

            var view = table.CopyToDataTable().AsDataView();
            view.Sort = "MachineName";
            return view;
        }

        private void AddServiceRow_Click(object sender, RoutedEventArgs e)
        {
            if (FactoryComboBox.SelectedItem == null || MachinesListBox.SelectedItem == null) return;

            var factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            var machineId = Convert.ToInt32(MachinesListBox.SelectedValue);

            var addServiceAction = new AddServiceAction(0, factoryId, machineId, _fullAccess);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.ShowCatalogGrid(addServiceAction, "Новая операция");
        }

        private void MachinesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnJournalFilteringChanged();
        }

        private void OnJournalFilteringChanged()
        {
            UpdateJournalView();
        }

        public void UpdateJournalView()
        {
            var factoryFilter = JournalFactoryFilterEnable.IsChecked.Value;
            int factoryId = -1;
            if (factoryFilter && FactoryComboBox.SelectedItem != null)
                factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            var showFree = JournalFreeActionsFilterEnable.IsChecked.Value;
            var showOverdue = JournalOverdueActionsFilterEnable.IsChecked.Value;
            var showAll = JournalMachineFilterEnable.IsChecked.Value;
            int machineId = -1;
            if (!showAll && MachinesListBox.SelectedItem != null)
                machineId = Convert.ToInt32(MachinesListBox.SelectedValue);

            ServiceJournalDataGrid.ItemsSource =
                FilterJournalData(factoryFilter, factoryId, showFree, showOverdue, showAll, machineId);
        }


        private void OnServiceJournalRowMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!_fullAccess) return;

            if (ServiceJournalDataGrid.SelectedItem == null) return;

            var factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            var machineId = Convert.ToInt32(MachinesListBox.SelectedValue);

            var serviceJournalId = Convert.ToInt32(ServiceJournalDataGrid.SelectedValue);
            var addServiceAction = new AddServiceAction(serviceJournalId, factoryId, machineId, _fullAccess);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.ShowCatalogGrid(addServiceAction, "Информация по операции");
        }

        private void AddResponsibleWorker_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceJournalDataGrid.SelectedItem == null) return;

            var serviceJournalId = Convert.ToInt32(ServiceJournalDataGrid.SelectedValue);
            var addResponsibilities = new AddResponsibilitiesWorkers(serviceJournalId);

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.ShowCatalogGrid(addResponsibilities, "Назначить ответсвенных");
        }

        private void ServiceHistoryMachineFactoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceHistoryMachineFactoryList.SelectedItem == null)
            {
                ServiceHistoryMachineList.ItemsSource = null;
            }
            else
            {
                var factoryId = Convert.ToInt32(ServiceHistoryMachineFactoryList.SelectedValue);
                ServiceHistoryMachineList.ItemsSource = GetMachines(factoryId);
                if (ServiceHistoryMachineList.HasItems)
                    ServiceHistoryMachineList.SelectedIndex = 0;
            }
        }

        private void ServiceHistoryMachineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Convert.ToBoolean(MachineFiltering.IsChecked)) return;

            if (ServiceHistoryMachineList.SelectedItem == null || ServiceHistoryMachineFactoryList.SelectedItem == null)
            {
                ServiceHistoryList.ItemsSource = null;
                return;
            }

            var machineId = Convert.ToInt32(ServiceHistoryMachineList.SelectedValue);
            var factoryId = Convert.ToInt32(ServiceHistoryMachineFactoryList.SelectedValue);

            ServiceHistoryList.ItemsSource = GetServiceHistoryView(factoryId, machineId);
            if (ServiceHistoryList.HasItems)
                ServiceHistoryList.SelectedIndex = 0;
        }

        private void ServiceHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceHistoryList.SelectedItem == null)
            {
                ResponsibilitiesList.ItemsSource = null;
                return;
            }

            var serviceHistoryId = Convert.ToInt32(ServiceHistoryList.SelectedValue);
            ResponsibilitiesList.ItemsSource = GetResponsibilitiesView(serviceHistoryId);
        }

        private DataView GetServiceHistoryView(int factoryId, int machineId)
        {
            var serviceJournal = _sec.ServiceJournal.Table.AsEnumerable().
                Where(r => r.Field<Int64>("FactoryID") == factoryId && r.Field<Int64>("MachineID") == machineId);

            var serviceHistory = _sec.ServiceHistory.Table.AsEnumerable().
                Where(
                    h =>
                        serviceJournal.Any(j => j.Field<Int64>("ServiceJournalID") == h.Field<Int64>("ServiceJournalID")));

            return serviceHistory.AsDataView();
        }

        private DataView GetResponsibilitiesView(int serviceHistoryId)
        {
            var responsibilities = _sec.ServiceResponsibilities.Table.AsEnumerable().
                Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId);

            return responsibilities.AsDataView();
        }

        private DataView GetServiceHistoryView(int workerId)
        {
            var responsibilitiesForWorker = _sec.ServiceResponsibilities.Table.AsEnumerable().
                Where(r => r.Field<Int64>("WorkerID") == workerId);

            var serviceHistoryForWorker = _sec.ServiceHistory.Table.AsEnumerable().
                Where(r => responsibilitiesForWorker.
                    Any(rW => rW.Field<Int64>("ServiceHistoryID") == r.Field<Int64>("ServiceHistoryID")));

            return serviceHistoryForWorker.AsDataView();
        }

        private void ServiceHistoryWorkerFactoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceHistoryWorkerFactoryList.SelectedItem == null)
            {
                ServiceHistoryWorkersList.ItemsSource = null;
            }
            else
            {
                var factoryId = Convert.ToInt32(ServiceHistoryWorkerFactoryList.SelectedValue);
                ServiceHistoryWorkersList.ItemsSource =
                    _sc.FilterWorkers(false, -1, true, factoryId, false, -1).AsDataView();
                if (ServiceHistoryWorkersList.HasItems)
                    ServiceHistoryWorkersList.SelectedIndex = 0;
            }
        }

        private void ServiceHistoryWorkersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Convert.ToBoolean(WorkerFiltering.IsChecked)) return;

            if (ServiceHistoryWorkersList.SelectedItem == null || ServiceHistoryWorkerFactoryList.SelectedItem == null)
            {
                ServiceHistoryList.ItemsSource = null;
                return;
            }

            var workerId = Convert.ToInt32(ServiceHistoryWorkersList.SelectedValue);

            ServiceHistoryList.ItemsSource = GetServiceHistoryView(workerId);
            if (ServiceHistoryList.HasItems)
                ServiceHistoryList.SelectedIndex = 0;
        }

        private void MachineFiltering_Checked(object sender, RoutedEventArgs e)
        {
            ServiceHistoryMachineFactoryList_SelectionChanged(null, null);
        }

        private void WorkerFiltering_Checked(object sender, RoutedEventArgs e)
        {
            ServiceHistoryWorkerFactoryList_SelectionChanged(null, null);
        }

        private void FillServiceHistory_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceHistoryDateFrom.SelectedDate == null || ServiceHistoryDateTo.SelectedDate == null) return;

            var dateFrom = ServiceHistoryDateFrom.SelectedDate.Value;
            var dateTo = ServiceHistoryDateTo.SelectedDate.Value.AddDays(1);

            _sec.ServiceHistory.Fill(dateFrom, dateTo);
        }

        private void Switch_Checked(object sender, RoutedEventArgs e)
        {
            if (ServiceHistorySwitch.IsChecked.Value)
                ServiceHistory.IsSelected = true;
            else if (ServiceJournalSwitch.IsChecked.Value)
                ServiceJournal.IsSelected = true;

            CloseAppBar();
        }

        private void SearchMachineTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MachinesListBox.ItemsSource == null) return;

            MachinesListBox.SelectionChanged -= MachinesListBox_SelectionChanged;

            var searchText = SearchMachineTextBox.Text.Trim().ToLower();
            var filteredView = ((DataView) MachinesListBox.ItemsSource).Table.AsEnumerable().
                Where(r => r.Field<string>("MachineName").ToLower().Contains(searchText)).AsDataView();
            filteredView.Sort = "MachineName";
            MachinesListBox.ItemsSource = filteredView;
            if (MachinesListBox.HasItems)
                MachinesListBox.SelectedIndex = 0;

            MachinesListBox.SelectionChanged += MachinesListBox_SelectionChanged;
            MachinesListBox_SelectionChanged(null, null);
        }

        private void DeleteServiceRow_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceJournalDataGrid.SelectedItem == null) return;
            if (MessageBox.Show("Вы действительно хотите удалить операцию?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var serviceJournalId = Convert.ToInt32(ServiceJournalDataGrid.SelectedValue);
            var historyRows = _sec.ServiceHistory.Table.AsEnumerable().
                Where(r => !r.Field<bool>("IsClosing") && r.Field<Int64>("ServiceJournalID") == serviceJournalId);
            if (historyRows.Count() != 0)
            {
                var messageResult = MessageBox.Show("В данный момент операция находится на этапе исполнения." +
                                                    " При удалении будут утрачены все действующие работы, " +
                                                    "выполняемые работниками по данной операции.",
                    "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageResult != MessageBoxResult.Yes) return;

                var serviceHistory = historyRows.First();
                var serviceHistoryId = Convert.ToInt32(serviceHistory["ServiceHistoryID"]);

                //Delete every responsibilities for history row
                foreach (var responsibleRow in _sec.ServiceResponsibilities.Table.AsEnumerable().
                    Where(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId))
                {
                    responsibleRow.Delete();
                }
                _sec.ServiceResponsibilities.Update();

                //Delete service history row
                _sec.ServiceHistory.Delete(serviceHistoryId);
            }

            //Delete service journal row
            _sec.ServiceJournal.Delete(serviceJournalId);

            UpdateJournalView();
        }

        private void ConfirmServiceAction_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceHistoryList.SelectedItem == null) return;

            var serviceHistoryRow = ServiceHistoryList.SelectedItem as DataRowView;
            if (serviceHistoryRow != null)
            {
                var isClosed = Convert.ToBoolean(serviceHistoryRow["IsClosing"]);
                if(isClosed) return;
            }

            var serviceHistoryId = Convert.ToInt32(ServiceHistoryList.SelectedValue);
            if (
                _sec.ServiceResponsibilities.Table.AsEnumerable()
                    .Any(r => r.Field<Int64>("ServiceHistoryID") == serviceHistoryId &&
                              !r.Field<bool>("IsClosing")))
            {
                MessageBox.Show("Операция не завершена работником(ами)",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var serviceJournalId = Convert.ToInt32(((DataRowView) ServiceHistoryList.SelectedItem)["ServiceJournalID"]);
            _sec.ServiceHistory.Confirm(serviceHistoryId, currentDate, currentWorkerId);
            _sec.ServiceJournal.ChangeLastDate(serviceJournalId, currentDate);
        }

        private void RequestFactoryFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RequestFactoryFilter.SelectedItem == null)
            {
                RequestMachineFilter.ItemsSource = null;
                return;
            }

            var factoryId = Convert.ToInt32(RequestFactoryFilter.SelectedValue);
            RequestMachineFilter.ItemsSource = GetMachines(factoryId);
            if (RequestMachineFilter.HasItems)
                RequestMachineFilter.SelectedIndex = 0;
        }

        private void RequestMachineFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CrashMachinesListBox.ItemsSource != null)
            {
                ((DataView) CrashMachinesListBox.ItemsSource).RowFilter = _modeFilter;

                if (RequestMachineFilter.SelectedItem != null)
                {
                    var machineId = Convert.ToInt32(RequestMachineFilter.SelectedValue);
                    ((DataView) CrashMachinesListBox.ItemsSource).RowFilter +=
                        string.Format(" AND WorkSubSectionID = {0}", machineId);
                }

                if (!showClosedRequestCheckBox.IsChecked.Value)
                    ((DataView) CrashMachinesListBox.ItemsSource).RowFilter += " AND RequestClose = 'False'";
            }
        }

        private void RequestMachineFilterEnable_OnChecked(object sender, RoutedEventArgs e)
        {
            if (RequestFactoryFilter.HasItems)
                RequestFactoryFilter.SelectedIndex = 0;
        }

        private void RequestMachineFilterEnable_OnUnchecked(object sender, RoutedEventArgs e)
        {
            RequestFactoryFilter.SelectedIndex = -1;
        }

        private void FilteringEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (MachineFiltering.IsChecked.Value)
                MachineFiltering_Checked(null, null);
            else if (WorkerFiltering.IsChecked.Value)
                WorkerFiltering_Checked(null, null);
        }

        private void FilteringEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            ServiceHistoryList.ItemsSource = _sec.ServiceHistory.Table.AsDataView();
        }

        private DataView FilterJournalData(bool factoryFilter, int factoryId, bool showFree, bool showOverdue,
            bool showAll, int machineId)
        {
            var filteringView = _sec.ServiceJournal.Table.AsEnumerable().
                Where(r => r.Field<bool>("IsEnabled"));

            if (factoryFilter)
                filteringView = filteringView.Where(r => r.Field<Int64>("FactoryID") == factoryId);
            if (showOverdue)
            {
                filteringView = filteringView.Where(r => r.Field<object>("IsOverdue") != DBNull.Value &&
                                                         r.Field<bool>("IsOverdue"));
            }
            if (!showAll)
                filteringView = filteringView.Where(r => r.Field<Int64>("MachineID") == machineId);
            if (showFree)
            {
                filteringView = filteringView.Where(r =>
                    !_sec.ServiceHistory.Table.AsEnumerable().
                        Any(h => h.Field<DateTime>("NeededConfirmationDate") == r.Field<DateTime>("NextDate") &&
                                 h.Field<Int64>("ServiceJournalID") == r.Field<Int64>("ServiceJournalID")));
            }

            return filteringView.Count() != 0
                ? filteringView.CopyToDataTable().AsDataView()
                : null;
        }

        private void JournalFactoryFilterEnable_Checked(object sender, RoutedEventArgs e)
        {
            if (FactoryComboBox.HasItems)
                FactoryComboBox.SelectedIndex = 0;

            OnJournalFilteringChanged();
        }

        private void JournalFactoryFilterEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            FactoryComboBox.SelectedIndex = -1;

            OnJournalFilteringChanged();
        }

        private void JournalMachineFilterEnable_Checked(object sender, RoutedEventArgs e)
        {
            SearchMachineTextBox.TextChanged -= SearchMachineTextBox_TextChanged;
            SearchMachineTextBox.Text = string.Empty;
            SearchMachineTextBox.TextChanged += SearchMachineTextBox_TextChanged;
            SearchMachineTextBox_TextChanged(null, null);

            JournalMachineFilteringPanel.IsEnabled = false;
            MachinesListBox.SelectedIndex = -1;

            OnJournalFilteringChanged();
        }

        private void JournalMachineFilterEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchMachineTextBox.TextChanged -= SearchMachineTextBox_TextChanged;
            SearchMachineTextBox.Text = string.Empty;
            SearchMachineTextBox.TextChanged += SearchMachineTextBox_TextChanged;
            SearchMachineTextBox_TextChanged(null, null);

            JournalMachineFilteringPanel.IsEnabled = true;
            if (MachinesListBox.HasItems)
                MachinesListBox.SelectedIndex = 0;

            OnJournalFilteringChanged();
        }

        private void JournalFilterCheckStatesChanged(object sender, RoutedEventArgs e)
        {
            OnJournalFilteringChanged();
        }



        private void OnReceiveButtonClick(object sender, RoutedEventArgs e)
        {
            var request = CrashMachinesListBox.SelectedItem as DataRowView;
            if (request == null) return;

            var globalId = request["GlobalID"].ToString();
            var taskName = new IdToWorkSubSectionConverter().Convert(request["WorkSubSectionID"], typeof (string), null,
                CultureInfo.InvariantCulture).ToString();
            var taskDescription = request["RequestNotes"].ToString();
            const TaskClass.SenderApplications senderApplication = TaskClass.SenderApplications.ServiceDamage;

            var addNewTaskWindow = new AddNewTask(globalId, taskName, taskDescription, senderApplication);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewTaskWindow, "Выбрать исполнителей");
            }
        }

        public void ReceiveRequest(string receivedNotes)
        {
            if (CrashMachinesListBox.SelectedItem == null) return;

            var request = (DataRowView) CrashMachinesListBox.SelectedItem;
            var crashMachineId = Convert.ToInt32(request["CrashMachineID"]);
            var receivedDate = App.BaseClass.GetDateFromSqlServer();
            var receiverWorkerId = AdministrationClass.CurrentWorkerId;
            _sec.FillReceivedInfo(crashMachineId, receivedDate, receiverWorkerId, receivedNotes);
            AdministrationClass.AddNewAction(9);

            var requestDate = Convert.ToDateTime(request["RequestDate"]);
            var requestWorkerId = Convert.ToInt32(request["RequestWorkerID"]);
            string workerName =
                new IdToNameConverter().Convert(receiverWorkerId, typeof (string), "ShortName", new CultureInfo("ru-RU"))
                    .ToString();
            var newsText = string.Format(ReceivedText, workerName, receivedDate);

            NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);
        }

        private void OnRequestMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CrashMachinesListBox.SelectedItem == null) return;

            var drv = (DataRowView) CrashMachinesListBox.SelectedItem;
            if (drv["ReceivedDate"] == DBNull.Value) return;

            var globalId = drv["GlobalID"].ToString();
            var rows = _taskClass.Tasks.Table.AsEnumerable().Where(t => t.Field<string>("GlobalID") == globalId);
            if (rows.Any())
            {
                var task = rows.First();
                var mainWorkerId = Convert.ToInt32(drv["ReceivedWorkerID"]);
                var fullAccess = mainWorkerId == AdministrationClass.CurrentWorkerId;

                var addNewTaskWindow = new AddNewTask(task, fullAccess);
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowCatalogGrid(addNewTaskWindow, "Список исполнителей");
                }
            }
            
        }





        private void OnEditReceiveNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            if (crashRow == null || crashRow["ReceivedDate"] == DBNull.Value ||
                Convert.ToBoolean(crashRow["RequestClose"])) return;

            var reciveNotes = crashRow["ReceivedNotes"].ToString();
            ChangeReceiveNotesTextBox.Text = reciveNotes;

            ReceivedNotesTextBlock.Visibility = Visibility.Collapsed;
            ChangeReceiveNotesTextBox.Visibility = Visibility.Visible;

            EditReceiveNotesPanel.Visibility = Visibility.Visible;
            EditReceiveNotesButton.Visibility = Visibility.Collapsed;
        }

        private void OnSaveReceiveNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            if (crashRow == null || crashRow["ReceivedDate"] == DBNull.Value ||
                Convert.ToBoolean(crashRow["RequestClose"])) return;

            var crashMachineId = Convert.ToInt32(crashRow["CrashMachineID"]);
            var receivedNotes = ChangeReceiveNotesTextBox.Text;
            _sec.ChangeReceivedNotes(crashMachineId, receivedNotes);

            OnCancelEditReceiveNotesButtonClick(null, null);
        }

        private void OnCancelEditReceiveNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            SetRecieveNotesVisibility(crashRow);
        }

        private void OnEditCompletionNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            if (crashRow == null || crashRow["CompletionDate"] == DBNull.Value ||
                Convert.ToBoolean(crashRow["RequestClose"])) return;

            var reciveNotes = crashRow["CompletionNotes"].ToString();
            ChangeCompletionNotesTextBox.Text = reciveNotes;

            CompletionNotesTextBlock.Visibility = Visibility.Collapsed;
            ChangeCompletionNotesTextBox.Visibility = Visibility.Visible;

            EditCompletionNotesPanel.Visibility = Visibility.Visible;
            EditCompletionNotesButton.Visibility = Visibility.Collapsed;
        }

        private void OnSaveCompletionNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            if (crashRow == null || crashRow["CompletionDate"] == DBNull.Value ||
                Convert.ToBoolean(crashRow["RequestClose"])) return;

            var crashMachineId = Convert.ToInt32(crashRow["CrashMachineID"]);
            var completionNotes = ChangeCompletionNotesTextBox.Text;
            _sec.ChangeCompletionNotes(crashMachineId, completionNotes);

            OnCancelEditCompletionNotesButtonClick(null, null);
        }

        private void OnCancelEditCompletionNotesButtonClick(object sender, RoutedEventArgs e)
        {
            var crashRow = CrashMachinesListBox.SelectedItem as DataRowView;
            SetCompletionNotesVisibility(crashRow);
        }

        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }

        private void OnExportCrashStatisticsToExcelButtonClick(object sender, RoutedEventArgs e)
        {
            if (RequestMachineFilterEnable.IsChecked.HasValue && RequestMachineFilterEnable.IsChecked.Value &&
                RequestMachineFilter.SelectedItem != null)
            {
                var machineId = Convert.ToInt32(RequestMachineFilter.SelectedValue);
                ExportToExcel.GenerateServiceEquipmentCrashStatisticsReport(machineId, ref ExportStatisticToExcelSwitchBox);
                return;
            }

            ExportToExcel.GenerateServiceEquipmentCrashStatisticsReport(ref ExportStatisticToExcelSwitchBox);
        }

        private void OnExportDiagrammItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExportToExcel.GenerateServiceEquipmentDiagrammReport(ref ExportStatisticToExcelSwitchBox);
        }
    }
}
