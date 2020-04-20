using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FA2.Classes;
using FA2.Converters;
using FA2.XamlFiles;

namespace FA2.ChildPages.TaskPage
{
    /// <summary>
    /// Логика взаимодействия для TaskTimeTrackingInfo.xaml
    /// </summary>
    public partial class TaskTimeTrackingInfo
    {
        private readonly TaskClass _taskClass;
        private readonly long _performerId;
        private BindingListCollectionView _taskTimeTrackingView;

        public TaskTimeTrackingInfo(long performerId, bool fullAccess)
        {
            InitializeComponent();

            _performerId = performerId;
            App.BaseClass.GetTaskClass(ref _taskClass);

            FillData();

            DataContext = fullAccess;
        }

        private void FillData()
        {
            var taskTimeTrackingView = _taskClass.TaskTimeTracking.Table.Copy().AsDataView();
            _taskTimeTrackingView = new BindingListCollectionView(taskTimeTrackingView);
            if (_taskTimeTrackingView.GroupDescriptions != null)
                _taskTimeTrackingView.GroupDescriptions.Add(new PropertyGroupDescription("Date"));
            _taskTimeTrackingView.CustomFilter = string.Format("PerformerID = {0}", _performerId);

            TimeTrackingItemsControl.ItemsSource = _taskTimeTrackingView;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't save if information is empty
            if (_taskTimeTrackingView.Count == 0)
            {
                CancelButton_Click(null, null);
                return;
            }

            // Save confirm and edited information
            var totalTime = new TimeSpan();
            foreach (var dataRow in _taskTimeTrackingView.Cast<DataRowView>())
            {
                var taskTimeTrackingId = Convert.ToInt32(dataRow["TaskTimeTrackingID"]);
                var timeStart = (TimeSpan) dataRow["TimeStart"];
                var timeEnd = (TimeSpan) dataRow["TimeEnd"];
                var interval = TimeIntervalCountConverter.CalculateTimeInterval(timeStart, timeEnd);
                totalTime = totalTime.Add(interval);

                _taskClass.TaskTimeTracking.SetTimeInterval(taskTimeTrackingId, timeStart, timeEnd);

                if (Convert.ToBoolean(dataRow["IsVerificated"]))
                {
                    var verificationDate = Convert.ToDateTime(dataRow["VerificationDate"]);
                    var verificationWorkerId = Convert.ToInt32(dataRow["VerificationWorkerID"]);

                    _taskClass.TaskTimeTracking.Confirm(taskTimeTrackingId,
                        verificationWorkerId, verificationDate, false);
                }
            }

            _taskClass.Performers.SetTimeSpend(_performerId, totalTime);

            CancelButton_Click(null, null);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            var confirmButton = sender as Button;
            if (confirmButton == null) return;

            var rowView = confirmButton.DataContext as DataRowView;
            if (rowView == null) return;

            var verificationDate = App.BaseClass.GetDateFromSqlServer();
            var verificationWorkerId = AdministrationClass.CurrentWorkerId;

            rowView["VerificationDate"] = verificationDate;
            rowView["VerificationWorkerID"] = verificationWorkerId;
            rowView["IsVerificated"] = true;
        }
    }
}
