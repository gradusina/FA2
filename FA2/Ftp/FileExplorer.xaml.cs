using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary.UserControls;
using MySql.Data.MySqlClient;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace FA2.Ftp
{
    /// <summary>
    /// Логика взаимодействия для FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer
    {
        private readonly FtpClient _ftpClient;

        private readonly string _basicDirectory;

        private readonly bool _fullAccess;
        private readonly string _connectionString;
        private long _fileSize;
        private string _neededOpeningFilePath;

        private Thread _listLoadingThread;
        private WaitWindow _processWindow;

        private readonly List<NavigationItem> _navigationItems;

        private readonly Queue<FtpFileDirectoryInfo> _downloadItems = new Queue<FtpFileDirectoryInfo>();
        private string _neededDownloadDirectoryPath;
        private int _downloadedChangesCount;
        private readonly Queue<string> _uploadItems = new Queue<string>();
        private string _neededUploadDirectoryPath;
        private int _neededUploadItemsCount;
        private int _uploadedChangesCount;


        public FileExplorer(int currentModuleId, bool fullAccess)
        {
            InitializeComponent();

            _basicDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/";
            _connectionString = App.ConnectionInfo.ConnectionString;
            _fullAccess = fullAccess;
            DataContext = _fullAccess;

            ShowWaitAnnimation();
            CreateTempFolder();

            InitializeView();

            _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
            //_ftpClient = new FtpClient(BasicDirectory, "fa2prog", "468255PassWord");
            _ftpClient.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            _ftpClient.UploadProgressChanged += OnUploadProgressChanged;

            _navigationItems = new List<NavigationItem>();

            PreviewKeyDown += OnExplorerPanelPreviewKeyDown;

            var thread = new Thread(() =>
                                    {
                                        FillData();

                                        CreateAttachmentsNavigationItem();
                                        CreateCommonFilesNavigationItem();

                                        CreateFolders();

                                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                            new ThreadStart(
                                                () =>
                                                {
                                                    NavigationListBox.ItemsSource = _navigationItems;
                                                    SelectNavigationItem(currentModuleId);
                                                }));
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void InitializeView()
        {
            var visibility = _fullAccess ? Visibility.Visible : Visibility.Collapsed;

            AddNewFilesButton.Visibility = visibility;
            CreateNewFolderButton.Visibility = visibility;
        }

        private static void CreateTempFolder()
        {
            if (Directory.Exists(App.TempFolder)) return;

            try
            {
                Directory.CreateDirectory(App.TempFolder);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Не удаётся создать папку Temp по адресу: " + 
                    App.TempFolder + "\n" + exp.Message);
            }
        }

        private void FillData()
        {
            const string sqlCommandText = @"SELECT ModuleID, ModuleName 
                                            FROM FAIIAdministration.Modules 
                                            WHERE ShowInFileStorage = TRUE 
                                            ORDER BY ModuleName";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    try
                    {
                        sqlConn.Open();
                        using (var sqlReader = sqlCommand.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                var moduleId = Convert.ToInt32(sqlReader["ModuleID"]);
                                var moduleName = sqlReader["ModuleName"].ToString();
                                var navigationItem = new NavigationItem(moduleId, moduleName);
                                _navigationItems.Add(navigationItem);
                            }
                        }
                    }
                    catch (MySqlException)
                    {
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }
        }

        private void CreateFolders()
        {
            foreach (
                var navigationItem in
                    _navigationItems.Where(navigationItem =>
                                           {
                                               var navPath = string.Concat(_basicDirectory, navigationItem.ItemName, "/");
                                               var exist = _ftpClient.DirectoryExist(navPath);
                                               return !exist;
                                           }))
            {
                _ftpClient.MakeDirectory(string.Concat(_basicDirectory, navigationItem.ItemName, "/"));
            }
        }

        private void SelectNavigationItem(int moduleId)
        {
            if (NavigationListBox.Items.Count <= 0) return;
            if (moduleId != 0 && _navigationItems.Any(nV => nV.ModuleId == moduleId))
            {
                var navItem = _navigationItems.First(nv => nv.ModuleId == moduleId);
                NavigationListBox.SelectedIndex = _navigationItems.IndexOf(navItem);
            }
            else
            {
                NavigationListBox.SelectedIndex = NavigationListBox.Items.Count - 1;
            }

            NavigationListBox.ScrollIntoView(NavigationListBox.SelectedItem);
        }

        private void CreateAttachmentsNavigationItem()
        {
            var navItem = new NavigationItem(-1, "Файлы живой ленты");
            _navigationItems.Add(navItem);
        }

        private void CreateCommonFilesNavigationItem()
        {
            var navItem = new NavigationItem(-1, "Общие файлы");
            _navigationItems.Add(navItem);
        }


        private void FillViews(string directoryPath)
        {
            ShowWaitAnnimation();

            if (_listLoadingThread != null && _listLoadingThread.IsAlive)
            {
                _listLoadingThread.Abort();
                _listLoadingThread.Join();
            }

            _ftpClient.CurrentPath = directoryPath;

            _listLoadingThread = new Thread(() =>
                                            {
                                                //if (!_ftpClient.DirectoryExist(directoryPath)) return;

                                                var dirDetails = _ftpClient.ListDirectoryDetails().ToList();

                                                if (_ftpClient.ParentUri != _basicDirectory)
                                                    dirDetails.Add(new FtpFileDirectoryInfo(null, null, null, null, null,
                                                        "..", null) {IsFolderUpAction = true});

                                                var sortedList =
                                                    dirDetails.OrderByDescending(d => d.IsFolderUpAction)
                                                        .ThenByDescending(d => d.IsDirectory);

                                                Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(
                                                    () =>
                                                    {
                                                        ExplorerTileListBox.ItemsSource = sortedList;
                                                        if (ExplorerTileListBox.Items.Count != 0)
                                                            ExplorerTileListBox.SelectedIndex = 0;

                                                        ExplorerDataGrid.ItemsSource = sortedList;

                                                        HideWaitAnnimation();
                                                    }));
                                            });
            _listLoadingThread.SetApartmentState(ApartmentState.STA);
            _listLoadingThread.IsBackground = true;
            _listLoadingThread.Start();
        }

        private void ShowWaitAnnimation()
        {
            ShadowGrid.Children.Clear();
            ShadowGrid.Visibility = Visibility.Visible;
            var stackPanel = new StackPanel
                             {
                                 Orientation = Orientation.Horizontal,
                                 HorizontalAlignment = HorizontalAlignment.Center,
                                 VerticalAlignment = VerticalAlignment.Center
                             };
            ShadowGrid.Children.Add(stackPanel);
            var circularFadingLine = new CircularFadingLine();
            stackPanel.Children.Add(circularFadingLine);
            var textBlock = new TextBlock();
            stackPanel.Children.Add(textBlock);

            NavigationListBox.IsEnabled = false;
        }

        private void HideWaitAnnimation()
        {
            ShadowGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Children.Clear();

            NavigationListBox.IsEnabled = true;
        }



        #region Downloading

        private void DownloadFiles()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            var dialogWindow = new System.Windows.Forms.FolderBrowserDialog();
            var dialogResult = dialogWindow.ShowDialog();

            if (dialogResult != System.Windows.Forms.DialogResult.OK) return;
            _neededDownloadDirectoryPath = dialogWindow.SelectedPath;
            DownloadQueueFile();
        }

        private void DownloadQueueFile()
        {
            while (true)
            {
                if (!_downloadItems.Any()) return;

                var fileDirectoryInfo = _downloadItems.Dequeue();

                if (fileDirectoryInfo.IsDirectory) return;

                if (_processWindow != null)
                    _processWindow.Close(true);

                _downloadedChangesCount = 250;
                var path = fileDirectoryInfo.Adress;
                var uri = new Uri(path);
                _fileSize = _ftpClient.GetFileSize(fileDirectoryInfo.Adress);

                var newFile = Path.Combine(_neededDownloadDirectoryPath, fileDirectoryInfo.Name);

                if (File.Exists(newFile))
                {
                    if (
                        MessageBox.Show(
                            string.Format("Файл '{0}' уже существует в указанной папке, заменить файл?",
                                fileDirectoryInfo.Name), "Предупреждение", MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                _processWindow.Show(Window.GetWindow(this), true);

                _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
                _ftpClient.DownloadFileAsync(uri, newFile);
                break;
            }
        }

        private void OpenFile(FtpFileDirectoryInfo fileDirectoryInfo)
        {
            if (fileDirectoryInfo.IsDirectory) return;

            var filePath = Path.Combine(App.TempFolder, fileDirectoryInfo.Name);

            if (File.Exists(filePath))
            {
                Process.Start(filePath);
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            _downloadedChangesCount = 250;
            _neededOpeningFilePath = filePath;
            var path = fileDirectoryInfo.Adress;
            var uri = new Uri(path);

            if (_processWindow != null)
                _processWindow.Close(true);

            _processWindow = new WaitWindow { Text = "Загрузка файла..." };
            _processWindow.Show(Window.GetWindow(this), true);

            _fileSize = _ftpClient.GetFileSize(fileDirectoryInfo.Adress);
            _ftpClient.DownloadFileCompleted += OnFtpClientDownloadAndOpenFileCompleted;
            _ftpClient.DownloadFileAsync(uri, filePath);
        }

        private void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Thread.Sleep(100);
            if (_processWindow != null)
                _processWindow.Close(true);

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;

            DownloadQueueFile();
        }

        private void OnFtpClientDownloadAndOpenFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            try
            {
                Process.Start(_neededOpeningFilePath);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            if (_processWindow != null)
                _processWindow.Close(true);

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadAndOpenFileCompleted;
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_downloadedChangesCount < 250)
            {
                _downloadedChangesCount++;
                return;
            }

            if (_processWindow != null)
            {
                _downloadedChangesCount = 0;
                var progress = e.BytesReceived/(double) _fileSize;
                _processWindow.Progress = progress*100;
                var kBytesReceived = e.BytesReceived/1024;
                var receivedBytesString = kBytesReceived > 1024
                    ? string.Format("{0} МБ", kBytesReceived/1024)
                    : string.Format("{0} кБ", kBytesReceived);
                var totalKBytesToReceive = e.TotalBytesToReceive/1024;
                var totalBytesToReceiveString = totalKBytesToReceive > 1024
                    ? string.Format("{0} МБ", totalKBytesToReceive/1024)
                    : string.Format("{0} кБ", totalKBytesToReceive);
                _processWindow.Text = string.Format("Загрузка файла... \n{0} из {1}", receivedBytesString,
                    totalBytesToReceiveString);
            }
        }

        #endregion



        #region Uploading

        private void OnAddNewFileButtonClick(object sender, RoutedEventArgs e)
        {
            if(!_fullAccess) return;

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            var ofd = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };

            if (!ofd.ShowDialog().Value) return;

            _neededUploadDirectoryPath = _ftpClient.CurrentPath;
            _uploadItems.Clear();
            foreach (var fileName in ofd.FileNames)
            {
                _uploadItems.Enqueue(fileName);
            }

            _neededUploadItemsCount = _uploadItems.Count;
            UploadQueueFile();
        }

        private void UploadQueueFile()
        {
            while (true)
            {
                if (!_uploadItems.Any()) return;

                if (_processWindow != null)
                    _processWindow.Close(true);

                _uploadedChangesCount = 250;
                var filePath = _uploadItems.Dequeue();
                var adress = string.Concat(_neededUploadDirectoryPath, Path.GetFileName(filePath));
                var uri = new Uri(adress);

                if (_ftpClient.FileExist(adress))
                {
                    if (
                        MessageBox.Show(
                            string.Format("Файл '{0}' уже существует в указанной папке, заменить файл?",
                                Path.GetFileName(filePath)), "Предупреждение", MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                _processWindow.Show(Window.GetWindow(this), true);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadFileCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }

            Thread.Sleep(200);
            FillViews(_ftpClient.CurrentPath);
        }

        private void OnFtpClientUploadFileCompleted(object sender, UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadFileCompleted;

            Thread.Sleep(200);
            if (_processWindow != null)
                _processWindow.Close(true);

            UploadQueueFile();
        }

        private void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (_uploadedChangesCount < 250)
            {
                _uploadedChangesCount++;
                return;
            }

            if (_processWindow != null)
            {
                _uploadedChangesCount = 0;
                var uploadNumber = _neededUploadItemsCount - _uploadItems.Count;
                _processWindow.Progress = (e.BytesSent / (double)e.TotalBytesToSend) * 100;
                var kBytesSend = e.BytesSent/1024;
                var sendedBytesString = kBytesSend > 1024
                    ? string.Format("{0} МБ", kBytesSend/1024)
                    : string.Format("{0} кБ", kBytesSend);
                var totalKBytesToSend = e.TotalBytesToSend/1024;
                var totalBytesToSendString = totalKBytesToSend > 1024
                    ? string.Format("{0} МБ", totalKBytesToSend / 1024)
                    : string.Format("{0} кБ", totalKBytesToSend);
                _processWindow.Text = string.Format("Загрузка файла({0} из {1}) \n{2}/{3}", uploadNumber,
                    _neededUploadItemsCount, sendedBytesString, totalBytesToSendString);
            }
        }

        #endregion



        private void OnFileInfoMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fileInfo = ((FrameworkElement)sender).DataContext as FtpFileDirectoryInfo;
            if (fileInfo == null) return;

            if (fileInfo.IsFolderUpAction)
            {
                UpButtonClick(null, null);
            }
            else if (fileInfo.IsDirectory)
            {
                _ftpClient.CurrentPath = fileInfo.Adress + "/";
                FillViews(_ftpClient.CurrentPath);
            }
            else
            {
                OpenFile(fileInfo);
            }
        }

        private void OnFileInfoPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                OnFileInfoMouseDoubleClick(sender, null);
            }

            if (e.Key == Key.Delete && _fullAccess)
            {
                OnDeleteButtonClick(sender, null);
            }
        }

        private void OnExplorerPanelPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && NewFolderGrid.Visibility != Visibility.Visible)
            {
                UpButtonClick(null, null);
            }
        }

        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            if (_ftpClient.ParentUri == _basicDirectory) return;

            _ftpClient.CurrentPath = _ftpClient.ParentUri;
            FillViews(_ftpClient.CurrentPath);
        }

        public static ImageSource GetIcon(string fileName)
        {
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
            if (icon != null)
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
            return null;
        }

        private void OnExplorerDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Column.SortDirection = e.Column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            ((DataGrid) sender).Items.SortDescriptions.Clear();
            ((DataGrid) sender).Items.SortDescriptions.Add(new SortDescription("IsFolderUpAction",
                ListSortDirection.Descending));
            ((DataGrid) sender).Items.SortDescriptions.Add(new SortDescription("IsDirectory",
                ListSortDirection.Descending));
            if (e.Column.SortDirection == ListSortDirection.Ascending)
                ((DataGrid) sender).Items.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath,
                    ListSortDirection.Descending));
            else
                ((DataGrid) sender).Items.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath,
                    ListSortDirection.Ascending));

            e.Handled = true;
        }

        private void OnNavigationListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(NavigationListBox.SelectedItem == null)
            {
                ExplorerDataGrid.ItemsSource = null;
                ExplorerTileListBox.ItemsSource = null;
                return;
            }

            var navItem = (NavigationItem) NavigationListBox.SelectedItem;

            if (navItem.ItemName == "Файлы живой ленты")
            {
                // Block file storage edit
                CreateNewFolderButton.Visibility = Visibility.Collapsed;
                AddNewFilesButton.Visibility = Visibility.Collapsed;
                DataContext = false;
            }
            else
            {
                InitializeView();
                DataContext = _fullAccess;
            }

            ShadowGrid.Visibility = Visibility.Visible;
            var directoryPath = string.Concat(_basicDirectory, navItem.ItemName, "/");
            FillViews(directoryPath);
        }


        #region AddingNewFolder

        private void OnAddNewFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_fullAccess) return;

            if (string.IsNullOrEmpty(NewFolderNameTextBox.Text)) return;

            var newFolderName = NewFolderNameTextBox.Text;
            var newFolderPath = string.Concat(_ftpClient.CurrentPath, "/", newFolderName, "/");
            if (!_ftpClient.DirectoryExist(newFolderPath))
            {
                _ftpClient.MakeDirectory(newFolderPath);
                FillViews(_ftpClient.CurrentPath);
            }
            OnCancelAddFolderButtonClick(null, null);
        }

        private void OnCancelAddFolderButtonClick(object sender, RoutedEventArgs e)
        {
            NewFolderGrid.Visibility = Visibility.Collapsed;
            PreviewKeyDown += OnExplorerPanelPreviewKeyDown;
        }

        private void OnCreateNewFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_fullAccess) return;

            NewFolderGrid.Visibility = Visibility.Visible;
            NewFolderNameTextBox.Text = string.Empty;
            NewFolderNameTextBox.Focus();
            PreviewKeyDown -= OnExplorerPanelPreviewKeyDown;
        }

        #endregion


        private class NavigationItem
        {
            public int ModuleId { get; private set; }

            public string ItemName { get; private set; }

            public NavigationItem(int moduleId, string itemName)
            {
                ModuleId = moduleId;
                ItemName = itemName;
            }
        }

        private void OnCloseFileExlorerButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            mainWindow.HideToolsGrid();
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var fileInfo = ((FrameworkElement)sender).DataContext as FtpFileDirectoryInfo;
            if (fileInfo == null) return;
            if (fileInfo.IsFolderUpAction) return;

            if (fileInfo.IsDirectory)
            {
                if (
                    MessageBox.Show("Вы действительно хотите удалить папку и её содержимое?", "Удаление",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                var directoryName = string.Concat(fileInfo.Adress, "/");
                _ftpClient.RemoveDirectory(directoryName);

                FillViews(_ftpClient.CurrentPath);
            }
            else
            {
                if (
                    MessageBox.Show("Вы действительно хотите удалить файл?", "Удаление", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                var fileName = fileInfo.Adress;
                _ftpClient.DeleteFile(fileName);

                FillViews(_ftpClient.CurrentPath);
            }
        }

        private void OnDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            _downloadItems.Clear();

            foreach (
                var fileInfo in
                    ExplorerDataGrid.SelectedItems.Cast<object>()
                        .Select(item => item as FtpFileDirectoryInfo)
                        .Where(fileInfo => fileInfo != null && !fileInfo.IsFolderUpAction && !fileInfo.IsDirectory))
            {
                _downloadItems.Enqueue(fileInfo);
            }

            if (!_downloadItems.Any()) return;
            DownloadFiles();
        }


        #region Mode switch

        private void OnViewSwitcherBoxChecked(object sender, RoutedEventArgs e)
        {
            ExplorerTileGrid.Visibility = Visibility.Visible;
            ExplorerDataGrid.Visibility = Visibility.Hidden;
        }

        private void OnViewSwitcherBoxUnchecked(object sender, RoutedEventArgs e)
        {
            ExplorerTileGrid.Visibility = Visibility.Hidden;
            ExplorerDataGrid.Visibility = Visibility.Visible;
            ExplorerDataGrid.Focus();
        }

        #endregion
    }
}
