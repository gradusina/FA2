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
    /// Логика взаимодействия для OperationGroupsPage.xaml
    /// </summary>
    public partial class OperationGroupsPage
    {
        private CatalogClass _cc;
        private string _tempName;

         public OperationGroupsPage()
        {
            InitializeComponent();
             EditGrid.Height = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetClasses();
            SetBindings();

            OperationGroupsListBox.SelectedIndex = 0;
        }

        private void GetClasses()
        {
            App.BaseClass.GetCatalogClass(ref _cc);
        }

        private void SetBindings()
        {
            OperationGroupsListBox.ItemsSource = _cc.GetOperationGroups();
        }


        private void ShowEditGrid()
        {
            if(EditGrid.Height == 40) return;

            var da = new DoubleAnimation {Duration = TimeSpan.FromSeconds(0.1), From = 0, To = 40};
            EditGrid.BeginAnimation(HeightProperty, da);

            NameTextBox.Focus();

            LockControls();
        }

        private void HideEditGrid()
        {
            var da = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.1), From = 40, To = 0 };
            EditGrid.BeginAnimation(HeightProperty, da);

            LockControls(true);
        }

        private void OperationGroupsListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NameTextBox.DataContext = OperationGroupsListBox.SelectedItem;
            ShowEditGrid();

            OkButton.Content = "Сохранить";

            _tempName = NameTextBox.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.DataContext != null)
                ((DataRowView)OperationGroupsListBox.SelectedItem)["OperationGroupName"] = _tempName;

            HideEditGrid();
            NameTextBox.DataContext = null;
            NameTextBox.Text = string.Empty;
        }

        private void LockControls(bool isEnabled = false)
        {
            ButtonsStackPanel.IsEnabled = isEnabled;
            OperationGroupsListBox.IsEnabled = isEnabled;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Trim() == string.Empty) return;

            if (NameTextBox.DataContext == null)
                _cc.AddOperationGroup(NameTextBox.Text.Trim());

            _cc.SaveOperationGroups();

            NameTextBox.DataContext = null;
            NameTextBox.Text = string.Empty;

            HideEditGrid();
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.DataContext = null;
            NameTextBox.Text = string.Empty;

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
            if (OperationGroupsListBox.SelectedItem == null) return;
            if (Convert.ToBoolean(((DataRowView) OperationGroupsListBox.SelectedItem)["Locked"])) return;

            var result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView)OperationGroupsListBox.SelectedItem)["OperationGroupName"] +
                    "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView)OperationGroupsListBox.SelectedItem)["Visible"] = false;
                _cc.SaveOperationGroups();
            }
        }

        private void OperationGroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameTextBox.DataContext == null) return;

            NameTextBox.DataContext = OperationGroupsListBox.SelectedItem;
        }
    }
}
