using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FA2.ChildPages.NewsFeedPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FAIIControlLibrary.UserControls;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для NewsFeed.xaml
    /// </summary>
    public partial class NewsFeed
    {
        #region Переменные для ввода нового сообщения

        private NewsStatus _addingNewsStatus;

        #endregion

        #region Переменные для новостных страниц

        private readonly RadioButton[] _pageButtons;
        private int _pageCount;
        private int _selectedPageNumber = 1;
        private const int NewsCountByPage = 15;

        #endregion

        #region Переменные для обновления

        private bool _openConnection;
        private readonly BackgroundWorker _timerBackground;
        private System.Windows.Forms.Timer _timer;
        private const int UpdatePeriod = 300000;

        #endregion

        private NewsFeedClass _newsFeedCore;
        private StaffClass _staffClass;

        private List<NewsStatus> _filterNewsStatuses;
        private List<NewsStatusGroup> _filterNewsStatusGroups;

        private List<NewsStatus> _addingNewsStatuses;
        private List<NewsStatusGroup> _addingNewsStatusGroups;

        private FilteringMode _filteringMode;
        private NewsStatusGroup _currentNewsStatusGroup;
        private NewsStatus _currentNewsStatus;
        private int? _currentProdStatusId;
        private readonly BrushConverter _tempBrushConverter = new BrushConverter();
        private readonly Brush _newsBackgroundBrush;
        private readonly Brush _latestNewsBackgroundBrush;
        private const int ProfilManageId = 6;
        private const int TPSManageId = 7;

        private bool _firstTime = true;
        private DateTime _lastUpdate;
        private bool _loadingNewsFeedData;

        private Thread _newsDownloadedThread;

        private List<Worker> _workersList;
        private Thread _photoDownloadedThread;

        private Grid _currentAddingCommentPanel;

        //public List<string> SelectedImagesPathList;
        //public string SelectedImagePath;
        //public string SelectedDocumentPath;
        private readonly String[] _imageExtensions = {".jpg", ".jpeg", ".png", ".gif", ".tiff", ".tif", ".bmp"};
        //private readonly String[] _excelExtensions = {".xls", ".xlm", ".xlsx", ".xlsm"};
        //private readonly String[] _wordExtensions = {".doc", ".docx", ".docm"};

        #region Ftp Variables

        private FtpClient _ftpClient;
        private readonly string _basicDirectory;
        private readonly string _tempDirectory;
        private WaitWindow _processWindow;
        private long _fileSize;
        private string _neededOpeningFilePath;

        private readonly Queue<string> _uploadFiles = new Queue<string>();        
        private readonly Queue<string> _uploadCommentAttachments = new Queue<string>();

        private readonly List<RenamedFile> _renamedFiles = new List<RenamedFile>();       
        private readonly ObservableCollection<RenamedFile> _renamedCommentAttachments = new ObservableCollection<RenamedFile>();

        private readonly Queue<string> _downloadImagesFromNewsQueue = new Queue<string>();
        private string _tempPathForDownloadInQueue;
        private string _ftpSourcePathForDownloadInQueue;
        private string _neededOpeningFilePathInQueue;
        private int _downloadFilesInQueueCount;

        #endregion


        private enum FilteringMode
        {
            ByNewsStatusGroup,
            ByNewsStatus,
            ByNewsStatusAndProdStatus
        }

        private enum AutorizationMode
        {
            WithAutorization,
            WithoutAutorization
        }

        private enum UploadingMode
        {
            FromAddingNews,
            FromAddingComment
        }

        private enum DownloadingMode
        {
            Single,
            InQueue
        }

        private AutorizationMode _autorizationMode;
        private UploadingMode _uploadingMode;
        private DownloadingMode _downloadingMode;

        private struct RenamedFile
        {
            public readonly string OriginalName;
            public readonly string Renamed;

            public RenamedFile(string originalName, string renamed)
            {
                OriginalName = originalName;
                Renamed = renamed;
            }

            public override string ToString()
            {
                return OriginalName;
            }
        }




        public NewsFeed()
        {
            InitializeComponent();

            _basicDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Файлы живой ленты/";
            _tempDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Temp/";

            _newsBackgroundBrush = _tempBrushConverter.ConvertFrom("#FFFDFDF7") as Brush;
            _latestNewsBackgroundBrush = _tempBrushConverter.ConvertFrom("#FFffecb3") as Brush;

            _timerBackground = new BackgroundWorker();
            _timerBackground.DoWork += OnBackgroundWorkerDoWork;
            _timerBackground.RunWorkerCompleted += OnBackgroundWorkerRunWorkerCompleted;

            _pageButtons = new RadioButton[7];
            _pageButtons[0] = toggle0;
            _pageButtons[1] = toggle1;
            _pageButtons[2] = toggle2;
            _pageButtons[3] = toggle3;
            _pageButtons[4] = toggle4;
            _pageButtons[5] = toggle5;
            _pageButtons[6] = toggle6;

            FtpInitializing();

            // Initializing data class
            CreateData();

            BindingFiltersBox();
        }

        private void FtpInitializing()
        {
            _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
            _ftpClient.UploadProgressChanged += OnFtpClientUploadProgressChanged;
            _ftpClient.DownloadProgressChanged += OnFtpClientDownloadProgressChanged;

            //можно выкл
            //----------------------------------------------------
            //if (!_ftpClient.DirectoryExist(_basicDirectory))
            //    _ftpClient.MakeDirectory(_basicDirectory);
            //if (!_ftpClient.DirectoryExist(_tempDirectory))
            //    _ftpClient.MakeDirectory(_tempDirectory);
            //----------------------------------------------------
        }

        private void CreateData()
        {
            App.BaseClass.GetNewsFeedClass(ref _newsFeedCore);
            App.BaseClass.GetStaffClass(ref _staffClass);
        }

        private void BindingFiltersBox()
        {
            _filteringMode = FilteringMode.ByNewsStatusGroup;

            _filterNewsStatusGroups = new List<NewsStatusGroup>();
            _newsFeedCore.NewsStatusGroups.ForEach(nsg => _filterNewsStatusGroups.Add((NewsStatusGroup)nsg.Clone()));
            _filterNewsStatuses = new List<NewsStatus>();
            _filterNewsStatusGroups.ForEach(nsg => _filterNewsStatuses.AddRange(nsg.NewsStatuses));

            RenameField(_filterNewsStatuses, "Новость", "Новости");
            RenameField(_filterNewsStatuses, "Объявление", "Объявления");
            RenameField(_filterNewsStatuses, "Предложение", "Предложения");
            RenameField(_filterNewsStatuses, "Распоряжение", "Распоряжения");
            RenameField(_filterNewsStatuses, "Приказ", "Приказы");
            RenameField(_filterNewsStatuses, "Заявка", "Заявки");
            RenameField(_filterNewsStatuses, "Задача", "Задачи");

            NewsFilterTreeView.ItemsSource = _filterNewsStatusGroups;

            _addingNewsStatusGroups = new List<NewsStatusGroup>();
            _newsFeedCore.NewsStatusGroups.ForEach(nsg => _addingNewsStatusGroups.Add((NewsStatusGroup)nsg.Clone()));

            // Remove personal news status group
            _addingNewsStatusGroups.RemoveAt(2);
            _addingNewsStatuses = new List<NewsStatus>();
            _addingNewsStatusGroups.ForEach(nsg => _addingNewsStatuses.AddRange(nsg.NewsStatuses));
            if (_addingNewsStatuses.Any())
                _addingNewsStatus = _addingNewsStatuses.First();

            AddingNewsStatusTreeView.ItemsSource = _addingNewsStatusGroups;
        }

        private void BindingProdStatuses()
        {
            var prodStatuses = _staffClass.GetProductionStatuses().ToTable();
            //var workerGroups = NewsFeedClass.GetWorkerGroupsIds(AdministrationClass.CurrentWorkerId);
            //if (workerGroups.All(g => g != 1))
            //{
            if (!AdministrationClass.IsAdministrator)
            {
                var workerProdStatuses = NewsFeedClass.GetWorkerProdStatuses(AdministrationClass.CurrentWorkerId);
                prodStatuses = workerProdStatuses.Any()
                    ? prodStatuses.AsEnumerable().Where(r => workerProdStatuses.Any(ws => ws == r.Field<Int64>("ProdStatusID"))).CopyToDataTable()
                    : prodStatuses.Clone();
            }
            //}

            RenameProdStatusName(prodStatuses.AsEnumerable(), "Технологический", "Технологическая");
            RenameProdStatusName(prodStatuses.AsEnumerable(), "Оперативный", "Оперативная");
            RenameProdStatusName(prodStatuses.AsEnumerable(), "Экономический", "Экономическая");
            RenameProdStatusName(prodStatuses.AsEnumerable(), "Технический", "Техническая");

            ProdStatusesComboBox.ItemsSource = prodStatuses.AsDataView();
            if (ProdStatusesComboBox.HasItems)
                ProdStatusesComboBox.SelectedIndex = 0;
            
            ProdStatusFilterComboBox.ItemsSource = prodStatuses.AsDataView();
            if (ProdStatusFilterComboBox.HasItems)
                ProdStatusFilterComboBox.SelectedIndex = 0;
        }

        private static void RenameField(IEnumerable<NewsStatus> newsStatuses, string originalName, string newName)
        {
            var statuses = newsStatuses.Where(ns => ns.NewsStatusName == originalName);
            var list = statuses as IList<NewsStatus> ?? statuses.ToList();
            if (!list.Any()) return;

            list.First().NewsStatusName = newName;
        }

        private static void RenameProdStatusName(IEnumerable<DataRow> rows, string originalName, string newName)
        {
            var prodStatuses = rows.Where(r => r.Field<string>("ProdStatusName") == originalName).ToList();
            if (!prodStatuses.Any()) return;

            prodStatuses.First()["ProdStatusName"] = newName;
        }




        private void OnNewsFeedLoaded(object sender, RoutedEventArgs e)
        {
            if (_autorizationMode == AutorizationMode.WithAutorization)
            {
                AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.NewsFeed);
            }

            if (!_firstTime)
                CreateView(_filteringMode);

            _firstTime = false;
        }

        public void ShowNews()
        {
            // Not authorized mode
            _autorizationMode = AutorizationMode.WithoutAutorization;

            Initializing();
            BindingProdStatuses();
            _selectedPageNumber = 1;
            CreateView(_filteringMode);
        }

        public void ShowNews(DateTime lastExit)
        {
            // Authorized mode
            _lastUpdate = lastExit;
            _autorizationMode = AutorizationMode.WithAutorization;

            Initializing();

            BindingProdStatuses();
            SetLastUpdatesForFilters();
            GetLatestEventsCount();
            
            _selectedPageNumber = 1;
            CreateView(_filteringMode);
        }

        private void Initializing()
        {
            OnCancelNewsButtonClick(null, null);

            NewsHeaderGrid.Visibility = _autorizationMode == AutorizationMode.WithAutorization
                ? Visibility.Visible
                : Visibility.Collapsed;
            NewsListBox.Margin = _autorizationMode == AutorizationMode.WithAutorization
                ? new Thickness(0, 45, 0, 0)
                : new Thickness(0, 5, 0, 0);

            // Clear memory
            NewsListBox.ItemsSource = null;
            GC.Collect(0);
            GC.GetTotalMemory(true);

            if (_filterNewsStatusGroups.Any())
            {
                var firstNewsStatusGroup = _filterNewsStatusGroups.First();
                firstNewsStatusGroup.IsSelected = true;
                _currentNewsStatusGroup = firstNewsStatusGroup;

                FilterNewsToggleButton.Content = firstNewsStatusGroup.NewsStatusGroupName;
                FilterNewsToggleButton.Foreground = new BrushConverter().ConvertFrom("#DD000000") as Brush;
            }

            SetTimerProperties();
        }

        private void SetLastUpdatesForFilters()
        {
            var updateTimeLimit = _lastUpdate != DateTime.MinValue
                ? _lastUpdate.Subtract(TimeSpan.FromMilliseconds(UpdatePeriod))
                : DateTime.MinValue;

            foreach (var newsStatusGroup in _filterNewsStatusGroups)
            {
                newsStatusGroup.LastUpdate = updateTimeLimit;
                foreach (var newsStatus in newsStatusGroup.NewsStatuses)
                {
                    newsStatus.LastUpdate = updateTimeLimit;
                }
            }
        }

        private void GetLatestEventsCount()
        {
            if (_autorizationMode == AutorizationMode.WithoutAutorization) return;

            foreach (var newsStatus in _filterNewsStatuses)
            {
                var newStatusEventsCount =
                    NewsFeedClass.GetNewMessagesCount(newsStatus, AdministrationClass.CurrentWorkerId,
                        newsStatus.LastUpdate);
                newsStatus.NewEventsCount = newStatusEventsCount;
            }

            CountUpLatestEvents();
        }

        private void CountUpLatestEvents()
        {
            var totalNewEventsCount = 0;
            foreach (var newsStatusGroup in _filterNewsStatusGroups)
            {
                var newGroupEventsCount = newsStatusGroup.NewsStatuses.Sum(newsStatus => newsStatus.NewEventsCount);
                newsStatusGroup.NewEventsCount = newGroupEventsCount;
                totalNewEventsCount += newGroupEventsCount;
            }

            if (totalNewEventsCount != 0)
            {
                if (_currentNewsStatus != null)
                    _currentNewsStatus.IsSelected = false;
                if (_currentNewsStatusGroup != null)
                    _currentNewsStatusGroup.IsSelected = false;
                FilterNewsToggleButton.Content = string.Format("Новых сообщений: {0}", totalNewEventsCount);
                FilterNewsToggleButton.Foreground = Brushes.Tomato;
            }
        }





        private void CreateView(FilteringMode filteringMode)
        {
            ShowWaitAnimation();

            // Creating page buttons
            CreatePagePanel(filteringMode);

            // Fill news
            FillNews(filteringMode);
        }

        private void CreatePagePanel(FilteringMode filteringMode)
        {
            var newsCount = filteringMode == FilteringMode.ByNewsStatusGroup
                ? _newsFeedCore.GetNewsCount(_currentNewsStatusGroup)
                : _newsFeedCore.GetNewsCount(_currentNewsStatus, _currentProdStatusId);

            _pageCount = Convert.ToInt32(Math.Ceiling(newsCount/(decimal) NewsCountByPage));
            if (_pageCount <= _selectedPageNumber) _selectedPageNumber = _pageCount;
            if (_selectedPageNumber == 0) _selectedPageNumber = 1;

            FormPagePanel();
        }

        private void FormPagePanel()
        {
            //foreach (var toggle in _pageButtons)
            //{
            //    toggle.IsEnabled = true;
            //}
            if (_pageCount > 7)
            {
                if (_selectedPageNumber <= 4)
                {
                    PageNumbersGrid.ColumnDefinitions[1].Width = new GridLength(0);
                    PageNumbersGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[6].Width = new GridLength(0);
                    PageNumbersGrid.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Auto);

                    _pageButtons[_selectedPageNumber - 1].Checked -= OnPageButtonChecked;
                    _pageButtons[_selectedPageNumber - 1].IsChecked = true;
                    _pageButtons[_selectedPageNumber - 1].Checked += OnPageButtonChecked;

                    var index = 1;
                    foreach (var tbutton in _pageButtons)
                    {
                        tbutton.Content = index;
                        index++;
                    }
                    toggle6.Content = _pageCount;
                }
                else if (_selectedPageNumber > 4 && _selectedPageNumber < _pageCount - 3)
                {
                    PageNumbersGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[6].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Auto);

                    toggle3.Checked -= OnPageButtonChecked;
                    toggle3.IsChecked = true;
                    toggle3.Checked += OnPageButtonChecked;
                    toggle3.Content = _selectedPageNumber;

                    toggle0.Content = 1;
                    toggle1.Content = _selectedPageNumber - 2;
                    toggle2.Content = _selectedPageNumber - 1;
                    toggle4.Content = _selectedPageNumber + 1;
                    toggle5.Content = _selectedPageNumber + 2;
                    toggle6.Content = _pageCount;
                }
                else if (_selectedPageNumber >= _pageCount - 3)
                {
                    PageNumbersGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[2].Width = new GridLength(0);
                    PageNumbersGrid.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[6].Width = new GridLength(1, GridUnitType.Auto);
                    PageNumbersGrid.ColumnDefinitions[7].Width = new GridLength(0);

                    _pageButtons[6 - (_pageCount - _selectedPageNumber)].Checked -= OnPageButtonChecked;
                    _pageButtons[6 - (_pageCount - _selectedPageNumber)].IsChecked = true;
                    _pageButtons[6 - (_pageCount - _selectedPageNumber)].Checked += OnPageButtonChecked;

                    var index = _pageCount - 6;
                    foreach (var tbutton in _pageButtons)
                    {
                        tbutton.Content = index;
                        index++;
                    }
                    toggle0.Content = 1;
                }
            }
            else
            {
                foreach (var tbutton in _pageButtons)
                {
                    tbutton.Content = null;
                }
                for (int i = 0; i < _pageCount; i++)
                {
                    _pageButtons[i].Content = i + 1;
                }
                //for (int i = 0; i < 7; i++)
                //{
                //    PageNumbersGrid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
                //}
                foreach (var column in PageNumbersGrid.ColumnDefinitions)
                {
                    column.Width = new GridLength(1, GridUnitType.Auto);
                }
                PageNumbersGrid.ColumnDefinitions[1].Width = new GridLength(0);
                PageNumbersGrid.ColumnDefinitions[7].Width = new GridLength(0);

                if (_selectedPageNumber != 0)
                {
                    _pageButtons[_selectedPageNumber - 1].Checked -= OnPageButtonChecked;
                    _pageButtons[_selectedPageNumber - 1].IsChecked = true;
                    _pageButtons[_selectedPageNumber - 1].Checked += OnPageButtonChecked;
                }
            }
        }

        private void FillNews(FilteringMode filteringMode)
        {
            var indexFrom = (_selectedPageNumber - 1)*NewsCountByPage;

            if (_newsDownloadedThread != null)
            {
                _newsDownloadedThread.Abort();
                _newsDownloadedThread.Join();
            }
            _newsDownloadedThread = new Thread(() =>
            {
                _loadingNewsFeedData = true;

                // Fill news according filter and fill comments and attachments
                if (filteringMode == FilteringMode.ByNewsStatusGroup)
                {
                    _newsFeedCore.Fill(indexFrom, NewsCountByPage, _currentNewsStatusGroup);
                }
                else
                {
                    _newsFeedCore.Fill(indexFrom, NewsCountByPage, _currentNewsStatus, _currentProdStatusId);
                }

                // Make news collection as items source
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    _loadingNewsFeedData = false;

                    // Fill worker photos
                    FillWorkers();
                    FillBindings();
                }));
            });
            _newsDownloadedThread.SetApartmentState(ApartmentState.STA);
            _newsDownloadedThread.IsBackground = true;
            _newsDownloadedThread.Start();
        }

        // Workers filling
        private void FillWorkers()
        {
            if (_staffClass == null) return;

            // WorkerIds from selected news
            var workerIdsFromNews =
                (from newsView in _newsFeedCore.News.AsEnumerable()
                    select newsView.Field<Int64>("WorkerID")).ToList();

            // WorkerIds from selected comments
            var workerIdsFromComments =
                (from commentView in
                    _newsFeedCore.Comments.AsEnumerable()
                    select commentView.Field<Int64>("WorkerID")).ToList();
            workerIdsFromNews.AddRange(workerIdsFromComments);
            // Add current worker to list
            workerIdsFromNews.Add(AdministrationClass.CurrentWorkerId);

            FillWorkersList(workerIdsFromNews.Distinct().ToList());
            DownloadPhotos();
        }

        private void FillWorkersList(IEnumerable<long> workersIds)
        {
            _workersList = new List<Worker> {new Worker(0, "Гость", null)};

            foreach (var worker in workersIds.Select(workerId => new Worker(workerId, null, null)))
            {
                _workersList.Add(worker);
            }
        }

        // Download photos in seppatate thread
        private void DownloadPhotos()
        {
            if (_photoDownloadedThread != null)
            {
                _photoDownloadedThread.Abort();
                _photoDownloadedThread.Join();
            }
            _photoDownloadedThread = new Thread(() =>
                                                {
                                                    foreach (var worker in _workersList)
                                                    {
                                                        var photo =
                                                            _staffClass.GetObjectPhotoFromDataBase((int) worker.WorkerID);
                                                        var image = AdministrationClass.ObjectToBitmapImage(photo);
                                                        worker.Photo = image;
                                                    }
                                                });
            _photoDownloadedThread.SetApartmentState(ApartmentState.STA);
            _photoDownloadedThread.IsBackground = true;
            _photoDownloadedThread.Start();
        }

        private void FillBindings()
        {
            // Clear memory
            NewsListBox.ItemsSource = null;
            GC.Collect(0);
            GC.GetTotalMemory(true);

            // Set source for news view
            var newsView = _newsFeedCore.News.AsDataView();
            newsView.Sort = "LastEditing DESC, NewsDate DESC";
            NewsListBox.ItemsSource = newsView;

            var scrollViewer = NewsListBox.Template.FindName("NewsScrollViewer", NewsListBox) as ScrollViewer;
            if (scrollViewer != null)
                scrollViewer.ScrollToTop();

            HideWaitAnimation();
        }

        private void ShowWaitAnimation()
        {
            ShadowBorder.Child = null;
            ShadowBorder.Visibility = Visibility.Visible;
            var stackPanel = new StackPanel
                             {
                                 Orientation = Orientation.Horizontal,
                                 HorizontalAlignment = HorizontalAlignment.Center,
                                 VerticalAlignment = VerticalAlignment.Center
                             };
            ShadowBorder.Child = stackPanel;
            var circularFadingLine = new CircularFadingLine();
            stackPanel.Children.Add(circularFadingLine);
            var textBlock = new TextBlock();
            stackPanel.Children.Add(textBlock);

            PageButtonsGrid.IsEnabled = false;
            FilterButtonsGrid.IsEnabled = false;
        }

        private void HideWaitAnimation()
        {
            ShadowBorder.Visibility = Visibility.Collapsed;
            ShadowBorder.Child = null;

            PageButtonsGrid.IsEnabled = true;
            FilterButtonsGrid.IsEnabled = true;
        }





        private void OnPageButtonChecked(object sender, RoutedEventArgs e)
        {
            _selectedPageNumber = Convert.ToInt32(((RadioButton) sender).Content);

            CreateView(_filteringMode);
        }


        #region AddingNews

        private void OnNewsTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NewsTextBox.PreviewMouseLeftButtonDown -= OnNewsTextBoxPreviewMouseLeftButtonDown;

            if (_addingNewsStatuses.Any())
            {
                var firstNewsStatus = _addingNewsStatuses.First();
                firstNewsStatus.IsSelected = true;
            }

            const int speed = 300;
            var fastDuration = new Duration(TimeSpan.FromMilliseconds(speed/3));
            var mediumDuration = new Duration(TimeSpan.FromMilliseconds(speed));
            var opacityAnimation = new DoubleAnimation(0, fastDuration);
            var heightAnimation = new DoubleAnimation(350, mediumDuration) {DecelerationRatio = 0.8};
            opacityAnimation.Completed += (o, args) =>
            {
                NewsTextBox.Text = string.Empty;
                NewsTextBox.WatermarkText = "Введите текст";
                NewsHeaderTextBlock.Visibility = Visibility.Visible;
                AddingNewsStatusPanel.Visibility = Visibility.Visible;
                AddNewsAttachmentsPanel.Visibility = Visibility.Visible;
                AddNewsButtonsPanel.Visibility = Visibility.Visible;
                opacityAnimation = new DoubleAnimation(1, mediumDuration);
                AddNewsGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            };
            AddNewsGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            AddNewsBorder.BeginAnimation(HeightProperty, heightAnimation);
        }

        private void OnCancelNewsButtonClick(object sender, RoutedEventArgs e)
        {
            NewsTextBox.PreviewMouseLeftButtonDown += OnNewsTextBoxPreviewMouseLeftButtonDown;

            const int speed = 300;
            var fastDuration = new Duration(TimeSpan.FromMilliseconds(speed/3));
            var mediumDuration = new Duration(TimeSpan.FromMilliseconds(speed));
            var opacityAnimation = new DoubleAnimation(0, fastDuration);
            var heightAnimation = new DoubleAnimation(45, mediumDuration) {DecelerationRatio = 0.8};
            opacityAnimation.Completed += (o, args) =>
                                          {
                                              NewsTextBox.Text = string.Empty;
                                              NewsTextBox.WatermarkText = "Оставить запись...";
                                              NewsHeaderTextBlock.Visibility = Visibility.Collapsed;
                                              AddingNewsStatusPanel.Visibility = Visibility.Collapsed;
                                              AddNewsAttachmentsPanel.Visibility = Visibility.Collapsed;
                                              AddNewsButtonsPanel.Visibility = Visibility.Collapsed;
                                              NewAttachmentsListBox.Items.Clear();

                                              // Delete attached files in ftp Temp directory
                                              foreach (var renamedFile in _renamedFiles)
                                              {
                                                  _ftpClient.DeleteFile(renamedFile.Renamed);
                                              }
                                              _renamedFiles.Clear();

                                              opacityAnimation = new DoubleAnimation(1, fastDuration);
                                              AddNewsGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };
            AddNewsGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            AddNewsBorder.BeginAnimation(HeightProperty, heightAnimation);
        }

        private void OnAddingNewsStatusRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var addingNewsStatusRadioButton = sender as RadioButton;
            if (addingNewsStatusRadioButton == null) return;

            var newsStatus = addingNewsStatusRadioButton.DataContext as NewsStatus;
            if (newsStatus == null) return;

            if(newsStatus.NewsStatusId == ProfilManageId || newsStatus.NewsStatusId == TPSManageId)
            {
                AddingNewsProdStatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AddingNewsProdStatusPanel.Visibility = Visibility.Collapsed;
                HasProdStatusCheckBox.IsChecked = false;
            }

            _addingNewsStatus = newsStatus;
        }

        private void OnSendNewsButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewsTextBox.Text.Trim())) return;

            var currentTime = App.BaseClass.GetDateFromSqlServer();
            var newsText = NewsTextBox.Text;
            int? prodStatusId;
            if (HasProdStatusCheckBox.IsChecked.HasValue && HasProdStatusCheckBox.IsChecked.Value && ProdStatusesComboBox.SelectedItem != null)
                prodStatusId = Convert.ToInt32(ProdStatusesComboBox.SelectedValue);
            else
                prodStatusId = null;

            var newsId = _newsFeedCore.AddNews(newsText, currentTime, _addingNewsStatus.NewsStatusId,
                AdministrationClass.CurrentWorkerId, null, prodStatusId);
            AdministrationClass.AddNewAction(93);

            if (NewAttachmentsListBox.Items.Count != 0)
            {
                AddAttachments(newsId);
            }

            _selectedPageNumber = 1;
            var newsStatusGroup =
                _filterNewsStatusGroups.First(
                    nsg => nsg.NewsStatuses.Any(ns => ns.NewsStatusId == _addingNewsStatus.NewsStatusId));
            newsStatusGroup.IsSelected = true;

            OnCancelNewsButtonClick(null, null);
        }

        private void OnAddAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
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

            _uploadFiles.Clear();
            foreach (var fileName in ofd.FileNames)
            {
                _uploadFiles.Enqueue(fileName);
            }

            UploadQueueFile();
        }

        private void AddAttachments(int newsId)
        {
            if (newsId < 1) return;

            var newsPath = GetNewsPath(newsId);
            if(newsPath == null) return;

            // Parrent directory doesnt exist
            if (!_ftpClient.DirectoryExist(newsPath))
            {
                // Create news directory
                _ftpClient.MakeDirectory(newsPath);
            }

            foreach (var renamedFile in _renamedFiles)
            {
                var adress = GetDistinctFileName(renamedFile.OriginalName, newsPath);

                var attachmentName = Path.GetFileName(adress);
                _newsFeedCore.AddAttachment(newsId, attachmentName);

                var sourceUri = new Uri(renamedFile.Renamed, UriKind.Absolute);
                var targetUri = new Uri(adress, UriKind.Absolute);
                var targetUriRelative = sourceUri.MakeRelativeUri(targetUri);

                _ftpClient.Rename(renamedFile.Renamed, Uri.UnescapeDataString(targetUriRelative.OriginalString));
            }

            _renamedFiles.Clear();
        }

        private void OnShowNewAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var showNewAttachmentButton = sender as Button;
            if (showNewAttachmentButton == null) return;

            if (showNewAttachmentButton.DataContext == null) return;
            var renamedFile = (RenamedFile) showNewAttachmentButton.DataContext;

            try
            {
                Process.Start(renamedFile.OriginalName);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void OnDeleteNewAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteNewAttachmentButton = sender as Button;
            if (deleteNewAttachmentButton == null) return;

            if (deleteNewAttachmentButton.DataContext == null) return;
            var renamedFile = (RenamedFile) deleteNewAttachmentButton.DataContext;

            _ftpClient.DeleteFile(renamedFile.Renamed);
            _renamedFiles.Remove(renamedFile);

            NewAttachmentsListBox.Items.Remove(renamedFile);
        }

        #endregion


        #region Filters

        private void OnFilterByNewsStatusGroupRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var filterByNewsStatusGroupRadioButton = sender as RadioButton;
            if (filterByNewsStatusGroupRadioButton == null) return;

            var newsStatusGroup = filterByNewsStatusGroupRadioButton.DataContext as NewsStatusGroup;
            if (newsStatusGroup == null) return;

            ProdStatusFilterPanel.Visibility = Visibility.Collapsed;

            FilterNewsToggleButton.IsChecked = false;
            FilterNewsToggleButton.Content = newsStatusGroup.NewsStatusGroupName;
            FilterNewsToggleButton.Foreground = new BrushConverter().ConvertFrom("#DD000000") as Brush;

            newsStatusGroup.NewEventsCount = 0;
            var currentTime = App.BaseClass.GetDateFromSqlServer();
            _lastUpdate = newsStatusGroup.LastUpdate;
            newsStatusGroup.LastUpdate = currentTime;
            foreach (var newsStatus in newsStatusGroup.NewsStatuses)
            {
                newsStatus.NewEventsCount = 0;
                newsStatus.LastUpdate = currentTime;
            }

            CountUpLatestEvents();

            _filteringMode = FilteringMode.ByNewsStatusGroup;
            _currentNewsStatusGroup = newsStatusGroup;
            _selectedPageNumber = 1;
            CreateView(_filteringMode);
        }

        private void OnFilterByNewsStatusRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var filterByNewsStatusRadioButton = sender as RadioButton;
            if (filterByNewsStatusRadioButton == null) return;

            var newsStatus = filterByNewsStatusRadioButton.DataContext as NewsStatus;
            if (newsStatus == null) return;

            if (newsStatus.NewsStatusId == ProfilManageId || newsStatus.NewsStatusId == TPSManageId)
            {
                ProdStatusFilterPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ProdStatusFilterPanel.Visibility = Visibility.Collapsed;
                EnableProdStatusFilterCheckBox.Unchecked -= OnEnableProdStatusFilterCheckBoxUnchecked;
                EnableProdStatusFilterCheckBox.IsChecked = false;
                EnableProdStatusFilterCheckBox.Unchecked += OnEnableProdStatusFilterCheckBoxUnchecked;
            }

            FilterNewsToggleButton.IsChecked = false;
            FilterNewsToggleButton.Content = newsStatus.NewsStatusName;
            FilterNewsToggleButton.Foreground = new BrushConverter().ConvertFrom(newsStatus.NewsStatusColor) as Brush;

            var currentTime = App.BaseClass.GetDateFromSqlServer();
            _lastUpdate = newsStatus.LastUpdate;
            newsStatus.NewEventsCount = 0;
            newsStatus.LastUpdate = currentTime;

            CountUpLatestEvents();

            if(EnableProdStatusFilterCheckBox.IsChecked.HasValue && EnableProdStatusFilterCheckBox.IsChecked.Value &&
                ProdStatusFilterComboBox.SelectedItem != null)
            {
                _filteringMode = FilteringMode.ByNewsStatusAndProdStatus;
                _currentProdStatusId = Convert.ToInt32(ProdStatusFilterComboBox.SelectedValue);
            }
            else
            {
                _filteringMode = FilteringMode.ByNewsStatus;
                _currentProdStatusId = null;
            }
            _currentNewsStatus = newsStatus;
            _selectedPageNumber = 1;
            CreateView(_filteringMode);
        }

        #endregion


        #region Update

        private void SetTimerProperties()
        {
            if (_timer == null) _timer = new System.Windows.Forms.Timer();
            _timer.Interval = _autorizationMode == AutorizationMode.WithAutorization
                ? (int) (UpdatePeriod*0.5)
                : UpdatePeriod;
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            if (!_timer.Enabled)
                _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _openConnection = false;
            if (!_timerBackground.IsBusy)
                _timerBackground.RunWorkerAsync();
        }

        private void OnBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _openConnection = TryConnection();
        }

        private void OnBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
                                                   {
                                                       if (_openConnection)
                                                       {
                                                           SqlConnectionFaledTextBlock.Foreground = Brushes.Transparent;

                                                           if (_autorizationMode == AutorizationMode.WithAutorization)
                                                           {
                                                               GetLatestEventsCount();
                                                               _timer.Interval = (int) (UpdatePeriod*0.5);
                                                           }
                                                           else
                                                           {
                                                               CreateView(_filteringMode);
                                                               _timer.Interval = UpdatePeriod;
                                                           }
                                                       }
                                                       else
                                                       {
                                                           _timer.Interval = (int) (UpdatePeriod*0.25);
                                                           SqlConnectionFaledTextBlock.Foreground =
                                                               (Brush) _tempBrushConverter.ConvertFrom("#B9E82121");
                                                       }
                                                   }));
        }    

        private static bool TryConnection()
        {
            var result = false;

            try
            {
                using (var con = new MySqlConnection(App.ConnectionInfo.ConnectionString))
                {
                    MySqlConnection.ClearPool(con);
                    con.Open();
                    con.Close();
                    result = true;
                }
            }
            catch (MySqlException)
            {
            }

            return result;
        }

        #endregion


        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollControl = sender as ScrollViewer;
            if (scrollControl == null) return;

            if (e.Handled) return;

            if ((e.Delta > 0 && Math.Abs(scrollControl.VerticalOffset) < 0.001)

                ||
                (e.Delta <= 0 &&
                 scrollControl.VerticalOffset >= scrollControl.ExtentHeight - scrollControl.ViewportHeight))
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                               {
                                   RoutedEvent =
                                       MouseWheelEvent,
                                   Source = sender
                               };
                var scrollViewer = NewsListBox.Template.FindName("NewsScrollViewer", NewsListBox) as ScrollViewer;
                if (scrollViewer != null)
                    scrollViewer.RaiseEvent(eventArg);
            }
        }



        #region NewsEvents

        private void OnNewsItemLoaded(object sender, RoutedEventArgs e)
        {
            if (_loadingNewsFeedData) return;

            var newsItem = sender as Grid;
            if (newsItem == null) return;

            // Get datacontext DataRowView
            var news = newsItem.DataContext as DataRowView;
            if (news == null) return;

            var newsId = Convert.ToInt32(news["NewsID"]);
            var workerId = Convert.ToInt32(news["WorkerID"]);

            newsItem.Background = _newsBackgroundBrush;
            if (_autorizationMode == AutorizationMode.WithAutorization &&
                AdministrationClass.CurrentWorkerId != workerId)
            {
                var newsDate = Convert.ToDateTime(news.Row["NewsDate"]);
                var updateTimeLimit = _lastUpdate != DateTime.MinValue
                    ? _lastUpdate.Subtract(TimeSpan.FromMilliseconds(UpdatePeriod))
                    : DateTime.MinValue;

                //Mark latest news
                if (newsDate > updateTimeLimit)
                    newsItem.Background = _latestNewsBackgroundBrush;
            }

            var imageBorder = newsItem.FindName("ImageBorder") as Border;
            if (imageBorder != null)
            {
                if (_workersList.Any(w => w.WorkerID == workerId))
                {
                    var worker = _workersList.First(w => w.WorkerID == workerId);
                    imageBorder.DataContext = worker;
                }
            }

            // Make edit panel enable, if current worker and news worker are simple
            var editNewsPanel = newsItem.FindName("EditNewsPanel") as StackPanel;
            if (editNewsPanel != null)
            {
                if ((workerId == AdministrationClass.CurrentWorkerId || AdministrationClass.IsAdministrator) &&
                    _autorizationMode == AutorizationMode.WithAutorization)
                    editNewsPanel.Visibility = Visibility.Visible;
            }

            // Set items source for attachments list
            var attachmentList = newsItem.FindName("AttachmentsList") as ItemsControl;
            if (attachmentList != null)
            {
                if (_newsFeedCore.Attachments.Columns.Count != 0)
                {
                    var attachmentsView = _newsFeedCore.Attachments.AsDataView();
                    attachmentsView.RowFilter = string.Format("NewsID = {0}", newsId);
                    attachmentList.ItemsSource = attachmentsView;

                    if (attachmentsView.Count != 0)
                    {
                        var imagesCount =
                            attachmentsView.Cast<DataRowView>()
                                .Select(attachment => attachment["AttachmentName"].ToString())
                                .Select(Path.GetExtension)
                                .Where(extension => !string.IsNullOrEmpty(extension))
                                .Count(extension => _imageExtensions.Contains(extension.ToLower()));

                        if (imagesCount > 1)
                        {
                            var showAllImagesButton = newsItem.FindName("ShowAllImagesInNewsButton") as Button;
                            if (showAllImagesButton != null)
                            {
                                showAllImagesButton.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }

            // Set enable for comment buttons
            if (_autorizationMode == AutorizationMode.WithAutorization)
            {
                if (_newsFeedCore.Comments.Columns.Count != 0)
                {
                    var comments = _newsFeedCore.Comments.AsEnumerable().ToList();
                    if (comments.All(c => Convert.ToInt32(c["NewsID"]) != newsId))
                    {
                        var firstCommentButton = newsItem.FindName("FirstCommentButton") as Button;
                        if (firstCommentButton != null)
                            firstCommentButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        var secondCommentButton = newsItem.FindName("SecondCommentButton") as Button;
                        if (secondCommentButton != null)
                            secondCommentButton.Visibility = Visibility.Visible;
                    }
                }
            }

            // Set items source for comments list
            var commentsList = newsItem.FindName("CommentsList") as ItemsControl;
            if (commentsList != null)
            {
                if (_newsFeedCore.Comments.Columns.Count != 0)
                {
                    var view = _newsFeedCore.Comments.AsDataView();
                    view.RowFilter = string.Format("NewsID = {0}", newsId);
                    commentsList.ItemsSource = view;
                }
            }
        }

        private void OnDeleteNewsButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteNewsButton = sender as Button;
            if (deleteNewsButton == null) return;

            // Get datacontext DataRowView
            var news = deleteNewsButton.DataContext as DataRowView;
            if (news == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить сообщение?", "Удаление", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            int newsId;
            Int32.TryParse(news["NewsID"].ToString(), out newsId);

            var comments = _newsFeedCore.Comments.Select(string.Format("NewsID = {0}", newsId));
            foreach (var comment in comments)
            {
                int commentId;
                Int32.TryParse(comment["CommentID"].ToString(), out commentId);

                var commentPath = GetCommentAttachmentPath(newsId, commentId);
                if (commentPath != null)
                {
                    if (_ftpClient.DirectoryExist(commentPath))
                    {
                        _ftpClient.CurrentPath = commentPath;
                        var filesList = _ftpClient.ListDirectory();
                        foreach (var file in filesList)
                        {
                            // Delete all comment attachments on ftp-server
                            _ftpClient.DeleteFile(string.Concat(commentPath, file));
                        }
                    }
                }

                // Delete all attachments rows, that comment includes
                _newsFeedCore.DeleteCommentAttachments(commentId);

                _newsFeedCore.DeleteComment(commentId);
            }

            var newsPath = GetNewsPath(newsId);
            if (newsPath != null)
            {
                if (_ftpClient.DirectoryExist(newsPath))
                {
                    _ftpClient.CurrentPath = newsPath;
                    var filesList = _ftpClient.ListDirectory();
                    foreach (var file in filesList)
                    {
                        // Delete all attachments on ftp-server
                        _ftpClient.DeleteFile(string.Concat(_ftpClient.CurrentPath, file));
                    }
                }

            }

            // Delete all attachments rows, that news includes
            _newsFeedCore.DeleteAttachments(newsId);

            _newsFeedCore.DeleteNews(newsId);
            AdministrationClass.AddNewAction(94);
        }

        #endregion



        #region CommentsEvents

        private void OnCommentItemLoaded(object sender, RoutedEventArgs e)
        {
            if (_loadingNewsFeedData) return;

            var commentItem = sender as Grid;
            if (commentItem == null) return;

            var comment = commentItem.DataContext as DataRowView;
            if (comment == null) return;

            if(_newsFeedCore.Comments.Columns.Count == 0) return;

            var commentId = Convert.ToUInt32(comment["CommentID"]);
            var workerId = Convert.ToInt32(comment["WorkerID"]);

            if (_autorizationMode == AutorizationMode.WithAutorization &&
                AdministrationClass.CurrentWorkerId != workerId)
            {
                var commentDate = Convert.ToDateTime(comment["CommentDate"]);
                var updateTimeLimit = _lastUpdate != DateTime.MinValue
                    ? _lastUpdate.Subtract(TimeSpan.FromMilliseconds(UpdatePeriod))
                    : DateTime.MinValue;

                //Mark latest comments
                if (commentDate > updateTimeLimit)
                    commentItem.Background = _latestNewsBackgroundBrush;
            }

            // Make edit panel enable, if current worker and news worker are simple
            var editCommentPanel = commentItem.FindName("EditCommentPanel") as StackPanel;
            if (editCommentPanel != null)
            {
                if ((workerId == AdministrationClass.CurrentWorkerId || AdministrationClass.IsAdministrator) &&
                    _autorizationMode == AutorizationMode.WithAutorization)
                    editCommentPanel.Visibility = Visibility.Visible;
            }

            // Set items source for comment attachments list
            var commentAttachmentList = commentItem.FindName("CommentAttachmentsItemsControl") as ItemsControl;
            if (commentAttachmentList != null)
            {
                if (_newsFeedCore.CommentsAttachments.Columns.Count != 0)
                {
                    var commentAttachmentsView = _newsFeedCore.CommentsAttachments.AsDataView();
                    commentAttachmentsView.RowFilter = string.Format("CommentID = {0}", commentId);
                    commentAttachmentList.ItemsSource = commentAttachmentsView;
                }
            }

            var imageBorder = commentItem.FindName("ImageBorder") as Border;
            if (imageBorder != null)
            {
                if (_workersList.Any(w => w.WorkerID == workerId))
                {
                    var worker = _workersList.First(w => w.WorkerID == workerId);
                    imageBorder.DataContext = worker;
                }
            }
        }

        private void OnCommentButtonClick(object sender, RoutedEventArgs e)
        {
            CloseAddingCommentPanels();

            _renamedCommentAttachments.Clear();

            var commentButton = sender as Button;
            if (commentButton == null) return;

            var addCommentPanel = commentButton.FindName("AddCommentPanel") as Grid;
            if (addCommentPanel == null) return;

            _currentAddingCommentPanel = addCommentPanel;

            var duration = new Duration(TimeSpan.FromMilliseconds(250));
            var showOpacityAnnimation = new DoubleAnimation(1, duration);

            var heightAnnimation = new DoubleAnimation(0, 250, duration) {DecelerationRatio = 0.5};

            addCommentPanel.Visibility = Visibility.Visible;
            commentButton.Visibility = Visibility.Collapsed;
            addCommentPanel.BeginAnimation(OpacityProperty, showOpacityAnnimation);
            addCommentPanel.BeginAnimation(MaxHeightProperty, heightAnnimation);

            var userCommentBorder = commentButton.FindName("CommentImageBorder") as Border;
            if (userCommentBorder != null)
            {
                if (_workersList.Any(w => w.WorkerID == AdministrationClass.CurrentWorkerId))
                {
                    var worker = _workersList.First(w => w.WorkerID == AdministrationClass.CurrentWorkerId);
                    userCommentBorder.DataContext = worker;
                }
            }

            var commentAttachmentsItemsControl = commentButton.FindName("CommentAttachmentsItemsControl") as ItemsControl;
            if (commentAttachmentsItemsControl != null)
            {
                commentAttachmentsItemsControl.ItemsSource = null;
            }

            var addCommentText = commentButton.FindName("AddCommentTextBox") as TextBox;
            if (addCommentText == null) return;
            addCommentText.Focus();
            addCommentText.Text = string.Empty;
        }

        private void OnCancelAddCommentClick(object sender, RoutedEventArgs e)
        {
            var cancelAddCommentButton = sender as Button;
            if (cancelAddCommentButton == null) return;

            var duration = new Duration(TimeSpan.FromMilliseconds(300));

            var addCommentPanel = cancelAddCommentButton.FindName("AddCommentPanel") as Grid;
            if (addCommentPanel != null)
            {
                var hideOpacityAnimation = new DoubleAnimation(0, duration);
                hideOpacityAnimation.Completed += (o, args) =>
                                                  {
                                                      addCommentPanel.Visibility = Visibility.Collapsed;

                                                      // Delete attached files in ftp Temp directory
                                                      foreach (var renamedFile in _renamedCommentAttachments)
                                                      {
                                                          _ftpClient.DeleteFile(renamedFile.Renamed);
                                                      }
                                                      _renamedCommentAttachments.Clear();
                                                  };
                var heightAnimation = new DoubleAnimation(0, duration) {DecelerationRatio = 0.5};
                addCommentPanel.BeginAnimation(OpacityProperty, hideOpacityAnimation);
                addCommentPanel.BeginAnimation(MaxHeightProperty, heightAnimation);
            }

            var news = cancelAddCommentButton.DataContext as DataRowView;
            if (news == null) return;
            var newsId = Convert.ToInt32(news["NewsID"]);

            var showOpacityAnimation = new DoubleAnimation(1, duration);
            if (_newsFeedCore.Comments.AsEnumerable().All(c => Convert.ToInt32(c["NewsID"]) != newsId))
            {
                var firstCommentButton = cancelAddCommentButton.FindName("FirstCommentButton") as Button;
                if (firstCommentButton == null) return;
                firstCommentButton.Visibility = Visibility.Visible;
                firstCommentButton.BeginAnimation(OpacityProperty, showOpacityAnimation);
            }
            else
            {
                var secondCommentButton = cancelAddCommentButton.FindName("SecondCommentButton") as Button;
                if (secondCommentButton == null) return;
                secondCommentButton.Visibility = Visibility.Visible;
                secondCommentButton.BeginAnimation(OpacityProperty, showOpacityAnimation);
            }
        }

        private void OnAddCommentAttachmentsButtonClick(object sender, RoutedEventArgs e)
        {
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

            _uploadCommentAttachments.Clear();
            foreach (var fileName in ofd.FileNames)
            {
                _uploadCommentAttachments.Enqueue(fileName);
            }

            UploadQueueCommentFile();
        }

        private void OnAddCommentButonClick(object sender, RoutedEventArgs e)
        {
            var addCommentButton = sender as Button;
            if (addCommentButton == null) return;

            var addCommentText = addCommentButton.FindName("AddCommentTextBox") as TextBox;
            if (addCommentText == null) return;
            var commentText = addCommentText.Text;
            if (string.IsNullOrEmpty(commentText)) return;

            var news = addCommentButton.DataContext as DataRowView;
            if (news != null)
            {
                var newsId = Convert.ToInt32(news["NewsID"]);
                var commentDate = App.BaseClass.GetDateFromSqlServer();
                var workerId = AdministrationClass.CurrentWorkerId;
                var commentId = _newsFeedCore.AddComment(commentText, commentDate, newsId, workerId);
                AdministrationClass.AddNewAction(96);
                _newsFeedCore.UpdateNewsLastEditing(newsId, commentDate);

                if (_renamedCommentAttachments.Any())
                {
                    AddCommentAttachments(commentId);
                }
            }

            var cancelAddComment = addCommentButton.FindName("CancelAddComment") as Button;
            if (cancelAddComment != null)
            {
                OnCancelAddCommentClick(cancelAddComment, null);
            }
        }

        private void AddCommentAttachments(int commentId)
        {
            if (commentId < 1) return;

            var commentsRows = _newsFeedCore.Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!commentsRows.Any()) return;

            var commentRow = commentsRows.First();
            var newsId = Convert.ToInt32(commentRow["NewsID"]);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if(commentPath == null) return;

            if (!_ftpClient.DirectoryExist(commentPath))
                _ftpClient.MakeDirectory(commentPath);

            foreach (var renamedCommentAttachment in _renamedCommentAttachments)
            {
                var adress = GetDistinctFileName(renamedCommentAttachment.OriginalName, commentPath);

                var commentAttachmentName = Path.GetFileName(adress);
                _newsFeedCore.AddCommentAttachment(commentId, commentAttachmentName);

                var sourceUri = new Uri(renamedCommentAttachment.Renamed, UriKind.Absolute);
                var targetUri = new Uri(adress, UriKind.Absolute);
                var targetUriRelative = sourceUri.MakeRelativeUri(targetUri);

                _ftpClient.Rename(renamedCommentAttachment.Renamed, Uri.UnescapeDataString(targetUriRelative.OriginalString));
            }

            _renamedCommentAttachments.Clear();
        }

        private void OnDeleteCommentButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteCommentButton = sender as Button;
            if (deleteCommentButton == null) return;

            var comment = deleteCommentButton.DataContext as DataRowView;
            if (comment == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить комментарий?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            int commentId;
            Int32.TryParse(comment["CommentID"].ToString(), out commentId);
            int newsId;
            Int32.TryParse(comment["NewsID"].ToString(), out newsId);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if (commentPath != null)
            {
                if (_ftpClient.DirectoryExist(commentPath))
                {
                    _ftpClient.CurrentPath = commentPath;
                    var filesList = _ftpClient.ListDirectory();
                    foreach (var file in filesList)
                    {
                        // Delete all comment attachments on ftp-server
                        _ftpClient.DeleteFile(string.Concat(commentPath, file));
                    }
                }
            }

            // Delete all attachments rows, that comment includes
            _newsFeedCore.DeleteCommentAttachments(commentId);

            _newsFeedCore.DeleteComment(commentId);
            AdministrationClass.AddNewAction(97);
        }

        private void CloseAddingCommentPanels()
        {
            for (var i = 0; i < NewsListBox.Items.Count; i++)
            {
                var newsItem = (ListBoxItem) (NewsListBox.ItemContainerGenerator.ContainerFromIndex(i));
                var myContentPresenter = FindFirstElementInVisualTree<ContentPresenter>(newsItem);
                var myDataTemplate = myContentPresenter.ContentTemplate;
                var addCommentPanel = myDataTemplate.FindName("AddCommentPanel", myContentPresenter) as Grid;
                if (addCommentPanel == null) return;

                if (addCommentPanel.Visibility == Visibility.Visible)
                {
                    var cancelAddComment = addCommentPanel.FindName("CancelAddComment") as Button;
                    if (cancelAddComment != null)
                    {
                        OnCancelAddCommentClick(cancelAddComment, null);
                    }
                }
            }
        }

        private string GetCommentAttachmentPath(int newsId, int commentId)
        {
            var newsRows = _newsFeedCore.News.Select(string.Format("NewsID = {0}", newsId));
            if (!newsRows.Any()) return null;

            // Get news status name, and make basic parrent directory path
            var newsStatusId = Convert.ToInt32(newsRows.First()["NewsStatus"]);
            var newsStatusName = new IdToNewsStatusConverter().Convert(newsStatusId, typeof (string),
                "NewsStatusName", CultureInfo.InvariantCulture).ToString();

            var newsDir = string.Format("Сообщение_[id]{0}", newsId);
            var commentDir = string.Format("Комментарий_[id]{0}", commentId);

            var commentPath = string.Concat(_basicDirectory, newsStatusName, "/", newsDir,
                "/", commentDir, "/");

            return commentPath;
        }

        private void OnDeleteNewCommentAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteNewAttachmentButton = sender as Button;
            if (deleteNewAttachmentButton == null) return;

            if (deleteNewAttachmentButton.DataContext == null) return;
            var renamedFile = (RenamedFile)deleteNewAttachmentButton.DataContext;

            _ftpClient.DeleteFile(renamedFile.Renamed);
            _renamedCommentAttachments.Remove(renamedFile);
        }

        #endregion



        #region Ftp


        #region Uploading

        private void UploadQueueFile()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            while (true)
            {
                if (!_uploadFiles.Any()) return;

                var filePath = _uploadFiles.Dequeue();

                var fileUploadingEnable = GetFileUploadingEnable(filePath);
                if (!fileUploadingEnable)
                {
                    UploadQueueFile();
                    return;
                }

                _uploadingMode = UploadingMode.FromAddingNews;
                SendNewsButton.IsEnabled = false;
                CancelNewsButton.IsEnabled = false;

                AddAttachmentsProgressBar.Value = 0;
                AddNewsUploadingStatusPanel.Visibility = Visibility.Visible;
                AddAttachmentButton.Visibility = Visibility.Collapsed;

                // Get distinct file name in Temp directory
                var adress = GetDistinctFileName(filePath, _tempDirectory);

                _renamedFiles.Add(new RenamedFile(filePath, adress));
                var uri = new Uri(adress);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadFileCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }
        }     

        private void UploadQueueCommentFile()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            while (true)
            {
                if (!_uploadCommentAttachments.Any()) return;

                var filePath = _uploadCommentAttachments.Dequeue();

                var fileUploadingEnable = GetFileUploadingEnable(filePath);
                if (!fileUploadingEnable)
                {
                    UploadQueueCommentFile();
                    return;
                }

                _uploadingMode = UploadingMode.FromAddingComment;

                if (_currentAddingCommentPanel != null)
                {
                    var uploadProgress = _currentAddingCommentPanel.FindName("AddAttachmentsProgressBar") as ProgressBar;
                    if (uploadProgress != null)
                    {
                        uploadProgress.Value = 0;
                        uploadProgress.Visibility = Visibility.Visible;
                    }
                    var addCommentAttachmentsButton =
                        _currentAddingCommentPanel.FindName("AddCommentAttachmentsButton") as Button;
                    if (addCommentAttachmentsButton != null)
                    {
                        addCommentAttachmentsButton.Visibility = Visibility.Collapsed;
                    }
                }

                // Get distinct file name in Temp directory
                var adress = GetDistinctFileName(filePath, _tempDirectory);

                _renamedCommentAttachments.Add(new RenamedFile(filePath, adress));
                var uri = new Uri(adress);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadCommentAttachmentCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }
        }

        private string GetDistinctFileName(string filePath, string parrentDirectory)
        {
            var j = 1;
            var originalFileName = Path.GetFileNameWithoutExtension(filePath);
            var fileName = originalFileName;
            var fileExtension = Path.GetExtension(filePath);

            var adress = string.Concat(parrentDirectory, fileName + fileExtension);
            while (_ftpClient.FileExist(adress))
            {
                fileName = originalFileName;
                fileName += "(" + j + ")";
                j++;
                adress = string.Concat(parrentDirectory, fileName + fileExtension);
            }

            return adress;
        }

        private static bool GetFileUploadingEnable(string filePath)
        {
            var fileLocked = AdministrationClass.IsFileLocked(new FileInfo(filePath));
            while (fileLocked)
            {
                var message =
                    string.Format(
                        "Файл {0} невозможно прикрепить. " +
                        "\nФайл заблокирован другим процессом." +
                        "\nПродолжить отправку без данного файла?",
                        filePath);
                if (MessageBox.Show(message, "Файл заблокирован", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    return false;
                }

                fileLocked = AdministrationClass.IsFileLocked(new FileInfo(filePath));
            }

            return true;
        }

        private string GetNewsPath(int newsId)
        {
            var newsRows = _newsFeedCore.News.Select(string.Format("NewsID = {0}", newsId));
            if (!newsRows.Any()) return null;

            // Get news status name, and make basic parrent directory path
            var newsStatusId = Convert.ToInt32(newsRows.First()["NewsStatus"]);
            var newsStatusName = new IdToNewsStatusConverter().Convert(newsStatusId, typeof (string),
                "NewsStatusName", CultureInfo.InvariantCulture).ToString();

            var newsDir = string.Format("Сообщение_[id]{0}", newsId);

            var newsPath = string.Concat(_basicDirectory, newsStatusName, "/", newsDir, "/");

            return newsPath;
        }

        private void OnFtpClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var progress = (e.BytesSent/(double) e.TotalBytesToSend)*100;

            switch (_uploadingMode)
            {
                case UploadingMode.FromAddingNews:
                    AddAttachmentsProgressBar.Value = progress;
                    break;
                case UploadingMode.FromAddingComment:
                {
                    if (_uploadCommentAttachments != null)
                    {
                        var uploadProgress =
                            _currentAddingCommentPanel.FindName("AddAttachmentsProgressBar") as ProgressBar;
                        if (uploadProgress != null)
                        {
                            uploadProgress.Value = progress;
                        }
                    }
                }
                    break;
            }
        }

        private void OnFtpClientUploadFileCompleted(object sender,
            UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadFileCompleted;

            SendNewsButton.IsEnabled = true;
            CancelNewsButton.IsEnabled = true;
            NewAttachmentsListBox.Items.Add(_renamedFiles.Last());
            AddNewsUploadingStatusPanel.Visibility = Visibility.Collapsed;
            AddAttachmentButton.Visibility = Visibility.Visible;

            UploadQueueFile();
        }

        private void OnFtpClientUploadCommentAttachmentCompleted(object sender,
            UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadCommentAttachmentCompleted;

            if (_currentAddingCommentPanel != null)
            {
                var uploadProgress = _currentAddingCommentPanel.FindName("AddAttachmentsProgressBar") as ProgressBar;
                if (uploadProgress != null)
                {
                    uploadProgress.Visibility = Visibility.Collapsed;
                }
                var addCommentAttachmentsButton =
                        _currentAddingCommentPanel.FindName("AddCommentAttachmentsButton") as Button;
                if (addCommentAttachmentsButton != null)
                {
                    addCommentAttachmentsButton.Visibility = Visibility.Visible;
                }
                var commentAttachnmentsItemsControl =
                    _currentAddingCommentPanel.FindName("CommentAttachmentsItemsControl") as ItemsControl;
                if (commentAttachnmentsItemsControl != null)
                {
                    commentAttachnmentsItemsControl.ItemsSource = null;
                    commentAttachnmentsItemsControl.ItemsSource = _renamedCommentAttachments;
                }
            }

            UploadQueueCommentFile();
        }

        #endregion


        #region Downloading

        private void DownloadQueueFile()
        {
            while (true)
            {
                if (!_downloadImagesFromNewsQueue.Any())
                {
                    if (_processWindow != null)
                        _processWindow.Close(true);
                    _processWindow = null;
                    return;
                }

                var fileName = _downloadImagesFromNewsQueue.Dequeue();
                var fileTempPath = Path.Combine(_tempPathForDownloadInQueue, fileName);

                if (File.Exists(fileTempPath))
                {
                    if (!_downloadImagesFromNewsQueue.Any())
                    {
                        try
                        {
                            Process.Start(fileTempPath);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                    }
                    continue;
                }


                if (_ftpClient.IsBusy)
                {
                    MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                    return;
                }

                if (!Directory.Exists(_tempPathForDownloadInQueue))
                {
                    Directory.CreateDirectory(_tempPathForDownloadInQueue);
                }

                var ftpFilePath = string.Concat(_ftpSourcePathForDownloadInQueue, "/", fileName);
                if (_ftpClient.FileExist(ftpFilePath))
                {
                    _neededOpeningFilePathInQueue = fileTempPath;
                    var uri = new Uri(ftpFilePath);

                    if (_processWindow == null)
                    {
                        _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                        _processWindow.Show(Window.GetWindow(this), true);
                    }

                    _downloadingMode = DownloadingMode.InQueue;

                    _fileSize = _ftpClient.GetFileSize(ftpFilePath);
                    _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileInQueueuCompleted;
                    _ftpClient.DownloadFileAsync(uri, fileTempPath);

                    break;
                }

                MessageBox.Show(string.Format("Невозможно найти файл {0}, возможно он был удалён ранее.", fileName));
            }
        }

        private void OnFtpClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.BytesReceived/(double) _fileSize;
            if (_processWindow == null) return;

            switch (_downloadingMode)
            {
                case DownloadingMode.Single:
                    _processWindow.Progress = progress*100;
                    _processWindow.Text = string.Format("Загрузка файла... \n{0} кБ", e.BytesReceived/1024);
                    break;
                case DownloadingMode.InQueue:
                    var index = _downloadFilesInQueueCount - _downloadImagesFromNewsQueue.Count;
                    _processWindow.Progress = progress*100;
                    _processWindow.Text = string.Format("Загрузка файла {0} из {1} \n{2} кБ", index,
                        _downloadFilesInQueueCount, e.BytesReceived/1024);
                    break;
            }
        }

        private void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (_processWindow != null)
                _processWindow.Text = "Открытие файла...";

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
            _processWindow = null;

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;
        }

        private void OnFtpClientDownloadFileInQueueuCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (!_downloadImagesFromNewsQueue.Any())
            {
                if (_processWindow != null)
                    _processWindow.Text = "Открытие файла...";

                try
                {
                    Process.Start(_neededOpeningFilePathInQueue);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
            }

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileInQueueuCompleted;
            DownloadQueueFile();
        }

        #endregion


        #endregion


        private void OnAttachmentFileNameClick(object sender, RoutedEventArgs e)
        {
            var showAttachmentButton = sender as Button;
            if (showAttachmentButton == null) return;

            var attachment = showAttachmentButton.DataContext as DataRowView;
            if (attachment == null) return;

            var attachmentName = attachment["AttachmentName"].ToString();

            int newsId;
            Int32.TryParse(attachment["NewsID"].ToString(), out newsId);

            var newsPath = GetNewsPath(newsId);
            if (newsPath == null) return;

            var tempPath = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId, attachmentName);
            var tempDirectory = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId);
            if (File.Exists(tempPath))
            {
                try
                {
                    Process.Start(tempPath);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            var filePath = string.Concat(newsPath, attachmentName);
            if (_ftpClient.FileExist(filePath))
            {
                _neededOpeningFilePath = tempPath;
                var uri = new Uri(filePath);

                if (_processWindow != null)
                    _processWindow.Close(true);

                _processWindow = new WaitWindow {Text = "Загрузка файла..."};
                _processWindow.Show(Window.GetWindow(this), true);

                _downloadingMode = DownloadingMode.Single;

                _fileSize = _ftpClient.GetFileSize(filePath);
                _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
                _ftpClient.DownloadFileAsync(uri, tempPath);
            }
            else
            {
                MessageBox.Show("Невозможно найти указанный файл, возможно он был удалён ранее.");
            }
        }

        private void OnCommentAttachmentFileNameClick(object sender, RoutedEventArgs e)
        {
            var showCommentAttachmentButton = sender as Button;
            if (showCommentAttachmentButton == null) return;

            var commentAttachment = showCommentAttachmentButton.DataContext as DataRowView;
            if (commentAttachment == null) return;

            var commentAttachmentName = commentAttachment["CommentAttachmentName"].ToString();

            int commentId;
            Int32.TryParse(commentAttachment["CommentID"].ToString(), out commentId);

            var commentRows = _newsFeedCore.Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!commentRows.Any()) return;

            var commentRow = commentRows.First();
            var newsId = Convert.ToInt32(commentRow["NewsID"]);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if (commentPath == null) return;

            var tempPath = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId, "Комментарий_[id]" + commentId,
                commentAttachmentName);
            var tempDirectory = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId, "Комментарий_[id]" + commentId);
            if (File.Exists(tempPath))
            {
                try
                {
                    Process.Start(tempPath);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            var filePath = string.Concat(commentPath, "/", commentAttachmentName);
            if (_ftpClient.FileExist(filePath))
            {
                _neededOpeningFilePath = tempPath;
                var uri = new Uri(filePath);

                if (_processWindow != null)
                    _processWindow.Close(true);

                _processWindow = new WaitWindow {Text = "Загрузка файла..."};
                _processWindow.Show(Window.GetWindow(this), true);

                _downloadingMode = DownloadingMode.Single;

                _fileSize = _ftpClient.GetFileSize(filePath);
                _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
                _ftpClient.DownloadFileAsync(uri, tempPath);
            }
            else
            {
                MessageBox.Show("Невозможно найти указанный файл, возможно он был удалён ранее.");
            }
        }

        private void OnShowAllImagesInNewsButtonClick(object sender, RoutedEventArgs e)
        {
            var showAllImagesInNewsButton = sender as Button;
            if (showAllImagesInNewsButton == null) return;

            var news = showAllImagesInNewsButton.DataContext as DataRowView;
            if(news == null) return;

            int newsId;
            Int32.TryParse(news["NewsID"].ToString(), out newsId);

            var attachmentList = showAllImagesInNewsButton.FindName("AttachmentsList") as ItemsControl;
            if (attachmentList != null)
            {
                var attachmentsView = attachmentList.ItemsSource as DataView;
                if (attachmentsView != null)
                {
                    if (attachmentsView.Count != 0)
                    {
                        var imagesList = (from DataRowView attachment in attachmentsView
                            select attachment["AttachmentName"].ToString()
                            into attachmentName
                            let extension = Path.GetExtension(attachmentName)
                            where !string.IsNullOrEmpty(extension)
                            where _imageExtensions.Contains(extension.ToLower())
                            select attachmentName).ToList();

                        _downloadImagesFromNewsQueue.Clear();
                        foreach (var image in imagesList)
                        {
                            _downloadImagesFromNewsQueue.Enqueue(image);
                        }

                        _downloadFilesInQueueCount = _downloadImagesFromNewsQueue.Count;
                        _tempPathForDownloadInQueue = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId);
                        _ftpSourcePathForDownloadInQueue = GetNewsPath(newsId);
                        DownloadQueueFile();
                    }
                }
            }
        }



        private void OnEditNewsButtonClick(object sender, RoutedEventArgs e)
        {
            var editNewsButton = sender as Button;
            if (editNewsButton == null) return;

            var news = editNewsButton.DataContext as DataRowView;
            if (news == null) return;

            var editCommentPage = new EditNewsAndCommentPage(news, EditNewsAndCommentPage.EditMode.News);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(editCommentPage, "Редактировать сообщение");
            }
        }

        private void OnEditCommentButtonClick(object sender, RoutedEventArgs e)
        {
            var editCommentButton = sender as Button;
            if (editCommentButton == null) return;

            var comment = editCommentButton.DataContext as DataRowView;
            if (comment == null) return;

            var editCommentPage = new EditNewsAndCommentPage(comment, EditNewsAndCommentPage.EditMode.Comment);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(editCommentPage, "Редактировать комментарий");
            }
        }



        private static T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child is T)
                {
                    return (T) child;
                }

                var result = FindFirstElementInVisualTree<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }



        private void OnProdStatusFilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var prodStatus = ProdStatusFilterComboBox.SelectedItem as DataRowView;
            if(prodStatus == null)
            {
                _currentProdStatusId = null;
            }
            else
            {
                var prodStatusId = Convert.ToInt32(prodStatus["ProdStatusID"]);
                _currentProdStatusId = prodStatusId;
            }

            if (EnableProdStatusFilterCheckBox.IsChecked.HasValue && EnableProdStatusFilterCheckBox.IsChecked.Value)
                CreateView(_filteringMode);
        }

        private void OnEnableProdStatusFilterCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (_currentNewsStatus == null) return;
            var prodStatus = ProdStatusFilterComboBox.SelectedItem as DataRowView;
            if (prodStatus == null) return;
            var prodStatusId = Convert.ToInt32(prodStatus["ProdStatusID"]);
            _currentProdStatusId = prodStatusId;

            _filteringMode = FilteringMode.ByNewsStatusAndProdStatus;
            CreateView(_filteringMode);
        }

        private void OnEnableProdStatusFilterCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (_currentNewsStatus == null) return;

            _currentProdStatusId = null;
            _filteringMode = FilteringMode.ByNewsStatus;
            CreateView(_filteringMode);
        }
    }
}
