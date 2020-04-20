using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using FA2.Classes;
using FA2.Converters;
using FA2.XamlFiles;

namespace FA2.ChildPages.ServiceEquipmentPage
{
    /// <summary>
    /// Логика взаимодействия для ServiceEquipmentInfo.xaml
    /// </summary>
    public partial class ServiceEquipmentInfo
    {
        private readonly IdToNameConverter _workerNameConverter;
        private readonly Brush _foregroundBrush = (Brush)new BrushConverter().ConvertFrom("#FF444444");
        //private const string ReceivedText = "\n\nЗаявка принята: {0} дата: {1}.";
        private const string RequestClosedText = "\nЗаявка закрыта: {0} дата: {1}.";
        private ServiceEquipmentClass _sec;

        readonly DataRowView _rowView;
        readonly int _curWorkerId;
        private MainWindow _mw;

        public ServiceEquipmentInfo(DataRowView rowView, int currentWorkerId)
        {
            InitializeComponent();
            App.BaseClass.GetServiceEquipmentClass(ref _sec);
            _rowView = rowView;
            _curWorkerId = currentWorkerId;
            _workerNameConverter = new IdToNameConverter();

            DataContext = rowView;
            SetRowsHeight();
            FillBlank();
            scrollViewer.ScrollToEnd();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                _mw.HideCatalogGrid();
            }
        }

        private void SetRowsHeight()
        {
            if (_rowView != null)
            {
                if (_rowView.Row["ReceivedDate"] == DBNull.Value)
                    editingReceivedRow.Height = new GridLength(1, GridUnitType.Auto);
                else if (_rowView.Row["CompletionDate"] == DBNull.Value)
                {
                    receivedRow.Height = new GridLength(1, GridUnitType.Auto);
                    editingCompleteRow.Height = new GridLength(1, GridUnitType.Auto);
                }
                else if (_rowView.Row["LaunchDate"] == DBNull.Value)
                {
                    receivedRow.Height = new GridLength(1, GridUnitType.Auto);
                    compltetRow.Height = new GridLength(1, GridUnitType.Auto);
                    editingLaunchRow.Height = new GridLength(1, GridUnitType.Auto);
                }
                else if (_rowView.Row["LaunchDate"] != DBNull.Value)
                {
                    receivedRow.Height = new GridLength(1, GridUnitType.Auto);
                    compltetRow.Height = new GridLength(1, GridUnitType.Auto);
                    launchRow.Height = new GridLength(1, GridUnitType.Auto);
                }

                if (_rowView.Row["CrashReason"] != DBNull.Value)
                {
                    crashReasonRow.Height = new GridLength(1, GridUnitType.Auto);
                    crashReasonText.Text = _rowView.Row["CrashReason"].ToString();
                }
            }
        }

        private void FillBlank()
        {
            FillRequestInfo(_rowView.Row["RequestWorkerID"]);

            if (_rowView.Row["ReceivedDate"] != DBNull.Value)
            {
                FillReceivedInfo(_rowView.Row["ReceivedWorkerID"], _rowView.Row["ReceivedNotes"]);
            }

            if (_rowView.Row["CompletionDate"] != DBNull.Value)
            {
                FillCompletionInfo(_rowView.Row["CompletionWorkerID"], _rowView.Row["CompletionNotes"]);
            }

            if (_rowView.Row["LaunchDate"] != DBNull.Value)
            {
                FillLaunchInfo(_rowView.Row["LaunchWorkerID"], _rowView.Row["LaunchNotes"]);
                cancelButton.Width = 0;
                cancelButton.IsEnabled = false;
            }
            else
            {
                launchEditNoteTextBox.Text = string.Empty;
                cancelButton.Width = 100;
            }
        }

        private void FillRequestInfo(object requestWorkerID)
        {
            requestWorkerInfoLabel.Content = _workerNameConverter.Convert(requestWorkerID, "FullName");
        }

        private void FillReceivedInfo(object receivedWorkerId, object receivedNotes)
        {
            receivedWorkerInfoLabel.Content = _workerNameConverter.Convert(receivedWorkerId, "FullName");
            if (receivedNotes != DBNull.Value)
            {
                receivedNoteTextBox.Text = receivedNotes.ToString();
                receivedNoteTextBox.Foreground = _foregroundBrush;
            }
            else
            {
                receivedNoteTextBox.Text = "Нет записей";
                receivedNoteTextBox.Foreground = Brushes.Gray;
            }
        }

        private void FillCompletionInfo(object completionWorkerId, object completionNotes)
        {
            completeWorkerInfoLabel.Content = _workerNameConverter.Convert(completionWorkerId, "FullName");
            if (completionNotes != DBNull.Value)
            {
                completNoteTextBox.Text = completionNotes.ToString();
                completNoteTextBox.Foreground = _foregroundBrush;
            }
            else
            {
                completNoteTextBox.Text = "Нет записей";
                completNoteTextBox.Foreground = Brushes.Gray;
            }
        }

        private void FillLaunchInfo(object launchWorkerId, object launchNotes)
        {
            launchWorkerInfoLabel.Content = _workerNameConverter.Convert(launchWorkerId, "FullName");
            if (launchNotes != DBNull.Value)
            {
                launchNoteTextBox.Text = launchNotes.ToString();
                launchNoteTextBox.Foreground = _foregroundBrush;
            }
            else
            {
                launchNoteTextBox.Text = "Нет записей";
                launchNoteTextBox.Foreground = Brushes.Gray;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var dr = _sec.Table.Select("CrashMachineID = " + _rowView.Row["CrashMachineID"]);
            if (dr.Length == 0) return;

            var dataRow = dr[0];
            //if (dataRow["ReceivedDate"] == DBNull.Value)
            //    AddReceivedInfo(dataRow);
            //else if (dataRow["CompletionDate"] == DBNull.Value)
            //    AddCompletionInfo(dataRow);
            if (dataRow["LaunchDate"] == DBNull.Value)
                AddLaunchInfo(dataRow);

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;
            if(_mw != null)
            {
                var servEquipPage = _mw.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    servEquipPage.RefillInfo();
                }
            }

            cancelButton_Click(null, null);
        }

        //private void AddReceivedInfo(DataRow dataRow)
        //{
        //    int crashMachineId = Convert.ToInt32(dataRow["CrashMachineID"]);
        //    DateTime receivedDate = App.BaseClass.GetDateFromSqlServer();
        //    string receivedNote = null;
        //    if (!string.IsNullOrEmpty(receivedEditNoteTextBox.Text))
        //        receivedNote = receivedEditNoteTextBox.Text;
        //    _sec.FillReceivedInfo(crashMachineId, receivedDate, _curWorkerId, receivedNote);

        //    var requestDate = Convert.ToDateTime(dataRow["RequestDate"]);
        //    var requestWorkerId = Convert.ToInt32(dataRow["RequestWorkerID"]);
        //    string workerName = string.Empty;
        //    workerName =
        //        _workerNameConverter.Convert(_curWorkerId, typeof (string), "ShortName", new CultureInfo("ru-RU"))
        //            .ToString();
        //    var newsText = string.Format(ReceivedText, workerName, receivedDate);

        //    NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);
        //}

        //private void AddCompletionInfo(DataRow dataRow)
        //{
        //    int crashMachineId = Convert.ToInt32(dataRow["CrashMachineID"]);
        //    DateTime completionDate = App.BaseClass.GetDateFromSqlServer();
        //    string completionNote = null;
        //    if (!string.IsNullOrEmpty(completEditNoteTextBox.Text))
        //        completionNote = completEditNoteTextBox.Text;
        //    _sec.FillCompletionInfo(crashMachineId, completionDate, _curWorkerId, completionNote);

        //    var requestDate = Convert.ToDateTime(dataRow["RequestDate"]);
        //    var requestWorkerId = Convert.ToInt32(dataRow["RequestWorkerID"]);
        //    string workerName = string.Empty;
        //    workerName = _workerNameConverter.Convert(_curWorkerId, typeof(string), "ShortName", new CultureInfo("ru-RU")).ToString();
        //    var newsText = string.Format(RequestClosedText, workerName, completionDate);

        //    NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);
        //}

        private void AddLaunchInfo(DataRow dataRow)
        {
            var crashMachineId = Convert.ToInt32(dataRow["CrashMachineID"]);
            var launchDate = App.BaseClass.GetDateFromSqlServer();
            string launchNote = null;
            if (!string.IsNullOrEmpty(launchEditNoteTextBox.Text))
                launchNote = launchEditNoteTextBox.Text;
            _sec.FillLaunchInfo(crashMachineId, launchDate, _curWorkerId, launchNote);
            AdministrationClass.AddNewAction(11);

            var requestDate = Convert.ToDateTime(dataRow["RequestDate"]);
            var requestWorkerId = Convert.ToInt32(dataRow["RequestWorkerID"]);
            var workerName =
                _workerNameConverter.Convert(_curWorkerId, typeof (string), "ShortName", new CultureInfo("ru-RU"))
                    .ToString();
            var newsText = string.Format(RequestClosedText, workerName, launchDate);
            NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);

            _mw = Window.GetWindow(this) as MainWindow;
            if (_mw != null)
            {
                var servEquipPage = _mw.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    servEquipPage.OpenPopup(dataRow["GlobalID"]);
                }
            }
        }
    }
}
