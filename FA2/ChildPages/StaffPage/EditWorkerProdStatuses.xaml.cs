using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для EditWorkerProdStatuses.xaml
    /// </summary>
    public partial class EditWorkerProdStatuses : Page
    {
        private StaffClass _sc;
        int _workerId;

        public EditWorkerProdStatuses(int workerId)
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            _workerId = workerId;
            BindingData();
        }

        private void BindingData()
        {
            WorkerProdStatusComboBox.ItemsSource = _sc.GetProductionStatuses();
            WorkerProdStatusComboBox.DisplayMemberPath = "ProdStatusName";
            WorkerProdStatusComboBox.SelectedValuePath = "ProdStatusID";

            var workerProdStatusView = _sc.GetWorkerProdStatuses();
            workerProdStatusView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            WorkerProdStatusListBox.SelectedValuePath = "WorkerProdStatusID";
            WorkerProdStatusListBox.ItemsSource = workerProdStatusView;
            if (WorkerProdStatusListBox.HasItems)
                WorkerProdStatusListBox.SelectedIndex = 0;
        }

        private void ShowSecondWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    AddProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                AddProcedure();
            }
        }

        private void AddProcedure()
        {
            ViewGrid.Visibility = Visibility.Hidden;
            RedactorGrid.Visibility = Visibility.Visible;
            RedactorGrid.DataContext = null;
            AddWorkerProdStatusButton.Visibility = Visibility.Visible;
            ChangeWorkerProdStatusButton.Visibility = Visibility.Hidden;
            if (WorkerProdStatusComboBox.HasItems) WorkerProdStatusComboBox.SelectedIndex = 0;
        }

        private void HideSecondWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    CancelProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelProcedure();
            }
        }

        private void CancelProcedure()
        {
            ViewGrid.Visibility = Visibility.Visible;
            RedactorGrid.Visibility = Visibility.Hidden;
            WorkerProdStatusListBox.IsEnabled = true;
            if (WorkerProdStatusListBox.HasItems)
                WorkerProdStatusListBox.SelectedIndex = 0;
        }

        private void ChangeWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var workerProdStatus = WorkerProdStatusListBox.SelectedItem as DataRowView;
            if (workerProdStatus == null) return;

            if (WorkerProdStatusComboBox.SelectedItem == null) return;

            var workerProdStatusId = Convert.ToInt64(workerProdStatus["WorkerProdStatusID"]);
            var prodStatusId = Convert.ToInt32(WorkerProdStatusComboBox.SelectedValue);
            var currentDate = App.BaseClass.GetDateFromSqlServer();

            _sc.ChangeWorkerProdStatus(workerProdStatusId, prodStatusId, currentDate);

            HideSecondWorkerProdStatusButton_Click(null, null);
        }

        private void DeleteWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var workerProdStatus = WorkerProdStatusListBox.SelectedItem as DataRowView;
            if (workerProdStatus == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранный навык?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var workerProdStatusId = Convert.ToInt64(workerProdStatus["WorkerProdStatusID"]);
            _sc.DeleteWorkerProdStatus(workerProdStatusId);
        }

        private void AddWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerProdStatusComboBox.SelectedItem == null) return;

            int prodStatusId = Convert.ToInt32(WorkerProdStatusComboBox.SelectedValue);
            if (_sc.WorkerProdStatusesDataTable.Select(string.Format("WorkerID = {0} AND ProdStatusID = {1}", _workerId,
                                                           prodStatusId)).Length != 0)
            {
                MessageBox.Show("У выбранного работника уже существует данный навык!", "Предупреждение", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _sc.AddNewWorkerProdStatus(_workerId, prodStatusId, currentDate);

            HideSecondWorkerProdStatusButton_Click(null, null);
        }

        private void CancelWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void EditWorkerProdStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerProdStatusListBox.SelectedItem == null) return;

            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    EditProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditProcedure();
            }

            WorkerProdStatusListBox.IsEnabled = false;
        }

        private void EditProcedure()
        {
            ViewGrid.Visibility = Visibility.Hidden;
            RedactorGrid.Visibility = Visibility.Visible;
            RedactorGrid.DataContext = WorkerProdStatusListBox.SelectedItem;
            AddWorkerProdStatusButton.Visibility = Visibility.Hidden;
            ChangeWorkerProdStatusButton.Visibility = Visibility.Visible;
        }

        private void WorkerProdStatusListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditWorkerProdStatusButton_Click(null, null);
        }
    }
}
