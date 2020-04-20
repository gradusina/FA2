using FA2.XamlFiles;
using FA2.Classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Data;
using System.Linq;
using System.Windows.Data;
using FA2.Converters;
using System.ComponentModel;
using System.Windows.Media;

namespace FA2.ChildPages.AdmissionPage
{
    /// <summary>
    /// Логика взаимодействия для WorkerAdmissionsPage.xaml
    /// </summary>
    public partial class WorkerAdmissionsPage : Page
    {
        private AdmissionsClass _admClass;
        private StaffClass _staffClass;
        private CatalogClass _catalogClass;
        private long _workerId;
        private DataTable _workerProfessionsTable;
        private DataTable _choosenOperationsTable;
        private bool _unitToFactory;

        private const double NormalHeight = 450;
        private const double WorkOperaionDescriptionHeight = 550;

        public WorkerAdmissionsPage(long workerId)
        {
            InitializeComponent();

            _workerId = workerId;
            FillData();
            FillBindings();
        }

        private void FillData()
        {
            App.BaseClass.GetAdmissionsClass(ref _admClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetCatalogClass(ref _catalogClass);
        }

        private void FillBindings()
        {
            var workerAdmissions = _admClass.WorkerAdmissionsTable.AsDataView();
            var workerAdmissionsView = new BindingListCollectionView(workerAdmissions);
            if (workerAdmissionsView.GroupDescriptions != null)
                workerAdmissionsView.GroupDescriptions.Add(new PropertyGroupDescription("AdmissionID"));
            workerAdmissionsView.CustomFilter = string.Format("WorkerID = {0}", _workerId);
            WorkerAdmissionsListBox.ItemsSource = workerAdmissionsView;

            AdmissionsComboBox.ItemsSource = _admClass.GetAdmissionsView();

            var workerProfessionsView = _staffClass.GetWorkerProfessions();
            workerProfessionsView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            _workerProfessionsTable = workerProfessionsView.ToTable();
            _workerProfessionsTable.Columns.Add(new DataColumn("IsSelected", typeof(bool)));
            WorkerProfessionsItemsControl.ItemsSource = _workerProfessionsTable.DefaultView;

            BindingGroupComboBox();
            BindingFactoryComboBox();
            BindingWorkUnitsComboBox();
            BindingWorkSectionsComboBox();
            BindingWorkSubsectionsComboBox();

            _choosenOperationsTable = _catalogClass.WorkOperationsDataTable.Clone();
            ChoosenOperationsListBox.ItemsSource = _choosenOperationsTable.AsDataView();
        }

        private void BindingGroupComboBox()
        {
            WorkGroupsComboBox.ItemsSource = _catalogClass.GetWorkersGroups();
            WorkGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkGroupsComboBox.SelectedValuePath = "WorkerGroupID";
        }

        private void BindingFactoryComboBox()
        {
            FactoriesComboBox.ItemsSource = _catalogClass.GetFactories();
            FactoriesComboBox.DisplayMemberPath = "FactoryName";
            FactoriesComboBox.SelectedValuePath = "FactoryID";
        }

        private void BindingWorkUnitsComboBox()
        {
            WorkUnitsComboBox.DisplayMemberPath = "WorkUnitName";
            WorkUnitsComboBox.SelectedValuePath = "WorkUnitID";
        }

        private void BindingWorkSectionsComboBox()
        {
            WorkSectionsComboBox.DisplayMemberPath = "WorkSectionName";
            WorkSectionsComboBox.SelectedValuePath = "WorkSectionID";
        }

        private void BindingWorkSubsectionsComboBox()
        {
            WorkSubsectionsComboBox.DisplayMemberPath = "WorkSubsectionName";
            WorkSubsectionsComboBox.SelectedValuePath = "WorkSubsectionID";
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnEditWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var workerAdmission = WorkerAdmissionsListBox.SelectedItem as DataRowView;
            if (workerAdmission == null) return;

            var admissionId = Convert.ToInt32(workerAdmission["AdmissionID"]);
            if(admissionId == AdmissionsClass.WorkSubsectionAdmissionId)
            {
                var workerAdmissionId = Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
                ShowWorkSubsectionSelection(workerAdmissionId, false);
                return;
            }

            var admissionDate = Convert.ToDateTime(workerAdmission["AdmissionDate"]);
            var workerProfessionId = Convert.ToInt64(workerAdmission["WorkerProfessionID"]);

            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.Visibility = Visibility.Visible;
                AddNewWorkerAdmissionButton.Visibility = Visibility.Collapsed;
                ChangeWorkerAdmissionButton.Visibility = Visibility.Visible;
                RedactorGrid.DataContext = workerAdmission;
                AdmissionsComboBox.IsEnabled = false;
                AdmissionsComboBox.SelectedValue = admissionId;
                AdmissionDatePicker.SelectedDate = admissionDate;
                var workerProfessions = _workerProfessionsTable.Select(string.Format("WorkerProfessionID = {0}", workerProfessionId));
                if (workerProfessions.Any())
                    workerProfessions.First()["IsSelected"] = true;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnGoBackButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Visible;
                RedactorGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.DataContext = null;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            var heightAnimation = new DoubleAnimation(NormalHeight, new Duration(TimeSpan.FromMilliseconds(150)));
            MachineOptionsGrid.Visibility = Visibility.Collapsed;
            BeginAnimation(HeightProperty, heightAnimation);

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnAddWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.Visibility = Visibility.Visible;
                AddNewWorkerAdmissionButton.Visibility = Visibility.Visible;
                ChangeWorkerAdmissionButton.Visibility = Visibility.Collapsed;
                RedactorGrid.DataContext = null;
                AdmissionsComboBox.IsEnabled = true;
                if (AdmissionsComboBox.HasItems)
                {
                    AdmissionsComboBox.SelectionChanged -= OnAdmissionsComboBoxSelectionChanged;
                    AdmissionsComboBox.SelectedIndex = 0;
                    AdmissionsComboBox.SelectionChanged += OnAdmissionsComboBoxSelectionChanged;
                    OnAdmissionsComboBoxSelectionChanged(null, null);
                }
                if (_workerProfessionsTable.Rows.Count != 0)
                {
                    _workerProfessionsTable.Rows[0]["IsSelected"] = true;
                }
                AdmissionDatePicker.SelectedDate = App.BaseClass.GetDateFromSqlServer();

                WorkGroupPanel.IsEnabled = true;
                WorkUnitPanel.IsEnabled = true;
                WorkSubsectionsComboBox.IsEnabled = true;

                if (WorkGroupsComboBox.Items.Count > 1)
                    WorkGroupsComboBox.SelectedIndex = 1;
                else if(WorkGroupsComboBox.HasItems)
                    WorkGroupsComboBox.SelectedIndex = 0;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnAdmissionsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var machineAdmission = AdmissionsComboBox.SelectedItem is DataRowView &&
                Convert.ToInt32(((DataRowView)AdmissionsComboBox.SelectedItem)["AdmissionID"]) == AdmissionsClass.WorkSubsectionAdmissionId;
            var heightAnimation = machineAdmission 
                ? new DoubleAnimation(WorkOperaionDescriptionHeight, new Duration(TimeSpan.FromMilliseconds(150)))
                : new DoubleAnimation(NormalHeight, new Duration(TimeSpan.FromMilliseconds(150)));
            MachineOptionsGrid.Visibility = machineAdmission 
                ? Visibility.Visible
                : Visibility.Collapsed;

            BeginAnimation(HeightProperty, heightAnimation);
        }

        private void OnAddNewWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var admission = AdmissionsComboBox.SelectedItem as DataRowView;
            if (admission != null)
            {
                var selectedProfessions = _workerProfessionsTable.Select("IsSelected = TRUE");
                if(selectedProfessions.Any() && AdmissionDatePicker.SelectedDate.HasValue)
                {
                    var workerProfessionId = Convert.ToInt64(selectedProfessions.First()["WorkerProfessionID"]);
                    var admissionDate = AdmissionDatePicker.SelectedDate.Value;
                    var admissionId = Convert.ToInt32(admission["AdmissionID"]);
                    if (admissionId == AdmissionsClass.WorkSubsectionAdmissionId)
                    {
                        if(WorkSubsectionsComboBox.SelectedItem != null && ChoosenOperationsListBox.HasItems)
                        {
                            var workSubsectionId = Convert.ToInt64(WorkSubsectionsComboBox.SelectedValue);
                            long workerAdmissionId = -1;
                            try
                            {
                                workerAdmissionId = _admClass.AddWorkerAdmission(admissionId, _workerId, workerProfessionId, admissionDate);
                                var workerHasAdmissionsForWorkSubsection = _admClass.HasWorkerAdmissionsToWorkSubsection(_workerId, workerAdmissionId, workSubsectionId);
                                if (workerHasAdmissionsForWorkSubsection)
                                    _admClass.DeleteWorkOperationWorkerAdmissions(workerAdmissionId, workSubsectionId);

                                foreach (var workOperationRow in _choosenOperationsTable.AsEnumerable())
                                {
                                    var workOperationId = Convert.ToInt64(workOperationRow["WorkOperationID"]);
                                    _admClass.AddWorkOperationWorkerAdmission(workerAdmissionId, _workerId, workSubsectionId, workOperationId);
                                }

                                WorkerAdmissionsListBox.Items.Refresh();
                                OnGoBackButtonClick(null, null);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            _admClass.AddWorkerAdmission(admissionId, _workerId, workerProfessionId, admissionDate);
                            OnGoBackButtonClick(null, null);
                        }
                        catch(Exception exp)
                        {
                            MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        return;
                    }
                }
            }

            MessageBox.Show("Присутствуют пустые поля для заполнения. Информация не сохранена.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WorkSubsectionsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllowToWorkerCategoryCheckBox.IsChecked = false;

            var workSubsection = WorkSubsectionsComboBox.SelectedItem as DataRowView;
            if (workSubsection == null)
            {
                WorkOperationsListBox.ItemsSource = null;
                _choosenOperationsTable.Clear();
                return;
            }

            var workSubSectionId = Convert.ToInt64(workSubsection["WorkSubsectionID"]);
            var workOperations = _catalogClass.GetWorkOperations();
            workOperations.RowFilter = string.Format("WorkSubsectionID  IN ({0}, -1) AND Visible = True", workSubSectionId);
            var operationsCollectionView = new BindingListCollectionView(workOperations);

            if (operationsCollectionView.GroupDescriptions != null)
            {
                operationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("OperationTypeID",
                    new OperationTypeConverter()));
                operationsCollectionView.SortDescriptions.Add(new SortDescription("OperationTypeID",
                    ListSortDirection.Ascending));
            }

            WorkOperationsListBox.ItemsSource = operationsCollectionView;
            _choosenOperationsTable.Clear();
        }

        private void OnAddMachineOperationButtonClick(object sender, RoutedEventArgs e)
        {
            var workOperation = WorkOperationsListBox.SelectedItem as DataRowView;
            if (workOperation == null) return;

            var workOperationId = Convert.ToInt64(workOperation["WorkOperationID"]);
            if(_choosenOperationsTable.AsEnumerable().Any(r => r.Field<Int64>("WorkOperationID") == workOperationId))
            {
                MessageBox.Show("Данная операция уже присутствует в списке операций выбранных Вами.");
                return;
            }

            _choosenOperationsTable.Rows.Add(workOperation.Row.ItemArray);
        }

        private void OnRemoveMachineOperationButtonClick(object sender, RoutedEventArgs e)
        {
            var choosenWorkOperation = ChoosenOperationsListBox.SelectedItem as DataRowView;
            if (choosenWorkOperation == null) return;

            var choosenWorkOperationRow = choosenWorkOperation.Row;

            _choosenOperationsTable.Rows.Remove(choosenWorkOperationRow);
        }

        private void OnCancelSelectionWorkSubsectionAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            HideWorkSubsectionSelection();
        }

        private void ShowWorkSubsectionSelection(long workerAdmissionId, bool deleteSelection)
        {
            OpacityGrid.IsEnabled = false;
            ShadowGrid.Visibility = Visibility.Visible;

            if (deleteSelection)
            {
                ChangeWorkSubsectionWorkerAdmissionButton.Visibility = Visibility.Collapsed;
                DeleteWorkSubsectionWorkerAdmissionButton.Visibility = Visibility.Visible;

                WorkSubsectionSelectionChangeTextBlock.Visibility = Visibility.Collapsed;
                WorkSubsectionSelectionDeleteTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ChangeWorkSubsectionWorkerAdmissionButton.Visibility = Visibility.Visible;
                DeleteWorkSubsectionWorkerAdmissionButton.Visibility = Visibility.Collapsed;

                WorkSubsectionSelectionChangeTextBlock.Visibility = Visibility.Visible;
                WorkSubsectionSelectionDeleteTextBlock.Visibility = Visibility.Collapsed;
            }

            var workOperationWorkerAdmissions = _admClass.WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
            if (workOperationWorkerAdmissions.Any())
            {
                var workSubsectionIds = workOperationWorkerAdmissions.Select(r => r.Field<Int64>("WorkSubsectionID")).Distinct();
                WorkSubsectionSelectionListBox.ItemsSource = workSubsectionIds;
            }
            else
            {
                WorkSubsectionSelectionListBox.ItemsSource = null;
            }

            var widthAnimation = new DoubleAnimation(351d, new Duration(TimeSpan.FromMilliseconds(200)));
            WorkSubsectionSelectionGrid.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color { A = 20, R = 0, G = 0, B = 0 };
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void HideWorkSubsectionSelection()
        {
            OpacityGrid.IsEnabled = true;
            WorkSubsectionSelectionListBox.ItemsSource = null;

            var widthAnimation = new DoubleAnimation(0d, new Duration(TimeSpan.FromMilliseconds(200)));
            WorkSubsectionSelectionGrid.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color { A = 0, R = 0, G = 0, B = 0 };
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            colorAnimation.Completed += (s, args) => { ShadowGrid.Visibility = Visibility.Collapsed; };
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void OnChangeWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var workerAdmission = WorkerAdmissionsListBox.SelectedItem as DataRowView;
            if (workerAdmission == null) return;

            var workerAdmissionId = Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
            var selectedProfessions = _workerProfessionsTable.Select("IsSelected = TRUE");
            if (selectedProfessions.Any() && AdmissionDatePicker.SelectedDate.HasValue)
            {
                var workerProfessionId = Convert.ToInt64(selectedProfessions.First()["WorkerProfessionID"]);
                var admissionDate = AdmissionDatePicker.SelectedDate.Value;
                var admissionId = Convert.ToInt32(workerAdmission["AdmissionID"]);
                if (admissionId == AdmissionsClass.WorkSubsectionAdmissionId)
                {
                    if (WorkSubsectionsComboBox.SelectedItem != null && ChoosenOperationsListBox.HasItems)
                    {
                        var workSubsectionId = Convert.ToInt64(WorkSubsectionsComboBox.SelectedValue);
                        try
                        {
                            var workerHasAdmissionsForWorkSubsection = _admClass.HasWorkerAdmissionsToWorkSubsection(_workerId, workerAdmissionId, workSubsectionId);
                            if (workerHasAdmissionsForWorkSubsection)
                                _admClass.DeleteWorkOperationWorkerAdmissions(workerAdmissionId, workSubsectionId);

                            foreach (var workOperationRow in _choosenOperationsTable.AsEnumerable())
                            {
                                var workOperationId = Convert.ToInt64(workOperationRow["WorkOperationID"]);
                                _admClass.AddWorkOperationWorkerAdmission(workerAdmissionId, _workerId, workSubsectionId, workOperationId);
                            }

                            WorkerAdmissionsListBox.Items.Refresh();
                            OnGoBackButtonClick(null, null);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        return;
                    }
                }
                else
                {
                    try
                    {
                        _admClass.ChangeWorkerAdmission(workerAdmissionId, workerProfessionId, admissionDate);
                        WorkerAdmissionsListBox.Items.Refresh();
                        OnGoBackButtonClick(null, null);
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    return;
                }
            }

            MessageBox.Show("Присутствуют пустые поля для заполнения. Информация не сохранена.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Information);

        }



        private DataView WorkUnitGroupFilter(int groupId)
        {
            var workUnitByGroup =
                    (_catalogClass.WorkUnitsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("WorkerGroupID") == groupId));

            if (workUnitByGroup.Count() != 0)
            {
                var wuNamesDT = workUnitByGroup.CopyToDataTable();
                return wuNamesDT.DefaultView;
            }
            return null;
        }

        private DataView WorkUnitGroupFilter(int groupId, int factoryID)
        {
            var workUnitByGroup =
                (_catalogClass.WorkUnitsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerGroupID") == groupId).Where(r => r.Field<Int64>("FactoryID") == factoryID));

            if (workUnitByGroup.Count() != 0)
            {
                var wuNamesDT = workUnitByGroup.CopyToDataTable();
                return wuNamesDT.DefaultView;
            }
            return null;
        }

        private DataView WorkSectionUnitFilter(int unitID)
        {
            var workSectionByUnit =
                (_catalogClass.WorkSectionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkUnitID") == unitID));

            if (workSectionByUnit.Count() != 0)
            {
                var wSectionDT = workSectionByUnit.CopyToDataTable();
                return wSectionDT.DefaultView;
            }
            return null;
        }

        private DataView WorkSubSectionFilter(int workSectionID)
        {
            var workSubSectionsBySection =
                (_catalogClass.WorkSubsectionsDataTable.AsEnumerable().Where(
                    r => r.Field<bool>("Visible") && r.Field<Int64>("WorkSectionID") == workSectionID));

            if (workSubSectionsBySection.Count() != 0)
            {
                var wSubSectionDT = workSubSectionsBySection.CopyToDataTable();
                return wSubSectionDT.DefaultView;
            }
            return null;
        }

        private void OnWorkGroupsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkGroupsComboBox.Items.Count == 0 || WorkGroupsComboBox.SelectedValue == null) return;

            _unitToFactory = Convert.ToBoolean(((DataRowView)WorkGroupsComboBox.SelectedItem).Row["UnitToFactory"]);

            if (!_unitToFactory)
            {
                FactoriesComboBox.IsEnabled = false;
                WorkUnitsComboBox.ItemsSource = WorkUnitGroupFilter(Convert.ToInt32(WorkGroupsComboBox.SelectedValue));
                WorkUnitsComboBox.SelectedIndex = 0;
            }
            else
            {
                FactoriesComboBox.IsEnabled = true;
                FactoriesComboBox.SelectedIndex = 0;
                OnFactoriesComboBoxSelectionChanged(null, null);
            }
        }

        private void OnFactoriesComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoriesComboBox.Items.Count == 0 || FactoriesComboBox.SelectedValue == null) return;

            WorkUnitsComboBox.ItemsSource = WorkUnitGroupFilter(Convert.ToInt32(WorkGroupsComboBox.SelectedValue),
                                                                Convert.ToInt32(FactoriesComboBox.SelectedValue));
            WorkUnitsComboBox.SelectedIndex = 0;
        }

        private void OnWorkUnitsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsComboBox.Items.Count == 0 || WorkUnitsComboBox.SelectedValue == null) return;

            WorkSectionsComboBox.ItemsSource = WorkSectionUnitFilter(Convert.ToInt32(WorkUnitsComboBox.SelectedValue));
            WorkSectionsComboBox.SelectedIndex = 0;
        }

        private void OnWorkSectionsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSectionsComboBox.Items.Count == 0 || WorkSectionsComboBox.SelectedValue == null) return;

            WorkSubsectionsComboBox.ItemsSource =
                WorkSubSectionFilter(Convert.ToInt32(WorkSectionsComboBox.SelectedValue));
            WorkSubsectionsComboBox.SelectedIndex = 0;
        }



        private void OnAllowToWorkerCategoryCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            _choosenOperationsTable.Clear();

            var workSubsection = WorkSubsectionsComboBox.SelectedItem as DataRowView;
            if (workSubsection == null) return;

            var workerProfessions = _workerProfessionsTable.AsEnumerable().Where(r => r.Field<bool>("IsSelected"));
            if (!workerProfessions.Any()) return;
            var workerProfession = workerProfessions.First();
            var workerCategory = Convert.ToInt32(workerProfession["Category"]);

            var workSubsectionId = Convert.ToInt64(workSubsection["WorkSubsectionID"]);
            var workOperations = _catalogClass.WorkOperationsDataTable.Select(string.Format("WorkSubsectionID = {0} AND Visible = TRUE", workSubsectionId));
            if (!workOperations.Any()) return;

            var machineOperations = _catalogClass.MachinesOperationsDataTable.AsEnumerable().
                Where(mO => workOperations.Any(wO => wO.Field<Int64>("WorkOperationID") == mO.Field<Int64>("WorkOperationID")));
            if (!machineOperations.Any()) return;

            foreach (var machineOperation in machineOperations)
            {
                if (machineOperation["MinWorkerCategory"] == DBNull.Value || string.IsNullOrEmpty(machineOperation["MinWorkerCategory"].ToString()))
                {
                    var workOperationId = Convert.ToInt64(machineOperation["WorkOperationID"]);
                    var workOperation = workOperations.First(wO => wO.Field<Int64>("WorkOperationID") == workOperationId);
                    _choosenOperationsTable.Rows.Add(workOperation.ItemArray);
                }
                else if (machineOperation["MinWorkerCategory"] != DBNull.Value)
                {
                    var workOperationId = Convert.ToInt64(machineOperation["WorkOperationID"]);
                    var workOperation = workOperations.First(wO => wO.Field<Int64>("WorkOperationID") == workOperationId);

                    int minWorkerCategory = 2;
                    var result = int.TryParse(machineOperation["MinWorkerCategory"].ToString(), out minWorkerCategory);
                    if(result)
                    {
                        if(minWorkerCategory <= workerCategory)
                            _choosenOperationsTable.Rows.Add(workOperation.ItemArray);
                    }
                    else
                    {
                        _choosenOperationsTable.Rows.Add(workOperation.ItemArray);
                    }
                }
            }
        }

        private void OnDeleteWorkerAdmissionButton(object sender, RoutedEventArgs e)
        {
            var workerAdmission = WorkerAdmissionsListBox.SelectedItem as DataRowView;
            if (workerAdmission == null) return;

            var workerAdmissionId = Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
            var admissionId = Convert.ToInt32(workerAdmission["AdmissionID"]);
            if (admissionId == AdmissionsClass.WorkSubsectionAdmissionId && _admClass.HasWorkerAdmissionsToWorkOperations(_workerId))
            {
                ShowWorkSubsectionSelection(workerAdmissionId, true);
                return;
            }

            if (MessageBox.Show("Вы действительно хотите удалить данный допуск?", "Удаление", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                _admClass.DeleteWorkerAdmission(workerAdmissionId);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnChangeWorkSubsectionWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            if (WorkSubsectionSelectionListBox.SelectedItem == null) return;
            var workerAdmission = WorkerAdmissionsListBox.SelectedItem as DataRowView;
            if (workerAdmission == null) return;

            var admissionId = Convert.ToInt32(workerAdmission["AdmissionID"]);
            if (admissionId != AdmissionsClass.WorkSubsectionAdmissionId) return;

            var workerAdmissionId = Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
            var workSubsectionId = Convert.ToInt64(WorkSubsectionSelectionListBox.SelectedItem);
            var admissionDate = Convert.ToDateTime(workerAdmission["AdmissionDate"]);
            var workerProfessionId = Convert.ToInt64(workerAdmission["WorkerProfessionID"]);

            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.Visibility = Visibility.Visible;
                AddNewWorkerAdmissionButton.Visibility = Visibility.Collapsed;
                ChangeWorkerAdmissionButton.Visibility = Visibility.Visible;
                RedactorGrid.DataContext = workerAdmission;

                AdmissionsComboBox.IsEnabled = false;
                if(AdmissionsComboBox.HasItems)
                {
                    AdmissionsComboBox.SelectionChanged -= OnAdmissionsComboBoxSelectionChanged;
                    AdmissionsComboBox.SelectedValue = admissionId;
                    AdmissionsComboBox.SelectionChanged += OnAdmissionsComboBoxSelectionChanged;
                    OnAdmissionsComboBoxSelectionChanged(null, null);
                }

                AdmissionDatePicker.SelectedDate = admissionDate;
                var workerProfessions = _workerProfessionsTable.Select(string.Format("WorkerProfessionID = {0}", workerProfessionId));
                if (workerProfessions.Any())
                    workerProfessions.First()["IsSelected"] = true;

                BindingComboBoxesByWorkSubsectionId(workSubsectionId);
                WorkGroupPanel.IsEnabled = false;
                WorkUnitPanel.IsEnabled = false;
                WorkSubsectionsComboBox.IsEnabled = false;

                _choosenOperationsTable.Clear();
                var workSubsectionAdmissions = 
                    _admClass.WorkOperationWorkerAdmissionsTable.Select(string.Format("WorkerAdmissionID = {0}", workerAdmissionId));
                if (workSubsectionAdmissions.Any())
                {
                    var workOperations =
                        _catalogClass.WorkOperationsDataTable.AsEnumerable().
                        Where(r => workSubsectionAdmissions.Where(wA => wA.Field<Int64>("WorkSubsectionID") == workSubsectionId).
                        Any(wA => wA.Field<Int64>("WorkOperationID") == r.Field<Int64>("WorkOperationID")));

                    foreach(var workOperation in workOperations)
                        _choosenOperationsTable.Rows.Add(workOperation.ItemArray);
                }

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            HideWorkSubsectionSelection();
        }

        private void BindingComboBoxesByWorkSubsectionId(long workSubsectionId)
        {
            var workSubsections = _catalogClass.WorkSubsectionsDataTable.AsEnumerable().
                Where(s => s.Field<Int64>("WorkSubsectionID") == workSubsectionId);
            if (!workSubsections.Any()) return;

            var workSectionId = Convert.ToInt64(workSubsections.First()["WorkSectionID"]);
            var workSections = _catalogClass.WorkSectionsDataTable.AsEnumerable().
                Where(s => s.Field<Int64>("WorkSectionID") == workSectionId);
            if (!workSections.Any()) return;

            var workUnitId = Convert.ToInt64(workSections.First()["WorkUnitID"]);
            var workUnits = _catalogClass.WorkUnitsDataTable.AsEnumerable().
                Where(u => u.Field<Int64>("WorkUnitID") == workUnitId);
            if (!workUnits.Any()) return;

            var workGroupId = Convert.ToInt64(workUnits.First()["WorkerGroupID"]);
            var workerGroups = _catalogClass.WorkerGroupsDataTable.AsEnumerable().
                Where(g => g.Field<Int64>("WorkerGroupID") == workGroupId);
            if (!workerGroups.Any()) return;

            long factoryId = 1;
            var unitToFactory = Convert.ToBoolean(workerGroups.First()["UnitToFactory"]);
            if(unitToFactory)
            {
                factoryId = Convert.ToInt64(workUnits.First()["FactoryID"]);
            }


            if (WorkGroupsComboBox.HasItems)
                WorkGroupsComboBox.SelectedValue = workGroupId;

            if (FactoriesComboBox.HasItems)
            {
                FactoriesComboBox.SelectionChanged -= OnFactoriesComboBoxSelectionChanged;
                FactoriesComboBox.SelectedValue = factoryId;
                FactoriesComboBox.SelectionChanged += OnFactoriesComboBoxSelectionChanged;
                OnFactoriesComboBoxSelectionChanged(null, null);
            }

            if (WorkUnitsComboBox.HasItems)
            {
                WorkUnitsComboBox.SelectionChanged -= OnWorkUnitsComboBoxSelectionChanged;
                WorkUnitsComboBox.SelectedValue = workUnitId;
                WorkUnitsComboBox.SelectionChanged += OnWorkUnitsComboBoxSelectionChanged;
                OnWorkUnitsComboBoxSelectionChanged(null, null);
            }

            if (WorkSectionsComboBox.HasItems)
            {
                WorkSectionsComboBox.SelectionChanged -= OnWorkSectionsComboBoxSelectionChanged;
                WorkSectionsComboBox.SelectedValue = workSectionId;
                WorkSectionsComboBox.SelectionChanged += OnWorkSectionsComboBoxSelectionChanged;
                OnWorkSectionsComboBoxSelectionChanged(null, null);
            }

            if (WorkSubsectionsComboBox.HasItems)
            {
                WorkSubsectionsComboBox.SelectedValue = workSubsectionId;
            }
        }

        private void OnDeleteWorkSubsectionWorkerAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            if (WorkSubsectionSelectionListBox.SelectedItem == null) return;
            var workerAdmission = WorkerAdmissionsListBox.SelectedItem as DataRowView;
            if (workerAdmission == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить допуск на данное оборудование/станок?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var workerAdmissionId = Convert.ToInt64(workerAdmission["WorkerAdmissionID"]);
            var workSubsectionId = Convert.ToInt64(WorkSubsectionSelectionListBox.SelectedItem);

            try
            {
                _admClass.DeleteWorkOperationWorkerAdmissions(workerAdmissionId, workSubsectionId);

                if (!_admClass.HasWorkerAdmissionsToWorkOperations(_workerId))
                    _admClass.DeleteWorkerAdmission(workerAdmissionId);

                WorkerAdmissionsListBox.Items.Refresh();
                HideWorkSubsectionSelection();
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
