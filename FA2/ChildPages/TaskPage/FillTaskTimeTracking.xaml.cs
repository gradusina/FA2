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
    /// Логика взаимодействия для AddTaskTimeTracking.xaml
    /// </summary>
    public partial class FillTaskTimeTracking
    {
        private readonly TaskClass _taskClass;
        private readonly TimeTrackingClass _timeTrackingClass;
        private readonly long _taskId;
        private readonly long _performerId;
        private BindingListCollectionView _taskTimeTrackingView;

        public FillTaskTimeTracking(long taskId, long performerId)
        {
            InitializeComponent();

            _taskId = taskId;
            _performerId = performerId;
            App.BaseClass.GetTaskClass(ref _taskClass);
            App.BaseClass.GetTimeTrackingClass(ref _timeTrackingClass);

            FillData();

            TotalTimeIntervalLabel.Content = CalculateTotalTime();
        }

        private void FillData()
        {
            var taskTimeTrackingView = _taskClass.TaskTimeTracking.Table.AsDataView();
            _taskTimeTrackingView = new BindingListCollectionView(taskTimeTrackingView);
            if (_taskTimeTrackingView.GroupDescriptions != null)
                _taskTimeTrackingView.GroupDescriptions.Add(new PropertyGroupDescription("Date"));
            _taskTimeTrackingView.CustomFilter = string.Format("PerformerID = {0}", _performerId);

            TimeTrackingItemsControl.ItemsSource = _taskTimeTrackingView;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void DeleteTimeTracking_OnClick(object sender, RoutedEventArgs e)
        {
            var deleteButton = sender as Button;
            if (deleteButton == null) return;

            var taskTimeTracking = deleteButton.DataContext as DataRowView;
            if (taskTimeTracking == null) return;

            int taskTimeTrackingId;
            Int32.TryParse(taskTimeTracking["TaskTimeTrackingID"].ToString(), out taskTimeTrackingId);

            _taskClass.DeleteTaskTimeTracking(taskTimeTrackingId);

            var totalTime = CalculateTotalTime();
            _taskClass.Performers.SetTimeSpend(_performerId, totalTime);
            TotalTimeIntervalLabel.Content = totalTime;
        }

        private TimeSpan CalculateTotalTime()
        {
            if (_taskTimeTrackingView.Count == 0) return TimeSpan.Zero;

            var totalTime = new TimeSpan();
            totalTime =
                _taskTimeTrackingView.Cast<DataRowView>().Select(dataRow =>
                    TimeIntervalCountConverter.CalculateTimeInterval(
                        (TimeSpan)dataRow["TimeStart"], (TimeSpan)dataRow["TimeEnd"]))
                    .Aggregate(totalTime, (current, interval) => current.Add(interval));
            return totalTime;
        }

        private void OnAddTaskTimeTrackingButtonClick(object sender, RoutedEventArgs e)
        {         
            if (_timeTrackingClass.GetIsDayEnd(AdministrationClass.CurrentWorkerId))
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.BlinkWorkingDayButton();
                }
                return;
            }

            var timeStart = TimeStarTimeControl.TotalTime;
            var timeEnd = TimeEndTimeControl.TotalTime;

            if (timeStart == timeEnd) return;

            var date = _timeTrackingClass.CurrentWorkerStartDate;
            var timeSpentAtWorkId = _timeTrackingClass.CurrentTimeSpentAtWorkID;

            _taskClass.AddNewTaskTimeTracking(_taskId, _performerId, timeSpentAtWorkId, date, timeStart, timeEnd);

            var totalTime = CalculateTotalTime();
            _taskClass.Performers.SetTimeSpend(_performerId, totalTime);

            TotalTimeIntervalLabel.Content = totalTime;

            TimeStarTimeControl.TotalTime = TimeSpan.Zero;
            TimeEndTimeControl.TotalTime = TimeSpan.Zero;
        }
    }
}
