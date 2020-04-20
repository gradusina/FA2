using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FA2.ChildPages.StimulationPage;
using FA2.Classes;
using FA2.Notifications;
using FAIIControlLibrary;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для StimulationPage.xaml
    /// </summary>
    public partial class StimulationPage
    {
       bool _firstRun = true;

        #region ClassMembers

        private StimulationClass _stc;
        private StaffClass _sc;

        private readonly int _currentWorkerId;
        private readonly bool _fullAccess;
        private DateTime _currentDate;
        private int _selectedWorkerStimId;
        private MainWindow _mw;

        #endregion

        public StimulationPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
            _currentWorkerId = AdministrationClass.CurrentWorkerId;

            SetFullVersionEnable(_fullAccess);
            SetLightVersionEnable(!_fullAccess);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.WorkersStimulation);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.WorkersStimulation);

            if (_firstRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) => FillData();
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           FillBindings();

                                                           _mw = Window.GetWindow(this) as MainWindow;
                                                           if (_mw != null) _mw.HideWaitAnnimation();
                                                       };

                backgroundWorker.RunWorkerAsync();

                _firstRun = false;
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillData()
        {
            _currentDate = App.BaseClass.GetDateFromSqlServer();
            App.BaseClass.GetStimulationClass(ref _stc);
            _stc.FillWorkersStim(new DateTime(_currentDate.Year, _currentDate.Month, 1),
                new DateTime(_currentDate.Year, _currentDate.Month, DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month)));
            App.BaseClass.GetStaffClass(ref _sc);
        }

        private void FillBindings()
        {
            BindingData();
            BindingDataGrids();
            WorkersNameListBox_SelectionChanged(WorkersNameListBox, null);
        }

        private void SetFullVersionEnable(bool enabled)
        {
            FullVersionFilterGrid.IsEnabled = enabled;
            FullVersionFilterGrid.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            AddPromotionButton.IsEnabled = enabled;
            AddFineButton.IsEnabled = enabled;
        }

        private void SetLightVersionEnable(bool enabled)
        {
            LightVersionFilterGrid.IsEnabled = enabled;
            LightVersionFilterGrid.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            FineCatalogButton.IsEnabled = !enabled;
            AdditionalMenuToggleButton.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        }


        #region Bindings

        private void BindingData()
        {
            FullVersionDateFrom.SelectedDate = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            FullVersionDateTo.SelectedDate = new DateTime(_currentDate.Year, _currentDate.Month,
                DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month));
            LightVersionDateFrom.SelectedDate = FullVersionDateFrom.SelectedDate;
            LightVersionDateTo.SelectedDate = FullVersionDateTo.SelectedDate;

            WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;
            WorkersNameListBox.ItemsSource = _sc.GetStaffPersonalInfo();
            WorkersNameListBox.SelectedValuePath = "WorkerID";
            WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;
            WorkersNameListBox.Items.MoveCurrentToFirst();

            if (_fullAccess)
            {
                WorkersGroupsComboBox.SelectionChanged -= FilterComboBox_SelectionChanged;
                WorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
                WorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
                WorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
                WorkersGroupsComboBox.SelectedIndex = 1;
                WorkersGroupsComboBox.SelectionChanged += FilterComboBox_SelectionChanged;

                FactoriesComboBox.SelectionChanged -= FilterComboBox_SelectionChanged;
                FactoriesComboBox.DisplayMemberPath = "FactoryName";
                FactoriesComboBox.SelectedValuePath = "FactoryID";
                FactoriesComboBox.ItemsSource = _sc.GetFactories();
                FactoriesComboBox.SelectedIndex = 0;
                FactoriesComboBox.SelectionChanged += FilterComboBox_SelectionChanged;

                FilterComboBox_SelectionChanged(null, null);
            }
            else
            {
                WorkersNameListBox.SelectedValue = _currentWorkerId;
            }
        }

        private void BindingDataGrids()
        {
            PromotionDataGrid.ItemsSource = _stc.WorkerStimView(1);
            if (PromotionDataGrid.ItemsSource != null)
                ((DataView) PromotionDataGrid.ItemsSource).Sort = "Date";
            FineDataGrid.ItemsSource = _stc.WorkerStimView(2);
            if (FineDataGrid.ItemsSource != null)
                ((DataView) FineDataGrid.ItemsSource).Sort = "Date";
        }

        #endregion

        private void WorkersNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string defaultFilter;
            if (WorkersNameListBox.SelectedItem == null)
            {
                defaultFilter = "Available = 'TRUE' AND WorkerID = -1";
            }
            else
            {
                var workerId = Convert.ToInt32(WorkersNameListBox.SelectedValue);
                defaultFilter = string.Format("Available = 'TRUE' AND WorkerID = {0}", workerId);
            }

            if (PromotionDataGrid.ItemsSource != null)
                ((DataView) PromotionDataGrid.ItemsSource).RowFilter = defaultFilter;
            if (FineDataGrid.ItemsSource != null)
                ((DataView) FineDataGrid.ItemsSource).RowFilter = defaultFilter;

            CalculateTotalStimulationSize();
        }

        public void CalculateTotalStimulationSize()
        {
            double promotionMoney = 0;
            double promotionHours = 0;
            double fineMoney = 0;
            double fineHours = 0;

            if (PromotionDataGrid.ItemsSource != null)
                foreach (var drv in from DataRowView drv in (DataView) PromotionDataGrid.ItemsSource
                    where drv.Row["StimulationUnitID"] != DBNull.Value
                    select drv)
                {
                    switch (Convert.ToInt32(drv.Row["StimulationUnitID"]))
                    {
                        case 1:
                            promotionHours += Convert.ToDouble(drv.Row["StimulationSize"]);
                            break;
                        case 2:
                            promotionMoney += Convert.ToDouble(drv.Row["StimulationSize"]);
                            break;
                    }
                }
            if (FineDataGrid.ItemsSource != null)
                foreach (var drv in from DataRowView drv in (DataView) FineDataGrid.ItemsSource
                    where drv.Row["StimulationUnitID"] != DBNull.Value
                    select drv)
                {
                    switch (Convert.ToInt32(drv.Row["StimulationUnitID"]))
                    {
                        case 1:
                            fineHours += Convert.ToDouble(drv.Row["StimulationSize"]);
                            break;
                        case 2:
                            fineMoney += Convert.ToDouble(drv.Row["StimulationSize"]);
                            break;
                    }
                }

            HoursRatioControl.LeftValue = promotionHours;
            HoursRatioControl.RightValue = fineHours;

            MoneyRatioControl.LeftValue = promotionMoney;
            MoneyRatioControl.RightValue = fineMoney;
        }


        #region Filters

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersGroupsComboBox.SelectedItem == null || FactoriesComboBox.SelectedItem == null)
            {
                WorkersNameListBox.ItemsSource = null;
                return;
            }

            var workerGroupId = Convert.ToInt32(WorkersGroupsComboBox.SelectedValue);
            var factoryId = Convert.ToInt32(FactoriesComboBox.SelectedValue);
            var showNotEmptyWorkers = Convert.ToBoolean(ShowNotEmptyWorkerCheckBox.IsChecked);
            FilterWorkers(workerGroupId, factoryId, showNotEmptyWorkers);
        }

        #endregion


        #region StiomulationCatalog

        private void FineCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            var stimCatalog = new StimulationCatalog();
            if (_mw != null)
            {
                _mw.ShowCatalogGrid(stimCatalog, "Поощрения/штрафы");
            }

            CloseAppBar();
        }

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }

        #endregion


        #region WorkerStimulation

        private void AddPromotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            CloseAppBar();

            var workerStim = new WorkerStimulation((DataRowView)WorkersNameListBox.SelectedItem, _currentWorkerId);
            if (_mw != null)
            {
                _mw.ShowCatalogGrid(workerStim, "Добавить поощрение");
            }
        }

        private void AddFineButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            CloseAppBar();

            var workerStim = new WorkerStimulation((DataRowView)WorkersNameListBox.SelectedItem, _currentWorkerId, false);
            if (_mw != null)
            {
                _mw.ShowCatalogGrid(workerStim, "Добавить штраф");
            }
        }

        #endregion


        private void ApplyFullVersionFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (FullVersionDateFrom.SelectedDate == null || FullVersionDateTo.SelectedDate == null) return;

            var dateFrom = FullVersionDateFrom.SelectedDate.Value;
            var dateTo = FullVersionDateTo.SelectedDate.Value;

            _stc.FillWorkersStim(dateFrom, dateTo);
            PromotionDataGrid.ItemsSource = null;
            FineDataGrid.ItemsSource = null;
            BindingDataGrids();

            FilterComboBox_SelectionChanged(null, null);
            WorkersNameListBox_SelectionChanged(WorkersNameListBox, null);
        }

        private void ApplyLightVersionFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (LightVersionDateFrom.SelectedDate == null || LightVersionDateTo.SelectedDate == null) return;

            var dateFrom = LightVersionDateFrom.SelectedDate.Value;
            var dateTo = LightVersionDateTo.SelectedDate.Value;

            _stc.FillWorkersStim(dateFrom, dateTo);
            PromotionDataGrid.ItemsSource = null;
            FineDataGrid.ItemsSource = null;
            BindingDataGrids();

            WorkersNameListBox_SelectionChanged(WorkersNameListBox, null);
        }



        #region DeletingRow

        private void PromotionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PromotionDataGrid.SelectedItem == null || PromotionDataGrid.Items.Count == 0) return;

            var drv = (DataRowView)PromotionDataGrid.SelectedItem;
            if (drv != null)
            {
                var result = MetroMessageBox.Show("Вы действительно хотите удалить запись?", "Удаление",
                                                          MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var workerStimId = Convert.ToInt32(drv.Row["WorkerStimID"]);
                    _stc.DeleteWorkerStim(workerStimId, _currentWorkerId, App.BaseClass.GetDateFromSqlServer());
                    var defaultFilter = ((DataView)PromotionDataGrid.ItemsSource).RowFilter;
                    PromotionDataGrid.ItemsSource = null;
                    PromotionDataGrid.ItemsSource = _stc.WorkerStimView(1);
                    if (PromotionDataGrid.ItemsSource != null)
                    {
                        ((DataView)PromotionDataGrid.ItemsSource).RowFilter = defaultFilter;
                        ((DataView)PromotionDataGrid.ItemsSource).Sort = "Date";
                    }   
                    CalculateTotalStimulationSize();
                }
            }
        }

        private void PromotionRowMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!_fullAccess)
                e.Handled = true;
        }

        private void FineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FineDataGrid.SelectedItem == null || FineDataGrid.Items.Count == 0) return;

            var drv = (DataRowView)FineDataGrid.SelectedItem;
            if (drv != null)
            {
                var result = MetroMessageBox.Show("Вы действительно хотите удалить запись?", "Удаление",
                                                          MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var workerStimId = Convert.ToInt32(drv.Row["WorkerStimID"]);
                    _stc.DeleteWorkerStim(workerStimId, _currentWorkerId, App.BaseClass.GetDateFromSqlServer());
                    var defaultFilter = ((DataView)FineDataGrid.ItemsSource).RowFilter;
                    FineDataGrid.ItemsSource = null;
                    FineDataGrid.ItemsSource = _stc.WorkerStimView(2);
                    if (FineDataGrid.ItemsSource != null)
                    {
                        ((DataView)FineDataGrid.ItemsSource).RowFilter = defaultFilter;
                        ((DataView)FineDataGrid.ItemsSource).Sort = "Date";
                    }
                        
                    CalculateTotalStimulationSize();
                }
            }
        }

        private void FineRowMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!_fullAccess)
                e.Handled = true;
        }

        #endregion



        #region ChangeRow

        private void PromotionRow_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!_fullAccess)
                return;

            if (PromotionDataGrid.SelectedItem == null || PromotionDataGrid.Items.Count == 0 ||
                WorkersNameListBox.SelectedItem == null) return;

            CloseAppBar();

            var drv = (DataRowView) PromotionDataGrid.SelectedItem;
            if (drv != null)
            {
                _selectedWorkerStimId = Convert.ToInt32(drv["WorkerStimID"]);
                var workerStim = new WorkerStimulation((DataRowView) WorkersNameListBox.SelectedItem, drv,
                    _currentWorkerId, _selectedWorkerStimId);
                if (_mw != null)
                {
                    _mw.ShowCatalogGrid(workerStim, "Редактировать поощрение");
                }
            }
        }

        private void FineRow_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!_fullAccess)
                return;

            if (FineDataGrid.SelectedItem == null || FineDataGrid.Items.Count == 0) return;

            CloseAppBar();

            var drv = (DataRowView)FineDataGrid.SelectedItem;
            if (drv != null)
            {
                _selectedWorkerStimId = Convert.ToInt32(drv["WorkerStimID"]);
                var workerStim = new WorkerStimulation((DataRowView)WorkersNameListBox.SelectedItem, drv, 
                    _currentWorkerId, _selectedWorkerStimId, false);
                if (_mw != null)
                {
                    _mw.ShowCatalogGrid(workerStim, "Редактировать штраф");
                }
            }
        }

        #endregion



        public void UpdateDataGridSource(int workerId, bool promotionGrid = true, bool fineGrid = true)
        {
            var defaultFilter = string.Format("Available = 'TRUE' AND WorkerID = {0}", workerId);

            if(promotionGrid)
            {
                PromotionDataGrid.ItemsSource = null;
                PromotionDataGrid.ItemsSource = _stc.WorkerStimView(1);
                if (PromotionDataGrid.ItemsSource != null)
                    ((DataView)PromotionDataGrid.ItemsSource).Sort = "Date";
                if (PromotionDataGrid.ItemsSource != null)
                    ((DataView)PromotionDataGrid.ItemsSource).RowFilter = defaultFilter;
            }
            if(fineGrid)
            {
                FineDataGrid.ItemsSource = null;
                FineDataGrid.ItemsSource = _stc.WorkerStimView(2);
                if (FineDataGrid.ItemsSource != null)
                    ((DataView)FineDataGrid.ItemsSource).Sort = "Date";
                if (FineDataGrid.ItemsSource != null)
                    ((DataView)FineDataGrid.ItemsSource).RowFilter = defaultFilter;
            }
        }

        private void ShowNotEmptyWorkerCheckBox_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            if (WorkersGroupsComboBox.SelectedItem == null || FactoriesComboBox.SelectedItem == null)
            {
                WorkersNameListBox.ItemsSource = null;
                return;
            }

            var workerGroupId = Convert.ToInt32(WorkersGroupsComboBox.SelectedValue);
            var factoryId = Convert.ToInt32(FactoriesComboBox.SelectedValue);
            var showNotEmptyWorkers = Convert.ToBoolean(ShowNotEmptyWorkerCheckBox.IsChecked);
            FilterWorkers(workerGroupId, factoryId, showNotEmptyWorkers);
        }

        private void FilterWorkers(int workerGroupId, int factoryId, bool showNotEmptyWorkers)
        {
            var filteredTable = _sc.FilterWorkers(true, workerGroupId, true, factoryId, false, -1);
            var filteredView = new DataView();
            if (filteredTable != null)
            {
                if (showNotEmptyWorkers)
                {
                    var notEmptyTable =
                        filteredTable.AsEnumerable()
                            .Where(w => _stc.WorkersStimDataTable.AsEnumerable()
                                .Any(s => Convert.ToInt32(s["WorkerID"]) == Convert.ToInt32(w["WorkerID"]) &&
                                          Convert.ToBoolean(s["Available"])));
                    if (notEmptyTable.Any())
                        filteredView = notEmptyTable.AsDataView();
                }
                else
                {
                    filteredView = filteredTable.AsDataView();
                }
            }

            WorkersNameListBox.ItemsSource = filteredView;

            WorkersNameListBox.Items.MoveCurrentToFirst();
            WorkersNameListBox.Items.Refresh();
        }

        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }
    }
}
