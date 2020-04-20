using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FA2.Notifications;
using FA2.ToolsPages;
using FAIIControlLibrary.UserControls;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private bool _firstRun = true;
        private DateTime _currentDate;
        private readonly System.Windows.Forms.Timer _timeTimer;
        private readonly System.Windows.Forms.Timer _workinDayTimeTimer;

        private TimeSpan _currentWorkerTimeAtWork;

        readonly IdToModuleNameConverter _idToModuleNameConverter = new IdToModuleNameConverter();

        private StaffClass _sc;
        private TimeTrackingClass _ttc;

        private NewsFeed _newsFeed = new NewsFeed();

        private CatalogPage _cp;
        private TimeTrackingPage _ttp;
        private AdministrationPage _administrationPage;
        private TimeControlPage _timeControlPage;
        private ServiceEquipmentPage _serviceEquipmentPage;
        private TaskPage _taskPage;
        private TechnologyProblemPage _technologyProblemPage;
        private StimulationPage _stimulationPage;
        private ProdRoomsPage _prodRoomsPage;
        private WorkshopMapPage _workshopMapPage;
        private WorkerRequestsPage _workerRequestsPage;
        private StaffPage _staffPage;
        private TimesheetPage _timeSheetPage;
        private PlannedWorksPage _plannedWorksPage;

        private bool _workshopMode = true;

        public MainWindow()
        {
            Splasher.SetStatusText("Загрузка интерфейса");
            InitializeComponent();

            _timeTimer = new System.Windows.Forms.Timer { Interval = 60000 };
            _timeTimer.Tick += TimeTimer_Tick;

            _workinDayTimeTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _workinDayTimeTimer.Tick += WorkinDayTimeTimer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Splasher.SetStatusText("Загрузка новостной ленты");

            if (_firstRun)
            {
                _firstRun = false;

                GetClasses();
                SetBindings();

                BindingLoginData();

                MainFrame.Navigate(_newsFeed);


                if (App.AppArguments[0] == "-m")
                {
                    var lastExit = AdministrationClass.LastModuleExit(AdministrationClass.Modules.NewsFeed);
                    _newsFeed.ShowNews(lastExit);

                    NotificationManager.ShowNotifications(AdministrationClass.CurrentWorkerId);

                    _workshopMode = false;
                }
            }

            Splasher.CloseSplashWindow();
            ShowInTaskbar = true;
            WindowState = WindowState.Maximized;
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            if (sender != null)
                _currentDate = _currentDate.AddMinutes(1);

            TimeLabel.Content = _currentDate.ToShortTimeString();

            if (DateTimeFormatInfo.CurrentInfo != null)
                DayLabel.Content =
                    DateTimeFormatInfo.CurrentInfo.DayNames[
                        int.Parse(_currentDate.DayOfWeek.ToString("D"))];

            DateLabel.Content = _currentDate.Date.ToString("dd MMMM");

            WeekOfYearLabel.Content = GetIso8601WeekOfYear(_currentDate); //_currentDate.W
        }

        private void WorkinDayTimeTimer_Tick(object sender, EventArgs e)
        {
            _currentWorkerTimeAtWork = _currentWorkerTimeAtWork.Add(new TimeSpan(0, 0, 1));

            if (_currentWorkerTimeAtWork.Days == 0)
            {
                WorkinDayTimeLabel.Content = _currentWorkerTimeAtWork.ToString(@"hh\:mm\:ss");
            }
            else
            {
                int hours = (_currentWorkerTimeAtWork.Days * 24 + _currentWorkerTimeAtWork.Hours);
                WorkinDayTimeLabel.Content = hours + ":" +
                                             _currentWorkerTimeAtWork.ToString(@"mm\:ss");
            }
        }

        private void GetClasses()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetTimeTrackingClass(ref _ttc);
        }

        private void SetBindings()
        {
            _currentDate = App.BaseClass.GetDateFromSqlServer();
            _timeTimer.Start();
            TimeTimer_Tick(null, null);

            if (App.AppArguments[0] == "-m")
            {
                MenuListBox.ItemsSource = AdministrationClass.GetAvailableModulesForWorker().DefaultView;
                AdministrationClass.GetFavoritesModulesIdsForWorker();

                MenuGroupsListBox.SelectedIndex = 0;
                MenuGroupsListBox_SelectionChanged(null, null);

                LoadPersonalInformation();

                SetDefaultViewWorkingDayGrid();
                CalculateWorkerTime();
            }
            else if (App.AppArguments[0] == "-w")
            {
                //GoHomeButton_Click(null, null);

                UserGrid.Visibility = Visibility.Collapsed;
                MinimazeMenu();

                _newsFeed.ShowNews();

                WorkerLoginGrid.Visibility = Visibility.Visible;
            }
            //GetWorkerStat();
        }

        private void BindingLoginData()
        {
            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.DisplayMemberPath = "FactoryName";
            FactoriesComboBox.SelectedValuePath = "FactoryID";
            FactoriesComboBox.ItemsSource = _sc.GetFactories();
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.Items.MoveCurrentToFirst();

            WorkersNamesListBox.SelectionChanged -= WorkersNamesListBox_SelectionChanged;
            WorkersNamesListBox.SelectedValuePath = "WorkerID";
            WorkersNamesListBox.SelectionChanged += WorkersNamesListBox_SelectionChanged;
            WorkersNamesListBox.Items.MoveCurrentToFirst();

            WorkersGroupsComboBox.SelectionChanged -= WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            WorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkersGroupsComboBox.SelectedValue = 2;
            WorkersGroupsComboBox.SelectionChanged += WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox_SelectionChanged(null, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Shutdown();
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private void MenuGroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuGroupsListBox.SelectedIndex == 0)
            {
                ((DataView) (MenuListBox.ItemsSource)).RowFilter = "ModuleID IN (" +
                                                                   AdministrationClass.FavoritesModulesForWorker + ")";
            }
            else
            {
                ((DataView)(MenuListBox.ItemsSource)).RowFilter = "ModulesGroupsID= " + MenuGroupsListBox.SelectedIndex;
            }
        }

        private void MenuTogleButton_Checked(object sender, RoutedEventArgs e)
        {
            //MenuPopupBorder
            if (MenuPopupBorder.Child == null)
            {
                MainGrid.Children.Remove(MenuGrid);
                MenuPopupBorder.Child = MenuGrid;
            }
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem == null) return;

            HideCatalogGrid();

            try
            {
                LoadPageById(Convert.ToInt32(((DataRowView)MenuListBox.SelectedItem)["ModuleID"]));
            }
            catch (Exception exp)
            {

                MessageBox.Show(exp.Message);
            }

        }

        private void MinimazeMenu()
        {
            if (MenuPopupBorder.Child == null)
            {
                MainGrid.Children.Remove(MenuGrid);
                MenuPopupBorder.Child = MenuGrid;
            }

            MenuPopup.IsOpen = false;
        }

        private void MenuPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                MenuListBox.SelectedItem = null;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void LoadPageById(int moduleId)
        {
            switch (moduleId)
            {
                case (int)AdministrationClass.Modules.OperationCatalog:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is CatalogPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.OperationCatalog);
                            if (_cp == null) _cp = new CatalogPage(hasFullAccess);
                            _cp.Tag = moduleId;
                            MainFrame.Navigate(_cp);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.Workers:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is StaffPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.Workers);
                            if (_staffPage == null) _staffPage = new StaffPage(hasFullAccess);
                            _staffPage.Tag = moduleId;
                            MainFrame.Navigate(_staffPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.TimeTracking:
                    {
                        if (_ttc.GetIsDayEnd(AdministrationClass.CurrentWorkerId))
                        {
                            //MenuTogleButton.IsChecked = false;

                            //MessageBox.Show("Для запуска данного модуля необходимо начать рабочий день!", "Информация",
                            //    MessageBoxButton.OK, MessageBoxImage.Information);
                            //break;


                            BlinkWorkingDayButton();
                        }
                        else
                        {
                            if (!MainFrame.HasContent || !(MainFrame.Content is TimeTrackingPage))
                            {
                                if (_ttp == null) _ttp = new TimeTrackingPage();
                                _ttp.Tag = moduleId;
                                MainFrame.Navigate(_ttp);
                                ShowWaitAnimation();
                            }
                        }
                        MinimazeMenu();
                    }
                    break;

                case (int)AdministrationClass.Modules.Administration:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is AdministrationPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.Administration);
                            if (_administrationPage == null)
                                _administrationPage = new AdministrationPage(hasFullAccess);
                            _administrationPage.Tag = moduleId;
                            MainFrame.Navigate(_administrationPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.ControlTimeTracking:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is TimeControlPage))
                        {
                            if (_timeControlPage == null) _timeControlPage = new TimeControlPage();
                            _timeControlPage.Tag = moduleId;
                            MainFrame.Navigate(_timeControlPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.ServiceEquipment:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is ServiceEquipmentPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.ServiceEquipment);
                            if (_serviceEquipmentPage == null)
                                _serviceEquipmentPage = new ServiceEquipmentPage(hasFullAccess);
                            _serviceEquipmentPage.Tag = moduleId;
                            MainFrame.Navigate(_serviceEquipmentPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.TasksPage:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is TaskPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.TasksPage);
                            if (_taskPage == null) _taskPage = new TaskPage(hasFullAccess);
                            _taskPage.Tag = moduleId;
                            MainFrame.Navigate(_taskPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.TechnologyProblem:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is TechnologyProblemPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.TechnologyProblem);
                            if (_technologyProblemPage == null) _technologyProblemPage = new TechnologyProblemPage(hasFullAccess);
                            _technologyProblemPage.Tag = moduleId;
                            MainFrame.Navigate(_technologyProblemPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.WorkersStimulation:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is StimulationPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.WorkersStimulation);
                            if (_stimulationPage == null) _stimulationPage = new StimulationPage(hasFullAccess);
                            _stimulationPage.Tag = moduleId;
                            MainFrame.Navigate(_stimulationPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;

                case (int)AdministrationClass.Modules.ProductionRooms:
                    {
                        if (!MainFrame.HasContent || !(MainFrame.Content is ProdRoomsPage))
                        {
                            var hasFullAccess =
                            AdministrationClass.HasFullAccess(AdministrationClass.Modules.ProductionRooms);
                            if (_prodRoomsPage == null) _prodRoomsPage = new ProdRoomsPage(hasFullAccess);
                            _prodRoomsPage.Tag = moduleId;
                            MainFrame.Navigate(_prodRoomsPage);
                            ShowWaitAnimation();
                        }

                        MinimazeMenu();
                    }
                    break;

                case (int)AdministrationClass.Modules.WorkshopMap:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is WorkshopMapPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.WorkshopMap);
                            if (_workshopMapPage == null) _workshopMapPage = new WorkshopMapPage(hasFullAccess);
                            _workshopMapPage.Tag = moduleId;
                            MainFrame.Navigate(_workshopMapPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;
                case (int)AdministrationClass.Modules.WorkerRequests:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is WorkerRequestsPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.WorkerRequests);
                            if (_workerRequestsPage == null) _workerRequestsPage = new WorkerRequestsPage(hasFullAccess);
                            _workerRequestsPage.Tag = moduleId;
                            MainFrame.Navigate(_workerRequestsPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;
                case (int)AdministrationClass.Modules.TimeSheet:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is TimesheetPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.TimeSheet);
                            if (_timeSheetPage == null) _timeSheetPage = new TimesheetPage();
                            _timeSheetPage.Tag = moduleId;
                            MainFrame.Navigate(_timeSheetPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;
                case (int)AdministrationClass.Modules.PlannedWorks:
                    {
                        MinimazeMenu();

                        if (!MainFrame.HasContent || !(MainFrame.Content is PlannedWorksPage))
                        {
                            var hasFullAccess =
                                AdministrationClass.HasFullAccess(AdministrationClass.Modules.PlannedWorks);
                            if (_plannedWorksPage == null) _plannedWorksPage = new PlannedWorksPage(hasFullAccess);
                            _plannedWorksPage.Tag = moduleId;
                            MainFrame.Navigate(_plannedWorksPage);
                            ShowWaitAnimation();
                        }
                    }
                    break;
            }
        }

        public void BlinkWorkingDayButton()
        {
            var t = new System.Windows.Forms.Timer {Interval = 200};

            int count = 0;

            t.Tick += (s, e) =>
            {
                if (count != 3)
                {
                    if (StartWorkingDayButton.Style == StartWorkingDayButton.FindResource("RedBtn"))
                        StartWorkingDayButton.Style = (Style) StartWorkingDayButton.FindResource("GreenBtn");

                    else StartWorkingDayButton.Style = (Style)StartWorkingDayButton.FindResource("RedBtn");
                    count++;
                }
                else
                {
                    StartWorkingDayButton.Style = (Style)StartWorkingDayButton.FindResource("GreenBtn");
                    t.Stop();
                }
            };

            t.Start();


        }

        #region Catalogs
        public void ShowCatalogGrid(Page page, string title)
        {
            CatalogGrid.Visibility = Visibility.Visible;
            CatalogPanel.Visibility = Visibility.Visible;
            CatalogTitleTextBox.Text = title;
            MainFrame.IsEnabled = false;
            CatalogFrame.Navigate(page);

            var color = new Color {A = 153, R = 0, G = 0, B = 0};

            var animation = new ColorAnimation(color, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            CatalogGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        public void HideCatalogGrid()
        {
            CatalogPanel.Visibility = Visibility.Hidden;
            MainFrame.IsEnabled = true;
            var color = new Color();
            var animation = new ColorAnimation(color, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            animation.Completed += (s, e) =>
            {
                CatalogGrid.Visibility = Visibility.Hidden;
                if (CatalogFrame.HasContent)
                    CatalogFrame.Content = null;
            };
            CatalogGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
        #endregion

        private void HideCatalogGridButton_Click(object sender, RoutedEventArgs e)
        {
            HideCatalogGrid();
        }



        private void LoadPersonalInformation()
        {
            UserNameLabel.Content = _sc.GetWorkerName(AdministrationClass.CurrentWorkerId);
            CurrentUserImage.Source =
                AdministrationClass.ObjectToBitmapImage(
                    _sc.GetObjectPhotoFromDataBase(AdministrationClass.CurrentWorkerId));
        }


        #region TimeTrackingPath

        private void StartWorkingDayButton_Click(object sender, RoutedEventArgs e)
        {
            _ttc.StartWorkingDay();
            ChangeWorkingDayGrid();

            StartDinnerButton.IsEnabled = _ttc.EnableDinnerButton;

            _currentWorkerTimeAtWork = new TimeSpan(0, 0, 0);
            _workinDayTimeTimer.Start();

            MenuListBox.SelectedItems.Clear();

            //if (MainFrame.Content.GetType().Name == "MainPage")
            //{
            //    //((MainPage)MainFrame.Content).UnLockTiles();

            //}

            //MessageBox.Show("UnLockTiles");

            WorkinDayTimeTimer_Tick(null, null);
        }

        private void StartDinnerButton_Click(object sender, RoutedEventArgs e)
        {
            _ttc.StartDinner();
            ChangeWorkingDayGrid();

            _workinDayTimeTimer.Stop();
        }

        private void EndWorkingDayButton_Click(object sender, RoutedEventArgs e)
        {
            _ttc.EndWorkingDay();
            ChangeWorkingDayGrid();

            _workinDayTimeTimer.Stop();

            GoHomeButton_Click(null, null);

            ClearModules();

            //while (MainFrame.Content.GetType().Name != "MainPage")
            //{
            //    MainFrame.GoBack();
            //    DispatcherHelper.DoEvents();
            //}
            //MessageBox.Show("GoHome");

            //if (MainFrame.Content.GetType().Name == "MainPage")
            //{
            //    //((MainPage)MainFrame.Content).LockTiles();
            //    MessageBox.Show("LockTiles");
            //}
            //MessageBox.Show("LockTiles");
        }

        private void EndDinnerButton_Click(object sender, RoutedEventArgs e)
        {
            _ttc.EndDinner();
            ChangeWorkingDayGrid();

            _workinDayTimeTimer.Start();
        }

        private void ChangeWorkingDayGrid()
        {
            WorkingDayStackPanel.Children.Clear();

            if (_ttc.WorkingDayButtonsVis.StartWorkingDayButtonVis)
                WorkingDayStackPanel.Children.Add(StartWorkingDayButton);

            if (_ttc.WorkingDayButtonsVis.DinnerButtonVis)
                WorkingDayStackPanel.Children.Add(StartDinnerButton);

            if (_ttc.WorkingDayButtonsVis.EndWorkingDayButtonVis)
                WorkingDayStackPanel.Children.Add(EndWorkingDayButton);

            if (_ttc.WorkingDayButtonsVis.EndDinnerButtonVis)
                WorkingDayStackPanel.Children.Add(EndDinnerButton);

             StartDinnerButton.IsEnabled = _ttc.EnableDinnerButton;
        }

        public void CalculateWorkerTime()
        {
            var wtaw = _ttc.CurrentWorkerTimeAtWork();

            if (wtaw == new TimeSpan(-1, 0, 0))
            {
                WorkinDayTimeLabel.Content = "00:00:00";
                _workinDayTimeTimer.Stop();

                //if (MainFrame.Content.GetType().Name == "MainPage")
                //{
                //    //((MainPage)MainFrame.Content).LockTiles();

                //}
                //MessageBox.Show("LockTiles");
            }
            else
            {
                _currentWorkerTimeAtWork = wtaw;
                if (_ttc.StartWorkinDayTimeTimer)
                {
                    _workinDayTimeTimer.Start();
                    WorkinDayTimeTimer_Tick(null, null);
                }
                else
                {
                    _ttc.StartWorkinDayTimeTimer = true;
                    _workinDayTimeTimer.Stop();
                    WorkinDayTimeTimer_Tick(null, null);
                }

                //if (MainFrame.Content.GetType().Name == "MainPage")
                //{
                //    //((MainPage)MainFrame.Content).UnLockTiles();

                //}
                //MessageBox.Show("UnLockTiles");
            }
        }

        public void SetDefaultViewWorkingDayGrid()
        {
            _ttc.GetWorkingDayStage();
            ChangeWorkingDayGrid();
        }

        #endregion

        private void GoHomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.HasContent)
            {
                if (!(MainFrame.Content is NewsFeed))
                {
                    _newsFeed = new NewsFeed();

                    MainFrame.Navigate(_newsFeed);

                    var lastExit = AdministrationClass.LastModuleExit(AdministrationClass.Modules.NewsFeed);
                    _newsFeed.ShowNews(lastExit);

                    NotificationManager.ShowNotifications(AdministrationClass.CurrentWorkerId);
                }
            }

            if (MenuPopupBorder.Child != null)
            {
                MenuPopupBorder.Child = null;
                MainGrid.Children.Add(MenuGrid);
            }

            MenuListBox.SelectedItems.Clear();
        }

        public void ShowToolsGrid(Page content, string title)
        {
            ShowWaitAnimation();

            ToolsGrid.Visibility = Visibility.Visible;
            ToolsPanel.Visibility = Visibility.Visible;
            ToolsTitleTextBox.Text = title;
            MainFrame.IsEnabled = false;
            ToolsFrame.Navigate(content);

            var color = new Color {A = 153, R = 0, G = 0, B = 0};
            var animation = new ColorAnimation(color, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            ToolsGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        public void HideToolsGrid()
        {
            ToolsPanel.Visibility = Visibility.Collapsed;
            MainFrame.IsEnabled = true;
            var color = new Color();
            var animation = new ColorAnimation(color, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            animation.Completed += (s, e) =>
            {
                ToolsGrid.Visibility = Visibility.Collapsed;
                if (ToolsFrame.HasContent)
                    ToolsFrame.Content = null;
            };
            ToolsGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        public void ShowWaitAnimation()
        {
            try
            {
                LoadingAnimationBorder.Child = null;
                LoadingAnimationGrid.Visibility = Visibility.Visible;
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                LoadingAnimationBorder.Child = stackPanel;
                //var circularFadingLine = new CircularFadingLine();

                var vw = new VisualWrapper
                {
                    Child = CreateAnnimationOnWorkerThread(),
                    Width = 17,
                    Height = 17,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                stackPanel.Children.Add(vw);

                var textBlock = new TextBlock {Width = 80};

                var stranim = new StringAnimationUsingKeyFrames
                {
                    AutoReverse = false,
                    RepeatBehavior = RepeatBehavior.Forever,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 2))
                };

                var kf1 = new DiscreteStringKeyFrame
                {
                    Value = "Загрузка",
                    KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0))
                };

                var kf2 = new DiscreteStringKeyFrame
                {
                    Value = "Загрузка.",
                    KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 500))
                };

                var kf3 = new DiscreteStringKeyFrame
                {
                    Value = "Загрузка..",
                    KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 1))
                };

                var kf4 = new DiscreteStringKeyFrame
                {
                    Value = "Загрузка...",
                    KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 1, 500))
                };

                stranim.KeyFrames.Add(kf1);
                stranim.KeyFrames.Add(kf2);
                stranim.KeyFrames.Add(kf3);
                stranim.KeyFrames.Add(kf4);

                stackPanel.Children.Add(textBlock);

                textBlock.BeginAnimation(TextBlock.TextProperty, stranim);

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

        }

        public void HideWaitAnnimation()
        {
            LoadingAnimationGrid.Visibility = Visibility.Collapsed;
            LoadingAnimationBorder.Child = null;
        }

        private HostVisual CreateAnnimationOnWorkerThread()
        {
            var hostVisual = new HostVisual();

            var thread = new Thread(AnnimationWorkerThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start(hostVisual);

            SEvent.WaitOne();

            return hostVisual;
        }

        private void AnnimationWorkerThread(object arg)
        {
            var hostVisual = (HostVisual)arg;
            var visualTargetPs = new VisualTargetPresentationSource(hostVisual);
            SEvent.Set();

            visualTargetPs.RootVisual = CreateElement();

            System.Windows.Threading.Dispatcher.Run();
        }

        private FrameworkElement CreateElement()
        {
            //var fadingLine = new CircularFadingLine();
            var fadingLine = new CircularFadingLine
            {
                Width = 17,
                Height = 17,
                Background = Brushes.White,
                StrokeThickness = 5,
            };

            return fadingLine;

        }

        private static readonly AutoResetEvent SEvent = new AutoResetEvent(false);

        //public void GetWorkerStat()
        //{
        //    var bw = new BackgroundWorker { WorkerReportsProgress = true };

        //    DataRow statDataRow = null;
        //    DataRow plannedClockRateDataRow = null;

        //    FillWorkerStat();

        //    bw.DoWork += (obj, ea) => Dispatcher.BeginInvoke(new ThreadStart(delegate
        //    {
        //        statDataRow = _ttc.GetWorkerStatForCurrentYearAndMonth(AdministrationClass.CurrentWorkerId);
        //        plannedClockRateDataRow = _ttc.GetPlannedClockRateForCurrentYearAndMonth();

        //    }));

        //    bw.RunWorkerCompleted += (obj, ea) => Dispatcher.BeginInvoke(new ThreadStart(delegate
        //    {
        //        FillWorkerStat(statDataRow);

        //        if (plannedClockRateDataRow != null)
        //            PlannedClockRateLabel.Content = "Норма часов: " + plannedClockRateDataRow["Standart40Time"];
        //        else
        //            PlannedClockRateLabel.Content = "Норма часов: --";
        //    }));

        //    bw.RunWorkerAsync();
        //}
        private void FileStorageButton_Click(object sender, RoutedEventArgs e)
        {
            var fileExplorer = new FileExplorer((int) AdministrationClass.CurrentModuleId, !_workshopMode);

            ShowToolsGrid(fileExplorer, "Файлы");
            HideWaitAnnimation();
            DispatcherHelper.DoEvents();
        }

        private void MyWorkersButton_Click(object sender, RoutedEventArgs e)
        {
            var myWorkers = new MyWorkersPage();
            ShowToolsGrid(myWorkers, "Мои работники");
        }

        private void ProductionCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            var prodCalendarPage = new ProductionCalendarPage();
            ShowToolsGrid(prodCalendarPage, "Производственный календарь");

        }

        private void HideToolsGridButton_Click(object sender, RoutedEventArgs e)
        {
            HideToolsGrid();
        }

        private void ChangeUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.HasContent)
            {
                if (!(MainFrame.Content is NewsFeed))
                {
                    _newsFeed = new NewsFeed();
                    MainFrame.Navigate(_newsFeed);
                }
            }
            MenuListBox.SelectedItems.Clear();

            AdministrationClass.CloseModuleEntry();
            AdministrationClass.CloseProgramEntry();

            UserGrid.Visibility = Visibility.Collapsed;
            MinimazeMenu();
            _newsFeed.ShowNews();

            WorkerLoginGrid.Visibility = Visibility.Visible;

            NotificationManager.StopUpdate();
        }

        private void WorkersNamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersNamesListBox.SelectedItem == null)
            {
                CurrentUserLabel.Text = string.Empty;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.None;
                bitmapImage.UriSource = new Uri("pack://application:,,,/Resources/user.png", UriKind.Absolute);
                bitmapImage.EndInit();
                WorkerImage.Source = bitmapImage;
                return;
            }

            var workerId = Convert.ToInt32(WorkersNamesListBox.SelectedValue);
            CurrentUserLabel.Text = _sc.GetWorkerName(workerId, true);

            var photo = _sc.GetObjectPhotoFromDataBase(workerId);

            var resultImage = new BitmapImage();
            if (photo != DBNull.Value)
            {
                resultImage = AdministrationClass.ObjectToBitmapImage(photo);
            }
            else
            {
                resultImage.BeginInit();
                resultImage.CacheOption = BitmapCacheOption.None;
                resultImage.UriSource = new Uri("pack://application:,,,/Resources/user.png", UriKind.Absolute);
                resultImage.EndInit();
            }

            WorkerImage.Source = resultImage;
            WorkerPasswordBox.IsEnabled = true;

            WorkerPasswordBox.Password = string.Empty;
            WorkerPasswordBox.Focus();
        }


        private void WorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersGroupsComboBox.Items.Count == 0) return;

            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.SelectedIndex = 0;
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;

            FilterWorkers();
        }

        private void FactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWorkers();
        }

        private void FilterWorkers()
        {
            if (WorkersGroupsComboBox.Items.Count == 0) return;
            if (FactoriesComboBox.Items.Count == 0) return;

            var staffDataTable = _sc.FilterWorkers(Convert.ToInt32(WorkersGroupsComboBox.SelectedValue),
                Convert.ToInt32(FactoriesComboBox.SelectedValue));
            BindingListCollectionView cv = null;

            if (staffDataTable != null)
            {
                cv = new BindingListCollectionView(staffDataTable.DefaultView);

                if (cv.GroupDescriptions != null)
                {
                    cv.GroupDescriptions.Add(new PropertyGroupDescription("Name", new FirstLetterConverter()));
                    cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
            }

            WorkersNamesListBox.ItemsSource = cv;
            WorkersNamesListBox.SelectedIndex = 0;
            WorkersNamesListBox_SelectionChanged(null, null);
        }

        private void WorkerPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterButton_Click(null, null);
            }
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkersNamesListBox.SelectedItems.Count == 0) return;

            if (WorkerPasswordBox.Password == string.Empty)
            {
                MessageBox.Show("Необходимо ввести пароль", "Информация", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                WorkerPasswordBox.Focus();
                return;
            }

            //if (!_sc.CheckPassword(WorkerPasswordBox.Password, Convert.ToInt32(WorkersNamesListBox.SelectedValue)))
            //{
            //    MessageBox.Show("Неверный пароль!", "Информация", MessageBoxButton.OK,
            //        MessageBoxImage.Information);
            //    WorkerPasswordBox.Password = string.Empty;
            //    WorkerPasswordBox.Focus();
            //    return;
            //}


            //LoadPageForWorker(Convert.ToInt32(WorkersNamesListBox.SelectedValue));

            AdministrationClass.OpenNewProgramEntry(Convert.ToInt32(WorkersNamesListBox.SelectedValue));
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.NewsFeed);

            MenuListBox.ItemsSource = AdministrationClass.GetAvailableModulesForWorker().DefaultView;
            AdministrationClass.GetFavoritesModulesIdsForWorker();


            MenuGroupsListBox.SelectedIndex = 0;
            MenuGroupsListBox_SelectionChanged(null, null);

            LoadPersonalInformation();

            SetDefaultViewWorkingDayGrid();
            CalculateWorkerTime();

            var lastExit = AdministrationClass.LastModuleExit(AdministrationClass.Modules.NewsFeed);
            _newsFeed.ShowNews(lastExit);
            NotificationManager.ShowNotifications(AdministrationClass.CurrentWorkerId);

            WorkerLoginGrid.Visibility = Visibility.Collapsed;
            UserGrid.Visibility = Visibility.Visible;

            if (MenuPopupBorder.Child != null)
            {
                MenuPopupBorder.Child = null;
                MainGrid.Children.Add(MenuGrid);
            }

            MenuListBox.SelectedItems.Clear();


            WorkersNamesListBox.SelectedIndex = 0;
            WorkerPasswordBox.Password = string.Empty;


            ClearModules();
            //HideLoginGrid();
        }

        private void ClearModules()
        {
            _cp = null;
            _ttp = null;
            _administrationPage = null;
            _timeControlPage = null;
            _serviceEquipmentPage = null;
            _taskPage = null;
            _technologyProblemPage = null;
            _stimulationPage = null;
            _prodRoomsPage = null;
            _workshopMapPage = null;
            _staffPage = null;
            _workerRequestsPage = null;
            _plannedWorksPage = null;


            //while (MainFrame.NavigationService.b)
            //{
            //    MainFrame.NavigationService.RemoveBackEntry();
            //}

            while (MainFrame.CanGoBack)
            {
                MainFrame.RemoveBackEntry();
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainFrame.CanGoBack) return;

            MainFrame.GoBack();
            MinimazeMenu();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainFrame.CanGoForward) return;

            MainFrame.GoForward();
            MinimazeMenu();
        }

        private void MainFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            string moduleName = _idToModuleNameConverter.Convert(((Page) e.Content).Tag).ToString();

            Title = moduleName == string.Empty ? "FA2" : "FA2." + moduleName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var chnPassPage = new ChangePasswordPage();
            ShowToolsGrid(chnPassPage, "Сменить пароль");
        }
    }
}
