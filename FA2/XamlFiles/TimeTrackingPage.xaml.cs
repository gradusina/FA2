using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.Converters;
using FA2.Notifications;
using FAIIControlLibrary.CustomControls;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для TimeTrackingPage.xaml
    /// </summary>
    public partial class TimeTrackingPage
    {
        private StaffClass _sc;

        private bool _firstRun = true;

        private CatalogClass _cc;
        private int _workerGroupId;

        private TimeTrackingClass _ttc;

        private TaskClass _taskClass;


        private double _currentProductivity;

        private string _currentMeasureUnitName = string.Empty;

        private double _currentVclp;

        public TimeTrackingPage()
        {
            InitializeComponent();
        }

        private void GetClasses()
        {
            App.BaseClass.GetTimeTrackingClass(ref _ttc);
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetCatalogClass(ref _cc);
            App.BaseClass.GetTaskClass(ref _taskClass);
        }


        private void BackgroundWorker_RunWorkerCompleted()
        {
            BindingData();

            TimeTrackingDataGridSettings();

            if (AdministrationClass.HasFullAccess(AdministrationClass.Modules.TimeTracking))
            {
                BrigadeToggleButton.Visibility = Visibility.Visible;
                BindingWorkersData();
            }
            else
                BrigadeToggleButton.Visibility = Visibility.Hidden;

            BindingStudentsData();

            FillTasksData();
            BindingTasksData();

            WorkersGroupsComboBox_SelectionChanged(null, null);

            _firstRun = false;

            TimeTrackingDataGrid.ItemsSource = _ttc.GetTimeTracking(AdministrationClass.CurrentWorkerId, _ttc.CurrentWorkerStartDate);

            TotalTimeLabel.Content = _ttc.CountingTotalTime();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null) mainWindow.HideWaitAnnimation();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.TimeTracking);
            NotificationManager.ClearNotifications(AdministrationClass.Modules.TimeTracking);

            if (_firstRun)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (o, args) => GetClasses();

                backgroundWorker.RunWorkerCompleted += (o, args) => BackgroundWorker_RunWorkerCompleted();

                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                FillTasksData();
                BindingTasksData();
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }
        }

        private void FillTasksData()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();

            _taskClass.Fill(_ttc.CurrentWorkerStartDate.Date, currentDate.AddDays(1),
                AdministrationClass.CurrentWorkerId);
        }

        private void BindingData()
        {
            #region OperationsCatalog

            WorkersGroupsComboBox.SelectionChanged -= WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            WorkersGroupsComboBox.ItemsSource = _cc.GetWorkersGroups();
            
            WorkersGroupsComboBox.SelectionChanged += WorkersGroupsComboBox_SelectionChanged;
            
            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.DisplayMemberPath = "FactoryName";
            FactoriesComboBox.SelectedValuePath = "FactoryID";
            FactoriesComboBox.ItemsSource = _cc.GetFactories();
            FactoriesComboBox.SelectedIndex = 0;
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

            WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;
            WorkUnitsListBox.SelectedValuePath = "WorkUnitID";
            WorkUnitsListBox.ItemsSource = _cc.GetWorkUnits();
            WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;
            
            WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;
            WorkSectionsListBox.SelectedValuePath = "WorkSectionID";
            WorkSectionsListBox.ItemsSource = _cc.GetWorkSections();
            WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;
            
            WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;
            WorkSubSectionsListBox.SelectedValuePath = "WorkSubsectionID";
            WorkSubSectionsListBox.ItemsSource = _cc.GetWorkSubsections();
            WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;
            
            OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
            OperationsListBox.SelectedValuePath = "WorkOperationID";
            OperationsListBox.ItemsSource = _cc.GetWorkOperations();
            OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

            WorkersGroupsComboBox.SelectedIndex = 1;

            DoubleTimeSet.TotalTimeChanged += DoubleTimeSet_TotalTimeChanged;

            WorkersGroupsComboBox_SelectionChanged(null, null);

            #endregion
        }

        private void BindingStudentsData()
        {
            StFactoriesComboBox.SelectionChanged -= StFactoriesComboBox_SelectionChanged;
            StFactoriesComboBox.DisplayMemberPath = "FactoryName";
            StFactoriesComboBox.SelectedValuePath = "FactoryID";
            StFactoriesComboBox.ItemsSource = _sc.GetFactories();
            StFactoriesComboBox.SelectedIndex = 0;
            StFactoriesComboBox.SelectionChanged += StFactoriesComboBox_SelectionChanged;

            StudentsNamesListBox.SelectionChanged -= StudentsNamesListBox_SelectionChanged;
            StudentsNamesListBox.ItemsSource = _sc.GetStaffPersonalInfo();
            StudentsNamesListBox.SelectedValuePath = "WorkerID";
            StudentsNamesListBox.SelectionChanged += StudentsNamesListBox_SelectionChanged;

            StWorkersGroupsComboBox.SelectionChanged -= StWorkersGroupsComboBox_SelectionChanged;
            StWorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            StWorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            StWorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            StWorkersGroupsComboBox.SelectedValue = 2;
            StWorkersGroupsComboBox.SelectionChanged += StWorkersGroupsComboBox_SelectionChanged;
            StWorkersGroupsComboBox_SelectionChanged(null, null);
        }

        private void BindingTasksData()
        {
            var workerTasksTable = new DataTable();
            workerTasksTable.Columns.Add("TaskID", typeof (long));
            workerTasksTable.Columns.Add("PerformerID", typeof (long));
            workerTasksTable.Columns.Add("TaskName", typeof (string));
            workerTasksTable.Columns.Add("Description", typeof (string));
            workerTasksTable.Columns.Add("GlobalID", typeof (string));
            workerTasksTable.Columns.Add("SenderAppID", typeof (long));
            workerTasksTable.Columns.Add("TaskStatusID", typeof (long));
            workerTasksTable.Columns.Add("MainWorkerID", typeof (long));
            workerTasksTable.Columns.Add("TaskCreationDate", typeof (DateTime));
            workerTasksTable.Columns.Add("TaskCompletionDate", typeof (DateTime));
            workerTasksTable.Columns.Add("StartDate", typeof (DateTime));
            workerTasksTable.Columns.Add("CompletionDate", typeof(DateTime));
            workerTasksTable.Columns.Add("IsComplete", typeof (bool));

            foreach (
                var resultRow in
                    _taskClass.Performers.Table.AsEnumerable()
                        .Where(p => p.Field<Int64>("WorkerID") == AdministrationClass.CurrentWorkerId)
                        .Join(_taskClass.Tasks.Table.AsEnumerable(),
                            performer => performer.Field<Int64>("TaskID"), task => task.Field<Int64>("TaskID"),
                            (performer, task) =>
                            {
                                var newRow = workerTasksTable.NewRow();
                                newRow["TaskID"] = task["TaskID"];
                                newRow["PerformerID"] = performer["PerformerID"];
                                newRow["TaskName"] = task["TaskName"];
                                newRow["Description"] = task["Description"];
                                newRow["GlobalID"] = task["GlobalID"];
                                newRow["SenderAppID"] = task["SenderAppID"];
                                newRow["TaskStatusID"] = performer["TaskStatusID"];
                                newRow["MainWorkerID"] = task["MainWorkerID"];
                                newRow["TaskCreationDate"] = task["CreationDate"];
                                newRow["TaskCompletionDate"] = task["CompletionDate"];
                                newRow["StartDate"] = performer["StartDate"];
                                newRow["CompletionDate"] = performer["CompletionDate"];
                                newRow["IsComplete"] = performer["IsComplete"];
                                return newRow;
                            }).Where(newRow =>
                                //(newRow.Field<object>("StartDate") != null &&
                                // newRow.Field<DateTime>("StartDate") >= _taskClass.GetDateFrom() &&
                                // newRow.Field<DateTime>("StartDate") <= _taskClass.GetDateTo())
                                //||
                                (newRow.Field<object>("CompletionDate") != null &&
                                 newRow.Field<DateTime>("CompletionDate") >= _taskClass.GetDateFrom() &&
                                 newRow.Field<DateTime>("CompletionDate") <= _taskClass.GetDateTo())
                                || !newRow.Field<bool>("IsComplete")))
            {
                workerTasksTable.Rows.Add(resultRow);
            }

            var workerTasksView = workerTasksTable.AsDataView();
            workerTasksView.Sort = "TaskStatusID, TaskCreationDate";
            TasksDataGrid.ItemsSource = workerTasksView;

            TasksTabItem.Header = workerTasksView.Count;
        }

        private void OpacityAnimation(UIElement control, bool visible)
        {
            if ((control.Opacity == 1) && visible) return;
            if ((control.Opacity == 0) && !visible) return;

            var da = new DoubleAnimation {From = visible ? 0 : 1, To = visible ? 1 : 0};

            da.Completed += (sender, e) => control.IsEnabled = visible;

            da.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            control.BeginAnimation(OpacityProperty, da);
        }

        private void WorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersGroupsComboBox.Items.Count == 0) return;

            var unitToFactory = ((DataRowView) WorkersGroupsComboBox.SelectedItem).Row["UnitToFactory"].ToString();

            OpacityAnimation(FactoryStackPanel, unitToFactory != string.Empty && Convert.ToBoolean(unitToFactory));

            _workerGroupId = Convert.ToInt32(WorkersGroupsComboBox.SelectedValue) - 1;

            if (_workerGroupId > _cc.WorkersGroupsTitlesDataTable.Rows.Count)
                _workerGroupId = 2;

            WorkUnitsLabel.Content = _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["UnitsTitle"].ToString();
            WorkSectionsLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["SectionsTitle"].ToString();
            WorkSubSectionsLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["SubsectionsTitle"].ToString();
            WorkOperationsNameLabel.Content =
                _cc.WorkersGroupsTitlesDataTable.Rows[_workerGroupId]["OperationsTitle"].ToString();




            if (!Convert.ToBoolean(unitToFactory))
            {
                WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;

                ((DataView) (WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                                        WorkersGroupsComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
                CountUnitLabel.Content = ((DataView) (WorkUnitsListBox.ItemsSource)).Count;
            }

            else
            {


                FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
                FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

                if (FactoriesComboBox.Items.Count == 0) return;

                ((DataView) (WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                                        WorkersGroupsComboBox.SelectedValue +
                                                                        " AND FactoryID = " +
                                                                        FactoriesComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
            }

            CountUnitLabel.Content = ((DataView) (WorkUnitsListBox.ItemsSource)).Count;
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var unitToFactory = ((DataRowView) WorkersGroupsComboBox.SelectedItem).Row["UnitToFactory"].ToString();
            
            if (Convert.ToBoolean(unitToFactory))
            {
                WorkUnitsListBox.SelectionChanged -= WorkUnitsListBox_SelectionChanged;

                FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
                FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

                if (FactoriesComboBox.Items.Count == 0) return;

                ((DataView)(WorkUnitsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkerGroupID = " +
                                                       WorkersGroupsComboBox.SelectedValue + " AND FactoryID = " +
                                                       FactoriesComboBox.SelectedValue;

                WorkUnitsListBox.SelectedIndex = 0;
                WorkUnitsListBox.SelectionChanged += WorkUnitsListBox_SelectionChanged;

                WorkUnitsListBox_SelectionChanged(null, null);
            }

            CountUnitLabel.Content = ((DataView)(WorkUnitsListBox.ItemsSource)).Count;
        }

        private void WorkUnitsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsListBox.Items.Count != 0 && WorkUnitsListBox.SelectedItem != null)
            {
                WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;

                ((DataView)(WorkSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkUnitID=" +
                                                                           WorkUnitsListBox.SelectedValue;

                WorkSectionsListBox.SelectedIndex = 0;
                WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;
            }
            else
            {
                WorkSectionsListBox.SelectionChanged -= WorkSectionsListBox_SelectionChanged;

                ((DataView)(WorkSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkUnitID=" + -1;

                WorkSectionsListBox.SelectedIndex = 0;
                WorkSectionsListBox.SelectionChanged += WorkSectionsListBox_SelectionChanged;
            }

            CountSectionsLabel.Content = ((DataView)(WorkSectionsListBox.ItemsSource)).Count;

            WorkSectionsListBox_SelectionChanged(null, null);
        }

        private void WorkSectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSectionsListBox.Items.Count != 0 && WorkSectionsListBox.SelectedItem != null)
            {
                WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;

                ((DataView) (WorkSubSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkSectionID=" +
                                                                              WorkSectionsListBox.SelectedValue;

                WorkSubSectionsListBox.SelectedIndex = 0;
                WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;
            }
            else
            {
                WorkSubSectionsListBox.SelectionChanged -= WorkSubSectionsListBox_SelectionChanged;

                ((DataView)(WorkSubSectionsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkSectionID=" + -1;

                WorkSubSectionsListBox.SelectedIndex = 0;
                WorkSubSectionsListBox.SelectionChanged += WorkSubSectionsListBox_SelectionChanged;
            }

            //if (((DataView)(WorkSubSectionsListBox.ItemsSource)).Count != 0) WorkSubSectionsListBox.SelectedIndex = 0;
            CountSubSectionsLabel.Content = ((DataView) (WorkSubSectionsListBox.ItemsSource)).Count;

            WorkSubSectionsListBox_SelectionChanged(null, null);
        }

        private void WorkSubSectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;

            if (WorkSubSectionsListBox.SelectedItem == null || WorkSubSectionsListBox.SelectedValue == null)
            {
                OperationsListBox.ItemsSource = null;

                CountOperationsLabel.Content = 0;

                OperationsListBox_SelectionChanged(null, null);

                return;
            }


            //((DataView) (OperationsListBox.ItemsSource)).RowFilter = "Visible = 'True'" + " AND WorkSubsectionID=" +
            //                                                             WorkSubSectionsListBox.SelectedValue;


            DataView operationsDataView = _cc.GetWorkOperations();

            bool showAditOperations;

            Boolean.TryParse((((DataRowView) WorkSubSectionsListBox.SelectedItem)["HasAdditOperations"]).ToString(),
                out showAditOperations);


            if (showAditOperations)
            {
                operationsDataView.RowFilter = "WorkSubsectionID  IN (" +
                                               Convert.ToInt32(WorkSubSectionsListBox.SelectedValue) +
                                               ", -1) AND Visible = 'True'";


                var operationsCollectionView =
                    new BindingListCollectionView(operationsDataView);

                if (operationsCollectionView.GroupDescriptions != null)
                {
                    operationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("OperationTypeID",
                        new OperationTypeConverter()));
                    operationsCollectionView.SortDescriptions.Add(new SortDescription("OperationTypeID",
                        ListSortDirection.Ascending));

                    OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
                    OperationsListBox.ItemsSource = operationsCollectionView;
                    OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;
                }
            }
            else
            {
                operationsDataView.RowFilter = "WorkSubsectionID =" +
                                               Convert.ToInt32(WorkSubSectionsListBox.SelectedValue) +
                                               " AND Visible = 'True'";

                OperationsListBox.SelectionChanged -= OperationsListBox_SelectionChanged;
                OperationsListBox.ItemsSource = operationsDataView;
                OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

                OperationsListBox.Items.Refresh();
            }

            if (Convert.ToInt32(((DataRowView) WorkSubSectionsListBox.SelectedItem).Row["SubsectionGroupID"]) == 2)
            {
                NormBorder.Height = 22;
                WorkScopeUpDownControl.IsEnabled = true;

            }
            else
            {
                NormBorder.Height = 0;
                WorkScopeUpDownControl.IsEnabled = false;
                WorkScopeUpDownControl.Value = 0;
                VCLPLabel.Content = 0;
            }

            OperationsListBox.SelectedIndex = 0;

            OperationsListBox.SelectionChanged += OperationsListBox_SelectionChanged;

            CountOperationsLabel.Content = OperationsListBox.Items.Count;

            OperationsListBox_SelectionChanged(null, null);
        }

        private void OperationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationsListBox.SelectedValue == null || OperationsListBox.Items.Count == 0 ||
                OperationsListBox.SelectedItem == null)
                return;

            int currenrWorkOperationId = Convert.ToInt32(OperationsListBox.SelectedValue);

            var custView = new DataView(_cc.MachinesOperationsDataTable, "", "WorkOperationID", DataViewRowState.CurrentRows);
            var foundRows = custView.FindRows(currenrWorkOperationId);


            if (foundRows.Any())
            {
                var result = Double.TryParse(foundRows[0].Row["Productivity"].ToString(), out _currentProductivity);
                if (result)
                {
                    _currentMeasureUnitName = (new IdToMeasureUnitNameConverter()).Convert(OperationsListBox.SelectedValue);
                    NormLabel.Content = _currentProductivity + " " + _currentMeasureUnitName;
                    MeasureUnitNameLabel.Content = _currentMeasureUnitName;
                    _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
                    VCLPLabel.Content = _currentVclp;
                    return;
                }
            }

            _currentProductivity = 0;
            _currentVclp = 0;
            NormLabel.Content = string.Empty;
            MeasureUnitNameLabel.Content = string.Empty;
            VCLPLabel.Content = _currentVclp;
        }

        private double GetVCLP(decimal workScope, double currentProductivity, double totalHours)
        {
            double vclp;

            if (workScope == -1 || currentProductivity == -1)
            {
                return 0;
            }

            if (currentProductivity != 0 && totalHours != 0)
            {
                vclp = Convert.ToDouble(workScope) /
                              (currentProductivity * totalHours);
            }
            else
            {
                vclp = 0;
            }

            return Math.Round(vclp, 3);
        }

        private void DoubleTimeSet_TotalTimeChanged(object sender, RoutedEventArgs e)
        {
            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
            VCLPLabel.Content = _currentVclp;
        }

        private void AddRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationsListBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать операцию!", "Внимание", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int operationGroupID =
                Convert.ToInt32(((DataRowView) OperationsListBox.SelectedItem)["OperationGroupID"]);

            int operationTypeID =
                Convert.ToInt32(((DataRowView) OperationsListBox.SelectedItem)["OperationTypeID"]);

            if ((DoubleTimeSet.startTime != TimeSpan.Zero) && (DoubleTimeSet.stopTime != TimeSpan.Zero))
            {
                var workStatusID = 1;
                if (StudentsNamesListBox.SelectedItems.Count != 0)
                    workStatusID = 2;

                if (WorkersNamesListBox.SelectedItems.Count != 0)
                {
                    ShowFillBrigadeWorkScopeGrid();
                    return;
                }

               // OperationGroupID, @OperationTypeID




                _ttc.AddNewTimeRecord(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                    Convert.ToInt32(FactoriesComboBox.SelectedValue),
                    Convert.ToInt32(WorkUnitsListBox.SelectedValue),
                    Convert.ToInt32(WorkSectionsListBox.SelectedValue),
                    Convert.ToInt32(WorkSubSectionsListBox.SelectedValue),
                    Convert.ToInt32(OperationsListBox.SelectedValue), operationGroupID, operationTypeID,
                    DoubleTimeSet.startTime, DoubleTimeSet.stopTime,
                    Convert.ToDecimal(WorkScopeUpDownControl.Value),
                    Convert.ToDouble(_currentVclp), NotesPopUpTextBox.Text,
                    workStatusID, -1, -1);
                AdministrationClass.AddNewAction(59);


                if (StudentsNamesListBox.SelectedItems.Count != 0)
                {
                    foreach (var item in StudentsNamesListBox.SelectedItems)
                    {


                        _ttc.AddNewTimeRecord(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                            Convert.ToInt32(FactoriesComboBox.SelectedValue),
                            Convert.ToInt32(WorkUnitsListBox.SelectedValue),
                            Convert.ToInt32(WorkSectionsListBox.SelectedValue),
                            Convert.ToInt32(WorkSubSectionsListBox.SelectedValue),
                            Convert.ToInt32(OperationsListBox.SelectedValue), operationGroupID, operationTypeID,
                            DoubleTimeSet.startTime, DoubleTimeSet.stopTime, 
                            Convert.ToDecimal(WorkScopeUpDownControl.Value),
                            Convert.ToDouble(_currentVclp),
                            NotesPopUpTextBox.Text, 3,
                            Convert.ToInt32(((DataRowView)item).Row["WorkerId"]), AdministrationClass.CurrentWorkerId);
                    }
                    StudentsNamesListBox.SelectedItems.Clear();
                }

                NotesPopUpTextBox.Text = string.Empty;
                WorkScopeUpDownControl.Value = 0;
                WorkScopeUpDownControl_PreviewKeyUp(null, null);
                TotalTimeLabel.Content =_ttc.CountingTotalTime(); 
            }
            else
            {
                MessageBox.Show("Необходимо ввести интервал времени!", "Внимание", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void ShowFillBrigadeWorkScopeGrid()
        {
            BrigadierNameCheckBox.Content = (new IdToNameConverter()).Convert(AdministrationClass.CurrentWorkerId, "ShortName");
            BrigadierWorkScopeUpDownControl.Value = WorkScopeUpDownControl.Value;
            BrigadierMeasureUnitsLabel.Content = _currentMeasureUnitName;
            BrigadeMeasureUnitsLabel.Content = _currentMeasureUnitName;
            BrigadierVCLPLabel.Content = _currentVclp;

            WorkersWorkScopeGrid.Visibility = Visibility.Visible;

            foreach (var item in WorkersNamesListBox.SelectedItems)
            {
                int workerID = Convert.ToInt32(((DataRowView)item).Row["WorkerId"]);

                var nameChkBox = new CheckBox
                {
                    Name = "Name_" + workerID,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsChecked = true,
                    Content = (new IdToNameConverter()).Convert(workerID, "ShortName"),
                    FontSize = 16,
                    Margin = new Thickness(2),
                    Height = 31,
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#FF444444")
                };

                nameChkBox.Checked += nameChkBox_Checked;
                nameChkBox.Unchecked += nameChkBox_Unchecked;

                WorkersWSStackPanel.Children.Add(nameChkBox);

                var workScopeUpDownControl = new NumericControl
                {
                    Name = "WorkScope_" + workerID,
                    Height = 31,
                    Margin = new Thickness(2),
                };
                workScopeUpDownControl.PreviewKeyUp += workScopeUpDownControl_PreviewKeyUp;

                WorkScopeWSStackPanel.RegisterName(workScopeUpDownControl.Name, workScopeUpDownControl);
                
                WorkScopeWSStackPanel.Children.Add(workScopeUpDownControl);

                var measureUnitslbl = new Label
                {
                    Content = _currentMeasureUnitName,
                    FontSize = 12,
                    Margin = new Thickness(2),
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#FF444444"),
                    Height = 31,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                MeasureUnitsWSStackPanel.Children.Add(measureUnitslbl);

                var vclpLbl = new Label
                {
                    Name = "VCLP_" + workerID,
                    ContentStringFormat = "КТУ: {0}",
                    Content = 0,
                    FontSize = 16,
                    Margin = new Thickness(2),
                    Foreground = (Brush) new BrushConverter().ConvertFrom("#FFD84A35")
                };
                VCLPWSStackPanel.RegisterName(vclpLbl.Name, vclpLbl);

                VCLPWSStackPanel.Children.Add(vclpLbl);
            }
        }

        void nameChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var nameChkBox = (CheckBox)sender;

            int index = WorkersWSStackPanel.Children.IndexOf(nameChkBox);

            ((NumericControl)WorkScopeWSStackPanel.Children[index]).Value = 0;
            WorkScopeWSStackPanel.Children[index].IsEnabled = false;

            ((Label) VCLPWSStackPanel.Children[index]).Content = 0;
        }

        void nameChkBox_Checked(object sender, RoutedEventArgs e)
        {
            var nameChkBox = (CheckBox)sender;
            int index = WorkersWSStackPanel.Children.IndexOf(nameChkBox);

            WorkScopeWSStackPanel.Children[index].IsEnabled = true;

            if (BrigadeWorkScopeRadioButton.IsChecked == true)
                CalculateBrigadeWorkScope();
        }

        void workScopeUpDownControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var workScopeUpDownControl = (NumericControl)sender;

            int wid = Convert.ToInt32(workScopeUpDownControl.Name.Substring(10));

            object obj = VCLPWSStackPanel.FindName(("VCLP_" + wid).ToString(CultureInfo.InvariantCulture));
            if (obj != null)
                ((Label) obj) .Content = GetVCLP(Convert.ToDecimal(workScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
        }

        private void OKWorkersWorkScopeButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationsListBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать операцию!", "Внимание", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int operationGroupID =
                Convert.ToInt32(((DataRowView)OperationsListBox.SelectedItem)["OperationGroupID"]);

            int operationTypeID =
                Convert.ToInt32(((DataRowView)OperationsListBox.SelectedItem)["OperationTypeID"]);

            const int workStatusId = 1;

            if (BrigadierNameCheckBox.IsChecked == true)
                _ttc.AddNewTimeRecord(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                    Convert.ToInt32(FactoriesComboBox.SelectedValue),
                    Convert.ToInt32(WorkUnitsListBox.SelectedValue),
                    Convert.ToInt32(WorkSectionsListBox.SelectedValue),
                    Convert.ToInt32(WorkSubSectionsListBox.SelectedValue),
                    Convert.ToInt32(OperationsListBox.SelectedValue), operationGroupID, operationTypeID,
                    DoubleTimeSet.startTime, DoubleTimeSet.stopTime,
                    Convert.ToDecimal(BrigadierWorkScopeUpDownControl.Value),
                    Convert.ToDouble(BrigadierVCLPLabel.Content), NotesPopUpTextBox.Text,
                    workStatusId, -1, -1);

            foreach (var control in WorkScopeWSStackPanel.Children)
            {
                var workScopeUpDownControl = (NumericControl) control;

                var index = WorkScopeWSStackPanel.Children.IndexOf(workScopeUpDownControl);

                var workerID = Convert.ToInt32(workScopeUpDownControl.Name.Substring(10));

                var workScope = workScopeUpDownControl.Value;

                var vclp = Convert.ToDouble(((Label) VCLPWSStackPanel.Children[index]).Content);

                _ttc.AddNewTimeRecord(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                    Convert.ToInt32(FactoriesComboBox.SelectedValue),
                    Convert.ToInt32(WorkUnitsListBox.SelectedValue),
                    Convert.ToInt32(WorkSectionsListBox.SelectedValue),
                    Convert.ToInt32(WorkSubSectionsListBox.SelectedValue),
                    Convert.ToInt32(OperationsListBox.SelectedValue), operationGroupID, operationTypeID,
                    DoubleTimeSet.startTime, DoubleTimeSet.stopTime, Convert.ToDecimal(workScope), vclp,
                    NotesPopUpTextBox.Text, workStatusId, workerID);
            }

            AdministrationClass.AddNewAction(61);
            WorkersNamesListBox.SelectedItems.Clear();

            NotesPopUpTextBox.Text = string.Empty;
            TotalTimeLabel.Content = _ttc.CountingTotalTime();

            CancelWorkersWorkScopeButton_Click(null, null);
        }

        private void CancelWorkersWorkScopeButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersWorkScopeGrid.Visibility = Visibility.Hidden;

            WorkersWSStackPanel.Children.Clear();

            foreach (var obj in WorkScopeWSStackPanel.Children)
            {
                WorkersWSStackPanel.UnregisterName(((NumericControl)obj).Name);
            }
            WorkScopeWSStackPanel.Children.Clear();

            MeasureUnitsWSStackPanel.Children.Clear();

            foreach (var obj in VCLPWSStackPanel.Children)
            {
                VCLPWSStackPanel.UnregisterName(((Label)obj).Name);
            }

            VCLPWSStackPanel.Children.Clear();
        }

        private void AdditionalInfoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TimeTrackingDataGridSettings();
        }

        private void AdditionalInfoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TimeTrackingDataGridSettings();
        }

        private void TimeTrackingDataGridSettings()
        {
            Visibility columnVisibility = AdditionalInfoCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Hidden;

            foreach (DataGridColumn timeTrackingDataGridColumn in TimeTrackingDataGrid.Columns)
            {
                if (timeTrackingDataGridColumn.Header.ToString() == "Фабрика")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }

                if (timeTrackingDataGridColumn.Header.ToString() == "Участок")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }

                if (timeTrackingDataGridColumn.Header.ToString() == "Подучасток")
                {
                    timeTrackingDataGridColumn.Visibility = columnVisibility;
                }
            }
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            var drv = (DataRowView)TimeTrackingDataGrid.SelectedItem;
            if (drv == null) return;
            
            _ttc.DeleteRecord(drv.Row);
            AdministrationClass.AddNewAction(60);
            TotalTimeLabel.Content = _ttc.CountingTotalTime();
        }

        private void BindingWorkersData()
        {
            BrFactoriesComboBox.SelectionChanged -= BrFactoriesComboBox_SelectionChanged;
            BrFactoriesComboBox.DisplayMemberPath = "FactoryName";
            BrFactoriesComboBox.SelectedValuePath = "FactoryID";
            BrFactoriesComboBox.ItemsSource = _sc.GetFactories();
            BrFactoriesComboBox.SelectedIndex = 0;
            BrFactoriesComboBox.SelectionChanged += BrFactoriesComboBox_SelectionChanged;
            
            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;
            WorkersNamesListBox.ItemsSource = _sc.GetStaffPersonalInfo();
            WorkersNamesListBox.SelectedValuePath = "WorkerID";
            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;
            WorkersNamesListBox.Items.MoveCurrentToFirst();

            BrWorkersGroupsComboBox.SelectionChanged -= BrWorkersGroupsComboBox_SelectionChanged;
            BrWorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            BrWorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            BrWorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            BrWorkersGroupsComboBox.SelectedValue = 2;
            BrWorkersGroupsComboBox.SelectionChanged += BrWorkersGroupsComboBox_SelectionChanged;
            BrWorkersGroupsComboBox_SelectionChanged(null, null);
        }

        private void BrWorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrFilterWorkers();
        }

        private void BrFactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrFilterWorkers();
        }

        private void BrFilterWorkers()
        {
            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;

            WorkersNamesListBox.ItemsSource =
                _sc.FilterWorkers(true, Convert.ToInt32(BrWorkersGroupsComboBox.SelectedValue), true,
                    Convert.ToInt32(BrFactoriesComboBox.SelectedValue), false, 0).DefaultView;

            ((DataView)WorkersNamesListBox.ItemsSource).RowFilter = "AvailableInList = 'True'";

            //WorkersNamesListBox.SelectedIndex = 0;

            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;

            WorkersNamesListBox_SelectionChanged(null, null);
        }

        private void WorkersNamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersNamesListBox.SelectedItems.Count > 0)
            {
                StudentToggleButton.IsChecked = false;
                StudentToggleButton.IsEnabled = false;
                ResetStudentsButton_Click(null, null);
                CancelStudentsButton_Click(null, null);

                WorkScopeUpDownControl.IsEnabled = false;
                WorkScopeUpDownControl.Value = 0;
                VCLPLabel.Content = 0;
            }
            else
            {
                StudentToggleButton.IsEnabled = true;

                WorkScopeUpDownControl.IsEnabled = true;
            }

            BrigadeToggleButton.Content = "Бригада(" + WorkersNamesListBox.SelectedItems.Count + ")";
        }

        private void ResetWorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersNamesListBox.SelectedItems.Clear();
        }

        private void CancelWorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersPopup.IsOpen = false;
            WorkersNamesListBox.SelectedItems.Clear();
        }

        private void WorkersPopup_Opened(object sender, EventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;

            if (mw == null) return;

            mw.LocationChanged += (s, ea) =>
            {
                WorkersPopup.IsOpen = false;
                mw.LocationChanged += (ts, tea) => { };
            };

            mw.SizeChanged += (s, ea) =>
            {
                WorkersPopup.IsOpen = false;
                mw.SizeChanged += (ts, tea) => { };
            };
        }

        private void OKWorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersPopup.IsOpen = false;
        }

        private void StudentsNamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsNamesListBox.SelectedItems.Count > 0)
            {
                BrigadeToggleButton.IsChecked = false;
                BrigadeToggleButton.IsEnabled = false;
                ResetWorkersButton_Click(null, null);
                CancelWorkersButton_Click(null, null);
            }
            else
            {
                BrigadeToggleButton.IsEnabled = true;
            }

            StudentToggleButton.Content = "Ученик(" + StudentsNamesListBox.SelectedItems.Count + ")";
        }

        private void StWorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StFilterWorkers();
        }

        private void StFactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StFilterWorkers();
        }

        private void StFilterWorkers()
        {
            StudentsNamesListBox.SelectionChanged -= StudentsNamesListBox_SelectionChanged;

            StudentsNamesListBox.ItemsSource =
                _sc.FilterWorkers(true, Convert.ToInt32(StWorkersGroupsComboBox.SelectedValue), true,
                    Convert.ToInt32(StFactoriesComboBox.SelectedValue), false, 0).DefaultView;

            ((DataView)StudentsNamesListBox.ItemsSource).RowFilter = "AvailableInList = 'True'";

            StudentsNamesListBox.SelectionChanged += StudentsNamesListBox_SelectionChanged;

            StudentsNamesListBox_SelectionChanged(null, null);
        }

        private void CancelStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            StudentsPopup.IsOpen = false;
            StudentsNamesListBox.SelectedItems.Clear();
        }

        private void OKStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            StudentsPopup.IsOpen = false;
        }

        private void ResetStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            StudentsNamesListBox.SelectedItems.Clear();
        }

        private void StudentToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (StudentToggleButton.IsChecked != true) return;

            if (StudentsNamesListBox.SelectedItems.Count == 0)
                StWorkersGroupsComboBox.SelectedValue = 2;
            WorkersPopup.IsOpen = false;
        }

        private void BrigadeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (BrigadeToggleButton.IsChecked != true) return;

            if (WorkersNamesListBox.SelectedItems.Count == 0)
                BrWorkersGroupsComboBox.SelectedValue = 2;

            StudentsPopup.IsOpen = false;
        }
        
        private void WorkScopeUpDownControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _currentVclp = GetVCLP(Convert.ToDecimal(WorkScopeUpDownControl.Value), _currentProductivity, DoubleTimeSet.TotalHours);
            VCLPLabel.Content = _currentVclp;
        }

        private void BrigadeWorkScopeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CalculateBrigadeWorkScope();
        }

        private void BrigadeWorkScopeRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            BrigadeWorkScopeUpDownControl.Value = 0;
        }

        private void PersonalWorkScopeRadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void PersonalWorkScopeRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void BrigadeWorkScopeUpDownControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            CalculateBrigadeWorkScope();
        }

        private void CalculateBrigadeWorkScope()
        {
            if (BrigadeWorkScopeUpDownControl == null) return;

            decimal workScope = Convert.ToDecimal(BrigadeWorkScopeUpDownControl.Value);

            var indexList = new List<int>();

            if (workScope != 0)
            {
                foreach (var namesChkBox in WorkersWSStackPanel.Children)
                {
                    if (((CheckBox) namesChkBox).IsChecked == true)
                    {
                        indexList.Add(WorkersWSStackPanel.Children.IndexOf((CheckBox) namesChkBox));
                    }
                }
                
                int workersCount = BrigadierNameCheckBox.IsChecked == true ? indexList.Count + 1 : indexList.Count;

                decimal workerWorkScope = workScope / workersCount;

                if (BrigadierNameCheckBox.IsChecked == true)
                {
                    BrigadierWorkScopeUpDownControl.Value = workerWorkScope;
                    BrigadierVCLPLabel.Content = GetVCLP(Convert.ToDecimal(BrigadierWorkScopeUpDownControl.Value), _currentProductivity,
                        DoubleTimeSet.TotalHours);
                }



                foreach (var i in indexList)
                {
                    ((NumericControl)WorkScopeWSStackPanel.Children[i]).Value = workerWorkScope;

                    workScopeUpDownControl_PreviewKeyUp(WorkScopeWSStackPanel.Children[i], null);
                }
            }
        }

        private void BrigadierNameCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CalculateBrigadeWorkScope();
        }

        private void BrigadierNameCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CalculateBrigadeWorkScope();
            BrigadierVCLPLabel.Content = 0;
            BrigadierWorkScopeUpDownControl.Value = 0;
        }

        private void BrigadierWorkScopeUpDownControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            BrigadierVCLPLabel.Content = GetVCLP(Convert.ToDecimal(BrigadierWorkScopeUpDownControl.Value), _currentProductivity,
                        DoubleTimeSet.TotalHours);
        }



        #region Tasks

        private void OnTasksDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var taskRowView = TasksDataGrid.SelectedItem as DataRowView;

            SetTasksButtonsVisibility(taskRowView);
            FillTaskTimeTrackingInfo(taskRowView);
        }

        private void SetTasksButtonsVisibility(DataRowView taskRowView)
        {
            StartTaskButton.Visibility = Visibility.Hidden;
            EndTaskButton.Visibility = Visibility.Hidden;

            if (taskRowView == null) return;

            if (taskRowView["StartDate"] == DBNull.Value)
                StartTaskButton.Visibility = Visibility.Visible;
            else if (taskRowView["StartDate"] != DBNull.Value && taskRowView["CompletionDate"] == DBNull.Value)
                EndTaskButton.Visibility = Visibility.Visible;
        }

        private void FillTaskTimeTrackingInfo(DataRowView taskRowView)
        {
            StartTaskTimeTrackingTimeControl.TotalTime = TimeSpan.Zero;
            EndTaskTimeTrackingTimeControl.TotalTime = TimeSpan.Zero;

            if (taskRowView != null)
            {
                int performerId;
                Int32.TryParse(taskRowView["PerformerID"].ToString(), out performerId);
                var taskTimeTrackingView = new BindingListCollectionView(_taskClass.TaskTimeTracking.Table.AsDataView())
                {
                    CustomFilter = string.Format("PerformerID = {0}", performerId)
                };
                if (taskTimeTrackingView.GroupDescriptions != null)
                    taskTimeTrackingView.GroupDescriptions.Add(new PropertyGroupDescription("Date"));
                TaskTimeTrackingListBox.ItemsSource = taskTimeTrackingView;
                FillTaskTimeTrackingBorder.IsEnabled = true;
                TotalTaskTimeTrackingIntervalLabel.Content = CalculateTaskTimeTrackingTotalTime(taskTimeTrackingView);
                return;
            }

            TotalTaskTimeTrackingIntervalLabel.Content = TimeSpan.Zero;
            FillTaskTimeTrackingBorder.IsEnabled = false;
            TaskTimeTrackingListBox.ItemsSource = null;
        }

        private static TimeSpan CalculateTaskTimeTrackingTotalTime(CollectionView timeTrackingCollection)
        {
            if (timeTrackingCollection.Count == 0) return TimeSpan.Zero;

            var totalTime = new TimeSpan();
            totalTime =
                timeTrackingCollection.Cast<DataRowView>().Select(dataRow =>
                    TimeIntervalCountConverter.CalculateTimeInterval(
                        (TimeSpan) dataRow["TimeStart"], (TimeSpan) dataRow["TimeEnd"]))
                    .Aggregate(totalTime, (current, interval) => current.Add(interval));
            return totalTime;
        }

        private void OnAddTaskTimeTrackingButtonClick(object sender, RoutedEventArgs e)
        {
            var taskRowView = TasksDataGrid.SelectedItem as DataRowView;
            if (taskRowView == null) return;

            var timeStart = StartTaskTimeTrackingTimeControl.TotalTime;
            var timeEnd = EndTaskTimeTrackingTimeControl.TotalTime;

            if (timeStart == timeEnd) return;

            var taskId = Convert.ToInt32(taskRowView["TaskID"]);
            var performerId = Convert.ToInt32(taskRowView["PerformerID"]);

            var date = _ttc.CurrentWorkerStartDate;
            var timeSpentAtWorkId = _ttc.CurrentTimeSpentAtWorkID;

            _taskClass.AddNewTaskTimeTracking(taskId, performerId, timeSpentAtWorkId, date, timeStart, timeEnd);
            AdministrationClass.AddNewAction(90);

            var totalTime = CalculateTaskTimeTrackingTotalTime((CollectionView)TaskTimeTrackingListBox.ItemsSource);
            _taskClass.Performers.SetTimeSpend(performerId, totalTime);
            TotalTaskTimeTrackingIntervalLabel.Content = totalTime;

            StartTaskTimeTrackingTimeControl.TotalTime = TimeSpan.Zero;
            EndTaskTimeTrackingTimeControl.TotalTime = TimeSpan.Zero;
        }

        private void OnDeleteTaskTimeTrackingButtonClick(object sender, RoutedEventArgs e)
        {
            var taskRowView = TasksDataGrid.SelectedItem as DataRowView;
            if (taskRowView == null) return;
            if (TaskTimeTrackingListBox.SelectedItem == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную запись?", "Удаление", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var performerId = Convert.ToInt32(taskRowView["PerformerID"]);
            var taskTimeTrackingId = Convert.ToInt32(TaskTimeTrackingListBox.SelectedValue);
            _taskClass.DeleteTaskTimeTracking(taskTimeTrackingId);

            var totalTime = CalculateTaskTimeTrackingTotalTime((CollectionView)TaskTimeTrackingListBox.ItemsSource);
            _taskClass.Performers.SetTimeSpend(performerId, totalTime);
            TotalTaskTimeTrackingIntervalLabel.Content = totalTime;
        }

        private void OnStartTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var taskRowView = TasksDataGrid.SelectedItem as DataRowView;
            if (taskRowView == null) return;

            var taskId = Convert.ToInt32(taskRowView["TaskID"]);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            _taskClass.Performers.StartTask(taskId, currentWorkerId, currentDate);
            AdministrationClass.AddNewAction(91);
            _taskClass.Tasks.StartTask(taskId);

            taskRowView["StartDate"] = currentDate;
            taskRowView["TaskStatusID"] = (int)TaskClass.TaskStatuses.IsPerformed;

            var globalId = taskRowView["GlobalID"].ToString();
            var senderAppId = Convert.ToInt32(taskRowView["SenderAppID"]);
            switch ((TaskClass.SenderApplications)senderAppId)
            {
                case TaskClass.SenderApplications.ServiceJournal:
                    ServiceEquipmentClass.ServiceResponsibilitiesClass.AcceptAndStart(globalId, currentWorkerId,
                        currentDate);
                    break;
            }

            SetTasksButtonsVisibility(taskRowView);
        }

        private void OnEndTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var taskRowView = TasksDataGrid.SelectedItem as DataRowView;
            if (taskRowView == null) return;

            var taskId = Convert.ToInt32(taskRowView["TaskID"]);
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            _taskClass.Performers.EndTask(taskId, currentWorkerId, currentDate);
            AdministrationClass.AddNewAction(92);

            taskRowView["CompletionDate"] = currentDate;
            taskRowView["TaskStatusID"] = (int)TaskClass.TaskStatuses.IsCompleted;
            taskRowView["IsComplete"] = true;

            var globalId = taskRowView["GlobalID"].ToString();
            var senderAppId = Convert.ToInt32(taskRowView["SenderAppID"]);
            switch ((TaskClass.SenderApplications)senderAppId)
            {
                // If sender application is service journal, than complete journal row
                case TaskClass.SenderApplications.ServiceJournal:
                    ServiceEquipmentClass.ServiceResponsibilitiesClass.Complete(globalId, currentWorkerId,
                        currentDate);
                    break;
            }

            // Complete task if every performers is allready completed
            if (_taskClass.Performers.Table.AsEnumerable().Where(p => p.Field<Int64>("TaskID") == taskId)
                .All(p => p.Field<bool>("IsComplete")))
            {
                _taskClass.Tasks.EndTask(taskId, currentDate);

                taskRowView["TaskCompletionDate"] = currentDate;

                switch ((TaskClass.SenderApplications)senderAppId)
                {
                    // If sender application is service equipment, program will complete crash request
                    case TaskClass.SenderApplications.ServiceDamage:
                        {
                            ServiceEquipmentClass servEquipClass = null;
                            App.BaseClass.GetServiceEquipmentClass(ref servEquipClass);

                            if (servEquipClass != null)
                            {
                                servEquipClass.FillCompletionInfo(globalId, currentDate, currentWorkerId, null);
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
                                techProblemClass.FillCompletionInfo(globalId, currentDate, currentWorkerId);
                            }

                            break;
                        }
                    case TaskClass.SenderApplications.PlannedWorks:
                        {
                            PlannedWorksClass plannedWorksClass = null;
                            App.BaseClass.GetPlannedWorksClass(ref plannedWorksClass);
                            if (plannedWorksClass != null)
                            {
                                plannedWorksClass.FinishPlannedWorks(taskId);
                            }
                            break;
                        }
                }
            }

            SetTasksButtonsVisibility(taskRowView);
        }

        #endregion

    }
}
