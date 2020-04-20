using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.Classes.WorkerRequestsEnums;
using FA2.XamlFiles;
using FA2.Notifications;
using FA2.Converters;
using System.Linq;
using System.Globalization;

namespace FA2.ChildPages.WorkerRequestsPage
{
    /// <summary>
    /// Логика взаимодействия для AddNewWorkerRequestPage.xaml
    /// </summary>
    public partial class AddNewWorkerRequestPage
    {
        private enum InitializeMode
        {
            FromSender,
            FromMainWorker
        }

        private InitializeMode _initializeMode;


        private enum SelectionWorkerMode
        {
            WorkerSelection,
            MainWorkerSelection
        }

        private SelectionWorkerMode _selectionWorkerMode;


        private WorkerRequestsClass _workerRequestsClass;
        private StaffClass _staffClass;
        private long? _selectedWorkerId;
        private long? _selectedMainWorkerId;


        public AddNewWorkerRequestPage(long workerId)
        {
            _initializeMode = InitializeMode.FromSender;
            _selectedWorkerId = workerId;

            InitializeComponent();

            FillData();
            BindingData();
            SetEnables();
        }

        public AddNewWorkerRequestPage(long? workerId, long mainWorkerId)
        {
            _initializeMode = InitializeMode.FromMainWorker;
            _selectedWorkerId = workerId;
            _selectedMainWorkerId = mainWorkerId;

            InitializeComponent();

            FillData();
            BindingData();
            SetEnables();
        }



        private void FillData()
        {
            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestsClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
        }

        private void BindingData()
        {
            SalarySaveTypeComboBox.ItemsSource = _workerRequestsClass.SalatySaveTypesTable.AsDataView();
            if (SalarySaveTypeComboBox.HasItems)
                SalarySaveTypeComboBox.SelectedIndex = 0;

            IntervalTypeComboBox.ItemsSource = _workerRequestsClass.IntervalTypesTable.AsDataView();
            if (IntervalTypeComboBox.HasItems)
                IntervalTypeComboBox.SelectedIndex = 0;

            InitiativeTypeComboBox.ItemsSource = _workerRequestsClass.InitiativeTypesTable.AsDataView();
            if (InitiativeTypeComboBox.HasItems)
                InitiativeTypeComboBox.SelectedIndex = 0;

            WorkingOffTypeComboBox.ItemsSource = _workerRequestsClass.WorkingOffTypesTable.AsDataView();
            if (WorkingOffTypeComboBox.HasItems)
                WorkingOffTypeComboBox.SelectedIndex = 0;

            RequestTypeComboBox.ItemsSource = _workerRequestsClass.RequestTypesTable.AsDataView();
            if (RequestTypeComboBox.HasItems)
                RequestTypeComboBox.SelectedIndex = 0;

            RequestReasonCompleteComboBox.DataSource = _workerRequestsClass.DefaultRequestReasonsTable.AsDataView();
            RequestReasonCompleteComboBox.DisplayMemberPath = "DefaultRequestReasonString";
            RequestReasonCompleteComboBox.SelectedValuePath = "DefaultRequestReasonID";

            var workerGroupsView = _staffClass.GetWorkerGroups();
            WorkerGroupsView.ItemsSource = workerGroupsView;
            if (WorkerGroupsView.HasItems)
                WorkerGroupsView.SelectedIndex = 0;

            var factoriesView = _staffClass.GetFactories();
            FactoriesView.ItemsSource = factoriesView;
            if (FactoriesView.HasItems)
                FactoriesView.SelectedIndex = 0;

            WorkerTextBlock.DataContext = _selectedWorkerId;
            if (_initializeMode == InitializeMode.FromMainWorker)
                MainWorkerTextBlock.DataContext = _selectedMainWorkerId;
        }

        private void SetEnables()
        {
            ChangeWorkerButton.Visibility =
                _initializeMode == InitializeMode.FromSender
                ? Visibility.Collapsed
                : Visibility.Visible;
        }


        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnIntervalTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var intervalType = IntervalTypeComboBox.SelectedItem as DataRowView;
            if (intervalType == null)
            {
                DuringTheDayPanel.Visibility = Visibility.Visible;
                DuringTheDayHoursPanel.Visibility = Visibility.Visible;
                DuringTheSomeDaysPanel.Visibility = Visibility.Hidden;
                return;
            }

            var intervalTypeId = Convert.ToInt32(intervalType["IntervalTypeID"]);
            switch ((IntervalType)intervalTypeId)
            {
                case IntervalType.DurringSomeHours:
                    DuringTheDayPanel.Visibility = Visibility.Visible;
                    DuringTheDayHoursPanel.Visibility = Visibility.Visible;
                    DuringTheSomeDaysPanel.Visibility = Visibility.Hidden;
                    break;
                case IntervalType.DurringWorkingDay:
                    DuringTheDayPanel.Visibility = Visibility.Visible;
                    DuringTheDayHoursPanel.Visibility = Visibility.Hidden;
                    DuringTheSomeDaysPanel.Visibility = Visibility.Hidden;
                    break;
                case IntervalType.DurringSomeDays:
                    DuringTheDayPanel.Visibility = Visibility.Hidden;
                    DuringTheDayHoursPanel.Visibility = Visibility.Hidden;
                    DuringTheSomeDaysPanel.Visibility = Visibility.Visible;
                    break;
            }
        }



        private void OnShowMainWorkersViewButtonClick(object sender, RoutedEventArgs e)
        {
            _selectionWorkerMode = SelectionWorkerMode.MainWorkerSelection;
            ShowWorkersView();
        }

        private void OnShowWorkersViewButtonClick(object sender, RoutedEventArgs e)
        {
            _selectionWorkerMode = SelectionWorkerMode.WorkerSelection;
            ShowWorkersView();
        }

        private void ShowWorkersView()
        {
            MainGrid.IsEnabled = false;
            ShadowGrid.Visibility = Visibility.Visible;

            var widthAnimation = new DoubleAnimation(250d, new Duration(TimeSpan.FromMilliseconds(200))) { DecelerationRatio = 0.9 };
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color { A = 20, R = 0, G = 0, B = 0 };
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void WorkerFilters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WorkerSearchTextBox.TextChanged -= WorkerSearchTextBox_OnTextChanged;
            WorkerSearchTextBox.Text = string.Empty;
            WorkerSearchTextBox.TextChanged += WorkerSearchTextBox_OnTextChanged;

            if (WorkerGroupsView.SelectedItem == null || FactoriesView.SelectedItem == null)
            {
                WorkersView.ItemsSource = null;
                return;
            }

            var workerGroupId = Convert.ToInt32(WorkerGroupsView.SelectedValue);
            var factoryId = Convert.ToInt32(FactoriesView.SelectedValue);
            var workersView = _staffClass.FilterWorkers(true, workerGroupId, true, factoryId, false, -1);
            WorkersView.ItemsSource = workersView.AsDataView();
        }

        private void WorkerSearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (WorkersView.ItemsSource == null) return;

            var searchText = WorkerSearchTextBox.Text.Trim().ToLower();
            var filteredView = ((DataView)WorkersView.ItemsSource).Table.AsEnumerable().
                Where(r => r.Field<string>("Name").ToLower().Contains(searchText)).AsDataView();
            filteredView.Sort = "Name";
            WorkersView.ItemsSource = filteredView;
        }

        private void AddPerformersButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedWorker = WorkersView.SelectedItem as DataRowView;
            if(selectedWorker == null) return;

            if(_selectionWorkerMode == SelectionWorkerMode.MainWorkerSelection)
            {
                _selectedMainWorkerId = Convert.ToInt64(selectedWorker["WorkerID"]);
                MainWorkerTextBlock.DataContext = _selectedMainWorkerId;
            }
            else
            {
                _selectedWorkerId = Convert.ToInt64(selectedWorker["WorkerID"]);
                WorkerTextBlock.DataContext = _selectedWorkerId;
            }

            CancelAddPerformersButton_OnClick(null, null);
        }

        private void CancelAddPerformersButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainGrid.IsEnabled = true;
            WorkersView.SelectedIndex = -1;

            var widthAnimation = new DoubleAnimation(0d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color { A = 0, R = 0, G = 0, B = 0 };
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            colorAnimation.Completed += (s, args) => { ShadowGrid.Visibility = Visibility.Collapsed; };
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            #region Check for empty fields

            var hasEmptyField = false;
            var errorBrush = Resources["RedForeground"] != null
                ? Resources["RedForeground"] as SolidColorBrush
                : Brushes.IndianRed;
            var defaultBrush = Resources["AdditTextBlackBrush"] != null
                ? Resources["AdditTextBlackBrush"] as SolidColorBrush
                : Brushes.LightGray;

            if (RequestTypeComboBox.SelectedItem == null)
            {
                RequestTypeDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                RequestTypeDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (SalarySaveTypeComboBox.SelectedItem == null)
            {
                SalarySaveTypeDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                SalarySaveTypeDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (IntervalTypeComboBox.SelectedItem == null)
            {
                IntervalTypeDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                IntervalTypeDescriptionTextBlock.Foreground = defaultBrush;
                var intTypeId = Convert.ToInt32(IntervalTypeComboBox.SelectedValue);
                switch ((IntervalType)intTypeId)
                {
                    case IntervalType.DurringSomeHours:
                        {
                            if (DuringTheDayRequestDatePicker.SelectedDate == null ||
                                RequestFromTimeControl.TotalTime == TimeSpan.Zero ||
                                RequestToTimeControl.TotalTime == TimeSpan.Zero)
                            {
                                IntervalTypeDescriptionTextBlock.Foreground = errorBrush;
                                hasEmptyField = true;
                            }
                        }
                        break;
                    case IntervalType.DurringWorkingDay:
                        {
                            if (DuringTheDayRequestDatePicker.SelectedDate == null)
                            {
                                IntervalTypeDescriptionTextBlock.Foreground = errorBrush;
                                hasEmptyField = true;
                            }
                        }
                        break;
                    case IntervalType.DurringSomeDays:
                        {
                            if (RequestFromDatePicker.SelectedDate == null ||
                                RequestToDatePicker.SelectedDate == null)
                            {
                                IntervalTypeDescriptionTextBlock.Foreground = errorBrush;
                                hasEmptyField = true;
                            }
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(RequestReasonCompleteComboBox.Text))
            {
                RequestReasonDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                RequestReasonDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (WorkingOffTypeComboBox.SelectedItem == null)
            {
                WorkingOffTypeDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                WorkingOffTypeDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (InitiativeTypeComboBox.SelectedItem == null)
            {
                InitiativeTypeDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                InitiativeTypeDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (_selectedMainWorkerId == null)
            {
                MainWorkerDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                MainWorkerDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (_selectedWorkerId == null)
            {
                WorkerDescriptionTextBlock.Foreground = errorBrush;
                hasEmptyField = true;
            }
            else
            {
                WorkerDescriptionTextBlock.Foreground = defaultBrush;
            }

            if (hasEmptyField) return;

            #endregion

            var requestType = (RequestType)Convert.ToInt32(RequestTypeComboBox.SelectedValue);
            var salarySaveType = (SalarySaveType)Convert.ToInt32(SalarySaveTypeComboBox.SelectedValue);
            var intervalType = (IntervalType)Convert.ToInt32(IntervalTypeComboBox.SelectedValue);
            var requestDateFrom = new DateTime();
            var requestDateTo = new DateTime();
            switch (intervalType)
            {
                case IntervalType.DurringSomeHours:
                    {
                        requestDateFrom =
                            DuringTheDayRequestDatePicker.SelectedDate.Value.Date.Add(RequestFromTimeControl.TotalTime);
                        requestDateTo =
                            DuringTheDayRequestDatePicker.SelectedDate.Value.Date.Add(RequestToTimeControl.TotalTime);
                    }
                    break;
                case IntervalType.DurringWorkingDay:
                    {
                        requestDateFrom = DuringTheDayRequestDatePicker.SelectedDate.Value;
                        requestDateTo = DuringTheDayRequestDatePicker.SelectedDate.Value;
                    }
                    break;
                case IntervalType.DurringSomeDays:
                    {
                        requestDateFrom = RequestFromDatePicker.SelectedDate.Value;
                        requestDateTo = RequestToDatePicker.SelectedDate.Value;
                    }
                    break;
            }
            var requestReason = RequestReasonCompleteComboBox.Text;
            var initiativeType = (InitiativeType)Convert.ToInt32(InitiativeTypeComboBox.SelectedValue);
            var workingOffType = (WorkingOffType)Convert.ToInt32(WorkingOffTypeComboBox.SelectedValue);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            var workerRequestId = _workerRequestsClass.AddWorkerRequest(requestType, _selectedWorkerId.Value, requestDateFrom,
                requestDateTo, salarySaveType, initiativeType, intervalType, workingOffType, currentDate, currentWorkerId, requestReason,
                _selectedMainWorkerId.Value);

            if(_initializeMode == InitializeMode.FromSender)
                AdministrationClass.AddNewAction(79);
            else
                AdministrationClass.AddNewAction(80);

            NotificationManager.AddNotification((int)_selectedMainWorkerId.Value, AdministrationClass.Modules.WorkerRequests, (int)workerRequestId);

            var newsText = string.Format("Заявка на {0}, составлена на: {1} \nПериод: {2} \nПричина: {3} \nСоставитель заявки: {4} \nКому на подтверждение: {5}",
                new WorkerRequestConverter().Convert((int)requestType, typeof(string), "RequestTypeName", CultureInfo.InvariantCulture).ToString().ToLower(),
                new IdToNameConverter().Convert(_selectedWorkerId.Value, "FullName"),
                WorkerRequestDurationConverter.GetDateDuration(intervalType, requestDateFrom, requestDateTo) + " (" + WorkerRequestDurationConverter.GetTimeDuration(intervalType, requestDateFrom, requestDateTo) + ")",
                requestReason,
                new IdToNameConverter().Convert(currentWorkerId, "ShortName"),
                new IdToNameConverter().Convert(_selectedMainWorkerId.Value, "ShortName"));

            string globalId = null;
            var rows = _workerRequestsClass.WorkerRequestsTable.Select(string.Format("WorkerRequestID = {0}", workerRequestId));
            if(rows.Any())
            {
                globalId = rows.First()["GlobalID"].ToString();
            }
            NewsHelper.AddNews(currentDate, newsText, 9, (int)_selectedWorkerId.Value, globalId);

            OnClosePageButtonClick(null, null);
        }



        private void OnRequestTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var requestTypeRow = RequestTypeComboBox.SelectedItem as DataRowView;
            if (requestTypeRow == null)
            {
                WorkingOffTypeComboBox.IsEnabled = true;
                return;
            }

            var requestType = (RequestType)Convert.ToInt32(RequestTypeComboBox.SelectedValue);
            switch (requestType)
            {
                case RequestType.Vacation:
                    WorkingOffTypeComboBox.IsEnabled = true;
                    break;
                case RequestType.ExtraWork:
                    if (WorkingOffTypeComboBox.HasItems)
                        WorkingOffTypeComboBox.SelectedValue = (int)WorkingOffType.Without;
                    WorkingOffTypeComboBox.IsEnabled = false;
                    break;
            }
        }
    }
}
