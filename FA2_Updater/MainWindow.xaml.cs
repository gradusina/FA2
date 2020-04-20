using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace FA2_Updater
{
    public partial class MainWindow
    {
        #region connection_check

        readonly string _mainDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FA2");

        private static string _connectionArg;

        private const string ModeParameters = "-m, -w, -mw";

        private static string[] _arguments;

        private bool _canRun;

        public struct FilesInfoStruct
        {
            public string Name;
            public string FullName;
            public DateTime Date;
        }

        public struct ProgressStruct
        {
            public string StatusText;
            public decimal DownloadPerc;
        }

        private static FilesInfoStruct[] _filesStruct;
        private static FilesInfoStruct[] _serverFilesStruct;

        readonly BackgroundWorker _updateBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };

        public MainWindow()
        {
            InitializeComponent();
            //ShowInTaskbar = true;
           // this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //----------------------------------------------------
            //----------------------------------------------------
            StatusTextBlock.Text = "Проверка наличия обновлений";
            //----------------------------------------------------
            //----------------------------------------------------

            _updateBackgroundWorker.ProgressChanged += (s, args) =>
            {
                CommonProgressLabel.Content = args.ProgressPercentage;
                UpdateProgressBar.Value = args.ProgressPercentage;

                var ps = (ProgressStruct) args.UserState;

                if (!ps.Equals(null))
                {
                    if (ps.StatusText.Trim() != string.Empty)
                    {
                        StatusTextBlock.Text = ps.StatusText;
                    }

                    if (ps.StatusText.Trim() != string.Empty)
                    {
                        FileProgressLabel.Content = Math.Round(ps.DownloadPerc, 2);
                        DownloadProgressBar.Value = Convert.ToInt32(Math.Round(ps.DownloadPerc, 0));
                    }
                }
            };

            _updateBackgroundWorker.DoWork += (s, args) =>
            {
                _canRun = CheckCanRun();

                switch (_connectionArg)
                {
                    case "-lc":
                        App.FtpUrl = @"ftp://192.168.1.200/fa2_update/";
                        break;
                    case "-ic":
                        App.FtpUrl = @"ftp://82.209.219.219:21/fa2_update/";
                        break;
                    case "-mc":
                        App.FtpUrl = @"ftp://192.168.1.200/fa2_update/";
                        break;
                }

                CheckUpdates();

                var ps = new ProgressStruct {DownloadPerc = 0, StatusText = "Запуск программы"};
                _updateBackgroundWorker.ReportProgress(100, ps);
            };

            _updateBackgroundWorker.RunWorkerCompleted += (s, args) =>
            {
                //----------------------------------------------------
                //----------------------------------------------------
                StatusTextBlock.Text = "Запуск программы";
                //----------------------------------------------------
                //----------------------------------------------------

                _arguments = App.AppArguments;

                string startArg = "-w";

                if (_arguments.Length != 0)
                {
                    startArg = _arguments[0];
                }

                if (!ModeParameters.Contains(startArg))
                {
                    MessageBox.Show("Исполняемый файл программы не найден!",
                        "Запуск невозможен", MessageBoxButton.OK, MessageBoxImage.Error);

                    Environment.Exit(0);
                }

                StatusTextBlock.Text = "Тестирование подключения...";

                if (CheckCanRun())
                {
                    StatusTextBlock.Text = "OK";
                    StatusTextBlock.Text = "Старт программы...";

                    string mainFilePath = Path.Combine(_mainDirectory, "MainProgram");

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(mainFilePath, "FA2.exe"),
                        Arguments = startArg + " " + _connectionArg,
                    };

                    try
                    {
                        Process.Start(startInfo);
                        StatusTextBlock.Text = "OK";
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show("Неверный параметр запуска\n" + exp.Message,
                            "Запуск невозможен", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                    MessageBox.Show(
                        "Ведуться техниеские работы.\nПопробуйте запустить приложение через некоторое время.",
                        "Запуск невозможен", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            };

            _updateBackgroundWorker.RunWorkerAsync();
        }

        private void OnSplashWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimazeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private static bool CheckCanRun()
        {
            bool result = false;

            string conStr = GetConnectionString(2, out _connectionArg);

            const string sql = "SELECT CanRun FROM FAIIAdministration.Settings";

            if (conStr != null)
            {
                using (var conn = new MySqlConnection(conStr))
                {
                    var cmd = new MySqlCommand(sql, conn);
                    conn.Open();
                    result = (Boolean)cmd.ExecuteScalar();
                    conn.Close();
                }
            }
            else
            {
                MessageBox.Show("Нет связи с сервером!", "Запуск невозможен", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Environment.Exit(0);
            }
            return result;
        }

        private static string GetConnectionString(int connectionTimeOut, out string arg)
        {
            var connectionStrings = new List<string>();
            arg = null;

            connectionStrings.Add(Decrypt(ConfigurationManager.AppSettings["Local_Connection"], "24Denis03") +
                                  " Connection Timeout=" + connectionTimeOut);

            connectionStrings.Add(Decrypt(ConfigurationManager.AppSettings["Internet_Connection"], "24Denis03") +
                                  " Connection Timeout=" + connectionTimeOut);

            connectionStrings.Add(Decrypt(ConfigurationManager.AppSettings["MyServer_Connection"], "24Denis03") +
                                  " Connection Timeout=" + connectionTimeOut);

            foreach (
                string connectionStr in
                    connectionStrings.Where(connectStr => connectStr != null))
            {
                try
                {
                    using (var con = new MySqlConnection(connectionStr))
                    {
                        con.Open();
                        con.Close();
                    }

                    switch (connectionStrings.IndexOf(connectionStr))
                    {
                        case 0:
                            arg = "-lc";
                            break;
                        case 1:
                            arg = "-ic";
                            break;

                        case 2:
                            arg = "-mc";
                            break;
                    }
                    return connectionStr;
                }
                catch (MySqlException)
                {

                }
            }
            
            return null;
        }

        private static string Decrypt(string cipherText, string password,
            string salt = "Kosher", string hashAlgorithm = "MD5",
            int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY",
            int keySize = 256)
        {
            if (string.IsNullOrEmpty(cipherText))
                return "";

            byte[] initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm,
                passwordIterations);
            // ReSharper disable once CSharpWarnings::CS0618
            byte[] keyBytes = derivedPassword.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount;

            using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream(cipherTextBytes))
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {
                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            symmetricKey.Clear();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        #endregion


        private void CheckUpdates()
        {
            string versionString = GetServerVersion(App.FtpUrl, App.FtpUser, App.FtpPass);
            if (versionString != string.Empty)
            {
                long servVerNumb = Convert.ToInt64(versionString.Replace(".", ""));

                string verNumbStr = string.Empty;

                string filePath = Path.Combine(_mainDirectory, "ver");

                if (File.Exists(filePath)) 
                    verNumbStr = File.ReadAllText(filePath).Trim();


                if (verNumbStr != string.Empty)
                {
                    long verNumb = Convert.ToInt64(verNumbStr.Replace(".", ""));

                    if (servVerNumb > verNumb)
                    {
                        GetFilesInFtpDirectory(App.FtpUrl, App.FtpUser, App.FtpPass);

                        GetFilesInDirectory();

                        if (_serverFilesStruct != null && _filesStruct != null)
                        {

                            //List<FilesInfoStruct> l1 = GetFilesNamesForDelete();
                            //List<FilesInfoStruct> l2 = GetFilesNamesForDownload();
                            //List<FilesInfoStruct> l3 = GetFilesNamesForUpdate();


                            //MessageBox.Show("Кол-во файлов: " + _filesStruct.Count() + "\n" +
                            //                "Кол-во файлов на сервере :" + _serverFilesStruct.Count() + "\n" +
                            //                "Файлов для удаления: " + l1.Count + "\n" +
                            //                "Файлов для загрузки: " + l2.Count + "\n" +
                            //                "Файлов для обновления: " + l3.Count);


                            Dispatcher.BeginInvoke(new Action(
                                delegate
                                {
                                    ProgressGrid.Visibility = Visibility.Visible;
                                    WaitCanvas.Opacity = 0;
                                }));

                            UpdateProgram();

                        }
                    }
                }
            }
        }

        private static string GetServerVersion(string url, string username, string password)
        {
            try
            {
                var tmpReq =
                    (FtpWebRequest) WebRequest.Create(url+"ver");
                tmpReq.Credentials = new NetworkCredential(username, password);

                using (WebResponse tmpRes = tmpReq.GetResponse())
                {
                    using (Stream tmpStream = tmpRes.GetResponseStream())
                    {
                        using (TextReader tmpReader = new StreamReader(tmpStream))
                        {
                            return tmpReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Сервер обновлений недоступен!\n" + exp.Message , "Нет доступа", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return string.Empty;
            }
        }

        private void UpdateProgram()
        {
            bool isUpdateOK = true;

            try
            {
                string updatesPath = Path.Combine(_mainDirectory, "update");

                if (Directory.Exists(updatesPath))
                    Directory.Delete(updatesPath, true);

                Directory.CreateDirectory(updatesPath);

                List<FilesInfoStruct> l3 = GetFilesNamesForUpdate();

                //----------------------------------------------------
                //----------------------------------------------------
                var ps = new ProgressStruct {DownloadPerc = 0, StatusText = "Загрузка обновлений"};
                _updateBackgroundWorker.ReportProgress(5, ps);
                //----------------------------------------------------
                //----------------------------------------------------
               //Thread.Sleep(3000);

                decimal perc = 100 / l3.Count;
                decimal allperc = 0;

                foreach (var filesInfoStruct in l3)
                {
                    using (var request = new WebClient())
                    {
                        string filePath = Path.Combine(updatesPath, filesInfoStruct.Name);

                        try
                        {
                            request.Credentials = new NetworkCredential(App.FtpUser, App.FtpPass);
                            byte[] fileData = request.DownloadData(filesInfoStruct.FullName);

                            using (FileStream file = File.Create(filePath))
                            {
                                file.Write(fileData, 0, fileData.Length);
                                file.Close();
                            }

                            allperc = allperc + perc;

                            //----------------------------------------------------
                            //----------------------------------------------------
                            var ps5 = new ProgressStruct { DownloadPerc = allperc, StatusText = "Загрузка обновлений" };
                            _updateBackgroundWorker.ReportProgress(15, ps5);
                            //----------------------------------------------------
                            //----------------------------------------------------
                            //Thread.Sleep(500);
                        }
                        catch (Exception)
                        {
                            File.Delete(filePath);
                            isUpdateOK = false;
                        }
                    }
                }


                if (isUpdateOK)
                {

                    //----------------------------------------------------
                    //----------------------------------------------------
                    var ps2 = new ProgressStruct
                    {
                        DownloadPerc = 100,
                        StatusText = "Завершение основного процесса программы"
                    };
                    _updateBackgroundWorker.ReportProgress(30, ps2);
                    //----------------------------------------------------
                    //----------------------------------------------------
                    //Thread.Sleep(3000);

                    try
                    {
                        var currentUser = WindowsIdentity.GetCurrent().Name;


                        // Start the child process.
                        var p = new Process
                        {
                            StartInfo =
                            {
                                CreateNoWindow = false,
                                UseShellExecute = false,
                                FileName = "TASKKILL",
                                WindowStyle = ProcessWindowStyle.Normal,

                                Arguments = currentUser != null ? "/F /FI \"USERNAME eq " + currentUser + "\" /IM fa2.exe" : "/F /IM fa2.exe"
                            }
                        };

                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception msgEx)
                    {
                        MessageBox.Show(msgEx.Message);
                    }

                    //----------------------------------------------------
                    //----------------------------------------------------
                    var ps1 = new ProgressStruct()
                    {
                        DownloadPerc = 100,
                        StatusText = "Удаление устаревших версий файлов программы"
                    };
                    _updateBackgroundWorker.ReportProgress(45, ps1);

                    //----------------------------------------------------
                    //----------------------------------------------------
                    //Thread.Sleep(3000);

                    foreach (var filesInfoStruct in l3)
                    {
                        if (filesInfoStruct.Name == "report") continue;

                        string destFilePath = Path.Combine(Path.Combine(_mainDirectory, "MainProgram"), filesInfoStruct.Name);

                        try
                        {
                            File.Delete(destFilePath);
                        }
                        catch
                        {
                        }
                    }

                    //----------------------------------------------------
                    //----------------------------------------------------
                    var ps3 = new ProgressStruct {DownloadPerc = 100, StatusText = "Установка обновлений"};
                    _updateBackgroundWorker.ReportProgress(60, ps3);
                    //----------------------------------------------------
                    //----------------------------------------------------

                    foreach (var filesInfoStruct in l3)
                    {
                        switch (filesInfoStruct.Name)
                        {
                            case "report":
                                continue;
                            case "ver":
                                try
                                {
                                    File.Copy(Path.Combine(updatesPath, filesInfoStruct.Name),
                                        Path.Combine(_mainDirectory, filesInfoStruct.Name), true);
                                }
                                catch
                                {
                                }
                                continue;

                            default:
                                try
                                {

                                    string sourceFilePath = Path.Combine(updatesPath, filesInfoStruct.Name);
                                    string destFilePath = Path.Combine(Path.Combine(_mainDirectory, "MainProgram"), filesInfoStruct.Name);

                                    File.Copy(sourceFilePath, destFilePath, true);
                                }
                                catch
                                {
                                }
                                break;
                        }
                    }

                }
                else
                {
                    MessageBox.Show("Ошибка обновления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                //----------------------------------------------------
                //----------------------------------------------------
                var ps4 = new ProgressStruct {DownloadPerc = 100, StatusText = "Удаление временных файлов"};
                _updateBackgroundWorker.ReportProgress(95, ps4);

                if (Directory.Exists(updatesPath))
                {
                    Directory.Delete(updatesPath, true);
                }

                //----------------------------------------------------
                //----------------------------------------------------
                var ps6 = new ProgressStruct { DownloadPerc = 100, StatusText = "Удаление временных файлов" };
                _updateBackgroundWorker.ReportProgress(100, ps6);
                //----------------------------------------------------
                //----------------------------------------------------
                //Thread.Sleep(500);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка обновления!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetFilesInDirectory()
        {
            try
            {
                string mainFilePath = Path.Combine(_mainDirectory, "MainProgram");
                var dir = new DirectoryInfo(mainFilePath); // папка с файлами 
                var fi = dir.GetFiles();
                _filesStruct = new FilesInfoStruct[fi.Count()];

                for (int i = 0; i < fi.Count(); i++)
                {
                    _filesStruct[i].Name = Path.GetFileName(fi[i].FullName);
                    _filesStruct[i].FullName = fi[i].FullName;
                    _filesStruct[i].Date = fi[i].LastWriteTimeUtc;
                }
            }
            catch (Exception)
            {
                _filesStruct = null;
            }
        }

        public void GetFilesInFtpDirectory(string url, string username, string password)
        {
            try
            {
                var request = (FtpWebRequest) WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                var response = (FtpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();
                reader.Close();
                response.Close();

                List<string> s = names.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();

                _serverFilesStruct = new FilesInfoStruct[s.Count()];

                for (int i = 0; i < _serverFilesStruct.Count(); i++)
                {
                    var requestForFile = (FtpWebRequest) WebRequest.Create(url + s[i]);
                    requestForFile.Credentials = new NetworkCredential(username, password);
                    requestForFile.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    var responseForFile = (FtpWebResponse) requestForFile.GetResponse();

                    _serverFilesStruct[i].Name = s[i];
                    _serverFilesStruct[i].FullName = url + s[i];
                    _serverFilesStruct[i].Date = responseForFile.LastModified;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Сервер обновлений недоступен!\n" + exp.Message, "Нет доступа", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                _serverFilesStruct = null;
            }
        }

        private static List<FilesInfoStruct> GetFilesNamesForDelete()
        {
            return _filesStruct.Where(fs => !ContainsFilesStruct(_serverFilesStruct, fs, true)).ToList();
        }

        private static List<FilesInfoStruct> GetFilesNamesForDownload()
        {
            //var result = new List<FilesInfoStruct>();

            //foreach (var sfs in serverFilesStruct)
            //{
            //    if (!ContainsFilesStruct(filesStruct, sfs, true))
            //    {
            //        result.Add(sfs);
            //    }
            //}

            //return result;

            return _serverFilesStruct.Where(sfs => !ContainsFilesStruct(_filesStruct, sfs, true)).ToList();
        }

        private static List<FilesInfoStruct> GetFilesNamesForUpdate()
        {
            //var result = new List<FilesInfoStruct>();

            //foreach (var sfs in serverFilesStruct)
            //{
            //    if (!ContainsFilesStruct(filesStruct, sfs))
            //    {
            //        result.Add(sfs);
            //    }
            //}

            //return result;

            return _serverFilesStruct.Where(sfs => !ContainsFilesStruct(_filesStruct, sfs)).ToList();
        }
        
        private static bool ContainsFilesStruct(IEnumerable<FilesInfoStruct> source, FilesInfoStruct value,
            bool nameOnly = false)
        {
            //foreach (var filesInfoStruct in source)
            //{
            //    bool result = (filesInfoStruct.Name == value.Name && filesInfoStruct.Date == value.Date);
            //    if (result)
            //        return true;
            //}

            //return false;

            return nameOnly
                ? source.Select(filesInfoStruct => (filesInfoStruct.Name == value.Name)).Any(result => result)
                : source.Select(
                    filesInfoStruct => (filesInfoStruct.Name == value.Name && filesInfoStruct.Date == value.Date))
                    .Any(result => result);
        }
    }
}
