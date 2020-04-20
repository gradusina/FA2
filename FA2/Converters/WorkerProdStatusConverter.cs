using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FA2.Classes;
using System.Windows.Media.Imaging;
using FA2.XamlFiles;
using FA2.ChildPages.StaffPage;

namespace FA2.Converters
{
    class WorkerProdStatusConverter : IValueConverter
    {
        private ProductStatusColorConverter _prodStatusColorConverter;
        private StaffClass _sc;
        private AdmissionsClass _admClass;

        private DateTime? _currentDate;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            var stackPan = (VirtualizingStackPanel)value;

            var workerId = System.Convert.ToInt64(((DataRowView)stackPan.DataContext).Row["WorkerID"]);
            if (_sc == null)
                App.BaseClass.GetStaffClass(ref _sc);

            switch (parameter.ToString())
            {
                case "WorkerInfoPanel":
                    {
                        if (_admClass == null)
                            App.BaseClass.GetAdmissionsClass(ref _admClass);

                        if (_currentDate == null)
                            _currentDate = App.BaseClass.GetDateFromSqlServer();

                        var button = new Button();
                        button.Click -= OnInfoButtonClick;
                        button.Click += OnInfoButtonClick;

                        var image = new Image
                        {
                            SnapsToDevicePixels = true,
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Question.png")),
                            Stretch = Stretch.Uniform,
                            ToolTip = "Просмотреть информацию по работнику"
                        };

                        if (_admClass != null)
                        {
                            var hasWorkerEndedAdmissions = _admClass.HasWorkerEndedAdmissions(workerId, _currentDate.Value);
                            if (hasWorkerEndedAdmissions)
                            {
                                image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Alert.png"));
                                image.ToolTip = "Присутствует просроченный допуск";
                            }
                        }

                        button.Content = image;

                        stackPan.Children.Add(button);

                        return null;
                    }

                case "WorkerProdStatuses":
                    {
                        if (_prodStatusColorConverter == null)
                            _prodStatusColorConverter = new ProductStatusColorConverter();

                        if (_sc != null)
                        {
                            var custView = new DataView(_sc.WorkerProdStatusesDataTable, "", "WorkerID",
                                                        DataViewRowState.CurrentRows);

                            DataRowView[] foundRows = custView.FindRows(workerId);

                            if (foundRows.Count() != 0)
                            {
                                var sortedView = from foundRow in foundRows
                                                 orderby foundRow["ProdStatusID"] descending
                                                 select foundRow;
                                foreach (var dataRowView in sortedView)
                                {
                                    int prodStatusId = System.Convert.ToInt32(dataRowView.Row["ProdStatusID"]);
                                    var canvas = new Border
                                    {
                                        BorderThickness = new Thickness(0),
                                        SnapsToDevicePixels = true,
                                        CornerRadius = new CornerRadius(4),
                                        Width = 9,
                                        Height = 9,
                                        Margin = new Thickness(1),
                                        Background = (Brush)
                                            _prodStatusColorConverter.Convert(prodStatusId, typeof(Brush),
                                                "Color",
                                                CultureInfo.CurrentCulture),
                                        ToolTip =
                                            _prodStatusColorConverter.Convert(prodStatusId, typeof(Brush), "Name",
                                                CultureInfo.CurrentCulture)
                                    };
                                    stackPan.Children.Add(canvas);
                                }
                            }
                        }

                        return null;
                    }
            }

            return null;
        }

        private void OnInfoButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var worker = button.DataContext as DataRowView;
            if (worker == null) return;

            var workerId = System.Convert.ToInt64(worker["WorkerID"]);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if(mainWindow != null)
            {
                var hasFullAccess = AdministrationClass.HasFullAccess(AdministrationClass.Modules.Workers);
                var workerPersonalInfoPage = new WorkerPersonalInfoPage(workerId, hasFullAccess);
                mainWindow.ShowCatalogGrid(workerPersonalInfoPage, "Информация по работнику");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
