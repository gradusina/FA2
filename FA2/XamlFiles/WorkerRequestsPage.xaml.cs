using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using FA2.Classes;
using FA2.Notifications;
using FA2.ChildPages.WorkerRequestsPage;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для WorkerRequestsPage.xaml
    /// </summary>
    public partial class WorkerRequestsPage : Page
    {
        private readonly bool _fullAccess;
        private bool _firstTimePageRun = true;

        private DateTime _dateFrom;
        private DateTime _dateTo;

        private WorkerRequestsClass _workerRequestsClass;




        public WorkerRequestsPage(bool fullAccess)
        {
            _fullAccess = fullAccess;
            InitializeComponent();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.WorkerRequests);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.WorkerRequests);

            if (_firstTimePageRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) =>
                {
                    _firstTimePageRun = false;
                    FillData();
                };
                backgroundWorker.RunWorkerCompleted += (o, args) =>
                {
                    BindingData();
                    SetDateProperties();
                    SetEnables();

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideWaitAnnimation();
                };

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillData()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _dateTo = currentDate;
            _dateFrom = currentDate != DateTime.MinValue
                ? currentDate.Month > 2
                    ? new DateTime(currentDate.Year, currentDate.Month - 2, 1)
                    : new DateTime(currentDate.Year - 1, 12 + currentDate.Month - 2, 1)
                : DateTime.MinValue;

            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestsClass);

            _workerRequestsClass.FillWorkerRequests(_dateFrom, _dateTo);
        }

        private void BindingData()
        {
            WorkerRequestsDataGrid.ItemsSource = _workerRequestsClass.WorkerRequestsTable.AsDataView();
        }

        private void SetDateProperties()
        {
            DateFromPicker.SelectedDate = _dateFrom;
            DateToPicker.SelectedDate = _dateTo;
        }

        private void SetEnables()
        {
            if (!_fullAccess)
            {
                ShowPersonalRequestsCheckBox.IsChecked = true;
                ShowPersonalRequestsCheckBox.Visibility = Visibility.Collapsed;

                AddRequestForWorkerComboBoxItem.IsEnabled = false;
            }
        }



        private void OnShowPersonalRequestsCheckBoxCheckStateChanged(object sender, RoutedEventArgs e)
        {
            var workerRequestsView = WorkerRequestsDataGrid.ItemsSource as DataView;
            if (workerRequestsView == null) return;

            workerRequestsView.RowFilter = ShowPersonalRequestsCheckBox.IsChecked.HasValue
                ? ShowPersonalRequestsCheckBox.IsChecked.Value
                    ? string.Format("WorkerID = {0} OR MainWorkerID = {0}", AdministrationClass.CurrentWorkerId)
                    : string.Empty
                : string.Empty;
        }

        private void OnFillWorkerRequestsButonClick(object sender, RoutedEventArgs e)
        {
            if (DateFromPicker.SelectedDate == null || DateToPicker.SelectedDate == null) return;

            _dateFrom = DateFromPicker.SelectedDate.Value;
            _dateTo = DateToPicker.SelectedDate.Value;

            _workerRequestsClass.FillWorkerRequests(_dateFrom, _dateTo);
        }

        private void OnWorkerRequestsDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;

            SetButtonsEnable(workerRequest);
        }

        private void SetButtonsEnable(DataRowView workerRequest)
        {
            ConfirmRequestButton.Visibility = Visibility.Collapsed;
            DontConfirmRequestButton.Visibility = Visibility.Collapsed;
            DeleteWorkerRequestButton.Visibility = Visibility.Collapsed;
            ExportToWordButton.IsEnabled = false;

            if (workerRequest == null) return;


            if (workerRequest["IsConfirmed"] == DBNull.Value)
            {
                ExportToWordButton.IsEnabled = false;

                var mainWorkerId = Convert.ToInt64(workerRequest["MainWorkerID"]);
                var requestCreatedWorkerId = Convert.ToInt64(workerRequest["RequestCreatedWorkerID"]);
                var currentWorkerId = AdministrationClass.CurrentWorkerId;
                var workerId = Convert.ToInt64(workerRequest["WorkerID"]);

                if (mainWorkerId == currentWorkerId)
                {
                    ConfirmRequestButton.Visibility = Visibility.Visible;
                    DontConfirmRequestButton.Visibility = Visibility.Visible;
                }

                if (workerId == currentWorkerId || requestCreatedWorkerId == currentWorkerId)
                {
                    DeleteWorkerRequestButton.Visibility = Visibility.Visible;
                }
            }
            else if (workerRequest["IsConfirmed"] != null && Convert.ToBoolean(workerRequest["IsConfirmed"]))
            {
                ExportToWordButton.IsEnabled = true;
            }

            if (AdministrationClass.IsAdministrator)
            {
                DeleteWorkerRequestButton.Visibility = Visibility.Visible;
            }
        }

        private void OnDeleteWorkerRequestButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;
            if (workerRequest == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную заявку?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var workerRequestId = Convert.ToInt64(workerRequest["WorkerRequestID"]);
            var workerId = Convert.ToInt64(workerRequest["WorkerID"]);
            var creationDate = Convert.ToDateTime(workerRequest["CreationDate"]);

            _workerRequestsClass.DeleteWorkerRequest(workerRequestId);
            AdministrationClass.AddNewAction(81);
            NewsHelper.DeleteNews(creationDate, (int)workerId);
        }

        private void OnConfirmRequestButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;
            if (workerRequest == null) return;

            if (workerRequest["IsConfirmed"] != DBNull.Value) return;

            var mainWorkerId = Convert.ToInt64(workerRequest["MainWorkerID"]);
            if (mainWorkerId != AdministrationClass.CurrentWorkerId) return;

            var setWorkerRequestConfirmInfoPage = 
                new SetWorkerRequestConfirmationInfoPage(workerRequest, WorkerRequestConfirmMode.Confirm);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(setWorkerRequestConfirmInfoPage, "Подтвердить заявку");
            }
        }

        private void OnDontConfirmRequestButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;
            if (workerRequest == null) return;

            if (workerRequest["IsConfirmed"] != DBNull.Value) return;

            var mainWorkerId = Convert.ToInt64(workerRequest["MainWorkerID"]);
            if (mainWorkerId != AdministrationClass.CurrentWorkerId) return;

            var setWorkerRequestConfirmInfoPage =
                new SetWorkerRequestConfirmationInfoPage(workerRequest, WorkerRequestConfirmMode.DontConfirm);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(setWorkerRequestConfirmInfoPage, "Отклонить заявку");
            }
        }

        public void RefreshVisualState()
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;
            SetButtonsEnable(workerRequest);
        }




        private void OnExportToWordButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequest = WorkerRequestsDataGrid.SelectedItem as DataRowView;
            if (workerRequest == null) return;

            if (workerRequest["IsConfirmed"] == null || !Convert.ToBoolean(workerRequest["IsConfirmed"]))
                return;

            var workerRequestReport = new WorkerRequestToWordReportPage(workerRequest);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(workerRequestReport, "Сформировать заявление");
            }
        }

        private void OnExportToExcelButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequestsView = WorkerRequestsDataGrid.ItemsSource as DataView;
            if (workerRequestsView == null) return;

            ExportToExcel.GenerateWorkerRequestsReport(workerRequestsView);
        }



        private void OnAddRequestForWorkerComboBoxItemPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var addNewRequestPage = new AddNewWorkerRequestPage(null, AdministrationClass.CurrentWorkerId);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewRequestPage, "Добавить новую заявку");
            }
        }

        private void OnAddNewRequestButtonClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var addNewRequestPage = new AddNewWorkerRequestPage(AdministrationClass.CurrentWorkerId);
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewRequestPage, "Добавить новую заявку");
            }
        }
    }
}
