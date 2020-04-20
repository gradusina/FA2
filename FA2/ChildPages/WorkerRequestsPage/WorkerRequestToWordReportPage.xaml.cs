using FA2.XamlFiles;
using System;
using System.Collections.Generic;
using System.Data;
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
using FA2.Classes;
using FA2.Classes.WorkerRequestsEnums;

namespace FA2.ChildPages.WorkerRequestsPage
{
    /// <summary>
    /// Логика взаимодействия для WorkerRequestToWordReportPage.xaml
    /// </summary>
    public partial class WorkerRequestToWordReportPage : Page
    {
        private DataRowView _workerRequest;
        private StaffClass _staffClass;
        private CatalogClass _catalogClass;

        public WorkerRequestToWordReportPage(DataRowView workerRequest)
        {
            InitializeComponent();

            _workerRequest = workerRequest;
            DataContext = workerRequest;

            FillData();
            BindingData();
        }

        private void FillData()
        {
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetCatalogClass(ref _catalogClass);
        }

        private void BindingData()
        {
            if (_workerRequest == null) return;

            var workerId = Convert.ToInt32(_workerRequest["WorkerID"]);
            var mainWorkerId = Convert.ToInt32(_workerRequest["MainWorkerID"]);

            var workerProfessions = _staffClass.WorkerProfessionsDataTable.Select(string.Format("WorkerID = {0}", workerId));
            WorkerProfessionsListBox.ItemsSource = workerProfessions.Any()
                ? workerProfessions.CopyToDataTable().AsDataView()
                : null;

            var mainWorkerProfessions = _staffClass.WorkerProfessionsDataTable.Select(string.Format("WorkerID = {0}", mainWorkerId));
            MainWorkerProfessionsListBox.ItemsSource = mainWorkerProfessions.Any()
                ? mainWorkerProfessions.CopyToDataTable().AsDataView()
                : null;

            FactoryComboBox.ItemsSource = _catalogClass.GetFactories();
            if (FactoryComboBox.HasItems)
                FactoryComboBox.SelectedIndex = 0;
        }



        private void OnFactoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int factoryId = -1;
            if (FactoryComboBox.SelectedItem != null)
            {
                factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            }

            if (WorkerProfessionsListBox.ItemsSource != null)
                ((DataView)WorkerProfessionsListBox.ItemsSource).RowFilter = string.Format("FactoryID = {0}", factoryId);
            if (WorkerProfessionsListBox.HasItems)
                WorkerProfessionsListBox.SelectedIndex = 0;

            if (MainWorkerProfessionsListBox.ItemsSource != null)
                ((DataView)MainWorkerProfessionsListBox.ItemsSource).RowFilter = string.Format("FactoryID = {0}", factoryId);
            if (MainWorkerProfessionsListBox.HasItems)
                MainWorkerProfessionsListBox.SelectedIndex = 0;
        }

        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnExportReportButtonClick(object sender, RoutedEventArgs e)
        {
            if (_workerRequest == null) return;

            if (FactoryComboBox.SelectedItem == null || WorkerProfessionsListBox.SelectedItem == null ||
                MainWorkerProfessionsListBox.SelectedItem == null)
                return;

            var factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            var directoryName = factoryId == 1 ? "Авдей Ф.А." : factoryId == 2 ? "Авдей М.А." : string.Empty;
            var reportStruct = new WorkerRequestMicrosoftWordReportStruct()
            {
                FactoryId = factoryId,
                DirectoryName = directoryName,
                ProfessionId = Convert.ToInt64(WorkerProfessionsListBox.SelectedValue),
                WorkerId = Convert.ToInt32(_workerRequest["WorkerID"]),
                SalarySaveType = (SalarySaveType)_workerRequest["SalarySaveTypeID"],
                IntervalType = (IntervalType)_workerRequest["IntervalTypeID"],
                RequestDate = Convert.ToDateTime(_workerRequest["RequestDate"]),
                RequestFinishDate = Convert.ToDateTime(_workerRequest["RequestFinishDate"]),
                WorkingOffType = (WorkingOffType)_workerRequest["WorkingOffTypeID"],
                WorkerNotes = _workerRequest["RequestNotes"].ToString(),
                MainWorkerProfessionId = Convert.ToInt64(MainWorkerProfessionsListBox.SelectedValue),
                MainWorkerId = Convert.ToInt32(_workerRequest["MainWorkerID"])
            };

            ExportToWord.CreateWorkerRequestReport(reportStruct);
            AdministrationClass.AddNewAction(84);
        }
    }
}
