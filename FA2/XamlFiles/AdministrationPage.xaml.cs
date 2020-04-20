using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FA2.ChildPages.AdministrationPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FAIIControlLibrary;
using System.Windows.Media.Animation;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для ProgramJournalPage.xaml
    /// </summary>
    public partial class AdministrationPage
    {
        private bool _firstTimePageRun = true;
        private bool _fullAccess;
        private AdministrationClass _adc;
        private StaffClass _sc;
        private DateTime _currentDate;
        private MainWindow _mw;

        private TimeControlClass _tcc;
        private CatalogClass _cc;
        private DateTime _fromDateTime;
        private DateTime _toDateTime;
        private int _workerGroupId;
        private double _currentProductivity;
        private string _currentMeasureUnitName = string.Empty;
        private double _currentVclp;

        // Views
        private BindingListCollectionView _programEntryView;
        private DataView _browsingModulesView;
        private DataView _workInModulesView;

        private enum Mode
        {
            Statistics,
            WorkerAccess,
            ProgrammReport,
            TimeTrackingRedactor
        }

        private enum StatisticMode
        {
            ProgrammEntry,
            ModuleEntry
        }

        private Mode _workingMode;
        private StatisticMode _statisticMode;
        private DataRowView _previewsSelectedWorker;
        private bool _savingNeeded;

        private bool _isEditingRecord = false;

        public AdministrationPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
            FromDatePicker.SelectedDateChanged += (o, args) => _fromDateTime = (DateTime)FromDatePicker.SelectedDate;
            ToDatePicker.SelectedDateChanged += (o, args) => _toDateTime = (DateTime)ToDatePicker.SelectedDate;
            FromDatePicker.SelectedDate = DateTime.Now.AddDays(-7);
            ToDatePicker.SelectedDate = DateTime.Now;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.Administration);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.Administration);
             
            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) => FillData();
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           BindingData();
                                                           BindingFilters();
                                                           TimeTrackingDataGridSettings();
                                                           SetAccessEnable(_fullAccess);

                                                           ProgramEntryJournalRadioButton.IsChecked = true;
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

                _tcc.FillShifts(_fromDateTime, _toDateTime);
                _tcc.FillTimeTracking(_fromDateTime, _toDateTime);
                WorkersNameListBox_SelectionChanged(WorkersNameListBox, null);
            }
        }

        private void FillData()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetAdministrationClass(ref _adc);
            _currentDate = App.BaseClass.GetDateFromSqlServer();
            var dateFrom = _currentDate.Subtract(new TimeSpan(10, 0, 0, 0)).Date;
            var dateTo = _currentDate;
            _adc.Fill(dateFrom, dateTo);
            _adc.FillProgrammReport(dateTo.Subtract(TimeSpan.FromDays(5)), dateTo);

            App.BaseClass.GetTimeControlClass(ref _tcc);
            App.BaseClass.GetCatalogClass(ref _cc);
            _tcc.FillShifts(_fromDateTime, _toDateTime);
            _tcc.FillTimeTracking(_fromDateTime, _toDateTime);
        }

        private void BindingData()
        {
            _workInModulesView = _adc.JournalWorkInModulesView;
            _workInModulesView.RowFilter = "JBMID = -1";
            WorkInModulesDataGrid.ItemsSource = _workInModulesView;

            _browsingModulesView = _adc.JournalBrowsingModulesView;
            _browsingModulesView.RowFilter = "JPEID = -1";
            BrowsingModulesDataGrid.ItemsSource = _browsingModulesView;

            _programEntryView = new BindingListCollectionView(_adc.JournalProgramEntryView);
            if (_programEntryView.GroupDescriptions != null)
                _programEntryView.GroupDescriptions.Add(new PropertyGroupDescription("EntryDate",
                    new GroupByDateConverter()));
            _programEntryView.CustomFilter = "WorkerID = -1";
            ProgramEntriesListBox.ItemsSource = _programEntryView;

            var programmReportView = _adc.ReportTable.AsDataView();
            programmReportView.Sort = "Date ASC";
            ProgrammReportDataGrid.ItemsSource = programmReportView;

            //AccessGroupStructureListBox.ItemsSource = _adc.AccessGroupStructureView;

            AccessGroupsComboBox.ItemsSource = _adc.AccessGroupsView;

            InfoByModulesListBox.ItemsSource = _adc.ModulesView;


            ShiftsListBox.SelectionChanged -= ShiftsListBox_SelectionChanged;
            ShiftsListBox.ItemsSource = _tcc.GetShifts();
            ShiftsListBox.SelectionChanged += ShiftsListBox_SelectionChanged;

            TimeTrackingDataGrid.ItemsSource = _tcc.GetTimeTracking();

            #region OperationsCatalog

            TrackingGroupsComboBox.SelectionChanged -= TrackingGroupsComboBox_SelectionChanged;
            TrackingGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            TrackingGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            TrackingGroupsComboBox.ItemsSource = _cc.GetWorkersGroups();
            TrackingGroupsComboBox.SelectionChanged += TrackingGroupsComboBox_SelectionChanged;

            TrackingFactoriesComboBox.SelectionChanged -= TrackingFactoriesComboBox_SelectionChanged;
            TrackingFactoriesComboBox.DisplayMemberPath = "FactoryName";
            TrackingFactoriesComboBox.SelectedValuePath = "FactoryID";
            TrackingFactoriesComboBox.ItemsSource = _cc.GetFactories();
            TrackingFactoriesComboBox.SelectedIndex = 0;
            TrackingFactoriesComboBox.SelectionChanged += TrackingFactoriesComboBox_SelectionChanged;

            WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;
            WorkUnitsListBox.SelectedValuePath = "WorkUnitID";
            WorkUnitsListBox.ItemsSource = _cc.GetWorkUnits();
            WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

            WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;
            WorkSectionsListBox.SelectedValuePath = "WorkSectionID";
            WorkSectionsListBox.ItemsSource = _cc.GetWorkSections();
            WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;

            WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;
            WorkSubSectionsListBox.SelectedValuePath = "WorkSubsectionID";
            WorkSubSectionsListBox.ItemsSource = _cc.GetWorkSubsections();
            WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;

            OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
            OperationsListBox.SelectedValuePath = "WorkOperationID";
            OperationsListBox.ItemsSource = _cc.GetWorkOperations();
            OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

            TrackingGroupsComboBox.SelectedIndex = 1;

            DoubleTimeSet.TotalTimeChanged += DoubleTimeSet_TotalTimeChanged;

            TrackingGroupsComboBox_SelectionChanged(null, null);

            #endregion
        }

        private void BindingFilters()
        {
            var substrCurDate = _currentDate.Subtract(new TimeSpan(10, 0, 0, 0));
            DateFromPicker.SelectedDate = substrCurDate.Date;
            DateToPicker.SelectedDate = _currentDate;

            ProgramReportDateFromPicker.SelectedDate = _currentDate.Subtract(TimeSpan.FromDays(5)).Date;
            ProgramReportDateToPicker.SelectedDate = _currentDate;

            WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;
            //WorkersNameListBox.ItemsSource = _sc.GetStaffPersonalInfo();
            WorkersNameListBox.SelectedValuePath = "WorkerID";
            WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;

            if (WorkersNameListBox.HasItems)
                WorkersNameListBox.SelectedIndex = 0;

            WorkersGroupsComboBox.SelectionChanged -= WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            WorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkersGroupsComboBox.SelectedValue = 2;
            WorkersGroupsComboBox.SelectionChanged += WorkersGroupsComboBox_SelectionChanged;

            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.DisplayMemberPath = "FactoryName";
            FactoriesComboBox.SelectedValuePath = "FactoryID";
            FactoriesComboBox.ItemsSource = _sc.GetFactories();
            FactoriesComboBox.SelectedIndex = 0;
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

            FactoriesComboBox_SelectionChanged(null, null);
        }

        private void SetAccessEnable(bool fullAccess)
        {
            ModulesButton.IsEnabled = fullAccess;
            AccessGroupsRedactorButton.IsEnabled = fullAccess;
            ActionsRedactorButton.IsEnabled = fullAccess;
            WorkerRightsRadioButton.IsEnabled = fullAccess;
            ProgrammReportRadioButton.IsEnabled = fullAccess;
            TimeTrackingRedactorRadioButton.IsEnabled = fullAccess;
        }


        #region Filters

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DateFromPicker.SelectedDate == null || DateToPicker.SelectedDate == null) return;

            _adc.Fill(DateFromPicker.SelectedDate.Value, DateToPicker.SelectedDate.Value.AddDays(1));

            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void FilterWorkersByProfessions()
        {
            if (WorkersGroupsComboBox.SelectedItem == null || FactoriesComboBox.SelectedItem == null) return;

            // Get view of workers, filtered by professions.
            WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;
            WorkersNameListBox.ItemsSource =
                _sc.FilterWorkers(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                                            Convert.ToInt32(FactoriesComboBox.SelectedValue)).DefaultView;

            if (WorkersNameListBox.ItemsSource != null)
                ((DataView)WorkersNameListBox.ItemsSource).RowFilter = "AvailableInList = 'TRUE'";
            if (WorkersNameListBox.Items.Count != 0)
                WorkersNameListBox.SelectedIndex = 0;
            WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;
            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void WorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkersByProfessions();
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkersByProfessions();
        }

        private void WorkersNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workerId = WorkersNameListBox.SelectedValue != null
                ? Convert.ToInt32(WorkersNameListBox.SelectedValue)
                : -1;

            if (_workingMode == Mode.Statistics)
            {
                if (_statisticMode == StatisticMode.ProgrammEntry)
                {
                    _programEntryView.CustomFilter = string.Format("WorkerID = {0}", workerId);

                    if (ProgramEntriesListBox.Items.Count != 0)
                        ProgramEntriesListBox.SelectedIndex = 0;
                }
                else
                {
                    OnInfoByModulesListBoxSelectionChanged(null, null);
                }
            }
            else if (_workingMode == Mode.WorkerAccess)
            {
                if (_savingNeeded)
                {
                    if (
                        MetroMessageBox.Show("Информация не сохранена! Желаете продолжить?", "Предупреждение",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;
                        WorkersNameListBox.SelectedItem = _previewsSelectedWorker;
                        WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;
                        return;
                    }
                }

                _savingNeeded = false;
                _previewsSelectedWorker = WorkersNameListBox.SelectedItem as DataRowView;
                SaveWorkerAccessButton.IsEnabled = false;
                CancelSaveWorkerAccessButton.IsEnabled = false;

                if (WorkersNameListBox.SelectedItem == null)
                {
                    WorkerAccessListBox.ItemsSource = null;
                    return;
                }

                var view = GetModulesAccessForWorker(workerId).AsDataView();
                view.Sort = "Access DESC, ModuleName";
                WorkerAccessListBox.ItemsSource = view;

                SelectAccessGroup();
            }
            else if(_workingMode == Mode.TimeTrackingRedactor)
            {
                if (ShiftsListBox.ItemsSource == null) return;

                if ((WorkersNameListBox.Items.Count == 0) || (WorkersNameListBox.SelectedValue == null))
                {
                    ((DataView)ShiftsListBox.ItemsSource).RowFilter = "WorkerID = -1";

                    ShiftsListBox_SelectionChanged(null, null);
                    return;
                }

                ((DataView)ShiftsListBox.ItemsSource).RowFilter = "WorkerID = " + workerId;

                if (ShiftsListBox.Items.Count != 0)
                {
                    ShiftsListBox.SelectedIndex = 0;
                    ShiftsListBox.BringIntoView();
                }
            }
        }

        #endregion


        private void ProgramEntriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var jpeid = ProgramEntriesListBox.SelectedValue != null
                ? Convert.ToInt32(ProgramEntriesListBox.SelectedValue)
                : -1;
            _browsingModulesView.RowFilter = string.Format("JPEID = {0}", jpeid);

            if (BrowsingModulesDataGrid.Items.Count != 0)
                BrowsingModulesDataGrid.SelectedIndex = 0;
        }

        private void BrowsingModulesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var jbmid = BrowsingModulesDataGrid.SelectedValue != null
                ? Convert.ToInt32(BrowsingModulesDataGrid.SelectedValue)
                : -1;
            _workInModulesView.RowFilter = string.Format("JBMID = {0}", jbmid);

            if (WorkInModulesDataGrid.Items.Count != 0)
                WorkInModulesDataGrid.SelectedIndex = 0;
        }



        #region Modes switch

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }

        private void ProgramEntryJournalToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Setting enabled for toggle buttons
            AddButtonRow.Height = new GridLength(0);
            WorkerFilterColumn.Width = new GridLength(1, GridUnitType.Auto);
            FilterWorkInModulesByWorkerRow.Height = new GridLength(1, GridUnitType.Auto);
            WorkersNameListBox.SelectionMode = SelectionMode.Single;
            TabControl.SelectedIndex = 0;
            CloseAppBar();

            _workingMode = Mode.Statistics;
            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void WorkerRightsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Setting enabled for toggle buttons
            AddButtonRow.Height = new GridLength(1, GridUnitType.Auto);
            WorkerFilterColumn.Width = new GridLength(1, GridUnitType.Auto);
            FilterWorkInModulesByWorkerRow.Height = new GridLength(0);
            WorkersNameListBox.SelectionMode = SelectionMode.Extended;
            TabControl.SelectedIndex = 1;
            CloseAppBar();

            _workingMode = Mode.WorkerAccess;
            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void OnProgrammReportRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            // Setting enabled for toggle buttons
            AddButtonRow.Height = new GridLength(0);
            WorkerFilterColumn.Width = new GridLength(0);
            FilterWorkInModulesByWorkerRow.Height = new GridLength(0);
            TabControl.SelectedIndex = 2;
            CloseAppBar();

            _workingMode = Mode.ProgrammReport;
        }

        private void OnTimeReackingRedactorRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            // Setting enabled for toggle buttons
            AddButtonRow.Height = new GridLength(0);
            WorkerFilterColumn.Width = new GridLength(1, GridUnitType.Auto);
            FilterWorkInModulesByWorkerRow.Height = new GridLength(0);
            TabControl.SelectedIndex = 3;
            CloseAppBar();

            _workingMode = Mode.TimeTrackingRedactor;

            WorkersNameListBox_SelectionChanged(WorkersNameListBox, null);
        }

        #endregion


        private void ModulesButton_Click(object sender, RoutedEventArgs e)
        {
            var modulesRedactor = new ModulesRedactor();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
                _mw.ShowCatalogGrid(modulesRedactor, "Модули");

            CloseAppBar();
        }

        private void AccessGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccessGroupsComboBox.SelectedItem == null) return;

            var accessGroupId = Convert.ToInt32(AccessGroupsComboBox.SelectedValue);
            var view = GetModulesAccessForAccessGroup(accessGroupId).AsDataView();
            view.Sort = "Access DESC, ModuleName";
            WorkerAccessListBox.ItemsSource = view;

            _savingNeeded = true;
            SaveWorkerAccessButton.IsEnabled = true;
            CancelSaveWorkerAccessButton.IsEnabled = true;

            //if (AccessGroupsComboBox.SelectedItem == null)
            //{
            //    AvailableModulesItemsControl.ItemsSource = null;
            //    AccessGroupStructureListBox.ItemsSource = null;
            //    return;
            //}

            //var accessGroupId = Convert.ToInt32(AccessGroupsComboBox.SelectedValue);
            //var availableModules = _adc.GetAvailableModulesForAccessGroup(accessGroupId).AsDataView();
            //availableModules.Sort = "ModuleName";
            //AvailableModulesItemsControl.ItemsSource = availableModules;

            //AccessGroupStructureListBox.ItemsSource = GetSortedWorkersInAccessGroup(accessGroupId);
        }

        //public void SelectAccessGroupItem(int accessGroupId)
        //{
        //    AccessGroupsComboBox.SelectedValue = accessGroupId;
        //    AccessGroupsComboBox_SelectionChanged(null, null);
        //}

        //private void AddWorkerToGroupButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (WorkersNameListBox.SelectedItems.Count == 0 || AccessGroupsComboBox.SelectedItem == null)
        //        return;

        //    var workerIds = from workersView in WorkersNameListBox.SelectedItems.Cast<DataRowView>()
        //                    select workersView["WorkerID"];
        //    var workerIdsList = workerIds.Cast<Int64>().ToList();
        //    var accessGroupId = Convert.ToInt32(AccessGroupsComboBox.SelectedValue);
        //    _adc.AddWorkersToGroup(workerIdsList, accessGroupId);

        //    //Refill items source
        //    //AccessGroupStructureListBox.ItemsSource = GetSortedWorkersInAccessGroup(accessGroupId);
        //    WorkersNameListBox.Items.Refresh();
        //}

        //private void DeleteWorkerFromGroupButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (AccessGroupStructureListBox.SelectedItems.Count == 0 || AccessGroupsComboBox.SelectedItem == null)
        //        return;

        //    var accessGroupId = Convert.ToInt32(AccessGroupsComboBox.SelectedValue);
        //    var message = string.Format("Вы действительно хотите удалить {0} работника(ов) из группы '{1}'",
        //        AccessGroupStructureListBox.SelectedItems.Count,
        //        new IdToAccessGroupNameConverter().
        //        Convert(accessGroupId, typeof(string), string.Empty, new System.Globalization.CultureInfo("ru-RU")));

        //    if(MetroMessageBox.Show(message, "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        var groupsIds = from accessGroupStructureView in AccessGroupStructureListBox.SelectedItems.Cast<DataRowView>()
        //                        select accessGroupStructureView["AccessGroupStructureID"];
        //        var groupsIdsList = groupsIds.Cast<Int64>().ToList();
        //        _adc.DeleteWorkersFromGroup(groupsIdsList);

        //        //Refill items source
        //        AccessGroupStructureListBox.ItemsSource = GetSortedWorkersInAccessGroup(accessGroupId);
        //        WorkersNameListBox.Items.Refresh();
        //    }
        //}


        //private void WorkersRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (WorkersNameListBox.SelectedItem == null) return;

        //    var workerId = Convert.ToInt32(WorkersNameListBox.SelectedValue);
        //    var accessGroup = _adc.AccessGroupStructureTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerID") == workerId);
        //    if (accessGroup.Count() == 0) return;
        //    var accessGroupId = Convert.ToInt32(accessGroup.First()["AccessGroupId"]);
        //    SelectAccessGroupItem(accessGroupId);

        //    // Navigate to access mode
        //    if (Convert.ToBoolean(ProgramEntryJournalToggleButton.IsChecked))
        //        WorkerRightsToggleButton.IsChecked = true;
        //}

        private void ActionsRedactorButton_Click(object sender, RoutedEventArgs e)
        {
            var actionsRedactor = new ActionsRedactor();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
                _mw.ShowCatalogGrid(actionsRedactor, "Действия");

            CloseAppBar();
        }

        private DataView GetSortedWorkersInAccessGroup(int accessGroupId)
        {
            //Result table
            var table = new DataTable();
            table.Columns.Add("AccessGroupStructureID", typeof(Int64));
            table.Columns.Add("WorkerID", typeof(Int64));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("AccessGroupID", typeof(Int64));

            var groupStructure = _adc.AccessGroupStructureTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId);
            if (!groupStructure.Any()) return table.DefaultView;
            var sortedTable = groupStructure.Join(_sc.StaffPersonalInfoDataTable.AsEnumerable(),
                outer => outer["WorkerID"],
                inner => inner["WorkerID"],
                (outer, inner) => 
                {
                    var newRow = table.NewRow();
                    newRow["AccessGroupStructureID"] = outer["AccessGroupStructureID"];
                    newRow["WorkerID"] = outer["WorkerID"];
                    newRow["Name"] = inner["Name"];
                    newRow["AccessGroupID"] = outer["AccessGroupID"];
                    return newRow;
                }).CopyToDataTable();

            //Sort collection
            var sortedCollection = sortedTable.DefaultView;
            sortedCollection.Sort = "Name";

            return sortedCollection;
        }

        //private void ExportToExcelAccessGroup_Click(object sender, RoutedEventArgs e)
        //{
        //    ExportToExcel.GenerateAccessGroupReport(ref ExportToExcelAccessGroup);
        //}

        private DataTable GetModulesAccessForWorker(int workerId)
        {
            //Result table
            var table = new DataTable();
            table.Columns.Add("ModuleID", typeof(Int64));
            table.Columns.Add("ModuleName", typeof(string));
            table.Columns.Add("ModuleDescription", typeof(string));
            table.Columns.Add("ModuleIcon", typeof(byte[]));
            table.Columns.Add("ModuleColor", typeof(string));
            table.Columns.Add("FullAccess", typeof(bool));
            table.Columns.Add("Access", typeof (bool));

            var availables = _adc.WorkersAccessTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerID") == workerId);

            var modules = _adc.ModulesTable.AsEnumerable().
                Where(m => m.Field<bool>("IsEnabled") && m.Field<bool>("ShowInFileStorage"));
            foreach (var module in modules)
            {
                var availableModule =
                    availables.FirstOrDefault(a => a.Field<Int64>("ModuleID") == Convert.ToInt64(module["ModuleID"]));

                var newRow = table.NewRow();
                newRow["ModuleID"] = module["ModuleID"];
                newRow["ModuleName"] = module["ModuleName"];
                newRow["ModuleDescription"] = module["ModuleDescription"];
                newRow["ModuleIcon"] = module["ModuleIcon"];
                newRow["ModuleColor"] = module["ModuleColor"];
                if (availableModule != null)
                {
                    newRow["FullAccess"] = availableModule["FullAccess"];
                    newRow["Access"] = availableModule["Access"];
                }
                else
                {
                    newRow["FullAccess"] = false;
                    newRow["Access"] = false;
                }

                table.Rows.Add(newRow);
            }

            return table;
        }

        private DataTable GetModulesAccessForAccessGroup(int accessGroupId)
        {
            //Result table
            var table = new DataTable();
            table.Columns.Add("ModuleID", typeof(Int64));
            table.Columns.Add("ModuleName", typeof(string));
            table.Columns.Add("ModuleDescription", typeof(string));
            table.Columns.Add("ModuleIcon", typeof(byte[]));
            table.Columns.Add("ModuleColor", typeof(string));
            table.Columns.Add("FullAccess", typeof(bool));
            table.Columns.Add("Access", typeof(bool));

            var availables = _adc.AvailableModulesTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId);
            var modules = _adc.ModulesTable.AsEnumerable().
                Where(m => m.Field<bool>("IsEnabled") && m.Field<bool>("ShowInFileStorage"));

            foreach (var module in modules)
            {
                var availableModule =
                    availables.FirstOrDefault(a => a.Field<Int64>("ModuleID") == Convert.ToInt64(module["ModuleID"]));

                var newRow = table.NewRow();
                newRow["ModuleID"] = module["ModuleID"];
                newRow["ModuleName"] = module["ModuleName"];
                newRow["ModuleDescription"] = module["ModuleDescription"];
                newRow["ModuleIcon"] = module["ModuleIcon"];
                newRow["ModuleColor"] = module["ModuleColor"];
                if (availableModule != null)
                {
                    newRow["FullAccess"] = availableModule["FullAccess"];
                    newRow["Access"] = true;
                }
                else
                {
                    newRow["FullAccess"] = false;
                    newRow["Access"] = false;
                }

                table.Rows.Add(newRow);
            }

            return table;
        }

        private void OnFullAccessToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return;

            SelectAccessGroup();

            _savingNeeded = true;
            SaveWorkerAccessButton.IsEnabled = true;
            CancelSaveWorkerAccessButton.IsEnabled = true;
        }

        private void OnFullAccessToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return;

            SelectAccessGroup();

            _savingNeeded = true;
            SaveWorkerAccessButton.IsEnabled = true;
            CancelSaveWorkerAccessButton.IsEnabled = true;
        }

        private void OnAccessToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return;

            SelectAccessGroup();

            //WorkerAccessListBox.Items.Refresh();
            _savingNeeded = true;
            SaveWorkerAccessButton.IsEnabled = true;
            CancelSaveWorkerAccessButton.IsEnabled = true;
        }

        private void OnAccessToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;
            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return;

            SelectAccessGroup();

            //WorkerAccessListBox.Items.Refresh();
            _savingNeeded = true;
            SaveWorkerAccessButton.IsEnabled = true;
            CancelSaveWorkerAccessButton.IsEnabled = true;
        }

        private void SelectAccessGroup()
        {
            var checkSum = CalculateCheckSum();
            var rows =
                _adc.AccessGroupsTable.AsEnumerable()
                    .Where(r => r.Field<object>("CheckSum") != null && r.Field<Int32>("CheckSum") == checkSum);
            if (rows.Any())
            {
                var accessGroup = rows.First();
                var accessGroupId = Convert.ToInt32(accessGroup["AccessGroupID"]);
                AccessGroupsComboBox.SelectionChanged -= AccessGroupsComboBox_SelectionChanged;
                AccessGroupsComboBox.SelectedValue = accessGroupId;
                AccessGroupsComboBox.SelectionChanged += AccessGroupsComboBox_SelectionChanged;
            }
            else
            {
                AccessGroupsComboBox.SelectedItem = null;
            }
        }

        private int CalculateCheckSum()
        {
            var checkSum = 0;
            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return 0;

            foreach (var row in workerAccess.Table.AsEnumerable().Where(r => r.Field<bool>("Access")))
            {
                var moduleId = Convert.ToInt32(row["ModuleID"]);
                checkSum += moduleId*10;
                var fullAccess = Convert.ToBoolean(row["FullAccess"]);
                if (fullAccess)
                    checkSum += 1;
            }

            return checkSum;
        }

        private void OnCancelSaveWorkerAccessButtonClick(object sender, RoutedEventArgs e)
        {
            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void OnSaveWorkerAccessButtonClick(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItem == null) return;

            var workerAccess = WorkerAccessListBox.ItemsSource as DataView;
            if (workerAccess == null) return;

            var workerId = Convert.ToInt32(WorkersNameListBox.SelectedValue);

            // Get base available modules for worker
            var availableModules =
                _adc.WorkersAccessTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerID") == workerId);

            if (availableModules.Any())
            {
                foreach (var row in availableModules.CopyToDataTable().AsEnumerable())
                {
                    var moduleId = Convert.ToInt32(row["ModuleID"]);

                    // Delete all non access modules
                    if (workerAccess.Table.AsEnumerable()
                        .Any(r => !r.Field<bool>("Access") && r.Field<Int64>("ModuleID") == moduleId))
                        _adc.DeleteWorkerAccess(workerId, moduleId);
                    else
                    {
                        if (workerAccess.Table.AsEnumerable().All(r => r.Field<Int64>("ModuleID") != moduleId))
                            continue;

                        var baseFullAccess = Convert.ToBoolean(row["FullAccess"]);
                        var fullAccess =
                            workerAccess.Table.AsEnumerable()
                                .First(r => r.Field<Int64>("ModuleID") == moduleId)
                                .Field<bool>("FullAccess");

                        // Set full access if old and new values are different
                        if (baseFullAccess != fullAccess)
                            _adc.SetFullAccessForWorker(workerId, moduleId, fullAccess);
                    }
                }
            }

            foreach (var row in workerAccess.Table.AsEnumerable()
                .Where(r =>
                    r.Field<bool>("Access") &&
                    availableModules.AsEnumerable().All(a => a.Field<Int64>("ModuleID") != r.Field<Int64>("ModuleID"))))
            {
                var moduleId = Convert.ToInt32(row["ModuleID"]);
                var fullAccess = Convert.ToBoolean(row["FullAccess"]);
                _adc.AddWorkerAccess(workerId, moduleId, fullAccess);
            }

            _adc.RefillWorkerAccess();



            // Add worker to access group, if access is equal
            if (AccessGroupsComboBox.SelectedItem != null)
            {
                var accessGroupId = Convert.ToInt32(AccessGroupsComboBox.SelectedValue);
                _adc.AddWorkersToGroup(new long[] {workerId}, accessGroupId);
            }
            else
            {
                _adc.DeleteWorkersFromGroup(new long[] {workerId});
            }


            AdministrationClass.AddNewAction(99);
            _savingNeeded = false;
            SaveWorkerAccessButton.IsEnabled = false;
            CancelSaveWorkerAccessButton.IsEnabled = false;
            WorkersNameListBox.Items.Refresh();
        }

        private void OnAccessGroupsRedactorButtonClick(object sender, RoutedEventArgs e)
        {
            var accessRedactor = new AccessGroupsRedactor();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
                _mw.ShowCatalogGrid(accessRedactor, "Группы доступа");

            CloseAppBar();
        }

        private void OnWorkInModulesSwitchToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            _statisticMode = StatisticMode.ModuleEntry;

            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void OnWorkInModulesSwitchToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            _statisticMode = StatisticMode.ProgrammEntry;

            WorkersNameListBox_SelectionChanged(null, null);
        }

        private DataView GetModuleEntryStattistic(int moduleId, long? workerId)
        {
            var table = new DataTable();
            table.Columns.Add("ActionTypeID", typeof(long));
            table.Columns.Add("ActionDate", typeof(DateTime));
            table.Columns.Add("EntryDate", typeof(DateTime));
            table.Columns.Add("WorkerID", typeof(long));

            var moduleEntry =
                _adc.JournalBrowsingModulesTable.AsEnumerable().Where(m => m.Field<Int64>("ModuleID") == moduleId)
                .Join(_adc.JournalProgramEntryTable.AsEnumerable(), outer => outer["JPEID"], inner => inner["JPEID"],
                    (outer, inner) =>
                    {
                        return new
                        {
                            EntryDate = Convert.ToDateTime(outer["EntryDate"]),
                            WorkerId = Convert.ToInt64(inner["WorkerID"]),
                            JBMID = outer["JBMID"]
                        };
                    });

            if(workerId.HasValue)
            {
                moduleEntry = moduleEntry.Where(m => m.WorkerId == workerId);
            }

            if (!moduleEntry.Any()) return table.AsDataView();

            var workerModuleEntriesQuery = _adc.JournalWorkInModulesTable.AsEnumerable()
                .Join(moduleEntry, outer => outer["JBMID"], inner => inner.JBMID,
                (outer, inner) =>
                {
                    var row = table.NewRow();
                    row["ActionTypeID"] = outer["ActionTypeID"];
                    row["ActionDate"] = outer["ActionDate"];
                    row["EntryDate"] = inner.EntryDate;
                    row["WorkerID"] = inner.WorkerId;
                    return row;

                });

            var workerModuleEntries = workerModuleEntriesQuery as IList<DataRow> ?? workerModuleEntriesQuery.ToList();
            if (!workerModuleEntries.Any()) return table.AsDataView();

            var workerModuleEntriesTable = workerModuleEntries.CopyToDataTable();
            var view = workerModuleEntriesTable.AsDataView();
            return view;
        }


        private void OnApplyProgramReportFilterButtonClick(object sender, RoutedEventArgs e)
        {
            if(ProgramReportDateFromPicker.SelectedDate.HasValue && ProgramReportDateToPicker.SelectedDate.HasValue)
            {
                _adc.FillProgrammReport(ProgramReportDateFromPicker.SelectedDate.Value, ProgramReportDateToPicker.SelectedDate.Value);
            }
        }


        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }


        #region TimeTracking

        private void ApplyDateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            _tcc.FillShifts(_fromDateTime, _toDateTime);
            _tcc.FillTimeTracking(_fromDateTime, _toDateTime);
        }

        private void ShiftsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeTrackingDataGrid.ItemsSource == null) return;

            if (ShiftsListBox.SelectedItems.Count != 1)
            {
                ((DataView)TimeTrackingDataGrid.ItemsSource).RowFilter = "TimeSpentAtWorkID= -1";
                ((DataView)TimeTrackingDataGrid.ItemsSource).RowFilter = "TimeSpentAtWorkID= -1";
                return;
            }

            DataRow dr = ((DataRowView)ShiftsListBox.SelectedItem).Row;

            ((DataView)TimeTrackingDataGrid.ItemsSource).RowFilter = "TimeSpentAtWorkID=" +
                                                                            dr["TimeSpentAtWorkID"] + " AND DeleteRecord = FALSE";

            var totalTime = _tcc.CountingTotalTime((DataView)TimeTrackingDataGrid.ItemsSource);
            TotalTimeLabel.Content = totalTime;
        }

        private void TrackingGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackingGroupsComboBox.Items.Count == 0) return;

            var unitToFactory = ((DataRowView)TrackingGroupsComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            OpacityAnimation(TrackingFactoryStackPanel, unitToFactory != string.Empty && Convert.ToBoolean(unitToFactory));

            _workerGroupId = Convert.ToInt32(TrackingGroupsComboBox.SelectedValue) - 1;

            if (_workerGroupId > _cc.WorkersGroupsTitlesDataTable.Rows.Count)
                _workerGroupId = 2;

            WorkUnitsLabel.Content = _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["UnitsTitle"].ToString();
            WorkSectionsLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["SectionsTitle"].ToString();
            WorkSubSectionsLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["SubsectionsTitle"].ToString();
            WorkOperationsNameLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["OperationsTitle"].ToString();

            if (!Convert.ToBoolean(unitToFactory))
            {
                WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;

                ((DataView)(WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                                        TrackingGroupsComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
                CountUnitLabel.Content = ((DataView)(WorkUnitsListBox.ItemsSource)).Count;
            }

            else
            {
                TrackingFactoriesComboBox.SelectionChanged -= TrackingFactoriesComboBox_SelectionChanged;
                TrackingFactoriesComboBox.SelectionChanged += TrackingFactoriesComboBox_SelectionChanged;

                if (TrackingFactoriesComboBox.Items.Count == 0) return;

                ((DataView)(WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                                        TrackingGroupsComboBox.SelectedValue +
                                                                        " AND FactoryID = " +
                                                                        TrackingFactoriesComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
            }

            CountUnitLabel.Content = ((DataView)(WorkUnitsListBox.ItemsSource)).Count;
        }

        private void OpacityAnimation(UIElement control, bool visible)
        {
            if ((control.Opacity == 1) && visible) return;
            if ((control.Opacity == 0) && !visible) return;

            var da = new DoubleAnimation { From = visible ? 0 : 1, To = visible ? 1 : 0 };

            da.Completed += (sender, e) => control.IsEnabled = visible;

            da.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            control.BeginAnimation(OpacityProperty, da);
        }

        private void TrackingFactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var unitToFactory = ((DataRowView)TrackingGroupsComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            if (Convert.ToBoolean(unitToFactory))
            {
                WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;

                TrackingFactoriesComboBox.SelectionChanged -= TrackingFactoriesComboBox_SelectionChanged;
                TrackingFactoriesComboBox.SelectionChanged += TrackingFactoriesComboBox_SelectionChanged;

                if (TrackingFactoriesComboBox.Items.Count == 0) return;

                ((DataView)(WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                       TrackingGroupsComboBox.SelectedValue + " AND FactoryID = " +
                                                       TrackingFactoriesComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
            }

            CountUnitLabel.Content = ((DataView)(WorkUnitsListBox.ItemsSource)).Count;
        }

        private void WorkUnitsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsListBox.Items.Count != 0 && WorkUnitsListBox.SelectedItem != null)
            {
                WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;

                ((DataView)(WorkSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkUnitID=" +
                                                                           WorkUnitsListBox.SelectedValue;

                WorkSectionsListBox.SelectedIndex = 0;
                WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;
            }
            else
            {
                WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;

                ((DataView)(WorkSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkUnitID=" + -1;

                WorkSectionsListBox.SelectedIndex = 0;
                WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;
            }

            CountSectionsLabel.Content = ((DataView)(WorkSectionsListBox.ItemsSource)).Count;

            WorkSectionsListBox_SelectionChanged(null, null);
        }

        private void WorkSectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSectionsListBox.Items.Count != 0 && WorkSectionsListBox.SelectedItem != null)
            {
                WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;

                ((DataView)(WorkSubSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkSectionID=" +
                                                                              WorkSectionsListBox.SelectedValue;

                WorkSubSectionsListBox.SelectedIndex = 0;
                WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;
            }
            else
            {
                WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;

                ((DataView)(WorkSubSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkSectionID=" + -1;

                WorkSubSectionsListBox.SelectedIndex = 0;
                WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;
            }

            //if (((DataView)(WorkSubSectionsListBox.ItemsSource)).Count != 0) WorkSubSectionsListBox.SelectedIndex = 0;
            CountSubSectionsLabel.Content = ((DataView)(WorkSubSectionsListBox.ItemsSource)).Count;

            WorkSubSectionsListBox_SelectionChanged(null, null);
        }

        private void WorkSubSectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;

            if (WorkSubSectionsListBox.SelectedItem == null || WorkSubSectionsListBox.SelectedValue == null)
            {
                OperationsListBox.ItemsSource = null;

                CountOperationsLabel.Content = 0;

                OperationsListBox_SelectionChanged(null, null);

                return;
            }

            DataView operationsDataView = _cc.GetWorkOperations();

            bool showAditOperations;

            Boolean.TryParse((((DataRowView)WorkSubSectionsListBox.SelectedItem)["HasAdditOperations"]).ToString(),
                out showAditOperations);


            if (showAditOperations)
            {
                operationsDataView.RowFilter = "WorkSubsectionID  IN (" +
                                               Convert.ToInt32(WorkSubSectionsListBox.SelectedValue) +
                                               ", -1) AND Visible = 'True'";

                var operationsCollectionView =
                    new BindingListCollectionView(operationsDataView);

                if (operationsCollectionView.GroupDescriptions != null)
                {
                    operationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("OperationTypeID",
                        new OperationTypeConverter()));
                    operationsCollectionView.SortDescriptions.Add(new SortDescription("OperationTypeID",
                        ListSortDirection.Ascending));

                    OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
                    OperationsListBox.ItemsSource = operationsCollectionView;
                    OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;
                }
            }
            else
            {
                operationsDataView.RowFilter = "WorkSubsectionID =" +
                                               Convert.ToInt32(WorkSubSectionsListBox.SelectedValue) +
                                               " AND Visible = 'True'";

                OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
                OperationsListBox.ItemsSource = operationsDataView;
                OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

                OperationsListBox.Items.Refresh();
            }

            if (Convert.ToInt32(((DataRowView)WorkSubSectionsListBox.SelectedItem).Row["SubsectionGroupID"]) == 2)
            {
                NormBorder.Height = 22;
                WorkScopeUpDownControl.IsEnabled = true;

            }
            else
            {
                NormBorder.Height = 0;
                WorkScopeUpDownControl.IsEnabled = false;
                WorkScopeUpDownControl.Value = 0;
                VCLPLabel.Content = 0;
            }

            OperationsListBox.SelectedIndex = 0;

            OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

            CountOperationsLabel.Content = OperationsListBox.Items.Count;

            OperationsListBox_SelectionChanged(null, null);
        }

        private void OperationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationsListBox.SelectedValue == null || OperationsListBox.Items.Count == 0 ||
                OperationsListBox.SelectedItem == null)
                return;

            int currenrWorkOperationId = Convert.ToInt32(OperationsListBox.SelectedValue);

            var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(currenrWorkOperationId);


            if(foundRows.Any())
            {
                var result = Double.TryParse(foundRows[0].Row["Productivity"].ToString(), out _currentProductivity);
                if(result)
                {
                    _currentMeasureUnitName = (new IdToMeasureUnitNameConverter()).Convert(OperationsListBox.SelectedValue);
                    NormLabel.Content = _currentProductivity + " " + _currentMeasureUnitName;
                    MeasureUnitNameLabel.Content = _currentMeasureUnitName;
                    _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
                    VCLPLabel.Content = _currentVclp;
                    return;
                }
            }

            _currentProductivity = 0;
            _currentVclp = 0;
            NormLabel.Content = string.Empty;
            MeasureUnitNameLabel.Content = string.Empty;
            VCLPLabel.Content = _currentVclp;
        }

        private double GetVCLP(decimal workScope, double currentProductivity, double totalHours)
        {
            double vclp;

            if (workScope == -1 || currentProductivity == -1)
            {
                return 0;
            }

            if (currentProductivity != 0 && totalHours != 0)
            {
                vclp = Convert.ToDouble(workScope) /
                              (currentProductivity * totalHours);
            }
            else
            {
                vclp = 0;
            }

            return Math.Round(vclp, 3);
        }

        private void DoubleTimeSet_TotalTimeChanged(object sender, RoutedEventArgs e)
        {
            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
            VCLPLabel.Content = _currentVclp;
        }


        private void AddRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationsListBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать операцию!", "Внимание", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if(ShiftsListBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо рабочий день!", "Внимание", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var timeSpentAtWorkId = Convert.ToInt64(((DataRowView)ShiftsListBox.SelectedItem)["TimeSpentAtWorkID"]);
            var workDayTimeStart = Convert.ToDateTime(((DataRowView)ShiftsListBox.SelectedItem)["WorkDayTimeStart"]);
            var workerId = Convert.ToInt64(WorkersNameListBox.SelectedValue);

            int operationGroupID =
                Convert.ToInt32(((DataRowView)OperationsListBox.SelectedItem)["OperationGroupID"]);

            int operationTypeID =
                Convert.ToInt32(((DataRowView)OperationsListBox.SelectedItem)["OperationTypeID"]);

            if ((DoubleTimeSet.startTime != TimeSpan.Zero) && (DoubleTimeSet.stopTime != TimeSpan.Zero))
            {
                var workStatusID = 1;

                _tcc.AddNewTimeRecord(timeSpentAtWorkId, workDayTimeStart, workerId,
                    Convert.ToInt32(TrackingGroupsComboBox.SelectedValue),
                    Convert.ToInt32(TrackingFactoriesComboBox.SelectedValue),
                    Convert.ToInt32(WorkUnitsListBox.SelectedValue),
                    Convert.ToInt32(WorkSectionsListBox.SelectedValue),
                    Convert.ToInt32(WorkSubSectionsListBox.SelectedValue),
                    Convert.ToInt32(OperationsListBox.SelectedValue),
                    operationGroupID, operationTypeID, DoubleTimeSet.startTime, DoubleTimeSet.stopTime,
                    Convert.ToDecimal(WorkScopeUpDownControl.Value), Convert.ToDouble(_currentVclp), NotesPopUpTextBox.Text, workStatusID, -1);

                AdministrationClass.AddNewAction(106);
                NotesPopUpTextBox.Text = string.Empty;
                WorkScopeUpDownControl.Value = 0;

                var totalTime = _tcc.CountingTotalTime((DataView)TimeTrackingDataGrid.ItemsSource);
                TotalTimeLabel.Content = totalTime;
            }
            else
            {
                MessageBox.Show("Необходимо ввести интервал времени!", "Внимание", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void WorkScopeUpDownControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
            VCLPLabel.Content = _currentVclp;
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView)TimeTrackingDataGrid.SelectedItem;
            if (drv == null) return;

            _tcc.DeleteRecord(drv.Row);
            AdministrationClass.AddNewAction(107);

            var totalTime = _tcc.CountingTotalTime((DataView)TimeTrackingDataGrid.ItemsSource);
            TotalTimeLabel.Content = totalTime;
        }

        private void AdditionalInfoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TimeTrackingDataGridSettings();
        }

        private void AdditionalInfoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TimeTrackingDataGridSettings();
        }

        private void TimeTrackingDataGridSettings()
        {
            Visibility columnVisibility = AdditionalInfoCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Hidden;

            foreach (DataGridColumn timeTrackingDataGridColumn in TimeTrackingDataGrid.Columns)
            {
                if (timeTrackingDataGridColumn.Header.ToString() == "Фабрика")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }

                if (timeTrackingDataGridColumn.Header.ToString() == "Участок")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }

                if (timeTrackingDataGridColumn.Header.ToString() == "Подучасток")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }
            }
        }


        private void OnDeleteWorkerShiftButtonClick(object sender, RoutedEventArgs e)
        {
            var shift = ShiftsListBox.SelectedItem as DataRowView;
            if (shift == null) return;

            if(TimeTrackingDataGrid.Items.Count != 0)
            {
                if (MessageBox.Show("В указанном рабочем дне присутствуют выполненные работы! Вы действительно хотите его удалить?",
                    "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
            }
            else
            {
                if (MessageBox.Show("Вы действительно хотите его удалить?",
                    "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }

            var timeSpentAtWorkId = Convert.ToInt64(shift["TimeSpentAtWorkID"]);
            _tcc.DeleteWorkerShift(timeSpentAtWorkId);
            AdministrationClass.AddNewAction(110);
        }

        private void OnAddWorkerShiftButtonClick(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedValue == null || !AddWorkerShiftDatePicker.SelectedDate.HasValue 
                || AddWorkerDayStartEndTimeSetControl.startTime == TimeSpan.Zero
                || AddWorkerDayStartEndTimeSetControl.stopTime == TimeSpan.Zero)
                return;

            var workerId = Convert.ToInt64(WorkersNameListBox.SelectedValue);
            var date = AddWorkerShiftDatePicker.SelectedDate.Value;
            var timeStart = AddWorkerDayStartEndTimeSetControl.startTime;
            var timeEnd = AddWorkerDayStartEndTimeSetControl.stopTime;

            var dayStart = date.Add(timeStart);
            var dayEnd = timeStart < timeEnd
                ? date.Add(timeEnd)
                : date.AddDays(1).Add(timeEnd);

            _tcc.AddWorkerShift(workerId, dayStart, dayEnd);
            AdministrationClass.AddNewAction(109);
        }

        #endregion


        private void OnTimeTrackingDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditingRecord) return;

            var timeTrackingRow = TimeTrackingDataGrid.SelectedItem as DataRowView;
            if (timeTrackingRow == null) return;

            TimeSpan timeStart;
            TimeSpan.TryParse(timeTrackingRow["TimeStart"].ToString(), out timeStart);

            TimeSpan timeEnd;
            TimeSpan.TryParse(timeTrackingRow["TimeEnd"].ToString(), out timeEnd);

            decimal workScope;
            decimal.TryParse(timeTrackingRow["WorkScope"].ToString(), out workScope);

            string workerNotes = timeTrackingRow["WorkerNotes"].ToString();

            DoubleTimeSet.startTime = timeStart;
            DoubleTimeSet.stopTime = timeEnd;
            WorkScopeUpDownControl.Value = workScope;
            NotesPopUpTextBox.Text = workerNotes;


            int currenrWorkOperationId = Convert.ToInt32(timeTrackingRow["WorkOperationID"]);

            var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID",
                    DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(currenrWorkOperationId);
            if (!foundRows.Any())
            {
                _currentProductivity = 0;
                _currentVclp = 0;

                NormLabel.Content = "";
                VCLPLabel.Content = "";
                return;
            }

            if (Double.TryParse(foundRows[0].Row["Productivity"].ToString(), out _currentProductivity))
            {
                _currentMeasureUnitName = (new IdToMeasureUnitNameConverter()).Convert(currenrWorkOperationId);
                NormLabel.Content = _currentProductivity + " " + _currentMeasureUnitName;
            }
            else
            {
                _currentMeasureUnitName = string.Empty;
                _currentProductivity = -1;
            }

            if (_currentProductivity == -1) NormLabel.Content = "";

            MeasureUnitNameLabel.Content = _currentMeasureUnitName;
            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
            VCLPLabel.Content = _currentVclp;
        }

        private void OnEditTrackingRowButtonClick(object sender, RoutedEventArgs e)
        {
            if (TimeTrackingDataGrid.SelectedItem == null) return;

            _isEditingRecord = true;
            EditTrackingRowButton.Visibility = Visibility.Collapsed;
            SaveRecordPanel.Visibility = Visibility.Visible;

            DeleteRecordButton.IsEnabled = false;
            AddRecordButton.IsEnabled = false;
            WorkUnitsBorder.IsEnabled = false;
            WorkSectionsBorder.IsEnabled = false;
            WorkSubSectionsBorder.IsEnabled = false;
            WorkOperationsNameBorder.IsEnabled = false;

            OnTimeTrackingDataGridSelectionChanged(null, null);
        }

        private void OnSaveRecordButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_isEditingRecord) return;

            var timeTrackingRow = TimeTrackingDataGrid.SelectedItem as DataRowView;
            if (timeTrackingRow == null) return;

            var workerTimeTrackingId = Convert.ToInt64(timeTrackingRow["WorkersTimeTrackingID"]);

            if ((DoubleTimeSet.startTime != TimeSpan.Zero) && (DoubleTimeSet.startTime != TimeSpan.Zero))
            {
                _tcc.EditTimeRecord(workerTimeTrackingId, DoubleTimeSet.startTime, DoubleTimeSet.stopTime, 
                    Convert.ToDecimal(WorkScopeUpDownControl.Value), Convert.ToDouble(_currentVclp));
                AdministrationClass.AddNewAction(108);

                NotesPopUpTextBox.Text = string.Empty;
                WorkScopeUpDownControl.Value = 0;

                var totalTime = _tcc.CountingTotalTime((DataView)TimeTrackingDataGrid.ItemsSource);
                TotalTimeLabel.Content = totalTime;

                OnCancelSavingRecordButtonClick(null, null);
            }
            else
            {
                MessageBox.Show("Необходимо ввести интервал времени!", "Внимание", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void OnCancelSavingRecordButtonClick(object sender, RoutedEventArgs e)
        {
            _isEditingRecord = false;
            EditTrackingRowButton.Visibility = Visibility.Visible;
            SaveRecordPanel.Visibility = Visibility.Collapsed;

            DeleteRecordButton.IsEnabled = true;
            AddRecordButton.IsEnabled = true;
            WorkUnitsBorder.IsEnabled = true;
            WorkSectionsBorder.IsEnabled = true;
            WorkSubSectionsBorder.IsEnabled = true;
            WorkOperationsNameBorder.IsEnabled = true;

            OperationsListBox_SelectionChanged(null, null);
        }



        private void OnInfoByModulesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var module = InfoByModulesListBox.SelectedItem as DataRowView;
            if (module == null) return;

            var moduleId = Convert.ToInt32(module["ModuleID"]);
            long? workerId = null;
            if (FilterWorkInModulesByWorkerCheckBox.IsChecked.HasValue && FilterWorkInModulesByWorkerCheckBox.IsChecked.Value)
            {
                var worker = WorkersNameListBox.SelectedItem as DataRowView;
                if (worker == null) workerId = -1;

                workerId = Convert.ToInt64(worker["WorkerID"]);
            }

            var actionsInModules = GetModuleEntryStattistic(moduleId, workerId);

            ActionsInfoByModulesDataGrid.ItemsSource = actionsInModules;
        }

        private void OnFilterWorkInModulesByWorkerCheckBoxCheckStateChanged(object sender, RoutedEventArgs e)
        {
            OnInfoByModulesListBoxSelectionChanged(null, null);
        }
    }
}
