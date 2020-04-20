using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using FA2.Classes;
using FA2.XamlFiles;
using FAII.Classes;

namespace FA2
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        private static string[] _arguments;

        private static ConnectionClass _cc;

        private static BaseClass _base;

        public static string GetFtpUrl
        {
            get { return _ftpUrl; }
        }

        public static string TempFolder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FA2", "Temp");}
        }

        public static string FtpUser
        {
            get { return @"fa2app"; }
        }

        public static string FtpPass
        {
            get { return @"Franc1961"; }
        }

        private static string _ftpUrl = string.Empty;

        public static string[] AppArguments
        {
            get { return _arguments; }
        }

        public static BaseClass BaseClass
        {
            get { return _base; }
        }

        public static ConnectionClass.ConnectionStruct ConnectionInfo
        {
            get { return _cc.ConnectionString; }
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

            if (!_arguments.Any())
            {
                ShutdownApp();
                return;
            }

            _base = new BaseClass();
            _cc = new ConnectionClass(_arguments[1]);

            switch (_arguments[1])
            {
                case "-lc":
                    _ftpUrl = @"ftp://192.168.1.200/";
                    break;
                case "-ic":
                    _ftpUrl = @"ftp://82.209.219.219/";
                    break;
                case "-mc":
                    _ftpUrl = @"ftp://192.168.1.200/";
                    break;
            }
            //AdministrationClass.GrandFirewallAuthorization();

            Splasher.ShowSplashWindow("Подготовка к началу работы программы", _arguments[0]);
            Splasher.SetStatusText("Пропускай");



            Splasher.SetStatusText("Очистка Temp");
            AdministrationClass.CleatTempFolder();

            //можно выкл
            //-------------------------------------------------
            //Splasher.SetStatusText("Очистка FtpTemp");
            //AdministrationClass.ClearFtpTempFolder();
            //-------------------------------------------------


            Splasher.SetStatusText("Подготовка к началу работы программы завершена");
        }

        private static void ShowWindow()
        {
            var mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.ShowInTaskbar = false;
            mainWindow.Show();
        }

        public static void SW()
        {
            Current.Dispatcher.BeginInvoke(new Action(ShowWindow));
        }

        private void Application_DispatcherUnhandledException(object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                e.Exception.Message +
                "\n\nПерезапустите приложение. Если ошибка повторится, обратитесь к администратору.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);

            var userName = string.Empty;
            var machineName = string.Empty;
            var osVersion = string.Empty;
            var windowsIdentity = WindowsIdentity.GetCurrent();

            if (windowsIdentity != null)
            {
                userName = windowsIdentity.Name.ToString(CultureInfo.InvariantCulture);
                machineName = Environment.MachineName.ToString(CultureInfo.InvariantCulture);
                osVersion = Environment.OSVersion.VersionString;
            }

            var message = e.Exception.Message;
            var source = e.Exception.Source;
            var targetSite = e.Exception.TargetSite.ToString();
            var stackTrace = e.Exception.StackTrace;


            AdministrationClass.SendMessageToServer(userName, machineName, osVersion,
                message, source, targetSite, stackTrace);

            AdministrationClass.SendMessageToReport(userName, machineName, osVersion,
                message, source, targetSite, stackTrace);

            CloseAdministrationJournal();
        }

        //private void Application_DispatcherUnhandledException(object sender,
        //System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{



        //    MessageBox.Show(
        //        e.Exception.Message +
        //        "\n\nПерезапустите приложение. Если ошибка повторится, обратитесь к администратору.", "Ошибка",
        //        MessageBoxButton.OK, MessageBoxImage.Error);

        //    var reportFile = new FileInfo("report");

        //    if (!reportFile.Exists)
        //    {
        //        StreamWriter sw = reportFile.CreateText();
        //        sw.Close();
        //    }

        //    var streamWriter = new StreamWriter("report", true);
        //    streamWriter.Write("\r\n####################################################################");
        //    var windowsIdentity = WindowsIdentity.GetCurrent();
        //    if (windowsIdentity != null)
        //        streamWriter.Write("\r\n" + DateTime.Now + " Пользователь: " +
        //                           windowsIdentity.Name.ToString(
        //                               CultureInfo.InvariantCulture) + " Компьютер: " +
        //                           Environment.MachineName.ToString(CultureInfo.InvariantCulture) + " ОС: " +
        //                           Environment.OSVersion.VersionString);
        //    streamWriter.Write("\r\n --------------------------------------------------------------------");
        //    streamWriter.Write("\r\nОшибка: " + e.Exception.Message);
        //    streamWriter.Write("\r\n --------------------------------------------------------------------");
        //    streamWriter.Write("\r\n" + "Имя приложения или объекта, вызывавшего ошибку: " + e.Exception.Source);
        //    streamWriter.Write("\r\n" + e.Exception.Source);
        //    streamWriter.Write("\r\n --------------------------------------------------------------------");
        //    streamWriter.Write("\r\n" + "Метод, создавший текущее исключение:");
        //    streamWriter.Write("\r\n" + e.Exception.TargetSite);
        //    streamWriter.Write("\r\n --------------------------------------------------------------------");
        //    streamWriter.Write("\r\n" + "Строковое представление непосредственных кадров в стеке вызова:");
        //    streamWriter.Write("\r\n" + e.Exception.StackTrace);
        //    streamWriter.Write("\r\n####################################################################");
        //    streamWriter.Write("\r\n ");
        //    streamWriter.Close();

        //    CloseAdministrationJournal();
        //}

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CloseAdministrationJournal();
        }

        private static void CloseAdministrationJournal()
        {
            AdministrationClass.CloseModuleEntry();
            AdministrationClass.CloseProgramEntry();
        }

        private void ShutdownApp()
        {
            MessageBox.Show("Запуск приложения возможен только с помощью лаунчера!", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);

            if (Current != null)
                Current.Shutdown();
        }

        public static void CloseApp()
        {
            Current.Shutdown();
        }


    }
}
