using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FAIIControlLibrary;
using FA2.ChildPages.ProdRoomsPage;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для ProdRoomsPage.xaml
    /// </summary>
    public partial class ProdRoomsPage
    {
        private DataTable _openingDt;
        private DataTable _closingDt;
        private bool _editingMode;
        private bool _editingWeekendResponsiblesMode;
        private bool _firstTimePageRun = true;
        private bool _fullAccess;

        private IdToNameConverter _iConverter;
        private ProdRoomsClass _prc;
        private StaffClass _sc;

        private DateTimeFormatInfo _dtformatInfo;
        private readonly int _currentWorker;

        private int _currentYear;
        private int _currentMonth;

        public ProdRoomsPage(bool fullAccess)
        {
            InitializeComponent();
            _currentWorker = AdministrationClass.CurrentWorkerId;
            _fullAccess = fullAccess;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.ProductionRooms);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.ProductionRooms);

            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) => FillData();
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           BindingData();
                                                           SetAccessMode(_fullAccess);
                                                           SetRaportComboBoxItemsAvailability();
                                                           SetResponsibleArriveButtonEnable();
                                                           LocksActualStatusButton.IsChecked = true;
                                                           _firstTimePageRun = false;

                                                           var mainWindow = Application.Current.MainWindow as MainWindow;
                                                           if (mainWindow != null) mainWindow.HideWaitAnnimation();
                                                       };

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();

                FillJournalGrids(_prc.JournalYear, _prc.JournalMonth);
                ProdRoomsActualStatusItemsControl.Items.Refresh();

                SetRaportComboBoxItemsAvailability();
                SetResponsibleArriveButtonEnable();
            }
        }

        private void FillData()
        {
            App.BaseClass.GetProdRoomsClass(ref _prc);
            App.BaseClass.GetStaffClass(ref _sc);

            _openingDt = new DataTable();
            _closingDt = new DataTable();
            _iConverter = new IdToNameConverter();

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _currentYear = currentDate.Year;
            _currentMonth = currentDate.Month;
        }

        private void BindingData()
        {
            BindingYearComboBox();
            BindingMonthComboBox();

            BindingClosingDoorComboBox();

            BindingProdRoomsActualStatusItemsControl();
            BindingLockRedactorListBox();
            BindingActionListBox();
            BindingActionStatusComboBoxes();

            FillGrids(DateTime.Now.Year, DateTime.Now.Month);
            openingDataGrid.ItemsSource = _openingDt.DefaultView;
            closingDataGrid.ItemsSource = _closingDt.DefaultView;
            editingWorkerName.Text = _prc.GetTimeSheetEditingDate();

            FillWeekendResponsiblesDataGrids(DateTime.Now.Year, DateTime.Now.Month);
            WeekendResponsiblesDataGrid.ItemsSource = _prc.GetWeekendsResponsiblesView();
            ResponsiblesArrivesDataGrid.ItemsSource = _prc.ResponsibleArrivesTable.AsDataView();

            FillJournalGrids(DateTime.Now.Year, DateTime.Now.Month);
            ClosingJournalDataGrid.ItemsSource = _prc.JournalProductionsTable.AsDataView();
            OpeningJournalDataGrid.ItemsSource = _prc.JournalProductionsTable.AsDataView();
            FilterClosingView();
            FilterOpeningView();

            BindingOpeningData();
            BindingClosingData();
            BindingWeekendData();

            ResponsibleWorker.Text = string.Format("Ответственный: {0}", _iConverter.Convert(_currentWorker, "FullName"));

            if (openingGroupComboBox.Items.Count != 0 && closingGroupComboBox.Items.Count != 0)
            {
                openingGroupComboBox.SelectedIndex = 0;
                closingGroupComboBox.SelectedIndex = 0;
            }

            if (WeekendResponsiblesGroupComboBox.HasItems)
                WeekendResponsiblesGroupComboBox.SelectedIndex = 0;
        }

        private void SetAccessMode(bool fullAccess)
        {
            RedactorItemButton.Visibility = fullAccess
                ? Visibility.Visible
                : Visibility.Collapsed;

            editingButton.IsEnabled = fullAccess;
            EditWeekendResponsiblesButton.IsEnabled = fullAccess;
            ExportResponsibleRaportButton.IsEnabled = fullAccess;

            SaveClosingOpeningTimeSheetButton.IsEnabled = fullAccess;
            SaveWeekendResponsiblesButton.IsEnabled = fullAccess;
        }


        #region Bindings

        private void BindingYearComboBox()
        {
            yearComboBox.ItemsSource = ProdRoomsClass.DistinctYears();
            yearComboBox.SelectedItem = _currentYear;

            WeekendResponsiblesYearComboBox.ItemsSource = ProdRoomsClass.DistinctWeekendResponsiblesYears();
            WeekendResponsiblesYearComboBox.SelectedItem = _currentYear;

            JournalYearComboBox.ItemsSource = ProdRoomsClass.DistinctJournalYears();
            JournalYearComboBox.SelectedItem = _currentYear;
        }

        private void BindingMonthComboBox()
        {
            var ci = new CultureInfo("ru-RU");
            _dtformatInfo = ci.DateTimeFormat;
            for (var i = 1; i <= 12; i++)
            {
                var monthName = _dtformatInfo.GetMonthName(i);
                monthComboBox.Items.Add(monthName);
                WeekendResponsiblesMonthComboBox.Items.Add(monthName);
                JournalMonthComboBox.Items.Add(monthName);
            }
            monthComboBox.SelectedIndex = _currentMonth - 1;
            WeekendResponsiblesMonthComboBox.SelectedIndex = _currentMonth - 1;
            JournalMonthComboBox.SelectedIndex = _currentMonth - 1;
        }

        private void BindingClosingDoorComboBox()
        {
            ClosingDoorComboBox.ItemsSource = _prc.Locks.Table.AsDataView();
            ((DataView) ClosingDoorComboBox.ItemsSource).RowFilter = "IsEnable = 'True'";
            ClosingDoorComboBox.SelectedValuePath = "LockID";
            ClosingDoorComboBox.DisplayMemberPath = "LockName";
            if (ClosingDoorComboBox.HasItems)
                ClosingDoorComboBox.SelectedIndex = 0;
        }

        private void BindingOpeningData()
        {
            openingFactoryComboBox.SelectionChanged -= openingFactoryComboBox_SelectionChanged;
            openingFactoryComboBox.ItemsSource = _sc.GetFactories();
            openingFactoryComboBox.DisplayMemberPath = "FactoryName";
            openingFactoryComboBox.SelectedValuePath = "FactoryID";
            openingFactoryComboBox.SelectionChanged += openingFactoryComboBox_SelectionChanged;
            openingFactoryComboBox.Items.MoveCurrentToFirst();

            openingNameComboBox.ItemsSource = _sc.GetStaffPersonalInfo();
            openingNameComboBox.DisplayMemberPath = "Name";
            openingNameComboBox.SelectedValuePath = "WorkerID";
            openingNameComboBox.Items.MoveCurrentToFirst();

            openingGroupComboBox.SelectionChanged -= openingGroupComboBox_SelectionChanged;
            openingGroupComboBox.DisplayMemberPath = "WorkerGroupName";
            openingGroupComboBox.SelectedValuePath = "WorkerGroupID";
            openingGroupComboBox.ItemsSource = _sc.GetWorkerGroups();
            openingGroupComboBox.SelectionChanged += openingGroupComboBox_SelectionChanged;
            openingGroupComboBox.Items.MoveCurrentToFirst();
        }

        private void BindingClosingData()
        {
            closingFactoryComboBox.SelectionChanged -= closingFactoryComboBox_SelectionChanged;
            closingFactoryComboBox.ItemsSource = _sc.GetFactories();
            closingFactoryComboBox.DisplayMemberPath = "FactoryName";
            closingFactoryComboBox.SelectedValuePath = "FactoryID";
            closingFactoryComboBox.SelectionChanged += closingFactoryComboBox_SelectionChanged;
            closingFactoryComboBox.Items.MoveCurrentToFirst();

            closingNameComboBox.ItemsSource = _sc.GetStaffPersonalInfo();
            closingNameComboBox.DisplayMemberPath = "Name";
            closingNameComboBox.SelectedValuePath = "WorkerID";
            closingNameComboBox.Items.MoveCurrentToFirst();

            closingGroupComboBox.SelectionChanged -= closingGroupComboBox_SelectionChanged;
            closingGroupComboBox.DisplayMemberPath = "WorkerGroupName";
            closingGroupComboBox.SelectedValuePath = "WorkerGroupID";
            closingGroupComboBox.ItemsSource = _sc.GetWorkerGroups();
            closingGroupComboBox.SelectionChanged += closingGroupComboBox_SelectionChanged;
            closingGroupComboBox.Items.MoveCurrentToFirst();
        }

        private void BindingWeekendData()
        {
            WeekendResponsiblesNameComboBox.ItemsSource = _sc.GetStaffPersonalInfo();
            WeekendResponsiblesNameComboBox.DisplayMemberPath = "Name";
            WeekendResponsiblesNameComboBox.SelectedValuePath = "WorkerID";

            WeekendResponsiblesFactoryComboBox.SelectionChanged -= OnWeekendResponsiblesFactoryComboBoxSelectionChanged;
            WeekendResponsiblesFactoryComboBox.ItemsSource = _sc.GetFactories();
            WeekendResponsiblesFactoryComboBox.DisplayMemberPath = "FactoryName";
            WeekendResponsiblesFactoryComboBox.SelectedValuePath = "FactoryID";
            WeekendResponsiblesFactoryComboBox.SelectionChanged += OnWeekendResponsiblesFactoryComboBoxSelectionChanged;

            WeekendResponsiblesGroupComboBox.SelectionChanged -= OnWeekendResponsiblesGroupComboBoxSelectionChanged;
            WeekendResponsiblesGroupComboBox.DisplayMemberPath = "WorkerGroupName";
            WeekendResponsiblesGroupComboBox.SelectedValuePath = "WorkerGroupID";
            WeekendResponsiblesGroupComboBox.ItemsSource = _sc.GetWorkerGroups();
            WeekendResponsiblesGroupComboBox.SelectionChanged += OnWeekendResponsiblesGroupComboBoxSelectionChanged;
        }

        private void BindingProdRoomsActualStatusItemsControl()
        {
            ProdRoomsActualStatusItemsControl.ItemsSource = _prc.GetLocks();
        }

        private void BindingLockRedactorListBox()
        {
            LockRedactorListBox.ItemsSource = _prc.Locks.Table.AsDataView();
            ((DataView)LockRedactorListBox.ItemsSource).RowFilter = "IsEnable = 'True'";
            LockRedactorListBox.DisplayMemberPath = "LockName";
            LockRedactorListBox.SelectedValuePath = "LockID";
            if (LockRedactorListBox.HasItems)
                LockRedactorListBox.SelectedIndex = 0;
        }

        private void BindingActionListBox()
        {
            _prc.Actions.Table.DefaultView.RowFilter = "Visible = 'True'";
            var view = CollectionViewSource.GetDefaultView(_prc.Actions.Table);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("ActionStatus"));
            view.SortDescriptions.Add(new SortDescription("ActionStatus", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("ActionNumber", ListSortDirection.Ascending));
            ActionListBox.ItemsSource = view;
            ActionListBox.SelectedValuePath = "ActionID";
            ActionListBox.SelectedIndex = 0;
        }

        private void BindingActionStatusComboBoxes()
        {
            AddActionStatusComboBox.ItemsSource = _prc.Actions.Status.DefaultView;
            AddActionStatusComboBox.SelectedValuePath = "ActionStatusID";
            AddActionStatusComboBox.DisplayMemberPath = "ActionStatusName";
            if (AddActionStatusComboBox.Items.Count != 0)
                AddActionStatusComboBox.SelectedIndex = 0;

            ActionStatusComboBox.ItemsSource = _prc.Actions.Status.DefaultView;
            ActionStatusComboBox.SelectedValuePath = "ActionStatusID";
            ActionStatusComboBox.DisplayMemberPath = "ActionStatusName";
        }

        #endregion


        private void FilterClosingView()
        {
            var closingView = ClosingJournalDataGrid.ItemsSource as DataView;
            if (closingView == null) return;

            var filter = "LockStatusID = 2 AND Visible = 'True'";
            if (!string.IsNullOrEmpty(ClosingSealNumerSearchTextBox.Text))
                filter += " AND SealNumber LIKE '" + ClosingSealNumerSearchTextBox.Text + "*'";
            closingView.RowFilter = filter;
            closingView.Sort = "Date DESC";
        }

        private void FilterOpeningView()
        {
            var closingView = OpeningJournalDataGrid.ItemsSource as DataView;
            if (closingView == null) return;

            var filter = "LockStatusID = 1 AND Visible = 'True'";
            if (!string.IsNullOrEmpty(OpeningSealNumerSearchTextBox.Text))
                filter += " AND SealNumber LIKE '" + OpeningSealNumerSearchTextBox.Text + "*'";
            closingView.RowFilter = filter;
            closingView.Sort = "Date ASC";
        }

        private void OnOpeningDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(bool))
            {
                ((DataGridCheckBoxColumn)e.Column).ElementStyle = Resources["OpeningCheckBox"] as Style;
                ((DataGridCheckBoxColumn)e.Column).EditingElementStyle = Resources["EditingOpeningCheckBox"] as Style;
            }
        }

        private void OnClosingDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(bool))
            {
                ((DataGridCheckBoxColumn)e.Column).ElementStyle = Resources["ClosingCheckBox"] as Style;
                ((DataGridCheckBoxColumn)e.Column).EditingElementStyle = Resources["EditingClosingCheckBox"] as Style;
            }
        }

        private void OnOpeningDataGridAutoGeneratedColumns(object sender, EventArgs e)
        {
            SetOpeningCellStyle(Convert.ToInt32(yearComboBox.SelectedItem), monthComboBox.SelectedIndex + 1);
        }

        private void OnClosingDataGridAutoGeneratedColumns(object sender, EventArgs e)
        {
            SetClosingCellStyle(Convert.ToInt32(yearComboBox.SelectedItem), monthComboBox.SelectedIndex + 1);
        }

        private void SetOpeningCellStyle(int selectedYear, int selectedMonth)
        {
            openingDataGrid.Columns[1].Visibility = Visibility.Hidden;
            var daysCount = DateTime.DaysInMonth(selectedYear, selectedMonth);
            for (var i = 1; i < daysCount + 1; i++)
            {
                var date = new DateTime(selectedYear, selectedMonth, i);
                SetStyleForCells(ref openingDataGrid, i + 1, date);
            }

            if (_openingDt.Rows[0]["WorkerID"].ToString() == string.Empty)
                _openingDt.Rows[0].Delete();
        }

        private void SetClosingCellStyle(int selectedYear, int selectedMonth)
        {
            closingDataGrid.Columns[1].Visibility = Visibility.Hidden;
            var daysCount = DateTime.DaysInMonth(selectedYear, selectedMonth);
            for (var i = 1; i < daysCount + 1; i++)
            {
                var date = new DateTime(selectedYear, selectedMonth, i);
                SetStyleForCells(ref closingDataGrid, i + 1, date);
            }

            if (_closingDt.Rows[0]["WorkerID"].ToString() == string.Empty)
                _closingDt.Rows[0].Delete();
        }

        private void FillGrids(int selectedYear, int selectedMonth)
        {
            _prc.FillTimeSheet(selectedYear, selectedMonth);
            _openingDt = _prc.GetOpeningTimeSheeTable();
            _closingDt = _prc.GetClosingTimeSheeTable();

            SetTablesNames(selectedYear.ToString(CultureInfo.InvariantCulture),
                _dtformatInfo.GetMonthName(selectedMonth));
        }

        private void FillWeekendResponsiblesDataGrids(int selectedYear, int selectedMonth)
        {
            _prc.FillWeekendTimeSheet(selectedYear, selectedMonth);
            _prc.FillResponsibleArrives(selectedYear, selectedMonth);
        }


        private void FillJournalGrids(int selectedYear, int selectedMonth)
        {
            _prc.FillJournalProductionTable(selectedYear, selectedMonth);
            var ids = from taskView in _prc.JournalProductionsTable.AsEnumerable()
                      select taskView.Field<Int64>("JournalID");
            _prc.FillConfirmDataTable(ids);
        }

        private void SetTablesNames(string year, string month)
        {
            var name = "(" + month + ", " + year + "):";
            OpeningNameTextBlock.Text = name;
            ClosingNameTextBlock.Text = name;
        }

        private void SetStyleForCells(ref DataGrid dataGrid, int columnNumber, DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                dataGrid.Columns[columnNumber].CellStyle = Resources["WeekendDay"] as Style;

            if (date.DayOfYear == DateTime.Now.DayOfYear && date.Year == DateTime.Now.Year)
                dataGrid.Columns[columnNumber].HeaderStyle = Resources["CurrentDay"] as Style;
        }



        private void showGraphButton_Click(object sender, RoutedEventArgs e)
        {
            openingDataGrid.ItemsSource = null;
            closingDataGrid.ItemsSource = null;
            FillGrids(Convert.ToInt32(yearComboBox.SelectedItem), monthComboBox.SelectedIndex + 1);
            openingDataGrid.ItemsSource = _openingDt.DefaultView;
            closingDataGrid.ItemsSource = _closingDt.DefaultView;
            editingWorkerName.Text = _prc.GetTimeSheetEditingDate();
        }

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }


        private void editingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_editingMode)
            {
                CloseAppBar();
                openingEditRow.Height = new GridLength(0);
                ClosingEditRow.Height = new GridLength(0);
                SaveRow.Height = new GridLength(0);
                editingButton.Content = "Перейти к редактированию";
                openingDataGrid.IsReadOnly = true;
                closingDataGrid.IsReadOnly = true;

                exportToExcelButton.Visibility = Visibility.Visible;
                ExportResponsibleRaportButton.Visibility = Visibility.Visible;
                _editingMode = false;
            }
            else
            {
                CloseAppBar();
                openingEditRow.Height = new GridLength(40);
                ClosingEditRow.Height = new GridLength(40);
                SaveRow.Height = new GridLength(50);
                editingButton.Content = "Выйти из редактирования";
                openingDataGrid.IsReadOnly = false;
                closingDataGrid.IsReadOnly = false;

                exportToExcelButton.Visibility = Visibility.Collapsed;
                ExportResponsibleRaportButton.Visibility = Visibility.Collapsed;
                _editingMode = true;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            _prc.SaveOpeningTimeSheetChanges();
            _prc.SaveClosingTimeSheetChanges();
            AdministrationClass.AddNewAction(4);
            showGraphButton_Click(null, null);
        }

        private void addOpeningWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            if (openingNameComboBox.SelectedItem != null)
                _prc.AddNewWorkerToOpeningTimeSheet(Convert.ToInt32(openingNameComboBox.SelectedValue));
        }

        private void addClosingWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            if (closingNameComboBox.SelectedValue != null)
                _prc.AddNewWorkerToClosingTimeSheet(Convert.ToInt32(closingNameComboBox.SelectedValue));
        }



        private void openingFactoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (openingFactoryComboBox.Items.Count == 0) return;

            openingNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(openingGroupComboBox.SelectedValue),
                                                                 Convert.ToInt32(openingFactoryComboBox.SelectedValue));
            openingNameComboBox.SelectedIndex = 0;
        }

        private void openingGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (openingGroupComboBox.Items.Count == 0) return;

            var unitToFactory = ((DataRowView)openingGroupComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            if (unitToFactory == string.Empty || !Convert.ToBoolean(unitToFactory))
            {
                openingFactoryColumn.Width = new GridLength(0);
                openingNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(openingGroupComboBox.SelectedValue));
                openingNameComboBox.SelectedIndex = 0;
            }
            else
            {
                openingFactoryColumn.Width = new GridLength(1, GridUnitType.Auto);
                openingFactoryComboBox.SelectedIndex = 0;
                openingFactoryComboBox_SelectionChanged(null, null);
            }
        }

        private void closingFactoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (closingFactoryComboBox.Items.Count == 0) return;

            closingNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(closingGroupComboBox.SelectedValue),
                                                                 Convert.ToInt32(closingFactoryComboBox.SelectedValue));
            closingNameComboBox.SelectedIndex = 0;
        }

        private void closingGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (closingGroupComboBox.Items.Count == 0) return;

            var unitToFactory = ((DataRowView)closingGroupComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            if (unitToFactory == string.Empty || !Convert.ToBoolean(unitToFactory))
            {
                closingFactoryColumn.Width = new GridLength(0);
                closingNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(closingGroupComboBox.SelectedValue));
                closingNameComboBox.SelectedIndex = 0;
            }
            else
            {
                closingFactoryColumn.Width = new GridLength(1, GridUnitType.Auto);
                closingFactoryComboBox.SelectedIndex = 0;
                closingFactoryComboBox_SelectionChanged(null, null);
            }
        }


        private DataView WorkersGroupFilter(int groupID)
        {
            var workerProfessionsByGroup =
                    (_sc.WorkerProfessionsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("WorkerGroupID") == groupID));

            var workersNamesByGroups =
                (_sc.StaffPersonalInfoDataTable.AsEnumerable().Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
                    x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID"))));

            if (workersNamesByGroups.Count() != 0)
            {
                var wNamesDt = workersNamesByGroups.CopyToDataTable();
                return wNamesDt.DefaultView;
            }
            return null;
        }

        private DataView WorkersGroupFilter(int groupID, int factoryID)
        {
            var workerProfessionsByGroup =
                (_sc.WorkerProfessionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerGroupID") == groupID).Where(r => r.Field<Int64>("FactoryID") == factoryID));

            var workersNamesByGroups =
                (_sc.StaffPersonalInfoDataTable.AsEnumerable().Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
                    x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID"))));

            if (workersNamesByGroups.Count() != 0)
            {
                var wNamesDt = workersNamesByGroups.CopyToDataTable();
                return wNamesDt.DefaultView;
            }
            return null;
        }


        private void OnLocksActualStatusButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_firstTimePageRun)
                CloseAppBar();

            ((ToggleButton)sender).IsEnabled = false;
            CheckItemButton.IsEnabled = true;
            CheckItemButton.IsChecked = false;
            TabItemButton.IsEnabled = true;
            TabItemButton.IsChecked = false;
            WeekendResponsiblesToggleButton.IsEnabled = true;
            WeekendResponsiblesToggleButton.IsChecked = false;
            RedactorItemButton.IsEnabled = true;
            RedactorItemButton.IsChecked = false;
            editingButton.Visibility = Visibility.Collapsed;
            exportToExcelButton.Visibility = Visibility.Collapsed;
            ExportResponsibleRaportButton.Visibility = Visibility.Collapsed;
            ExportJournalButton.Visibility = Visibility.Collapsed;
            EditWeekendResponsiblesButton.Visibility = Visibility.Collapsed;
            ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Collapsed;
            TabControl.SelectedIndex = 0;

            ProdRoomsActualStatusItemsControl.Items.Refresh();
        }

        private void CheckItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_firstTimePageRun)
                CloseAppBar();

            ((ToggleButton) sender).IsEnabled = false;
            LocksActualStatusButton.IsEnabled = true;
            LocksActualStatusButton.IsChecked = false;
            TabItemButton.IsEnabled = true;
            TabItemButton.IsChecked = false;
            WeekendResponsiblesToggleButton.IsEnabled = true;
            WeekendResponsiblesToggleButton.IsChecked = false;
            RedactorItemButton.IsEnabled = true;
            RedactorItemButton.IsChecked = false;
            editingButton.Visibility = Visibility.Collapsed;
            exportToExcelButton.Visibility = Visibility.Collapsed;
            ExportResponsibleRaportButton.Visibility = Visibility.Collapsed;
            ExportJournalButton.Visibility = Visibility.Visible;
            EditWeekendResponsiblesButton.Visibility = Visibility.Collapsed;
            ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Collapsed;
            TabControl.SelectedIndex = 1;
        }

        private void TabItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_firstTimePageRun)
                CloseAppBar();

            ((ToggleButton)sender).IsEnabled = false;
            LocksActualStatusButton.IsEnabled = true;
            LocksActualStatusButton.IsChecked = false;
            CheckItemButton.IsEnabled = true;
            CheckItemButton.IsChecked = false;
            WeekendResponsiblesToggleButton.IsEnabled = true;
            WeekendResponsiblesToggleButton.IsChecked = false;
            RedactorItemButton.IsEnabled = true;
            RedactorItemButton.IsChecked = false;
            editingButton.Visibility = Visibility.Visible;
            if (!_editingMode)
            {
                exportToExcelButton.Visibility = Visibility.Visible;
                ExportResponsibleRaportButton.Visibility = Visibility.Visible;
            }
            ExportJournalButton.Visibility = Visibility.Collapsed;
            EditWeekendResponsiblesButton.Visibility = Visibility.Collapsed;
            ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Collapsed;
            TabControl.SelectedIndex = 2;
        }

        private void OnWeekendResponsiblesToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_firstTimePageRun)
                CloseAppBar();

            ((ToggleButton)sender).IsEnabled = false;
            LocksActualStatusButton.IsEnabled = true;
            LocksActualStatusButton.IsChecked = false;
            CheckItemButton.IsEnabled = true;
            CheckItemButton.IsChecked = false;
            TabItemButton.IsEnabled = true;
            TabItemButton.IsChecked = false;
            RedactorItemButton.IsEnabled = true;
            RedactorItemButton.IsChecked = false;
            editingButton.Visibility = Visibility.Collapsed;
            exportToExcelButton.Visibility = Visibility.Collapsed;
            ExportResponsibleRaportButton.Visibility = Visibility.Collapsed;
            ExportJournalButton.Visibility = Visibility.Collapsed;
            EditWeekendResponsiblesButton.Visibility = Visibility.Visible;
            if (!_editingWeekendResponsiblesMode)
                ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Visible;
            SaveWeekendResponsiblesButton.Visibility = _editingWeekendResponsiblesMode ? Visibility.Visible : Visibility.Collapsed;
            TabControl.SelectedIndex = 3;
        }

        private void RedactorItemButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_firstTimePageRun)
                CloseAppBar();

            ((ToggleButton) sender).IsEnabled = false;
            LocksActualStatusButton.IsEnabled = true;
            LocksActualStatusButton.IsChecked = false;
            TabItemButton.IsEnabled = true;
            TabItemButton.IsChecked = false;
            CheckItemButton.IsEnabled = true;
            CheckItemButton.IsChecked = false;
            WeekendResponsiblesToggleButton.IsEnabled = true;
            WeekendResponsiblesToggleButton.IsChecked = false;
            editingButton.Visibility = Visibility.Collapsed;
            exportToExcelButton.Visibility = Visibility.Collapsed;
            ExportResponsibleRaportButton.Visibility = Visibility.Collapsed;
            ExportJournalButton.Visibility = Visibility.Collapsed;
            EditWeekendResponsiblesButton.Visibility = Visibility.Collapsed;
            ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Collapsed;
            TabControl.SelectedIndex = 4;
        }

        private void exportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedYear = Convert.ToInt32(yearComboBox.SelectedItem);
            var selectedMonth = monthComboBox.SelectedIndex + 1;
            ExportToExcel.GenerateScheduleReport(ref openingDataGrid, ref closingDataGrid, selectedYear,
                                  selectedMonth, _dtformatInfo.GetMonthName(selectedMonth));
        }

        private void OnExportResponsibleRaportButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedYear = Convert.ToInt32(yearComboBox.SelectedItem);
            var selectedMonth = monthComboBox.SelectedIndex + 1;
            ExportToExcel.GenerateProdRoomsResponsiblesReport(selectedYear, selectedMonth);
        }

        private void deletingOpeningCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_editingMode)
            {
                var result = MetroMessageBox.Show("Вы действительно хотите удалить работника из списка",
                                                          string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (openingDataGrid.SelectedIndex == -1
                        || ((DataRowView)openingDataGrid.SelectedItem).Row["WorkerID"].ToString() == string.Empty)
                        return;
                    var workerID = Convert.ToInt32(((DataRowView)openingDataGrid.SelectedItem).Row["WorkerID"]);
                    _prc.DeleteWorkerFromOpeningTimeSheet(workerID);
                }
            }
        }

        private void deletingClosingCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_editingMode)
            {
                var result = MetroMessageBox.Show("Вы действительно хотите удалить работника из списка",
                                                          string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (closingDataGrid.SelectedIndex == -1
                        || ((DataRowView)closingDataGrid.SelectedItem).Row["WorkerID"].ToString() == string.Empty)
                        return;
                    var workerID = Convert.ToInt32(((DataRowView)closingDataGrid.SelectedItem).Row["WorkerID"]);
                    _prc.DeleteWorkerFromClosingTimeSheet(workerID);
                }
            }
        }

        private void OnShowInstructionButtonClick(object sender, RoutedEventArgs e)
        {
            CloseAppBar();
            ApplyEffect();

            ActionNotePopup.IsOpen = true;

            ActionNoteItemsControl.ItemsSource = _prc.Actions.Table.DefaultView;
        }


        private void ApplyEffect()
        {
            var objBlur = new System.Windows.Media.Effects.BlurEffect {Radius = 20};
            TabControl.Effect = objBlur;
            TabControl.IsEnabled = false;
        }

        private void ClearEffect()
        {
            TabControl.Effect = null;
            TabControl.IsEnabled = true;
        }


        private void OnClosingCheckButtonClick(object sender, RoutedEventArgs e)
        {
            if (ClosingDoorComboBox.SelectedItem == null || string.IsNullOrEmpty(ClosingStampNumber.Text))
                return;

            var lockId = Convert.ToInt32(ClosingDoorComboBox.SelectedValue);
            if (_prc.JournalProductionsTable.AsEnumerable().Any(jP => jP.Field<Int64>("LockID") == lockId
                                                                      && jP.Field<bool>("Visible")
                                                                      && jP.Field<bool>("IsClosed")))
            {
                if (MessageBox.Show(
                    "Данная дверь уже отмечена как закрытая. Необходимо открыть дверь в программе. " +
                    "\nПерейти к препятствующей записи?", "Предупреждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var dt = ((DataView) ClosingJournalDataGrid.ItemsSource).ToTable();
                    var drs = dt.AsEnumerable().Where(jP => jP.Field<Int64>("LockID") == lockId
                                                            && jP.Field<bool>("Visible")
                                                            && jP.Field<bool>("IsClosed"));

                    if (drs.Any())
                    {
                        var rowNumber = dt.Rows.IndexOf(drs.First());
                        var drv = ((DataView) ClosingJournalDataGrid.ItemsSource)[rowNumber];
                        ClosingJournalDataGrid.SelectedItem = drv;
                        ClosingJournalDataGrid.ScrollIntoView(drv);
                    }

                }

                return;
            }

            var rows = _prc.Locks.Table.Select(string.Format("LockID = {0}", lockId));
            if (!rows.Any()) return;

            var lockRow = rows.First();
            if (!string.IsNullOrEmpty(lockRow["LockNotes"].ToString()))
            {
                CloseAppBar();

                ClosingNotePopup.IsOpen = true;
                closingDoorNoteTextBlock.Text =
                    _prc.Locks.Table.Select("LockID = " + ClosingDoorComboBox.SelectedValue)[0]["LockNotes"].ToString();
                ApplyEffect();
            }
            else
            {
                AddRowsToTables();

                ClosingDoorComboBox.SelectedIndex = ClosingDoorComboBox.Items.Count != 0 ? -1 : 0;
                ClosingStampNumber.Text = string.Empty;
            }
        }

        private void AddRowsToTables()
        {
            var lockID = Convert.ToInt32(ClosingDoorComboBox.SelectedValue);
            var sealNumber = ClosingStampNumber.Text;
            var date = App.BaseClass.GetDateFromSqlServer();

            var journalId = _prc.AddNewJournalRow(date, DBNull.Value, lockID, sealNumber, _currentWorker, 2, -1);

            _prc.AddNewJournalRow(DBNull.Value, date, lockID, DBNull.Value,
                DBNull.Value, 1, journalId);
            AdministrationClass.AddNewAction(1);

            if (ClosingJournalDataGrid.Items.Count != 0)
            {
                ClosingJournalDataGrid.SelectedIndex = 0;
                ClosingJournalDataGrid.ScrollIntoView(ClosingJournalDataGrid.SelectedItem);
            }
        }

        private void OnOpeningCheckButtonClick(object sender, RoutedEventArgs e)
        {
            if (OpeningStampNumber.Text == string.Empty) return;

            var openingRow = OpeningJournalDataGrid.SelectedItem as DataRowView;
            if(openingRow == null) return;

            if (openingRow.Row["JournalID"] == DBNull.Value || openingRow.Row["Date"] != DBNull.Value) return;

            var sealNumber = OpeningStampNumber.Text;
            var closingDate = Convert.ToDateTime(openingRow.Row["ClosingDate"]);
            var lockId = Convert.ToInt32(openingRow.Row["LockID"]);

            var dataRows =
                _prc.JournalProductionsTable.AsEnumerable()
                    .Where(
                        r =>
                            r.Field<object>("Date") != null && r.Field<DateTime>("Date") == closingDate &&
                            r.Field<Int64>("LockID") == lockId && r.Field<Int64>("LockStatusID") == 2);

            if (dataRows.Any() && dataRows.First()["SealNumber"].ToString() != sealNumber)
            {
                var result =
                    MetroMessageBox.Show("Номер пломбы не совпадает с номер пломбы, введённым при закрытии двери. \n" +
                                         "Проверте правильность введения пломбы. В случае неправильного ввода номера пломбы при закрытии, укажите это в примечании. \n\n" +
                                         "Желаете продолжить?", string.Empty, MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
            }

            var journalId = Convert.ToInt32(openingRow.Row["JournalID"]);
            var date = App.BaseClass.GetDateFromSqlServer();

            if (dataRows.Count() != 0)
            {
                var closingRow = dataRows.Last();
                var closingJournalId = Convert.ToInt32(closingRow["JournalID"]);
                _prc.OpenClosingRow(closingJournalId);
            }
            _prc.AddInfoToOpeningRow(journalId, date, sealNumber, _currentWorker);
            AdministrationClass.AddNewAction(2);

            OpeningStampNumber.Text = string.Empty;
        }


        private void OnClosingJournalDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClosingJournalDataGrid.SelectedItem == null) return;
            var lockID = Convert.ToInt32(((DataRowView)ClosingJournalDataGrid.SelectedItem).Row["LockID"]);
            var closingDate = Convert.ToDateTime(((DataRowView)ClosingJournalDataGrid.SelectedItem).Row["Date"]);

            SelectJournalOpeningRow(lockID, closingDate);
        }

        private void SelectJournalOpeningRow(int lockID, DateTime closingDate)
        {
            var dt = ((DataView) OpeningJournalDataGrid.ItemsSource).ToTable();
            var filter = "LockID = " + lockID + " AND ClosingDate = '" + closingDate.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                         "'";
            var dr = dt.Select(filter);
            if (dr.Length == 0)
            {
                OpeningJournalDataGrid.SelectedIndex = -1;
                return;
            }

            var rowNumber = dt.Rows.IndexOf(dr[0]);
            var drv = ((DataView) OpeningJournalDataGrid.ItemsSource)[rowNumber];
            OpeningJournalDataGrid.SelectionChanged -= OnOpeningJournalDataGridSelectionChanged;
            OpeningJournalDataGrid.SelectedItem = drv;
            OpeningJournalDataGrid.ScrollIntoView(drv);
            OpeningJournalDataGrid.SelectionChanged += OnOpeningJournalDataGridSelectionChanged;
        }

        private void OnOpeningJournalDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpeningJournalDataGrid.SelectedItem == null) return;
            var lockID = Convert.ToInt32(((DataRowView) OpeningJournalDataGrid.SelectedItem).Row["LockID"]);
            var date = Convert.ToDateTime(((DataRowView) OpeningJournalDataGrid.SelectedItem).Row["ClosingDate"]);
            SelectJournalClosingRow(lockID, date);
        }

        private void SelectJournalClosingRow(int lockID, DateTime date)
        {
            var dt = ((DataView)ClosingJournalDataGrid.ItemsSource).ToTable();
            var filter = "LockID = " + lockID + " AND Date = '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" +
                         " AND LockStatusID = 2";
            var dr = dt.Select(filter);
            if (dr.Length == 0)
            {
                ClosingJournalDataGrid.SelectedIndex = -1;
                return;
            }

            var rowNumber = dt.Rows.IndexOf(dr[0]);
            var drv = ((DataView)ClosingJournalDataGrid.ItemsSource)[rowNumber];
            ClosingJournalDataGrid.SelectionChanged -= OnClosingJournalDataGridSelectionChanged;
            ClosingJournalDataGrid.SelectedItem = drv;
            ClosingJournalDataGrid.ScrollIntoView(drv);
            ClosingJournalDataGrid.SelectionChanged += OnClosingJournalDataGridSelectionChanged;
        }

        private void OnShowJournalButtonClick(object sender, RoutedEventArgs e)
        {
            FillJournalGrids(Convert.ToInt32(JournalYearComboBox.SelectedItem), JournalMonthComboBox.SelectedIndex + 1);
        }

        private void GuardClosingCheckButton_Checked(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView)ClosingJournalDataGrid.SelectedItem;
            if (drv.Row["JournalID"] == DBNull.Value)
            {
                ((CheckBox) sender).IsChecked = false;
                return;
            }
            var journalID = Convert.ToInt32(drv.Row["JournalID"]);
            _prc.CheckGuard(journalID, _currentWorker);
        }

        private void GuardOpeningCheckButton_Checked(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView) OpeningJournalDataGrid.SelectedItem;
            if (drv.Row["JournalID"] == DBNull.Value || drv.Row["Date"] == DBNull.Value)
            {
                ((CheckBox) sender).IsChecked = false;
                return;
            }
            var journalID = Convert.ToInt32(drv.Row["JournalID"]);
            _prc.CheckGuard(journalID, _currentWorker);
        }



        private void ClosingWorkerNotePopup_Opened(object sender, EventArgs e)
        {
            //((Popup)sender).PlacementRectangle = new Rect(-230, 0, 0, 22);

            var drv = (DataRowView)ClosingJournalDataGrid.SelectedItem;

            var border = ((Popup)sender).Child as Border;
            if (border == null) return;

            var grid = border.Child as Grid;
            if (grid == null) return;

            var textBox = grid.Children[0] as TextBox;
            var saveButton = grid.Children[1] as Button;

            textBox.IsReadOnly = true;
            saveButton.Visibility = Visibility.Hidden;

            if (drv.Row["WorkerID"] == DBNull.Value) return;
            if (Convert.ToInt32(drv.Row["WorkerID"]) == _currentWorker)
            {
                textBox.IsReadOnly = false;
                saveButton.Visibility = Visibility.Visible;
            }

            if (drv.Row["WorkerNotes"] != DBNull.Value)
                textBox.Text = drv.Row["WorkerNotes"].ToString();
        }

        private void ClosingWorkerSaveNote_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView)ClosingJournalDataGrid.SelectedItem;
            if (drv.Row["WorkerID"] == DBNull.Value) return;

            var grid = VisualTreeHelper.GetParent((Button)sender) as Grid;
            var textBox = grid.Children[0] as TextBox;

            var date = Convert.ToDateTime(drv.Row["Date"]);
            _prc.AddWorkerNote(date, textBox.Text);
            var popup = ((Button)sender).Tag as Popup;
            if (popup != null) popup.IsOpen = false;
        }

        private void OnRowMenuContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ClosingJournalDataGrid.SelectedItem == null || ClosingJournalDataGrid.Items.Count == 0) return;

            var cellsPresenter = (DataGridCellsPresenter)e.Source;

            var drv = cellsPresenter.Item as DataRowView;
            if (drv == null) return;

            var date = Convert.ToDateTime(drv.Row["Date"]);

            var openedRows =
                _prc.JournalProductionsTable.Select("ClosingDate = '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

            if (openedRows.Any() && openedRows.First()["Date"] != DBNull.Value ||
                openedRows.First()["GuardID"] != DBNull.Value)
                e.Handled = true;

            var workerId = Convert.ToInt64(drv["WorkerID"]);
            if (!AdministrationClass.IsAdministrator && AdministrationClass.CurrentWorkerId != workerId)
                e.Handled = true;
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (ClosingJournalDataGrid.SelectedItem == null || ClosingJournalDataGrid.Items.Count == 0) return;

            var drv = ClosingJournalDataGrid.SelectedItem as DataRowView;
            if (drv != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить выбранную запись?", string.Empty,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var journalId = Convert.ToInt64(drv["JournalID"]);
                    if (ClosingJournalDataGrid.SelectedItem == null) return;
                    _prc.DeleteClosingDoor(journalId);
                }
            }
        }

        private void OpeningWorkerNotePopup_Opened(object sender, EventArgs e)
        {
            var border = ((Popup)sender).Child as Border;
            if (border == null) return;

            var grid = border.Child as Grid;
            if (grid == null) return;

            var textBox = grid.Children[0] as TextBox;
            var saveButton = grid.Children[1] as Button;

            textBox.IsReadOnly = true;
            saveButton.Visibility = Visibility.Hidden;

            var drv = (DataRowView)OpeningJournalDataGrid.SelectedItem;

            if (drv.Row["WorkerID"] == DBNull.Value) return;
            if (Convert.ToInt32(drv.Row["WorkerID"]) == _currentWorker)
            {
                textBox.IsReadOnly = false;
                saveButton.Visibility = Visibility.Visible;
            }

            if (drv.Row["WorkerNotes"] != DBNull.Value)
                textBox.Text = drv.Row["WorkerNotes"].ToString();
        }

        private void OpeningWorkerSaveNote_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView) OpeningJournalDataGrid.SelectedItem;
            if (drv.Row["WorkerID"] == DBNull.Value) return;

            var grid = VisualTreeHelper.GetParent((Button) sender) as Grid;
            var textBox = grid.Children[0] as TextBox;

            var date = Convert.ToDateTime(drv.Row["Date"]);
            _prc.AddWorkerNote(date, textBox.Text);
            var popup = ((Button) sender).Tag as Popup;
            if (popup != null) popup.IsOpen = false;
        }


        private void ClosingGuardPopup_Opened(object sender, EventArgs e)
        {
            //((Popup)sender).PlacementRectangle = new Rect(-230, 0, 0, 22);

            var drv = (DataRowView)ClosingJournalDataGrid.SelectedItem;

            var border = ((Popup)sender).Child as Border;
            if (border == null) return;

            var grid = border.Child as Grid;
            if (grid == null) return;

            var textBox = grid.Children[0] as TextBox;
            var saveButton = grid.Children[1] as Button;

            textBox.IsReadOnly = true;
            saveButton.Visibility = Visibility.Hidden;

            if (drv.Row["GuardID"] == DBNull.Value) return;
            if (Convert.ToInt32(drv.Row["GuardID"]) == _currentWorker)
            {
                textBox.IsReadOnly = false;
                saveButton.Visibility = Visibility.Visible;
            }

            if (drv.Row["GuardNotes"] != DBNull.Value)
                textBox.Text = drv.Row["GuardNotes"].ToString();
        }

        private void ClosingGuardSaveNotes_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView)ClosingJournalDataGrid.SelectedItem;
            if (drv.Row["GuardID"] == DBNull.Value) return;

            var grid = VisualTreeHelper.GetParent((Button)sender) as Grid;
            var textBox = grid.Children[0] as TextBox;

            var journalID = Convert.ToInt32(drv.Row["JournalID"]);
            _prc.AddGuardNote(journalID, textBox.Text);
            var popup = ((Button)sender).Tag as Popup;
            if (popup != null) popup.IsOpen = false;
        }

        private void OpeningGuardPopup_Opened(object sender, EventArgs e)
        {
            //((Popup)sender).PlacementRectangle = new Rect(-230, 0, 0, 22);

            var border = ((Popup) sender).Child as Border;
            if (border == null) return;

            var grid = border.Child as Grid;
            if (grid == null) return;

            var textBox = grid.Children[0] as TextBox;
            var saveButton = grid.Children[1] as Button;

            textBox.IsReadOnly = true;
            saveButton.Visibility = Visibility.Hidden;

            var drv = (DataRowView) OpeningJournalDataGrid.SelectedItem;

            if (drv.Row["GuardID"] == DBNull.Value) return;
            if (Convert.ToInt32(drv.Row["GuardID"]) == _currentWorker)
            {
                textBox.IsReadOnly = false;
                saveButton.Visibility = Visibility.Visible;
            }

            if (drv.Row["GuardNotes"] != DBNull.Value)
                textBox.Text = drv.Row["GuardNotes"].ToString();
        }

        private void OpeningGuardSaveNotes_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView) OpeningJournalDataGrid.SelectedItem;
            if (drv.Row["GuardID"] == DBNull.Value) return;

            var grid = VisualTreeHelper.GetParent((Button) sender) as Grid;
            var textBox = grid.Children[0] as TextBox;

            var journalID = Convert.ToInt32(drv.Row["JournalID"]);
            _prc.AddGuardNote(journalID, textBox.Text);
            var popup = ((Button) sender).Tag as Popup;
            if (popup != null) popup.IsOpen = false;
        }

        private void CancelNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var popup = ((Button)sender).Tag as Popup;
            if (popup != null) popup.IsOpen = false;
        }


        private void LockRedactorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LockRedactorListBox.SelectedItem == null)
            {
                LockNameTextBox.Text = string.Empty;
                LockNoteTextBox.Text = string.Empty;
                NullSourceText.Visibility = Visibility.Visible;
                DoorPhoto.Source = null;
                LastLockChengesTextBlock.Text = "Последнее редактирование: --:--:--";
                return;
            }

            var drv = (DataRowView) LockRedactorListBox.SelectedItem;
            LockNameTextBox.Text = drv.Row["LockName"].ToString();
            LockNoteTextBox.Text = drv.Row["LockNotes"].ToString();

            if (drv.Row["LockPhoto"] == DBNull.Value | drv.Row["LockPhoto"] == null)
            {
                NullSourceText.Visibility = Visibility.Visible;
                DoorPhoto.Source = null;
            }
            else
            {
                NullSourceText.Visibility = Visibility.Hidden;
                DoorPhoto.Source = AdministrationClass.ObjectToBitmapImage(drv.Row["LockPhoto"]);
            }


            LastLockChengesTextBlock.Text = "Последнее редактирование: " +
                                            _iConverter.Convert(drv.Row["EditingWorkerID"], "ShortName") + "  " +
                                            drv.Row["EditingDate"];
        }

        private void SaveLockChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (LockNameTextBox.Text == string.Empty || LockRedactorListBox.SelectedItem == null) return;

            object lockPhoto = null;
            if (DoorPhoto.Source != null)
                lockPhoto = AdministrationClass.BitmapImageToByte((BitmapImage) DoorPhoto.Source);

            _prc.Locks.SaveChanges(Convert.ToInt32(LockRedactorListBox.SelectedValue), LockNameTextBox.Text,
                                  LockNoteTextBox.Text, lockPhoto,
                                  _currentWorker, App.BaseClass.GetDateFromSqlServer());
            AdministrationClass.AddNewAction(6);

            var drv = (DataRowView)LockRedactorListBox.SelectedItem;
            LastLockChengesTextBlock.Text = "Последнее редактирование: " +
                                            _iConverter.Convert(drv.Row["EditingWorkerID"], "ShortName") + "  " +
                                            drv.Row["EditingDate"];

        }

        private void AddLockButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddLockTextBox.Text == string.Empty || AddLockTextBox.Text == "Добавить...") return;

            var result = _prc.Locks.AddNewLock(AddLockTextBox.Text, _currentWorker, App.BaseClass.GetDateFromSqlServer());
            AdministrationClass.AddNewAction(5);

            if (result)
            {
                var dt = ((DataView) LockRedactorListBox.ItemsSource).ToTable();
                var dr = dt.Select("LockName = '" + AddLockTextBox.Text + "'");
                if(dr.Length != 0)
                {
                    var number = dt.Rows.IndexOf(dr[0]);
                    LockRedactorListBox.SelectedIndex = number;
                }               
            }

            AddLockTextBox.Text = string.Empty;
        }

        private void DeleteLockButton_Click(object sender, RoutedEventArgs e)
        {
            if (LockRedactorListBox.SelectedItem == null) return;

            var result = MetroMessageBox.Show("Вы действительно хотите удалить выбранную дверь?", string.Empty,
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _prc.Locks.DeleteLock(Convert.ToInt32(LockRedactorListBox.SelectedValue));
                AdministrationClass.AddNewAction(6);
            }
        }


        private void agreeClosingButton_Click(object sender, RoutedEventArgs e)
        {
            AddRowsToTables();

            ClosingDoorComboBox.SelectedIndex = -1;

            if (ClosingDoorComboBox.Items.Count != 0)
                ClosingDoorComboBox.SelectedIndex = 0;
            ClosingStampNumber.Text = string.Empty;

            dontAgreeClosingButton_Click(null, null);
            ProdRoomsActualStatusItemsControl.Items.Refresh();
        }

        private void dontAgreeClosingButton_Click(object sender, RoutedEventArgs e)
        {
            ClearEffect();
            ClosingNotePopup.IsOpen = false;
        }


        private void ActionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionListBox.SelectedItem == null)
            {
                ActionStatusComboBox.SelectedIndex = -1;
                ActionNumberTextBox.Text = string.Empty;
                ActionTextBox.Text = string.Empty;
                LastActionChangesTextBlock.Text = "Последнее редактирование: --:--:--";
                return;
            }

            var DRV = (DataRowView)ActionListBox.SelectedItem;
            ActionStatusComboBox.SelectedIndex = Convert.ToInt32(DRV.Row["ActionStatus"]) - 1;
            ActionNumberTextBox.Text = DRV.Row["ActionNumber"].ToString();
            ActionTextBox.Text = DRV.Row["ActionText"].ToString();
            LastActionChangesTextBlock.Text = "Последнее редактирование: " +
                                            _iConverter.Convert(DRV.Row["EditingWorkerID"], "ShortName") + "  " +
                                            DRV.Row["EditingDate"];
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            int actionNumber;
            var succes = int.TryParse(AddActionNumberTextBox.Text, out actionNumber);
            if (!succes) return;

            if (string.IsNullOrEmpty(AddActionPopupTextBox.Text) || AddActionPopupTextBox.Text == "Добавить..." ||
                AddActionNumberTextBox.Text == string.Empty || AddActionStatusComboBox.SelectedItem == null) return;

            var result = _prc.Actions.AddNewAction(Convert.ToInt32(AddActionStatusComboBox.SelectedValue), actionNumber,
                                                   AddActionPopupTextBox.Text,
                                                   _currentWorker, App.BaseClass.GetDateFromSqlServer());
            AdministrationClass.AddNewAction(75);

            if (result)
            {               
                var dt = ((DataView)((ICollectionView) ActionListBox.ItemsSource).SourceCollection).ToTable();
                var dr =
                    dt.Select("ActionNumber = " + actionNumber + " AND ActionStatus = " +
                              AddActionStatusComboBox.SelectedValue);
                if (dr.Length != 0)
                {
                    var number = dt.Rows.IndexOf(dr[0]);
                    ActionListBox.SelectedIndex = number;
                }
                AddActionStatusComboBox.SelectedIndex = AddActionStatusComboBox.Items.Count != 0 ? 0 : -1;
                AddActionNumberTextBox.Text = string.Empty;
                AddActionPopupTextBox.Text = string.Empty;
            }
        }

        private void SaveChangesActionButton_Click(object sender, RoutedEventArgs e)
        {
            int actionNumber;
            var succes = int.TryParse(ActionNumberTextBox.Text, out actionNumber);
            if (!succes) return;

            if (ActionNumberTextBox.Text == string.Empty || ActionTextBox.Text == string.Empty ||
                ActionListBox.SelectedItem == null || ActionStatusComboBox.SelectedItem == null) return;

            _prc.Actions.SaveChanges(Convert.ToInt32(ActionListBox.SelectedValue),
                                    Convert.ToInt32(ActionStatusComboBox.SelectedValue), actionNumber,
                                    ActionTextBox.Text, _currentWorker, App.BaseClass.GetDateFromSqlServer());
            AdministrationClass.AddNewAction(76);

            var drv = (DataRowView) ActionListBox.SelectedItem;
            LastActionChangesTextBlock.Text = "Последнее редактирование: " +
                                              _iConverter.Convert(drv.Row["EditingWorkerID"], "ShortName") + "  " +
                                              drv.Row["EditingDate"];
        }

        private void DeleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionListBox.SelectedItem == null) return;

            var result = MetroMessageBox.Show("Вы действительно хотите удалить выбранное действие?", string.Empty,
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _prc.Actions.DeleteAction(Convert.ToInt32(ActionListBox.SelectedValue));
                AdministrationClass.AddNewAction(76);
            }
        }



        private void CancelActionNote_Click(object sender, RoutedEventArgs e)
        {
            ClearEffect();
            ActionNotePopup.IsOpen = false;
        }

        private void ExportJournalButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel.GenerateProdRoomsActualStatusReport();
        }

        private void ShowConfirmPopup_Opened(object sender, EventArgs e)
        {
            var drv = (DataRowView) ClosingJournalDataGrid.SelectedItem;
            if (drv == null) return;

            var border = ((Popup) sender).Child as Border;
            var row = (RowDefinition) ((Popup) sender).FindName("ConfirmRow");
            var warningText = (TextBlock) ((Popup) sender).FindName("WarningTextBlock");
            var itemsControl = (ItemsControl)((Popup)sender).FindName("ConfirmItemsControl");
            var textBox = (TextBox) ((Popup) sender).FindName("confirmTextBox");

            row.Height = new GridLength(30);
            border.IsEnabled = true;
            warningText.Height = 0;
            if (drv.Row["JournalID"] == DBNull.Value)
            {
                border.IsEnabled = false;
                warningText.Height = 15;
                warningText.Text = "Для подтверждения необходимо сохранить текущую запись";
                return;
            }
            var date = drv.Row["Date"];
            var rows = _prc.JournalProductionsTable.AsEnumerable().
                Where(
                    r =>
                        r.Field<object>("ClosingDate") != null &&
                        r.Field<DateTime>("ClosingDate") == Convert.ToDateTime(date)).
                Cast<DataRow>();
            if (!rows.Any() || rows.First()["Date"] != DBNull.Value)
            {
                border.IsEnabled = false;
                row.Height = new GridLength(0);
                warningText.Height = 15;
                warningText.Text = "Данная пломба сорвана";
            }

            textBox.Text = drv.Row["SealNumber"].ToString();

            _prc.ConfirmDataTable.DefaultView.RowFilter = "JournalID = " + drv.Row["JournalID"];
            _prc.ConfirmDataTable.DefaultView.Sort = "Date DESC";
            itemsControl.ItemsSource = _prc.ConfirmDataTable.DefaultView;
        }

        private void AddConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView) ClosingJournalDataGrid.SelectedItem;
            if (drv == null) return;

            var currentTime = App.BaseClass.GetDateFromSqlServer();
            if (_prc.ConfirmDataTable.Select("JournalID = " + drv.Row["JournalID"] + " AND WorkerID = " + _currentWorker)
                .Any(
                    dataRow =>
                        Convert.ToDateTime(dataRow["Date"]).DayOfYear == currentTime.DayOfYear))
            {
                return;
            }

            _prc.AddConfirmRow(Convert.ToInt32(drv.Row["JournalID"]), _currentWorker,
                ((Button) sender).Tag.ToString(), currentTime);

            var toggleButton = (ToggleButton) ((Button) sender).FindName("ShowConfirmToggleButton");
            if (toggleButton != null)
                toggleButton.Content = _prc.ConfirmDataTable.Select("JournalID = " + drv.Row["JournalID"]).Count();

            var popup = (Popup) ((Button) sender).FindName("ShowConfirmPopup");
            if (popup != null) popup.IsOpen = false;
        }

        private void ChangeDoorPhotoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (LockRedactorListBox.SelectedItem == null) return;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.* "
            };

            if (dlg.ShowDialog() == true)
            {
                const int newX = 200;

                System.Drawing.Bitmap resizedImage;
                using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(dlg.FileName))
                {
                    double x = originalImage.Width;
                    double y = originalImage.Height;
                    var newYd = newX * (y / x);
                    var newY = Convert.ToInt32(newYd);

                    resizedImage = new System.Drawing.Bitmap(originalImage, newX, newY);
                }

                DoorPhoto.Source = AdministrationClass.BitmapToBitmapImage(resizedImage);
                NullSourceText.Visibility = Visibility.Hidden;

                resizedImage.Dispose();
            }
        }

        private void ClosingSealNumerSearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OpeningSealNumerSearchTextBox.Text = string.Empty;
            FilterClosingView();
        }

        private void OpeningSealNumerSearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ClosingSealNumerSearchTextBox.Text = string.Empty;
            FilterOpeningView();
        }

        private void DeleteDoorPhotoButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoorPhoto.Source = null;
            NullSourceText.Visibility = Visibility.Visible;
        }

        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }



        private void OnLockPhotoToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var lockPhoto = sender as Image;
            if (lockPhoto == null) return;

            var toolTipImage = lockPhoto.FindName("LockImageToolTip") as Image;
            if(toolTipImage != null)
            {
                toolTipImage.Source = lockPhoto.Source;
            }
        }

        private void OnCloseDoorButtonClick(object sender, RoutedEventArgs e)
        {
            var closeDoorButton = sender as Button;
            if (closeDoorButton == null) return;

            var lockItemBorder = closeDoorButton.FindName("LockItemBorder") as Border;
            if (lockItemBorder == null) return;

            var lockStatusView = lockItemBorder.DataContext as DataRowView;
            if (lockStatusView == null) return;

            var lockId = Convert.ToInt32(lockStatusView["LockID"]);
            var lockName = lockStatusView["LockName"].ToString();
            var lockNotes = lockStatusView["LockNotes"].ToString();

            var sealNumberTextBox = closeDoorButton.FindName("SealNumberTextBox") as FAIIControlLibrary.CustomControls.WatermarkTextBox;
            if (sealNumberTextBox == null)
                return;

            var sealNumber = sealNumberTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sealNumber))
            {
                MessageBox.Show("Необходимо ввести номер пломбы закрытия! Поле не может быть пустым.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(MessageBox.Show(string.Format("Вы действительно хотите закрыть дверь/помещение '{0}'", lockName), "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_prc.JournalProductionsTable.AsEnumerable().Any(jP => jP.Field<Int64>("LockID") == lockId
                                                                      && jP.Field<bool>("Visible")
                                                                      && jP.Field<bool>("IsClosed")))
            {
                if (MessageBox.Show(
                    "Данная дверь уже отмечена как закрытая. Необходимо открыть дверь в подмодуле 'Журнал'. " +
                    "\nПерейти к препятствующей записи?", "Предупреждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var dt = ((DataView)ClosingJournalDataGrid.ItemsSource).ToTable();
                    var drs = dt.AsEnumerable().Where(jP => jP.Field<Int64>("LockID") == lockId
                                                            && jP.Field<bool>("Visible")
                                                            && jP.Field<bool>("IsClosed"));

                    if (drs.Any())
                    {
                        var rowNumber = dt.Rows.IndexOf(drs.First());
                        var drv = ((DataView)ClosingJournalDataGrid.ItemsSource)[rowNumber];
                        ClosingJournalDataGrid.SelectedItem = drv;
                        ClosingJournalDataGrid.ScrollIntoView(drv);
                    }

                }

                return;
            }

            if (!string.IsNullOrEmpty(lockNotes))
            {
                ClosingNotePopup.IsOpen = true;
                closingDoorNoteTextBlock.Text =
                    _prc.Locks.Table.Select("LockID = " + ClosingDoorComboBox.SelectedValue)[0]["LockNotes"].ToString();
                ApplyEffect();
            }
            else
            {
                var date = App.BaseClass.GetDateFromSqlServer();
                var journalId = _prc.AddNewJournalRow(date, DBNull.Value, lockId, sealNumber, _currentWorker, 2, -1);
                AdministrationClass.AddNewAction(72);

                _prc.AddNewJournalRow(DBNull.Value, date, lockId, DBNull.Value, DBNull.Value, 1, journalId);
                ProdRoomsActualStatusItemsControl.Items.Refresh();
            }
        }

        private void OnOpenDoorButtonClick(object sender, RoutedEventArgs e)
        {
            var openDoorButton = sender as Button;
            if (openDoorButton == null) return;

            var lockItemBorder = openDoorButton.FindName("LockItemBorder") as Border;
            if (lockItemBorder == null) return;

            var lockStatusView = lockItemBorder.DataContext as DataRowView;
            if (lockStatusView == null) return;

            var lockId = Convert.ToInt32(lockStatusView["LockID"]);
            var lockName = lockStatusView["LockName"].ToString();

            var needOpeningRows = _prc.JournalProductionsTable.AsEnumerable()
                .Where(r => r.Field<Int64>("LockID") == lockId && r.Field<Int64>("LockStatusID") == 1 && r.Field<bool>("IsClosed"));
            if (!needOpeningRows.Any())
            {
                MessageBox.Show(string.Format("Дверь '{0}' уже открыта! Проверьте данные во вкладке 'Журнал'"), "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var lastOpeningRow = needOpeningRows.Last();
            var closingDate = Convert.ToDateTime(lastOpeningRow["ClosingDate"]);

            var dataRows =
                _prc.JournalProductionsTable.AsEnumerable()
                    .Where(
                        r =>
                            r.Field<object>("Date") != null && r.Field<DateTime>("Date") == closingDate &&
                            r.Field<Int64>("LockID") == lockId && r.Field<Int64>("LockStatusID") == 2);

            if (!dataRows.Any())
            {
                MessageBox.Show("Не возможно найти запись по закрытию данной двери. Попробуйте произвести открытие через вкладку 'Журнал'", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var journalId = Convert.ToInt32(lastOpeningRow["JournalID"]);
            var date = App.BaseClass.GetDateFromSqlServer();

            var closingRow = dataRows.Last();
            var closingJournalId = Convert.ToInt32(closingRow["JournalID"]);
            var sealNumber = closingRow["SealNumber"].ToString();

            if (MessageBox.Show(string.Format("Номер пломбы: {0}. Проверьте соответствие номера пломбы, в случае обратного - внесите данные через вкладку 'Журнал' и укажите правильный номер пломбы. " +
                "\nВы действительно хотите открыть дверь/помещение '{1}'", sealNumber, lockName), "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _prc.OpenClosingRow(closingJournalId);
            _prc.AddInfoToOpeningRow(journalId, date, sealNumber, _currentWorker);
            AdministrationClass.AddNewAction(71);

            ProdRoomsActualStatusItemsControl.Items.Refresh();
        }

        public void SetRaportComboBoxItemsAvailability()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            CloseRaportComboBoxItem.PreviewMouseLeftButtonDown -= OnCloseRaportItemMouseDown;
            CloseRaportComboBoxItem.PreviewMouseLeftButtonDown -= OnDeleteCloseRaportItemMouseDown;

            OpenRaportComboBoxItem.PreviewMouseLeftButtonDown -= OnOpenRaportItemMouseDown;
            OpenRaportComboBoxItem.PreviewMouseLeftButtonDown -= OnDeleteOpenRaportItemMouseDown;

            var hasWorkerCurrentCloseReport = _prc.HasWorkerCurrentReport(currentWorkerId, currentDate, 2);
            var hasWorkerCurrentOpenReport = _prc.HasWorkerCurrentReport(currentWorkerId, currentDate, 1);

            if(hasWorkerCurrentCloseReport)
            {
                CloseRaportComboBoxItem.Content = "Удалить текущий рапорт по закрытию";
                CloseRaportComboBoxItem.PreviewMouseLeftButtonDown += OnDeleteCloseRaportItemMouseDown;
            }
            else
            {
                CloseRaportComboBoxItem.Content = "По закрытию";
                CloseRaportComboBoxItem.PreviewMouseLeftButtonDown += OnCloseRaportItemMouseDown;
            }

            if (hasWorkerCurrentOpenReport)
            {
                OpenRaportComboBoxItem.Content = "Удалить текущий рапорт по открытию";             
                OpenRaportComboBoxItem.PreviewMouseLeftButtonDown += OnDeleteOpenRaportItemMouseDown;
            }
            else
            {
                OpenRaportComboBoxItem.Content = "По открытию";
                OpenRaportComboBoxItem.PreviewMouseLeftButtonDown += OnOpenRaportItemMouseDown;
            }
        }

        private void OnCloseRaportItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            var raportPage = new RaportPage(2);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.ShowCatalogGrid(raportPage, "Составить рапорт по закрытию помещений");
        }

        private void OnOpenRaportItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            var raportPage = new RaportPage(1);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.ShowCatalogGrid(raportPage, "Составить рапорт по открытию помещений");
        }

        private void OnDeleteCloseRaportItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();

            if (MessageBox.Show(string.Format("Вы действительно хотите удалить Ваш рапорт по закрытию помещений за {0:dd.MM.yyyy}", currentDate), "Удаление", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            _prc.DeleteWorkerCurrentReport(currentWorkerId, currentDate, 2);

            SetRaportComboBoxItemsAvailability();
        }

        private void OnDeleteOpenRaportItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();

            if (MessageBox.Show(string.Format("Вы действительно хотите удалить Ваш рапорт по открытию помещений за {0:dd.MM.yyyy}", currentDate), "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            _prc.DeleteWorkerCurrentReport(currentWorkerId, currentDate, 1);

            SetRaportComboBoxItemsAvailability();
        }

        private void OnLockSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var locksView = ProdRoomsActualStatusItemsControl.ItemsSource as DataView;
            if (locksView == null) return;

            var searchCriteria = LockSearchTextBox.Text.Trim();
            if(string.IsNullOrEmpty(searchCriteria))
            {
                locksView.RowFilter = "IsEnable = 'True'";
                return;
            }

            locksView.RowFilter = string.Format("IsEnable = 'True' AND LockName LIKE '{0}*'", searchCriteria);
        }


        private void OnLoadWeekendResponsiblesButtonClick(object sender, RoutedEventArgs e)
        {
            WeekendResponsiblesDataGrid.ItemsSource = null;
            FillWeekendResponsiblesDataGrids(Convert.ToInt32(WeekendResponsiblesYearComboBox.SelectedItem), WeekendResponsiblesMonthComboBox.SelectedIndex + 1);
            WeekendResponsiblesDataGrid.ItemsSource = _prc.GetWeekendsResponsiblesView();
        }

        private void OnWeekendResponsiblesDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(bool))
            {
                ((DataGridCheckBoxColumn)e.Column).ElementStyle = Resources["OpeningCheckBox"] as Style;
                ((DataGridCheckBoxColumn)e.Column).EditingElementStyle = Resources["EditingOpeningCheckBox"] as Style;
            }
        }

        private void OnWeekendResponsiblesDataGridAutoGeneratedColumns(object sender, EventArgs e)
        {
            SetWeekendResponsiblesCellStyle(Convert.ToInt32(WeekendResponsiblesYearComboBox.SelectedItem), WeekendResponsiblesMonthComboBox.SelectedIndex + 1);
        }

        private void SetWeekendResponsiblesCellStyle(int selectedYear, int selectedMonth)
        {
            WeekendResponsiblesDataGrid.Columns[1].Visibility = Visibility.Hidden;
            var daysCount = DateTime.DaysInMonth(selectedYear, selectedMonth);
            for (var i = 1; i < daysCount + 1; i++)
            {
                var date = new DateTime(selectedYear, selectedMonth, i);
                SetStyleForCells(ref WeekendResponsiblesDataGrid, i + 1, date);
            }
        }


        private void OnWeekendResponsiblesGroupComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekendResponsiblesGroupComboBox.Items.Count == 0) return;

            var unitToFactory = ((DataRowView)WeekendResponsiblesGroupComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            if (unitToFactory == string.Empty || !Convert.ToBoolean(unitToFactory))
            {
                WeekendResponsiblesFactoryColumn.Width = new GridLength(0);
                WeekendResponsiblesNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(WeekendResponsiblesGroupComboBox.SelectedValue));
                WeekendResponsiblesNameComboBox.SelectedIndex = 0;
            }
            else
            {
                WeekendResponsiblesFactoryColumn.Width = new GridLength(1, GridUnitType.Auto);
                WeekendResponsiblesFactoryComboBox.SelectedIndex = 0;
                OnWeekendResponsiblesFactoryComboBoxSelectionChanged(null, null);
            }
        }

        private void OnWeekendResponsiblesFactoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekendResponsiblesFactoryComboBox.Items.Count == 0) return;

            WeekendResponsiblesNameComboBox.ItemsSource = WorkersGroupFilter(Convert.ToInt32(WeekendResponsiblesGroupComboBox.SelectedValue),
                                                                 Convert.ToInt32(WeekendResponsiblesFactoryComboBox.SelectedValue));
            WeekendResponsiblesNameComboBox.SelectedIndex = 0;
        }

        private void OnAddWeekendResponsibleButtonClick(object sender, RoutedEventArgs e)
        {
            var worker = WeekendResponsiblesNameComboBox.SelectedItem as DataRowView;
            if (worker == null) return;

            var workerId = Convert.ToInt64(worker["WorkerID"]);
            _prc.AddWorkerToWeekendTimeSheet(workerId);
        }

        private void OnEditWeekendResponsiblesButtonClick(object sender, RoutedEventArgs e)
        {
            if (_editingWeekendResponsiblesMode)
            {
                CloseAppBar();
                
                WeekendResponsiblesEditRow.Height = new GridLength(0);
                EditWeekendResponsiblesButton.Content = "Перейти к редактированию";
                WeekendResponsiblesDataGrid.IsReadOnly = true;

                ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Visible;
                SaveWeekendResponsiblesButton.Visibility = Visibility.Collapsed;
                _editingWeekendResponsiblesMode = false;
            }
            else
            {
                CloseAppBar();
                WeekendResponsiblesEditRow.Height = new GridLength(40);
                EditWeekendResponsiblesButton.Content = "Выйти из редактирования";
                WeekendResponsiblesDataGrid.IsReadOnly = false;

                ExportWeekendResponsiblesToExcelButton.Visibility = Visibility.Collapsed;
                SaveWeekendResponsiblesButton.Visibility = Visibility.Visible;
                _editingWeekendResponsiblesMode = true;
            }
        }

        private void OnSaveWeekendResponsiblesButtonClick(object sender, RoutedEventArgs e)
        {
            var editingWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _prc.SaveWeekendTimeSheet(editingWorkerId, currentDate);
            AdministrationClass.AddNewAction(77);
            OnEditWeekendResponsiblesButtonClick(null, null);
        }

        private void OnWeekendResponsiblesDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rowView = WeekendResponsiblesDataGrid.SelectedItem as DataRowView;
            if (rowView == null)
            {
                WeekendResponsiblesEditingInfoTextBlock.Text = "--:--:--";
                return;
            }

            var workerId = Convert.ToInt64(rowView["WorkerID"]);
            var editInfo = _prc.GetWeekendEditingInfo(workerId) as DataRow;
            if(editInfo == null)
            {
                WeekendResponsiblesEditingInfoTextBlock.Text = "--:--:--";
                return;
            }

            WeekendResponsiblesEditingInfoTextBlock.Text = 
                string.Format("{0} {1:dd.MM.yyyy HH:mm}", _iConverter.Convert(Convert.ToInt64(editInfo["EditingWorkerID"]), "ShortName"), Convert.ToDateTime(editInfo["EditingDate"]));
        }

        private void OnNameWeekendResponsibleCellMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_editingWeekendResponsiblesMode) return;

            var weekendResponsible = WeekendResponsiblesDataGrid.SelectedItem as DataRowView;
            if (weekendResponsible == null) return;

            var workerId = Convert.ToInt64(weekendResponsible["WorkerID"]);
            if (MessageBox.Show(string.Format("Вы действительно хотите удалить сотрудника '{0}' из графика", new IdToNameConverter().Convert(workerId, "FullName")),
                "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _prc.DeleteWorkerFromWeekendTimeSheet(workerId);
        }

        private void OnExportWeekendResponsiblesReportButtonClick(object sender, RoutedEventArgs e)
        {
            var weekendResponsiblesTimeSheetDataView = WeekendResponsiblesDataGrid.ItemsSource as DataView;
            ExportToExcel.GenerateWeekendResponsiblesReport(weekendResponsiblesTimeSheetDataView);
        }


        private void SetResponsibleArriveButtonEnable()
        {
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var hasWorkerNearlyArrive = _prc.HasWorkerNearlyArrive(currentWorkerId, currentDate, TimeSpan.FromHours(2));

            ResponsibleArriveButton.Visibility = hasWorkerNearlyArrive
                ? Visibility.Collapsed
                : Visibility.Visible;

            DeleteResponsibleArriveButton.Visibility = hasWorkerNearlyArrive
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnResponsibleArriveButtonClick(object sender, RoutedEventArgs e)
        {
            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var additionalInfo = ResponsibleArriveAdditionalInfoTextBox.Text;
            var hasWorkerNearlyArrive = _prc.HasWorkerNearlyArrive(currentWorkerId, currentDate, TimeSpan.FromHours(2));
            if(hasWorkerNearlyArrive)
            {
                MessageBox.Show("Вы уже отмечали недавний выход на работу! Отметка станет доступной по истечению некоторого времени.", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _prc.AddResponsibleArrive(currentWorkerId, currentDate, additionalInfo);
            AdministrationClass.AddNewAction(78);

            ResponsibleArriveAdditionalInfoTextBox.Text = string.Empty;
            SetResponsibleArriveButtonEnable();
        }

        private void OnDeleteResponsibleArriveButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить Вашу последнюю отметку о выходе на работу в праздничный/выходной день?",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var currentWorkerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var responsibleArriveId = _prc.GetWorkerNearlyArrive(currentWorkerId, currentDate, TimeSpan.FromHours(2));
            if (responsibleArriveId == null) return;

            _prc.DeleteResponsibleArrive(responsibleArriveId.Value);

            SetResponsibleArriveButtonEnable();
        }
    }
}
