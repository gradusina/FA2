using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FA2.Classes;
using FA2.XamlFiles;
using System.Windows.Media.Animation;
using System.Data;

namespace FA2.ChildPages.PlannedWorksPage
{
    /// <summary>
    /// Логика взаимодействия для EmptyWorkReasonsPage.xaml
    /// </summary>
    public partial class EmptyWorkReasonsPage : Page
    {
        private PlannedWorksClass _plannedWorksClass;

        public EmptyWorkReasonsPage()
        {
            InitializeComponent();
            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);

            BindingData();
        }

        private void BindingData()
        {
            EmptyWorkReasonsListBox.ItemsSource = _plannedWorksClass.GetEmptyWorkReasons();
            if (EmptyWorkReasonsListBox.HasItems)
                EmptyWorkReasonsListBox.SelectedIndex = 0;
        }


        private void OnEmptyWorkReasonsCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnEmptyWorkReasonsDontAddButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(300, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

            heightAnimation.Completed += (s, args) =>
            {
                EmptyWorkReasonsViewGrid.Visibility = Visibility.Visible;
                EmptyWorkReasonsRedactorGrid.Visibility = Visibility.Hidden;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        }


        private void OnEmptyWorkReasonsAddButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(150, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            heightAnimation.Completed += (s, args) =>
            {
                EmptyWorkReasonsViewGrid.Visibility = Visibility.Hidden;
                EmptyWorkReasonsRedactorGrid.Visibility = Visibility.Visible;
                EmptyWorkReasonsRedactorGrid.DataContext = null;
                EmptyWorkReasonsOkButton.Visibility = Visibility.Visible;
                EmptyWorkReasonsSaveButton.Visibility = Visibility.Collapsed;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        }

        private void OnEmptyWorkReasonsOkButtonClick(object sender, RoutedEventArgs e)
        {
            var emptyWorkReasonName = EmptyWorkReasonNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(emptyWorkReasonName)) return;

            _plannedWorksClass.AddEmptyWorkReason(emptyWorkReasonName);
            AdministrationClass.AddNewAction(53);

            OnEmptyWorkReasonsDontAddButtonClick(null, null);
        }


        private void OnEmptyWorkReasonsChangeButtonClick(object sender, RoutedEventArgs e)
        {
            var emptyWorkReason = EmptyWorkReasonsListBox.SelectedItem as DataRowView;
            if (emptyWorkReason == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var heightAnimation = new DoubleAnimation(150, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            heightAnimation.Completed += (s, args) =>
            {
                EmptyWorkReasonsViewGrid.Visibility = Visibility.Hidden;
                EmptyWorkReasonsRedactorGrid.Visibility = Visibility.Visible;
                EmptyWorkReasonsRedactorGrid.DataContext = null;
                EmptyWorkReasonsRedactorGrid.DataContext = emptyWorkReason;
                EmptyWorkReasonsOkButton.Visibility = Visibility.Collapsed;
                EmptyWorkReasonsSaveButton.Visibility = Visibility.Visible;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, heightAnimation);
        }

        private void OnEmptyWorkReasonItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnEmptyWorkReasonsChangeButtonClick(null, null);
        }

        private void OnEmptyWorkReasonsSaveButtonClick(object sender, RoutedEventArgs e)
        {
            var emptyWorkReason = EmptyWorkReasonsListBox.SelectedItem as DataRowView;
            if (emptyWorkReason == null) return;

            var emptyWorkReasonName = EmptyWorkReasonNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(emptyWorkReasonName)) return;

            var emptyWorkReasonId = Convert.ToInt32(emptyWorkReason["EmptyWorkReasonID"]);
            _plannedWorksClass.ChangeEmptyWorkReason(emptyWorkReasonId, emptyWorkReasonName);
            AdministrationClass.AddNewAction(55);

            OnEmptyWorkReasonsDontAddButtonClick(null, null);
        }


        private void OnEmptyWorkReasonsDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var emptyWorkReason = EmptyWorkReasonsListBox.SelectedItem as DataRowView;
            if (emptyWorkReason == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить данную причину?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var emptyWorkReasonId = Convert.ToInt32(emptyWorkReason["EmptyWorkReasonID"]);
            _plannedWorksClass.DeleteEmptyWorkReason(emptyWorkReasonId);
            AdministrationClass.AddNewAction(54);
        }
    }
}
