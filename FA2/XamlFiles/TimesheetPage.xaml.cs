using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.Converters;
using FAIIControlLibrary.CustomControls;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для TimesheetPage.xaml
    /// </summary>

    public partial class TimesheetPage
    {
        private bool _firstRun = true;

        private DateTime _currentDate;

        private Label _currentAllLabel;
        private Label _currentShiftLabel;
        private Label _currentNightHoursLabel;

        private CalendarDayButton _currentCalendarDayButton;

        private PlannedScheduleClass _pSc;

        //public static WorkersClass _wc;
        private StaffClass _sc;

        public static ProductionCalendarClass _pcc;

        public TimeTrackingClass _ttc;

        public static TimeSheetClass _tsc;

        private DataRow _currentScheduleDataRow;

        private IdToNameConverter _idToNameConv;
        //private MainUserIDtoNameConverter _mainUserIdToNameConv;

        private DateConverter _dateConv;

        private decimal _totalHours;
        private decimal _totalNightHours;
        private int _workingDaysCount;

        private decimal _currentHours;
        private decimal _currentNightHours;

        private DataView _workersDataView;

        private int _standartTime;

        private BackgroundWorker _timeSheetBackgroundWorker;

        private ScrollViewer _workerProfessionsDataGridScrollViewer;
        private ScrollViewer _timeSheetDataGridScrollViewer;
        private ScrollViewer _timesheetSummStatDataGridScrollViewer;

        private int _selectedTimesheetMonth = DateTime.Now.Month;
        private int _selectedTimesheetYear = DateTime.Now.Year;

        private ExportToExcel _exportToExcel;

        public TimesheetPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_firstRun)
            {
                FillData();
                Bindings();
                
                ActualTimeSheetToggleButton.IsChecked = true;

                CloseShowAppBar(true);
                _firstRun = false;

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillData()
        {
            App.BaseClass.GetPlannedScheduleClass(ref _pSc);
            App.BaseClass.GetStaffClass(ref _sc);

            App.BaseClass.GetProductionCalendarClass(ref _pcc);
            App.BaseClass.GetTimeTrackingClass(ref _ttc);
            App.BaseClass.GetTimeSheetClass(ref _tsc);

            App.BaseClass.GetExportToExcelClass(ref _exportToExcel);

            GetDataGridScrolls();

            _idToNameConv = new IdToNameConverter();
            //_mainUserIdToNameConv = new MainUserIDtoNameConverter();

            _dateConv = new DateConverter();
        }

        private void Bindings()
        {
            _currentDate = App.BaseClass.GetDateFromSqlServer();

            _tsc.HolidaysDataTable = _pcc.HolidaysDataTable;

            BindingDate();
            BindingData();
        }

        private void BindingDate()
        {
            for (int i = 2013; i < _currentDate.Year + 1; i++)
                YearComboBox.Items.Add(i);

            YearComboBox.SelectedItem = _currentDate.Year;

            var ci = new CultureInfo("ru-RU");
            var dtformatInfo = ci.DateTimeFormat;
            for (int i = 1; i <= 12; i++)
            {
                MonthComboBox.Items.Add(dtformatInfo.GetMonthName(i));
            }

            MonthComboBox.SelectedIndex = _currentDate.Month - 1;

            for (int i = 2013; i < _currentDate.Year + 1; i++)
            {
                TimeSheetYearComboBox.Items.Add(i);
                TempTimeSheetYearComboBox.Items.Add(i);
            }

            TimeSheetYearComboBox.SelectedItem = _currentDate.Year;
            TempTimeSheetYearComboBox.SelectedItem = _currentDate.Year;

            for (int i = 1; i <= 12; i++)
            {
                TimeSheetMonthComboBox.Items.Add(dtformatInfo.GetMonthName(i));
                TempTimeSheetMonthComboBox.Items.Add(i);
            }

            TimeSheetMonthComboBox.SelectedIndex = _currentDate.Month - 1;
            TempTimeSheetMonthComboBox.SelectedIndex = _currentDate.Month - 1;
        }

        private void BindingData()
        {
            #region PlannedSchedule

            WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;
            WorkersNameListBox.ItemsSource = _sc.GetStaffPersonalInfo();
            WorkersNameListBox.SelectedValuePath = "WorkerID";
            WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;
            WorkersNameListBox.Items.MoveCurrentToFirst();

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
            #endregion

            #region TimeSheet

            TimeSheetWorkersGroupsComboBox.SelectionChanged -= TimeSheetWorkersGroupsComboBox_SelectionChanged;
            TimeSheetWorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            TimeSheetWorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            TimeSheetWorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            TimeSheetWorkersGroupsComboBox.SelectedValue = 2;
            TimeSheetWorkersGroupsComboBox.SelectionChanged += TimeSheetWorkersGroupsComboBox_SelectionChanged;

            TimeSheetFactoriesComboBox.SelectionChanged -= TimeSheetFactoriesComboBox_SelectionChanged;
            TimeSheetFactoriesComboBox.DisplayMemberPath = "FactoryName";
            TimeSheetFactoriesComboBox.SelectedValuePath = "FactoryID";
            TimeSheetFactoriesComboBox.ItemsSource = _sc.GetFactories();
            TimeSheetFactoriesComboBox.SelectedIndex = 0;
            TimeSheetFactoriesComboBox.SelectionChanged += TimeSheetFactoriesComboBox_SelectionChanged;
            TimeSheetFactoriesComboBox_SelectionChanged(null, null);

           


            #endregion

            WorkerProfessionsDataGrid.ItemsSource = _tsc.TimesheetDataTable.DefaultView;
            TimeSheetDataGrid.ItemsSource = _tsc.TimesheetDataTable.DefaultView;
            TimesheetStatDataGrid.ItemsSource = _tsc.TimesheetDataTable.DefaultView;

            AddButtonsOnAbsencesWrapPanel();

            BindingAbsences();
        }

        private void BindingAbsences()
        {
            _tsc.AbsencesTypesViewSource.SortDescriptions.Add(new SortDescription("AbsencesName",
                ListSortDirection.Ascending));
            AbsencesListBox.ItemsSource = _tsc.AbsencesTypesViewSource;
            AbsencesListBox.DisplayMemberPath = "AbsencesName";
            AbsencesListBox.SelectedValuePath = "AbsencesTypeID";
            AbsencesListBox.SelectionChanged += AbsencesListBox_SelectionChanged;
            if (AbsencesListBox.Items.Count != 0)
                AbsencesListBox.SelectedIndex = 0;
        }

        private void CalendarDayButton_Click(object sender, RoutedEventArgs e)
        {
            var cdb = ((CalendarDayButton) sender);

            if (!cdb.IsSelected)
            {
                EditDayButtonBorder.Visibility = Visibility.Hidden;
                return;
            }

            _currentCalendarDayButton = cdb;

            _currentCalendarDayButton.SizeChanged += (obj, ea) =>
            {
                EditDayButtonBorder.Width = ((CalendarDayButton) obj).ActualWidth;
                EditDayButtonBorder.Height = ((CalendarDayButton) obj).ActualHeight;

                Point p = _currentCalendarDayButton.TranslatePoint(new Point(0, 0), CalendarMainGrid);
                EditDayButtonBorder.Margin = new Thickness(p.X, p.Y - 40, 0, 0);
            };

            double w = _currentCalendarDayButton.ActualWidth;
            EditDayButtonBorder.Width = w;
            double h = _currentCalendarDayButton.ActualHeight;
            EditDayButtonBorder.Height = h;

            var gr1 = VisualTreeHelper.GetChild(cdb, 0) as Grid;

            if (gr1 != null)
            {
                var gr2 = VisualTreeHelper.GetChild(gr1, 0) as Grid;
                if (gr2 != null)
                {
                    var txtbl = VisualTreeHelper.GetChild(gr2, 0) as TextBlock;
                    if (txtbl != null) DayNumberTextBlock.Text = txtbl.Text;
                }
            }

            if (gr1 != null)
            {
                var gr3 = VisualTreeHelper.GetChild(gr1, 1) as Grid;
                if (gr3 != null)
                {
                    _currentAllLabel = VisualTreeHelper.GetChild(gr3, 0) as Label;
                    _currentShiftLabel = VisualTreeHelper.GetChild(gr3, 1) as Label;
                    _currentNightHoursLabel = VisualTreeHelper.GetChild(gr3, 2) as Label;
                }
            }

            if (_currentAllLabel != null && _currentAllLabel.Content.ToString() != "-")
            {
                _currentHours = Convert.ToDecimal(_currentAllLabel.Content.ToString().Replace(".", ","));

                if (_currentNightHoursLabel != null)
                    _currentNightHours = Convert.ToDecimal(_currentNightHoursLabel.Content.ToString().Replace(".", ","));

                AllTextBox.Value = _currentHours;

                if (_currentShiftLabel != null)
                    ShiftTextBox.Value = Convert.ToDecimal(_currentShiftLabel.Content.ToString().Replace(".", ","));
                NightHoursTextBox.Value = _currentNightHours;

                Point startPoint = cdb.TranslatePoint(new Point(0, 0), CalendarMainGrid);
                EditDayButtonBorder.Margin = new Thickness(startPoint.X, startPoint.Y - 40, 0, 0);

                EditDayButtonBorder.Visibility = Visibility.Visible;
            }
        }

        private void WorkersNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersNameListBox.Items.Count == 0) return;

            SavePlannedScheduleButton.IsEnabled = true;

            if (WorkersNameListBox.SelectedValue != null)
            {
                DataRow[] dataRows =
                    _pSc.PlannedScheduleDataTable.Select("WorkerID = " + WorkersNameListBox.SelectedValue);

                _currentScheduleDataRow = dataRows.Length != 0 ? dataRows[0] : null;

                SelectedWorkersCountLabel.Content = WorkersNameListBox.SelectedItems.Count;
                MultisavePlannedScheduleButton.Content = WorkersNameListBox.SelectedItems.Count;
                SavePlannedScheduleButton.Content =
                    GetShortName(((DataRowView) (WorkersNameListBox.SelectedItem))["Name"].ToString());

                MultisavePlannedScheduleButton.Visibility = WorkersNameListBox.SelectedItems.Count < 2
                    ? Visibility.Hidden
                    : Visibility.Visible;

                if (_currentScheduleDataRow != null)
                {
                    var name = _idToNameConv.Convert(_currentScheduleDataRow["EditingWorkerID"], "ShortName");
                    EditingInfoLabel.Content = name + "  " + _currentScheduleDataRow["EditingDate"];
                }
                else
                {
                    EditingInfoLabel.Content = "-";
                }

                FillPlannedScheduleCalendar();
            }
        }

        private string GetShortName(string fullName)
        {
            string shortName = string.Empty;
            string[] fio = fullName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (fio.Length != 0)
            {
                bool first = true;

                foreach (string s in fio.Where(s => s.Length > 1))
                {
                    if (!first)
                        shortName += " " + s.Remove(1) + ".";
                    else
                    {
                        shortName += s;
                        first = false;
                    }
                }
            }
            else
            {
                shortName = "--";
            }

            return shortName;
        }

        private void WorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FilterWorkers()
        {
            WorkersNameListBox.SelectionChanged -= WorkersNameListBox_SelectionChanged;

            WorkersNameListBox.ItemsSource =
                _sc.FilterWorkers(true, Convert.ToInt32(WorkersGroupsComboBox.SelectedValue), true,
                    Convert.ToInt32(FactoriesComboBox.SelectedValue), false, 0).DefaultView;

            ((DataView)WorkersNameListBox.ItemsSource).RowFilter = "AvailableInList = 'True'";



            //WorkersNameListBox.SelectedIndex = 0;

            WorkersNameListBox.SelectionChanged += WorkersNameListBox_SelectionChanged;

            WorkersNameListBox_SelectionChanged(null, null);
        }

        private void FillPlannedScheduleCalendar(bool reset = false)
        {
            _totalHours = 0;
            _totalNightHours = 0;
            _workingDaysCount = 0;

            var calendarGrid = VisualTreeHelper.GetChild(PlannedScheduleCalendar, 0) as Grid;
            var ci = VisualTreeHelper.GetChild(calendarGrid, 0) as CalendarItem;
            var ciGrid = VisualTreeHelper.GetChild(ci, 0) as Grid;
            var ciBorder = VisualTreeHelper.GetChild(ciGrid, 0) as Border;
            var ci2Grid = VisualTreeHelper.GetChild(ciBorder, 0) as Grid;
            var ci3Grid = VisualTreeHelper.GetChild(ci2Grid, 1) as Grid;

            var childrenCount = VisualTreeHelper.GetChildrenCount(ci3Grid);

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(ci3Grid, i);

                if ((child != null) && (child.GetType().Name == "CalendarDayButton"))
                {
                    var tempCalendarDayButton = (CalendarDayButton) child;

                    var dayNumberTextBlock = tempCalendarDayButton.Template.FindName("DayNumberTextBlock",
                        (CalendarDayButton) child);

                    if ((dayNumberTextBlock == null) || (((TextBlock) dayNumberTextBlock).Text == string.Empty))
                        continue;

                    var txt1 = tempCalendarDayButton.Template.FindName("txt1", (CalendarDayButton) child) as Label;
                    var txt2 = tempCalendarDayButton.Template.FindName("txt2", (CalendarDayButton) child) as Label;
                    var txt3 = tempCalendarDayButton.Template.FindName("txt3", (CalendarDayButton) child) as Label;

                    if (!reset)
                    {
                        #region Fill

                        decimal hours = Math.Round(8m, 2);
                        decimal shiftNumber = Math.Round(1m, 2);
                        decimal nightHours = Math.Round(0m, 2);

                        if (_currentScheduleDataRow == null)
                        {
                            #region

                            switch (Convert.ToInt32(((TextBlock) dayNumberTextBlock).Tag))
                            {
                                case 0:
                                {
                                    hours = 8.00m;
                                    shiftNumber = 1;
                                    nightHours = 0.00m;
                                    break;
                                }

                                case 1:
                                {
                                    hours = 7.00m;
                                    shiftNumber = 1;
                                    nightHours = 0.00m;
                                    break;
                                }
                                case 2:
                                {
                                    hours = 0.00m;
                                    shiftNumber = 0;
                                    nightHours = 0.00m;
                                    break;
                                }
                                case 3:
                                {
                                    hours = 0.00m;
                                    shiftNumber = 0;
                                    nightHours = 0.00m;
                                    break;
                                }
                            }

                            if (txt1 != null) txt1.Content = hours;
                            if (txt2 != null) txt2.Content = shiftNumber;
                            if (txt3 != null) txt3.Content = nightHours;

                            if ((hours != 0) || (nightHours != 0))
                                _workingDaysCount ++;

                            _totalHours = _totalHours + hours;
                            _totalNightHours = _totalNightHours + nightHours;

                            #endregion
                        }
                        else
                        {
                            var dayNumber = Convert.ToInt32(((TextBlock) dayNumberTextBlock).Text);

                            var dayParam = Convert.ToInt32(((TextBlock) dayNumberTextBlock).Tag);

                            decimal tHours = Convert.ToDecimal(_currentScheduleDataRow["d" + dayNumber]);
                            decimal tNightHours = Convert.ToDecimal(_currentScheduleDataRow["n" + dayNumber]);

                            if (txt1 != null) txt1.Content = tHours;
                            if (txt2 != null) txt2.Content = _currentScheduleDataRow["s" + dayNumber];
                            if (txt3 != null) txt3.Content = tNightHours;

                            if ((dayParam != 2) && (dayParam != 3))
                                if ((tHours != 0) || (tNightHours != 0))
                                    _workingDaysCount++;

                            _totalHours = _totalHours + Convert.ToDecimal(_currentScheduleDataRow["d" + dayNumber]);
                            _totalNightHours = _totalNightHours +
                                               Convert.ToDecimal(_currentScheduleDataRow["n" + dayNumber]);
                        }

                        #endregion
                    }
                    else
                    {
                        if (txt1 != null) txt1.Content = "-";
                        if (txt2 != null) txt2.Content = "-";
                        if (txt3 != null) txt3.Content = "-";
                    }
                }
            }

            if (!reset)
            {
                WorkingDaysCountLabel.Content = _workingDaysCount;
                TotalHoursCountLabel.Content = _totalHours;
                TotalNighthoursLabel.Content = _totalNightHours;
            }
            else
            {
                WorkingDaysCountLabel.Content = "-";
                TotalHoursCountLabel.Content = "-";
                TotalNighthoursLabel.Content = "-";
            }
        }

        private void ApplyDateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersNameListBox.SelectedItems.Clear();
            EditDayButtonBorder.Visibility = Visibility.Hidden;

            int currentYear = Convert.ToInt32(YearComboBox.SelectedItem);
            int currentmonth = Convert.ToInt32(MonthComboBox.SelectedIndex + 1);

            var tempDate = new DateTime(currentYear, currentmonth, 1);

            _pSc.RefillPlannedSchedule(tempDate);

            PlannedScheduleCalendar.DisplayDate = tempDate;

            SavePlannedScheduleButton.Content = "--";
            SavePlannedScheduleButton.IsEnabled = false;
            MultisavePlannedScheduleButton.Visibility = Visibility.Hidden;

            FillPlannedScheduleCalendar(true);
        }

        private void SavePlannedScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItems.Count == 0) return;

            var currentDateTime = new DateTime(Convert.ToInt32(YearComboBox.SelectedItem),
                MonthComboBox.SelectedIndex + 1, 1);

            var workerID = Convert.ToInt32(WorkersNameListBox.SelectedValue);

            _pSc.AddNewPlannedScheduleRow(
                _currentScheduleDataRow == null
                    ? FillPlannedScheduleRow(_pSc.PlannedScheduleDataTable.NewRow())
                    : FillPlannedScheduleRow(_currentScheduleDataRow),
                workerID, currentDateTime, AdministrationClass.CurrentWorkerId, _totalHours, _totalNightHours,
                _workingDaysCount);
        }

        private void MultisavePlannedScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNameListBox.SelectedItems.Count == 0) return;

            var currentDateTime = new DateTime(Convert.ToInt32(YearComboBox.SelectedItem),
                MonthComboBox.SelectedIndex + 1, 1);

            foreach (var selectedItem in WorkersNameListBox.SelectedItems)
            {
                int workerId = Convert.ToInt32(((DataRowView) selectedItem)["WorkerID"]);

                _pSc.AddNewPlannedScheduleRow(
                    _currentScheduleDataRow == null
                        ? FillPlannedScheduleRow(_pSc.PlannedScheduleDataTable.NewRow())
                        : FillPlannedScheduleRow(_currentScheduleDataRow),
                    workerId, currentDateTime, AdministrationClass.CurrentWorkerId, _totalHours, _totalNightHours,
                    _workingDaysCount);
            }
        }

        private DataRow FillPlannedScheduleRow(DataRow plannedScheduleDataRow)
        {
            var calendarGrid = VisualTreeHelper.GetChild(PlannedScheduleCalendar, 0) as Grid;
            if (calendarGrid != null)
            {
                var ci = VisualTreeHelper.GetChild(calendarGrid, 0) as CalendarItem;
                if (ci == null) return plannedScheduleDataRow;

                var ciGrid = VisualTreeHelper.GetChild(ci, 0) as Grid;
                if (ciGrid == null) return plannedScheduleDataRow;

                var ciBorder = VisualTreeHelper.GetChild(ciGrid, 0) as Border;
                if (ciBorder == null) return plannedScheduleDataRow;

                var ci2Grid = VisualTreeHelper.GetChild(ciBorder, 0) as Grid;
                if (ci2Grid == null) return plannedScheduleDataRow;

                var ci3Grid = VisualTreeHelper.GetChild(ci2Grid, 1) as Grid;
                if (ci3Grid == null) return plannedScheduleDataRow;

                var childrenCount = VisualTreeHelper.GetChildrenCount(ci3Grid);

                for (var i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(ci3Grid, i);

                    if ((child == null) || (child.GetType().Name != "CalendarDayButton")) continue;

                    var tempCalendarDayButton = (CalendarDayButton) child;

                    var dayNumberTextBlock = tempCalendarDayButton.Template.FindName("DayNumberTextBlock",
                        (CalendarDayButton) child);

                    if ((dayNumberTextBlock == null) ||
                        (((TextBlock) dayNumberTextBlock).Text == string.Empty)) continue;

                    var txt1 = tempCalendarDayButton.Template.FindName("txt1", (CalendarDayButton) child) as Label;
                    var txt2 = tempCalendarDayButton.Template.FindName("txt2", (CalendarDayButton) child) as Label;
                    var txt3 = tempCalendarDayButton.Template.FindName("txt3", (CalendarDayButton) child) as Label;

                    var dayNumber = Convert.ToInt32(((TextBlock) dayNumberTextBlock).Text);

                    if (txt1 != null)
                        plannedScheduleDataRow["d" + dayNumber] = txt1.Content.ToString().Replace(".", ",");
                    if (txt2 != null)
                        plannedScheduleDataRow["s" + dayNumber] = txt2.Content.ToString().Replace(".", ",");
                    if (txt3 != null)
                        plannedScheduleDataRow["n" + dayNumber] = txt3.Content.ToString().Replace(".", ",");
                }
            }

            return plannedScheduleDataRow;
        }

        private void OkCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            var tHours = Convert.ToDecimal(AllTextBox.Value.ToString().Replace(".", ","));
            var tNightHours =
                Convert.ToDecimal(NightHoursTextBox.Value.ToString().Replace(".", ","));

            if ((tHours == 0) && (tNightHours == 0))
                if ((tHours != _currentHours) || (tNightHours != _currentNightHours))
                    _workingDaysCount--;

            _totalHours = _totalHours - _currentHours + tHours;
            _totalNightHours = _totalNightHours - _currentNightHours + tNightHours;

            _currentHours = tHours;
            _currentNightHours = tNightHours;

            _currentAllLabel.Content = _currentHours;
            _currentShiftLabel.Content = ShiftTextBox.Value;
            _currentNightHoursLabel.Content = _currentNightHours;

            WorkingDaysCountLabel.Content = _workingDaysCount;
            TotalHoursCountLabel.Content = _totalHours;
            TotalNighthoursLabel.Content = _totalNightHours;

            EditDayButtonBorder.Visibility = Visibility.Hidden;
        }

        private void Page_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseShowAppBar();
        }

        private void CloseShowAppBar(bool close = false)
        {
            DoubleAnimation da;
            if (close)
            {
                da = new DoubleAnimation(AppBarGrid.Height, 0,
                    new Duration(new TimeSpan(0, 0, 0, 0, 200)));
            }
            else
                da = AppBarGrid.Height == 0
                    ? new DoubleAnimation(AppBarGrid.Height, 70,
                        new Duration(new TimeSpan(0, 0, 0, 0, 200)))
                    : new DoubleAnimation(AppBarGrid.Height, 0,
                        new Duration(new TimeSpan(0, 0, 0, 0, 200)));

            AppBarGrid.BeginAnimation(HeightProperty, da);
        }

        private void ActualTimeSheetToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ActualTimeSheetToggleButton.IsEnabled = false;

            PlannedScheduleTogleButton.IsEnabled = true;
            PlannedScheduleTogleButton.IsChecked = false;

            TimesheetTabControl.SelectedIndex = 1;

            if (!_firstRun)
                CloseShowAppBar(true);
        }

        private void PlannedScheduleTogleButton_Checked(object sender, RoutedEventArgs e)
        {
            PlannedScheduleTogleButton.IsEnabled = false;

            ActualTimeSheetToggleButton.IsChecked = false;
            ActualTimeSheetToggleButton.IsEnabled = true;

            TimesheetTabControl.SelectedIndex = 0;

            if (!_firstRun)
                CloseShowAppBar(true);
        }

        #region TimeSheet

        private void TimeSheetWorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeSheetFilterWorkers();
        }

        private void TimeSheetFactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeSheetFilterWorkers();
        }

        private void TimeSheetFilterWorkers()
        {
            _workersDataView =
                _sc.FilterWorkers(true, Convert.ToInt32(TimeSheetWorkersGroupsComboBox.SelectedValue), true,
                    Convert.ToInt32(TimeSheetFactoriesComboBox.SelectedValue), false, 0).DefaultView;

            _workersDataView.RowFilter = "AvailableInList = 'True'";
        }

        private void ApplyFilterTimeSheetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_workersDataView.Count == 0) return;

            _timeSheetBackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            ProgressTimeSheetProgressBar.Visibility = Visibility.Visible;
            ProgressTimeSheetLabel.Visibility = Visibility.Visible;
            DispatcherHelper.DoEvents();

            ApplyFilterTimeSheetButton.IsEnabled = false;

            CloseShowAppBar(true);
            MouseRightButtonDown -= Page_MouseRightButtonDown;

            _selectedTimesheetMonth = TimeSheetMonthComboBox.SelectedIndex + 1;
            _selectedTimesheetYear = Convert.ToInt32(TimeSheetYearComboBox.SelectedItem);

            _timeSheetBackgroundWorker.DoWork += (obj, ea) => Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                int factoryId = Convert.ToInt32(TimeSheetFactoriesComboBox.SelectedValue);

                _tsc.FillTablesForTimesheet(Convert.ToInt32(TimeSheetYearComboBox.SelectedItem),
                    TimeSheetMonthComboBox.SelectedIndex + 1,
                    _workersDataView, _standartTime, factoryId);

                FormatTimesheetDataGrid(Convert.ToInt32(TimeSheetYearComboBox.SelectedItem),
                    TimeSheetMonthComboBox.SelectedIndex + 1);

                _tsc.CalculateTimeSheet(Convert.ToInt32(TimeSheetYearComboBox.SelectedItem),
                    TimeSheetMonthComboBox.SelectedIndex + 1, factoryId);
            }));

            _tsc.ProgressChanged += (s, pe) => Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressTimeSheetProgressBar.Value = pe.ProgressPercentage;
                ProgressTimeSheetLabel.Content = pe.ProgressPercentage.ToString(CultureInfo.InvariantCulture) + " %";
                DispatcherHelper.DoEvents();
            }));

            _tsc.RunWorkerCompleted += (s, pe) =>
            {
                ProgressTimeSheetLabel.Visibility = Visibility.Hidden;
                ProgressTimeSheetProgressBar.Visibility = Visibility.Hidden;
                ProgressTimeSheetLabel.Content = "Подсчет...";
                ProgressTimeSheetProgressBar.Value = 0;

                ApplyFilterTimeSheetButton.IsEnabled = true;

                this.MouseRightButtonDown += Page_MouseRightButtonDown;
                _timeSheetBackgroundWorker.Dispose();
            };

            _timeSheetBackgroundWorker.RunWorkerAsync();
        }

        private void FormatTimesheetDataGrid(int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);

            foreach (DataGridColumn dgc in TimeSheetDataGrid.Columns)
            {
                dgc.Visibility = dgc.DisplayIndex + 1 > daysInMonth ? Visibility.Hidden : Visibility.Visible;
            }

            foreach (DataGridColumn dgc in DeviationTimeSheetDataGrid.Columns)
            {
                dgc.Visibility = dgc.DisplayIndex + 1 > daysInMonth ? Visibility.Hidden : Visibility.Visible;
            }

            foreach (DataGridColumn dgc in DeviationPlannedScheduleDataGrid.Columns)
            {
                dgc.Visibility = dgc.DisplayIndex + 1 > daysInMonth ? Visibility.Hidden : Visibility.Visible;
            }

            TempTimeSheetYearComboBox.SelectedItem = TimeSheetYearComboBox.SelectedItem;
            TempTimeSheetMonthComboBox.SelectedIndex = TimeSheetMonthComboBox.SelectedIndex;
        }

        #endregion

        private void GetPlannedHours()
        {
            if (TimeSheetMonthComboBox.SelectedIndex < 0) return;

            var fromDate = new DateTime(Convert.ToInt32(TimeSheetYearComboBox.SelectedItem),
                TimeSheetMonthComboBox.SelectedIndex + 1, 1);

            var prodCalendaDataRows =
                _pcc.ProdCalendarDataTable.AsEnumerable().Where(t => t.Field<DateTime>("Date") == fromDate).AsDataView();

            if (prodCalendaDataRows.Count == 0)
            {
                PlannedTimeLabel.Content = "--";
                _standartTime = 0;
            }
            else
            {
                _standartTime = Convert.ToInt32(prodCalendaDataRows[0]["Standart40Time"]);
                PlannedTimeLabel.Content = _standartTime;
            }
        }

        //private void TimeSheetBWCancelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _timeSheetBackgroundWorker.CancelAsync();
        //}

        #region TimesheetScrolls

        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Equals(sender, _workerProfessionsDataGridScrollViewer))
            {
                _timeSheetDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                _timesheetSummStatDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;

                _timeSheetDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                _timesheetSummStatDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                _timeSheetDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                _timesheetSummStatDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
            }
            else if (Equals(sender, _timeSheetDataGridScrollViewer))
            {

                double vo1 = e.VerticalOffset;
                double vo2 = _workerProfessionsDataGridScrollViewer.ScrollableHeight;

                if (vo1 <= vo2)
                {
                    _workerProfessionsDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                    _timesheetSummStatDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;

                    _workerProfessionsDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                    _timesheetSummStatDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                    _workerProfessionsDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                    _timesheetSummStatDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                }
                else
                {
                    _timeSheetDataGridScrollViewer.ScrollToVerticalOffset(vo2);
                    e.Handled = true;
                }
            }
            else if (Equals(sender, _timesheetSummStatDataGridScrollViewer))
            {
                _workerProfessionsDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                _timeSheetDataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;

                _workerProfessionsDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                _timeSheetDataGridScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

                _workerProfessionsDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                _timeSheetDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
            }
        }

        private ScrollViewer ScrollViewerFromFrameworkElement(DependencyObject frameworkElement)
        {
            if (VisualTreeHelper.GetChildrenCount(frameworkElement) == 0) return null;

            var child = VisualTreeHelper.GetChild(frameworkElement, 0) as FrameworkElement;

            if (child == null) return null;

            return child is ScrollViewer ? (ScrollViewer) child : ScrollViewerFromFrameworkElement(child);
        }

        private void GetDataGridScrolls()
        {
            _workerProfessionsDataGridScrollViewer = ScrollViewerFromFrameworkElement(WorkerProfessionsDataGrid);

            if (_workerProfessionsDataGridScrollViewer != null)
                _workerProfessionsDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;

            _timeSheetDataGridScrollViewer = ScrollViewerFromFrameworkElement(TimeSheetDataGrid);

            if (_timeSheetDataGridScrollViewer != null)
                _timeSheetDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;

            _timesheetSummStatDataGridScrollViewer = ScrollViewerFromFrameworkElement(TimesheetStatDataGrid);

            if (_timesheetSummStatDataGridScrollViewer != null)
                _timesheetSummStatDataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
        }

        #endregion

        #region AbsencesWrapPanel

        private void AddButtonsOnAbsencesWrapPanel()
        {
            AbsencesWrapPanel.Children.Clear();
            foreach (DataRowView absenceDataRow in _tsc.AbsencesTypesDataTable.DefaultView)
            {
                var bColorRect = new ButtonColorRect
                {
                    Content = "(" + absenceDataRow["AbsencesSymbol"] + ") " + absenceDataRow["AbsencesName"],
                    RectangleColor = new BrushConverter().ConvertFrom(absenceDataRow["Color"].ToString()) as Brush,
                    Width = 200,
                    Height = 30,
                    Margin = new Thickness(2),
                    Tag = absenceDataRow["AbsencesTypeID"]
                };

                bColorRect.Click += AbsenceButton_Click;

                AbsencesWrapPanel.Children.Add(bColorRect);
            }
        }

        private void AbsenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSheetDataGrid.SelectedCells.Count == 0) return;

            var currentRowView = TimeSheetDataGrid.SelectedCells[0].Item as DataRowView;

            foreach (var currentCell in TimeSheetDataGrid.SelectedCells.Where(currentCell => currentRowView != null))
            {
                currentRowView["t" + (currentCell.Column.DisplayIndex + 1)] =
                    Convert.ToInt32(((ButtonColorRect) sender).Tag);
            }
        }

        #endregion

        private void SaveTimesheetButton_Click(object sender, RoutedEventArgs e)
        {
            _tsc.SaveTimesheet();
            //_tsc.SaveTimesheetStat();
        }

        #region AbsencesCatalog

        private void ApplyEffect()
        {
            var objBlur = new System.Windows.Media.Effects.BlurEffect {Radius = 7};
            TimesheetTabControl.Effect = objBlur;
            TimesheetTabControl.IsEnabled = false;
        }

        private void ClearEffect()
        {
            TimesheetTabControl.Effect = null;
            TimesheetTabControl.IsEnabled = true;
        }

        private void AbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            CloseShowAppBar();
            MouseRightButtonDown -= Page_MouseRightButtonDown;

            ApplyEffect();
            AbsencesMainGrid.Visibility = Visibility.Visible;
        }

        // Добавление неявки
        private void AddAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            AbsencesListBox.IsEnabled = false;
            AbsencesListBox.SelectedIndex = -1;
            SetAbsencesEnabled(true);
            ConsiderInResult.IsChecked = true;
            GeneralButtonsPanel.Visibility = Visibility.Hidden;
            AdditionalButtonsPanel.Visibility = Visibility.Visible;
            FocusRectangle.Visibility = Visibility.Visible;
        }

        private void OkAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AbsencesNameTextBox.Text) || string.IsNullOrEmpty(AbsencesSymbolTextBox.Text))
                return;

            var absName = AbsencesNameTextBox.Text;
            var absSymbol = AbsencesSymbolTextBox.Text;
            var absDescription = AbsencesDescriptionTextBox.Text;
            var setInWeek = SetInWeekendCheckBox.IsChecked == true;
            var setInHol = SetInHolidayCheckBox.IsChecked == true;
            var considInRes = ConsiderInResult.IsChecked == true;
            var considNorm = ConsiderNormCheckBox.IsChecked == true;

            var absColor = BackgroundColorPicker.SelectedValue;

            if (
                !_tsc.AddAbsences(absName, absSymbol, absDescription, setInWeek, setInHol, considInRes, absColor,
                    considNorm))
                MessageBox.Show("Неявка под таким символом уже существует! \nНеявка не добавлена.", string.Empty,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                AbsencesListBox.SelectedValue = Convert.ToInt32(_tsc.LastItemFromAbsences()["AbsencesTypeID"]);
                AbsencesListBox.ScrollIntoView(AbsencesListBox.SelectedItem);

                AbsencesListBox.IsEnabled = true;
                GeneralButtonsPanel.Visibility = Visibility.Visible;
                AdditionalButtonsPanel.Visibility = Visibility.Hidden;
                FocusRectangle.Visibility = Visibility.Hidden;
            }
        }

        private void CloseAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            MouseRightButtonDown += Page_MouseRightButtonDown;

            ClearEffect();
            AbsencesMainGrid.Visibility = Visibility.Hidden;
        }

        // Выбор строк в ListBox-е
        private void AbsencesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверка на выбор строки
            if (((ListBox) sender).SelectedItem == null)
            {
                ClearAbsencesInfo();
                SetAbsencesEnabled(false);
                return;
            }

            var rowView = (DataRowView) ((ListBox) sender).SelectedItem;

            // Проверка на запечатанность
            if (rowView.Row["Locked"] == DBNull.Value || !Convert.ToBoolean(rowView.Row["Locked"]))
                SetAbsencesEnabled(true);
            else
                SetAbsencesEnabled(false);

            //Заполнение полей
            AbsencesNameTextBox.Text = rowView.Row["AbsencesName"].ToString();
            AbsencesSymbolTextBox.Text = rowView.Row["AbsencesSymbol"].ToString();
            AbsencesDescriptionTextBox.Text = rowView.Row["AbsencesDescription"].ToString();

            SetInWeekendCheckBox.IsChecked = (bool) rowView.Row["SetInWeekend"];
            SetInHolidayCheckBox.IsChecked = (bool) rowView.Row["SetInHoliday"];
            ConsiderInResult.IsChecked = (bool) rowView.Row["ConsiderInResult"];
            ConsiderNormCheckBox.IsChecked = (bool) rowView.Row["ConsiderNorm"];

            if (rowView.Row["Color"] != DBNull.Value)
            {
                var converter = new ColorConverter().ConvertFrom(rowView.Row["Color"]);
                if (converter != null)
                {
                    BackgroundColorPicker.SelectionChanged -= BackgroundColorPicker_SelectionChanged;
                    BackgroundColorPicker.SelectedValue = (Color) converter;
                    BackgroundColorPicker.SelectionChanged += BackgroundColorPicker_SelectionChanged;
                    BackgroundColorPicker_SelectionChanged(BackgroundColorPicker, null);
                    return;
                }
            }
            BackgroundColorPicker.SelectedIndex = -1;
        }

        // Очистка полей от записей
        private void ClearAbsencesInfo()
        {
            AbsencesNameTextBox.Text = string.Empty;
            AbsencesSymbolTextBox.Text = string.Empty;
            AbsencesDescriptionTextBox.Text = string.Empty;
            SetInWeekendCheckBox.IsChecked = false;
            SetInHolidayCheckBox.IsChecked = false;
            ConsiderInResult.IsChecked = false;
            BackgroundColorPicker.SelectedIndex = -1;
        }

        // Выставление доступности панели и кнопок
        private void SetAbsencesEnabled(bool isEnabled)
        {
            AbsencesNameTextBox.IsHitTestVisible = isEnabled;
            AbsencesSymbolTextBox.IsHitTestVisible = isEnabled;
            AbsencesDescriptionTextBox.IsHitTestVisible = isEnabled;
            SetInWeekendCheckBox.IsEnabled = isEnabled;
            SetInHolidayCheckBox.IsEnabled = isEnabled;
            ConsiderInResult.IsEnabled = isEnabled;
            BackgroundColorPicker.IsEnabled = isEnabled;
            DeleteAbsencesButton.IsEnabled = isEnabled;
            SaveAbsencesButton.IsEnabled = isEnabled;
            ConsiderNormCheckBox.IsEnabled = isEnabled;
        }

        private void DeleteAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (AbsencesListBox.SelectedItem == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную неявку?", string.Empty,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                _tsc.DeleteAbsences(Convert.ToInt32(AbsencesListBox.SelectedValue));

            AddButtonsOnAbsencesWrapPanel();
        }

        private void CancelAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            AbsencesListBox.IsEnabled = true;
            if (AbsencesListBox.Items.Count != 0)
                AbsencesListBox.SelectedIndex = 0;
            GeneralButtonsPanel.Visibility = Visibility.Visible;
            AdditionalButtonsPanel.Visibility = Visibility.Hidden;
            FocusRectangle.Visibility = Visibility.Hidden;
        }

        private void SaveAbsencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (AbsencesListBox.SelectedItem == null) return;

            var absId = Convert.ToInt32(AbsencesListBox.SelectedValue);
            var absName = AbsencesNameTextBox.Text;
            var absSymbol = AbsencesSymbolTextBox.Text;
            var absDescription = AbsencesDescriptionTextBox.Text;
            var setInWeek = SetInWeekendCheckBox.IsChecked == true;
            var setInHol = SetInHolidayCheckBox.IsChecked == true;
            var considInRes = ConsiderInResult.IsChecked == true;
            var considNorm = ConsiderNormCheckBox.IsChecked == true;
            var absColor = BackgroundColorPicker.SelectedValue ?? DBNull.Value;

            if (
                !_tsc.SaveAbsences(absId, absName, absSymbol, absDescription, setInWeek, setInHol, considInRes, absColor,
                    considNorm))
                MessageBox.Show("Неявка под таким символом уже существует! \nИзменения не сохранены.", string.Empty,
                    MessageBoxButton.OK, MessageBoxImage.Warning);

            AddButtonsOnAbsencesWrapPanel();
        }

        private void BackgroundColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var convertFrom = new ColorConverter().ConvertFrom(((ColorPicker) sender).SelectedValue.ToString());
            // Проверка, смог ли конвертер создать цвет
            if (convertFrom == null)
            {
                AbsencesSymbolTextBox.Background = Brushes.Transparent;
                return;
            }
            var color = (Color) convertFrom;
            AbsencesSymbolTextBox.Background = new SolidColorBrush(color);
        }

        private void AbsencesSymbolTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Запрет ввода более 5-ых символов
            if (((TextBox) sender).Text.Length > 5)
                e.Handled = true;
        }

        #endregion

        private void ConsiderInResult_Unchecked(object sender, RoutedEventArgs e)
        {
            ConsiderNormCheckBox.IsChecked = false;
        }

        private void CalculateTimesheetStatButton_Click(object sender, RoutedEventArgs e)
        {
            _tsc.UpdateTimesheetStat();
        }

        private void TempTimeSheetMonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetPlannedHours();
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel.GenerateTimesheetReport(ref TimeSheetDataGrid, _selectedTimesheetYear,
                _selectedTimesheetMonth, ref ExportToExcelButton, ref ApplyFilterTimeSheetButton,
                ref CalculateTimesheetStatButton, ref AbsencesWrapPanel);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_tsc != null)
                _tsc.TimesheetDataTable.Clear();
        }

        #region DeviationGrid

        private void DeviationButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseShowAppBar(true);

            DeviationTimeSheetDataGrid.Items.Clear();

            var currentTimesheetDrv =
                (DataRowView) TimeSheetDataGrid.Items.GetItemAt(TimesheetStatDataGrid.SelectedIndex);

            var workerId = Convert.ToInt32(currentTimesheetDrv["WorkerID"]);
            var timesheetDate = Convert.ToDateTime(currentTimesheetDrv["TimesheetDate"]);

            DeviationTimeSheetDataGrid.Items.Add(currentTimesheetDrv);

            DeviationNameLabel.Content = _idToNameConv.Convert(workerId, "FullName");

            DeviationDateLabel.Content = _dateConv.Convert(timesheetDate, "MMMM yyyy");

            _pSc.tempPlannedScheduleDataTable.Rows.Clear();
            DeviationPlannedScheduleDataGrid.ItemsSource = _pSc.tempPlannedScheduleDataTable.DefaultView;
            var dr = _pSc.tempPlannedScheduleDataTable.NewRow();

            DataRow dr2 = _pSc.GetPlannedScheduleRow(workerId, timesheetDate);

            if (dr2 != null)
            {
                dr.ItemArray = dr2.ItemArray;
                _pSc.tempPlannedScheduleDataTable.Rows.Add(dr);
                DeviationPlannedScheduleDataGrid.Items.Refresh();
            }

            if (DeviationTimeSheetDataGrid.Items.Count != 0 && DeviationPlannedScheduleDataGrid.Items.Count != 0)
            {
                App.BaseClass.DiffBetweenTwoDeviationRowsCollection = GetDifferenceTwoRows(DeviationTimeSheetDataGrid, DeviationPlannedScheduleDataGrid);
              
            }
            else
            {
                App.BaseClass.DiffBetweenTwoDeviationRowsCollection.Clear();
            }


            ApplyEffect();
            DeviationGrid.Visibility = Visibility.Visible;

            MouseRightButtonDown -= Page_MouseRightButtonDown;
        }

        private void DeviationCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearEffect();
            DeviationGrid.Visibility = Visibility.Hidden;

            MouseRightButtonDown += Page_MouseRightButtonDown;
        }

        public static void SelectCellByIndex(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.Cell))
                throw new ArgumentException("The SelectionUnit of the DataGrid must be set to Cell.");

            if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
                throw new ArgumentException(string.Format("{0} is an invalid row index.", rowIndex));

            if (columnIndex < 0 || columnIndex > (dataGrid.Columns.Count - 1))
                throw new ArgumentException(string.Format("{0} is an invalid column index.", columnIndex));

            dataGrid.SelectedCells.Clear();

            object item = dataGrid.Items[rowIndex]; //=Product X
            var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row == null)
            {
                dataGrid.ScrollIntoView(item);
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }
            if (row != null)
            {
                DataGridCell cell = GetCell(dataGrid, row, columnIndex);
                if (cell != null)
                {
                    var dataGridCellInfo = new DataGridCellInfo(cell);
                    dataGrid.SelectedCells.Add(dataGridCellInfo);
                    cell.Focus();
                }
            }
        }

        public static DataGridCell GetCell(DataGrid dataGrid, DataGridRow rowContainer, int column)
        {
            if (rowContainer != null)
            {
                var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    /* if the row has been virtualized away, call its ApplyTemplate() method 
                     * to build its visual tree in order for the DataGridCellsPresenter
                     * and the DataGridCells to be created */
                    rowContainer.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                if (presenter != null)
                {
                    var cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell == null)
                    {
                        /* bring the column into view
                         * in case it has been virtualized away */
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }
            return null;
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T) child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void DeviationTimeSheetDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (DeviationTimeSheetDataGrid.SelectedCells.Count == 0) return;
            if (DeviationPlannedScheduleDataGrid.Items.Count == 0) return;

            DeviationPlannedScheduleDataGrid.SelectedCellsChanged -=
                DeviationPlannedScheduleDataGrid_SelectedCellsChanged;

            int columnIndex = DeviationTimeSheetDataGrid.SelectedCells[0].Column.DisplayIndex;
            SelectCellByIndex(DeviationPlannedScheduleDataGrid, 0, columnIndex);

            DeviationPlannedScheduleDataGrid.SelectedCellsChanged +=
                DeviationPlannedScheduleDataGrid_SelectedCellsChanged;
        }

        private void DeviationPlannedScheduleDataGrid_SelectedCellsChanged(object sender,
            SelectedCellsChangedEventArgs e)
        {
            if (DeviationPlannedScheduleDataGrid.SelectedCells.Count == 0) return;
            if (DeviationTimeSheetDataGrid.Items.Count == 0) return;

            DeviationTimeSheetDataGrid.SelectedCellsChanged -= DeviationTimeSheetDataGrid_SelectedCellsChanged;

            int columnIndex = DeviationPlannedScheduleDataGrid.SelectedCells[0].Column.DisplayIndex;
            SelectCellByIndex(DeviationTimeSheetDataGrid, 0, columnIndex);

            DeviationTimeSheetDataGrid.SelectedCellsChanged += DeviationTimeSheetDataGrid_SelectedCellsChanged;
        }

        private ObservableCollection<int> GetDifferenceTwoRows(DataGrid timesheetDataGrid, DataGrid plannedScheduleDataGrid)
        {
            var diffList = new ObservableCollection<int>();

            foreach (var column in timesheetDataGrid.Columns)
            {
                string s1 = ((DataRowView) timesheetDataGrid.Items[0]).Row["d" + column.Header].ToString();
                string s2 = ((DataRowView) plannedScheduleDataGrid.Items[0]).Row["d" + column.Header].ToString();

                if (s1 == "" || s1 == "-")
                    s1 = 0.ToString(CultureInfo.InvariantCulture);

                decimal d1;
                decimal d2;

                if (Decimal.TryParse(s1, out d1) && Decimal.TryParse(s2, out d2))
                {
                    if (d1 != d2) diffList.Add(Convert.ToInt32(column.Header));
                }
                else
                {
                   diffList.Add(Convert.ToInt32(column.Header));
                }
            }

            return diffList;
        }

        #endregion


    }
}
