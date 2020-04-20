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
    /// Логика взаимодействия для ProductionStatusesCatalog.xaml
    /// </summary>
    public partial class ProductionStatusesCatalog : Page
    {
        private StaffClass _sc;

        public ProductionStatusesCatalog()
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            BindingData();
        }

        private void BindingData()
        {
            ProductionStatusesListBox.DisplayMemberPath = "ProdStatusName";
            ProductionStatusesListBox.SelectedValuePath = "ProdStatusID";
            ProductionStatusesListBox.ItemsSource = _sc.GetProductionStatuses();
            if (ProductionStatusesListBox.HasItems)
                ProductionStatusesListBox.SelectedIndex = 0;
        }

        // Close catalog
        private void ProductionStatusesCancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        // Change production status
        private void ProductionStatusesSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductionStatusesListBox.SelectedItem == null || string.IsNullOrEmpty(ProductionStatusesNameTextBox.Text) ||
                ProductionStatusesColorPicker.SelectedValue == null) return;

            int prodStatusId = Convert.ToInt32(ProductionStatusesListBox.SelectedValue);
            string prodStatusName = ProductionStatusesNameTextBox.Text;
            string prodStatusNotes = ProductionStatusesNotesTextBox.Text;
            object prodStatusColor = ProductionStatusesColorPicker.SelectedValue;

            var filterNameStr = 
                string.Format("ProdStatusID <> {0} AND ProdStatusName = '{1}' AND Enable = 'True'",
                prodStatusId, prodStatusName);

            if (ContainsProdStatusName(filterNameStr))
            {
                MetroMessageBox.Show("Навык с таким названием уже существует в базе!", "Внимание", MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                return;
            }

            var filterColorStr =
                string.Format("ProdStatusID <> {0} AND ProdStatusColor = '{1}' AND Enable = 'True'",
                prodStatusId, prodStatusColor);

            if (ContainsProdStatusColor(filterColorStr))
            {
                MetroMessageBox.Show("Навык с таким цветом уже существует в базе!", "Внимание", MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                return;
            }

            _sc.ChangeProdStatus(prodStatusId, prodStatusName, prodStatusNotes, prodStatusColor);
            ProductionStatusesDontAddButton_Click(null, null);
        }

        private bool ContainsProdStatusName(string filterStr)
        {
            var findRows = _sc.ProductionStatusesDataTable.Select(filterStr);
            return findRows.Any();
        }

        private bool ContainsProdStatusColor(string filterStr)
        {
            var findRows = _sc.ProductionStatusesDataTable.Select(filterStr);
            return findRows.Any();
        }

        // Delete production status
        private void ProductionStatusesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductionStatusesListBox.SelectedItem == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить выбранный статус?", "Удаление",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int prodStatusId = Convert.ToInt32(ProductionStatusesListBox.SelectedValue);
                _sc.DeleteProdStatus(prodStatusId);

                if (ProductionStatusesListBox.HasItems)
                    ProductionStatusesListBox.SelectedIndex = 0;
            }
        }

        private void ProductionStatusesAddButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
            {
                ProductionStatusesViewGrid.Visibility = Visibility.Hidden;
                ProductionStatusesRedactorGrid.Visibility = Visibility.Visible;
                ProductionStatusesRedactorGrid.DataContext = null;
                ProductionStatusesOkButton.Visibility = Visibility.Visible;
                ProductionStatusesSaveButton.Visibility = Visibility.Hidden;

                opacityAnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
        }

        private void ProductionStatusesDontAddButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));

            opacityAnimation.Completed += (s, args) =>
            {
                ProductionStatusesViewGrid.Visibility = Visibility.Visible;
                ProductionStatusesRedactorGrid.Visibility = Visibility.Hidden;

                if (ProductionStatusesListBox.Items.Count != 0)
                    ProductionStatusesListBox.SelectedIndex = 0;

                opacityAnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);

            ProductionStatusesListBox.IsEnabled = true;
        }

        // Add new production status
        private void ProductionStatusesOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductionStatusesNameTextBox.Text) || ProductionStatusesColorPicker.SelectedValue == null)
                return;

            var prodStatusName = ProductionStatusesNameTextBox.Text;
            var prodStatusColor = ProductionStatusesColorPicker.SelectedValue;
            var prodStatusNotes = ProductionStatusesNotesTextBox.Text;
            var filterNameStr = string.Format("ProdStatusName = '{0}' AND Enable = 'True'", prodStatusName);

            if (ContainsProdStatusName(filterNameStr))
            {
                MetroMessageBox.Show("Навык с таким названием уже существует в базе!", "Внимание", MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                return;
            }

            var filterColorStr = string.Format("ProdStatusColor = '{0}' AND Enable = 'True'", prodStatusColor);

            if (ContainsProdStatusColor(filterNameStr))
            {
                MetroMessageBox.Show("Навык с таким цветом уже существует в базе!", "Внимание", MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                return;
            }

            _sc.AddNewProdStatus(prodStatusName, prodStatusColor, prodStatusNotes);
            ProductionStatusesDontAddButton_Click(null, null);
        }

        private void ProductionStatusesChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductionStatusesListBox.SelectedItem == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
            {
                ProductionStatusesViewGrid.Visibility = Visibility.Hidden;
                ProductionStatusesRedactorGrid.Visibility = Visibility.Visible;
                ProductionStatusesRedactorGrid.DataContext = null;
                ProductionStatusesRedactorGrid.DataContext = ProductionStatusesListBox.SelectedItem;
                ProductionStatusesOkButton.Visibility = Visibility.Hidden;
                ProductionStatusesSaveButton.Visibility = Visibility.Visible;

                opacityAnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);

            ProductionStatusesListBox.IsEnabled = false;
        }

        private void ProductionStatusesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ProductionStatusesChangeButton_Click(null, null);
        }
    }
}
