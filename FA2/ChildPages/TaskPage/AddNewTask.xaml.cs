using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FA2.ViewModels;
using FA2.XamlFiles;

namespace FA2.ChildPages.TaskPage
{
    /// <summary>
    /// Логика взаимодействия для AddNewTask.xaml
    /// </summary>
    public partial class AddNewTask
    {
        private readonly int _taskId;
        private readonly string _globalId;
        private readonly TaskClass.SenderApplications _senderApplication;
        private readonly AddNewTaskOpeningType _openingType;
        private AddingWorkersMode _addingWorkersMode;

        private readonly TaskClass _taskClass;
        private readonly StaffClass _staffClass;
        private readonly NewsFeedClass _newsFeedClass;

        private ObservableCollection<Worker> _performers;
        private ObservableCollection<Worker> _newPerformers;
        private ObservableCollection<Worker> _deletedPerformers;

        private ObservableCollection<Worker> _observers;
        private ObservableCollection<Worker> _newObservers;
        private ObservableCollection<Worker> _deletedObservers;

        public AddNewTask(TaskClass.SenderApplications senderApplication)
        {
            InitializeComponent();

            CreateCollections();

            App.BaseClass.GetTaskClass(ref _taskClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);

            _openingType = AddNewTaskOpeningType.AddingEmpty;
            _senderApplication = senderApplication;
            _taskId = -1;

            FillPerformers();
            FillObservers();
            SetBindings();
        }

        public AddNewTask(object taskData, bool fullAccess)
        {
            InitializeComponent();

            if (!fullAccess)
            {
                TaskName.IsReadOnly = true;
                TaskDescription.IsReadOnly = true;
                IsDeadLineEnable.IsEnabled = false;
                DeadLineDate.IsEnabled = false;
                ShowWorkersViewButton.Visibility = Visibility.Collapsed;
                OkButton.Visibility = Visibility.Collapsed;
            }

            CreateCollections();

            App.BaseClass.GetTaskClass(ref _taskClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);

            _openingType = AddNewTaskOpeningType.Changing;
            DataContext = taskData;

            if (taskData == null)
                _taskId = -1;
            else
            {
                var row = taskData as DataRow;
                if (row != null)
                    _taskId = Convert.ToInt32(row["TaskID"]);
                else
                {
                    var rowView = taskData as DataRowView;
                    if (rowView != null)
                        _taskId = Convert.ToInt32(rowView["TaskID"]);
                }
            }

            FillPerformers();
            FillObservers();
            SetBindings();
        }

        public AddNewTask(string globalId, string taskName, string description,
            TaskClass.SenderApplications senderApplication,
            bool isDeadLine = false, DateTime deadLine = new DateTime())
        {
            InitializeComponent();

            CreateCollections();

            App.BaseClass.GetTaskClass(ref _taskClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);

            _openingType = AddNewTaskOpeningType.AddingBasedOnValues;
            _globalId = globalId;
            _senderApplication = senderApplication;
            TaskName.Text = taskName;
            TaskDescription.Text = description;

            if (isDeadLine)
            {
                IsDeadLineEnable.IsChecked = true;
                DeadLineDate.SelectedDate = deadLine;
            }

            FillPerformers();
            FillObservers();
            SetBindings();
        }

        private void CreateCollections()
        {
            _performers = new ObservableCollection<Worker>();
            _newPerformers = new ObservableCollection<Worker>();
            _deletedPerformers = new ObservableCollection<Worker>();

            _observers = new ObservableCollection<Worker>();
            _newObservers = new ObservableCollection<Worker>();
            _deletedObservers = new ObservableCollection<Worker>();
        }

        private void FillPerformers()
        {
            if (_openingType == AddNewTaskOpeningType.AddingEmpty ||
                _openingType == AddNewTaskOpeningType.AddingBasedOnValues)
            {
                var currentWorkerId = AdministrationClass.CurrentWorkerId;
                var currentWorkerName = _staffClass.GetWorkerName(currentWorkerId, true);
                var currentWorker = new Worker(currentWorkerId, currentWorkerName, null);

                _performers.Add(currentWorker);
                _newPerformers.Add(currentWorker);
            }
            else if (_openingType == AddNewTaskOpeningType.Changing)
            {
                var performers = _taskClass.Performers.Table.AsEnumerable()
                    .Where(t => t.Field<Int64>("TaskID") == _taskId)
                    .CopyToDataTable();
                foreach (var worker in from performer in performers.AsEnumerable()
                    select performer.Field<Int64>("WorkerID")
                    into workerId
                    let workerName = _staffClass.GetWorkerName((int) workerId, true)
                    select new Worker(workerId, workerName, null))
                {
                    _performers.Add(worker);
                }
            }

            var thread = new Thread(() =>
            {
                foreach (var performer in _performers)
                {
                    var worker = performer;

                    var photo = GetBitmapImage((int) worker.WorkerID);
                    worker.Photo = photo;
                }

            }) {IsBackground = true};
            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
        }

        private void FillObservers()
        {
            if (_openingType == AddNewTaskOpeningType.Changing)
            {
                var observers = _taskClass.Observers.Table.AsEnumerable()
                    .Where(t => t.Field<Int64>("TaskID") == _taskId);

                foreach (var worker in from observer in observers.AsEnumerable()
                    select observer.Field<Int64>("WorkerID")
                    into workerId
                    let workerName = _staffClass.GetWorkerName((int) workerId, true)
                    select new Worker(workerId, workerName, null))
                {
                    _observers.Add(worker);
                }
            }
        }

        private void SetBindings()
        {
            PerformersList.ItemsSource = _performers;
            ObserversItemsControl.ItemsSource = _observers;

            var workerGroupsView = _staffClass.GetWorkerGroups();
            WorkerGroupsView.ItemsSource = workerGroupsView;
            if (WorkerGroupsView.HasItems)
                WorkerGroupsView.SelectedIndex = 0;

            var factoriesView = _staffClass.GetFactories();
            FactoriesView.ItemsSource = factoriesView;
            if (FactoriesView.HasItems)
                FactoriesView.SelectedIndex = 0;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            PerformersList.ItemsSource = null;
            _performers = null;
            _newPerformers = null;
            _deletedPerformers = null;
            _observers = null;
            _newObservers = null;
            _deletedObservers = null;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }



        private void ShowWorkersViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            _addingWorkersMode = AddingWorkersMode.ToPerformers;

            ShowWorkersView();
        }

        private void OnShowObserversViewButtonClick(object sender, RoutedEventArgs e)
        {
            _addingWorkersMode = AddingWorkersMode.ToObservers;

            ShowWorkersView();
        }

        private void ShowWorkersView()
        {
            MainGrid.IsEnabled = false;
            ShadowGrid.Visibility = Visibility.Visible;

            var widthAnimation = new DoubleAnimation(250d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color {A = 20, R = 0, G = 0, B = 0};
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void CancelAddPerformersButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainGrid.IsEnabled = true;
            WorkersView.SelectedIndex = -1;

            var widthAnimation = new DoubleAnimation(0d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color {A = 0, R = 0, G = 0, B = 0};
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            colorAnimation.Completed += (s, args) => { ShadowGrid.Visibility = Visibility.Collapsed; };
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
            var filteredView = ((DataView) WorkersView.ItemsSource).Table.AsEnumerable().
                Where(r => r.Field<string>("Name").ToLower().Contains(searchText)).AsDataView();
            filteredView.Sort = "Name";
            WorkersView.ItemsSource = filteredView;
        }

        private void AddPerformersButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (WorkersView.SelectedItems.Count == 0) return;

            var workers = _addingWorkersMode == AddingWorkersMode.ToPerformers
                ? from DataRowView workerView in WorkersView.SelectedItems
                    select Convert.ToInt32(workerView["WorkerID"])
                    into workerId
                    where _performers.All(w => w.WorkerID != workerId)
                    let workerName = _staffClass.GetWorkerName(workerId, true)
                    select new Worker(workerId, workerName, null)
                : from DataRowView workerView in WorkersView.SelectedItems
                    select Convert.ToInt32(workerView["WorkerID"])
                    into workerId
                    where _observers.All(w => w.WorkerID != workerId)
                    let workerName = _staffClass.GetWorkerName(workerId, true)
                    select new Worker(workerId, workerName, null);

            foreach (var worker in workers)
            {
                var newWorker = worker;

                if (_addingWorkersMode == AddingWorkersMode.ToPerformers)
                {
                    _performers.Add(newWorker);
                    _newPerformers.Add(newWorker);
                }
                else
                {
                    _observers.Add(newWorker);
                    _newObservers.Add(newWorker);
                }

                var thread = new Thread(() =>
                {
                    var photo = GetBitmapImage((int) newWorker.WorkerID);
                    newWorker.Photo = photo;
                }) {IsBackground = true};
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            CancelAddPerformersButton_OnClick(null, null);
        }



        private BitmapImage GetBitmapImage(int workerId)
        {
            var photo = _staffClass.GetObjectPhotoFromDataBase(workerId);
            BitmapImage image;
            if (photo == DBNull.Value || photo == null)
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri("pack://application:,,,/Resources/user.png",
                    UriKind.Absolute);
                image.EndInit();
                image.Freeze();
            }
            else
            {
                image = AdministrationClass.ObjectToBitmapImage(photo);
            }

            return image;
        }

        private void AddNewTask_OnUnloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private void DeleteResponsible_OnClick(object sender, RoutedEventArgs e)
        {
            var deleteButton = (Button) sender;
            if (deleteButton.DataContext == null) return;

            var deletingPerformer = (Worker) deleteButton.DataContext;

            //Check if current worker start this task
            var rows =
                _taskClass.Performers.Table.AsEnumerable()
                    .Where(performerRow =>
                        Convert.ToInt32(performerRow["TaskID"]) == _taskId &&
                        Convert.ToInt32(performerRow["WorkerID"]) == deletingPerformer.WorkerID);
            if (rows.Any())
            {
                var performer = rows.First();
                if (performer["StartDate"] != DBNull.Value)
                {
                    if (
                        MessageBox.Show("Работник уже приступил к выполнению данной задачи.\nУдалить работника?",
                            "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Information) !=
                        MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }


            _performers.Remove(deletingPerformer);
            _newPerformers.Remove(deletingPerformer);

            if (_openingType == AddNewTaskOpeningType.Changing)
                _deletedPerformers.Add(deletingPerformer);
        }

        private void OnDeleteObserverButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteButton = (Button)sender;
            if (deleteButton.DataContext == null) return;

            var deletingObserver = (Worker)deleteButton.DataContext;

            _observers.Remove(deletingObserver);
            _newObservers.Remove(deletingObserver);

            if (_openingType == AddNewTaskOpeningType.Changing)
                _deletedObservers.Add(deletingObserver);
        }



        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TaskName.Text) || string.IsNullOrEmpty(TaskDescription.Text) ||
                (_performers.Count == 0 && _newPerformers.Count == 0)) return;

            if (IsDeadLineEnable.IsChecked.HasValue && IsDeadLineEnable.IsChecked.Value &&
                !DeadLineDate.SelectedDate.HasValue) return;

            switch (_openingType)
            {
                case AddNewTaskOpeningType.AddingEmpty:
                    Add();
                    break;
                case AddNewTaskOpeningType.AddingBasedOnValues:
                    Add();
                    break;
                case AddNewTaskOpeningType.Changing:
                    Change();
                    break;
            }

            CancelButton_OnClick(null, null);
        }

        private void Add()
        {
            var taskName = TaskName.Text;
            var taskDescription = TaskDescription.Text;
            var mainWorkerId = AdministrationClass.CurrentWorkerId;
            var creationDate = App.BaseClass.GetDateFromSqlServer();
            var taskId = -1;

            // Adding new task
            if (IsDeadLineEnable.IsChecked.HasValue && IsDeadLineEnable.IsChecked.Value)
            {
                if (DeadLineDate.SelectedDate.HasValue)
                {
                    var deadLineDate = DeadLineDate.SelectedDate.Value;
                    taskId = _openingType == AddNewTaskOpeningType.AddingEmpty
                        ? _taskClass.AddNewTask(taskName, mainWorkerId, creationDate, taskDescription,
                            _senderApplication, true,
                            deadLineDate)
                        : _taskClass.AddNewTask(_globalId, taskName, mainWorkerId, creationDate, taskDescription,
                            _senderApplication, true, deadLineDate);
                }
            }
            else
            {
                taskId = _openingType == AddNewTaskOpeningType.AddingEmpty
                    ? _taskClass.AddNewTask(taskName, mainWorkerId, creationDate, taskDescription,
                        TaskClass.SenderApplications.Tasks)
                    : _taskClass.AddNewTask(_globalId, taskName, mainWorkerId, creationDate, taskDescription,
                        _senderApplication);
            }

            // Adding performers and observers for task
            if (taskId == -1) return;

            foreach (var workerId in _newPerformers.Select(newPerformer => newPerformer.WorkerID))
            {
                _taskClass.AddNewPerformer(taskId, (int) workerId);
                NotificationManager.AddNotification((int) workerId,
                    AdministrationClass.Modules.TasksPage, taskId);
            }

            foreach (var workerId in _newObservers.Select(newObserver => newObserver.WorkerID))
            {
                _taskClass.AddNewObserver(taskId, (int) workerId);
            }

            var rows = _taskClass.Tasks.Table.Select(string.Format("TaskID = {0}", taskId));
            if (rows.Any())
            {
                var task = rows.First();
                var globalId = task["GlobalID"].ToString();

                var newsText = string.Format("Задача: {0}\nПостановщик задачи: {1}\nОписание задачи: {2}", taskName,
                    new IdToNameConverter().Convert(mainWorkerId, "ShortName"), taskDescription);
                _newsFeedClass.AddNews(newsText, creationDate, 11, mainWorkerId, globalId, null);
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var taskPage = mainWindow.MainFrame.Content as XamlFiles.TaskPage;
                if (taskPage != null)
                {
                    var taskViewModel = taskPage.DataContext as TasksViewModel;
                    if (taskViewModel != null)
                    {
                        // Select new task if TaskPage is activated
                        taskViewModel.SelectTask(taskId);
                        return;
                    }
                }

                var servEquipPage = mainWindow.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    // Receive request, if ServiceEquipmentPage is activated
                    servEquipPage.ReceiveRequest(taskDescription);
                    servEquipPage.RefillInfo();
                    return;
                }

                var techProblemPage = mainWindow.MainFrame.Content as XamlFiles.TechnologyProblemPage;
                if (techProblemPage != null)
                {
                    // Receive request, if TechnologyProblemPage is activated
                    techProblemPage.ReceiveRequest(_globalId, taskDescription);
                    techProblemPage.RefillInfo();
                }

                var plannedWorksPage = mainWindow.MainFrame.Content as XamlFiles.PlannedWorksPage;
                if(plannedWorksPage != null)
                {
                    plannedWorksPage.StartPlannedWorks(taskId, PlannedWorksClass.SelectedEmptyWorkReasonId);
                    plannedWorksPage.RefillInfo();
                }
            }
        }

        private void Change()
        {
            var taskName = TaskName.Text;
            var taskDescription = TaskDescription.Text;

            if (IsDeadLineEnable.IsChecked.HasValue && IsDeadLineEnable.IsChecked.Value)
            {
                if (DeadLineDate.SelectedDate.HasValue)
                {
                    var deadLineDate = DeadLineDate.SelectedDate.Value;
                    _taskClass.Tasks.Change(_taskId, taskName, taskDescription, true, deadLineDate);
                }
            }
            else
            {
                _taskClass.Tasks.Change(_taskId, taskName, taskDescription);
            }

            // Add new performers
            foreach (var workerId in _newPerformers.Select(newPerformer => newPerformer.WorkerID))
            {
                _taskClass.AddNewPerformer(_taskId, (int) workerId);
                NotificationManager.AddNotification((int) workerId,
                    AdministrationClass.Modules.TasksPage, _taskId);
            }

            // Add new observers
            foreach (var workerId in _newObservers.Select(newObserver => newObserver.WorkerID))
            {
                _taskClass.AddNewObserver(_taskId, (int) workerId);
            }

            // Delete performers
            foreach (var performerId in from deletedPerformer in _deletedPerformers
                select deletedPerformer.WorkerID
                into workerId
                select _taskClass.Performers.Table.Select("TaskID = " + _taskId + " AND WorkerID = " + workerId)
                into performers
                where performers.Any()
                select performers.First()
                into performer
                select Convert.ToInt32(performer["PerformerID"]))
            {
                _taskClass.DeletePerformer(performerId);
            }

            // Delete observers
            foreach (var observerId in from deletedObserver in _deletedObservers
                select deletedObserver.WorkerID
                into workerId
                select _taskClass.Observers.Table.Select("TaskID = " + _taskId + " AND WorkerID = " + workerId)
                into obserbers
                where obserbers.Any()
                select obserbers.First()
                into observer
                select Convert.ToInt32(observer["ObserverID"]))
            {
                _taskClass.DeleteObserver(observerId);
            }

            // Set task as completed if all workers allready has finished task
            if (_taskClass.Performers.Table.AsEnumerable().Where(p => p.Field<Int64>("TaskID") == _taskId)
                .All(p => p.Field<bool>("IsComplete")))
            {
                var currentDate = App.BaseClass.GetDateFromSqlServer();
                _taskClass.Tasks.EndTask(_taskId, currentDate);

                var tasks = _taskClass.Tasks.Table.AsEnumerable().Where(r => r.Field<Int64>("TaskID") == _taskId);
                if (tasks.Any())
                {
                    var task = tasks.First();
                    var senderAppId = Convert.ToInt32(task["SenderAppID"]);
                    var mainWorkerId = Convert.ToInt32(task["MainWorkerID"]);
                    var globalId = task["GlobalID"].ToString();
                    switch ((TaskClass.SenderApplications)senderAppId)
                    {
                        case TaskClass.SenderApplications.ServiceDamage:
                            {
                                ServiceEquipmentClass servEquipClass = null;
                                App.BaseClass.GetServiceEquipmentClass(ref servEquipClass);

                                if (servEquipClass != null)
                                {
                                    servEquipClass.FillCompletionInfo(globalId, currentDate, mainWorkerId, null);
                                    AdministrationClass.AddNewAction(10);
                                }

                                break;
                            }
                        case TaskClass.SenderApplications.TechnologyProblem:
                            {
                                TechnologyProblemClass techProblemClass = null;
                                App.BaseClass.GetTechnologyProblemClass(ref techProblemClass);
                                if (techProblemClass != null)
                                {
                                    techProblemClass.FillCompletionInfo(globalId, currentDate, mainWorkerId);
                                }
                                break;
                            }
                        case TaskClass.SenderApplications.PlannedWorks:
                            {
                                PlannedWorksClass plannedWorksClass = null;
                                App.BaseClass.GetPlannedWorksClass(ref plannedWorksClass);
                                if(plannedWorksClass != null)
                                {
                                    plannedWorksClass.FinishPlannedWorks(_taskId);
                                }
                                break;
                            }
                    }
                }
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var taskPage = mainWindow.MainFrame.Content as XamlFiles.TaskPage;
                if (taskPage != null)
                {
                    var taskViewModel = taskPage.DataContext as TasksViewModel;
                    if (taskViewModel != null)
                    {
                        taskViewModel.UpdateTaskView();
                        taskViewModel.SelectTask(_taskId);
                    }
                }

                var servEquipPage = mainWindow.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    servEquipPage.RefillInfo();
                }

                var techProblemPage = mainWindow.MainFrame.Content as XamlFiles.TechnologyProblemPage;
                if (techProblemPage != null)
                {

                    techProblemPage.ChangeReceivedNotes(taskDescription);
                    techProblemPage.RefillInfo();
                }
            }
        }
    }

    internal enum AddNewTaskOpeningType
    {
        AddingEmpty, AddingBasedOnValues, Changing
    }

    internal enum AddingWorkersMode
    {
        ToPerformers,
        ToObservers
    }
}
