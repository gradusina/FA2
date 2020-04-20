using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FA2.Classes;
using FA2.Converters;
using FAIIControlLibrary.UserControls;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow
    {
        private Storyboard _sb;

        private StaffClass _sc;

        private static string _startMode;

        private Thread _photoLoaderThread;



        public SplashWindow(string mode)
        {
            InitializeComponent();
            DotTextBlock.Text = string.Empty;

            _startMode = mode;

            switch (_startMode)
            {
                case "-w":
                    HideLoginGrid();

                    App.SW();
                    break;

                case "-m":

                    ShowLoginGrid();

                    GetClasses();
                    SetBindings();

                    break;
            }
        }

        private void HideLoginGrid(bool isAnimation = false)
        {
            if (!isAnimation)
            {
                TitleTextBlock.Visibility = Visibility.Visible;
                StatusTextBlock.Visibility = Visibility.Visible;
                DotTextBlock.Visibility = Visibility.Visible;
                ShowInTaskbar = false;

                LoginGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                var da = new DoubleAnimation {From = 1, To = 1, Duration = new Duration(TimeSpan.FromSeconds(0))};

                da.Completed += (s, e) =>
                {
                    HideLoginGrid();
                    _sb = FindResource("OnLoadedStoryboard") as Storyboard;
                    if (_sb != null) _sb.Begin();
                };
                LoginGrid.BeginAnimation(OpacityProperty, da);
            }
        }

        private void ShowLoginGrid()
        {
            TitleTextBlock.Visibility = Visibility.Hidden;
            StatusTextBlock.Visibility = Visibility.Hidden;
            DotTextBlock.Visibility = Visibility.Hidden;
            ShowInTaskbar = true;

            LoginGrid.Visibility = Visibility.Visible;
        }

        private void GetClasses()
        {
            if (_sc != null) return;
            if (!App.BaseClass.GetStaffClass(ref _sc)) Application.Current.Shutdown();
        }

        private void SetBindings()
        {
            if (_sc == null) return;

            var workersCollectionView =
                new BindingListCollectionView(_sc.FilterWorkers(true, 1, false, 0, false, 0).AsDataView())
                {
                    CustomFilter = "AvailableInList = 'True'"
                };

            if (workersCollectionView.GroupDescriptions != null)
            {
                workersCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Name",
                    new FirstLetterConverter()));
                workersCollectionView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                UsersComboBox.SelectionChanged -= UsersComboBox_SelectionChanged;
                UsersComboBox.ItemsSource = workersCollectionView;
                UsersComboBox.SelectedItem = null;
                UsersComboBox.SelectionChanged += UsersComboBox_SelectionChanged;
            }
        }

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof (string), typeof (SplashWindow),
                new PropertyMetadata(string.Empty));

        public string StatusText
        {
            get { return (string) GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (_startMode)
            {
                case "-w":
                    _sb = FindResource("OnLoadedStoryboard") as Storyboard;
                    if (_sb != null) _sb.Begin();
                    break;
                case "-m":
                    break;
            }

            int wId = AdministrationClass.GetWorkerIdByComputerName();

            if (wId != 0)
            {
                UsersComboBox.SelectedValue = wId;

                if (UsersComboBox.SelectedIndex != -1)
                {
                    PasswordBox.Focus();
                    UsersComboBox_SelectionChanged(null, null);
                }
            }
            else
            {
                UsersComboBox.SelectedIndex = -1;
                UsersComboBox.Focus();
            }

            try
            {
                const int mainversion = 1;
                int dayversion = 1;

                string ver = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTimeUtc.ToString("yyMMdd");

                string filePath = Path.Combine(AdministrationClass.GetMainDirectory(), "ver");

                var versionFile = new FileInfo(filePath);


                if (!Directory.Exists(AdministrationClass.GetMainDirectory()))
                    Directory.CreateDirectory(AdministrationClass.GetMainDirectory());

                if (!versionFile.Exists)
                {
                    StreamWriter sw = versionFile.CreateText();
                    sw.Close();
                }

                string version = File.ReadAllText(filePath).Trim();

                if (version != string.Empty)
                {
                    string curVer = version.Remove(0, 2);
                    try
                    {
                        curVer = curVer.Remove(6, curVer.Length - 6);
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }

                    if (curVer == ver)
                    {
                        int.TryParse(version.Remove(0, version.Length - 1), out dayversion);

                        if (dayversion == 0)
                            dayversion = 1;
                    }
                }

                VersionLabel.Content = mainversion + "." + ver + dayversion;

                File.WriteAllText(filePath, VersionLabel.Content.ToString());

            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось создать файл версии", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox_PasswordSubmited(null, null);
            }
        }

        private void PasswordBox_PasswordSubmited(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewProgramEntry(Convert.ToInt32(UsersComboBox.SelectedValue));

            HideLoginGrid(true);
            App.SW();

            //if (UsersComboBox.SelectedValue == null)
            //{
            //    UsersComboBox.Text = string.Empty;
            //    PasswordBox.Password = string.Empty;
            //    return;
            //}

            //if (!string.IsNullOrEmpty(PasswordBox.Password))
            //{
            //    if (_sc.CheckPassword(PasswordBox.Password, Convert.ToInt32(UsersComboBox.SelectedValue)))
            //    {
            //        AdministrationClass.OpenNewProgramEntry(Convert.ToInt32(UsersComboBox.SelectedValue));

            //        HideLoginGrid(true);
            //        App.SW();
            //    }
            //    else
            //    {
            //        PasswordBox.EmphasisText = "Неверный пароль";
            //        PasswordBox.IsEmphasized = true;
            //    }
            //}
            //else
            //{
            //    PasswordBox.EmphasisText = "Введите пароль";
            //}
        }

        private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_photoLoaderThread != null && _photoLoaderThread.IsAlive)
            {
                _photoLoaderThread.Abort();
                _photoLoaderThread.Join();
            }

            if (UsersComboBox.SelectedValue == null)
            {
                HideAnimation();
                UserImage.Visibility = Visibility.Visible;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.None;
                bitmapImage.UriSource = new Uri("pack://application:,,,/Resources/nophoto.jpg", UriKind.Absolute);
                bitmapImage.EndInit();
                UserImage.Source = bitmapImage;
                return;
            }

            var workerId = Convert.ToInt32(UsersComboBox.SelectedValue);
            UserImage.Visibility = Visibility.Hidden;
            ShowAnimation();

            _photoLoaderThread = new Thread(() =>
            {
                var photo = _sc.GetObjectPhotoFromDataBase(workerId);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(() =>
                {
                    if (photo != DBNull.Value)
                    {
                        var resultImage = AdministrationClass.ObjectToBitmapImage(photo);
                        UserImage.Source = resultImage;
                    }
                    else
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.None;
                        bitmapImage.UriSource = new Uri("pack://application:,,,/Resources/nophoto.jpg", UriKind.Absolute);
                        bitmapImage.EndInit();
                        UserImage.Source = bitmapImage;
                    }

                    HideAnimation();
                    UserImage.Visibility = Visibility.Visible;
                }));
            });
            _photoLoaderThread.SetApartmentState(ApartmentState.STA);
            _photoLoaderThread.IsBackground = true;
            _photoLoaderThread.Start();
        }

        private void ShowAnimation()
        {
            if (AnimationGrid.Children.Count != 0) return;

            var animation = new CircularFadingLine
            {
                Height = 20,
                Width = 20,
                StrokeThickness = 2
            };
            AnimationGrid.Children.Add(animation);
        }

        private void HideAnimation()
        {
            AnimationGrid.Children.Clear();
        }

        private void MinimazeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void OnSplashWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
