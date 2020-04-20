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
    /// Логика взаимодействия для FactoriesChildPage.xaml
    /// </summary>
    public partial class FactoriesChildPage
    {

        private CatalogClass _cc;
        private string _tempName;

        public FactoriesChildPage()
        {
            InitializeComponent();

            EditGrid.Height = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetClasses();
            SetBindings();

            FactoriesListBox.SelectedIndex = 0;
        }

        private void GetClasses()
        {
            App.BaseClass.GetCatalogClass(ref _cc);
        }

        private void SetBindings()
        {
            FactoriesListBox.ItemsSource = _cc.GetFactories();
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

        private void FactoriesListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NameTextBox.DataContext = FactoriesListBox.SelectedItem;
            ShowEditGrid();

            OkButton.Content = "Сохранить";

            _tempName = NameTextBox.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.DataContext != null)
                ((DataRowView)FactoriesListBox.SelectedItem)["FactoryName"] = _tempName;

            HideEditGrid();
            NameTextBox.DataContext = null;
            NameTextBox.Text = string.Empty;
        }

        private void LockControls(bool isEnabled = false)
        {
            ButtonsStackPanel.IsEnabled = isEnabled;
            FactoriesListBox.IsEnabled = isEnabled;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Trim() == string.Empty) return;

            if (NameTextBox.DataContext == null)
                _cc.AddFactory(NameTextBox.Text.Trim());

            _cc.SaveFactories();

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
            if (FactoriesListBox.SelectedItem == null) return;

            var result =
                MessageBox.Show(
                    "Удалить запись '" + ((DataRowView)FactoriesListBox.SelectedItem)["FactoryName"] +
                    "' ?",
                    "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ((DataRowView)FactoriesListBox.SelectedItem)["Visible"] = false;
                _cc.SaveFactories();
            }
        }

        private void FactoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameTextBox.DataContext == null) return;

            NameTextBox.DataContext = FactoriesListBox.SelectedItem;
        }
    }
}
