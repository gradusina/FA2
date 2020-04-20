using FA2.ChildPages.PlannedWorksPage;
using FA2.Classes;
using FA2.Notifications;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для PlannedWorksPage.xaml
    /// </summary>
    public partial class PlannedWorksPage : Page
    {
        private readonly bool _fullAccess;
        private bool _firstTimePageRun = true;

        private DateTime _dateFrom;
        private DateTime _dateTo;

        private PlannedWorksClass _plannedWorksClass;
        private TaskClass _taskClass;

        public PlannedWorksPage(bool fullAccess)
        {
            _fullAccess = fullAccess;
            InitializeComponent();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.PlannedWorks);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.PlannedWorks);

            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) =>
                {
                    _firstTimePageRun = false;
                    FillData();
                };
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                {
                    BindingData();
                    SetDateProperties();
                    SetAccessEnable(_fullAccess);
                    OnPlannedWorksDataGridSelectionChanged(null, null);

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideWaitAnnimation();
                };

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                _taskClass.Fill(_dateFrom, _dateTo);
                RefillInfo();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillData()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _dateTo = currentDate;
            _dateFrom = currentDate != DateTime.MinValue
                ? currentDate.Month > 2
                    ? new DateTime(currentDate.Year, currentDate.Month - 2, 1)
                    : new DateTime(currentDate.Year - 1, 12 + currentDate.Month - 2, 1)
                : DateTime.MinValue;

            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);
            App.BaseClass.GetTaskClass(ref _taskClass);

            _taskClass.Fill(_dateFrom, _dateTo);
        }

        private void BindingData()
        {
            PlannedWorksDataGrid.ItemsSource = _plannedWorksClass.GetPlannedWorks();
            FilterPlannedWorksView();

            StartedPlannedWorksItemsControl.ItemsSource = _taskClass.Tasks.Table.AsDataView();
        }

        private void SetDateProperties()
        {
            DateFromPicker.SelectedDate = _dateFrom;
            DateToPicker.SelectedDate = _dateTo;
        }

        private void SetAccessEnable(bool fullAccess)
        {
            if(!fullAccess)
            {
                AdditionalMenuToggleButton.Visibility = Visibility.Collapsed;
                StartPlannedWorksButton.Margin = new Thickness(5);
            }
        }

        private void FilterPlannedWorksView()
        {
            var plannedWorksView = PlannedWorksDataGrid.ItemsSource as DataView;
            if (plannedWorksView == null) return;

            var showJustStartedPlannedWorks = ShowJustStartedPlannedWorksCheckBox.IsChecked.HasValue
                ? ShowJustStartedPlannedWorksCheckBox.IsChecked.Value : false;
            var showWaitingForConfirmationPlannedWorks = ShowWaitingForConfirmationPlannedWorksCheckBox.IsChecked.HasValue
                ? ShowWaitingForConfirmationPlannedWorksCheckBox.IsChecked.Value : false;
            var showRejectedPlannedWorks = ShowRejectedPlannedWorksCheckBox.IsChecked.HasValue
                ? ShowRejectedPlannedWorksCheckBox.IsChecked.Value : false;

            string rowFilter = "IsEnable = 'True'";
            if(showJustStartedPlannedWorks)
            {
                var plannedWorksIds = _plannedWorksClass.PlannedWorksTable.AsEnumerable().
                    Where(p => _taskClass.Tasks.Table.AsEnumerable().Any(t => !string.IsNullOrEmpty(t.Field<string>("GlobalID")) 
                        && !string.IsNullOrEmpty(p.Field<string>("GlobalID")) && t.Field<string>("GlobalID") == p.Field<string>("GlobalID") && !t.Field<Boolean>("IsComplete"))).
                    Select(p => p.Field<Int64>("PlannedWorksID")).Distinct();
                if(plannedWorksIds.Any())
                {
                    var filterByIds = plannedWorksIds.Aggregate(string.Empty, (current, i) => current + ", " + i).Substring(2);
                    rowFilter += " AND PlannedWorksID IN (" + filterByIds + ")";
                }
                else
                    rowFilter += " AND PlannedWorksID = -1";
            }
            if(showWaitingForConfirmationPlannedWorks)
                rowFilter += " AND ConfirmationStatusID = 1";
            if (showRejectedPlannedWorks)
                rowFilter += " AND ConfirmationStatusID = 3";
            else
                rowFilter += " AND ConfirmationStatusID <> 3";

            plannedWorksView.RowFilter = rowFilter;
            plannedWorksView.Sort = "PlannedWorksName";
        }


        private void OnFillPlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            if (!DateFromPicker.SelectedDate.HasValue || !DateToPicker.SelectedDate.HasValue) return;

            _dateFrom = DateFromPicker.SelectedDate.Value;
            _dateTo = DateToPicker.SelectedDate.Value;
            _taskClass.Fill(_dateFrom, _dateTo);
        }

        private void OnAddNewPlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var addPlannedWorksPage = new AddPlannedWorksPage(_fullAccess);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addPlannedWorksPage, "Добавление плановых работ");
            }
        }

        private void OnPlannedWorksRowMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            var createdWorkerId = Convert.ToInt64(plannedWorksView["CreatedWorkerID"]);

            if (!_fullAccess && !AdministrationClass.IsAdministrator && createdWorkerId != AdministrationClass.CurrentWorkerId) return;
            
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                var addPlannedWorksPage = new AddPlannedWorksPage(plannedWorksView, _fullAccess);
                mainWindow.ShowCatalogGrid(addPlannedWorksPage, "Изменить плановые работы");
            }
        }


        private void OnPlannedWorksDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeletePlannedWorksButton.Visibility = Visibility.Collapsed;
            ConfirmPlannedWorksButton.Visibility = Visibility.Collapsed;
            RejectPlannedWorksButton.Visibility = Visibility.Collapsed;
            ActivatePlannedWorksButton.Visibility = Visibility.Collapsed;
            StartPlannedWorksButton.IsEnabled = false;

            var tasksView = StartedPlannedWorksItemsControl.ItemsSource as DataView;

            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null)
            {
                if (tasksView != null)
                    tasksView.RowFilter = string.Format("GlobalID = '{0}'", 0);
                return;
            }

            var globalId = plannedWorksView["GlobalID"].ToString();
            var createdWorkerId = Convert.ToInt64(plannedWorksView["CreatedWorkerID"]);
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            if (tasksView != null)
                tasksView.RowFilter = string.Format("GlobalID = '{0}'", globalId);

            if (_fullAccess || createdWorkerId == currentWorkerId || AdministrationClass.IsAdministrator)
            {
                DeletePlannedWorksButton.Visibility = Visibility.Visible;
            }

            var confirmationStatusId = Convert.ToInt32(plannedWorksView["ConfirmationStatusID"]);
            var isActive = Convert.ToBoolean(plannedWorksView["IsActive"]);
            if((ConfirmationStatus)confirmationStatusId == ConfirmationStatus.WaitingConfirmation && (_fullAccess || AdministrationClass.IsAdministrator))
            {
                ConfirmPlannedWorksButton.Visibility = Visibility.Visible;
                RejectPlannedWorksButton.Visibility = Visibility.Visible;
            }
            else if((ConfirmationStatus)confirmationStatusId == ConfirmationStatus.Confirmed && isActive)
            {
                StartPlannedWorksButton.IsEnabled = true;
            }
            else if((ConfirmationStatus)confirmationStatusId == ConfirmationStatus.Confirmed && !isActive 
                && (_fullAccess || AdministrationClass.IsAdministrator || createdWorkerId == currentWorkerId))
            {
                ActivatePlannedWorksButton.Visibility = Visibility.Visible;
            }
        }

        private void OnPlannedWorksRowContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PlannedWorksDataGrid.SelectedItem == null || PlannedWorksDataGrid.Items.Count == 0) return;
            var cellsPresenter = (DataGridCellsPresenter)e.Source;

            var drv = cellsPresenter.Item as DataRowView;
            if (drv == null) return;

            var createdWorkerId = Convert.ToInt64(drv["CreatedWorkerID"]);
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            if (createdWorkerId != currentWorkerId && !_fullAccess && !AdministrationClass.IsAdministrator)
                e.Handled = true;


            var dataGridRow = sender as DataGridRow;
            if (dataGridRow == null) return;

            var contextMenu = dataGridRow.ContextMenu;
            if (contextMenu == null || contextMenu.Items.Count < 3) return;

            var reloadPlannedWorksMenuItem = contextMenu.Items[2] as MenuItem;
            if (reloadPlannedWorksMenuItem != null)
            {
                var confirmationStatusId = Convert.ToInt32(drv["ConfirmationStatusID"]);
                var isActive = Convert.ToBoolean(drv["IsActive"]);

                reloadPlannedWorksMenuItem.IsEnabled = 
                    (ConfirmationStatus)confirmationStatusId == ConfirmationStatus.Confirmed && !isActive;
            }
        }

        private void OnDeletePlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить данную работу из списка?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var plannedWorksId = Convert.ToInt64(plannedWorksView["PlannedWorksID"]);
            _plannedWorksClass.DeletePlannedWorks(plannedWorksId);
            AdministrationClass.AddNewAction(48);
        }

        private void OnDeletePlannedWorksRowMenuItemClick(object sender, RoutedEventArgs e)
        {
            OnDeletePlannedWorksButtonClick(null, null);
        }

        private void OnChangePlannedWorksRowMenuItemClick(object sender, RoutedEventArgs e)
        {
            OnPlannedWorksRowMouseDoubleClick(null, null);
        }

        private void OnReloadPlannedWorksMenuItemClick(object sender, RoutedEventArgs e)
        {
            OnActivatePlannedWorksButtonClick(null, null);
        }

        private void OnConfirmPlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            if (MessageBox.Show("Вы действительно хотите подтвердить данную работу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var plannedWorksId = Convert.ToInt64(plannedWorksView["PlannedWorksID"]);
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _plannedWorksClass.ConfirmPlannedWorks(plannedWorksId, currentDate, currentWorkerId);
            AdministrationClass.AddNewAction(46);

            OnPlannedWorksDataGridSelectionChanged(null, null);
        }

        private void OnRejectPlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            if (MessageBox.Show("Вы действительно хотите отклонить данную работу?", "Отклонение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var plannedWorksId = Convert.ToInt64(plannedWorksView["PlannedWorksID"]);
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _plannedWorksClass.RejectPlannedWorks(plannedWorksId, currentDate, currentWorkerId);
            AdministrationClass.AddNewAction(47);

            OnPlannedWorksDataGridSelectionChanged(null, null);
        }

        private void OnActivatePlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            var plannedWorksId = Convert.ToInt64(plannedWorksView["PlannedWorksID"]);
            _plannedWorksClass.ActivatePlannedWorks(plannedWorksId);
            AdministrationClass.AddNewAction(58);

            OnPlannedWorksDataGridSelectionChanged(null, null);
        }


        private void OnStartPlannedWorksButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            var isActive = Convert.ToBoolean(plannedWorksView["IsActive"]);
            if (!isActive)
            {
                MessageBox.Show("Вы не можете приступить к выполнению данных работ. Они неактивны. \nТребуется активация работ ответственными сотрудниками.", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                var chooseEmptyWorkReasonPage = new ChooseEmptyWorkReasonPage(plannedWorksView);
                mainWindow.ShowCatalogGrid(chooseEmptyWorkReasonPage, "Укажите причину");
            }
        }

        public void StartPlannedWorks(long taskId, int emptyWorkReasonId)
        {
            var plannedWorksView = PlannedWorksDataGrid.SelectedItem as DataRowView;
            if (plannedWorksView == null) return;

            var plannedWorksId = Convert.ToInt64(plannedWorksView["PlannedWorksID"]);
            var isMultiple = Convert.ToBoolean(plannedWorksView["IsMultiple"]);
            _plannedWorksClass.AddStartedPlanedWorks(plannedWorksId, taskId, emptyWorkReasonId);
            AdministrationClass.AddNewAction(56);

            if (!isMultiple)
                _plannedWorksClass.DeactivatePlannedWorks(plannedWorksId);
        }

        public void RefillInfo()
        {
            PlannedWorksDataGrid.Items.Refresh();
            OnPlannedWorksDataGridSelectionChanged(null, null);
        }


        private void OnShowJustStartedPlannedWorksCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ShowWaitingForConfirmationPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowWaitingForConfirmationPlannedWorksCheckBox.IsChecked = false;
            ShowWaitingForConfirmationPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            ShowRejectedPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowRejectedPlannedWorksCheckBox.IsChecked = false;
            ShowRejectedPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            FilterPlannedWorksView();
        }

        private void OnShowWaitingForConfirmationPlannedWorksCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ShowJustStartedPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowJustStartedPlannedWorksCheckBox.IsChecked = false;
            ShowJustStartedPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            ShowRejectedPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowRejectedPlannedWorksCheckBox.IsChecked = false;
            ShowRejectedPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            FilterPlannedWorksView();
        }

        private void OnShowRejectedPlannedWorksCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ShowJustStartedPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowJustStartedPlannedWorksCheckBox.IsChecked = false;
            ShowJustStartedPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            ShowWaitingForConfirmationPlannedWorksCheckBox.Unchecked -= OnFilterStatesChanged;
            ShowWaitingForConfirmationPlannedWorksCheckBox.IsChecked = false;
            ShowWaitingForConfirmationPlannedWorksCheckBox.Unchecked += OnFilterStatesChanged;

            FilterPlannedWorksView();
        }

        private void OnFilterStatesChanged(object sender, RoutedEventArgs e)
        {
            FilterPlannedWorksView();
        }


        private void OnExporToExcelButtonClick(object sender, RoutedEventArgs e)
        {
            ExportToExcel.GeneratePlannedWorksReport();
            AdministrationClass.AddNewAction(70);
            AdditionalMenuToggleButton.IsChecked = false;
        }

        private void OnEmptyWorkReasonsButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var emptyWorkReasonsPage = new EmptyWorkReasonsPage();
                mainWindow.ShowCatalogGrid(emptyWorkReasonsPage, "Причины выполнения работ");
            }
        }

        private void OnPlannedWorksTypesButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                var plannedWorksTypesPage = new PlannedWorksTypesPage();
                mainWindow.ShowCatalogGrid(plannedWorksTypesPage, "Типы работ");
            }
        }

        private void OnShadowGridMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }
    }
}
