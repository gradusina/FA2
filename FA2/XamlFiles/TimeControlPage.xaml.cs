using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для TimeControlPage.xaml
    /// </summary>
    public partial class TimeControlPage
    {
        private StaffClass _sc;
        private TimeControlClass _tcc;
        private CatalogClass _cc;
        private MyWorkersClass _mwc;
        private TaskClass _taskClass;
        private ExportToExcel _expToExcel;
        
        private DataTable _myWorkersWithNamesDataTable;

        private DateTime _fromDateTime;
        private DateTime _toDateTime;
        private int _selectedWorkerID;
        
        private bool _firstRun = true;

        private string _previousFilter = string.Empty;

        private readonly MeasureUnitNameFromOperationIdConverter _measureUnitNameFromOperationIdConverter =
            new MeasureUnitNameFromOperationIdConverter();

        private double _currentVclp;
        private double _timeDuration;
        private int _currentOperationId;

        private IdToNameConverter _idToNameConverter = new IdToNameConverter();

        private List<int> _multWorkersStatList = new List<int> ();
        private bool _multWorkerStatMode;
        private bool _multWorkerStatModeForExport;
        private bool _splitWorkerStatByDate;


        private BackgroundWorker statBackgroundWorker;



        public TimeControlPage()
        {
            InitializeComponent();

            FromDatePicker.SelectedDateChanged += (o, args) => _fromDateTime = (DateTime) FromDatePicker.SelectedDate;
            ToDatePicker.SelectedDateChanged += (o, args) => _toDateTime = (DateTime) ToDatePicker.SelectedDate;

            FromDatePicker.SelectedDate = DateTime.Now.AddDays(-15);

            //FromDatePicker.SelectedDate = DateTime.Now.AddDays(-15);
            ToDatePicker.SelectedDate = DateTime.Now;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.ControlTimeTracking);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.ControlTimeTracking);

            if (_firstRun)
            {
                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (o, args) =>
                    GetClasses();

                backgroundWorker.RunWorkerCompleted += (o, args) =>
                {
                    BindingData();

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideWaitAnnimation();
                };

                backgroundWorker.RunWorkerAsync();

                _firstRun = false;
            }
            else
            {
                _taskClass.Fill(_tcc.GetDateFrom(), _tcc.GetDateTo());
                ShiftsListBox_SelectionChanged(null, null);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void GetClasses()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetTimeControlClass(ref _tcc);
            App.BaseClass.GetCatalogClass(ref _cc);
            App.BaseClass.GetMyWorkersClass(ref _mwc);
            App.BaseClass.GetTaskClass(ref _taskClass);

            App.BaseClass.GetExportToExcelClass(ref _expToExcel);

            _tcc.FillShifts(_fromDateTime, _toDateTime);
            _tcc.FillTimeTracking(_fromDateTime, _toDateTime);
        }

        private void BindingData()
        {
            // WorkersTimeTrackingListBox.SelectionChanged += Timetr;
            WorkersTimeTrackingListBox.ItemsSource = _tcc.GetTimeTracking();

            ///////////////////////////////////
            ///////////////////////////////////
            _taskClass.Fill(_tcc.GetDateFrom(), _tcc.GetDateTo());
            ///////////////////////////////////
            ///////////////////////////////////
            WorkerTaskTimeTrackingListBox.ItemsSource = _taskClass.TaskTimeTracking.Table.AsDataView();

            MultWorkersListBox.ItemsSource = _multWorkersStatList;

            ShiftsListBox.SelectionChanged -= ShiftsListBox_SelectionChanged;
            ShiftsListBox.ItemsSource = _tcc.GetShifts();
            ShiftsListBox.SelectionChanged += ShiftsListBox_SelectionChanged;

            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;

            CreateMyWorkersWithName();

            #region ByFactories

            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.ItemsSource = _sc.GetFactories();
            FactoriesComboBox.SelectedIndex = 0;
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

            WorkersGroupsComboBox.SelectionChanged -= WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkersGroupsComboBox.SelectedValue = 2;
            WorkersGroupsComboBox.SelectionChanged += WorkersGroupsComboBox_SelectionChanged;

            #endregion

            #region ByBrigades

            WorkersBrigadesComboBox.SelectionChanged -= WorkersBrigadesComboBox_SelectionChanged;
            WorkersBrigadesComboBox.ItemsSource = _mwc.GetMyWorkersGroups();


            MainWorkersComboBox.SelectionChanged -= MainWorkersComboBox_SelectionChanged;
            MainWorkersComboBox.ItemsSource = _mwc.GetMainWorkers();
            MainWorkersComboBox.SelectedIndex = 0;

            ((DataView) WorkersBrigadesComboBox.ItemsSource).RowFilter = "MainWorkerID=" +
                                                                         MainWorkersComboBox.SelectedValue;

            WorkersBrigadesComboBox.SelectedIndex = 0;
            MainWorkersComboBox.SelectionChanged += MainWorkersComboBox_SelectionChanged;
            WorkersBrigadesComboBox.SelectionChanged += WorkersBrigadesComboBox_SelectionChanged;

            #endregion

            SetDefaultWorkersFilter();

            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;

            WorkersNamesListBox.SelectedIndex = 0;


            #region CommStatFilter

            FactoryFilterComboBox.SelectionChanged -= FactoryFilterComboBox_SelectionChanged;
            FactoryFilterComboBox.ItemsSource = _cc.GetFactories();
            FactoryFilterComboBox.SelectedIndex = 0;
            FactoryFilterComboBox.SelectionChanged += FactoryFilterComboBox_SelectionChanged;

            UnitsFilterComboBox.SelectionChanged -= UnitsFilterComboBox_SelectionChanged;
            UnitsFilterComboBox.ItemsSource = _cc.GetWorkUnits();
            UnitsFilterComboBox.SelectionChanged += UnitsFilterComboBox_SelectionChanged;

            SectionsFilterComboBox.SelectionChanged -= SectionsFilterComboBox_SelectionChanged;
            SectionsFilterComboBox.ItemsSource = _cc.GetWorkSections();
            SectionsFilterComboBox.SelectionChanged += SectionsFilterComboBox_SelectionChanged;

            SubsectionsFilterComboBox.SelectionChanged -= SubsectionsFilterComboBox_SelectionChanged;
            SubsectionsFilterComboBox.ItemsSource = _cc.GetWorkSubsections();
            SubsectionsFilterComboBox.SelectionChanged += SubsectionsFilterComboBox_SelectionChanged;

            OperationsFilterComboBox.ItemsSource = _cc.GetWorkOperations();

            FactoryFilterComboBox_SelectionChanged(null, null);

            #endregion

            StatisticsTimeTrackingDataGrid.ItemsSource = _tcc.GetWorkersStat();

            CommonStatisticsTimeTrackingDataGrid.ItemsSource = _tcc.GetCommonStat();

            TimeControlRadioButton.IsChecked = true;
        }

        private void SetDefaultWorkersFilter()
        {
            if (_mwc.HaveMyWorkers(AdministrationClass.CurrentWorkerId))
            {
                ByBrigadesRadioButton.IsChecked = true;
                

                MainWorkersComboBox.SelectedValue = AdministrationClass.CurrentWorkerId;
                MainWorkersComboBox_SelectionChanged(null, null);

            }
            else
            {
                ByFactoriesRadioButton.IsChecked = true;
            }

            ByFactoriesRadioButton.IsChecked = true;
        }

        private void CreateMyWorkersWithName()
        {
            //Result table
            _myWorkersWithNamesDataTable = new DataTable();
            _myWorkersWithNamesDataTable.Columns.Add("MyWorkerID", typeof (Int64));
            _myWorkersWithNamesDataTable.Columns.Add("MainWorkerID", typeof (Int64));
            _myWorkersWithNamesDataTable.Columns.Add("MyWorkersGroupID", typeof (Int64));
            _myWorkersWithNamesDataTable.Columns.Add("WorkerID", typeof (Int64));
            _myWorkersWithNamesDataTable.Columns.Add("WorkerProfessionID", typeof (Int64));
            _myWorkersWithNamesDataTable.Columns.Add("IsEnable", typeof (bool));
            _myWorkersWithNamesDataTable.Columns.Add("Name", typeof (string));
            _myWorkersWithNamesDataTable.Columns.Add("AvailableInList", typeof (bool));

            var availableModules =
                _mwc.GetMyWorkers()
                    .Table.AsEnumerable()
                    .Join(_sc.GetStaffPersonalInfo().Table.AsEnumerable(), a => a["WorkerID"], m => m["WorkerID"],
                        (a, m) =>
                        {
                            var newRow = _myWorkersWithNamesDataTable.NewRow();
                            newRow["MyWorkerID"] = a["MyWorkerID"];
                            newRow["MainWorkerID"] = a["MainWorkerID"];
                            newRow["MyWorkersGroupID"] = a["MyWorkersGroupID"];
                            newRow["WorkerID"] = a["WorkerID"];
                            newRow["WorkerProfessionID"] = a["WorkerProfessionID"];
                            newRow["IsEnable"] = a["IsEnable"];
                            newRow["Name"] = m["Name"];
                            newRow["AvailableInList"] = m["AvailableInList"];
                            return newRow;
                        });

            _myWorkersWithNamesDataTable = availableModules.CopyToDataTable().Copy();
        }

        private void ApplyDateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            _tcc.FillShifts(_fromDateTime, _toDateTime);
            _tcc.FillTimeTracking(_fromDateTime, _toDateTime);
            _taskClass.Fill(_fromDateTime, _toDateTime);
        }

        private void WorkersNamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (statBackgroundWorker != null && statBackgroundWorker.IsBusy) return;

            if ((WorkersNamesListBox.Items.Count != 0) || (WorkersNamesListBox.SelectedValue != null))
            {
                _selectedWorkerID = Convert.ToInt32(WorkersNamesListBox.SelectedValue);
            }

            if (ModesWorkersStatTabControl.SelectedIndex == 0)
            {
                #region TimeControl_Mode

                if (ShiftsListBox.ItemsSource == null) return;

                if ((WorkersNamesListBox.Items.Count == 0) || (WorkersNamesListBox.SelectedValue == null))
                {
                    ((DataView) ShiftsListBox.ItemsSource).RowFilter = "WorkerID = -1";

                    ShiftsListBox_SelectionChanged(null, null);
                    return;
                }

                ((DataView) ShiftsListBox.ItemsSource).RowFilter = "WorkerID = " + _selectedWorkerID;

                if (ShiftsListBox.Items.Count != 0)
                {
                    ShiftsListBox.SelectedIndex = 0;
                    ShiftsListBox.BringIntoView();
                }

                #endregion
            }

            if (ModesWorkersStatTabControl.SelectedIndex == 1)
            {
                if (!_multWorkerStatMode)
                {
                    CalculateOperationsStatisticsButton_Click(null, null);
                    FormatStatisticsTimeTrackingDataGrid();
                }
            }
        }

        #region WorkersFilters

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchAndFilterWorkers();
        }

        //By_Factory
        private void WorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        //By_MainWorker
        private void MainWorkersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWorkersComboBox.SelectedValue == null || WorkersBrigadesComboBox.SelectedValue == null) return;

            ((DataView) WorkersBrigadesComboBox.ItemsSource).RowFilter = "MainWorkerID=" +
                                                                         MainWorkersComboBox.SelectedValue;
            WorkersBrigadesComboBox.SelectedIndex = 0;

            string filterStr = "MainWorkerID =  " + MainWorkersComboBox.SelectedValue + " AND MyWorkersGroupID =  " +
                               WorkersBrigadesComboBox.SelectedValue;

            ((DataView) WorkersNamesListBox.ItemsSource).RowFilter = filterStr;
            WorkersNamesListBox.SelectedIndex = 0;
        }

        private void WorkersBrigadesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWorkersComboBox.SelectedValue == null || WorkersBrigadesComboBox.SelectedValue == null) return;

            string filterStr = "MainWorkerID =  " + MainWorkersComboBox.SelectedValue + " AND MyWorkersGroupID =  " +
                               WorkersBrigadesComboBox.SelectedValue;

            ((DataView) WorkersNamesListBox.ItemsSource).RowFilter = filterStr;
            WorkersNamesListBox.SelectedIndex = 0;
        }

        private void WorkersFilterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetWorkersNamesList();
        }
        
        #endregion

        #region TimeControl_Mode

        private void SetWorkersNamesList()
        {
            SearchTextBox.Text = string.Empty;

            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;

            if (ByFactoriesRadioButton.IsChecked == true)
            {
                WorkersNamesListBox.ItemsSource = _sc.GetStaffPersonalInfo();

                WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;

                WorkersGroupsComboBox_SelectionChanged(null, null);

            }
            else if (ByBrigadesRadioButton.IsChecked == true)
            {
                WorkersNamesListBox.ItemsSource = _myWorkersWithNamesDataTable.AsDataView();

                WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;

                MainWorkersComboBox_SelectionChanged(null, null);
            }

            WorkersNamesListBox.Items.MoveCurrentToFirst();
        }

        private void ShiftsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersTimeTrackingListBox.ItemsSource == null) return;

            if (ShiftsListBox.SelectedItems.Count != 1)
            {
                ((DataView) WorkersTimeTrackingListBox.ItemsSource).RowFilter = "TimeSpentAtWorkID= -1";
                ((DataView) WorkerTaskTimeTrackingListBox.ItemsSource).RowFilter = "TimeSpentAtWorkID= -1";

                TotalTimeLabel.DataContext = TimeSpan.Zero;
                OperationsTabItem.DataContext = TimeSpan.Zero;
                TasksTabItem.DataContext = TimeSpan.Zero;

                return;
            }

            DataRow dr = ((DataRowView) ShiftsListBox.SelectedItem).Row;

            if (dr["ShiftNumber"] != DBNull.Value)
            {
                switch (Convert.ToInt32(dr["ShiftNumber"]))
                {
                    case 1:
                    {
                        FirstShiftRadioButton.IsChecked = true;
                        break;
                    }
                    case 2:
                    {
                        SecondShiftRadioButton.IsChecked = true;
                        break;
                    }
                    case 3:
                    {
                        ThirdShiftRadioButton.IsChecked = true;
                        break;
                    }
                }
            }


            if (Convert.ToBoolean(dr["DayEnd"]) != true)
            {
                TimeSpentAtWorkInfoGrid.IsEnabled = false;
                TimeSpentAtWorkInfoReset();
            }
            else
            {
                TimeSpentAtWorkInfoGrid.IsEnabled = true;

                VCLPUpDownControl.Value = dr["VCLP"] != DBNull.Value ? Convert.ToDecimal(dr["VCLP"]) : Decimal.Zero;

                TimeSpentAtWorkNotesTextBox.Text = dr["MainWorkerNotes"].ToString();

                TimeSpentAtWorkWorkerNameLabel.Content = dr["MainWorkerID"] != DBNull.Value
                    ? _sc.GetWorkerName(Convert.ToInt32(dr["MainWorkerID"]), true)
                    : "ФИО ответственного";
            }

            ((DataView) WorkersTimeTrackingListBox.ItemsSource).RowFilter = "TimeSpentAtWorkID=" +
                                                                            dr["TimeSpentAtWorkID"];

            FillFilters();
            FilterTimeTracking();

            var timeSpentAtWorkId = Convert.ToInt32(((DataRowView) ShiftsListBox.SelectedItem).Row["TimeSpentAtWorkID"]);
            ((DataView) WorkerTaskTimeTrackingListBox.ItemsSource).RowFilter = string.Format("TimeSpentAtWorkID = {0}",
                timeSpentAtWorkId);

            if (WorkersTimeTrackingListBox.Items.Count != 0)
            {
                WorkersTimeTrackingListBox.SelectedIndex = 0;
                WorkersTimeTrackingListBox.BringIntoView();
            }

            if (WorkerTaskTimeTrackingListBox.Items.Count != 0)
            {
                WorkerTaskTimeTrackingListBox.SelectedIndex = 0;
                WorkerTaskTimeTrackingListBox.BringIntoView();
            }

            var operationCount = _tcc.CountingTotalTime((DataView) WorkersTimeTrackingListBox.ItemsSource);
            OperationsTabItem.DataContext = operationCount;

            var tasksCount = CountTasksTotalTime((DataView) WorkerTaskTimeTrackingListBox.ItemsSource);
            TasksTabItem.DataContext = tasksCount;

            TotalTimeLabel.DataContext = operationCount.Add(tasksCount);
            //MessageBox.Show(operationCount.ToString());
        }

        private void FillFilters()
        {
            var view = (DataView) WorkersTimeTrackingListBox.ItemsSource;

            if (view != null)
            {
                DataTable distinctFactories = view.ToTable(true, "FactoryID");
                FactoryTimetrcFilComboBox.ItemsSource = distinctFactories.AsDataView();


                DataTable distinctUnits = view.ToTable(true, "WorkUnitID");
                UnitTimetrcFilComboBox.ItemsSource = distinctUnits.AsDataView();

                DataTable distinctSections = view.ToTable(true, "WorkSectionID");
                SectionTimetrcFilComboBox.ItemsSource = distinctSections.AsDataView();

                DataTable distinctSubsections = view.ToTable(true, "WorkSubsectionID");
                SubSectionTimetrcFilComboBox.ItemsSource = distinctSubsections.AsDataView();

                DataTable distinctOperations = view.ToTable(true, "WorkOperationID");
                OperationTimetrcFilComboBox.ItemsSource = distinctOperations.AsDataView();
            }
        }

        private void FilterTimeTracking()
        {
            if (ShiftsListBox.SelectedItem == null) return;

            string filter = "TimeSpentAtWorkID=" + ((DataRowView) ShiftsListBox.SelectedItem).Row["TimeSpentAtWorkID"];

            if (FactoryTimetrcFilComboBox.SelectedItem != null)
            {
                filter = filter + " AND FactoryID = " + Convert.ToInt32(FactoryTimetrcFilComboBox.SelectedValue);
            }

            if (UnitTimetrcFilComboBox.SelectedItem != null)
            {
                filter = filter + " AND WorkUnitID = " + Convert.ToInt32(UnitTimetrcFilComboBox.SelectedValue);
            }

            if (SectionTimetrcFilComboBox.SelectedItem != null)
            {
                filter = filter + " AND WorkSectionID = " + Convert.ToInt32(SectionTimetrcFilComboBox.SelectedValue);
            }

            if (SubSectionTimetrcFilComboBox.SelectedItem != null)
            {
                filter = filter + " AND WorkSubsectionID = " +
                         Convert.ToInt32(SubSectionTimetrcFilComboBox.SelectedValue);
            }

            if (OperationTimetrcFilComboBox.SelectedItem != null)
            {
                filter = filter + " AND WorkOperationID = " + Convert.ToInt32(OperationTimetrcFilComboBox.SelectedValue);
            }

            ((DataView) WorkersTimeTrackingListBox.ItemsSource).RowFilter = filter;

            OperationsTabItem.DataContext = _tcc.CountingTotalTime((DataView) WorkersTimeTrackingListBox.ItemsSource);
        }

        private TimeSpan CountTasksTotalTime(ICollection tasksTimeTrackingView)
        {
            if (tasksTimeTrackingView.Count == 0) return TimeSpan.Zero;

            var totalTime = new TimeSpan();
            totalTime =
                tasksTimeTrackingView.OfType<DataRowView>().Select(dataRow =>
                    TimeIntervalCountConverter.CalculateTimeInterval(
                        (TimeSpan)dataRow["TimeStart"], (TimeSpan)dataRow["TimeEnd"]))
                    .Aggregate(totalTime, (current, interval) => current.Add(interval));
            return totalTime;
        }




        private void FillStatFilters()
        {
            var view = (DataView)StatisticsTimeTrackingDataGrid.ItemsSource;

            if (view != null)
            {
                DataTable distinctFactories = view.ToTable(true, "FactoryID");
                FactoryStatFilComboBox.ItemsSource = distinctFactories.AsDataView();
                ((DataView) FactoryStatFilComboBox.ItemsSource).RowFilter = "FactoryID IS NOT NULL";

                DataTable distinctUnits = view.ToTable(true, "WorkUnitID");
                UnitStatFilComboBox.ItemsSource = distinctUnits.AsDataView();
                ((DataView)UnitStatFilComboBox.ItemsSource).RowFilter = "WorkUnitID IS NOT NULL";

                DataTable distinctSections = view.ToTable(true, "WorkSectionID");
                SectionStatFilComboBox.ItemsSource = distinctSections.AsDataView();
                ((DataView)SectionStatFilComboBox.ItemsSource).RowFilter = "WorkSectionID IS NOT NULL";

                DataTable distinctSubsections = view.ToTable(true, "WorkSubsectionID");
                SubSectionStatFilComboBox.ItemsSource = distinctSubsections.AsDataView();
                ((DataView)SubSectionStatFilComboBox.ItemsSource).RowFilter = "WorkSubsectionID IS NOT NULL";

                DataTable distinctOperations = view.ToTable(true, "WorkOperationID");
                OperationStatComboBox.ItemsSource = distinctOperations.AsDataView();
                ((DataView)OperationStatComboBox.ItemsSource).RowFilter = "WorkOperationID IS NOT NULL";

                DataTable distinctOperationGroups = view.ToTable(true, "OperationGroupID");
                OperationGroupStatComboBox.ItemsSource = distinctOperationGroups.AsDataView();
                ((DataView)OperationGroupStatComboBox.ItemsSource).RowFilter = "OperationGroupID IS NOT NULL";

                //DataTable distinctOperationTypes = view.ToTable(true, "OperationTypeID");
                //OperationTypeStatComboBox.ItemsSource = distinctOperationTypes.AsDataView();
                //((DataView)OperationTypeStatComboBox.ItemsSource).RowFilter = "OperationTypeID IS NOT NULL";
            }
        }

        private void FillCommStatFilters()
        {
            var view = (DataView)CommonStatisticsTimeTrackingDataGrid.ItemsSource;

            if (view != null)
            {
                DataTable distinctOperationGroups = view.ToTable(true, "OperationGroupID");
                OperationGroupCommStatComboBox.ItemsSource = distinctOperationGroups.AsDataView();
                ((DataView)OperationGroupCommStatComboBox.ItemsSource).RowFilter = "OperationGroupID IS NOT NULL";
            }
        }

        private void FilterCommStatTimeTracking()
        {
            bool isCanCalculateValues = false;

            string filter = String.Empty;

            if (OperationGroupCommStatComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "OperationGroupID = " + Convert.ToInt32(OperationGroupCommStatComboBox.SelectedValue);
                else
                    filter = filter + " AND OperationGroupID = " +
                             Convert.ToInt32(OperationGroupCommStatComboBox.SelectedValue);
            }

            if (OperationTypeCommStatComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "OperationTypeID = " + (Convert.ToInt32(OperationTypeCommStatComboBox.SelectedIndex) + 1);
                else
                    filter = filter + " AND OperationTypeID = " +
                             (Convert.ToInt32(OperationTypeCommStatComboBox.SelectedIndex) + 1);
            }

            ((DataView) CommonStatisticsTimeTrackingDataGrid.ItemsSource).RowFilter = filter;

            if (isCanCalculateValues)
            {
                DataView dv = ((DataView) CommonStatisticsTimeTrackingDataGrid.ItemsSource);

                decimal summTotalTime = 0;
                foreach (DataRowView drv in dv)
                {
                    decimal tempTotalTime;
                    Decimal.TryParse(drv.Row["Time"].ToString().Replace(".", ","), out tempTotalTime);
                    summTotalTime = summTotalTime + tempTotalTime;
                }

                CommStatHouresLabel.Content = summTotalTime;

                FilterCommStatValuesBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FilterCommStatValuesBorder.Visibility = Visibility.Collapsed;
                CommStatHouresLabel.Content = 0;
            }
        }

        private void ResetCommStatTimeTrackingFilter()
        {
            OperationGroupCommStatComboBox.SelectedItem = null;
            OperationTypeCommStatComboBox.SelectedItem = null;

            FilterCommStatTimeTracking();
        }

        private void FilterCommStatTimeTrackingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterCommStatTimeTracking();
        }








        private void FilterStatTimeTracking()
        {
            bool isSummWorkScope = false;
            bool isCanCalculateValues = false;

            string filter = String.Empty;

            if (FactoryStatFilComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;

                if (filter == String.Empty)
                    filter = "FactoryID = " + Convert.ToInt32(FactoryStatFilComboBox.SelectedValue);
                else
                    filter = filter + " AND FactoryID = " + Convert.ToInt32(FactoryStatFilComboBox.SelectedValue);
            }

            if (UnitStatFilComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;

                if (filter == String.Empty)
                    filter = "WorkUnitID = " + Convert.ToInt32(UnitStatFilComboBox.SelectedValue);
                else
                    filter = filter + " AND WorkUnitID = " + Convert.ToInt32(UnitStatFilComboBox.SelectedValue);
            }

            if (SectionStatFilComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "WorkSectionID = " + Convert.ToInt32(SectionStatFilComboBox.SelectedValue);
                else
                    filter = filter + " AND WorkSectionID = " + Convert.ToInt32(SectionStatFilComboBox.SelectedValue);
            }

            if (SubSectionStatFilComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "WorkSubsectionID = " + Convert.ToInt32(SubSectionStatFilComboBox.SelectedValue);
                else
                    filter = filter + " AND WorkSubsectionID = " +
                             Convert.ToInt32(SubSectionStatFilComboBox.SelectedValue);
            }

            if (OperationStatComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "WorkOperationID = " + Convert.ToInt32(OperationStatComboBox.SelectedValue);
                else
                    filter = filter + " AND WorkOperationID = " + Convert.ToInt32(OperationStatComboBox.SelectedValue);

                isSummWorkScope = true;
            }

            if (OperationGroupStatComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "OperationGroupID = " + Convert.ToInt32(OperationGroupStatComboBox.SelectedValue);
                else
                    filter = filter + " AND OperationGroupID = " +
                             Convert.ToInt32(OperationGroupStatComboBox.SelectedValue);
            }

            if (OperationTypeStatComboBox.SelectedItem != null)
            {
                isCanCalculateValues = true;
                if (filter == String.Empty)
                    filter = "OperationTypeID = " + (Convert.ToInt32(OperationTypeStatComboBox.SelectedIndex) + 1);
                else
                    filter = filter + " AND OperationTypeID = " +
                             (Convert.ToInt32(OperationTypeStatComboBox.SelectedIndex) + 1);
            }




            ((DataView) StatisticsTimeTrackingDataGrid.ItemsSource).RowFilter = filter;

            FormatStatisticsTimeTrackingDataGrid();

            if (isCanCalculateValues)
            {
                DataView dv = ((DataView) StatisticsTimeTrackingDataGrid.ItemsSource);

                if (isSummWorkScope)
                {
                    StatWorkScopeLabel.Visibility = Visibility.Visible;

                    decimal summWorkScope = 0;
                    string measureUnitName = String.Empty;


                    foreach (DataRowView drv in dv)
                    {
                        decimal tempWorkScope;
                        Decimal.TryParse(drv.Row["WorkScope"].ToString().Replace(".", ","), out tempWorkScope);
                        summWorkScope = summWorkScope + tempWorkScope;

                        measureUnitName = drv.Row["MeasureUnit"].ToString();
                    }

                    StatWorkScopeLabel.Content = String.Format("{0} {1}", summWorkScope, measureUnitName);
                }
                else
                {
                    StatWorkScopeLabel.Visibility = Visibility.Collapsed;
                }

                decimal summTotalTime = 0;
                foreach (DataRowView drv in dv)
                {
                    decimal tempTotalTime;
                    Decimal.TryParse(drv.Row["TotalTime"].ToString().Replace(".", ","), out tempTotalTime);
                    summTotalTime = summTotalTime + tempTotalTime;
                }
                StatHouresLabel.Content = summTotalTime;

                FilterStatValuesBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FilterStatValuesBorder.Visibility = Visibility.Collapsed;
                StatHouresLabel.Content = 0;
                StatWorkScopeLabel.Content = 0;
            }
        }

        private void ResetStatTimeTrackingFilter()
        {
            FactoryStatFilComboBox.SelectedItem = null;
            UnitStatFilComboBox.SelectedItem = null;
            SectionStatFilComboBox.SelectedItem = null;
            SubSectionStatFilComboBox.SelectedItem = null;
            OperationStatComboBox.SelectedItem = null;
            OperationGroupStatComboBox.SelectedItem = null;
            OperationTypeStatComboBox.SelectedItem = null;

            FilterStatTimeTracking();
        }

        private void FilterStatTimeTrackingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterStatTimeTracking();
        }

        private void ResetStatFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStatTimeTrackingFilter();
        }

        private void ResetTimeTrackingFilter()
        {
            FactoryTimetrcFilComboBox.SelectedItem = null;
            UnitTimetrcFilComboBox.SelectedItem = null;
            SectionTimetrcFilComboBox.SelectedItem = null;
            SubSectionTimetrcFilComboBox.SelectedItem = null;
            OperationTimetrcFilComboBox.SelectedItem = null;

            FilterTimeTracking();

            
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimeTrackingFilter();
        }

        private void TimeSpentAtWorkInfoReset()
        {
            FirstShiftRadioButton.IsChecked = true;
            TimeSpentAtWorkWorkerNameLabel.Content = "Отвественный";
            VCLPUpDownControl.Value = decimal.Zero;
            TimeSpentAtWorkNotesTextBox.Text = string.Empty;
        }

        private void FilterWorkers()
        {
            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;

            SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
            SearchTextBox.Text = string.Empty;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            DataTable workersDataTable = _sc.FilterWorkers(true, Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                true,
                Convert.ToInt32(FactoriesComboBox.SelectedValue), false, 0);

            WorkersNamesListBox.ItemsSource = workersDataTable != null ? workersDataTable.DefaultView : null;

            SearchAndFilterWorkers();
        }

        private void SearchAndFilterWorkers()
        {
            if (WorkersNamesListBox.ItemsSource == null) return;

            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;

            string filterString = "AvailableInList = 'True'";

            string searchText = SearchTextBox.Text.Trim();

            if (searchText != string.Empty)
            {
                if (ByBrigadesRadioButton.IsChecked == true)
                {
                    if (_previousFilter == string.Empty)
                        _previousFilter = ((DataView) WorkersNamesListBox.ItemsSource).RowFilter;
                }
                else
                    _previousFilter = string.Empty;


                if (filterString == string.Empty) filterString = "(Name LIKE  '" + searchText + "*')";
                else filterString = filterString + " AND (Name LIKE  '" + searchText + "*')";


                if (_previousFilter != string.Empty)
                {
                    if (filterString == string.Empty) filterString = _previousFilter;
                    else filterString = filterString + " AND " + _previousFilter;
                }

                ((DataView) WorkersNamesListBox.ItemsSource).RowFilter = filterString;
            }
            else
            {
                if (ByBrigadesRadioButton.IsChecked == true)
                {
                    if (_previousFilter != string.Empty)
                        ((DataView) WorkersNamesListBox.ItemsSource).RowFilter = _previousFilter;
                }
                _previousFilter = string.Empty;
            }


            WorkersNamesListBox.SelectedIndex = 0;

            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;

            WorkersNamesListBox_SelectionChanged(null, null);
        }

        private void SaveTimeSpentAtWorkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShiftsListBox.SelectedValue != null) return;

            var shiftNumber = 1;
            if (FirstShiftRadioButton.IsChecked == true)
                shiftNumber = 1;
            if (SecondShiftRadioButton.IsChecked == true)
                shiftNumber = 2;
            if (ThirdShiftRadioButton.IsChecked == true)
                shiftNumber = 3;

            int timeSpentAtWorkId = Convert.ToInt32(ShiftsListBox.SelectedValue);

            _tcc.SaveTimeSpentAtWork(TimeSpentAtWorkNotesTextBox.Text, Convert.ToDecimal(VCLPUpDownControl.Value),
                shiftNumber,
                timeSpentAtWorkId);

            AdministrationClass.AddNewAction(66);
        }

        private void WorkersTimeTrackingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersTimeTrackingListBox.Items.Count == 0 || WorkersTimeTrackingListBox.SelectedItem == null)
            {
                return;
            }

            var currentTimetrackingRow = (DataRowView) WorkersTimeTrackingListBox.SelectedItem;

            TimeTrackingInfoGrid.DataContext = currentTimetrackingRow;

            FSVButton.DataContext = WorkersTimeTrackingListBox.SelectedItem;
            SSVButton.DataContext = WorkersTimeTrackingListBox.SelectedItem;
            TSVButton.DataContext = WorkersTimeTrackingListBox.SelectedItem;

            decimal workScope;
            Decimal.TryParse(currentTimetrackingRow.Row["WorkScope"].ToString(), out workScope);

            WorkScopeUpDown.ValueChanged -= WorkScopeUpDown_ValueChanged;
            WorkScopeUpDown.Value = workScope;
            WorkScopeUpDown.ValueChanged += WorkScopeUpDown_ValueChanged;

            WorkerNotesTextBox.Text = currentTimetrackingRow.Row["WorkerNotes"].ToString();

            _currentOperationId = Convert.ToInt32(currentTimetrackingRow["WorkOperationID"]);

            TimeSpan startTimeofDuration;
            TimeSpan stopTimeofDuration;

            TimeSpan.TryParse(currentTimetrackingRow.Row["TimeStart"].ToString(), out startTimeofDuration);
            TimeSpan.TryParse(currentTimetrackingRow.Row["TimeEnd"].ToString(), out stopTimeofDuration);

            //MeasureUnitNameLabel.Content = _measureUnitNameFromOperationIdConverter.Convert(_currentOperationID,
            //    "MeasureUnitName");

            _timeDuration = CalculateDurationTime(startTimeofDuration, stopTimeofDuration);
        }

        private double CalculateDurationTime(TimeSpan startTimeofDuration, TimeSpan stopTimeofDuration)
        {
            TimeSpan timeDuration;

            if (stopTimeofDuration >= startTimeofDuration)
            {
                timeDuration = new TimeSpan(stopTimeofDuration.Hours - startTimeofDuration.Hours,
                    stopTimeofDuration.Minutes - startTimeofDuration.Minutes, 0);
            }
            else
            {
                timeDuration = new TimeSpan((24 - startTimeofDuration.Hours) + stopTimeofDuration.Hours,
                    stopTimeofDuration.Minutes - startTimeofDuration.Minutes, 0);
            }

            return Math.Round(timeDuration.TotalMinutes/60, 3);
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
                vclp = Convert.ToDouble(workScope)/
                       (currentProductivity*totalHours);
            }
            else
            {
                vclp = 0;
            }

            return Math.Round(vclp, 3);
        }

        private void WorkScopeUpDown_ValueChanged(object sender, RoutedEventArgs e)
        {
            double productivity =
                Convert.ToDouble(_measureUnitNameFromOperationIdConverter.Convert(_currentOperationId, "Productivity"));

            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDown.Value), productivity, _timeDuration);

            if (WorkersTimeTrackingListBox.SelectedItem != null)
            {
                ((DataRowView) WorkersTimeTrackingListBox.SelectedItem).Row["WorkScope"] = WorkScopeUpDown.Value;
                ((DataRowView) WorkersTimeTrackingListBox.SelectedItem).Row["VCLP"] = _currentVclp;
            }
        }

        private void SaveTimeTrackingButton_Click(object sender, RoutedEventArgs e)
        {
            _tcc.SaveTimeTracking();
            _taskClass.SaveTimeTracking();
        }

        private void FSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersTimeTrackingListBox.SelectedItem == null) return;

            WorkersTimeTrackingListBox.SelectionChanged -= WorkersTimeTrackingListBox_SelectionChanged;

            _tcc.FirstStageVerification(((DataRowView) WorkersTimeTrackingListBox.SelectedItem).Row);
            AdministrationClass.AddNewAction(62);

            WorkersTimeTrackingListBox.SelectionChanged += WorkersTimeTrackingListBox_SelectionChanged;

            WorkersTimeTrackingListBox.Items.Refresh();

            FSVButton.DataContext = null;
            WorkersTimeTrackingListBox_SelectionChanged(null, null);
        }

        private void SSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersTimeTrackingListBox.SelectedItem == null) return;

            WorkersTimeTrackingListBox.SelectionChanged -= WorkersTimeTrackingListBox_SelectionChanged;

            _tcc.SecondStageVerification(((DataRowView) WorkersTimeTrackingListBox.SelectedItem).Row);
            AdministrationClass.AddNewAction(63);

            WorkersTimeTrackingListBox.SelectionChanged += WorkersTimeTrackingListBox_SelectionChanged;

            WorkersTimeTrackingListBox.Items.Refresh();

            SSVButton.DataContext = null;
            WorkersTimeTrackingListBox_SelectionChanged(null, null);
        }

        private void TSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersTimeTrackingListBox.SelectedItem == null) return;

            WorkersTimeTrackingListBox.SelectionChanged -= WorkersTimeTrackingListBox_SelectionChanged;

            _tcc.ThirdStageVerification(((DataRowView) WorkersTimeTrackingListBox.SelectedItem).Row);
            AdministrationClass.AddNewAction(64);

            WorkersTimeTrackingListBox.SelectionChanged += WorkersTimeTrackingListBox_SelectionChanged;

            WorkersTimeTrackingListBox.Items.Refresh();

            TSVButton.DataContext = null;
            WorkersTimeTrackingListBox_SelectionChanged(null, null);
        }

        private void FilterTimeTrackingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterTimeTracking();
        }

        #endregion

        #region WorkersStatistics_Mode

        private void ShowVerificationLevelsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ShowHideVerificationLevels(ShowVerificationLevelsCheckBox.IsChecked == true);
        }

        private void ShowHideVerificationLevels(bool show)
        {
            if (show)
            {
                foreach (var dataGridColumn in StatisticsTimeTrackingDataGrid.Columns.Where(dataGridColumn =>
                    (Equals(dataGridColumn.Header, "1-ый ур.")) ||
                    (Equals(dataGridColumn.Header, "2-ой ур.")) ||
                    (Equals(dataGridColumn.Header, "3-ий ур.")) ||
                    (Equals(dataGridColumn.Header, "Полностью подтвержденное время")) ||
                    (Equals(dataGridColumn.Header, "Частично"))))
                {
                    dataGridColumn.Visibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (var dataGridColumn in StatisticsTimeTrackingDataGrid.Columns.Where(dataGridColumn =>
                    (Equals(dataGridColumn.Header, "1-ый ур.")) ||
                    (Equals(dataGridColumn.Header, "2-ой ур.")) ||
                    (Equals(dataGridColumn.Header, "3-ий ур."))||
                    (Equals(dataGridColumn.Header, "Полностью подтвержденное время")) ||
                    (Equals(dataGridColumn.Header, "Частично"))))
                {
                    dataGridColumn.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CalculateOperationsStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (statBackgroundWorker != null)
            {
                statBackgroundWorker.CancelAsync();
                statBackgroundWorker.Dispose();
            }

            statBackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };


            _multWorkerStatModeForExport = _multWorkerStatMode;

            FillCommonTimeOpStatComboBox(true);

            CalculateOperationsStatisticsButton.IsEnabled = false;
            AddToWorkerListButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            SplitByDatesCheckBox.IsEnabled = false;

            NameCommonTimeStatOpLabel.Content = !_multWorkerStatMode
                   ? _idToNameConverter.Convert(_selectedWorkerID, "ShortName")
                   : "-";

            TotalOperationsTimeStatLabel.Content = "-";
            WorkerCommonTimeStatOpLabel.Content = "-";
            MentorCommonTimeStatOpLabel.Content = "-";
            StudentCommonTimeStatOpLabel.Content = "-";
            TotalOperationsTimeStatLabel.Content = "-";

            DateCommonTimeStatOpLabel.Content = String.Format("{0} - {1}", _fromDateTime.ToShortDateString(),
                _toDateTime.ToShortDateString());

            ProgressGrid.Visibility = Visibility.Visible;

            _tcc.ResetStatisticsObjects();

            statBackgroundWorker.DoWork += (s, ev) =>
            {

                #region local_variables

                var shiftsDataView = _tcc.GetShifts();
                var timeTrackingDataView = _tcc.GetTimeTracking();
                var taskTimeTrackingDataView = _taskClass.TaskTimeTracking.Table.AsDataView();

                #endregion

                List<int> workerIDs = (!_multWorkerStatMode
                    ? new List<int> {_selectedWorkerID}
                    : _multWorkersStatList);

                foreach (var workerId in workerIDs)
                {
                    string workerName = _idToNameConverter.Convert(workerId, "ShortName");

                    if (_splitWorkerStatByDate)
                    {
                        shiftsDataView.RowFilter = "WorkerID=" + workerId;

                        int shiftsCount = shiftsDataView.Count;

                        decimal onePercent = shiftsCount != 0 ? (decimal) 100/shiftsCount : 100;
                        decimal currentPercent = 0;

                        foreach (DataRowView shiftDataRowView in shiftsDataView)
                        {
                            IEnumerable<DataRow> statisticsDRs = _tcc.CalculateStatisticsForOneShift(shiftDataRowView,
                                workerId,
                                timeTrackingDataView, taskTimeTrackingDataView, workerName);

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                foreach (DataRow sdr in statisticsDRs.Where(sdr => sdr.RowState != DataRowState.Added))
                                {
                                    _tcc.WorkersStatDataTable.Rows.Add(sdr);
                                }

                                _tcc.WorkersStatDataTable.Rows.Add();
                            }));

                            //Thread.Sleep(50);

                            currentPercent = currentPercent + onePercent;

                            statBackgroundWorker.ReportProgress(Convert.ToInt32(currentPercent));
                        }
                    }
                    else
                    {
                        var filerByWorker = String.Format("DeleteRecord <> 'True' AND WorkerID={0}", workerId);

                        timeTrackingDataView.RowFilter = filerByWorker;
                        taskTimeTrackingDataView = _taskClass.TaskTimeTracking.Table.AsEnumerable().
                            Where(t =>
                                t.Field<DateTime>("Date").Date >= _tcc.GetDateFrom().Date && 
                                t.Field<DateTime>("Date").Date <= _tcc.GetDateTo().Date 
                                &&
                                _taskClass.Performers.Table.AsEnumerable()
                                    .Any(p => p.Field<Int64>("WorkerID") == workerId &&
                                              p.Field<Int64>("PerformerID") == t.Field<Int64>("PerformerID")))
                            .AsDataView();

                        var distinctOperationIDs = _tcc.GetDistinctOperationIds(timeTrackingDataView);
                        var distinctTaskIDs = _tcc.GetDistinctTaskIds(taskTimeTrackingDataView);

                        var distOperationsCount = distinctOperationIDs.Count + distinctTaskIDs.Count;
                        decimal onePercent = distOperationsCount != 0 ? (decimal) 100/distOperationsCount : 100;

                        decimal currentPercent = 0;

                        var statisticsDRs = new List<DataRow>();

                        var timeTrackingDataRows =
                            timeTrackingDataView.Table.Select(String.Format(filerByWorker));
                        var taskTimeTrackingDataRows = taskTimeTrackingDataView.ToTable().Select();

                        foreach (var operationId in distinctOperationIDs)
                        {
                            _tcc.CalculateStatisticsForOneOperation(operationId, timeTrackingDataRows, workerName,
                                statisticsDRs);
                        }

                        foreach (var taskId in distinctTaskIDs)
                        {
                            _tcc.CalculateStatisticsForOneTask(taskId, taskTimeTrackingDataRows, workerName,
                                statisticsDRs);
                        }

                        decimal totalShiftTime = _tcc.CalculateTotalTimeForShift(statisticsDRs);
                        decimal commonFVShiftTime = _tcc.CalculateFirstVerificationTimeForShift(statisticsDRs);
                        decimal commonSVShiftTime = _tcc.CalculateSecondVerificationTimeForShift(statisticsDRs);
                        decimal commonTVShiftTime = _tcc.CalculateThirdVerificationTimeForShift(statisticsDRs);
                        decimal commonAllVShiftTime = _tcc.CalculateAllVerificationTimeForShift(statisticsDRs);
                        decimal commonOneOfVShiftTime = _tcc.CalculateOneOffVerificationTimeForShift(statisticsDRs);

                        _tcc.WorkersStatValuesStuct.CommonTime = _tcc.WorkersStatValuesStuct.CommonTime + totalShiftTime;
                        _tcc.WorkersStatValuesStuct.FVCommonTime = _tcc.WorkersStatValuesStuct.FVCommonTime +
                                                                   commonFVShiftTime;
                        _tcc.WorkersStatValuesStuct.SVCommonTime = _tcc.WorkersStatValuesStuct.SVCommonTime +
                                                                   commonSVShiftTime;
                        _tcc.WorkersStatValuesStuct.TVCommonTime = _tcc.WorkersStatValuesStuct.TVCommonTime +
                                                                   commonTVShiftTime;
                        _tcc.WorkersStatValuesStuct.AllVCommonTime = _tcc.WorkersStatValuesStuct.AllVCommonTime +
                                                                     commonAllVShiftTime;
                        _tcc.WorkersStatValuesStuct.OneOfVCommonTime = _tcc.WorkersStatValuesStuct.OneOfVCommonTime +
                                                                       commonOneOfVShiftTime;

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            foreach (DataRow sdr in statisticsDRs.Where(sdr => sdr.RowState != DataRowState.Added))
                            {
                                _tcc.WorkersStatDataTable.Rows.Add(sdr);
                            }

                            _tcc.WorkersStatDataTable.Rows.Add();
                        }));

                        currentPercent = currentPercent + onePercent;

                        statBackgroundWorker.ReportProgress(Convert.ToInt32(currentPercent));
                    }
                }
            };

            statBackgroundWorker.RunWorkerCompleted += (s, ev) =>
            {
                CalculateOperationsStatisticsButton.IsEnabled = true;
                ProgressGrid.Visibility = Visibility.Hidden;

                AddToWorkerListButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                SplitByDatesCheckBox.IsEnabled = true;

                TotalOperationsTimeStatLabel.Content = _tcc.WorkersStatValuesStuct.OperationTotalTime;
                WorkerCommonTimeStatOpLabel.Content = _tcc.WorkersStatValuesStuct.WorkerCommonTime;
                MentorCommonTimeStatOpLabel.Content = _tcc.WorkersStatValuesStuct.MentorCommonTime;
                StudentCommonTimeStatOpLabel.Content = _tcc.WorkersStatValuesStuct.StudentCommonTime;
                TotalTasksTimeStatLabel.Content = _tcc.WorkersStatValuesStuct.TasksTotalTime;

                FillCommonTimeOpStatComboBox(false);

                FillStatFilters();

                statBackgroundWorker.Dispose();

                //if (_multWorkerStatMode) RemoveAllFromWorkersListButton_Click(null, null);
            };


            statBackgroundWorker.ProgressChanged += (s, pe) =>
            {
                ProgressCommOpStatProgressBar.Value = pe.ProgressPercentage;
                ProgressCommOpStatLabel.Content = pe.ProgressPercentage.ToString(CultureInfo.InvariantCulture) + " %";
            };

            CalculateOperationsStatisticsButton.IsEnabled = false;

            statBackgroundWorker.RunWorkerAsync();
        }

        private void FillCommonTimeOpStatComboBox(bool reset)
        {
            CommonTimeOpStatComboBox.Items.Clear();

            if (reset)
            {
                CommonTimeOpStatComboBox.Items.Add("Введенное время: --");
                CommonTimeOpStatComboBox.Items.Add("1-ое подтверждение: --");
                CommonTimeOpStatComboBox.Items.Add("2-ое подтверждение: --");
                CommonTimeOpStatComboBox.Items.Add("Табельное подтверждение: --");
                CommonTimeOpStatComboBox.Items.Add("Частично подтверждено: --");
                CommonTimeOpStatComboBox.Items.Add("Полностью подтверждено: --");
            }
            else
            {
                CommonTimeOpStatComboBox.Items.Add("Введенное время: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.CommonTime, 2) + " ч.");
                CommonTimeOpStatComboBox.Items.Add("1-ое подверждение: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.FVCommonTime, 2) +
                                                   " ч.");
                CommonTimeOpStatComboBox.Items.Add("2-ое подверждение: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.SVCommonTime, 2) +
                                                   " ч.");
                CommonTimeOpStatComboBox.Items.Add("Табельное подверждение: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.TVCommonTime, 2) +
                                                   " ч.");
                CommonTimeOpStatComboBox.Items.Add("Частично подтверждено: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.OneOfVCommonTime, 2) + " ч.");
                CommonTimeOpStatComboBox.Items.Add("Полностью подтверждено: " +
                                                   Math.Round(_tcc.WorkersStatValuesStuct.AllVCommonTime, 2) +
                                                   " ч.");
            }

            CommonTimeOpStatComboBox.SelectedIndex = 0;
        }

        private void StatisticsTimeTrackingDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if ((((DataRowView) e.Row.Item)["FactoryID"] != DBNull.Value)) return;

            if ((((DataRowView) e.Row.Item)["TaskID"] != DBNull.Value))
            {
                //e.Row.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom("#226CCF70"));
            }
            else if ((((DataRowView) e.Row.Item)["Date"] != DBNull.Value))
            {
                e.Row.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom("#FFEBEBEB"));
            }
        }

        private void FormatStatisticsTimeTrackingDataGrid()
        {

            if (StatisticsTimeTrackingDataGrid.Items.Count == 0) return;

            for (int i = 0; i < StatisticsTimeTrackingDataGrid.Items.Count; i++)
            {

                var row =
                    (DataGridRow)StatisticsTimeTrackingDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row == null)
                {
                    StatisticsTimeTrackingDataGrid.UpdateLayout();
                    StatisticsTimeTrackingDataGrid.ScrollIntoView(StatisticsTimeTrackingDataGrid.Items[i]);
                    row = (DataGridRow)StatisticsTimeTrackingDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                }

                if ((((DataRowView)row.Item)["FactoryID"] != DBNull.Value)) return;

                if ((((DataRowView)row.Item)["TaskID"] != DBNull.Value))
                {
                    //row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#226CCF70"));
                }
                else if ((((DataRowView)row.Item)["Date"] != DBNull.Value))
                {
                    row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFEBEBEB"));
                }
            }
        }

        #endregion


        #region Modes switch

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }

        private void TimeControlRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;
            ModesFilterTabControl.SelectedIndex = 0;
            WorkersNamesListBox.SelectedIndex = 0;
            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;
            WorkersNamesListBox_SelectionChanged(null, null);
            ModesWorkersStatTabControl.SelectedIndex = 0;

            CloseAppBar();
        }

        private void WorkerStatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ModesFilterTabControl.SelectedIndex = 0;
            WorkersNamesListBox.SelectedIndex = 0;
            ModesWorkersStatTabControl.SelectedIndex = 1;
            CloseAppBar();

            WorkersNamesListBox_SelectionChanged(null, null);
        }

        private void CommonOperationsStatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;
            ModesFilterTabControl.SelectedIndex = 1;
            ModesWorkersStatTabControl.SelectedIndex = 2;
            CloseAppBar();
        }

        //private void ProgramEntryJournalToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //// Setting enabled for toggle buttons
        //AddButtonRow.Height = new GridLength(0);
        //WorkersNameListBox.SelectionMode = SelectionMode.Single;
        //TabControl.SelectedIndex = 0;
        //CloseAppBar();

        //_workingMode = Mode.Statistics;
        //WorkersNameListBox_SelectionChanged(null, null);
        //}

        //private void WorkerRightsToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //// Setting enabled for toggle buttons
        //AddButtonRow.Height = new GridLength(1, GridUnitType.Auto);
        //WorkersNameListBox.SelectionMode = SelectionMode.Extended;
        //TabControl.SelectedIndex = 1;
        //CloseAppBar();

        //_workingMode = Mode.WorkerAccess;
        //WorkersNameListBox_SelectionChanged(null, null);
        //}


        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }

        #endregion



        #region CommonOperationsStat

        #region Filter

        private void FactoryFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UnitsFilterCheckBox.IsChecked = false;
        }

        private void UnitsFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SectionsFilterCheckBox.IsChecked = false;
        }

        private void SectionsFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SubsectionsFilterCheckBox.IsChecked = false;
        }

        private void SubsectionsFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OperationsFilterCheckBox.IsChecked = false;
        }

        private void FactoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoryFilterComboBox.SelectedValue == null) return;

            ((DataView) UnitsFilterComboBox.ItemsSource).RowFilter = "FactoryID=" + FactoryFilterComboBox.SelectedValue;

            UnitsFilterComboBox.SelectedIndex = 0;
            UnitsFilterComboBox_SelectionChanged(null, null);
        }

        private void UnitsFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnitsFilterComboBox.SelectedValue == null) return;
            ((DataView) SectionsFilterComboBox.ItemsSource).RowFilter = "WorkUnitID=" +
                                                                        UnitsFilterComboBox.SelectedValue;
            SectionsFilterComboBox.SelectedIndex = 0;
            SectionsFilterComboBox_SelectionChanged(null, null);
        }

        private void SectionsFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SectionsFilterComboBox.SelectedValue == null) return;

            ((DataView) SubsectionsFilterComboBox.ItemsSource).RowFilter = "WorkSectionID=" +
                                                                           SectionsFilterComboBox.SelectedValue;
            SubsectionsFilterComboBox.SelectedIndex = 0;
            SubsectionsFilterComboBox_SelectionChanged(null, null);
        }

        private void SubsectionsFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubsectionsFilterComboBox.SelectedValue == null) return;

            ((DataView) OperationsFilterComboBox.ItemsSource).RowFilter = "WorkSubsectionID=" +
                                                                          SubsectionsFilterComboBox.SelectedValue;
            OperationsFilterComboBox.SelectedIndex = 0;
        }

        private string FilterCommonStatistics()
        {
            var filterString = string.Empty;

            if (FactoryFilterCheckBox.IsChecked == true)
            {
                if (filterString == string.Empty)
                {
                    filterString = "FactoryID=" + FactoryFilterComboBox.SelectedValue;
                }
                else
                {
                    filterString = filterString + " AND FactoryID=" + FactoryFilterComboBox.SelectedValue;
                }
            }

            if (UnitsFilterCheckBox.IsChecked == true)
            {
                if (filterString == string.Empty)
                {
                    filterString = "WorkUnitID=" + UnitsFilterComboBox.SelectedValue;
                }
                else
                {
                    filterString = filterString + " AND WorkUnitID=" + UnitsFilterComboBox.SelectedValue;
                }
            }

            if (SectionsFilterCheckBox.IsChecked == true)
            {
                if (filterString == string.Empty)
                {
                    filterString = "WorkSectionID=" + SectionsFilterComboBox.SelectedValue;
                }
                else
                {
                    filterString = filterString + " AND WorkSectionID=" + SectionsFilterComboBox.SelectedValue;
                }
            }

            if (SubsectionsFilterCheckBox.IsChecked == true)
            {
                if (filterString == string.Empty)
                {
                    filterString = "WorkSubsectionID=" + SubsectionsFilterComboBox.SelectedValue;
                }
                else
                {
                    filterString = filterString + " AND WorkSubsectionID=" + SubsectionsFilterComboBox.SelectedValue;
                }
            }

            if (OperationsFilterCheckBox.IsChecked == true)
            {
                if (filterString == string.Empty)
                {
                    filterString = "WorkOperationID=" + OperationsFilterComboBox.SelectedValue;
                }
                else
                {
                    filterString = filterString + " AND WorkOperationID=" + OperationsFilterComboBox.SelectedValue;
                }
            }

            return filterString;
        }

        #endregion

        private void CalculateOperationsCommonStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var bw = new BackgroundWorker {WorkerReportsProgress = true};

            ProgressCommStatGrid.Visibility = Visibility.Visible;

            CalculateOperationsCommonStatisticsButton.IsEnabled = false;
            ExportCommonStatisticsButton.IsEnabled = false;

            bw.DoWork += (obj, ea) => Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (ShiftFilterCheckBox.IsChecked == true)
                {
                    int shiftNumber = -1;
                    if (FirstShiftFilterRadioButton.IsChecked == true)
                        shiftNumber = 1;
                    if (SecondShiftFilterRadioButton.IsChecked == true)
                        shiftNumber = 2;
                    if (ThirdShiftFilterRadioButton.IsChecked == true)
                        shiftNumber = 3;
                    
                    _tcc.CalculateOperationsCommonStatistics(FilterCommonStatistics(), _fromDateTime, _toDateTime,
                        shiftNumber);
                }
                else
                {
                    _tcc.CalculateOperationsCommonStatistics(FilterCommonStatistics(), _fromDateTime, _toDateTime);
                }

                _tcc.CalculateCommonStatTime();

                FillCommonTimeCommStatComboBox(false);

                OperationsCountLabel.Content = CommonStatisticsTimeTrackingDataGrid.Items.Count;
            }));

            _tcc.ProgressChanged += (s, pe) => Dispatcher.BeginInvoke(new Action(() =>
            {
                CommStatProgressBar.Value = pe.ProgressPercentage;
                ProgressCommStatLabel.Content = pe.ProgressPercentage.ToString(CultureInfo.InvariantCulture) + " %";
                DispatcherHelper.DoEvents();

            }));

            _tcc.RunWorkerCompleted += (s, pe) =>
            {
                ProgressCommStatGrid.Visibility = Visibility.Hidden;
                CalculateOperationsCommonStatisticsButton.IsEnabled = true;
                ExportCommonStatisticsButton.IsEnabled = true;

                FillCommStatFilters();

                bw.Dispose();
            };

            FillCommonTimeCommStatComboBox(true);

            OperationsCountLabel.Content = "--";

            bw.RunWorkerAsync();
        }

        private void FillCommonTimeCommStatComboBox(bool reset)
        {
            CommonTimeCommStatComboBox.Items.Clear();

            if (reset)
            {
                CommonTimeCommStatComboBox.Items.Add("Введенное время: --");
                CommonTimeCommStatComboBox.Items.Add("1-ое подтверждение: --");
                CommonTimeCommStatComboBox.Items.Add("2-ое подтверждение: --");
                CommonTimeCommStatComboBox.Items.Add("Табельное подтверждение: --");
                CommonTimeCommStatComboBox.Items.Add("Частично подтверждено: --");
                CommonTimeCommStatComboBox.Items.Add("Полностью подтверждено: --");
            }
            else
            {
                CommonTimeCommStatComboBox.Items.Add("Введенное время: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.CommonTime, 2) + " ч.");
                CommonTimeCommStatComboBox.Items.Add("1-ое подтверждение: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.FVCommonTime, 2) + " ч.");
                CommonTimeCommStatComboBox.Items.Add("2-ое подтверждение: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.SVCommonTime, 2) + " ч.");
                CommonTimeCommStatComboBox.Items.Add("Табельное подтверждение: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.TVCommonTime, 2) + " ч.");
                CommonTimeCommStatComboBox.Items.Add("Частично подтверждено: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.OneOfVCommonTime, 2) + " ч.");
                CommonTimeCommStatComboBox.Items.Add("Полностью подтверждено: " +
                                                     Math.Round(_tcc.WorkersStatValuesStuct.AllVCommonTime, 2) + " ч.");
            }

            CommonTimeCommStatComboBox.SelectedIndex = 0;
        }

        private void CommonShowVerificationLevelsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommonShowHideVerificationLevels(CommonShowVerificationLevelsCheckBox.IsChecked == true);
        }

        private void CommonShowHideVerificationLevels(bool show)
        {
            if (show)
            {
                foreach (var dataGridColumn in CommonStatisticsTimeTrackingDataGrid.Columns.Where(dataGridColumn =>
                    (Equals(dataGridColumn.Header, "1-ый ур.")) ||
                    (Equals(dataGridColumn.Header, "2-ой ур.")) ||
                    (Equals(dataGridColumn.Header, "3-ий ур.")) ||
                    (Equals(dataGridColumn.Header, "Частично")) ||
                    (Equals(dataGridColumn.Header, "Полностью подтвержденное время"))))
                {
                    dataGridColumn.Visibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (var dataGridColumn in CommonStatisticsTimeTrackingDataGrid.Columns.Where(dataGridColumn =>
                    (Equals(dataGridColumn.Header, "1-ый ур.")) ||
                    (Equals(dataGridColumn.Header, "2-ой ур.")) ||
                    (Equals(dataGridColumn.Header, "3-ий ур.")) ||
                    (Equals(dataGridColumn.Header, "Частично")) ||
                    (Equals(dataGridColumn.Header, "Полностью подтвержденное время"))))
                {
                    dataGridColumn.Visibility = Visibility.Hidden;
                }
            }
        }

        #endregion

        private void ExportCommonStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel.GenerateCommonOperationStatisticsReport(ref CommonStatisticsTimeTrackingDataGrid,
                CommonTimeCommStatComboBox.Items);
            AdministrationClass.AddNewAction(69);
        }


        #region multUsers
        private void AddToWorkerListButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNamesListBox.SelectedItem == null) return;

            int workerID;

            if (!int.TryParse(WorkersNamesListBox.SelectedValue.ToString(), out workerID)) return;
            if (_multWorkersStatList.Contains(workerID)) return;

            if (MultWorkersGrid.Visibility == Visibility.Collapsed)
            {
                _multWorkersStatList.Clear();
                MultWorkersGrid.Visibility = Visibility.Visible;
                _multWorkerStatMode = true;
            }

            _multWorkersStatList.Add(workerID);
            MultWorkersListBox.Items.Refresh();
            CountSelectedWorkers.Content = MultWorkersListBox.Items.Count;
        }


        private void RemoveFromWorkersListButton_Click(object sender, RoutedEventArgs e)
        {
            if (MultWorkersListBox.SelectedItem == null) return;

            _multWorkersStatList.Remove((int) MultWorkersListBox.SelectedItem);
            MultWorkersListBox.Items.Refresh();
            CountSelectedWorkers.Content = MultWorkersListBox.Items.Count;

            if (_multWorkersStatList.Count == 0)
            {
                MultWorkersGrid.Visibility = Visibility.Collapsed;
                _multWorkerStatMode = false;
            }
        }


        private void RemoveAllFromWorkersListButton_Click(object sender, RoutedEventArgs e)
        {
            _multWorkersStatList.Clear();
            MultWorkersGrid.Visibility = Visibility.Collapsed;
            _multWorkerStatMode = false;
        }

        #endregion

        private void ModesWorkersStatTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddToWorkerListButton.Visibility = ModesWorkersStatTabControl.SelectedIndex != 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void WorkersNamesListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddToWorkerListButton_Click(null, null);
        }

        private void SplitByDatesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _splitWorkerStatByDate = SplitByDatesCheckBox.IsChecked == true;

            CalculateOperationsStatisticsButton_Click(null, null);
        }

        private void ResetCommStatFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCommStatTimeTrackingFilter();
        }



        private void OnTaskTimeTrackingVerificationButtonClick(object sender, RoutedEventArgs e)
        {
            var taskTimeTracking = WorkerTaskTimeTrackingListBox.SelectedItem as DataRowView;
            if (taskTimeTracking == null) return;

            var taskId = Convert.ToInt32(taskTimeTracking["TaskID"]);
            var tasks = _taskClass.Tasks.Table.AsEnumerable().Where(t => t.Field<Int64>("TaskID") == taskId);
            if(!tasks.Any()) return;

            var task = tasks.First();
            var mainWorkerId = Convert.ToInt32(task["MainWorkerID"]);
            if (mainWorkerId != AdministrationClass.CurrentWorkerId)
            {
                MessageBox.Show(
                    "Вы не являетесь постановщиком данной задачи, поэтому не можете подтвердить выбранное время.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            taskTimeTracking["VerificationDate"] = currentDate;
            taskTimeTracking["VerificationWorkerID"] = AdministrationClass.CurrentWorkerId;
            taskTimeTracking["IsVerificated"] = true;

            AdministrationClass.AddNewAction(65);
        }

        private void ExportButton_Click(object sender, MouseButtonEventArgs e)
        {
            var date = "с " + FromDatePicker + " по " + ToDatePicker;
            var workerName = _multWorkerStatModeForExport
                ? "-"
                : ((DataRowView)WorkersNamesListBox.SelectedItem).Row["Name"].
                    ToString();

            ExportToExcel.GenerateOperationStatisticsReport(ref StatisticsTimeTrackingDataGrid, workerName, date,
                CommonTimeOpStatComboBox.Items);
            AdministrationClass.AddNewAction(67);
        }

        private void OnShiftStatusticsReportClick(object sender, MouseButtonEventArgs e)
        {
            var workerIds = (!_multWorkerStatMode
                    ? new List<int> { _selectedWorkerID }
                    : _multWorkersStatList);

            ExportToExcel.GenerateShiftStatisticsReport(workerIds);
            AdministrationClass.AddNewAction(68);
        }
    }
}
