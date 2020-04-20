using FA2.Classes;
using FA2.XamlFiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FA2.ChildPages.PlannedWorksPage
{
    /// <summary>
    /// Логика взаимодействия для PlannedWorksTypesPage.xaml
    /// </summary>
    public partial class PlannedWorksTypesPage : Page
    {
        private PlannedWorksClass _plannedWorksClass;

        public PlannedWorksTypesPage()
        {
            InitializeComponent();
            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);

            BindingData();
        }

        private void BindingData()
        {
            PlannedWorksTypesListBox.ItemsSource = _plannedWorksClass.GetPlannedWorksTypes();
            if (PlannedWorksTypesListBox.HasItems)
                PlannedWorksTypesListBox.SelectedIndex = 0;
        }


        private void OnPlannedWorksTypesCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnPlannedWorksTypesDontAddButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(300, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

            heightAnimation.Completed += (s, args) =>
            {
                PlannedWorksTypesViewGrid.Visibility = Visibility.Visible;
                PlannedWorksTypesRedactorGrid.Visibility = Visibility.Hidden;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        }


        private void OnPlannedWorksTypesAddButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(150, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            heightAnimation.Completed += (s, args) =>
            {
                PlannedWorksTypesViewGrid.Visibility = Visibility.Hidden;
                PlannedWorksTypesRedactorGrid.Visibility = Visibility.Visible;
                PlannedWorksTypesRedactorGrid.DataContext = null;
                PlannedWorksTypesOkButton.Visibility = Visibility.Visible;
                PlannedWorksTypesSaveButton.Visibility = Visibility.Collapsed;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        } 

        private void OnPlannedWorksTypesOkButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksTypeName = PlannedWorksTypeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(plannedWorksTypeName)) return;

            _plannedWorksClass.AddPlannedWorksType(plannedWorksTypeName);
            AdministrationClass.AddNewAction(50);

            OnPlannedWorksTypesDontAddButtonClick(null, null);
        }


        private void OnPlannedWorksTypesChangeButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksType = PlannedWorksTypesListBox.SelectedItem as DataRowView;
            if (plannedWorksType == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(150, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            heightAnimation.Completed += (s, args) =>
            {
                PlannedWorksTypesViewGrid.Visibility = Visibility.Hidden;
                PlannedWorksTypesRedactorGrid.Visibility = Visibility.Visible;
                PlannedWorksTypesRedactorGrid.DataContext = null;
                PlannedWorksTypesRedactorGrid.DataContext = plannedWorksType;
                PlannedWorksTypesOkButton.Visibility = Visibility.Collapsed;
                PlannedWorksTypesSaveButton.Visibility = Visibility.Visible;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        }

        private void OnPlannedWorksTypeItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnPlannedWorksTypesChangeButtonClick(null, null);
        }

        private void OnPlannedWorksTypesSaveButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksType = PlannedWorksTypesListBox.SelectedItem as DataRowView;
            if (plannedWorksType == null) return;

            var plannedWorksTypeName = PlannedWorksTypeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(plannedWorksTypeName)) return;

            var plannedWorksTypeId = Convert.ToInt32(plannedWorksType["PlannedWorksTypeID"]);
            _plannedWorksClass.ChangePlannedWorksType(plannedWorksTypeId, plannedWorksTypeName);
            AdministrationClass.AddNewAction(52);

            OnPlannedWorksTypesDontAddButtonClick(null, null);
        }


        private void OnPlannedWorksTypesDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var plannedWorksType = PlannedWorksTypesListBox.SelectedItem as DataRowView;
            if (plannedWorksType == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить данный тип работ?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var plannedWorksTypeId = Convert.ToInt32(plannedWorksType["PlannedWorksTypeID"]);
            _plannedWorksClass.DeletePlannedWorksType(plannedWorksTypeId);
            AdministrationClass.AddNewAction(51);
        }
    }
}
