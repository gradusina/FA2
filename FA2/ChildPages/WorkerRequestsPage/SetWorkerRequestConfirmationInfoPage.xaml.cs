using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FA2.XamlFiles;
using FA2.Classes;
using FA2.Converters;
using System.Data;

namespace FA2.ChildPages.WorkerRequestsPage
{
    public enum WorkerRequestConfirmMode
    {
        Confirm,
        DontConfirm
    }

    /// <summary>
    /// Логика взаимодействия для SetWorkerRequestConfirmationInfoPage.xaml
    /// </summary>
    public partial class SetWorkerRequestConfirmationInfoPage : Page
    {
        private DataRowView _workerRequest;
        private WorkerRequestsClass _workerRequestsClass;



        public SetWorkerRequestConfirmationInfoPage(DataRowView workerRequest, WorkerRequestConfirmMode mode)
        {
            InitializeComponent();

            _workerRequest = workerRequest;
            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestsClass);

            if(mode == WorkerRequestConfirmMode.Confirm)
            {
                ConfirmWorkerRequestButton.Visibility = Visibility.Visible;
                DontConfirmWorkerRequestButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ConfirmWorkerRequestButton.Visibility = Visibility.Collapsed;
                DontConfirmWorkerRequestButton.Visibility = Visibility.Visible;
            }
        }

        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();

                var workerRequestsPage = mainWindow.MainFrame.Content as XamlFiles.WorkerRequestsPage;
                if (workerRequestsPage != null)
                {
                    workerRequestsPage.RefreshVisualState();
                }
            }
        }



        private void OnConfirmWorkerRequestButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequestId = Convert.ToInt64(_workerRequest["WorkerRequestID"]);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var mainWorkerNotes = !string.IsNullOrEmpty(MainWorkerNotesTextBox.Text)
                ? MainWorkerNotesTextBox.Text
                : null;
            _workerRequestsClass.SetConfirmationInfo(workerRequestId, mainWorkerNotes, true, currentDate);
            AdministrationClass.AddNewAction(82);

            var creationDate = Convert.ToDateTime(_workerRequest["CreationDate"]);
            var workerId = Convert.ToInt64(_workerRequest["WorkerID"]);

            var newsText = string.Format("\n\nЗаявка принята: {0} дата: {1} \nПримечание: {2}", 
                new IdToNameConverter().Convert(AdministrationClass.CurrentWorkerId, "ShortName"), 
                currentDate,
                string.IsNullOrEmpty(mainWorkerNotes) ? "отсутствует" : mainWorkerNotes);

            NewsHelper.AddTextToNews(creationDate, (int)workerId, newsText);

            OnClosePageButtonClick(null, null);
        }

        private void OnDontConfirmWorkerRequestButtonClick(object sender, RoutedEventArgs e)
        {
            var workerRequestId = Convert.ToInt64(_workerRequest["WorkerRequestID"]);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var mainWorkerNotes = !string.IsNullOrEmpty(MainWorkerNotesTextBox.Text)
                ? MainWorkerNotesTextBox.Text
                : null;
            _workerRequestsClass.SetConfirmationInfo(workerRequestId, mainWorkerNotes, false, currentDate);
            AdministrationClass.AddNewAction(83);

            var creationDate = Convert.ToDateTime(_workerRequest["CreationDate"]);
            var workerId = Convert.ToInt64(_workerRequest["WorkerID"]);

            var newsText = string.Format("\n\nЗаявка откланена: {0} дата: {1} \nПримечание: {2}",
                new IdToNameConverter().Convert(AdministrationClass.CurrentWorkerId, "ShortName"),
                currentDate,
                string.IsNullOrEmpty(mainWorkerNotes) ? "отсутствует" : mainWorkerNotes);

            NewsHelper.AddTextToNews(creationDate, (int)workerId, newsText);

            OnClosePageButtonClick(null, null);
        }
    }
}
