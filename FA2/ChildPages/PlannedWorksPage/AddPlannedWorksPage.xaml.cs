using FA2.Classes;
using FA2.XamlFiles;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace FA2.ChildPages.PlannedWorksPage
{
    /// <summary>
    /// Логика взаимодействия для AddPlannedWorksPage.xaml
    /// </summary>
    public partial class AddPlannedWorksPage : Page
    {
        private bool _fullAccess;
        private DataRowView _plannedWorkView;
        private PlannedWorksClass _plannedWorksClass;

        public AddPlannedWorksPage(bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;

            FillData();
            SetBindings();

            IsMultiplePlannedWorksCheckBox.IsChecked = true;

            AddPlannedWorkButton.Visibility = Visibility.Visible;
            ChangePlannedWorkButton.Visibility = Visibility.Collapsed;
        }

        public AddPlannedWorksPage(DataRowView plannedWorkView, bool fullAccess)
        {
            InitializeComponent();

            _fullAccess = fullAccess;
            _plannedWorkView = plannedWorkView;

            if(plannedWorkView == null)
            {
                MessageBox.Show("Попробуйте заново окрыть окно редактирования. Программе не удалось получить корректные данные. " +
                    "\nВ случае повторения, попробуйте перезапустить программу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            FillData();
            SetBindings();
            SetDataValues(_plannedWorkView);

            AddPlannedWorkButton.Visibility = Visibility.Collapsed;
            ChangePlannedWorkButton.Visibility = Visibility.Visible;
        }

        private void FillData()
        {
            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);
        }

        private void SetBindings()
        {
            PlannedWorksTypesComboBox.ItemsSource = _plannedWorksClass.GetPlannedWorksTypes();
            if (PlannedWorksTypesComboBox.HasItems)
                PlannedWorksTypesComboBox.SelectedIndex = 0;
        }

        private void SetDataValues(DataRowView plannedWorksView)
        {
            if (plannedWorksView == null) return;

            var plannedWorksTypeId = Convert.ToInt32(plannedWorksView["PlannedWorksTypeID"]);
            var plannedWorksName = plannedWorksView["PlannedWorksName"].ToString();
            var description = plannedWorksView["Description"] != DBNull.Value
                ? plannedWorksView["Description"].ToString()
                : string.Empty;
            var isMultiple = Convert.ToBoolean(plannedWorksView["IsMultiple"]);
            var isReloadEnable = Convert.ToBoolean(plannedWorksView["IsReloadEnable"]);

            PlannedWorksTypesComboBox.SelectedValue = plannedWorksTypeId;
            PlannedWorksNameTextBox.Text = plannedWorksName;
            PlannedWorksDescriptionTextBox.Text = description;
            IsReloadEnablePlannedWorksCheckBox.IsChecked = isReloadEnable;
            IsMultiplePlannedWorksCheckBox.IsChecked = isMultiple;
        }


        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnAddPlannedWorkButtonClick(object sender, RoutedEventArgs e)
        {
            if (PlannedWorksTypesComboBox.SelectedItem == null || string.IsNullOrEmpty(PlannedWorksNameTextBox.Text.Trim()) 
                || !IsMultiplePlannedWorksCheckBox.IsChecked.HasValue || !IsReloadEnablePlannedWorksCheckBox.IsChecked.HasValue) return;

            var plannedWorksTypeId = Convert.ToInt32(PlannedWorksTypesComboBox.SelectedValue);
            var plannedWorksName = PlannedWorksNameTextBox.Text.Trim();
            var description = PlannedWorksDescriptionTextBox.Text.Trim();
            var isMultiple = IsMultiplePlannedWorksCheckBox.IsChecked.Value;
            var isReloadEnable = IsReloadEnablePlannedWorksCheckBox.IsChecked.Value;
            var initiativeType = _fullAccess ? PlannedWorksInitiative.ByMentors : PlannedWorksInitiative.ByWorkers;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var currentWorkerId = AdministrationClass.CurrentWorkerId;

            _plannedWorksClass.AddPlannedWorks(initiativeType, plannedWorksTypeId, currentDate, currentWorkerId, plannedWorksName, description, isMultiple, isReloadEnable);
            AdministrationClass.AddNewAction(45);

            OnClosePageButtonClick(null, null);
        }

        private void OnChangePlannedWorkButtonClick(object sender, RoutedEventArgs e)
        {
            if (_plannedWorkView == null) return;
            if (PlannedWorksTypesComboBox.SelectedItem == null || string.IsNullOrEmpty(PlannedWorksNameTextBox.Text.Trim())
                || !IsMultiplePlannedWorksCheckBox.IsChecked.HasValue || !IsReloadEnablePlannedWorksCheckBox.IsChecked.HasValue) return;

            var plannedWorksId = Convert.ToInt64(_plannedWorkView["PlannedWorksID"]);
            var plannedWorksTypeId = Convert.ToInt32(PlannedWorksTypesComboBox.SelectedValue);
            var plannedWorksName = PlannedWorksNameTextBox.Text.Trim();
            var description = PlannedWorksDescriptionTextBox.Text.Trim();
            var isMultiple = IsMultiplePlannedWorksCheckBox.IsChecked.Value;
            var isReloadEnable = IsReloadEnablePlannedWorksCheckBox.IsChecked.Value;

            _plannedWorksClass.ChangePlannedWorks(plannedWorksId, plannedWorksTypeId, plannedWorksName, description, isMultiple, isReloadEnable);
            AdministrationClass.AddNewAction(49);

            OnClosePageButtonClick(null, null);
        }


        private void OnIsMultiplePlannedWorksCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            IsReloadEnablePlannedWorksCheckBox.IsEnabled = false;
            IsReloadEnablePlannedWorksCheckBox.IsChecked = true;
        }

        private void OnIsMultiplePlannedWorksCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            IsReloadEnablePlannedWorksCheckBox.IsEnabled = true;
        }
    }
}
