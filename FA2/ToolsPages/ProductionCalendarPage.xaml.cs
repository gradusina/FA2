using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ToolsPages
{
    /// <summary>
    /// Логика взаимодействия для ProductionCalendarPage.xaml
    /// </summary>
    public partial class ProductionCalendarPage : Page
    {
        private DateTime _currentDate;

        private ProductionCalendarClass _pcc;

        private int _halfYearNumber;
        private int _quarterYearNumber;

        public ProductionCalendarPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (o, args) =>
                GetClasses();

            backgroundWorker.RunWorkerCompleted += (o, args) =>
            {
                SetBindings();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            };

            backgroundWorker.RunWorkerAsync();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            mainWindow.HideToolsGrid();
        }

        private void ViewYearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewYearComboBox.SelectedItem != null)
            {
                _pcc.CreateViewProdCalendarDataTable(Convert.ToInt32(ViewYearComboBox.SelectedItem));

                string yearFilter = String.Format(CultureInfo.InvariantCulture.DateTimeFormat,
                    "Date >= #{0}# AND Date <=#{1}#", new DateTime(Convert.ToInt32(ViewYearComboBox.SelectedItem), 1, 1),
                    new DateTime(Convert.ToInt32(ViewYearComboBox.SelectedItem), 12, 30));

                ((DataView) HolidaysListBox.ItemsSource).RowFilter = yearFilter;
            }
        }


        private void GetClasses()
        {
            App.BaseClass.GetProductionCalendarClass(ref _pcc);
        }

        private void SetBindings()
        {
            _currentDate = App.BaseClass.GetDateFromSqlServer();

            BindingDate();

            Binding();

            ViewYearComboBox_SelectionChanged(null, null);
        }

        private void BindingDate()
        {
            ViewYearComboBox.SelectionChanged -= ViewYearComboBox_SelectionChanged;

            for (int i = 2013; i < _currentDate.Year + 2; i++)
                ViewYearComboBox.Items.Add(i);

            ViewYearComboBox.SelectedItem = _currentDate.Year;
            ViewYearComboBox.SelectionChanged += ViewYearComboBox_SelectionChanged;

            YearComboBox.SelectionChanged -= YearComboBox_SelectionChanged;

            for (int i = 2013; i < _currentDate.Year + 2; i++)
                YearComboBox.Items.Add(i);

            YearComboBox.SelectedItem = _currentDate.Year;
            YearComboBox.SelectionChanged += YearComboBox_SelectionChanged;


            MonthComboBox.SelectionChanged -= MonthComboBox_SelectionChanged;

            var ci = new CultureInfo("ru-RU");
            DateTimeFormatInfo dtformatInfo = ci.DateTimeFormat;
            for (int i = 1; i <= 12; i++)
            {
                MonthComboBox.Items.Add(dtformatInfo.GetMonthName(i));
            }

            MonthComboBox.SelectedIndex = _currentDate.Month - 1;
            MonthComboBox.SelectionChanged += MonthComboBox_SelectionChanged;
            MonthComboBox_SelectionChanged(null, null);
        }

        private void Binding()
        {
            ProductionCalendarDataGrid.ItemsSource = _pcc.GetViewProdCalendar();
            HolidaysListBox.ItemsSource = _pcc.GetHolidays();

            EditProductionCalendarDataGrid.ItemsSource = _pcc.GetProdCalendar();
            HolidaysDataGrid.ItemsSource = _pcc.GetHolidays();


            if (ViewYearComboBox.SelectedItem != null)
                _pcc.CreateViewProdCalendarDataTable(Convert.ToInt32(ViewYearComboBox.SelectedItem));

            YearComboBox_SelectionChanged(null, null);
        }

        private void ProductionCalendarDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DateTime dt;
            if (DateTime.TryParse(((DataRowView) e.Row.Item)["Date"].ToString(), out dt)) return;

            e.Row.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom("#CFD8DC"));
            e.Row.HorizontalContentAlignment = HorizontalAlignment.Right;
        }

        private void ProductionCalendarDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent
            };

            MainScrollViewer.RaiseEvent(eventArg);
        }

        private void HolidaysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HolidaysListBox.SelectedItems.Clear();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
        }



        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------

        private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string yearFilter = String.Format(CultureInfo.InvariantCulture.DateTimeFormat,
                "Date >= #{0}# AND Date <=#{1}#", new DateTime(Convert.ToInt32(YearComboBox.SelectedItem), 1, 1),
                new DateTime(Convert.ToInt32(YearComboBox.SelectedItem), 12, 30));

            ((DataView) EditProductionCalendarDataGrid.ItemsSource).RowFilter = yearFilter;
            ((DataView) HolidaysDataGrid.ItemsSource).RowFilter = yearFilter;
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonthComboBox.Items.Count == 0) return;

            var monthNumber = MonthComboBox.SelectedIndex + 1;


            _halfYearNumber = GetHalfYearNumber(monthNumber);
            _quarterYearNumber = GetQuarterYearNumber(monthNumber);

            HalfYearLabel.Content = _halfYearNumber;
            QuarterLabel.Content = _quarterYearNumber;

            if (YearComboBox.Items.Count != 0)
                CalendarDaysCountUpDown.Value = DateTime.DaysInMonth(Convert.ToInt32(YearComboBox.SelectedItem),
                    monthNumber);

            if ((YearComboBox.Items.Count != 0) && (MonthComboBox.Items.Count != 0))
                WeekendCountUpDown.Value = GetWeekendCount(Convert.ToInt32(YearComboBox.SelectedItem),
                    Convert.ToInt32(MonthComboBox.SelectedIndex + 1));

            NormalWorkingDaysCountUpDown.Value = Convert.ToDecimal(CalendarDaysCountUpDown.Value) -
                                                 (Convert.ToDecimal(WeekendCountUpDown.Value) +
                                                  Convert.ToDecimal(HolidaysCountUpDown.Value) +
                                                  Convert.ToDecimal(PreholidaysCountUpDown.Value));

            Standart40TimeUpDown.Value = GetStandart40Time(Convert.ToInt32(NormalWorkingDaysCountUpDown.Value),
                Convert.ToInt32(PreholidaysCountUpDown.Value));
            Standart35TimeUpDown.Value = GetStandart35Time(Convert.ToInt32(NormalWorkingDaysCountUpDown.Value),
                Convert.ToInt32(PreholidaysCountUpDown.Value));
        }

        private int GetStandart40Time(int normalWorkingDaysCount, int preholidaysCount)
        {
            int result = (normalWorkingDaysCount * 8) + (preholidaysCount * 7);
            return result;
        }

        private int GetStandart35Time(int normalWorkingDaysCount, int preholidaysCount)
        {
            int result = (normalWorkingDaysCount * 7) + (preholidaysCount * 6);
            return result;
        }

        private int GetHalfYearNumber(int monthNumber)
        {
            int result = monthNumber <= 6 ? 1 : 2;
            return result;
        }

        private int GetQuarterYearNumber(int monthNumber)
        {
            int result = ((monthNumber - 1) / 3) + 1;
            return result;
        }

        private int GetWeekendCount(int year, int monthNumber)
        {
            int result = 0;

            var month = new DateTime(year, monthNumber, 1);

            const DayOfWeek dw1 = DayOfWeek.Saturday;
            const DayOfWeek dw2 = DayOfWeek.Sunday;

            while (true)
                if (month.Month == monthNumber)
                {
                    if (month.DayOfWeek == dw1 || month.DayOfWeek == dw2)
                        result++;
                    month = month.AddDays(1);
                }
                else
                {
                    return result;
                }
        }

        private void AddMonthDaysButton_Click(object sender, RoutedEventArgs e)
        {
            _pcc.AddMonthDaysInfo(Convert.ToInt32(YearComboBox.SelectedItem),
                                    Convert.ToInt32((MonthComboBox.SelectedIndex) + 1),
                                    _halfYearNumber,
                                    _quarterYearNumber,
                                    Convert.ToInt32(CalendarDaysCountUpDown.Value),
                                    Convert.ToInt32(NormalWorkingDaysCountUpDown.Value),
                                    Convert.ToInt32(PreholidaysCountUpDown.Value),
                                    Convert.ToInt32(WeekendCountUpDown.Value),
                                    Convert.ToInt32(HolidaysCountUpDown.Value),
                                    Convert.ToInt32(Standart40TimeUpDown.Value),
                                    Convert.ToInt32(Standart35TimeUpDown.Value));

            if (ViewYearComboBox.SelectedItem != null)
                _pcc.CreateViewProdCalendarDataTable(Convert.ToInt32(ViewYearComboBox.SelectedItem));

            YearComboBox_SelectionChanged(null, null);
        }

        private void ProductionCalendarDataGridRow_Delete(object sender, RoutedEventArgs e)
        {
            if (EditProductionCalendarDataGrid.Items.Count == 0 || EditProductionCalendarDataGrid.SelectedItem == null) return;

            _pcc.DeleteMonthDaysInfo(((DataRowView) EditProductionCalendarDataGrid.SelectedItem).Row);

            if (ViewYearComboBox.SelectedItem != null)
            {
                _pcc.CreateViewProdCalendarDataTable(Convert.ToInt32(ViewYearComboBox.SelectedItem));

                YearComboBox_SelectionChanged(null, null);
            }
        }

        private void HolidaysDataGridRow_Delete(object sender, RoutedEventArgs e)
        {
            if (HolidaysDataGrid.Items.Count == 0 || HolidaysDataGrid.SelectedItem == null) return;

            _pcc.DeleteHoliday(((DataRowView) HolidaysDataGrid.SelectedItem).Row);
        }

        private void AddHolidayDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (HolidayDatePicker.SelectedDate == null) return;

            _pcc.AddHoliday(HolidayDatePicker.SelectedDate.Value, HolidayNamePopupTextBox.Text);
            HolidayNamePopupTextBox.Text = string.Empty;
        }

        private void EditProductionCalendarDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent
            };

            EditMainScrollViewer.RaiseEvent(eventArg);
        }


    }
}
