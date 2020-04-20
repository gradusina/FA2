using System.ComponentModel;
using System.Windows;
using FA2.Classes;
using FA2.Notifications;
using FA2.ViewModels;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для TaskPage.xaml
    /// </summary>
    public partial class TaskPage
    {
        private TasksViewModel _viewModel;
        private readonly bool _fullAccess;

        private bool _firstRun = true;

        public TaskPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
        }

        private void TaskPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.TasksPage);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.TasksPage);

            if (_firstRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) => _viewModel = new TasksViewModel(_fullAccess);

                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           DataContext = _viewModel;

                                                           var mainWindow = Application.Current.MainWindow as MainWindow;
                                                           if (mainWindow != null) mainWindow.HideWaitAnnimation();
                                                       };

                backgroundWorker.RunWorkerAsync();

                _firstRun = false;
            }
            else
            {
                _viewModel.FillTasksData();
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }
    }
}
