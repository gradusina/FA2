using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FA2.Classes
{
    internal class WaitWindow
    {

        #region local variables

        private bool _movedToCorner;
        private bool _oddAnimation;
        private int _annimationCount;

        private Window _window;
        private Thread _thr;
        private TextBlock _textBlock;
        private ProgressBar _pBar;
        private Grid _grid;


        public Window Owner { set; get; }

        private Window Window
        {
            set
            {
                _window = value;
                _window.ResizeMode = ResizeMode.NoResize;
                _window.ShowInTaskbar = false;
                _window.AllowsTransparency = true;
                _window.WindowStyle = WindowStyle.None;
                //_window.Background = Brushes.WhiteSmoke;
                _window.Background = Brushes.Gray;
                _window.BorderThickness = new Thickness(2);
                //_window.BorderBrush = new BrushConverter().ConvertFrom("#FF017BCD") as Brush;
                _window.BorderBrush = Brushes.LightGray;
                _window.Width = 250;
                _window.SizeToContent = SizeToContent.Height;
                _window.MinHeight = 65;
                _window.WindowState = WindowState.Normal;
                _window.Topmost = true;
                //_window.SizeToContent = SizeToContent.Height;
                _window.ShowActivated = false;
            }
            get { return _window; }
        }

        private TextBlock TextBlock
        {
            set
            {
                _textBlock = value;
                _textBlock.Text = "Выполнение";
                //_textBlock.Foreground = new BrushConverter().ConvertFrom("#FF444444") as Brush;
                _textBlock.Foreground = Brushes.White;
                _textBlock.FontSize = 16;
                _textBlock.Margin = new Thickness(5);
                _textBlock.TextWrapping = TextWrapping.Wrap;
                _textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _textBlock.VerticalAlignment = VerticalAlignment.Center;
                _textBlock.SetValue(Grid.RowProperty, 0);
            }
            get { return _textBlock; }
        }

        private ProgressBar ProgressBar
        {
            set
            {
                _pBar = value;
                _pBar.Height = 8;
                _pBar.Visibility = Visibility.Visible;
                _pBar.SetValue(Grid.RowProperty, 1);
                _pBar.Margin = new Thickness(8,5,8,8);
                _pBar.Background = Brushes.White;
                _pBar.VerticalAlignment = VerticalAlignment.Bottom;
            }
            get { return _pBar; }
        }

        private Grid Grid
        {
            set
            {
                _grid = value;
                _grid.RowDefinitions.Add(new RowDefinition());
                _grid.RowDefinitions.Add(new RowDefinition());
                _grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
            }
            get { return _grid; }
        }

        #endregion


        public string Text
        {
            set
            {
                // Make small wait before initializing TextBlock
                //Thread.Sleep(20);
                if (_textBlock != null)
                    _textBlock.Dispatcher.BeginInvoke(new ThreadStart(() => _textBlock.Text = value));
            }
        }

        public double Progress
        {
            set
            {
                //Thread.Sleep(30);
                if (_pBar != null)
                    _pBar.Dispatcher.BeginInvoke(new ThreadStart(() => _pBar.Value = value));
            }
        }

        public void Show(Window owner, bool moveToConer = false)
        {
            if (_window != null)
                _window.Dispatcher.BeginInvoke(
                    new ThreadStart(() => _window.Close()));

            // take parametrs from Owner
            Owner = owner;
            double left = Owner.Left;
            double top = Owner.Top;
            if (Owner.WindowState == WindowState.Maximized)
            {
                left = 0;
                top = 0;
            }
            double width = Owner.ActualWidth;
            double height = Owner.ActualHeight;
            _movedToCorner = false;

            // creating window with content in the new thread
            _thr = new Thread(() =>
                                  {
                                      // creating TextBlock
                                      TextBlock = new TextBlock();

                                      // creating Window and Grid
                                      Window = new Window();
                                      if(!moveToConer)
                                      {
                                          Window.Top = top + height / 2;
                                          Window.Left = left + width / 2 - Window.Width / 2;
                                      }
                                      else
                                      {
                                          Window.Top = 10;
                                          Window.Left = 10;
                                      }
                                      
                                      Grid = new Grid();
                                      Window.Content = Grid;

                                      Grid.Children.Add(TextBlock);

                                      // creating ProgressBar
                                      ProgressBar = new ProgressBar();
                                      Grid.Children.Add(ProgressBar);

                                      Window.ShowDialog();
                                  });
            _thr.SetApartmentState(ApartmentState.STA);
            _thr.IsBackground = true;
            _thr.Start();

            Owner.SizeChanged += OnOwnerSizeChanged;
            Owner.Deactivated += OnOwnerDeactivated;
            Owner.LocationChanged += OnOwnerLocationChanged;
        }

        private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveToCorner();
            DisposeOwnerEvents();
        }

        private void OnOwnerDeactivated(object sender, EventArgs e)
        {
            MoveToCorner();
            DisposeOwnerEvents();
        }

        private void OnOwnerLocationChanged(object sender, EventArgs e)
        {
            MoveToCorner();
            DisposeOwnerEvents();
        }

        public void Close(bool totally = false)
        {
            //closing window
            if (_window != null)
                if (!_movedToCorner || totally)
                {
                    _window.Dispatcher.BeginInvoke(new ThreadStart(() => _window.Close()));
                    DisposeOwnerEvents();
                    Thread.Sleep(500);
                    _thr.Abort();
                    _thr.Join();
                }
                else
                {
                    _window.Dispatcher.BeginInvoke(new ThreadStart(() =>
                                                                       {
                                                                           _grid.RowDefinitions[1].Height = new GridLength(0); //#FF71BBED
                                                                           _window.MouseLeftButtonDown +=
                                                                               OneLeftMouseDown;

                                                                           _oddAnimation = true;
                                                                           _annimationCount = 0;
                                                                           _window.BorderThickness = new Thickness(2);
                                                                           //var ca = NewColorAnimation("#FF017BCD",
                                                                           //                           "#FFE5BB13");
                                                                           //ca.Completed += OnBusyAnimationCompleted;
                                                                           //if (!_window.HasAnimatedProperties)
                                                                           //    _window.BorderBrush.BeginAnimation(
                                                                           //        SolidColorBrush.ColorProperty, ca, HandoffBehavior.SnapshotAndReplace);
                                                                       }));
                }
        }

        private void OnBusyAnimationCompleted(object sender, EventArgs e)
        {
            _window.Dispatcher.BeginInvoke(new ThreadStart(() =>
                                                               {
                                                                   if (_annimationCount < 5)
                                                                   {
                                                                       ColorAnimation ca;
                                                                       if (_oddAnimation)
                                                                       {
                                                                           ca = NewColorAnimation("#FFE5BB13", "#FF017BCD");
                                                                           _oddAnimation = false;
                                                                       }
                                                                       else
                                                                       {
                                                                           ca = NewColorAnimation("#FF017BCD", "#FFE5BB13");
                                                                           _oddAnimation = true;
                                                                       }
                                                                       ca.Completed += OnBusyAnimationCompleted;
                                                                       if (!_window.HasAnimatedProperties)
                                                                           _window.BorderBrush.BeginAnimation(
                                                                               SolidColorBrush.ColorProperty, ca, HandoffBehavior.SnapshotAndReplace);
                                                                       _annimationCount++;
                                                                   }
                                                                   else
                                                                   {
                                                                       _window.BorderThickness = new Thickness(1);
                                                                   }
                                                               }));
        }

        public void MoveToCorner()
        {
            // moving window to the left-top corner
            if (_window != null)
                _window.Dispatcher.BeginInvoke(new ThreadStart(delegate
                                                                   {
                                                                       _window.Top = 10;
                                                                       _window.Left = 10;
                                                                       _movedToCorner = true;

                                                                       //var ca = NewColorAnimation("#FFD3D3D3", "#FFCCCCFF");//("#FF017BCD",
                                                                       ////"#FF0FA861");
                                                                       //ca.Completed += OnSingleAnimationCompleted;
                                                                       //_window.BorderBrush = new SolidColorBrush();
                                                                       //if (!_window.HasAnimatedProperties)
                                                                       //    _window.BorderBrush.BeginAnimation(
                                                                       //        SolidColorBrush.ColorProperty, ca, HandoffBehavior.SnapshotAndReplace);
                                                                   }));
        }

        private void OnSingleAnimationCompleted(object sender, EventArgs e)
        {
            _window.Dispatcher.BeginInvoke(new ThreadStart(() =>
                                                               {
                                                                   var ca = NewColorAnimation("#FFCCCCFF", "#FFD3D3D3");
                                                                   _window.BorderBrush = new SolidColorBrush();
                                                                   if (!_window.HasAnimatedProperties)
                                                                       _window.BorderBrush.BeginAnimation(
                                                                           SolidColorBrush.ColorProperty, ca, HandoffBehavior.SnapshotAndReplace);
                                                               }));
        }

        private void OneLeftMouseDown(object sender, RoutedEventArgs e)
        {
            _window.Dispatcher.BeginInvoke(new ThreadStart(() => _window.Close()));
            _thr.Abort();
            _thr.Join();
        }

        private ColorAnimation NewColorAnimation(object colorFrom, object colorTo)
        {
            var ca = new ColorAnimation
                         {
                             Duration =
                                 new Duration(
                                 TimeSpan.FromSeconds(1))
                         };
            var convertFrom =
                new ColorConverter().ConvertFrom(colorFrom);
            ca.From = convertFrom != null
                ? (Color)convertFrom
                : Colors.Transparent;

            var convertTo =
                new ColorConverter().ConvertFrom(colorTo);
            ca.To = convertTo != null
                ? (Color) convertTo
                : Colors.Transparent;

            return ca;
        }

        private void DisposeOwnerEvents()
        {
            Owner.SizeChanged -= OnOwnerSizeChanged;
            Owner.Deactivated -= OnOwnerDeactivated;
            Owner.LocationChanged -= OnOwnerLocationChanged;
        }
    }
}
