using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для EditStaffEducation.xaml
    /// </summary>
    public partial class EditStaffEducation
    {
        private StaffClass _sc;
        private int _workerId;

        public EditStaffEducation(int workerId)
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            _workerId = workerId;
            BindingData();
        }

        private void BindingData()
        {
            BindingStaffEducationInstitutionTypeComboBox();
            BindingYearGraduationComboBox();

            var staffEducationView = _sc.GetStaffEducation();
            staffEducationView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            StaffEducationListBox.SelectedValuePath = "StaffEducationID";
            StaffEducationListBox.ItemsSource = staffEducationView;
            if (StaffEducationListBox.HasItems)
                StaffEducationListBox.SelectedIndex = 0;

            InstitutionNameCompleteBox.DataSource = _sc.GetStaffEducation();
            InstitutionNameCompleteBox.DisplayMemberPath = "InstitutionName";
            InstitutionNameCompleteBox.SelectedValuePath = "StaffEducationID";
        }

        private void BindingStaffEducationInstitutionTypeComboBox()
        {
            StaffEducationInstitutionTypeComboBox.ItemsSource = _sc.GetEducationInstitutionTypes();
            StaffEducationInstitutionTypeComboBox.DisplayMemberPath = "InstitutionTypeName";
            StaffEducationInstitutionTypeComboBox.SelectedValuePath = "InstitutionTypeID";
        }

        private void BindingYearGraduationComboBox()
        {
            var currentYear = App.BaseClass.GetDateFromSqlServer().Year;
            while(currentYear >= 1965)
            {
                YearGraduationComboBox.Items.Add(currentYear);
                currentYear--;
            }
        }

        private void ShowSecondStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
           if(AdministrationClass.AllowAnnimations)
           {
               var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
               opacityAnnimation.Completed += (s, args) =>
               {
                   AddProcedure();

                   opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                   StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
               };

               StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
           }
           else
           {
               AddProcedure();
           }
        }

        private void AddProcedure()
        {
            StaffEducationViewGrid.Visibility = Visibility.Hidden;
            StaffEducationRedactorGrid.Visibility = Visibility.Visible;
            AddStaffEducationButton.Visibility = Visibility.Visible;
            ChangeStaffEducationButton.Visibility = Visibility.Hidden;
            StaffEducationRedactorGrid.DataContext = null;
            var operations = BindingOperations.GetBindingExpression(InstitutionNameCompleteBox, TextBox.TextProperty);
            if (operations != null)
                operations.UpdateTarget();
            operations = BindingOperations.GetBindingExpression(SpecialtyNameTextBox, TextBox.TextProperty);
            if (operations != null)
                operations.UpdateTarget();
            operations = BindingOperations.GetBindingExpression(QualificationNameTextBox, TextBox.TextProperty);
            if (operations != null)
                operations.UpdateTarget();
            if (StaffEducationInstitutionTypeComboBox.HasItems) StaffEducationInstitutionTypeComboBox.SelectedIndex = 0;
            if (YearGraduationComboBox.HasItems) YearGraduationComboBox.SelectedIndex = 0;
        }

        private void HideSecondStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    CancelProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelProcedure();
            }
        }

        private void CancelProcedure()
        {
            StaffEducationViewGrid.Visibility = Visibility.Visible;
            StaffEducationRedactorGrid.Visibility = Visibility.Hidden;
            StaffEducationListBox.IsEnabled = true;
            if (StaffEducationListBox.HasItems)
                StaffEducationListBox.SelectedIndex = 0;
        }

        private void CancelStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.HideCatalogGrid();
        }

        private void AddStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaffEducationInstitutionTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(InstitutionNameCompleteBox.Text) ||
                YearGraduationComboBox.SelectedItem == null) return;

            var institutionTypeId = Convert.ToInt32(StaffEducationInstitutionTypeComboBox.SelectedValue);
            string institutionName = InstitutionNameCompleteBox.Text;
            var yearGraduation = Convert.ToInt32(YearGraduationComboBox.SelectedItem);
            string specialityName = SpecialtyNameTextBox.Text;
            string qualificationName = QualificationNameTextBox.Text;

            _sc.AddNewStaffEducation(_workerId, institutionTypeId, institutionName, yearGraduation, specialityName, qualificationName);
            HideSecondStaffEducationButton_Click(null, null);
        }

        private void ChangeStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            var staffEducation = StaffEducationListBox.SelectedItem as DataRowView;
            if (staffEducation == null) return;

            if (StaffEducationInstitutionTypeComboBox.SelectedValue == null ||
                string.IsNullOrEmpty(InstitutionNameCompleteBox.Text) || YearGraduationComboBox.SelectedItem == null)
                return;

            var staffEducationId = Convert.ToInt64(staffEducation["StaffEducationID"]);
            var institutionTypeId = Convert.ToInt32(StaffEducationInstitutionTypeComboBox.SelectedValue);
            var institutionName = InstitutionNameCompleteBox.Text;
            var yearGraduation = Convert.ToInt32(YearGraduationComboBox.SelectedItem);
            var specialityName = SpecialtyNameTextBox.Text;
            var qualificationName = QualificationNameTextBox.Text;

            _sc.ChangeStaffEducation(staffEducationId, institutionTypeId, institutionName, yearGraduation, specialityName, qualificationName);

            HideSecondStaffEducationButton_Click(null, null);
        }

        private void DeleteStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            var staffEducation = StaffEducationListBox.SelectedItem as DataRowView;
            if (staffEducation == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранный навык?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var staffEducationId = Convert.ToInt64(staffEducation["StaffEducationID"]);
            _sc.DeleteStaffEducation(staffEducationId);
        }

        private void EditStaffEducationButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaffEducationListBox.SelectedItem == null) return;

            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    EditProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                StaffEducationOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditProcedure();
            }

            StaffEducationListBox.IsEnabled = false;
        }

        private void EditProcedure()
        {
            StaffEducationViewGrid.Visibility = Visibility.Hidden;
            StaffEducationRedactorGrid.Visibility = Visibility.Visible;
            AddStaffEducationButton.Visibility = Visibility.Hidden;
            ChangeStaffEducationButton.Visibility = Visibility.Visible;
            StaffEducationRedactorGrid.DataContext = StaffEducationListBox.SelectedItem;
        }

        private void StaffEducationListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditStaffEducationButton_Click(null, null);
        }
    }
}
