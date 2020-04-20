using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FA2.XamlFiles;

namespace FA2.Classes
{
    static class Splasher
    {
        private static Dispatcher _dispatcher;
        private static SplashWindow _splashWindow;
        
        public static void ShowSplashWindow(string statusText, string mode)
        {
            var thread = new Thread(new ThreadStart(
                delegate
                {
                    _dispatcher = Dispatcher.CurrentDispatcher;

                    _dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            _splashWindow = new SplashWindow(mode) { StatusText = statusText };
                            _splashWindow.Show();
                        }));

                    Dispatcher.Run();
                }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            //_splashWindow = new SplashWindow(mode) { StatusText = statusText };
            //_splashWindow.Show();
        }

        public static void SetStatusText(string statusText)
        {
            if(_dispatcher == null) return;

            _dispatcher.BeginInvoke(new Action(
                delegate
                {
                    if (_splashWindow == null) return;
                    _splashWindow.StatusText = statusText;
                }));
        }

        public static void CloseSplashWindow()
        {
            if(_dispatcher == null) return;

            _dispatcher.BeginInvoke(new Action(
                delegate
                {
                    if (_splashWindow == null) return;
                    _splashWindow.Close();
                }));

            Application.Current.MainWindow.Activate();
            _dispatcher.InvokeShutdown();
        }
    }
}
