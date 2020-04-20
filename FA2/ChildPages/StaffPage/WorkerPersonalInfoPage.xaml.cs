using FA2.Classes;
using FA2.Converters;
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

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для WorkerPersonalInfoPage.xaml
    /// </summary>
    public partial class WorkerPersonalInfoPage : Page
    {
        private StaffClass _sc;
        private AdmissionsClass _admClass;
        private MyWorkersClass _mwc;

        private long _workerId;
        private bool _fullAccess;

        public WorkerPersonalInfoPage(long workerId, bool fullAccess)
        {
            InitializeComponent();

            _workerId = workerId;
            _fullAccess = fullAccess;

            if (!_fullAccess)
                ContactsTabItem.Visibility = Visibility.Collapsed;

            GetData();
            SetBindings();
        }

        private void GetData()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetAdmissionsClass(ref _admClass);
            App.BaseClass.GetMyWorkersClass(ref _mwc);
        }

        private void SetBindings()
        {
            BindingWorkerPhoto();

            BindingWorkerName();

            BindingWorkerProfessions();

            if (_fullAccess)
                BindingWorkerContacts();

            BindingWorkerProdStatuses();

            BindingWorkerAdmissions();

            BindingWorkerMainWorkers();
        }

        private void BindingWorkerPhoto()
        {
            StaffPhotoImage.Source =
                AdministrationClass.ObjectToBitmapImage(_sc.GetObjectPhotoFromDataBase(_workerId));
        }

        private void BindingWorkerName()
        {
            WorkerNameTextBlock.Text = new IdToNameConverter().Convert(_workerId, "FullName");
        }

        private void BindingWorkerProfessions()
        {
            var workerProfessionsView = new BindingListCollectionView(_sc.GetWorkerProfessions());
            workerProfessionsView.CustomFilter = string.Format("WorkerID = {0}", _workerId);
            workerProfessionsView.GroupDescriptions.Add(new PropertyGroupDescription("FactoryID"));
            MainWorkersProfessionsDataGrid.ItemsSource = workerProfessionsView;

            var additionalWorkerProfessionsView = _sc.GetAdditionalWorkerProfessions();
            additionalWorkerProfessionsView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            AdditionalWorkersProfessionsDataGrid.ItemsSource = additionalWorkerProfessionsView;
        }

        private void BindingWorkerContacts()
        {
            var adressesView = _sc.GetStaffAdresses();
            adressesView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            StaffAdressesDataGrid.ItemsSource = adressesView;

            var contactsView = _sc.GetStaffContacts();
            contactsView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            StaffContactsItemsControl.ItemsSource = contactsView;
        }

        private void BindingWorkerProdStatuses()
        {
            var workerProdStatusView = _sc.GetWorkerProdStatuses();
            workerProdStatusView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            StaffSkillsItemsControl.ItemsSource = workerProdStatusView;
        }

        private void BindingWorkerAdmissions()
        {
            var workerAdmissions = _admClass.WorkerAdmissionsTable.AsDataView();
            var workerAdmissionsView = new BindingListCollectionView(workerAdmissions);
            if (workerAdmissionsView.GroupDescriptions != null)
                workerAdmissionsView.GroupDescriptions.Add(new PropertyGroupDescription("AdmissionID"));
            workerAdmissionsView.CustomFilter = string.Format("WorkerID = {0}", _workerId);
            WorkerAdmissionsItemsControl.ItemsSource = workerAdmissionsView;
        }

        private void BindingWorkerMainWorkers()
        {
            var mainWorkers = _mwc.GetMyWorkers();
            mainWorkers.RowFilter = string.Format("WorkerID = {0}", _workerId);
            MainWorkersListBox.ItemsSource = mainWorkers;
        }

        public void DoCheckRow(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing)
            {
                var row = UIHelper.FindVisualParent<DataGridRow>(cell);
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;
                    e.Handled = true;
                }
            }
        }
    }
}
