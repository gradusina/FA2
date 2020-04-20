using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FA2.Classes;
using FA2.Converters;
using FA2.XamlFiles;

namespace FA2.ChildPages.TechnologyProblemPage
{
    /// <summary>
    /// Логика взаимодействия для TechnologyProblemInfo.xaml
    /// </summary>
    public partial class TechnologyProblemInfo
    {
        private readonly TechnologyProblemClass _tpr;
        private readonly CatalogClass _cc;
        private readonly IdToNameConverter _workerNameConverter;
        private readonly Brush _foregroundBrush = (Brush) new BrushConverter().ConvertFrom("#FF444444");
        private readonly DataRowView _techProblemView;
        private readonly int _curWorkerId;
        private MainWindow _mw;

        private const string RequestText =
            "Заявка №02{0} \nТехнологическая проблема: {1} ({2}) \nПричина: {3}";
        private const string RequestClosedText = "\nЗаявка закрыта: {0} дата: {1}.";

        public TechnologyProblemInfo(DataRowView techProblemView, int curWorkerId)
        {
            InitializeComponent();
            App.BaseClass.GetTechnologyProblemClass(ref _tpr);
            _curWorkerId = curWorkerId;

            Height = 450;
            Width = 600;
            RequestInfoGrid.Visibility = Visibility.Visible;
            RequestInfoGrid.IsEnabled = true;
            _workerNameConverter = new IdToNameConverter();
            _techProblemView = techProblemView;
            DataContext = techProblemView;
            SetRowsHeight();
            FillBlank();
            ScrollViewer.ScrollToEnd();
        }

        public TechnologyProblemInfo(int curWorkerId)
        {
            InitializeComponent();
            App.BaseClass.GetTechnologyProblemClass(ref _tpr);
            _curWorkerId = curWorkerId;

            Height = 400;
            Width = 500;
            App.BaseClass.GetCatalogClass(ref _cc);
            AddRequestGrid.Visibility = Visibility.Visible;
            AddRequestGrid.IsEnabled = true;
            FillBindings();
            if (FactoryComboBox.Items.Count != 0)
                FactoryComboBox.SelectedIndex = 0;
        }


        #region RequestInfo

        private void SetRowsHeight()
        {
            EditingReceivedRow.Height = new GridLength(0);
            ReceivedRow.Height = new GridLength(0);
            EditingCompleteRow.Height = new GridLength(0);
            CompltetRow.Height = new GridLength(0);

            if (_techProblemView != null)
            {
                if (_techProblemView.Row["ReceivedDate"] == DBNull.Value)
                    EditingReceivedRow.Height = new GridLength(1, GridUnitType.Auto);
                else if (!Convert.ToBoolean(_techProblemView["RequestClose"]))
                {
                    ReceivedRow.Height = new GridLength(1, GridUnitType.Auto);
                    EditingCompleteRow.Height = new GridLength(1, GridUnitType.Auto);
                }
                else if (Convert.ToBoolean(_techProblemView["RequestClose"]))
                {
                    ReceivedRow.Height = new GridLength(1, GridUnitType.Auto);
                    CompltetRow.Height = new GridLength(1, GridUnitType.Auto);
                }
            }
        }

        private void FillBlank()
        {
            FillRequestInfo(_techProblemView.Row["RequestWorkerID"], _techProblemView.Row["RequestNotes"]);

            if (_techProblemView.Row["ReceivedDate"] != DBNull.Value)
            {
                FillReceivedInfo(_techProblemView.Row["ReceivedWorkerID"], _techProblemView.Row["ReceivedNotes"]);
            }
            else
                ReceivedEditNoteTextBox.Text = string.Empty;

            if (Convert.ToBoolean(_techProblemView["RequestClose"]))
            {
                FillCompletionInfo(_techProblemView.Row["CompletionWorkerID"], _techProblemView.Row["CompletionNotes"]);
                CancelButton.Width = 0;
            }
            else
            {
                CompletEditNoteTextBox.Text = string.Empty;
                CancelButton.Width = 100;
            }
        }

        private void FillRequestInfo(object requestWorkerId, object requestNotes)
        {
            RequestWorkerInfoLabel.Content = _workerNameConverter.Convert(requestWorkerId, "FullName");
            if (requestNotes != DBNull.Value)
            {
                RequestNoteInfoTextBox.Text = requestNotes.ToString();
            }
        }

        private void FillReceivedInfo(object receivedWorkerId, object receivedNotes)
        {
            ReceivedWorkerInfoLabel.Content = _workerNameConverter.Convert(receivedWorkerId, "FullName");
            if (receivedNotes != DBNull.Value)
            {
                ReceivedNoteTextBox.Text = receivedNotes.ToString();
                ReceivedNoteTextBox.Foreground = _foregroundBrush;
            }
            else
            {
                ReceivedNoteTextBox.Text = "Нет записей";
                ReceivedNoteTextBox.Foreground = Brushes.Gray;
            }
        }

        private void FillCompletionInfo(object completionWorkerId, object completionNotes)
        {
            CompleteWorkerInfoLabel.Content = _workerNameConverter.Convert(completionWorkerId, "FullName");
            if (completionNotes != DBNull.Value)
            {
                CompletNoteTextBox.Text = completionNotes.ToString();
                CompletNoteTextBox.Foreground = _foregroundBrush;
            }
            else
            {
                CompletNoteTextBox.Text = "Нет записей";
                CompletNoteTextBox.Foreground = Brushes.Gray;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                _mw.HideCatalogGrid();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var dr = _tpr.TechnologyProblemsTable.
                Select("TechnologyProblemID = " + _techProblemView.Row["TechnologyProblemID"]);
            if (dr.Length == 0) return;

            var dataRow = dr[0];
            if (!Convert.ToBoolean(dataRow["RequestClose"]))
            {
                AddCompletionInfo(dataRow);
            }

            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;
            if (_mw != null)
            {
                var techProbPage = _mw.MainFrame.Content as XamlFiles.TechnologyProblemPage;
                if (techProbPage != null)
                {
                    techProbPage.RefillInfo();
                }
            }
            CancelButton_Click(null, null);
        }

        private void AddCompletionInfo(DataRow dataRow)
        {
            var crashMachineId = Convert.ToInt32(dataRow["TechnologyProblemID"]);
            var completionDate = App.BaseClass.GetDateFromSqlServer();
            string completionNote = null;
            if (!string.IsNullOrEmpty(CompletEditNoteTextBox.Text))
                completionNote = CompletEditNoteTextBox.Text;
            _tpr.CompleteRequest(crashMachineId, completionDate, _curWorkerId, completionNote);
            AdministrationClass.AddNewAction(19);

            var requestDate = Convert.ToDateTime(dataRow["RequestDate"]);
            var requestWorkerId = Convert.ToInt32(dataRow["RequestWorkerID"]);
            var workerName =
                _workerNameConverter.Convert(_curWorkerId, typeof(string), "ShortName", new CultureInfo("ru-RU"))
                    .ToString();
            var newsText = string.Format(RequestClosedText, workerName, completionDate);
            NewsHelper.AddTextToNews(requestDate, requestWorkerId, newsText);


            _mw = Window.GetWindow(this) as MainWindow;
            if (_mw != null)
            {
                var techProbPage = _mw.MainFrame.Content as XamlFiles.TechnologyProblemPage;
                if (techProbPage != null)
                {
                    techProbPage.OpenPopup(dataRow["GlobalID"]);
                }
            }
        }

        #endregion


        #region AddRequest

        private void FillBindings()
        {
            FactoryComboBox.ItemsSource = _cc.GetFactories();
            FactoryComboBox.DisplayMemberPath = "FactoryName";
            FactoryComboBox.SelectedValuePath = "FactoryID";

            WorkUnitsComboBox.DisplayMemberPath = "WorkUnitName";
            WorkUnitsComboBox.SelectedValuePath = "WorkUnitID";

            WorkSectionsComboBox.DisplayMemberPath = "WorkSectionName";
            WorkSectionsComboBox.SelectedValuePath = "WorkSectionID";
        }

        private void FactoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoryComboBox.SelectedValue == null) return;

            WorkUnitsComboBox.ItemsSource = WorkUnitGroupFilter(2, Convert.ToInt32(FactoryComboBox.SelectedValue));
            if (WorkUnitsComboBox.Items.Count != 0)
                WorkUnitsComboBox.SelectedIndex = 0;
        }

        private void WorkUnitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsComboBox.SelectedValue == null) return;

            WorkSectionsComboBox.ItemsSource = WorkSectionUnitFilter(Convert.ToInt32(WorkUnitsComboBox.SelectedValue));
            if (WorkSectionsComboBox.Items.Count != 0)
                WorkSectionsComboBox.SelectedIndex = 0;
        }

        private void OkRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (FactoryComboBox.SelectedValue == null || WorkUnitsComboBox.SelectedValue == null ||
                WorkSectionsComboBox.SelectedValue == null || string.IsNullOrEmpty(RequestNoteTextBox.Text)) return;

            var factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
            var workUnitId = Convert.ToInt32(WorkUnitsComboBox.SelectedValue);
            var workSectionId = Convert.ToInt32(WorkSectionsComboBox.SelectedValue);
            var requestNote = RequestNoteTextBox.Text;
            var requestDate = App.BaseClass.GetDateFromSqlServer();

            var newId = _tpr.AddNewRequest(factoryId, workUnitId, workSectionId, requestDate,
                _curWorkerId, requestNote);
            AdministrationClass.AddNewAction(17);

            var newsText = string.Format(RequestText, newId.ToString("00000"),
                new IdToWorkSectionConverter().Convert(workSectionId, typeof (string), string.Empty,
                    new CultureInfo("ru-RU")),
                new IdToWorkUnitConverter().Convert(workUnitId, typeof (string), string.Empty, new CultureInfo("ru-RU")),
                requestNote);
            var newsStatus = factoryId == 1 ? 6 : 7;
            NewsHelper.AddNews(requestDate, newsText, newsStatus, _curWorkerId);

            _mw = Window.GetWindow(this) as MainWindow;
            if (_mw != null)
            {
                var techProbPage = _mw.MainFrame.Content as XamlFiles.TechnologyProblemPage;
                if (techProbPage != null)
                {
                    techProbPage.SelectNewTableRow(newId);
                }
            }

            CancelRequestButton_Click(null, null);
        }

        private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;

            if (_mw != null)
            {
                _mw.HideCatalogGrid();
            }
        }

        private DataView WorkUnitGroupFilter(int groupId, int factoryId)
        {
            var workUnitByGroup =
                (_cc.WorkUnitsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerGroupID") == groupId)
                    .Where(r => r.Field<Int64>("FactoryID") == factoryId));

            if (workUnitByGroup.Count() != 0)
            {
                var wuNamesDt = workUnitByGroup.CopyToDataTable();
                return wuNamesDt.DefaultView;
            }
            return null;
        }

        private DataView WorkSectionUnitFilter(int unitId)
        {
            var workSectionByUnit =
                (_cc.WorkSectionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkUnitID") == unitId));

            if (workSectionByUnit.Count() != 0)
            {
                var wSectionDt = workSectionByUnit.CopyToDataTable();
                return wSectionDt.DefaultView;
            }
            return null;
        }

        #endregion
    }
}
