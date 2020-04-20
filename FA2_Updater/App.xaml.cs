using System.Windows;
using System.Windows.Threading;

namespace FA2_Updater
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    /// 

    public partial class App
    {
        private static string[] _arguments;

        public static string[] AppArguments
        {
            get { return _arguments; }
        }

        public static string FtpUrl { get; set; }

        public static string FtpUser
        {
            get { return @"fa2app"; }
        }

        public static string FtpPass
        {
            get { return @"Franc1961"; }
        }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int i = 0;
            _arguments = new string[e.Args.Length];

            foreach (string arg in e.Args)
            {
                _arguments[i] = arg;
                i++;
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Нарушен процесс обновления программы"+
                e.Exception.Message +
                "\n\nПерезапустите приложение. Если ошибка повторится, обратитесь к администратору.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
