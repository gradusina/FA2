using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;

namespace FA2.ChildPages.CatalogPage
{
    /// <summary>
    /// Логика взаимодействия для WorkerGroupsChildPage.xaml
    /// </summary>
    public partial class WorkerGroupsChildPage
    {
        private CatalogClass _cc;
        private string _tempName;
        private bool _unit;

        public WorkerGroupsChildPage()
        {
            InitializeComponent();

            EditGrid.Height = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetClasses();
            SetBindings();

            WorkerGroupsListBox.SelectedIndex = 0;
        }

        private void GetClasses()
        {
            App.BaseClass.GetCatalogClass(ref _cc);
        }

        private void SetBindings()
        {
            WorkerGroupsListBox.ItemsSource = _cc.GetWorkersGroups();
        }


        private void ShowEditGrid()
        {
            if(EditGrid.Height == 80) return;

            var da = new DoubleAnimation {Duration = TimeSpan.FromSeconds(0.1), From = 0, To = 80};
            EditGrid.BeginAnimation(HeightProperty, da);

            NameTextBox.Focus();

            LockControls();
        }

        private void HideEditGrid()
        {
            var da = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.1), From = 80, To = 0 };
            EditGrid.BeginAnimation(HeightProperty, da);

            LockControls(true);
        }

        private void WorkerGroupsListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditGrid.DataContext = WorkerGroupsListBox.SelectedItem;
            ShowEditGrid();

            OkButton.Content = "Сохранить";

            _tempName = NameTextBox.Text;
            _unit = UnitToFactoryCheckBox.IsChecked == true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditGrid.DataContext != null)
                ((DataRowView)WorkerGroupsListBox.SelectedItem)["WorkerGroupName"] = _tempName;

            HideEditGrid();
            EditGrid.DataContext = null;
            NameTextBox.Text = string.Empty;
            UnitToFactoryCheckBox.IsChecked = false;
        }

        private void LockControls(bool isEnabled = false)
        {
            ButtonsStackPanel.IsEnabled = isEnabled;
            WorkerGroupsListBox.IsEnabled = isEnabled;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Trim() == string.Empty) return;

            if (EditGrid.DataContext == null)
                _cc.AddWorkerGroup(NameTextBox.Text.Trim(), UnitToFactoryCheckBox.IsChecked == true);

            _cc.SaveWorkerGroups();

            EditGrid.DataContext = null;
            NameTextBox.Text = string.Empty;
            UnitToFactoryCheckBox.IsChecked = false;

            HideEditGrid();
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            EditGrid.DataContext = null;
            NameTextBox.Text = string.Empty;
            UnitToFactoryCheckBox.IsChecked = false;
            OkButton.Content = "Добавить";

            ShowEditGrid();
        }

        private void AdditOperationsNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OkButton_Click(null, null);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerGroupsListBox.SelectedItem == null) return;

            var result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView)WorkerGroupsListBox.SelectedItem)["WorkerGroupName"] +
                    "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView)WorkerGroupsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkerGroups();
            }
        }
    }
}
