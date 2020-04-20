using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FA2.ChildPages.StaffPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FA2.Notifications;
using FAIIControlLibrary;
using Microsoft.Win32;
using FA2.ChildPages.AdmissionPage;
using System.Windows.Data;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для StaffPage.xaml
    /// </summary>
    public partial class StaffPage
    {
        private StaffClass _sc;
        private StimulationClass _stc;
        private MyWorkersClass _mwc;
        private AdmissionsClass _admClass;

        private bool _firstRun = true;
        private MainWindow _mw;
        private bool _fullAccess;

        private DataView _staffPersonalInfoCollection;
        private DataView _workerProfessionsCollection;

        //readonly SpnxClass _spnxc = new SpnxClass();

        //private DirectoryInfo _currentWorkerDirecory;


        private FtpClient _ftpClient;
        private string _basicDirectory;
        private WaitWindow _processWindow;
        private long _fileSize;
        private string _neededOpeningFilePath;

        public StaffPage(bool fullAccess)
        {
            _fullAccess = fullAccess;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.Workers);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.Workers);

            if (_firstRun)
            {
                //LayoutUpdated += StaffPage_LayoutUpdated;
                _basicDirectory = App.GetFtpUrl + @"FtpFaII/WorkerFiles/";

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (o, args) =>
                    GetClasses();

                backgroundWorker.RunWorkerCompleted += (o, args) =>
                {
                    SetBindings();
                    SetEnables(_fullAccess);

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideWaitAnnimation();

                    _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
                    _ftpClient.UploadProgressChanged += OnUploadProgressChanged;
                    _ftpClient.DownloadProgressChanged += OnDownloadProgressChanged;
                    if (!_ftpClient.DirectoryExist(_basicDirectory))
                        _ftpClient.MakeDirectory(_basicDirectory);
                };

                backgroundWorker.RunWorkerAsync();

                _firstRun = false;
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void GetClasses()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetStimulationClass(ref _stc);
            App.BaseClass.GetMyWorkersClass(ref _mwc);
            App.BaseClass.GetAdmissionsClass(ref _admClass);
        }

        private void SetBindings()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var dateFrom = currentDate.AddYears(-1);
            var dateTo = currentDate.AddDays(1);
            _stc.FillWorkersStim(dateFrom, dateTo);
            PromotionsDateFrom.SelectedDate = dateFrom;
            PromotionsDateTo.SelectedDate = dateTo;


            BindingPromotions();

            WorkerFilesList.ItemsSource = _sc.WorkerFilesView;

            WorkerGroupsFilterComboBox.ItemsSource = _sc.GetWorkerGroups();

            FactoriesFilterComboBox.ItemsSource = _sc.GetFactories();

            WorkerStatusesFilterComboBox.ItemsSource = _sc.GetWorkerStatuses();

            _staffPersonalInfoCollection = _sc.GetStaffPersonalInfo();

            StaffListBox.ItemsSource = _staffPersonalInfoCollection;

            StaffStatusComboBox.ItemsSource = _sc.GetWorkerStatuses();

            _workerProfessionsCollection = _sc.GetWorkerProfessions();

            StaffSummProfessionsListBox.ItemsSource = _workerProfessionsCollection;

            MainWorkersProfessionsDataGrid.ItemsSource = _workerProfessionsCollection;

            AdditionalWorkersProfessionsDataGrid.ItemsSource = _sc.GetAdditionalWorkerProfessions();

            StaffAdressesDataGrid.ItemsSource = _sc.GetStaffAdresses();

            StaffEducatioDataGrid.ItemsSource = _sc.GetStaffEducation();

            StaffContactsItemsControl.ItemsSource = _sc.GetStaffContacts();

            StaffSkillsItemsControl.ItemsSource = _sc.GetWorkerProdStatuses();

            var workerAdmissions = _admClass.WorkerAdmissionsTable.AsDataView();
            var workerAdmissionsView = new BindingListCollectionView(workerAdmissions);
            if (workerAdmissionsView.GroupDescriptions != null)
                workerAdmissionsView.GroupDescriptions.Add(new PropertyGroupDescription("AdmissionID"));
            WorkerAdmissionsItemsControl.ItemsSource = workerAdmissionsView;

            MartialStatusComboBox.ItemsSource = _sc.GetMartialStatuses();

            MainWorkersListBox.ItemsSource = _mwc.GetMyWorkers();


            AvailableInListCheckBox.IsChecked = true;

            StaffListBox.SelectedIndex = 0;

            StaffCountLabel.Content = StaffListBox.Items.Count;
        }

        private void BindingPromotions()
        {
            PromotionDataGrid.ItemsSource = _stc.WorkerStimView(1);
            if (PromotionDataGrid.ItemsSource != null)
                ((DataView)PromotionDataGrid.ItemsSource).Sort = "Date";
            FineDataGrid.ItemsSource = _stc.WorkerStimView(2);
            if (FineDataGrid.ItemsSource != null)
                ((DataView)FineDataGrid.ItemsSource).Sort = "Date";
        }

        private void SetEnables(bool isEnabled)
        {
            var visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;

            StaffPersonalInfoExpander.Visibility = visibility;
            StaffPassportInfo.Visibility = visibility;
            StaffProfessionsExpander.Visibility = visibility;
            StaffBirthTextBlock.Visibility = visibility;

            StaffStatusComboBox.IsEnabled = isEnabled;
            SaveStaffPersonalInfo.Visibility = visibility;

            EditStaffContactButton.Visibility = visibility;
            StaffAdressesGrid.Visibility = visibility;

            EditStaffProfessionsButton.Visibility = visibility;

            StaffEducationExpander.Visibility = visibility;
            EditStaffEducationButton.Visibility = visibility;

            //EditStaffProdStatusButton.Visibility = visibility;

            AddFilesToWorkerButton.Visibility = visibility;
            DeleteFilesFromWorkerButton.Visibility = visibility;

            AdditionalMenuToggleButton.Visibility = visibility;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchAndFilterWorkers();
        }

        private void StaffListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StaffListBox.Items.Count == 0) return;
            if (StaffListBox.SelectedValue == null && StaffListBox.SelectedItem == null) return;

            int currentWorkerId = Convert.ToInt32(StaffListBox.SelectedValue);

            var currentStaffInfoRow = (DataRowView)StaffListBox.SelectedItem;

            MainStaffGrid.DataContext = currentStaffInfoRow;


            StaffPhotoImage.Source =
                AdministrationClass.ObjectToBitmapImage(_sc.GetObjectPhotoFromDataBase(currentWorkerId));

            _workerProfessionsCollection.RowFilter = "WorkerID =" + currentWorkerId;
            ((DataView)AdditionalWorkersProfessionsDataGrid.ItemsSource).RowFilter = "WorkerID =" + currentWorkerId;
            ((DataView)StaffAdressesDataGrid.ItemsSource).RowFilter = "WorkerID =" + currentWorkerId;
            ((DataView)StaffEducatioDataGrid.ItemsSource).RowFilter = "WorkerID =" + currentWorkerId;
            ((DataView)StaffContactsItemsControl.ItemsSource).RowFilter = "WorkerID =" + currentWorkerId;
            ((DataView)StaffSkillsItemsControl.ItemsSource).RowFilter = "WorkerID =" + currentWorkerId;
            ((BindingListCollectionView)WorkerAdmissionsItemsControl.ItemsSource).CustomFilter = "WorkerID = " + currentWorkerId;

            PassportNumberTextBlock.Text = currentStaffInfoRow["PassportNumber"].ToString();

            if (currentStaffInfoRow["PassportIssueDate"] != DBNull.Value)
                PassportIssueDateDatePicker.SelectedDate =
                    Convert.ToDateTime(currentStaffInfoRow["PassportIssueDate"].ToString());

            PassportAuthorityIssuingTextBox.Text = currentStaffInfoRow["PassportAuthorityIssuing"].ToString();

            if (WorkerFilesList.ItemsSource != null)
            {
                ((DataView)WorkerFilesList.ItemsSource).RowFilter = "WorkerID = " + currentWorkerId;
            }

            FilterPromotions();

            ((DataView)MainWorkersListBox.ItemsSource).RowFilter = "WorkerID= " + currentWorkerId;
        }

        private void FilterPromotions()
        {
            string defaultFilter;
            if (StaffListBox.SelectedItem == null)
            {
                defaultFilter = "Available = 'TRUE' AND WorkerID = -1";
            }
            else
            {
                var workerId = Convert.ToInt32(StaffListBox.SelectedValue);
                defaultFilter = string.Format("Available = 'TRUE' AND WorkerID = {0}", workerId);
            }

            if (PromotionDataGrid.ItemsSource != null)
                ((DataView)PromotionDataGrid.ItemsSource).RowFilter = defaultFilter;
            if (FineDataGrid.ItemsSource != null)
                ((DataView)FineDataGrid.ItemsSource).RowFilter = defaultFilter;

            CalculateTotalStimulationSize();
        }

        public void CalculateTotalStimulationSize()
        {
            double promotionMoney = 0;
            double promotionHours = 0;
            double fineMoney = 0;
            double fineHours = 0;

            if (PromotionDataGrid.ItemsSource != null)
                foreach (DataRowView drv in (DataView)PromotionDataGrid.ItemsSource)
                {
                    if (drv.Row["StimulationUnitID"] != DBNull.Value)
                    {
                        if (Convert.ToInt32(drv.Row["StimulationUnitID"]) == 1)
                            promotionHours += Convert.ToDouble(drv.Row["StimulationSize"]);
                        else if (Convert.ToInt32(drv.Row["StimulationUnitID"]) == 2)
                            promotionMoney += Convert.ToDouble(drv.Row["StimulationSize"]);
                    }
                }
            if (FineDataGrid.ItemsSource != null)
                foreach (DataRowView drv in (DataView)FineDataGrid.ItemsSource)
                {
                    if (drv.Row["StimulationUnitID"] != DBNull.Value)
                    {
                        if (Convert.ToInt32(drv.Row["StimulationUnitID"]) == 1)
                            fineHours += Convert.ToDouble(drv.Row["StimulationSize"]);
                        else if (Convert.ToInt32(drv.Row["StimulationUnitID"]) == 2)
                            fineMoney += Convert.ToDouble(drv.Row["StimulationSize"]);
                    }
                }

            //HoursRatioControl.LeftValue = promotionHours;
            //HoursRatioControl.RightValue = fineHours;

            MoneyRatioControl.LeftValue = promotionMoney;
            MoneyRatioControl.RightValue = fineMoney;
        }

        #region WorkerGroup_Filter
        private void WorkerGroupsFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void GroupFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WorkerGroupsFilterComboBox.SelectedIndex = 1;
        }

        private void GroupFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterWorkers();

            WorkerGroupsFilterComboBox.SelectedIndex = -1;
        }

        #endregion

        #region Factory_Filter
        private void FactoriesFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FactoryFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FactoriesFilterComboBox.SelectedIndex = 1;
        }

        private void FactoryFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterWorkers();

            FactoriesFilterComboBox.SelectedIndex = -1;
        }

        #endregion

        #region Status_Filter

        private void WorkerStatusesFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void StatusFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WorkerStatusesFilterComboBox.SelectedIndex = 1;

            AvailableInListCheckBox.IsChecked = false;
            AvailableInListCheckBox.IsEnabled = false;
        }

        private void StatusFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterWorkers();

            AvailableInListCheckBox.IsEnabled = true;

            WorkerStatusesFilterComboBox.SelectedIndex = -1;
        }

        #endregion

        private void FilterWorkers()
        {
            StaffListBox.SelectionChanged -= StaffListBox_SelectionChanged;

            SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
            SearchTextBox.Text = string.Empty;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            if (!GroupFilterCheckBox.IsChecked == true && !FactoryFilterCheckBox.IsChecked == true &&
                !StatusFilterCheckBox.IsChecked == true)
            {
                StaffListBox.ItemsSource = _staffPersonalInfoCollection;
            }
            else
            {
                int workerGroupsID = 0;
                bool isGroupFilter = GroupFilterCheckBox.IsChecked == true;
                if (isGroupFilter)
                    int.TryParse(WorkerGroupsFilterComboBox.SelectedValue.ToString(), out workerGroupsID);

                int factoryID = 0;
                bool isFactoriesFilter = FactoryFilterCheckBox.IsChecked == true;
                if (isFactoriesFilter) int.TryParse(FactoriesFilterComboBox.SelectedValue.ToString(), out factoryID);

                int statusID = 0;
                bool isStatusFilter = StatusFilterCheckBox.IsChecked == true;
                if (isStatusFilter) int.TryParse(WorkerStatusesFilterComboBox.SelectedValue.ToString(), out statusID);

                DataTable stafDataTable = _sc.FilterWorkers(isGroupFilter, workerGroupsID, isFactoriesFilter, factoryID,
                    isStatusFilter,
                    statusID);

                StaffListBox.ItemsSource = stafDataTable == null ? null : stafDataTable.DefaultView;
            }

            SearchAndFilterWorkers();
        }

        private void SearchAndFilterWorkers()
        {
            if (StaffListBox.ItemsSource == null) return;

            StaffListBox.SelectionChanged -= StaffListBox_SelectionChanged;

            string filterString = string.Empty;

            string searchText = SearchTextBox.Text.Trim();

            if (AvailableInListCheckBox.IsChecked == true)
            {
                filterString = "AvailableInList = 'True'";
            }

            if (searchText != string.Empty)
            {
                if (filterString == string.Empty) filterString = "(Name LIKE  '" + searchText + "*')";
                else filterString = filterString + " AND (Name LIKE  '" + searchText + "*')";
            }

            ((DataView)StaffListBox.ItemsSource).RowFilter = filterString;
            StaffListBox.SelectedIndex = 0;

            StaffCountLabel.Content = StaffListBox.Items.Count;

            StaffListBox.SelectionChanged += StaffListBox_SelectionChanged;

            StaffListBox_SelectionChanged(null, null);
        }

        private void AvailableInListCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SearchAndFilterWorkers();
        }

        private void AvailableInListCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchAndFilterWorkers();
        }

        private void EditStaffContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                //CloseAppBar(true);
                var editStaffContact = new EditStaffContact(Convert.ToInt32(StaffListBox.SelectedValue));
                _mw.ShowCatalogGrid(editStaffContact, "Контактная информация");
            }
        }

        private void EditStaffProfessionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                //CloseAppBar(true);
                var editStaffProfessions = new EditWorkerProfessions(Convert.ToInt32(StaffListBox.SelectedValue));
                _mw.ShowCatalogGrid(editStaffProfessions, "Список профессий");
            }
        }

        private void SaveStaffPersonalInfo_Click(object sender, RoutedEventArgs e)
        {
            var worker = StaffListBox.SelectedItem as DataRowView;
            if (worker == null) return;

            if (string.IsNullOrEmpty(StaffNameTextBox.Text) || !StaffBirthDatePicker.SelectedDate.HasValue ||
                MartialStatusComboBox.SelectedItem == null || string.IsNullOrEmpty(PassportNumberTextBlock.Text) ||
                !PassportIssueDateDatePicker.SelectedDate.HasValue || string.IsNullOrEmpty(PassportAuthorityIssuingTextBox.Text))
                return;

            var workerId = Convert.ToInt64(worker["WorkerID"]);
            var workerName = StaffNameTextBox.Text;
            var birthDay = StaffBirthDatePicker.SelectedDate.Value;
            var martialStatusId = Convert.ToInt32(MartialStatusComboBox.SelectedValue);
            var passportNumber = PassportNumberTextBlock.Text;
            var passportIssueDate = PassportIssueDateDatePicker.SelectedDate.Value;
            var passportAuthorityIssuing = PassportAuthorityIssuingTextBox.Text;

            var oldName = worker["Name"].ToString();

            _sc.ChangeWorkerInfo(workerId, workerName, birthDay, martialStatusId, passportNumber, passportIssueDate, passportAuthorityIssuing);

            if (!oldName.Equals(workerName))
                RenameWorkerFolder();
        }

        private void EditStaffProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                //CloseAppBar(true);
                var editStaffProdStatus = new EditWorkerProdStatuses(Convert.ToInt32(StaffListBox.SelectedValue));
                _mw.ShowCatalogGrid(editStaffProdStatus, "Список производственных навыков");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                //CloseAppBar(true);
                var editStaffEducation = new EditStaffEducation(Convert.ToInt32(StaffListBox.SelectedValue));
                _mw.ShowCatalogGrid(editStaffEducation, "Список полученных образований");
            }
        }


        public void SelectNewWorker(int workerId)
        {
            StaffListBox.SelectionChanged -= StaffListBox_SelectionChanged;
            StaffListBox.SelectedValue = workerId;
            StaffListBox.SelectionChanged += StaffListBox_SelectionChanged;

            StaffListBox_SelectionChanged(null, null);
            if (StaffListBox.SelectedItem != null)
                StaffListBox.ScrollIntoView(StaffListBox.SelectedItem);
        }

        private void StaffPhotoImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (!_fullAccess) return;

                if (StaffListBox.SelectedItem == null) return;
                if (_sc.SelectPhoto(Convert.ToInt32(StaffListBox.SelectedValue)))
                    StaffListBox_SelectionChanged(null, null);
            }
        }

        //private void AddW26Button_Click(object sender, RoutedEventArgs e)
        //{
        //    SpnxReaderReceiveW26();
        //}

        //public void SpnxReaderReceiveW26()
        //{
        //    string result = string.Empty;
        //    var bw = new BackgroundWorker();

        //    AddW26Button.IsEnabled = false;

        //    _spnxc.SpnxReaderOpen();

        //    bw.DoWork += (sender, args) => result = _spnxc.SpnxReaderReceiveW26();

        //    bw.RunWorkerCompleted += (obj, ea) => Dispatcher.BeginInvoke(new ThreadStart(delegate
        //    {
        //        if (result == string.Empty)
        //        {
        //            bw.RunWorkerAsync();
        //            return;
        //        }

        //        _sc.SetW26(Convert.ToInt32(StaffListBox.SelectedValue), result);

        //        AddW26Button.IsEnabled = true;
        //        _spnxc.SpnxReaderClose();
        //    }));

        //    bw.RunWorkerAsync();
        //}

        private void ClearW26Button_Click(object sender, RoutedEventArgs e)
        {
            if (((DataRowView)StaffListBox.SelectedItem).Row["W26"].ToString() == string.Empty) return;

            var workerId = Convert.ToInt32(StaffListBox.SelectedValue);

            var dialogResult = MessageBox.Show(
                "Выдействительно хотите открепить пропуск от '" +
                _sc.GetWorkerName(workerId) + "'", "Замена",
                MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                _sc.ClearW26(workerId);
            }
        }

        private void StaffStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_fullAccess) return;

            var worker = StaffListBox.SelectedItem as DataRowView;
            if (worker == null) return;

            var workersStatus = StaffStatusComboBox.SelectedItem as DataRowView;
            if (workersStatus == null) return;

            var workerId = Convert.ToInt64(worker["WorkerID"]);
            var workerStatusId = Convert.ToInt32(worker["StatusID"]);

            var statusId = Convert.ToInt32(workersStatus["WorkerStatusID"]);
            var availableInList = Convert.ToBoolean(workersStatus["AvailableInList"]);

            if (workerStatusId != statusId)
                _sc.ChangeWorkerStatus(workerId, statusId, availableInList);
        }
        

        private void ApplyPromptionsFilter_Click(object sender, RoutedEventArgs e)
        {
            if (PromotionsDateFrom.SelectedDate == null || PromotionsDateTo.SelectedDate == null) return;

            DateTime dateFrom = PromotionsDateFrom.SelectedDate.Value;
            DateTime dateTo = PromotionsDateTo.SelectedDate.Value;

            _stc.FillWorkersStim(dateFrom, dateTo);
            PromotionDataGrid.ItemsSource = null;
            FineDataGrid.ItemsSource = null;
            BindingPromotions();

            FilterPromotions();

            if (PromotionDataGrid.Items.Count != 0 || FineDataGrid.Items.Count != 0)
                WorkerPromotionsPanel.IsExpanded = true;
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


        #region Modes switch

        private void CloseAppBar()
        {
            AdditionalMenuToggleButton.IsChecked = false;
        }

        private void AddNewWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAppBar();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                //CloseAppBar(true);
                var addNewWorker = new AddNewWorker();
                _mw.ShowCatalogGrid(addNewWorker, "Добавить нового работника");
            }

        }

        private void ProductionStatusesAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAppBar();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                var prodStatusCatalog = new ProductionStatusesCatalog();
                _mw.ShowCatalogGrid(prodStatusCatalog, "Производственные навыки");
            }
        }

        private void ProfessionsCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAppBar();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                var profCatalog = new ProfessionsCatalog();
                _mw.ShowCatalogGrid(profCatalog, "Профессии");
            }
        }

        private void StatusesCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAppBar();

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                var workersStatuses = new WorkersStatusesCatalog();
                _mw.ShowCatalogGrid(workersStatuses, "Статусы");
            }
        }



        //private void ProgramEntryJournalToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //// Setting enabled for toggle buttons
        //AddButtonRow.Height = new GridLength(0);
        //WorkersNameListBox.SelectionMode = SelectionMode.Single;
        //TabControl.SelectedIndex = 0;
        //CloseAppBar();

        //_workingMode = Mode.Statistics;
        //WorkersNameListBox_SelectionChanged(null, null);
        //}

        //private void WorkerRightsToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //// Setting enabled for toggle buttons
        //AddButtonRow.Height = new GridLength(1, GridUnitType.Auto);
        //WorkersNameListBox.SelectionMode = SelectionMode.Extended;
        //TabControl.SelectedIndex = 1;
        //CloseAppBar();

        //_workingMode = Mode.WorkerAccess;
        //WorkersNameListBox_SelectionChanged(null, null);
        //}


        private void OnShadowGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAppBar();
        }

        #endregion

        #region Uploading

        private void AddFilesToWorkerButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            var workerId = Convert.ToInt32(StaffListBox.SelectedValue);
            var ofd = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };

            if (ofd.ShowDialog().Value)
            {
                try
                {
                    UploadFiles(ofd.FileNames, workerId);
                }
                catch (Exception ex)
                {
                    MetroMessageBox.Show("Невозможно найти файл: " + ex.Message, "Ошибка");
                }
            }
        }

        private void UploadFiles(IEnumerable<string> filePaths, int workerId)
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            var workerPath = GetWorkerPath(workerId);

            // Worker directory doesnt exist
            if (workerPath == null)
            {
                var workerName =
                    new IdToNameConverter().Convert(workerId, typeof(string), "FullName", new CultureInfo("ru-RU"))
                        .ToString();
                // Get right name of worker directory
                var renamedWorkerName = GetRenamedFileName(workerName);
                workerPath = string.Format("{0}_[id]{1}", renamedWorkerName, workerId);
                // Create worker directory
                _ftpClient.MakeDirectory(string.Concat(_ftpClient.CurrentPath, workerPath, "/"));
            }

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var adress = string.Concat(_ftpClient.CurrentPath, workerPath, "/", fileName);

                if (!_ftpClient.FileExist(adress))
                {
                    _sc.AddFileToWorker(Convert.ToInt32(StaffListBox.SelectedValue), fileName,
                            AdministrationClass.CurrentWorkerId, App.BaseClass.GetDateFromSqlServer());
                }
                else
                {
                    if (MetroMessageBox.Show("Файл '" + fileName + "' уже существует в данном каталоге. \n\n" +
                                             "Заменить существующий файл?", string.Empty, MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                }

                var uri = new Uri(adress);

                if (_processWindow != null)
                    _processWindow.Close(true);

                _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                _processWindow.Show(Window.GetWindow(this), true);
                _ftpClient.UploadFileCompleted += OnFtpClientUploadFileCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
            }
        }

        private void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (_processWindow != null)
            {
                _processWindow.Progress = (e.BytesSent / (double)e.TotalBytesToSend) * 100;
                _processWindow.Text = string.Format("Загрузка файла... \n{0} из {1} кБ", e.BytesSent / 1024,
                    e.TotalBytesToSend / 1024);
            }
        }

        private void OnFtpClientUploadFileCompleted(object sender, UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadFileCompleted;

            if (_processWindow != null)
                _processWindow.Close(true);
        }

        #endregion

        #region Deleting

        private void DeleteFilesFromWorkerButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null || WorkerFilesList.SelectedItem == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить файл работника?", "Удаление", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var workerFileId = Convert.ToInt32(WorkerFilesList.SelectedValue);
            var selectedFile = ((DataRowView)WorkerFilesList.SelectedItem)["FileName"].ToString();
            var workerPath = GetWorkerPath(Convert.ToInt32(StaffListBox.SelectedValue));
            var fullPath = string.Concat(_basicDirectory, workerPath, "/", selectedFile);

            _sc.DeleteWorkerFile(workerFileId);
            _ftpClient.DeleteFile(fullPath);
        }

        private void DeleteFileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (_fullAccess)
                DeleteFilesFromWorkerButton_OnClick(null, null);
        }

        #endregion

        private void RenameWorkerFolder()
        {
            if (StaffListBox.SelectedItem == null || string.IsNullOrEmpty(StaffNameTextBox.Text)) return;

            var workerId = Convert.ToInt32(StaffListBox.SelectedValue);
            var oldWorkerPath = GetWorkerPath(workerId);
            var newWorkerPath =
                string.Format("{0}_[id]{1}", GetRenamedFileName(StaffNameTextBox.Text), workerId);

            // If directory names are similar, return
            if (oldWorkerPath == newWorkerPath) return;

            _ftpClient.Rename(string.Concat(_basicDirectory, oldWorkerPath), newWorkerPath);
        }

        #region Opening

        private void WorkerFilesRow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            var selectedFile = ((DataRowView)WorkerFilesList.SelectedItem)["FileName"].ToString();
            var workerPath = GetWorkerPath(Convert.ToInt32(StaffListBox.SelectedValue));
            if (workerPath == null) return;

            var tempPath = Path.Combine(App.TempFolder, selectedFile);

            if (File.Exists(tempPath))
            {
                Process.Start(tempPath);
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            _neededOpeningFilePath = tempPath;
            var filePath = string.Concat(_basicDirectory, workerPath, "/", selectedFile);
            var uri = new Uri(filePath);

            if (_processWindow != null)
                _processWindow.Close(true);

            _processWindow = new WaitWindow { Text = "Загрузка файла..." };
            _processWindow.Show(Window.GetWindow(this), true);

            _fileSize = _ftpClient.GetFileSize(filePath);
            _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
            _ftpClient.DownloadFileAsync(uri, tempPath);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.BytesReceived / (double)_fileSize;

            if (_processWindow != null)
            {
                _processWindow.Progress = progress * 100;
                _processWindow.Text = string.Format("Загрузка файла... \n{0} кБ", e.BytesReceived / 1024);
            }
        }

        private void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            try
            {
                Process.Start(_neededOpeningFilePath);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            if (_processWindow != null)
                _processWindow.Close(true);

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;
        }

        private void OpenFileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            WorkerFilesRow_OnMouseDoubleClick(null, null);
        }

        #endregion

        private string GetWorkerPath(int workerId)
        {
            var searchCriteria = string.Format("[id]{0}", workerId);

            _ftpClient.CurrentPath = _basicDirectory;
            var filesList = _ftpClient.ListDirectory();
            return filesList.FirstOrDefault(f => f.Contains(searchCriteria));
        }

        private static string GetRenamedFileName(string fileName)
        {
            var renamedFileName = fileName;
            const string str = @"?:<>/|\""";
            foreach (var symbol in renamedFileName.Where(symbol => str.Any(s => s == symbol)))
            {
                renamedFileName = renamedFileName.Replace(symbol, '_');
            }
            return renamedFileName.Trim();
        }

        private void OnAdmissionsCatalogButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                var admissionsPage = new AdmissionsPage();
                mainWindow.ShowCatalogGrid(admissionsPage, "Допуски");
            }
        }

        private void OnEditWorkerAdmissionsButtonClick(object sender, RoutedEventArgs e)
        {
            if (StaffListBox.SelectedItem == null) return;

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                var workerId = Convert.ToInt64(StaffListBox.SelectedValue);
                var workerAdmissionsPage = new WorkerAdmissionsPage(workerId);
                _mw.ShowCatalogGrid(workerAdmissionsPage, string.Format("Список допусков ({0})", new IdToNameConverter().Convert(workerId, "ShortName")));
            }
        }
    }

    public static class UIHelper
    {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }
    }
}
