using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;

namespace FA2.ChildPages.CatalogPage
{
    /// <summary>
    /// Логика взаимодействия для AdditionalOperationsChildPage.xaml
    /// </summary>
    public partial class AdditionalOperationsChildPage
    {
        private CatalogClass _cc;
        //private string _tempName;
        private bool _fullAccess;

        public AdditionalOperationsChildPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
            EditGrid.Height = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetClasses();
            SetBindings();
            SetControlsEnable();

            WorkOperationListBox.SelectedIndex = 0;
        }

        private void GetClasses()
        {
            App.BaseClass.GetCatalogClass(ref _cc);
        }

        private void SetBindings()
        {           
            DataView operationsDataView = _cc.GetWorkOperations();
            operationsDataView.RowFilter = "WorkSubsectionID = '-1' AND Visible = 'True'";

            WorkOperationListBox.ItemsSource = operationsDataView;
        }

        private void SetControlsEnable()
        {
            var defaultVisibility = _fullAccess ? Visibility.Visible : Visibility.Collapsed;

            AddNewButton.Visibility = defaultVisibility;
            DeleteButton.Visibility = defaultVisibility;
        }


        private void ShowEditGrid()
        {
            if(EditGrid.Height == 40) return;

            var da = new DoubleAnimation {Duration = TimeSpan.FromSeconds(0.1), From = 0, To = 40};
            EditGrid.BeginAnimation(HeightProperty, da);

            AdditOperationsNameTextBox.Focus();

            LockControls();
        }

        private void HideEditGrid()
        {
            var da = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.1), From = 40, To = 0 };
            EditGrid.BeginAnimation(HeightProperty, da);

            LockControls(true);
        }

        private void AdditionalOperationsListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(!_fullAccess) return;

            var workOperation = WorkOperationListBox.SelectedItem as DataRowView;
            if (workOperation == null) return;

            AdditOperationsNameTextBox.Text = workOperation["WorkOperationName"].ToString();
            ShowEditGrid();

            OkButton.Visibility = Visibility.Visible;
            AddButton.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideEditGrid();
            AdditOperationsNameTextBox.Text = string.Empty;
        }

        private void LockControls(bool isEnabled = false)
        {
            ButtonsStackPanel.IsEnabled = isEnabled;
            WorkOperationListBox.IsEnabled = isEnabled;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdditOperationsNameTextBox.Text.Trim() == string.Empty) return;

            var workOperation = WorkOperationListBox.SelectedItem as DataRowView;
            if (workOperation == null) return;

            workOperation["WorkOperationName"] = AdditOperationsNameTextBox.Text.Trim();
            _cc.SaveWorkOperation();

            AdditOperationsNameTextBox.Text = string.Empty;
            HideEditGrid();
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            AdditOperationsNameTextBox.Text = string.Empty;

            OkButton.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;

            ShowEditGrid();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkOperationListBox.SelectedItem == null) return;

            var result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView)WorkOperationListBox.SelectedItem)["WorkOperationName"] +
                    "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView)WorkOperationListBox.SelectedItem)["Visible"] = false;
                _cc.SaveWorkOperation();
            }
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            if (AdditOperationsNameTextBox.Text.Trim() == string.Empty) return;

            _cc.AddAdditOperation(AdditOperationsNameTextBox.Text.Trim());
            _cc.SaveWorkOperation();

            AdditOperationsNameTextBox.Text = string.Empty;
            HideEditGrid();
        }
    }
}
