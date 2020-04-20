using System;
using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для AddNewWorker.xaml
    /// </summary>
    public partial class AddNewWorker : Page
    {
        private StaffClass _sc;

        public AddNewWorker()
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            NewWorkerNameTextBox.Focus();
        }

        private void CancelAddNewWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void AddNewWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewWorkerNameTextBox.Text.Trim())) return;

            string workerName = NewWorkerNameTextBox.Text.Trim();

            var dataRows = _sc.StaffPersonalInfoDataTable.Select(string.Format("Name = '{0}'", workerName));
            if(dataRows.Length != 0)
            {
                MetroMessageBox.Show("Работник с таким именем уже добавлен! \nОтобразите список всех работников. Проверьте, возможно данный работник ранее был удалён и для восстановления ему необходимо поменять статус работы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _sc.AddNewWorker(workerName);
            var rows = _sc.StaffPersonalInfoDataTable.Select(string.Format("Name = '{0}'", workerName));
            if(rows.Length != 0)
            {
                AdministrationClass.AddNewWorkerToGroupBySql(Convert.ToInt32(rows[0]["WorkerID"]), 3);
            }


            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var staffPage = mainWindow.MainFrame.Content as XamlFiles.StaffPage;
                if (staffPage != null)
                {
                    if (rows.Length != 0)
                        staffPage.SelectNewWorker(Convert.ToInt32(rows[0]["WorkerID"]));
                }
            }

            CancelAddNewWorkerButton_Click(null, null);
        }
    }
}
