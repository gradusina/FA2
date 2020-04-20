using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ToolsPages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        private StaffClass _sc;

        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            int result = _sc.ChangePassword(OldPassPasswordBox.Password.Trim(), NewPassPasswordBox.Password.Trim(),
                NewPass2PasswordBox.Password.Trim(), AdministrationClass.CurrentWorkerId);

            OldPassPasswordBox.Password = string.Empty;
            NewPassPasswordBox.Password = string.Empty;
            NewPass2PasswordBox.Password = string.Empty;

            switch (result)
            {
                case 1:
                {
                    OldPassPasswordBox.EmphasisText = "Неверный пароль";
                    OldPassPasswordBox.IsEmphasized = true;
                    break;
                }

                case 2:
                {
                    NewPassPasswordBox.EmphasisText = "Пароли не совпадают";
                    NewPassPasswordBox.IsEmphasized = true;

                    NewPass2PasswordBox.EmphasisText = "Пароли не совпадают";
                    NewPass2PasswordBox.IsEmphasized = true;

                    break;
                }

                case 3:
                {
                    MessageBox.Show("Ошибка сохранения пароля!", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    break;
                }

                case 4:
                {
                    MessageBox.Show("Пароль изменен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideToolsGrid();
                    break;
                }

                case 5:
                {
                    MessageBox.Show(
                        "Минимальная длина пароля состовляет 4 символа!\nИзмените новый пароль, чтобы он соответствовал требованиям",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                    NewPassPasswordBox.EmphasisText = "Короткий пароль";
                    NewPassPasswordBox.IsEmphasized = true;

                    NewPass2PasswordBox.EmphasisText = "Короткий пароль";
                    NewPass2PasswordBox.IsEmphasized = true;

                    break;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.BaseClass.GetStaffClass(ref _sc);

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null) mainWindow.HideWaitAnnimation();
        }
    }
}
