using FA2.Classes;
using FA2.XamlFiles;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FA2.ChildPages.AdmissionPage
{
    /// <summary>
    /// Логика взаимодействия для AdmissionsPage.xaml
    /// </summary>
    public partial class AdmissionsPage : Page
    {
        AdmissionsClass _admClass;

        public AdmissionsPage()
        {
            InitializeComponent();

            App.BaseClass.GetAdmissionsClass(ref _admClass);
            AdmissionsListBox.ItemsSource = _admClass.GetAdmissionsView();
            AdmissionPeriodNumericControl.Value = decimal.Zero;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnAddAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.Visibility = Visibility.Visible;
                AddNewAddmissionButton.Visibility = Visibility.Visible;
                ChangeAdmissionButton.Visibility = Visibility.Collapsed;
                AdmissionNameTextBox.IsEnabled = true;
                RedactorGrid.DataContext = null;
                AdmissionPeriodNumericControl.Value = decimal.Zero;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnGoBackButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Visible;
                RedactorGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.DataContext = null;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnEditAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var admission = AdmissionsListBox.SelectedItem as DataRowView;
            if (admission == null) return;
            var isLocked = Convert.ToBoolean(admission["IsLocked"]);
            var admissionPeriodEnable = Convert.ToBoolean(admission["AdmissionPeriodEnable"]);

            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                RedactorGrid.Visibility = Visibility.Visible;
                AddNewAddmissionButton.Visibility = Visibility.Collapsed;
                ChangeAdmissionButton.Visibility = Visibility.Visible;
                AdmissionNameTextBox.IsEnabled = !isLocked;
                RedactorGrid.DataContext = admission;
                AdmissionPeriodNumericControl.Value = admissionPeriodEnable
                    ? Convert.ToDecimal(admission["AdmissionPeriod"])
                    : 0;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
        }

        private void OnAddNewAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AdmissionNameTextBox.Text) ||
                (AdmissionPeriodEnableCheckBox.IsChecked.HasValue && AdmissionPeriodEnableCheckBox.IsChecked.Value && 
                (AdmissionPeriodNumericControl.Value == null || Convert.ToInt32(AdmissionPeriodNumericControl.Value) == 0)))
            {
                MessageBox.Show("Присутствуют пустые поля для заполнения. Информация не сохранена.", "Предупреждение", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var admissionName = AdmissionNameTextBox.Text;
            var admissionPeriodEnable = AdmissionPeriodEnableCheckBox.IsChecked.Value;
            int admissionPeriod = admissionPeriodEnable
                ? Convert.ToInt32(AdmissionPeriodNumericControl.Value)
                : 0;

            try
            {
                _admClass.AddAdmission(admissionName, admissionPeriodEnable, admissionPeriod);
                OnGoBackButtonClick(null, null);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnDeleteAdmissionsButtonClick(object sender, RoutedEventArgs e)
        {
            var admission = AdmissionsListBox.SelectedItem as DataRowView;
            if (admission == null) return;

            try
            {
                var admissionId = Convert.ToInt32(admission["AdmissionID"]);
                _admClass.DeleteAdmission(admissionId);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnChangeAdmissionButtonClick(object sender, RoutedEventArgs e)
        {
            var admission = AdmissionsListBox.SelectedItem as DataRowView;
            if (admission == null) return;

            if (string.IsNullOrEmpty(AdmissionNameTextBox.Text) ||
                (AdmissionPeriodEnableCheckBox.IsChecked.HasValue && AdmissionPeriodEnableCheckBox.IsChecked.Value &&
                (AdmissionPeriodNumericControl.Value == null || Convert.ToInt32(AdmissionPeriodNumericControl.Value) == 0)))
            {
                MessageBox.Show("Присутствуют пустые поля для заполнения. Информация не сохранена.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var admissionName = AdmissionNameTextBox.Text;
            var admissionPeriodEnable = AdmissionPeriodEnableCheckBox.IsChecked.Value;
            int admissionPeriod = admissionPeriodEnable
                ? Convert.ToInt32(AdmissionPeriodNumericControl.Value)
                : 0;

            try
            {
                var admissionId = Convert.ToInt32(admission["AdmissionID"]);
                _admClass.ChangeAdmission(admissionId, admissionName, admissionPeriodEnable, admissionPeriod);
                OnGoBackButtonClick(null, null);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
