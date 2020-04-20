using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для WorkersStatuses.xaml
    /// </summary>
    public partial class WorkersStatusesCatalog : Page
    {
        private StaffClass _sc;

        public WorkersStatusesCatalog()
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            BindingData();
        }

        private void BindingData()
        {
            StatusesListBox.DisplayMemberPath = "WorkerStatusName";
            StatusesListBox.SelectedValuePath = "WorkerStatusID";
            StatusesListBox.ItemsSource = _sc.GetWorkerStatuses();
            if (StatusesListBox.Items.Count != 0)
                StatusesListBox.SelectedIndex = 0;
        }

        private void CancelStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void SaveStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StatusNameTextBox.Text) || StatusesListBox.SelectedItem == null) return;

            int workerStatusId = Convert.ToInt32(StatusesListBox.SelectedValue);
            string statusName = StatusNameTextBox.Text;
            bool availableInList = Convert.ToBoolean(AvailableInListCheckBox.IsChecked);
            string filterStr = 
                string.Format("WorkerStatusName = '{0}' AND Enable = 'True' AND AvailableInList = '{1}' AND WorkerStatusID <> {2}",
                statusName, availableInList, workerStatusId);

            if (!ContainsWorkerStatus(filterStr))
            {
                _sc.SaveStatus(workerStatusId, statusName, availableInList);
                DontAddStatusButton_Click(null, null);
            }
            else
            {
                MetroMessageBox.Show("Даннай статус существует в базе!", "Внимание", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private bool ContainsWorkerStatus(string filterStr)
        {
            var findRows = _sc.WorkerStatusesDataTable.Select(filterStr);
            return findRows.Any();
        }

        private void AddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(250, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            animation.Completed += (s, args) =>
                {
                    WorkerStatusesViewGrid.Visibility = Visibility.Hidden;
                    WorkerStatusesRedactorGrid.Visibility = Visibility.Visible;
                    WorkerStatusesRedactorGrid.DataContext = null;
                    OkStatusButton.Visibility = Visibility.Visible;
                    SaveStatusButton.Visibility = Visibility.Hidden;
                    NewWorkerStatusTextBlock.Visibility = Visibility.Visible;
                    StatusFocus.Visibility = Visibility.Visible;

                    opacityAnimation.To = 1;
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, animation);
        }

        private void DeleteStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusesListBox.SelectedItem == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить выбранный статус?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var workerStatusId = Convert.ToInt32(StatusesListBox.SelectedValue);
                _sc.DeleteStatus(workerStatusId);

                if (StatusesListBox.Items.Count != 0)
                    StatusesListBox.SelectedIndex = 0;
            }
        }
        
        // Add a new status
        private void OkStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(StatusNameTextBox.Text))
            {
                string statusName = StatusNameTextBox.Text;
                bool availableInList = Convert.ToBoolean(AvailableInListCheckBox.IsChecked);
                string filterStr =
                    string.Format("WorkerStatusName = '{0}' AND Enable = 'True' AND AvailableInList = '{1}'",
                    statusName, availableInList);

                if (!ContainsWorkerStatus(filterStr))
                {
                    _sc.AddNewWorkerStatus(statusName, availableInList);
                    DontAddStatusButton_Click(null, null);
                }
                else
                {
                    MetroMessageBox.Show("Даннай статус существует в базе!", "Внимание", MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
        }

        // Add cancel
        private void DontAddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(400, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

            animation.Completed += (s, args) =>
            {
                WorkerStatusesViewGrid.Visibility = Visibility.Visible;
                WorkerStatusesRedactorGrid.Visibility = Visibility.Hidden;

                if (StatusesListBox.Items.Count != 0)
                StatusesListBox.SelectedIndex = 0;

                NewWorkerStatusTextBlock.Visibility = Visibility.Hidden;
                StatusFocus.Visibility = Visibility.Hidden;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, animation);

            StatusesListBox.IsEnabled = true;
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusesListBox.SelectedItem == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(250, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            animation.Completed += (s, args) =>
            {
                WorkerStatusesViewGrid.Visibility = Visibility.Hidden;
                WorkerStatusesRedactorGrid.Visibility = Visibility.Visible;
                WorkerStatusesRedactorGrid.DataContext = StatusesListBox.SelectedItem;
                OkStatusButton.Visibility = Visibility.Hidden;
                SaveStatusButton.Visibility = Visibility.Visible;

                NewWorkerStatusTextBlock.Visibility = Visibility.Hidden;
                StatusFocus.Visibility = Visibility.Hidden;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, animation);

            StatusesListBox.IsEnabled = false;
        }

        private void StatusesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeStatusButton_Click(null, null);
        }
    }
}
