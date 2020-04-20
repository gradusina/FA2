using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FA2.XamlFiles;

namespace FA2.ChildPages.StimulationPage
{
    /// <summary>
    /// Логика взаимодействия для WorkerStimulation.xaml
    /// </summary>
    public partial class WorkerStimulation
    {
        private readonly StimulationClass _stc;
        private readonly DataRowView _workerView;
        private readonly int _currentWorkerId;
        private readonly int _selectedWorkerStimId;

        private readonly IdToNameConverter _idToNameConverter;

        public WorkerStimulation(DataRowView workerView, int curWorkerId, bool promotion = true)
        {
            InitializeComponent();
            App.BaseClass.GetStimulationClass(ref _stc);
            FillBindings();
            _workerView = workerView;
            _currentWorkerId = curWorkerId;
            SetWorkerStimSettings(promotion);
            OkWorkerStimButton.Visibility = Visibility.Visible;
            SaveWorkerStimButton.Visibility = Visibility.Hidden;
        }

        public WorkerStimulation(DataRowView workerView, DataRowView dataRowView, int curWorkerId,
                                int selectedStimId, bool promotion = true)
        {
            InitializeComponent();
            App.BaseClass.GetStimulationClass(ref _stc);
            _idToNameConverter = new IdToNameConverter();
            FillBindings();
            _workerView = workerView;
            _currentWorkerId = curWorkerId;
            _selectedWorkerStimId = selectedStimId;
            SetChangingWorkerStimSettings(dataRowView, promotion);
            OkWorkerStimButton.Visibility = Visibility.Hidden;
            SaveWorkerStimButton.Visibility = Visibility.Visible;
        }

        private void FillBindings()
        {
            WorkerStimComboBox.SelectionChanged -= WorkerStimComboBox_SelectionChanged;
            WorkerStimComboBox.ItemsSource = _stc.StimView;
            WorkerStimComboBox.SelectedValuePath = "StimulationID";
            WorkerStimComboBox.DisplayMemberPath = "StimulationName";
            WorkerStimComboBox.SelectionChanged += WorkerStimComboBox_SelectionChanged;

            WorkerStimUnitsComboBox.ItemsSource = _stc.StimUnitsDataTable.DefaultView;
            WorkerStimUnitsComboBox.SelectedValuePath = "StimulationUnitID";
            WorkerStimUnitsComboBox.DisplayMemberPath = "StimulationUnitName";
        }


        private void SetWorkerStimSettings(bool promotion = true)
        {
            WorkerStimDatePicker.SelectedDate = App.BaseClass.GetDateFromSqlServer();
            WorkerStimNameLabel.Content = _workerView.Row["Name"];
            WarningCheckBox.IsChecked = true;
            if (WorkerStimUnitsComboBox.Items.Count != 0)
                WorkerStimUnitsComboBox.SelectedIndex = 0;
            ClearWorkerStimGrid();

            if (promotion)
            {
                PromotionPolygon.Visibility = Visibility.Visible;
                _stc.StimView.CustomFilter = "StimulationTypeID = 1 AND Available = 'TRUE'";
            }
            else
            {
                FinePolygon.Visibility = Visibility.Visible;
                _stc.StimView.CustomFilter = "StimulationTypeID = 2 AND Available = 'TRUE'";
            }

            if (WorkerStimComboBox.Items.Count != 0)
                WorkerStimComboBox.SelectedIndex = 0;
            WorkerStimComboBox_SelectionChanged(WorkerStimComboBox, null);

            LastWorkerStimEditingLabel.Visibility = Visibility.Hidden;
            LastWorkerStimEditingLabel.Content = string.Empty;
        }

        private void CancelWorkerStimButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void ClearWorkerStimGrid()
        {
            WorkerStimNotesTextBox.Text = string.Empty;
            WorkerStimSizeControl.Value = decimal.Zero;
        }

        private void WorkerStimComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox) sender).SelectedValue == null)
            {
                WarningCountLabel.Content = 0;
            }
            else
            {
                var stimulationId = Convert.ToInt32(((ComboBox) sender).SelectedValue);
                WarningCountLabel.Content = WorkerStimDatePicker.SelectedDate.HasValue
                    ? CalculateStimulationsWarning(stimulationId, WorkerStimDatePicker.SelectedDate.Value)
                    : 0;
            }
        }

        private void WorkerStimDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!((DatePicker) sender).SelectedDate.HasValue || WorkerStimComboBox.SelectedValue == null)
            {
                WarningCountLabel.Content = 0;
            }
            else
            {
                var stimulationId = Convert.ToInt32(WorkerStimComboBox.SelectedValue);
                WarningCountLabel.Content = CalculateStimulationsWarning(stimulationId,
                    WorkerStimDatePicker.SelectedDate.Value);
            }
        }

        private int CalculateStimulationsWarning(int stimulationId, DateTime dateTo)
        {
            var warningCount = 0;
            var workerId = Convert.ToInt32(_workerView.Row["WorkerID"]);
            var sortingTable = _stc.WorkersStimDataTable.
                Select(string.Format("WorkerID = {0} AND StimulationID = {1} AND Date < '{2}' AND Available = 'TRUE'",
                    workerId, stimulationId, dateTo.AddHours(1)));
            if (sortingTable.Count() != 0)
            {
                var dv = sortingTable.CopyToDataTable().DefaultView;
                dv.Sort = "Date DESC, WorkerStimID DESC";
                foreach (DataRowView drv in dv)
                {
                    if (Convert.ToBoolean(drv.Row["ClosedWarning"]))
                        return warningCount;
                    warningCount++;
                }
            }
            return warningCount;
        }

        private void WorkerStimUnitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox) sender).SelectedValue == null)
            {
                WorkerStimMesuarmentLabel.Content = string.Empty;
                WorkerStimSizeControl.Value = decimal.Zero;
                return;
            }
            WorkerStimMesuarmentLabel.Content = Convert.ToInt32(((ComboBox) sender).SelectedValue) == 1
                ? "часов"
                : "тыс. рублей";
        }

        private void WarningCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WorkerStimSizePanel.IsEnabled = false;
            WorkerStimSizeControl.BorderBrush = Brushes.LightGray;
            WorkerStimSizeControl.Foreground = Brushes.LightGray;
        }

        private void WarningCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WorkerStimSizePanel.IsEnabled = true;
            WorkerStimSizeControl.BorderBrush = Brushes.Gray;
            WorkerStimSizeControl.Foreground = Brushes.Black;
        }

        private void OkWorkerStimButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerStimComboBox.SelectedItem == null || WorkerStimDatePicker.SelectedDate == null) return;
            if (WarningCheckBox.IsChecked == false && (WorkerStimUnitsComboBox.SelectedItem == null ||
                                                       Convert.ToDecimal(WorkerStimSizeControl.Value) == decimal.Zero ||
                                                       WorkerStimSizeControl.Value == null)) return;

            var workerId = Convert.ToInt32(_workerView.Row["WorkerID"]);
            var date = WorkerStimDatePicker.SelectedDate.Value;
            var stimulationId = Convert.ToInt32(WorkerStimComboBox.SelectedValue);
            var notes = WorkerStimNotesTextBox.Text;
            var editingDate = App.BaseClass.GetDateFromSqlServer();

            if (WarningCheckBox.IsChecked == true)
            {
                _stc.AddWorkerStimWarning(workerId, date, stimulationId, notes, editingDate, _currentWorkerId);
            }
            else
            {
                var stimulationUnitId = Convert.ToInt32(WorkerStimUnitsComboBox.SelectedValue);
                var stimulationSize = stimulationUnitId == 1
                    ? Convert.ToDouble(WorkerStimSizeControl.Value)
                    : Convert.ToDouble(WorkerStimSizeControl.Value)*1000;
                _stc.AddWorkerStimNotWarning(workerId, date, stimulationId, notes, stimulationUnitId, stimulationSize,
                    editingDate, _currentWorkerId);
            }

            NotificationManager.AddNotification(workerId, AdministrationClass.Modules.WorkersStimulation, -1);

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var stimPage = mainWindow.MainFrame.Content as XamlFiles.StimulationPage;
                if (stimPage != null)
                {
                    stimPage.UpdateDataGridSource(workerId);
                    stimPage.CalculateTotalStimulationSize();
                }
            }
            CancelWorkerStimButton_Click(null, null);
        }




        private void SetChangingWorkerStimSettings(DataRowView dataRowView, bool promotion = true)
        {
            WorkerStimDatePicker.SelectedDate = Convert.ToDateTime(dataRowView["Date"]);
            WorkerStimNameLabel.Content = _workerView.Row["Name"];
            if (dataRowView["Notes"] != DBNull.Value)
                WorkerStimNotesTextBox.Text = dataRowView["Notes"].ToString();
            if (Convert.ToBoolean(dataRowView["Warning"]))
            {
                WarningCheckBox.IsChecked = true;
                WorkerStimSizeControl.Value = decimal.Zero;
                if (WorkerStimUnitsComboBox.Items.Count != 0)
                    WorkerStimUnitsComboBox.SelectedIndex = 0;
            }
            else
            {
                WarningCheckBox.IsChecked = false;
                var stimulationId = Convert.ToInt32(dataRowView["StimulationUnitID"]);
                WorkerStimUnitsComboBox.SelectedValue = stimulationId;
                if (stimulationId == 1)
                    WorkerStimSizeControl.Value = Convert.ToDecimal(dataRowView["StimulationSize"]);
                else
                    WorkerStimSizeControl.Value = Convert.ToDecimal(dataRowView["StimulationSize"])/1000;
                WorkerStimUnitsComboBox_SelectionChanged(WorkerStimUnitsComboBox, null);
            }
            if (promotion)
            {
                PromotionPolygon.Visibility = Visibility.Visible;
                _stc.StimView.CustomFilter = "StimulationTypeID = 1 AND Available = 'TRUE'";
            }
            else
            {
                FinePolygon.Visibility = Visibility.Visible;
                _stc.StimView.CustomFilter = "StimulationTypeID = 2 AND Available = 'TRUE'";
            }
            WorkerStimComboBox.SelectedValue = dataRowView["StimulationID"];
            WorkerStimComboBox_SelectionChanged(WorkerStimComboBox, null);

            LastWorkerStimEditingLabel.Visibility = Visibility.Visible;
            LastWorkerStimEditingLabel.Content =
                string.Format("{0} {1}", _idToNameConverter.Convert(dataRowView["EditingWorkerID"], "ShortName"),
                    Convert.ToDateTime(dataRowView["EditingDate"]).ToString("dd.MM.yyyy HH:mm"));
        }

        private void SaveWorkerStimButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerStimComboBox.SelectedItem == null || WorkerStimDatePicker.SelectedDate == null) return;
            if (WarningCheckBox.IsChecked == false && (WorkerStimUnitsComboBox.SelectedItem == null ||
                                                       Convert.ToDecimal(WorkerStimSizeControl.Value) == decimal.Zero ||
                                                       WorkerStimSizeControl.Value == null)) return;

            var workerId = Convert.ToInt32(_workerView.Row["WorkerID"]);
            var date = WorkerStimDatePicker.SelectedDate.Value;
            var stimulationId = Convert.ToInt32(WorkerStimComboBox.SelectedValue);
            var notes = WorkerStimNotesTextBox.Text;
            var editingDate = App.BaseClass.GetDateFromSqlServer();

            if (WarningCheckBox.IsChecked == true)
            {
                _stc.ChangeWorkerStimWarning(_selectedWorkerStimId, date, stimulationId, notes, editingDate,
                    _currentWorkerId);
            }
            else
            {
                var stimulationUnitId = Convert.ToInt32(WorkerStimUnitsComboBox.SelectedValue);
                var stimulationSize = stimulationUnitId == 1
                    ? Convert.ToDouble(WorkerStimSizeControl.Value)
                    : Convert.ToDouble(WorkerStimSizeControl.Value)*1000;
                _stc.ChangeWorkerStimNotWarning(_selectedWorkerStimId, date, stimulationId, notes, stimulationUnitId,
                    stimulationSize, editingDate, _currentWorkerId);
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var stimPage = mainWindow.MainFrame.Content as XamlFiles.StimulationPage;
                if (stimPage != null)
                {
                    stimPage.UpdateDataGridSource(workerId);
                    stimPage.CalculateTotalStimulationSize();
                }
            }

            CancelWorkerStimButton_Click(null, null);
        }
    }
}
