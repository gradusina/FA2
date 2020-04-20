using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using FA2.ChildPages.TaskPage;
using FA2.ChildPages.TechnologyProblemPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для TechnologyProblemPage.xaml
    /// </summary>
    public partial class TechnologyProblemPage
    {
        private bool _firstTimePageRun = true;
        private readonly bool _fullAccess;
        private DateTime _currentTime;
        private bool _needToOpenPupup;
        private int _openingDuration;
        private const string ReceivedText = "\n\nЗаявка принята: {0} дата: {1}.";

        private System.Windows.Forms.Timer _timer;

        private TechnologyProblemClass _tpr;
        private TaskClass _taskClass;
       
        public TechnologyProblemPage(bool fullAccess)
        {
            _fullAccess = fullAccess;
            InitializeComponent();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.TechnologyProblem);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.TechnologyProblem);

            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) =>
                                           {
                                               _firstTimePageRun = false;
                                               _currentTime = App.BaseClass.GetDateFromSqlServer();
                                               FillData();
                                           };
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {

                                                           BindingData();
                                                           SetTimerProperties();
                                                           OnShowClosedRequestCheckBoxUnchecked(null, null);

                                                           var mainWindow = Application.Current.MainWindow as MainWindow;
                                                           if (mainWindow != null) mainWindow.HideWaitAnnimation();
                                                       };

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                var selectedRow = MainDataGrid.SelectedItem as DataRowView;
                FillAndBindingRequestData(selectedRow);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillData()
        {
            App.BaseClass.GetTechnologyProblemClass(ref _tpr);
            App.BaseClass.GetTaskClass(ref _taskClass);

            _tpr.Fill(_currentTime.Subtract(TimeSpan.FromDays(180)), _currentTime);
        }

        private void BindingData()
        {
            DateFromPicker.SelectedDate = _currentTime.Subtract(TimeSpan.FromDays(180));
            DateToPicker.SelectedDate = _currentTime;

            TechProblemNotesListBox.ItemsSource = _tpr.TechnologyProblemsNotesTable.AsDataView();
            ((DataView) TechProblemNotesListBox.ItemsSource).RowFilter =
                string.Format("TechnologyProblemID = {0}", -1);

            MainDataGrid.ItemsSource = _tpr.TechnologyProblemsTable.DefaultView;
            FilterTechProblemRequests();

            FillWorkUnitFilter();
        }

        private void FillWorkUnitFilter()
        {
            var workUnits = _tpr.TechnologyProblemsTable.AsDataView().ToTable(true, "WorkUnitID");
            WorkUnitFilterComboBox.ItemsSource = workUnits.AsDataView();
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null || MainDataGrid.Items.Count == 0) return;

            var drv = (DataRowView) MainDataGrid.SelectedItem;
            if (drv != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить запись?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var techProblemId = Convert.ToInt32(drv.Row["TechnologyProblemID"]);
                    var globalId = drv["GlobalID"].ToString();

                    var requestDate = Convert.ToDateTime(drv["RequestDate"]);
                    var requestWorkerId = Convert.ToInt32(drv["RequestWorkerID"]);

                    _taskClass.DeleteTaskByGlobalId(globalId);

                    _tpr.DeleteTechnologyProblem(techProblemId);
                    AdministrationClass.AddNewAction(21);

                    if (MainDataGrid.Items.Count != 0)
                        MainDataGrid.SelectedIndex = 0;

                    NewsHelper.DeleteNews(requestDate, requestWorkerId);
                }
            }
        }

        private void OnRowMenuContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null || MainDataGrid.Items.Count == 0) return;

            var cellsPresenter = (DataGridCellsPresenter) e.Source;

            var drv = (DataRowView) cellsPresenter.Item;
            if (Convert.ToBoolean(drv.Row["RequestClose"]) || !_fullAccess)
            {
                e.Handled = true;
            }
        }


        #region AddingRequest

        private void OnAddRequestButtonClick(object sender, RoutedEventArgs e)
        {
            _currentTime = App.BaseClass.GetDateFromSqlServer();
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var techProbInfo = new TechnologyProblemInfo(AdministrationClass.CurrentWorkerId);
                mainWindow.ShowCatalogGrid(techProbInfo, "Добавить заявку");
            }
        }

        public void SelectNewTableRow(int problemId)
        {
            var dt = ((DataView)MainDataGrid.ItemsSource).ToTable();
            var dr = dt.Select("TechnologyProblemID = " + problemId);
            if (dr.Length != 0)
            {
                var dataRow = dr[0];
                var rowNumber = dt.Rows.IndexOf(dataRow);
                var drv = ((DataView)MainDataGrid.ItemsSource)[rowNumber];
                if (drv != null)
                    MainDataGrid.SelectedItem = drv;
            }
            else
            {
                if (MainDataGrid.Items.Count != 0)
                    MainDataGrid.SelectedIndex = 0;
            }
        }

        #endregion


        #region MainDataGridSelectionChanged

        private void OnMainDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drv = MainDataGrid.SelectedItem as DataRowView;

            FillAndBindingRequestData(drv);
        }

        private void FillAndBindingRequestData(DataRowView drv)
        {
            SetAdditionalCompletionInformation(drv);
            SetRequestStatusText(drv);
            SetButtonVisibility(drv);
            SetRequestResponsibilities(drv);
            FilterTechProblemNotes(drv);
            SetTechProblemNotesVisibility(drv);
        }

        private void SetAdditionalCompletionInformation(DataRowView drv)
        {
            if (drv == null)
            {
                SaveChangesButton.IsEnabled = false;
                PlannedLaunchDatePicker.IsEnabled = false;
                PlannedLaunchDatePicker.Width = 0;
                PlannedLaunchDatePicker.SelectedDate = null;
                PlannedLaunchLabel.Content = "---";
            }
            else
            {
                if (Convert.ToBoolean(drv.Row["RequestClose"]))
                {
                    SaveChangesButton.IsEnabled = false;
                    PlannedLaunchDatePicker.IsEnabled = false;
                    PlannedLaunchDatePicker.Width = 0;
                    if (drv.Row["PlanedCompletionDate"] != DBNull.Value)
                    {
                        var plannedDate = Convert.ToDateTime(drv.Row["PlanedCompletionDate"]);
                        PlannedLaunchLabel.Content = plannedDate.ToString("dd.MM.yyyy");
                    }
                    else
                        PlannedLaunchLabel.Content = "---";
                }
                else
                {
                    SaveChangesButton.IsEnabled = _fullAccess;
                    PlannedLaunchDatePicker.IsEnabled = _fullAccess;
                    PlannedLaunchDatePicker.Width = 130;
                    PlannedLaunchLabel.Content = "";
                    if (drv.Row["PlanedCompletionDate"] != DBNull.Value)
                    {
                        var plannedDate = Convert.ToDateTime(drv.Row["PlanedCompletionDate"]);
                        PlannedLaunchDatePicker.SelectedDate = plannedDate;
                    }
                    else
                        PlannedLaunchDatePicker.SelectedDate = null;
                }
            }
        }

        private void SetRequestStatusText(DataRowView drv)
        {
            NotReceivedStatusTextBlock.Visibility = Visibility.Collapsed;
            NotCompletedStatusTextBlock.Visibility = Visibility.Collapsed;
            WaitingForCompleteStatusTextBlock.Visibility = Visibility.Collapsed;
            CompletedStatusTextBlock.Visibility = Visibility.Collapsed;

            if (drv != null)
            {
                if (drv.Row["ReceivedDate"] == DBNull.Value)
                {
                    NotReceivedStatusTextBlock.Visibility = Visibility.Visible;
                }
                else if (drv.Row["CompletionDate"] == DBNull.Value)
                {
                    NotCompletedStatusTextBlock.Visibility = Visibility.Visible;
                }
                else if (drv.Row["CompletionDate"] != DBNull.Value)
                {
                    if (!Convert.ToBoolean(drv["RequestClose"]))
                        WaitingForCompleteStatusTextBlock.Visibility = Visibility.Visible;
                    else
                        CompletedStatusTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetButtonVisibility(DataRowView drv)
        {
            ReceiveButton.Visibility = Visibility.Collapsed;
            EditRequestButton.Visibility = Visibility.Collapsed;
            CompleteButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;

            if (drv != null)
            {
                if (drv.Row["ReceivedDate"] == DBNull.Value && _fullAccess)
                {
                    ReceiveButton.Visibility = Visibility.Visible;
                }
                else if (drv.Row["CompletionDate"] == DBNull.Value && _fullAccess)
                {
                    var receivedWorkerId = Convert.ToInt32(drv["ReceivedWorkerID"]);
                    if (receivedWorkerId == AdministrationClass.CurrentWorkerId)
                        EditRequestButton.Visibility = Visibility.Visible;
                }
                else if (drv.Row["CompletionDate"] != DBNull.Value)
                {
                    if (!Convert.ToBoolean(drv["RequestClose"]))
                        CompleteButton.Visibility = Visibility.Visible;
                    else
                        InfoButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetRequestResponsibilities(DataRowView drv)
        {
            if (drv == null)
            {
                TechnologyProblemResponsibilitiesItemsControl.ItemsSource = null;
                return;
            }

            var globalId = drv["GlobalID"].ToString();
            _taskClass.Fill(globalId);
            TechnologyProblemResponsibilitiesItemsControl.ItemsSource =
                _taskClass.Performers.Table.AsDataView();
        }

        private void FilterTechProblemNotes(DataRowView drv)
        {
            if (drv == null)
            {
                ((DataView) TechProblemNotesListBox.ItemsSource).RowFilter =
                    string.Format("TechnologyProblemID = {0}", -1);
                return;
            }

            var techProblemId = Convert.ToInt32(drv["TechnologyProblemID"]);
            ((DataView) TechProblemNotesListBox.ItemsSource).RowFilter =
                string.Format("TechnologyProblemID = {0}", techProblemId);
        }

        private void SetTechProblemNotesVisibility(DataRowView drv)
        {
            TechProblemNotesInfoGrid.Visibility = Visibility.Visible;
            TechProblemNotesInfoGrid.Opacity = 1;

            AddTechProblemNotesTextGrid.Visibility = Visibility.Collapsed;
            AddTechProblemNoteResponsibilitiesGrid.Visibility = Visibility.Collapsed;

            EnterTechProblemNoteTextButton.Visibility = Visibility.Collapsed;
            DeleteTechProblemNoteButton.Visibility = Visibility.Collapsed;

            if (drv == null) return;

            if (Convert.ToBoolean(drv["RequestClose"])) return;

            if (_fullAccess ||
                _taskClass.Performers.Table.AsEnumerable()
                    .Any(r => r.Field<Int64>("WorkerID") == AdministrationClass.CurrentWorkerId))
            {
                EnterTechProblemNoteTextButton.Visibility = Visibility.Visible;
                DeleteTechProblemNoteButton.Visibility = Visibility.Visible;
            }
        }

        public void RefillInfo()
        {
            if (MainDataGrid.SelectedItem != null)
            {
                OnMainDataGridSelectionChanged(null, null);
            }
        }

        #endregion


        #region RequestInfo

        private void OnInfoButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null) return;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var techProbInfo =
                    new TechnologyProblemInfo((DataRowView) MainDataGrid.SelectedItem,
                        AdministrationClass.CurrentWorkerId);
                mainWindow.ShowCatalogGrid(techProbInfo, "Информация");
            }
        }

        public void OpenPopup(object globalId)
        {
            _needToOpenPupup = true;
            RequestClosedPopup.IsOpen = true;
            RequestClosedPopup.StaysOpen = true;
            RequestClosedLabel.Content = globalId;
        }

        #endregion


        #region TimerClock

        private void SetTimerProperties()
        {
            if (_timer == null) _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            if (!_timer.Enabled)
                _timer.Start();
        }

        private void SetTimeOut()
        {
            if (MainDataGrid.Items.Count == 0) return;
            foreach (
                var drv in
                    MainDataGrid.ItemsSource.Cast<DataRowView>().Where(drv => drv["CompletionDate"] == DBNull.Value))
            {
                DateTime startTime;
                var succes = DateTime.TryParse(drv["RequestDate"].ToString(), out startTime);
                if (!succes) return;

                var span = _currentTime.Subtract(startTime);
                var hrs = Convert.ToInt32(Math.Truncate(span.TotalHours));
                var min = span.Minutes;
                var sec = span.Seconds;
                var days = span.Days;
                string time;

                if (days > 0)
                    time = String.Format("{0}:{1}:{2} ({3}д. {4}ч.)", hrs, min.ToString("00"), sec.ToString("00"), days,
                        span.Hours);
                else
                    time = String.Format("{0}:{1}:{2}", hrs, min.ToString("00"), sec.ToString("00"));

                var var = MainDataGrid.Columns[6].GetCellContent(drv) as TextBlock;
                if (var != null)
                    var.Text = time;
            }
        }

        private void FillTimeOutColumn()
        {
            if (MainDataGrid.Items.Count == 0) return;
            foreach (
                var drv in
                    MainDataGrid.Items.Cast<DataRowView>().Where(drv => drv.Row["CompletionDate"] != DBNull.Value))
            {
                DateTime startTime;
                var succes = DateTime.TryParse(drv["RequestDate"].ToString(), out startTime);
                if (!succes) return;
                DateTime stopTime;
                succes = DateTime.TryParse(drv["CompletionDate"].ToString(), out stopTime);
                if (!succes) return;

                var span = stopTime.Subtract(startTime);
                var hrs = Convert.ToInt32(Math.Truncate(span.TotalHours));
                var min = span.Minutes;
                var sec = span.Seconds;
                var days = span.Days;

                var time = days > 0
                    ? String.Format("{0}:{1}:{2} ({3}д. {4}ч.)", hrs, min.ToString("00"), sec.ToString("00"), days,
                        span.Hours)
                    : String.Format("{0}:{1}:{2}", hrs, min.ToString("00"), sec.ToString("00"));

                var var = MainDataGrid.Columns[6].GetCellContent(drv) as TextBlock;
                if (var != null)
                    var.Text = time;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _currentTime += new TimeSpan(0, 0, 1);
            SetTimeOut();

            if (_needToOpenPupup)
                OpenPopupForTime(5);

            if(ShowClosedRequestCheckBox.IsChecked == true)
                FillTimeOutColumn();
        }

        private void OpenPopupForTime(int duration)
        {
            _openingDuration++;

            if (_openingDuration == 3)
                RequestClosedPopup.StaysOpen = false;

            if (_openingDuration == duration)
            {
                RequestClosedPopup.IsOpen = false;
                _needToOpenPupup = false;
                _openingDuration = 0;
            }
        }

        #endregion


        private void OnShowClosedRequestCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            FilterTechProblemRequests();
        }

        private void OnShowClosedRequestCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            FilterTechProblemRequests();
        }

        private void OnShowButtonClick(object sender, RoutedEventArgs e)
        {
            if (DateFromPicker.SelectedDate == null || DateToPicker.SelectedDate == null) return;

            _tpr.Fill(DateFromPicker.SelectedDate.Value, DateToPicker.SelectedDate.Value);

            FillWorkUnitFilter();

            if (WorkSectionFilterEnable.IsChecked.HasValue && WorkSectionFilterEnable.IsChecked.Value)
            {
                if (WorkUnitFilterComboBox.HasItems)
                    WorkUnitFilterComboBox.SelectedIndex = 0;
            }
        }

        private void OnSaveChangesButtonClick(object sender, RoutedEventArgs e)
        {
            if (PlannedLaunchDatePicker.SelectedDate == null || MainDataGrid.SelectedItem == null) return;

            var techProblemId = Convert.ToInt32(((DataRowView) MainDataGrid.SelectedItem).Row["TechnologyProblemID"]);
            object plannedLaunchDate = DBNull.Value;
            if (PlannedLaunchDatePicker.SelectedDate != null)
                plannedLaunchDate = PlannedLaunchDatePicker.SelectedDate.Value;
            _tpr.FillPlannedCompletionDate(techProblemId, plannedLaunchDate, 
                App.BaseClass.GetDateFromSqlServer(), AdministrationClass.CurrentWorkerId);
            AdministrationClass.AddNewAction(20);
        }

        private void OnReceiveButtonClick(object sender, RoutedEventArgs e)
        {
            var request = MainDataGrid.SelectedItem as DataRowView;
            if (request == null) return;

            var globalId = request["GlobalID"].ToString();
            var taskName = new IdToWorkSectionConverter().Convert(request["WorkSectionID"], typeof(string), null,
                CultureInfo.InvariantCulture).ToString();
            var taskDescription = request["RequestNotes"].ToString();
            const TaskClass.SenderApplications senderApplication = TaskClass.SenderApplications.TechnologyProblem;

            var addNewTaskWindow = new AddNewTask(globalId, taskName, taskDescription, senderApplication);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewTaskWindow, "Выбрать исполнителей");
            }
        }

        public void ReceiveRequest(string globalId, string receivedNotes)
        {
            var rows = _tpr.TechnologyProblemsTable.AsEnumerable().Where(r => r.Field<string>("GlobalID") == globalId);
            if(!rows.Any()) return;

            var techProblem = rows.First();
            var techProblemId = Convert.ToInt32(techProblem["TechnologyProblemID"]);
            var receivedDate = App.BaseClass.GetDateFromSqlServer();
            var receiverWorkerId = AdministrationClass.CurrentWorkerId;
            _tpr.FillReceivedInfo(techProblemId, receivedDate, receiverWorkerId, receivedNotes);
            AdministrationClass.AddNewAction(18);

            var requestDate = Convert.ToDateTime(techProblem["RequestDate"]);
            var requestWorkerId = Convert.ToInt32(techProblem["RequestWorkerID"]);
            var workerName =
                new IdToNameConverter().Convert(receiverWorkerId, typeof (string), "ShortName", new CultureInfo("ru-RU"))
                    .ToString();
            var newsText = string.Format(ReceivedText, workerName, receivedDate);
            NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);
        }

        private void OnEditRequestButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null) return;

            var drv = (DataRowView) MainDataGrid.SelectedItem;
            if (drv["ReceivedDate"] == DBNull.Value) return;

            var globalId = drv["GlobalID"].ToString();
            var rows = _taskClass.Tasks.Table.AsEnumerable().
                Where(t => t.Field<string>("GlobalID") == globalId);
            if (!rows.Any()) return;

            var task = rows.First();
            var mainWorkerId = Convert.ToInt32(drv["ReceivedWorkerID"]);
            var fullAccess = mainWorkerId == AdministrationClass.CurrentWorkerId;

            var addNewTaskWindow = new AddNewTask(task, fullAccess);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewTaskWindow, "Список исполнителей");
            }
        }

        public void ChangeReceivedNotes(string receivedNotes)
        {
            var request = MainDataGrid.SelectedItem as DataRowView;
            if(request == null) return;

            var techProblemId = Convert.ToInt32(request["TechnologyProblemID"]);
            _tpr.ChangeReceivedNotes(techProblemId, receivedNotes);
        }

        private void OnCompleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null) return;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var techProbInfo =
                    new TechnologyProblemInfo((DataRowView)MainDataGrid.SelectedItem,
                        AdministrationClass.CurrentWorkerId);
                mainWindow.ShowCatalogGrid(techProbInfo, "Информация");
            }
        }



        #region Technology problem notes

        private void OnEnterTechProblemNoteTextButtonClick(object sender, RoutedEventArgs e)
        {
            var request = MainDataGrid.SelectedItem as DataRowView;
            if(request == null) return;

            TechProblemNoteTextBox.Text = string.Empty;

            var requestResponsibilities = _taskClass.Performers.Table.Copy().AsDataView();
            RequestResponsibilitiesItemsControl.ItemsSource = requestResponsibilities;

            var techProblemNoteResponsibilities = _taskClass.Performers.Table.Clone().AsDataView();
            TechProblemNoteResponsibilitiesItemsControl.ItemsSource = techProblemNoteResponsibilities;

            ChangePanelsVisibility(TechProblemNotesInfoGrid, AddTechProblemNotesTextGrid);
        }

        private void OnCancelAddTechProblemNotesButtonClick(object sender, RoutedEventArgs e)
        {
            ChangePanelsVisibility(AddTechProblemNotesTextGrid, TechProblemNotesInfoGrid);
        }

        private void OnSelectResponsibleWorkersForTechProblemNoteButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TechProblemNoteTextBox.Text)) return;

            ChangePanelsVisibility(AddTechProblemNotesTextGrid, AddTechProblemNoteResponsibilitiesGrid);
        }

        private void OnGoBackToTechProblemNoteTextButtonClick(object sender, RoutedEventArgs e)
        {
            ChangePanelsVisibility(AddTechProblemNoteResponsibilitiesGrid, AddTechProblemNotesTextGrid);
        }

        private void OnAddTechProblemNoteResponsibilityButtonClick(object sender, RoutedEventArgs e)
        {
            var performer = ((Button)sender).DataContext as DataRowView;
            if (performer == null) return;

            ((DataView)TechProblemNoteResponsibilitiesItemsControl.ItemsSource).Table.Rows.Add(performer.Row.ItemArray);
            ((DataView)RequestResponsibilitiesItemsControl.ItemsSource).Table.Rows.Remove(performer.Row);
        }

        private void OnRemoveTechProblemNoteResponsibilityButtonClick(object sender, RoutedEventArgs e)
        {
            var performer = ((Button)sender).DataContext as DataRowView;
            if (performer == null) return;

            ((DataView)RequestResponsibilitiesItemsControl.ItemsSource).Table.Rows.Add(performer.Row.ItemArray);
            ((DataView)TechProblemNoteResponsibilitiesItemsControl.ItemsSource).Table.Rows.Remove(performer.Row);
        }

        private void OnAddNewTechProblemNoteButtonClick(object sender, RoutedEventArgs e)
        {
            var request = MainDataGrid.SelectedItem as DataRowView;
            if (request == null) return;

            var techProblemNoteResponsibilities =
                TechProblemNoteResponsibilitiesItemsControl.ItemsSource as DataView;
            if (techProblemNoteResponsibilities == null ||
                techProblemNoteResponsibilities.Count == 0) return;

            var workerIds = from performer in techProblemNoteResponsibilities.Table.AsEnumerable()
                            select Convert.ToInt32(performer["WorkerID"]);
            if (!workerIds.Any()) return;

            var techProblemId = Convert.ToInt32(request["TechnologyProblemID"]);
            var techProblemNoteText = TechProblemNoteTextBox.Text;
            var techProblemNoteDate = App.BaseClass.GetDateFromSqlServer();

            var technologyProblemNoteId =
                _tpr.AddNewTechnologyProblemNote(techProblemId, techProblemNoteText, techProblemNoteDate);

            foreach (var workerId in workerIds)
            {
                _tpr.AddNewTechnologyProblemNoteResponsible(technologyProblemNoteId, techProblemId, workerId);
            }

            ChangePanelsVisibility(AddTechProblemNoteResponsibilitiesGrid, TechProblemNotesInfoGrid);
        }

        private static void ChangePanelsVisibility(UIElement hidingGrid, UIElement appearingGrid)
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(150));
            var animation = new DoubleAnimation(1, 0, duration, FillBehavior.Stop);
            animation.Completed += (o, args) =>
            {
                hidingGrid.Visibility = Visibility.Collapsed;
                appearingGrid.Visibility = Visibility.Visible;
                animation = new DoubleAnimation(0, 1, duration, FillBehavior.Stop);
                appearingGrid.BeginAnimation(OpacityProperty, animation);
            };
            hidingGrid.BeginAnimation(OpacityProperty, animation);
        }

        private void OnDeleteTechProblemNoteButtonClick(object sender, RoutedEventArgs e)
        {
            var techProblemNote = TechProblemNotesListBox.SelectedItem as DataRowView;
            if (techProblemNote == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную заметку?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var techProblemNoteId = Convert.ToInt32(techProblemNote["TechnologyProblemNoteID"]);
            _tpr.DeleteTechnologyProblemNote(techProblemNoteId);
        }

        #endregion


        private void OnWorkSectionFilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterTechProblemRequests();
        }

        private void OnWorkUnitFilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workUnit = WorkUnitFilterComboBox.SelectedItem as DataRowView;

            var workUnitId = workUnit != null
                ? Convert.ToInt32(workUnit["WorkUnitID"])
                : -1;
            var view = _tpr.TechnologyProblemsTable.AsDataView();
            view.RowFilter = "WorkUnitID = " + workUnitId;
            var workSections = view.ToTable(true, "WorkSectionID");

            WorkSectionFilterComboBox.ItemsSource = workSections.AsDataView();

            if (WorkSectionFilterComboBox.Items.Count != 0)
                WorkSectionFilterComboBox.SelectedIndex = 0;
        }

        private void FilterTechProblemRequests()
        {
            var showClosed = ShowClosedRequestCheckBox.IsChecked.HasValue &&
                             ShowClosedRequestCheckBox.IsChecked.Value;
            var workSectionFilterEnable = WorkSectionFilterEnable.IsChecked.HasValue &&
                                          WorkSectionFilterEnable.IsChecked.Value;

            var workSection = WorkSectionFilterComboBox.SelectedItem as DataRowView;
            var workSectionId = workSection != null
                ? Convert.ToInt32(workSection["WorkSectionID"])
                : -1;

            var filterString = "Enable = 'True'";
            if (!showClosed)
                filterString += " AND RequestClose = 'False'";
            if (workSectionFilterEnable)
                filterString += " AND WorkSectionID = " + workSectionId;

            _tpr.TechnologyProblemsTable.DefaultView.RowFilter = filterString;

            if (MainDataGrid.SelectedIndex == -1 && MainDataGrid.Items.Count != 0)
                MainDataGrid.SelectedIndex = 0;
        }

        private void OnWorkSectionFilterEnableChecked(object sender, RoutedEventArgs e)
        {
            if (WorkUnitFilterComboBox.HasItems)
                WorkUnitFilterComboBox.SelectedIndex = 0;
        }

        private void OnWorkSectionFilterEnableUnchecked(object sender, RoutedEventArgs e)
        {
            WorkUnitFilterComboBox.SelectedIndex = -1;
        }
    }
}
