using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FA2.ChildPages.CatalogPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage
    {
        private bool _firstRun = true;
        private bool _fullAccess;
        private StaffClass _sc;
        private CatalogClass _cc;

        private readonly Collection<Button> _saveButtons = new Collection<Button>();

        private readonly System.Windows.Forms.Timer _tmr = new System.Windows.Forms.Timer {Interval = 1000};

        public CatalogPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.OperationCatalog);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.OperationCatalog);

            if (_firstRun)
            {
                _tmr.Tick += tmr_Tick;

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (o, args) =>
                    GetClasses();

                backgroundWorker.RunWorkerCompleted += (o, args) =>
                                                       {
                                                           SetBindings();
                                                           SetControlsEnable();

                                                           var mainWindow = Application.Current.MainWindow as MainWindow;
                                                           if (mainWindow != null) mainWindow.HideWaitAnnimation();
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

        private void tmr_Tick(object sender, EventArgs e)
        {
            foreach (Button btn in _saveButtons)
            {
                btn.Content = "Сохранить изменения";
            }

            _saveButtons.Clear();

            _tmr.Stop();
        }


        private void SetBindings()
        {
            WorkerGroupsComboBox.SelectionChanged -= WorkerGroupsComboBox_SelectionChanged;
            WorkerGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkerGroupsComboBox.SelectionChanged += WorkerGroupsComboBox_SelectionChanged;


            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.ItemsSource = _cc.GetFactories();
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

            WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;
            WorkUnitsListBox.ItemsSource = _cc.GetWorkUnits();
            WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;


            WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;
            WorkSectionsListBox.ItemsSource = _cc.GetWorkSections();
            WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;


            WorkSubsectionsListBox.SelectionChanged -= WorkSubsectionsListBox_SelectionChanged;
            WorkSubsectionsListBox.ItemsSource = _cc.GetWorkSubsections();
            WorkSubsectionsListBox.SelectionChanged += WorkSubsectionsListBox_SelectionChanged;


            WorkOperationsListBox.SelectionChanged -= WorkOperationsListBox_SelectionChanged;
            WorkOperationsListBox.ItemsSource = _cc.GetWorkOperations();
            WorkOperationsListBox.SelectionChanged += WorkOperationsListBox_SelectionChanged;

            OperationGroupsComboBox.ItemsSource = _cc.GetOperationGroups();

            MeasureUnitsComboBox.ItemsSource = _cc.GetMeasureUnits();
            MeasureUnitsComboBox.SelectedIndex = 0;

            WorkerGroupsProp1ComboBox.ItemsSource = _sc.GetWorkerGroups();

            FactoriesProp1ComboBox.ItemsSource = _cc.GetFactories();

            SubsectionsGroupsProp3ComboBox.ItemsSource = _cc.GetWorkSubsectionsGroups();
            
            WorkerGroupsComboBox.SelectedIndex = 0;

            FactoriesComboBox.SelectedIndex = 0;
            WorkUnitsListBox.SelectedIndex = 0;
        }

        private void SetControlsEnable()
        {
            var defaultVisibility = _fullAccess ? Visibility.Visible : Visibility.Collapsed;

            EditWorkerGroupsButton.Visibility = defaultVisibility;
            EditFactoriesButton.Visibility = defaultVisibility;
            ShowNewUnitGridButton.Visibility = defaultVisibility;
            DeleteUnitButton.Visibility = defaultVisibility;
            ShowNewSectionGridButton.Visibility = defaultVisibility;
            DeleteSectionButton.Visibility = defaultVisibility;
            ShowNewSubsectionGridButton.Visibility = defaultVisibility;
            DeleteSubsectionButton.Visibility = defaultVisibility;
            ShowNewOperationGridButton.Visibility = defaultVisibility;
            DeleteOperationButton.Visibility = defaultVisibility;
            SaveUnitsButton.Visibility = defaultVisibility;
            SaveSectionsButton.Visibility = defaultVisibility;
            SaveSubsectionsButton.Visibility = defaultVisibility;
            SaveOperationsButton.Visibility = defaultVisibility;
            EditOperationGroupsButton.Visibility = defaultVisibility;

            WorkUnitInfoPanel.IsEnabled = _fullAccess;
            WorkSectionInfoPanel.IsEnabled = _fullAccess;
            WorkSubSectionsInfoPanel.IsEnabled = _fullAccess;
            WorkOperationInfoPanel.IsEnabled = _fullAccess;
        }

        private void GetClasses()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetCatalogClass(ref _cc);
        }

        private void WorkerGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkerGroupsComboBox.SelectedItem == null || WorkerGroupsComboBox.SelectedValue == null) return;
            CatalogListsGrid.DataContext = _cc.GetTitles(Convert.ToInt32(WorkerGroupsComboBox.SelectedValue));


            if (!Convert.ToBoolean(((DataRowView) WorkerGroupsComboBox.SelectedItem)["UnitToFactory"]))
            {
                ((DataView)WorkUnitsListBox.ItemsSource).RowFilter = String.Format("Visible = True" + " AND WorkerGroupID = {0}", WorkerGroupsComboBox.SelectedValue);
            }

            else
            {
                FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
                FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

                if (FactoriesComboBox.Items.Count == 0) return;

                ((DataView) WorkUnitsListBox.ItemsSource).RowFilter =
                    string.Format("Visible = True AND WorkerGroupID = {0} AND FactoryID = {1}",
                        WorkerGroupsComboBox.SelectedValue, FactoriesComboBox.SelectedValue);
            }

            WorkUnitsListBox.SelectedIndex = 0;
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoriesComboBox.SelectedItem == null || FactoriesComboBox.SelectedValue == null) return;

            if (!Convert.ToBoolean(((DataRowView) WorkerGroupsComboBox.SelectedItem)["UnitToFactory"]))
            {
                ((DataView)WorkUnitsListBox.ItemsSource).RowFilter = string.Format("Visible = True AND WorkerGroupID = {0}",
                        WorkerGroupsComboBox.SelectedValue);
            }

            else
            {
                FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
                FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

                if (FactoriesComboBox.Items.Count == 0) return;

                ((DataView)WorkUnitsListBox.ItemsSource).RowFilter = string.Format("Visible = True AND WorkerGroupID = {0} AND FactoryID = {1}",
                        WorkerGroupsComboBox.SelectedValue, FactoriesComboBox.SelectedValue);
            }

            WorkUnitsListBox.SelectedIndex = 0;
        }

        private void WorkUnitsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsListBox.SelectedItem == null || WorkUnitsListBox.SelectedValue == null)
            {

                prop1Grid.DataContext = null;

                ((DataView) WorkSectionsListBox.ItemsSource).RowFilter = "WorkUnitID = -1";

                WorkSectionsListBox_SelectionChanged(null, null);
                return;
            }

            prop1Grid.DataContext = WorkUnitsListBox.SelectedItem;

            ((DataView) WorkSectionsListBox.ItemsSource).RowFilter = string.Format("WorkUnitID = {0}",
                WorkUnitsListBox.SelectedValue);

            WorkSectionsListBox.SelectedIndex = 0;
            WorkSectionsListBox.ScrollIntoView(WorkSectionsListBox.SelectedItem);
        }

        private void WorkSectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSectionsListBox.SelectedItem == null || WorkSectionsListBox.SelectedValue == null)
            {
                prop2Grid.DataContext = null;

                ((DataView) WorkSubsectionsListBox.ItemsSource).RowFilter = "WorkSectionID = -1";

                WorkSubsectionsListBox_SelectionChanged(null, null);
                return;
            }

            prop2Grid.DataContext = WorkSectionsListBox.SelectedItem;

            ((DataView) WorkSubsectionsListBox.ItemsSource).RowFilter =
                string.Format("Visible = True AND WorkSectionID ={0}", WorkSectionsListBox.SelectedValue);

            WorkSubsectionsListBox.SelectedIndex = 0;
            WorkSubsectionsListBox.ScrollIntoView(WorkSubsectionsListBox.SelectedItem);
        }

        private void WorkSubsectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSubsectionsListBox.SelectedItem == null || WorkSubsectionsListBox.SelectedValue == null)
            {
                prop3Grid.DataContext = null;

                WorkOperationsListBox.ItemsSource = null;

                AdittProp3StackPanel.DataContext = null;

                WorkOperationsListBox_SelectionChanged(null, null);

                return;
            }

            prop3Grid.DataContext = WorkSubsectionsListBox.SelectedItem;

            DataView operationsDataView = _cc.GetWorkOperations();

            if (AdditOperationsChexkBox.IsChecked == true)
            {
                operationsDataView.RowFilter = String.Format("WorkSubsectionID  IN ({0}, -1) AND Visible = True", WorkSubsectionsListBox.SelectedValue);

                var operationsCollectionView =
                    new BindingListCollectionView(operationsDataView);

                if (operationsCollectionView.GroupDescriptions != null)
                {
                    operationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("OperationTypeID",
                        new OperationTypeConverter()));
                    operationsCollectionView.SortDescriptions.Add(new SortDescription("OperationTypeID",
                        ListSortDirection.Ascending));

                    WorkOperationsListBox.SelectionChanged -= WorkOperationsListBox_SelectionChanged;
                    WorkOperationsListBox.ItemsSource = operationsCollectionView;
                    WorkOperationsListBox.SelectionChanged += WorkOperationsListBox_SelectionChanged;
                }


            }
            else
            {
                operationsDataView.RowFilter = String.Format("WorkSubsectionID = {0} AND Visible = True", WorkSubsectionsListBox.SelectedValue);

                WorkOperationsListBox.SelectionChanged -= WorkOperationsListBox_SelectionChanged;
                WorkOperationsListBox.ItemsSource = operationsDataView;
                WorkOperationsListBox.SelectionChanged += WorkOperationsListBox_SelectionChanged;

                WorkOperationsListBox.Items.Refresh();
            }



            WorkOperationsListBox.SelectedIndex = 0;
            WorkOperationsListBox.ScrollIntoView(WorkOperationsListBox.SelectedItem);
            WorkOperationsListBox_SelectionChanged(null, null);

            AdittProp3StackPanel.DataContext =
                Convert.ToInt32(((DataRowView) WorkSubsectionsListBox.SelectedItem)["SubsectionGroupID"]) == 2
                    ? _cc.GetMachineInfo(Convert.ToInt32(WorkSubsectionsListBox.SelectedValue))
                    : null;
        }

        private void WorkOperationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkOperationsListBox.SelectedItem == null || WorkOperationsListBox.SelectedValue == null)
            {
                prop4Grid.DataContext = null;
                AdittProp4StackPanel.DataContext = null;

                return;
            }

            prop4Grid.IsEnabled = Convert.ToInt32(((DataRowView) WorkOperationsListBox.SelectedItem)["OperationTypeID"]) != 2;

            prop4Grid.DataContext = WorkOperationsListBox.SelectedItem;

            AdittProp4StackPanel.DataContext =
                _cc.GetMachineOperationInfo(Convert.ToInt32(WorkOperationsListBox.SelectedValue));
        }

        private void CancelNewUnitButton_Click(object sender, RoutedEventArgs e)
        {
            NewUnitNameTextBox.Text = string.Empty;
            AddNewUnitGrid.Visibility = Visibility.Hidden;
        }

        private void ShowNewUnitGridButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewUnitGrid.Visibility = Visibility.Visible;
            NewUnitNameTextBox.Focus();
        }

        private void AddNewUnitButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewUnitNameTextBox.Text.Trim() == string.Empty) return;

            _cc.AddWorkUnit(Convert.ToInt32(WorkerGroupsComboBox.SelectedValue), NewUnitNameTextBox.Text,
                Convert.ToInt32(FactoriesComboBox.SelectedValue));

            NewUnitNameTextBox.Text = string.Empty;

            AddNewUnitGrid.Visibility = Visibility.Hidden;

            AdministrationClass.AddNewAction(24);
        }

        private void DeleteUnitButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkUnitsListBox.SelectedItem == null) return;

            MessageBoxResult result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView) WorkUnitsListBox.SelectedItem)["WorkUnitName"] + "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView) WorkUnitsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkSections();

                AdministrationClass.AddNewAction(32);
            }
        }



        private void ShowNewSectionGridButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewSectionGrid.Visibility = Visibility.Visible;
            NewSectionNameTextBox.Focus();
        }

        private void CancelNewSectionButton_Click(object sender, RoutedEventArgs e)
        {
            NewSectionNameTextBox.Text = string.Empty;

            AddNewSectionGrid.Visibility = Visibility.Hidden;
        }

        private void AddNewSectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewSectionNameTextBox.Text.Trim() == string.Empty) return;

            _cc.AddWorkSection(Convert.ToInt32(WorkUnitsListBox.SelectedValue), NewSectionNameTextBox.Text);

            NewSectionNameTextBox.Text = string.Empty;

            AddNewSectionGrid.Visibility = Visibility.Hidden;

            AdministrationClass.AddNewAction(25);
        }

        private void DeleteSectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSectionsListBox.SelectedItem == null) return;

            MessageBoxResult result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView) WorkSectionsListBox.SelectedItem)["WorkSectionName"] + "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView) WorkSectionsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkSections();

                AdministrationClass.AddNewAction(33);
            }
        }

        private void AddNewSubsectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewSubsectionNameTextBox.Text.Trim() == string.Empty) return;

            _cc.AddWorkSubsection(Convert.ToInt32(WorkSectionsListBox.SelectedValue), NewSubsectionNameTextBox.Text);

            NewSubsectionNameTextBox.Text = string.Empty;

            AddNewSubsectionGrid.Visibility = Visibility.Hidden;

            AdministrationClass.AddNewAction(26);
        }

        private void CancelNewSubsectionButton_Click(object sender, RoutedEventArgs e)
        {
            NewSubsectionNameTextBox.Text = string.Empty;

            AddNewSubsectionGrid.Visibility = Visibility.Hidden;
        }

        private void ShowNewSubsectionGridButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewSubsectionGrid.Visibility = Visibility.Visible;
            NewSubsectionNameTextBox.Focus();
        }

        private void DeleteSubsectionButton_Click(object sender, RoutedEventArgs e)
        {

            if (WorkSectionsListBox.SelectedItem == null) return;

            MessageBoxResult result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView) WorkSubsectionsListBox.SelectedItem)["WorkSubsectionName"] +
                    "' ?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView) WorkSubsectionsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkSections();

                AdministrationClass.AddNewAction(34);
            }
        }

        private void ShowNewOperationGridButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewOperationGrid.Visibility = Visibility.Visible;
            NewOperationNameTextBox.Focus();
        }

        private void DeleteOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSectionsListBox.SelectedItem == null) return;

            MessageBoxResult result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView) WorkOperationsListBox.SelectedItem)["WorkOperationName"] + "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView) WorkOperationsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkOperation();

                AdministrationClass.AddNewAction(35);
            }
        }

        private void CancelNewOperationButton_Click(object sender, RoutedEventArgs e)
        {
            NewOperationNameTextBox.Text = string.Empty;
            AddNewOperationGrid.Visibility = Visibility.Hidden;
        }

        private void AddNewOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOperationNameTextBox.Text.Trim() == string.Empty) return;

            _cc.AddWorkOperation(Convert.ToInt32(WorkSubsectionsListBox.SelectedValue),
                Convert.ToInt32(SubsectionsGroupsProp3ComboBox.SelectedValue), NewOperationNameTextBox.Text);

            NewOperationNameTextBox.Text = string.Empty;

            AddNewOperationGrid.Visibility = Visibility.Hidden;

            AdministrationClass.AddNewAction(27);
        }

        private void SaveUnitsButton_Click(object sender, RoutedEventArgs e)
        {
            _cc.SaveWorkUnits();

            AdministrationClass.AddNewAction(37);

            SaveUnitsButton.Content = "Сохранено";
            //SaveUnitsButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#3C9300");
            _saveButtons.Add(SaveUnitsButton);
            _tmr.Start();
        }

        private void SaveSectionsButton_Click(object sender, RoutedEventArgs e)
        {
            _cc.SaveWorkSections();
            AdministrationClass.AddNewAction(29);

            SaveSectionsButton.Content = "Сохранено";
            //SaveSectionsButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#3C9300");
            _saveButtons.Add(SaveSectionsButton);
            _tmr.Start();
        }

        private void SaveSubsectionsButton_Click(object sender, RoutedEventArgs e)
        {
            int workSubsectionsGroupId =
                Convert.ToInt32(((DataRowView) SubsectionsGroupsProp3ComboBox.SelectedItem)["WorkSubsectionsGroupID"]);

            int workSubsectionsId =
                Convert.ToInt32(((DataRowView) WorkSubsectionsListBox.SelectedItem)["WorkSubSectionID"]);

            DataRow[] machineDr =
                _cc.MachinesDataTable.Select("WorkSubsectionID=" + workSubsectionsId + " AND IsVisible=True");

            if (!machineDr.Any())
            {
                if (workSubsectionsGroupId == 2)
                {
                    DataRow mdr = _cc.MachinesDataTable.NewRow();

                    mdr["WorkSubsectionID"] = workSubsectionsId;
                    mdr["MachineName"] =
                        ((DataRowView) WorkSubsectionsListBox.SelectedItem).Row["WorkSubsectionName"].ToString();

                    mdr["MachineSchemeNumber"] = MachineSchemeNumberNumericControl.Value;
                    mdr["MachineWorkPlacesCount"] = MachineWorkPlacesCountNumericControl.Value;

                    mdr["Width"] = Convert.ToDecimal(MachineWidthNumericControl.Value) != 0 
                        ? MachineWidthNumericControl.Value : 10;
                    mdr["Length"] = Convert.ToDecimal(MachineLengthNumericControl.Value) != 0
                        ? MachineLengthNumericControl.Value : 10;
                    mdr["IsVisible"] = true;

                    _cc.MachinesDataTable.Rows.Add(mdr);
                }
            }
            else
            {
                if (workSubsectionsGroupId == 1)
                {
                    machineDr[0]["IsVisible"] = false;
                }

                if (workSubsectionsGroupId == 2)
                {
                    machineDr[0]["MachineName"] =
                        ((DataRowView) WorkSubsectionsListBox.SelectedItem).Row["WorkSubsectionName"].ToString();
                    machineDr[0]["WorkSubsectionID"] = workSubsectionsId;
                    machineDr[0]["MachineSchemeNumber"] = MachineSchemeNumberNumericControl.Value;
                    machineDr[0]["MachineWorkPlacesCount"] = MachineWorkPlacesCountNumericControl.Value;
                    machineDr[0]["Width"] = Convert.ToDecimal(MachineWidthNumericControl.Value) != 0
                        ? MachineWidthNumericControl.Value : 10;
                    machineDr[0]["Length"] = Convert.ToDecimal(MachineLengthNumericControl.Value) != 0
                        ? MachineLengthNumericControl.Value : 10;
                    machineDr[0]["IsVisible"] = true;
                }
            }

            _cc.SaveWorkSubsection();

            AdministrationClass.AddNewAction(30);

            SaveSubsectionsButton.Content = "Сохранено";
            _saveButtons.Add(SaveSubsectionsButton);
            _tmr.Start();
        }

        private void SaveOperationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MeasureUnitsComboBox.SelectedValue == null)
                MeasureUnitsComboBox.SelectedIndex = 0;

            int workSubsectionsGroupId =
                Convert.ToInt32(((DataRowView) SubsectionsGroupsProp3ComboBox.SelectedItem)["WorkSubsectionsGroupID"]);

            int workOperationId =
                Convert.ToInt32(((DataRowView) WorkOperationsListBox.SelectedItem)["WorkOperationID"]);

            DataRow[] modr =
                _cc.MachinesOperationsDataTable.Select("WorkOperationID=" + workOperationId);

            if (!modr.Any())
            {
                if (workSubsectionsGroupId == 2)
                {
                    DataRow odr = _cc.MachinesOperationsDataTable.NewRow();

                    odr["WorkOperationID"] = workOperationId;
                    odr["OperationCode"] = OperationCodeTextBox.Text;
                    odr["MinWorkerCategory"] = MinWorkerCategoryTextBox.Text;
                    odr["OperationPlaceNumber"] = OperationPlaceNumberNumericControl.Value;

                    odr["MeasureUnitID"] = MeasureUnitsComboBox.SelectedValue;
                    odr["Productivity"] = ProductivityNumericControl.Value;

                    odr["Insalubrity"] = InsalubrityCheckBox.IsChecked;
                    odr["InsalubrityRate"] = InsalubrityRateNumericControl.Value;

                    _cc.MachinesOperationsDataTable.Rows.Add(odr);
                }
            }

            _cc.SaveWorkOperation();

            AdministrationClass.AddNewAction(31);

            SaveOperationsButton.Content = "Сохранено";
            //SaveOperationsButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#3C9300");
            _saveButtons.Add(SaveOperationsButton);
            _tmr.Start();
        }

        private void WithWorkUnitComboBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WorkerGroupsComboBox.SelectedValue == null || FactoriesComboBox.SelectedValue == null ||
                Convert.ToInt32(WorkerGroupsComboBox.SelectedValue) != 2) return;

            var workerGroupId = Convert.ToInt32(WorkerGroupsComboBox.SelectedValue);
            var factoryId = Convert.ToInt32(FactoriesComboBox.SelectedValue);
            ExportToExcel.GenerateMachineOperationsReport(workerGroupId, factoryId, true, ref ExportToExcelComboBox);
        }

        private void WithoutWorkUnitComboBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WorkerGroupsComboBox.SelectedValue == null || FactoriesComboBox.SelectedValue == null ||
                Convert.ToInt32(WorkerGroupsComboBox.SelectedValue) != 2) return;

            var workerGroupId = Convert.ToInt32(WorkerGroupsComboBox.SelectedValue);
            var factoryId = Convert.ToInt32(FactoriesComboBox.SelectedValue);
            ExportToExcel.GenerateMachineOperationsReport(workerGroupId, factoryId, false, ref ExportToExcelComboBox);
        }

        private void SecondaryOperationsButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;

            if (mw == null) return;
            var additOperationsChildPage = new AdditionalOperationsChildPage(_fullAccess);
            mw.ShowCatalogGrid(additOperationsChildPage, "Общие операции");
        }

        private void EditFactoriesButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;

            if (mw == null) return;
            var factoriesChildPage = new FactoriesChildPage();
            mw.ShowCatalogGrid(factoriesChildPage, "Фабрики");
        }

        private void EditWorkerGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;

            if (mw == null) return;
            var workerGroupsChildPage = new WorkerGroupsChildPage();
            mw.ShowCatalogGrid(workerGroupsChildPage, "Группы");
        }

        private void AdditOperationsChexkBox_Checked(object sender, RoutedEventArgs e)
        {
            WorkSubsectionsListBox_SelectionChanged(null, null);
        }

        private void AdditOperationsChexkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WorkSubsectionsListBox_SelectionChanged(null, null);
        }

        private void EditOperationGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;

            if (mw == null) return;
            var operationGroupsPage = new OperationGroupsPage();
            mw.ShowCatalogGrid(operationGroupsPage, "Группы операций");
        }
    }
}
