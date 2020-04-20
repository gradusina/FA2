using FA2.ChildPages.TaskPage;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FA2.ChildPages.PlannedWorksPage
{
    /// <summary>
    /// Логика взаимодействия для ChooseEmptyWorkReasonPage.xaml
    /// </summary>
    public partial class ChooseEmptyWorkReasonPage : Page
    {
        private PlannedWorksClass _plannedWorksClass;
        private DataRowView _plannedWorksRowView;

        public ChooseEmptyWorkReasonPage(DataRowView plannedWorksRowView)
        {
            InitializeComponent();

            _plannedWorksRowView = plannedWorksRowView;
            FillData();
            BindingData();
        }

        private void FillData()
        {
            App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);
        }

        private void BindingData()
        {
            EmptyWorkReasonsListBox.ItemsSource = _plannedWorksClass.GetEmptyWorkReasons();
        }

        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnChooseEmptyWorkReasonButtonClick(object sender, RoutedEventArgs e)
        {
            var emptyWorkReason = EmptyWorkReasonsListBox.SelectedItem as DataRowView;
            if (emptyWorkReason == null) return;

            var emptyWorkReasonId = Convert.ToInt32(emptyWorkReason["EmptyWorkReasonID"]);
            PlannedWorksClass.SelectedEmptyWorkReasonId = emptyWorkReasonId;

            var globalId = _plannedWorksRowView["GlobalID"].ToString();
            var plannedWorksName = _plannedWorksRowView["PlannedWorksName"].ToString();
            var description = _plannedWorksRowView["Description"].ToString();

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var newTaskPage = new AddNewTask(globalId, plannedWorksName, description, TaskClass.SenderApplications.PlannedWorks);
                mainWindow.ShowCatalogGrid(newTaskPage, "Выбрать исполнителей");
            }
        }
    }
}
