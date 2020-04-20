using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.AdministrationPage
{
    /// <summary>
    /// Логика взаимодействия для ModulesRedactor.xaml
    /// </summary>
    public partial class ModulesRedactor
    {
        private readonly AdministrationClass _admc;

        public ModulesRedactor()
        {
            InitializeComponent();
            App.BaseClass.GetAdministrationClass(ref _admc);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ModulesViewListBox.ItemsSource = _admc.ModulesView;
            if (ModulesViewListBox.Items.Count != 0)
                ModulesViewListBox.SelectedIndex = 0;
        }

        // Edit choosen module
        private void EditModuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModulesViewListBox.SelectedItem == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
                                          {
                                              ModulesViewGrid.Visibility = Visibility.Hidden;
                                              ModulesRedactorGrid.Visibility = Visibility.Visible;
                                              ChangeModuleButton.Visibility = Visibility.Visible;
                                              AddNewModuleButton.Visibility = Visibility.Hidden;

                                              ModulesRedactorGrid.DataContext = null;
                                              ModulesRedactorGrid.DataContext = ModulesViewListBox.SelectedItem;
                                              TileButton.IconTile =
                                                  ((DataRowView) ModulesViewListBox.SelectedItem).Row["ModuleIcon"] !=
                                                  DBNull.Value
                                                      ? AdministrationClass.ObjectToBitmapImage(
                                                          ((DataRowView) ModulesViewListBox.SelectedItem).Row[
                                                              "ModuleIcon"])
                                                      : null;

                                              opacityAnimation = new DoubleAnimation(1,
                                                  new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                                              OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void ModulesViewListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditModuleButton_Click(null, null);
        }

        // Add new module
        private void AddModuleButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
                                          {
                                              ModulesViewGrid.Visibility = Visibility.Hidden;
                                              ModulesRedactorGrid.Visibility = Visibility.Visible;
                                              AddNewModuleButton.Visibility = Visibility.Visible;
                                              ChangeModuleButton.Visibility = Visibility.Hidden;

                                              ModulesRedactorGrid.DataContext = null;
                                              TileButton.IconTile = null;

                                              opacityAnimation = new DoubleAnimation(1,
                                                  new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                                              OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        // Close editing choosen module
        private void CancelEditModuleButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
                                          {
                                              ModulesViewGrid.Visibility = Visibility.Visible;
                                              ModulesRedactorGrid.Visibility = Visibility.Hidden;
                                              ModulesRedactorGrid.DataContext = null;
                                              ModuleNameTextBox.IsEmphasized = false;
                                              ModuleDescriptionTextBox.IsEmphasized = false;
                                              ModuleColorTextBox.IsEmphasized = false;

                                              opacityAnimation = new DoubleAnimation(1,
                                                  new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                                              OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        // Close module redactor
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        // Change module icon
        private void ModuleIconImage_MouseDown(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF,*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.* "
            };

            if (dlg.ShowDialog() == true)
            {
                const int newX = 100;

                System.Drawing.Bitmap resizedImage;
                using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(dlg.FileName))
                {
                    double x = originalImage.Width;
                    double y = originalImage.Height;
                    double newYd = newX * (y / x);
                    int newY = Convert.ToInt32(newYd);
                    resizedImage = new System.Drawing.Bitmap(originalImage, newX, newY);
                }

                TileButton.IconTile = AdministrationClass.BitmapToBitmapImage(resizedImage);
            }
        }

        // Save changes
        private void ChangeModuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModulesViewListBox.SelectedItem == null || string.IsNullOrEmpty(ModuleNameTextBox.Text) ||
                string.IsNullOrEmpty(ModuleDescriptionTextBox.Text) || string.IsNullOrEmpty(ModuleColorTextBox.Text) ||
                TileButton.IconTile == null) return;

            Color moduleColor;
            try
            {
                var convertFrom = new ColorConverter().ConvertFrom(ModuleColorTextBox.Text);
                moduleColor = (Color)convertFrom;
            }
            catch
            {
                return;
            }

            var moduleId = Convert.ToInt32(ModulesViewListBox.SelectedValue);
            var moduleName = ModuleNameTextBox.Text;
            var moduleDescription = ModuleDescriptionTextBox.Text;
            var iconData = AdministrationClass.BitmapImageToByte((BitmapImage)TileButton.IconTile);
            var showInFileStorage = Convert.ToBoolean(ShowInFileStorageCheckBox.IsChecked);
            var isSwitchOff = Convert.ToBoolean(IsSwitchOffCheckBox.IsChecked);

            _admc.ChangeModule(moduleId, moduleName, moduleDescription, iconData,
                moduleColor, showInFileStorage, isSwitchOff);
            AdministrationClass.AddNewAction(105);

            CancelEditModuleButton_Click(null, null);
        }

        // Add new module to database
        private void AddNewModuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ModuleNameTextBox.Text) || string.IsNullOrEmpty(ModuleDescriptionTextBox.Text) ||
                string.IsNullOrEmpty(ModuleColorTextBox.Text) || TileButton.IconTile == null) return;

            Color moduleColor;
            try
            {
                var convertFrom = new ColorConverter().ConvertFrom(ModuleColorTextBox.Text);
                moduleColor = (Color)convertFrom;
            }
            catch
            {
                return;
            }

            var moduleName = ModuleNameTextBox.Text;
            var modules = _admc.ModulesTable.Select(string.Format("ModuleName = '{0}'", moduleName));
            if(modules.Length != 0)
            {
                MetroMessageBox.Show("Модуль с таким названием уже существует!", "Предупреждение",
                     MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var moduleDescription = ModuleDescriptionTextBox.Text;
            var iconData = AdministrationClass.BitmapImageToByte((BitmapImage)TileButton.IconTile);
            var showInFileStorage = Convert.ToBoolean(ShowInFileStorageCheckBox.IsChecked);
            var isSwitchOff = Convert.ToBoolean(IsSwitchOffCheckBox.IsChecked);

            _admc.AddNewModule(moduleName, moduleDescription, iconData,
                moduleColor, showInFileStorage, isSwitchOff);
            AdministrationClass.AddNewAction(103);

            CancelEditModuleButton_Click(null, null);
        }

        // Delete choosen module
        private void DeleteModuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModulesViewListBox.SelectedItem == null) return;

            if(MetroMessageBox.Show("Вы действительно хотите удалить выбранный модуль?", "Удаление",
                 MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var moduleId = Convert.ToInt32(ModulesViewListBox.SelectedValue);
                _admc.DeleteModule(moduleId);
                AdministrationClass.AddNewAction(104);
            }
        }

        // Change tile button color
        private void ModuleColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var brushText = ModuleColorTextBox.Text;
            try
            {
                var brush = (Brush)new BrushConverter().ConvertFrom(brushText);
                TileButton.Background = brush;
            }
            catch
            {
                TileButton.Background = Brushes.WhiteSmoke;
            }
            
        }
    }
}
