using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ToolsPages
{
    /// <summary>
    /// Логика взаимодействия для MyWorkersPage.xaml
    /// </summary>
    public partial class MyWorkersPage
    {
        private MyWorkersClass _mwc;
        private StaffClass _sc;
        public MyWorkersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (o, args) =>
                GetClasses();

            backgroundWorker.RunWorkerCompleted += (o, args) =>
            {
                SetBindings();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            };

            backgroundWorker.RunWorkerAsync();

            
            
        }

        private void GetClasses()
        {
            App.BaseClass.GetMyWorkersClass(ref _mwc);

            App.BaseClass.GetStaffClass(ref _sc);
        }

        private void SetBindings()
        {
            MyWorkersGroupComboBox.ItemsSource = _mwc.GetMyWorkersGroups();
            ((DataView)MyWorkersGroupComboBox.ItemsSource).RowFilter = "IsEnable='True' AND  MainWorkerID = '" +
                                                                       AdministrationClass.CurrentWorkerId + "'";

            MyWorkersGroupComboBox.SelectedIndex = 0;


            MyWorkersListBox.ItemsSource = _mwc.GetMyWorkers();

            MainWorkersListBox.ItemsSource = _mwc.GetMyWorkers();
            ((DataView) MainWorkersListBox.ItemsSource).RowFilter = "WorkerID='" + AdministrationClass.CurrentWorkerId +
                                                                    "'";

            WorkersListBox.ItemsSource = _sc.GetStaffPersonalInfo();

            WorkerGroupsComboBox.SelectionChanged -= WorkerFilters_OnSelectionChanged;
            WorkerGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkerGroupsComboBox.SelectedIndex = 0;
            WorkerGroupsComboBox.SelectionChanged += WorkerFilters_OnSelectionChanged;


            FactoriesComboBox.SelectionChanged -= WorkerFilters_OnSelectionChanged;
            FactoriesComboBox.ItemsSource = _sc.GetFactories();
            FactoriesComboBox.SelectedIndex = 0;
            FactoriesComboBox.SelectionChanged += WorkerFilters_OnSelectionChanged;

            FilterWorkers();

            WorkersListBox_SelectionChanged(null, null);
            MyWorkersGroupComboBox_SelectionChanged(null, null);
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            NewGroupStackPanel.Visibility = Visibility.Visible;
            ShowWorkersViewButton.IsEnabled = false;
            NewGroupNameTextBox.Text = string.Empty;
            NewGroupNameTextBox.Focus();
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyWorkersGroupComboBox.Items.Count == 0) return;
            if (MyWorkersGroupComboBox.SelectedItem == null) return;

            string groupName = ((DataRowView) MyWorkersGroupComboBox.SelectedItem).Row["MyWorkerGroupName"].ToString();

            if (MetroMessageBox.Show("Вы действительно хотите удалить группу '" + groupName + "' ?", string.Empty,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ((DataRowView) MyWorkersGroupComboBox.SelectedItem).Row["IsEnable"] = false;

                _mwc.SaveMyWorkersGroups();
            }
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyWorkersGroupComboBox.Items.Count == 0) return;
            if (MyWorkersGroupComboBox.SelectedItem == null) return;

            NewGroupStackPanel.Visibility = Visibility.Visible;
            ShowWorkersViewButton.IsEnabled = false;
            NewGroupNameTextBox.Text = string.Empty;
            NewGroupNameTextBox.Focus();

            var myBinding = new Binding("MyWorkerGroupName") { Mode = BindingMode.TwoWay};
            NewGroupNameTextBox.SetBinding(TextBox.TextProperty, myBinding);
            NewGroupNameTextBox.DataContext = MyWorkersGroupComboBox.SelectedItem;
        }

        private void ShowWorkersViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MyWorkersGroupComboBox.SelectedItem == null) return;

            MainGrid.IsEnabled = false;
            ShadowGrid.Visibility = Visibility.Visible;

            var widthAnimation = new DoubleAnimation(250d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color {A = 20, R = 0, G = 0, B = 0};
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void AddWorkerButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (WorkersListBox.SelectedItems.Count == 0) return;

            var myWorkersView = ((DataView) MyWorkersListBox.ItemsSource);

            int myWorkerGroupId = Convert.ToInt32(MyWorkersGroupComboBox.SelectedValue);

            string filter = myWorkersView.RowFilter;

            foreach (DataRowView workerView in WorkerProfessionsListBox.Items)
            {
                if (!Convert.ToBoolean(workerView["IsSelected"])) continue;

                var workerId = Convert.ToInt32(workerView["WorkerID"]);
                var workerProfessionID = Convert.ToInt32(workerView["WorkerProfessionID"]);

                if (myWorkersView != null)
                {
                    myWorkersView.RowFilter = filter + " AND WorkerID=" + workerId + " AND WorkerProfessionID=" +
                                              workerProfessionID;

                    if (myWorkersView.Count == 0)
                        _mwc.AddMyWorker(workerId, AdministrationClass.CurrentWorkerId, myWorkerGroupId,
                            workerProfessionID);
                }

                myWorkersView.RowFilter = filter;

                _mwc.SaveMyWorkers();

                CancelAddWorkerButton_OnClick(null, null);
            }
        }

        private void CancelAddWorkerButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainGrid.IsEnabled = true;
            SelectWorkersGrid.IsEnabled = true;
            WorkersListBox.SelectedIndex = -1;

            var widthAnimation = new DoubleAnimation(0d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            var shadowColor = new Color { A = 0, R = 0, G = 0, B = 0 };
            var colorAnimation = new ColorAnimation(shadowColor, new Duration(TimeSpan.FromMilliseconds(200)));
            colorAnimation.Completed += (s, args) => { ShadowGrid.Visibility = Visibility.Collapsed; };
            ShadowGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void WorkerSearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchAndFilterWorkers();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mwc.SaveMyWorkers();

            SelectWorkersGrid.IsEnabled = true;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            mainWindow.HideToolsGrid();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            mainWindow.HideToolsGrid();
        }

        private void WorkerFilters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FilterWorkers()
        {
            WorkerSearchTextBox.TextChanged -= WorkerSearchTextBox_OnTextChanged;
            WorkerSearchTextBox.Text = string.Empty;
            WorkerSearchTextBox.TextChanged += WorkerSearchTextBox_OnTextChanged;

            WorkersListBox.ItemsSource =
                _sc.FilterWorkers(true, Convert.ToInt32(WorkerGroupsComboBox.SelectedValue), true,
                    Convert.ToInt32(FactoriesComboBox.SelectedValue), false, 0).DefaultView;

            SearchAndFilterWorkers();
        }

        private void SearchAndFilterWorkers()
        {
            if (WorkersListBox.ItemsSource == null && WorkerSearchTextBox.Text.Trim().ToLower() == string.Empty) return;

            var searchText = WorkerSearchTextBox.Text.Trim().ToLower();
            var filteredView = ((DataView)WorkersListBox.ItemsSource).Table.AsEnumerable().Where(r => r.Field<bool>("AvailableInList")).
                Where(r => r.Field<string>("Name").ToLower().Contains(searchText)).AsDataView();
            filteredView.Sort = "Name";

            WorkersListBox.ItemsSource = filteredView;

            WorkersListBox.UnselectAll();
        }


        private void OKAddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewGroupNameTextBox.Text.Trim() == string.Empty)
            {
                NewGroupNameTextBox.Focus();
                return;
            }

            if (!BindingOperations.IsDataBound(NewGroupNameTextBox, TextBox.TextProperty))
                _mwc.AddMyWorkersGroup(AdministrationClass.CurrentWorkerId, NewGroupNameTextBox.Text.Trim());
            else
                _mwc.SaveMyWorkers();

            MyWorkersGroupComboBox.SelectedIndex = 0;

            CancelAddGroupButton_Click(null, null);
        }

        private void CancelAddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            BindingOperations.ClearBinding(NewGroupNameTextBox, TextBox.TextProperty);
            NewGroupStackPanel.Visibility = Visibility.Collapsed;
            ShowWorkersViewButton.IsEnabled = true;
            NewGroupNameTextBox.Text = string.Empty;
            MyWorkersGroupComboBox.Focus();
        }

        private void MyWorkersGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyWorkersGroupComboBox.SelectedValue == null || MyWorkersGroupComboBox.SelectedItem == null)
            {
                if (MyWorkersListBox.ItemsSource != null)
                    ((DataView) MyWorkersListBox.ItemsSource).RowFilter = "MainWorkerID= -1";

                return;
            }

            if (MyWorkersListBox.ItemsSource != null)
            ((DataView) MyWorkersListBox.ItemsSource).RowFilter = "MainWorkerID=" + AdministrationClass.CurrentWorkerId +
                                                                  " AND MyWorkersGroupID=" +
                                                                  Convert.ToInt32(MyWorkersGroupComboBox.SelectedValue) + " AND IsEnable= 'True'";
        }

        private void NextWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerProfessionsListBox.ItemsSource = null;

            DataTable dt = _sc.GetWorkerProfessions().Table.Copy();

            var dc = new DataColumn {ColumnName = "IsSelected", DataType = typeof (Boolean), DefaultValue = false};

            dt.Columns.Add(dc);

            string workersIDs =
                (from DataRowView workerView in WorkersListBox.SelectedItems
                    select Convert.ToInt32(workerView["WorkerID"])).Aggregate(string.Empty,
                        (current, workerId) => current + workerId + ",");

            workersIDs = workersIDs.Remove(workersIDs.Count() - 1);

            string filter = "WorkerID IN (" + workersIDs + ")";

            foreach (DataRowView workerView in WorkersListBox.SelectedItems)
            {
                var workerId = Convert.ToInt32(workerView["WorkerID"]);
                dt.DefaultView.RowFilter = filter + " AND WorkerID =" + workerId;
                dt.DefaultView[0]["IsSelected"] = true;
            }

            dt.DefaultView.RowFilter = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(dt.DefaultView);
            view.GroupDescriptions.Add(new PropertyGroupDescription("WorkerID"));

            WorkerProfessionsListBox.ItemsSource = view;

            var widthAnimation = new DoubleAnimation(600d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            SelectWorkersGrid.IsEnabled = false;
        }

        private void PreviousWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var widthAnimation = new DoubleAnimation(250d, new Duration(TimeSpan.FromMilliseconds(200)));
            AddPerformersPanel.BeginAnimation(WidthProperty, widthAnimation);

            SelectWorkersGrid.IsEnabled = true;
        }

        private void WorkersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NextWorkerButton.IsEnabled = WorkersListBox.SelectedItems.Count != 0;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MyWorkersListBox.ItemsSource = null;
            WorkerProfessionsListBox.ItemsSource = null;
            WorkerProfessionsListBox.ItemsSource = null;


            GC.Collect();
            GC.Collect();
        }

        private void DeleteMyWorker_Click(object sender, RoutedEventArgs e)
        {
            if (MetroMessageBox.Show("Вы действительно хотите удалить данного работника из своего списка?", string.Empty,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ((DataRowView) ((Button) sender).DataContext).Row["IsEnable"] = false;
                _mwc.SaveMyWorkers();
            }
        }

        private void CloseMainWorkersButton_Click(object sender, RoutedEventArgs e)
        {
            MainWorkersPanel.Visibility = Visibility.Hidden;
        }

        private void ShowMainWorkersViewButton_Click(object sender, RoutedEventArgs e)
        {
            MainWorkersPanel.Visibility = Visibility.Visible;
        }
    }
}
