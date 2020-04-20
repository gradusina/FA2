using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.Converters;
using FA2.XamlFiles;

namespace FA2.ChildPages.ServiceEquipmentPage
{
    /// <summary>
    /// Логика взаимодействия для AddServiceEquipment.xaml
    /// </summary>
    public partial class AddServiceEquipment
    {
        private ServiceEquipmentClass _sec;
        private CatalogClass _cc;
        private bool _unitToFactory;
        private readonly int _curWorkerId;
        private MainWindow _mw;

        private IdToWorkSubSectionConverter _workSubSectionConverter;
        private IdToWorkUnitConverter _workUnitConverter;
        private IdToWorkSectionConverter _workSectionConverter;

        private const string CrashRequestText = "Заявка №01{0} \nПоломка станка/приспособления: {1} ({2}/ {3}) \nПричина: {4}";
        private const string ProblemRequestText = "Заявка №01{0} \nЗамечание по оборудованию: {1} ({2}/ {3}) \nПричина: {4}";

        private readonly ServiceEquipmentClass.RequestType _requestType;
        private readonly bool _enteringLoad;
        private int _enteringWorkerGroupId;
        private int _enteringFactoryId;
        private int _enteringWorkUnitId;
        private int _enteringWorkSectionId;
        private readonly int _enteringWorkSubSectionId;

        public AddServiceEquipment(ServiceEquipmentClass.RequestType type)
        {
            InitializeComponent();
            
            _curWorkerId = AdministrationClass.CurrentWorkerId;
            _requestType = type;
        }

        public AddServiceEquipment(ServiceEquipmentClass.RequestType type, int workSubSectionId)
        {
            InitializeComponent();

            _curWorkerId = AdministrationClass.CurrentWorkerId;
            _requestType = type;
            _enteringLoad = true;
            _enteringWorkSubSectionId = workSubSectionId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.BaseClass.GetServiceEquipmentClass(ref _sec);
            App.BaseClass.GetCatalogClass(ref _cc);

            _workSubSectionConverter = new IdToWorkSubSectionConverter();
            _workUnitConverter = new IdToWorkUnitConverter();
            _workSectionConverter = new IdToWorkSectionConverter();

            BindingData();

            if(_enteringLoad)
            {
                RequestTypeComboBox.IsEnabled = false;
                GroupComboBox.IsEnabled = false;
                FactoryComboBox.IsEnabled = false;
                WorkUnitsComboBox.IsEnabled = false;
                WorkSectionsComboBox.IsEnabled = false;
                WorkSubSectionsComboBox.IsEnabled = false;
            }

            if (_requestType == ServiceEquipmentClass.RequestType.Crash)
            {
                if (RequestTypeComboBox.Items.Count != 0)
                    RequestTypeComboBox.SelectedIndex = 0;
            }
            else
            {
                if (RequestTypeComboBox.Items.Count > 1)
                    RequestTypeComboBox.SelectedIndex = 1;
                else if (RequestTypeComboBox.Items.Count != 0)
                    RequestTypeComboBox.SelectedIndex = 0;
            }
        }

        private void BindingData()
        {
            BindingGroupComboBox();
            BindingFactoryComboBox();
            BindingWorkUnitsComboBox();
            BindingWorkSectionsComboBox();
            BindingWorkSubSectionsComboBox();
            BindingRequestTypeComboBox();

            if (_enteringLoad)
            {
                _enteringWorkSectionId = Convert.ToInt32(_cc.WorkSubsectionsDataTable.AsEnumerable().
                    First(s => s.Field<Int64>("WorkSubsectionID") == _enteringWorkSubSectionId)["WorkSectionID"]);
                _enteringWorkUnitId = Convert.ToInt32(_cc.WorkSectionsDataTable.AsEnumerable().
                    First(s => s.Field<Int64>("WorkSectionID") == _enteringWorkSectionId)["WorkUnitID"]);
                _enteringWorkerGroupId = Convert.ToInt32(_cc.WorkUnitsDataTable.AsEnumerable().
                    First(u => u.Field<Int64>("WorkUnitID") == _enteringWorkUnitId)["WorkerGroupID"]);
                _enteringFactoryId = 1;
                _unitToFactory = Convert.ToBoolean(_cc.WorkerGroupsDataTable.AsEnumerable().
                    First(g => g.Field<Int64>("WorkerGroupID") == _enteringWorkerGroupId)["UnitToFactory"]);
                if (_unitToFactory)
                    _enteringFactoryId = Convert.ToInt32(_cc.WorkUnitsDataTable.AsEnumerable().
                        First(f => f.Field<Int64>("WorkUnitID") == _enteringWorkUnitId)["FactoryID"]);


                if (GroupComboBox.HasItems)
                    GroupComboBox.SelectedValue = _enteringWorkerGroupId;

                if (FactoryComboBox.HasItems)
                {
                    FactoryComboBox.SelectionChanged -= FactoryComboBox_SelectionChanged;
                    FactoryComboBox.SelectedValue = _enteringFactoryId;
                    FactoryComboBox.SelectionChanged += FactoryComboBox_SelectionChanged;
                    FactoryComboBox_SelectionChanged(null, null);
                }

                if (WorkUnitsComboBox.HasItems)
                {
                    WorkUnitsComboBox.SelectionChanged -= WorkUnitsComboBox_SelectionChanged;
                    WorkUnitsComboBox.SelectedValue = _enteringWorkUnitId;
                    WorkUnitsComboBox.SelectionChanged += WorkUnitsComboBox_SelectionChanged;
                    WorkUnitsComboBox_SelectionChanged(null, null);
                }

                if (WorkSectionsComboBox.HasItems)
                {
                    WorkSectionsComboBox.SelectionChanged -= WorkSectionsComboBox_SelectionChanged;
                    WorkSectionsComboBox.SelectedValue = _enteringWorkSectionId;
                    WorkSectionsComboBox.SelectionChanged += WorkSectionsComboBox_SelectionChanged;
                    WorkSectionsComboBox_SelectionChanged(null, null);
                }

                if(WorkSubSectionsComboBox.HasItems)
                {
                    WorkSubSectionsComboBox.SelectedValue = _enteringWorkSubSectionId;
                }
            }
            else
            {
                if (GroupComboBox.Items.Count > 1)
                    GroupComboBox.SelectedIndex = 1;
            }
        }

        private void BindingGroupComboBox()
        {
            GroupComboBox.ItemsSource = _cc.GetWorkersGroups();
            GroupComboBox.DisplayMemberPath = "WorkerGroupName";
            GroupComboBox.SelectedValuePath = "WorkerGroupID";
        }

        private void BindingFactoryComboBox()
        {
            FactoryComboBox.ItemsSource = _cc.GetFactories();
            FactoryComboBox.DisplayMemberPath = "FactoryName";
            FactoryComboBox.SelectedValuePath = "FactoryID";
        }

        private void BindingWorkUnitsComboBox()
        {
            WorkUnitsComboBox.DisplayMemberPath = "WorkUnitName";
            WorkUnitsComboBox.SelectedValuePath = "WorkUnitID";
        }

        private void BindingWorkSectionsComboBox()
        {
            WorkSectionsComboBox.DisplayMemberPath = "WorkSectionName";
            WorkSectionsComboBox.SelectedValuePath = "WorkSectionID";
        }

        private void BindingWorkSubSectionsComboBox()
        {
            WorkSubSectionsComboBox.DisplayMemberPath = "WorkSubsectionName";
            WorkSubSectionsComboBox.SelectedValuePath = "WorkSubsectionID";
        }

        private void BindingRequestTypeComboBox()
        {
            RequestTypeComboBox.ItemsSource = _sec.RequestTypes.Table.DefaultView;
            RequestTypeComboBox.DisplayMemberPath = "RequestTypeName";
            RequestTypeComboBox.SelectedValuePath = "RequestTypeID";
        }

        private DataView WorkUnitGroupFilter(int groupId)
        {
            var workUnitByGroup =
                    (_cc.WorkUnitsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("WorkerGroupID") == groupId));

            if (workUnitByGroup.Count() != 0)
            {
                var wuNamesDT = workUnitByGroup.CopyToDataTable();
                return wuNamesDT.DefaultView;
            }
            return null;
        }

        private DataView WorkUnitGroupFilter(int groupId, int factoryID)
        {
            var workUnitByGroup =
                (_cc.WorkUnitsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerGroupID") == groupId).Where(r => r.Field<Int64>("FactoryID") == factoryID));

            if (workUnitByGroup.Count() != 0)
            {
                var wuNamesDT = workUnitByGroup.CopyToDataTable();
                return wuNamesDT.DefaultView;
            }
            return null;
        }

        private DataView WorkSectionUnitFilter(int unitID)
        {
            var workSectionByUnit =
                (_cc.WorkSectionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkUnitID") == unitID));

            if (workSectionByUnit.Count() != 0)
            {
                var wSectionDT = workSectionByUnit.CopyToDataTable();
                return wSectionDT.DefaultView;
            }
            return null;
        }

        private DataView WorkSubSectionFilter(int workSectionID)
        {
            var workSubSectionsBySection =
                (_cc.WorkSubsectionsDataTable.AsEnumerable().Where(
                    r => r.Field<bool>("Visible") && r.Field<Int64>("WorkSectionID") == workSectionID));

            if (workSubSectionsBySection.Count() != 0)
            {
                var wSubSectionDT = workSubSectionsBySection.CopyToDataTable();
                return wSubSectionDT.DefaultView;
            }
            return null;
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.Items.Count == 0 || GroupComboBox.SelectedValue == null) return;

            _unitToFactory = Convert.ToBoolean(((DataRowView)GroupComboBox.SelectedItem).Row["UnitToFactory"]);

            if (!_unitToFactory)
            {
                FactoryRow.Height = new GridLength(0);
                WorkUnitsComboBox.ItemsSource = WorkUnitGroupFilter(Convert.ToInt32(GroupComboBox.SelectedValue));
                WorkUnitsComboBox.SelectedIndex = 0;
            }
            else
            {
                FactoryRow.Height = new GridLength(1, GridUnitType.Auto);
                FactoryComboBox.SelectedIndex = 0;
                FactoryComboBox_SelectionChanged(null, null);
            }
        }

        private void FactoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FactoryComboBox.Items.Count == 0 || FactoryComboBox.SelectedValue == null) return;

            WorkUnitsComboBox.ItemsSource = WorkUnitGroupFilter(Convert.ToInt32(GroupComboBox.SelectedValue),
                                                                Convert.ToInt32(FactoryComboBox.SelectedValue));
            WorkUnitsComboBox.SelectedIndex = 0;
        }

        private void WorkUnitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkUnitsComboBox.Items.Count == 0 || WorkUnitsComboBox.SelectedValue == null) return;

            WorkSectionsComboBox.ItemsSource = WorkSectionUnitFilter(Convert.ToInt32(WorkUnitsComboBox.SelectedValue));
            WorkSectionsComboBox.SelectedIndex = 0;
        }

        private void WorkSectionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkSectionsComboBox.Items.Count == 0 || WorkSectionsComboBox.SelectedValue == null) return;

            WorkSubSectionsComboBox.ItemsSource =
                WorkSubSectionFilter(Convert.ToInt32(WorkSectionsComboBox.SelectedValue));
            WorkSubSectionsComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GroupComboBox.SelectedValue == DBNull.Value || WorkUnitsComboBox.SelectedValue == DBNull.Value ||
                WorkSectionsComboBox.SelectedValue == DBNull.Value ||
                WorkSubSectionsComboBox.SelectedValue == DBNull.Value ||
                string.IsNullOrEmpty(RequestNoteTextBox.Text) || RequestTypeComboBox.SelectedValue == null ||
                (_unitToFactory && FactoryComboBox.SelectedValue == DBNull.Value)) return;

            int requestTypeId = Convert.ToInt32(RequestTypeComboBox.SelectedValue);
            int groupId = Convert.ToInt32(GroupComboBox.SelectedValue);
            int workUnitId = Convert.ToInt32(WorkUnitsComboBox.SelectedValue);
            int workSectionId = Convert.ToInt32(WorkSectionsComboBox.SelectedValue);
            int workSubSectionId = Convert.ToInt32(WorkSubSectionsComboBox.SelectedValue);
            string requestNote = RequestNoteTextBox.Text;
            var requestDate = App.BaseClass.GetDateFromSqlServer();
            int newCrashId;
            string newsText;
            int newsStatus = 6;

            if (_unitToFactory)
            {
                int factoryId = Convert.ToInt32(FactoryComboBox.SelectedValue);
                newCrashId = _sec.AddNewRequest(groupId, factoryId, workUnitId, workSectionId, workSubSectionId,
                    requestDate,
                    _curWorkerId, requestNote, requestTypeId);

                newsStatus = factoryId == 1 ? 6 : 7;
            }
            else
            {
                newCrashId = _sec.AddNewRequest(groupId, 1, workUnitId, workSectionId, workSubSectionId, requestDate,
                    _curWorkerId, requestNote, requestTypeId);
            }

            if (requestTypeId == 1)
                AdministrationClass.AddNewAction(7);
            else
                AdministrationClass.AddNewAction(8);

            if (requestTypeId == 1)
                newsText = string.Format(CrashRequestText, newCrashId.ToString("00000"),
                    _workSubSectionConverter.Convert(workSubSectionId, typeof (string), string.Empty,
                        new CultureInfo("ru-RU")),
                    _workUnitConverter.Convert(workUnitId, typeof (string), string.Empty, new CultureInfo("ru-RU")),
                    _workSectionConverter.Convert(workSectionId, typeof (string), string.Empty, new CultureInfo("ru-RU")),
                    requestNote);
            else
                newsText = string.Format(ProblemRequestText, newCrashId.ToString("00000"),
                    _workSubSectionConverter.Convert(workSubSectionId, typeof (string), string.Empty,
                        new CultureInfo("ru-RU")),
                    _workUnitConverter.Convert(workUnitId, typeof (string), string.Empty, new CultureInfo("ru-RU")),
                    _workSectionConverter.Convert(workSectionId, typeof (string), string.Empty, new CultureInfo("ru-RU")),
                    requestNote);

            NewsHelper.AddNews(requestDate, newsText, newsStatus, _curWorkerId);


            _mw = Window.GetWindow(this) as MainWindow;
            if (_mw != null)
            {
                var servEquipPage = _mw.MainFrame.Content as XamlFiles.ServiceEquipmentPage;
                if (servEquipPage != null)
                {
                    servEquipPage.SelectNewTableRow(newCrashId);
                }
            }

            CancelRequestButton_Click(null, null);
        }

        private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mw == null)
                _mw = Window.GetWindow(this) as MainWindow;
            if(_mw != null)
            {
                _mw.HideCatalogGrid();
            }
        }
    }
}
