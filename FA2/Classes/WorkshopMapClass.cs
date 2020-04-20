//using Xceed.Wpf.Toolkit.Core.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using FAIIControlLibrary;

namespace FA2.Classes
{
    public static class FrameworkElementExt
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel;
            if (parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
                .Where(x => x != element)
                .Select(Panel.GetZIndex)
                .Max();
            Panel.SetZIndex(element, maxZ + 1);
        }
    }

    public struct MachineInfo
    {
        public string MachineName;
        public int MachineId;
        public int LayerId;
        public SolidColorBrush Color;
    }

    public struct LockInfo
    {
        public string LockName;
        public int LockId;
        public int LayerId;
        public SolidColorBrush Color;
    }

    public struct ObjectInfo
    {
        public string ObjectName;
        public int ObjectId;
        public Brush Color;
        public int LayerId;
        public double Width;
        public double Height;
    }

    public class WorkshopMapClass
    {
        public double ScaleVar = 2.03;

        private readonly string _connectionString;

        #region Bindings_Declaration

        // Machines
        private MySqlDataAdapter _machinesDataAdapter;
        public DataTable MachinesDataTable;

        private MySqlDataAdapter _locksDataAdapter;
        public DataTable LocksDataTable;

        private MySqlDataAdapter _layerGroupsDataAdapter;
        public DataTable LayerGroupsDataTable;

        private MySqlDataAdapter _layersDataAdapter;
        public DataTable LayersDataTable;

        private MySqlDataAdapter _layersObjectsDataAdapter;
        public DataTable LayersObjectsDataTable;

        private MySqlDataAdapter _objectTypesDataAdapter;
        public DataTable ObjectTypesDataTable;

        private MySqlDataAdapter _rectanglesDataAdapter;
        public DataTable RectanglesDataTable;

        private MySqlDataAdapter _polylinesDataAdapter;
        public DataTable PolylinesDataTable;

        private MySqlDataAdapter _polygonsDataAdapter;
        public DataTable PolygonsDataTable;

        private MySqlDataAdapter _responsiblePersonsDataAdapter;
        public DataTable ResponsiblePersonsDataTable;

        private MySqlDataAdapter _responsibleTypesDataAdapter;
        public DataTable ResponsibleTypesDataTable;

        private MySqlDataAdapter _accessListDataAdapter;
        public DataTable AccessListDataTable;


        #endregion

        public WorkshopMapClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;
            Initialize();
        }

        private void Initialize()
        {
            Create();
            Fill();
            SetDefaultMachinesParameters();

            SetDefaultLocksParameters();
        }

        private void Create()
        {
            MachinesDataTable = new DataTable();
            LocksDataTable = new DataTable();

            LayerGroupsDataTable = new DataTable();
            LayersDataTable = new DataTable();
            LayersObjectsDataTable = new DataTable();
            ObjectTypesDataTable = new DataTable();
            LocksDataTable = new DataTable();

            RectanglesDataTable = new DataTable();
            PolylinesDataTable = new DataTable();
            PolygonsDataTable = new DataTable();

            ResponsiblePersonsDataTable = new DataTable();
            ResponsibleTypesDataTable = new DataTable();

            AccessListDataTable = new DataTable();
        }

        private void Fill()
        {
            FillAccessList();
            FillMachines();
            FillLayers();
            FillLayersObjects();
            FillObjectTypes();

            FillRectangles();
            FillPolylines();
            FillPolygons();

            FillResponsiblePersons();
            FillResponsibleTypes();

            FillLayerGroups();

            FillLocks();

        }

        private void FillMachines()
        {
            try
            {
                _machinesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MachineID, WorkSubSectionID, MachineName, MachineSchemeNumber, " +
                        "MachineWorkPlacesCount, Width, Length, Xpos, Ypos, IsTurned, IsVisible " +
                        "FROM FAIICatalog.Machines WHERE IsVisible = True",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_machinesDataAdapter);
                _machinesDataAdapter.Fill(MachinesDataTable);

                MachinesDataTable.Columns.Add("pxlWidth", typeof (Double));
                MachinesDataTable.Columns.Add("pxlLength", typeof (Double));
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillLayers()
        {
            try
            {
                if (LayersDataTable != null)
                {
                    LayersDataTable.Rows.Clear();
                }

                _layersDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT LayerGroupID, LayerID, LayerName, EditingWorkerID, EditingDate, IsLocked, IsEnabled, IsPrivate, IsEditLocked " +
                        "FROM FAIIWorkshopMap.Layers WHERE IsEnabled = True",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_layersDataAdapter);
                _layersDataAdapter.Fill(LayersDataTable);



                if (LayersDataTable != null && !LayersDataTable.Columns.Contains("IsVisible"))
                {
                    var dc = new DataColumn("IsVisible", typeof (Boolean)) {DefaultValue = false};
                    LayersDataTable.Columns.Add(dc);
                }

                if (LayersDataTable != null) SetVisibilityForLayers();
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillLayersObjects()
        {
            try
            {
                if (LayersObjectsDataTable != null) LayersObjectsDataTable.Clear();

                _layersObjectsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ObjectID, LayerID, ObjectTypeID, Color, Name, Description, IsEnabled, IsVisible " +
                        "FROM FAIIWorkshopMap.LayersObjects WHERE IsEnabled=True",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_layersObjectsDataAdapter);
                _layersObjectsDataAdapter.Fill(LayersObjectsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0003]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillObjectTypes()
        {
            try
            {
                if (ObjectTypesDataTable != null) ObjectTypesDataTable.Clear();

                _objectTypesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ObjectTypeID, ObjectTypeName FROM FAIIWorkshopMap.ObjectTypes",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_objectTypesDataAdapter);
                _objectTypesDataAdapter.Fill(ObjectTypesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0004]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillRectangles()
        {
            try
            {
                if (RectanglesDataTable != null) RectanglesDataTable.Clear();

                _rectanglesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT RectangleID, ObjectID, Width, Height, Xpos, Ypos FROM FAIIWorkshopMap.Rectangles",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_rectanglesDataAdapter);
                _rectanglesDataAdapter.Fill(RectanglesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0005]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillPolylines()
        {
            try
            {
                if (PolylinesDataTable != null) PolylinesDataTable.Clear();

                _polylinesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT PolylineID, ObjectID, Points FROM FAIIWorkshopMap.Polylines",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_polylinesDataAdapter);
                _polylinesDataAdapter.Fill(PolylinesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0006]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillPolygons()
        {
            try
            {
                if (PolygonsDataTable != null) PolygonsDataTable.Clear();

                _polygonsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT PolygonID ,ObjectID, Points, IsFill FROM FAIIWorkshopMap.Polygons",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_polygonsDataAdapter);
                _polygonsDataAdapter.Fill(PolygonsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0007]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillResponsiblePersons()
        {
            try
            {
                if (ResponsiblePersonsDataTable != null) ResponsiblePersonsDataTable.Clear();

                _responsiblePersonsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ResponsiblePersonID, WorkerID, ResponsibleTypeID, ObjectID, IsEnabled, LayerID FROM FAIIWorkshopMap.ResponsiblePersons",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_responsiblePersonsDataAdapter);
                _responsiblePersonsDataAdapter.Fill(ResponsiblePersonsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillResponsibleTypes()
        {
            try
            {
                if (ResponsibleTypesDataTable != null) ResponsibleTypesDataTable.Clear();

                _responsibleTypesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ResponsibleTypeID ,ResponsibleName FROM FAIIWorkshopMap.ResponsibleTypes",
                        _connectionString);
// ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_responsibleTypesDataAdapter);
                _responsibleTypesDataAdapter.Fill(ResponsibleTypesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillLayerGroups()
        {
            try
            {
                if (LayerGroupsDataTable != null)
                {
                    LayerGroupsDataTable.Rows.Clear();
                }

                _layerGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT LayerGroupID, LayerGroupName, IsEnabled FROM FAIIWorkshopMap.LayerGroups",
                        _connectionString);
// ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_layerGroupsDataAdapter);
                _layerGroupsDataAdapter.Fill(LayerGroupsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0010]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillLocks()
        {
            try
            {
                _locksDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT LockID, LockName, LockNotes, LockPhoto, EditingWorkerID, EditingSenderStatus, EditingDate, IsEnable, Xpos, Ypos, IsTurned " +
                        "FROM FAIIProdRooms.Locks  WHERE IsEnable = True",
                        _connectionString);
// ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_locksDataAdapter);
                _locksDataAdapter.Fill(LocksDataTable);

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0011]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillAccessList()
        {
            try
            {
                _accessListDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT AccessListID, LayerID, WorkerID, IsEnable FROM FAIIWorkshopMap.AccessList  WHERE IsEnable = True",
                        _connectionString);
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_accessListDataAdapter);
                _accessListDataAdapter.Fill(AccessListDataTable);

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [WMC0012]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Machines_Path

        private void SetDefaultMachinesParameters()
        {
            double xPoint = 10;
            double yPoint = 0;

            int i = 0;

            foreach (DataRowView dr in MachinesDataTable.DefaultView)
            {
                //-----------------
                if (dr["Width"] == DBNull.Value || Convert.ToInt32(dr["Width"]) == 0)
                {
                    return; // dr["Width"] = 2000;
                }

                if (dr["Length"] == DBNull.Value || Convert.ToInt32(dr["Length"]) == 0)
                {
                    return; // dr["Length"] = 4000;
                }
                //-----------------


                object w = dr["Width"];
                object l = dr["Length"];

                if ((w != DBNull.Value || Convert.ToInt32(w) != 0) &&
                    (l != DBNull.Value || Convert.ToInt32(l) != 0))
                {
                    if (!Convert.ToBoolean(dr["IsTurned"]))
                    {
                        dr["pxlWidth"] = Convert.ToInt32(w)*ScaleVar/1000;
                        dr["pxlLength"] = Convert.ToInt32(l)*ScaleVar/1000;
                    }
                    else
                    {
                        dr["pxlWidth"] = Convert.ToInt32(l)*ScaleVar/1000;
                        dr["pxlLength"] = Convert.ToInt32(w)*ScaleVar/1000;
                    }
                }

                if ((dr["Xpos"] == DBNull.Value || Convert.ToInt32(dr["Xpos"]) == 0) &&
                    (dr["Ypos"] == DBNull.Value || Convert.ToInt32(dr["Ypos"]) == 0))
                {
                    if (i == 10)
                    {
                        i = 0;

                        xPoint = xPoint + 15;

                        yPoint = 0;
                    }

                    dr["Xpos"] = xPoint;

                    yPoint = yPoint + 2 + Convert.ToDouble(dr["pxlLength"]);

                    dr["Ypos"] = yPoint;

                    i++;
                }
            }
        }

        public List<Border> GetMachinesIcons()
        {
            var brdList = new List<Border>();

            var xPosBinding = new Binding("Xpos") {Mode = BindingMode.TwoWay};

            var yPosBinding = new Binding("Ypos") {Mode = BindingMode.TwoWay};

            foreach (DataRowView dr in MachinesDataTable.DefaultView)
            {
                var mi = new MachineInfo
                {
                    MachineId = Convert.ToInt32(dr["MachineID"]),
                    MachineName = dr["MachineName"].ToString(),
                    LayerId = 1
                };


                #region create_tooltip

                var sp = new StackPanel {Margin = new Thickness(-5, 0, -5, -3)};

                var tb = new TextBlock
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    Text = dr["MachineName"].ToString(),
                    Margin = new Thickness(2)
                };

                var tb1 = new TextBlock
                {
                    Text = "Количество раб. мест: " + dr["MachineWorkPlacesCount"],
                    Margin = new Thickness(2)
                };

                var tb2 = new TextBlock
                {
                    Text = "Ширина: " + dr["Width"] + " мм",
                    Margin = new Thickness(2)
                };

                var tb3 = new TextBlock
                {
                    Text = "Длина: " + dr["Length"] + " мм",
                    Margin = new Thickness(2)
                };

                sp.Children.Add(tb);
                sp.Children.Add(tb1);

                sp.Children.Add(tb2);
                sp.Children.Add(tb3);

                #endregion


                var brd = new Border
                {
                    Tag = mi,
                    Width = Convert.ToDouble(dr["pxlWidth"]),
                    Height = Convert.ToDouble(dr["pxlLength"]),

                    BorderThickness = new Thickness(0.05),
                    CornerRadius = new CornerRadius(0.005),

                    SnapsToDevicePixels = true,
                    DataContext = dr,
                    ToolTip = sp
                };

                brd.SetBinding(Canvas.TopProperty, yPosBinding);

                brd.SetBinding(Canvas.LeftProperty, xPosBinding);

                brdList.Add(brd);
            }
            return brdList;
        }

        public void SaveMachines()
        {
            _machinesDataAdapter.Update(MachinesDataTable);
        }

        public void SetMachineValue(int machineId, string field, object value)
        {
            var custView = new DataView(MachinesDataTable, "", "MachineID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(machineId);

            if (foundRows.Length != 0) foundRows[0].Row[field] = value;
        }

        public object GetMachineValue(int machineId, string field)
        {
            var custView = new DataView(MachinesDataTable, "", "MachineID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(machineId);

            if (foundRows.Length != 0) return foundRows[0].Row[field];

            return null;
        }

        #endregion

        #region Locks_Path

        private void SetDefaultLocksParameters()
        {
            double xPoint = 10;
            double yPoint = 0;

            int i = 0;

            foreach (DataRowView dr in LocksDataTable.DefaultView)
            {
                if ((dr["Xpos"] == DBNull.Value || Convert.ToInt32(dr["Xpos"]) == 0) &&
                    (dr["Ypos"] == DBNull.Value || Convert.ToInt32(dr["Ypos"]) == 0))
                {
                    if (i == 10)
                    {
                        i = 0;

                        xPoint = xPoint + 15;

                        yPoint = 0;
                    }

                    dr["Xpos"] = xPoint;

                    yPoint = yPoint + 2 + 15;

                    dr["Ypos"] = yPoint;

                    i++;
                }
            }
        }

        public List<Border> GetLocksIcons()
        {
            var brdList = new List<Border>();

            var xPosBinding = new Binding("Xpos") {Mode = BindingMode.TwoWay};

            var yPosBinding = new Binding("Ypos") {Mode = BindingMode.TwoWay};

            foreach (DataRowView dr in LocksDataTable.DefaultView)
            {
                var lckInf = new LockInfo
                {
                    LockId = Convert.ToInt32(dr["LockId"]),
                    LockName = dr["LockName"].ToString(),
                    LayerId = 2
                };

                var brd = new Border
                {
                    Tag = lckInf,

                    BorderThickness = new Thickness(0.1),
                    CornerRadius = new CornerRadius(0.01),

                    DataContext = dr,

                    ToolTip = dr["LockName"]
                };

                if (!Convert.ToBoolean(dr["IsTurned"]))
                {

                    brd.Width = 3;
                    brd.Height = 8;
                }
                else
                {
                    brd.Width = 8;
                    brd.Height = 3;
                }

                brd.SetBinding(Canvas.TopProperty, yPosBinding);

                brd.SetBinding(Canvas.LeftProperty, xPosBinding);

                brdList.Add(brd);
            }
            return brdList;
        }

        public void SaveLocks()
        {
            _locksDataAdapter.Update(LocksDataTable);
        }

        public void SetLockValue(int lockId, string field, object value)
        {
            var custView = new DataView(LocksDataTable, "", "LockId",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(lockId);

            if (foundRows.Length != 0) foundRows[0].Row[field] = value;
        }

        public object GetLockValue(int lockId, string field)
        {
            var custView = new DataView(LocksDataTable, "", "LockId",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(lockId);

            if (foundRows.Length != 0) return foundRows[0].Row[field];

            return null;
        }

        #endregion


        public DataView GetAccessList()
        {
            return AccessListDataTable.AsDataView();
        }

        private void SaveAccessList()
        {
            _accessListDataAdapter.Update(AccessListDataTable);
            AccessListDataTable.Rows.Clear();

            FillAccessList();
        }

        public void AddWorkerToAccessList(int workerID)
        {
            DataRow dr = AccessListDataTable.NewRow();

            dr["LayerID"] = -1;
            dr["WorkerID"] = workerID;
            dr["IsEnable"] = true;

            AccessListDataTable.Rows.Add(dr);
        }

        public void ClearAccessList()
        {
            DataRow[] rows = AccessListDataTable.Select("LayerID = -1");

            foreach (var dataRow in rows)
            {
                dataRow.Delete();
            }
        }

        public DataView GetMachines()
        {
            return MachinesDataTable.AsDataView();
        }

        public DataView GetLocks()
        {
            return LocksDataTable.AsDataView();
        }

        public DataView GetLayers()
        {
            DataView dv = LayersDataTable.AsDataView();

            SetVisibilityForLayers();

            dv.RowFilter = "IsEnabled = 'True' AND IsVisible = 'True'";

            return dv;
        }

        private void SetVisibilityForLayers()
        {

            foreach (DataRowView dr in LayersDataTable.DefaultView)
            {
                dr["IsVisible"] = AdministrationClass.IsAdministrator || (!Convert.ToBoolean(dr["IsPrivate"]) ||
                                                                          (Convert.ToInt32(dr["EditingWorkerID"]) ==
                                                                           AdministrationClass.CurrentWorkerId ||
                                                                           CheckAccess(
                                                                               AdministrationClass.CurrentWorkerId,
                                                                               Convert.ToInt32(dr["LayerID"]))));
            }

            #region

            //if (AdministrationClass.IsAdministrator)
            //{
            //    dr["IsVisible"] = true;
            //}
            //else
            //{
            //    if (Convert.ToBoolean(dr["IsPrivate"]))
            //    {
            //        if (Convert.ToInt32(dr["EditingWorkerID"]) == AdministrationClass.CurrentWorkerId)
            //            dr["IsVisible"] = true;
            //        else
            //        {
            //            if (CheckAccess(AdministrationClass.CurrentWorkerId, Convert.ToInt32(dr["LayerID"])))
            //                dr["IsVisible"] = true;
            //            else
            //                dr["IsVisible"] = false;
            //        }
            //    }
            //    else dr["IsVisible"] = true;
            //}

            #endregion
        }


        private bool CheckAccess(int workerId, int layerId)
        {
            bool result = false;

            using (DataView accessDataView = GetAccessList())
            {

                accessDataView.RowFilter = "IsEnable = True AND LayerID = " + layerId + " AND WorkerID = " + workerId;

                if (accessDataView.Count != 0) result = true;
            }

            return result;
        }

        public DataView GetLayersObjects()
        {
            DataView dv = LayersObjectsDataTable.AsDataView();

            dv.RowFilter = "IsEnabled = 'True'";

            return dv;
        }

        public DataView GetResponsiblePersons()
        {
            DataView dv = ResponsiblePersonsDataTable.AsDataView();

            dv.RowFilter = "IsEnabled = 'True'";

            return dv;
        }

        public DataView GetResponsibleTypes()
        {
            DataView dv = ResponsibleTypesDataTable.AsDataView();

            return dv;
        }

        public DataView GetObjectTypes()
        {
            return ObjectTypesDataTable.AsDataView();
        }

        public DataView GetRectangles()
        {
            return RectanglesDataTable.AsDataView();
        }

        public DataView GetPolylines()
        {
            return PolylinesDataTable.AsDataView();
        }

        public DataView GetPolygons()
        {
            return PolygonsDataTable.AsDataView();
        }

        public DataView GetLayerGroups()
        {
            DataView dv = LayerGroupsDataTable.AsDataView();

            dv.RowFilter = "IsEnabled = 'True'";

            return dv;
        }

        public void AddNewLayer(int layerGroupId, string newLayerName, bool isPrivate, bool isEditLocked)
        {
            DataRow dr = LayersDataTable.NewRow();

            DateTime editingDate = App.BaseClass.GetDateFromSqlServer();

            dr["LayerGroupID"] = layerGroupId;
            dr["LayerName"] = newLayerName;
            dr["EditingWorkerID"] = AdministrationClass.CurrentWorkerId;
            dr["EditingDate"] = editingDate;
            dr["IsLocked"] = false;
            dr["IsEnabled"] = true;
            dr["IsPrivate"] = isPrivate;
            dr["IsEditLocked"] = isEditLocked;

            LayersDataTable.Rows.Add(dr);

            SaveLayers();


            using (DataView dv = GetAccessList())
            {
                dv.RowFilter = "LayerID = -1";

                if (dv.Count != 0)
                {
                    int layerID = GetLayerID(AdministrationClass.CurrentWorkerId, editingDate);

                    foreach (DataRowView drv in dv)
                    {
                        drv["LayerID"] = layerID;
                    }

                    SaveAccessList();
                }
            }


        }

        private int GetLayerID(int editingWorkerID, DateTime editingDate)
        {
            int result = -1;

            using (var con = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                using (
                    var cmd =
                        new MySqlCommand(
                            "SELECT LayerID FROM FAIIWorkshopMap.Layers WHERE IsEnabled = True AND EditingWorkerID = @editingWorkerID AND EditingDate = @editingDate;",
                            con))
                {
                    cmd.Parameters.AddWithValue("@editingWorkerID", editingWorkerID);
                    cmd.Parameters.AddWithValue("@editingDate", editingDate);
                    con.Open();

                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                        result = Convert.ToInt32(obj);

                    if (con.State == ConnectionState.Open) con.Close();
                }
            }

            return result;
        }

        public void SaveLayers()
        {
            _layersDataAdapter.Update(LayersDataTable);
            LayersDataTable.Rows.Clear();

            FillLayers();
        }

        public bool DeleteLayer(int layerId, string layerName)
        {
            bool result = false;

            if (
                MessageBox.Show("Вы действительно хотите удалить слой '" + layerName + "' ?", "Удаление",
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {

                var custView = new DataView(LayersDataTable, "", "LayerID",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(layerId);

                if (foundRows.Length != 0)
                {
                    foundRows[0].Row["IsEnabled"] = false;
                }

                SaveLayers();

                result = true;
            }

            return result;
        }

        public bool DeleteLayerObject(int objectId, string objectName)
        {
            bool result = false;

            if (
                MessageBox.Show("Вы действительно хотите удалить объект '" + objectName + "' ?", "Удаление",
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var custView = new DataView(LayersObjectsDataTable, "", "ObjectId",
                    DataViewRowState.CurrentRows);

                DataRowView[] foundRows = custView.FindRows(objectId);

                if (foundRows.Length != 0)
                {
                    foundRows[0].Row["IsEnabled"] = false;
                }
                result = true;
            }

            return result;
        }

        /// <summary>
        /// objectTypeId = 1 (Rectangle)
        /// </summary>
        public bool AddNewObject(int layerId, int objectTypeId, object color, string name, string description,
            double width, double height)
        {
            DataRow[] dataRows =
                LayersObjectsDataTable.Select("LayerId='" + layerId + "' AND Name='" + name + "' AND IsEnabled = 'True'");
            if (dataRows.Length != 0)
            {
                MessageBox.Show("Запись с таким названием существует!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            DataRow newObjectDataRow = LayersObjectsDataTable.NewRow();

            newObjectDataRow["LayerId"] = layerId;
            newObjectDataRow["ObjectTypeId"] = 1;
            newObjectDataRow["Color"] = color;
            newObjectDataRow["Name"] = name;
            newObjectDataRow["Description"] = description;

            LayersObjectsDataTable.Rows.Add(newObjectDataRow);
            _layersObjectsDataAdapter.Update(LayersObjectsDataTable);
            FillLayersObjects();


            DataRow[] dataRowsResult = LayersObjectsDataTable.Select("LayerId='" + layerId + "' AND Name='" + name + "'");

            int objectId = -1;

            if (dataRowsResult.Length != 0)
            {
                objectId = Convert.ToInt32(dataRowsResult[0]["ObjectId"]);
            }

            DataRow dr = RectanglesDataTable.NewRow();
            dr["ObjectId"] = objectId;
            dr["Width"] = width/1000*ScaleVar;
            dr["Height"] = height/1000*ScaleVar;
            dr["Xpos"] = 10;
            dr["Ypos"] = 10;

            RectanglesDataTable.Rows.Add(dr);
            _rectanglesDataAdapter.Update(RectanglesDataTable);
            FillRectangles();

            return true;
        }

        public bool CheckNewObjectName(int layerId, string name)
        {
            DataRow[] dataRows =
                LayersObjectsDataTable.Select("LayerId='" + layerId + "' AND Name='" + name + "' AND IsEnabled= 'True'");
            if (dataRows.Length != 0)
            {
                MessageBox.Show("Запись с таким названием существует!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// objectTypeId = 2 (Polygon)
        /// </summary>
        public bool AddNewObject(int layerId, int objectTypeId, string color, string lineName, string description,
            byte[] points, bool isFill)
        {
            DataRow newObjectDataRow = LayersObjectsDataTable.NewRow();

            newObjectDataRow["LayerId"] = layerId;
            newObjectDataRow["ObjectTypeId"] = 2;
            newObjectDataRow["Color"] = color;
            newObjectDataRow["Name"] = lineName;
            newObjectDataRow["Description"] = description;

            LayersObjectsDataTable.Rows.Add(newObjectDataRow);
            _layersObjectsDataAdapter.Update(LayersObjectsDataTable);

            FillLayersObjects();
            DataRow[] dataRowsResult =
                LayersObjectsDataTable.Select("LayerId='" + layerId + "' AND Name='" + lineName + "'");

            int objectId = -1;

            if (dataRowsResult.Length != 0)
            {
                objectId = Convert.ToInt32(dataRowsResult[0]["ObjectId"]);
            }

            DataRow dr = PolygonsDataTable.NewRow();
            dr["ObjectId"] = objectId;
            dr["Points"] = points;
            dr["IsFill"] = isFill;

            PolygonsDataTable.Rows.Add(dr);
            _polygonsDataAdapter.Update(PolygonsDataTable);

            return true;
        }

        /// <summary>
        /// objectTypeId = 3 (Polyline)
        /// </summary>
        public bool AddNewObject(int layerId, int objectTypeId, object color, string name, string description,
            byte[] points)
        {
            DataRow newObjectDataRow = LayersObjectsDataTable.NewRow();

            newObjectDataRow["LayerId"] = layerId;
            newObjectDataRow["ObjectTypeId"] = 3;
            newObjectDataRow["Color"] = color;
            newObjectDataRow["Name"] = name;
            newObjectDataRow["Description"] = description;

            LayersObjectsDataTable.Rows.Add(newObjectDataRow);
            _layersObjectsDataAdapter.Update(LayersObjectsDataTable);
            FillLayersObjects();
            DataRow[] dataRowsResult = LayersObjectsDataTable.Select("LayerId='" + layerId + "' AND Name='" + name + "'");

            int objectId = -1;

            if (dataRowsResult.Length != 0)
            {
                objectId = Convert.ToInt32(dataRowsResult[0]["ObjectId"]);
            }

            DataRow dr = PolylinesDataTable.NewRow();
            dr["ObjectId"] = objectId;
            dr["Points"] = points;

            PolylinesDataTable.Rows.Add(dr);
            _polylinesDataAdapter.Update(PolylinesDataTable);

            return true;
        }

        public List<Border> GetBordersObjects(int layerId)
        {
            var brdList = new List<Border>();

            var xPosBinding = new Binding("Xpos") {Mode = BindingMode.TwoWay};

            var yPosBinding = new Binding("Ypos") {Mode = BindingMode.TwoWay};

            var visBinding = new Binding("IsVisible")
            {
                Mode = BindingMode.TwoWay,
                Converter = new BooleanToVisibilityConverter()
            };


            DataRow[] brdObjects =
                LayersObjectsDataTable.Select("LayerID=" + layerId + " AND ObjectTypeID = 1 AND IsEnabled='True'");

            if (brdObjects.Length == 0) return brdList;

            foreach (DataRow brdObj in brdObjects)
            {
                int objectId = Convert.ToInt32(brdObj["ObjectID"]);
                bool isVisible = Convert.ToBoolean(brdObj["IsVisible"]);

                var color = (Brush) new BrushConverter().ConvertFrom(brdObj["Color"].ToString());
                color.Opacity = 0.9;

                RectanglesDataTable.DefaultView.RowFilter = "ObjectID=" + objectId;

                double width = Convert.ToDouble(RectanglesDataTable.DefaultView[0]["Width"]);
                double height = Convert.ToDouble(RectanglesDataTable.DefaultView[0]["Height"]);

                var objInfo = new ObjectInfo
                {
                    ObjectId = objectId,
                    ObjectName = brdObj["Name"].ToString(),
                    Color = color,
                    LayerId = layerId,
                    Width = width/ScaleVar,
                    Height = height/ScaleVar
                };

                var brd = new Border
                {
                    Tag = objInfo,
                    Width = width,
                    Height = height,
                    BorderBrush = color,
                    Background = color,

                    BorderThickness = new Thickness(0.3),
                    CornerRadius = new CornerRadius(0.05),

                    DataContext = RectanglesDataTable.DefaultView[0],
                    Visibility = isVisible ? Visibility.Visible : Visibility.Hidden,
                };

                brd.SetBinding(UIElement.VisibilityProperty, visBinding);

                brd.SetBinding(Canvas.TopProperty, yPosBinding);

                brd.SetBinding(Canvas.LeftProperty, xPosBinding);

                brdList.Add(brd);
            }
            return brdList;
        }

        public List<Polygon> GetPolygonsObjects(int layerId)
        {
            var polygonList = new List<Polygon>();

            DataRow[] plgnObjects =
                LayersObjectsDataTable.Select("LayerID=" + layerId + " AND ObjectTypeID = 2 AND IsEnabled='True'");
            if (plgnObjects.Length == 0) return polygonList;

            foreach (DataRow plgnObj in plgnObjects)
            {
                int objectId = Convert.ToInt32(plgnObj["ObjectID"]);
                bool isVisible = Convert.ToBoolean(plgnObj["IsVisible"]);

                string colorObjStr = plgnObj["Color"].ToString();
                var colorObj = (Brush) new BrushConverter().ConvertFrom(colorObjStr);

                var objInfo = new ObjectInfo
                {
                    LayerId = layerId,
                    ObjectId = objectId,
                    ObjectName = plgnObj["Name"].ToString(),
                    Color = colorObj
                };

                DataRow[] plgnDataRows = PolygonsDataTable.Select("ObjectID=" + objectId);

                bool isFill = Convert.ToBoolean(plgnDataRows[0]["IsFill"]);

                var plgn = new Polygon
                {
                    StrokeThickness = 0.3,
                    Tag = objInfo,
                    Stroke = colorObj,

                    Visibility = isVisible ? Visibility.Visible : Visibility.Hidden,
                };

                if (isFill)
                {
                    var fillColor = (Brush) new BrushConverter().ConvertFrom(colorObjStr);
                    fillColor.Opacity = 0.1;

                    plgn.Fill = fillColor;
                }

                PointCollection pc;

                var serializer = new XmlSerializer(typeof (PointCollection));

                using (var ms = new MemoryStream((Byte[]) plgnDataRows[0]["Points"]))
                {
                    pc = (PointCollection) serializer.Deserialize(ms);
                }

                plgn.Points = pc;

                polygonList.Add(plgn);
            }
            return polygonList;
        }

        public List<Polyline> GetPolylinesObjects(int layerId)
        {
            var polylineList = new List<Polyline>();

            DataRow[] pllnObjects =
                LayersObjectsDataTable.Select("LayerID=" + layerId + " AND ObjectTypeID = 3 AND IsEnabled='True'");
            if (pllnObjects.Length == 0) return polylineList;

            foreach (DataRow pllnObj in pllnObjects)
            {

                bool isVisible = Convert.ToBoolean(pllnObj["IsVisible"]);

                int objectId = Convert.ToInt32(pllnObj["ObjectID"]);

                string colorObjStr = pllnObj["Color"].ToString();
                var colorObj = (Brush) new BrushConverter().ConvertFrom(colorObjStr);
                colorObj.Opacity = 0.8;

                var objInfo = new ObjectInfo
                {
                    LayerId = layerId,
                    ObjectId = objectId,
                    ObjectName = pllnObj["Name"].ToString(),
                    Color = colorObj
                };

                DataRow[] pllnDataRows = PolylinesDataTable.Select("ObjectID=" + objectId);

                var plln = new Polyline
                {
                    StrokeThickness = 0.2,
                    Tag = objInfo,
                    Stroke = colorObj,
                    Visibility = isVisible ? Visibility.Visible : Visibility.Hidden,
                };

                PointCollection pc;

                var serializer = new XmlSerializer(typeof (PointCollection));

                using (var ms = new MemoryStream((Byte[]) pllnDataRows[0]["Points"]))
                {
                    pc = (PointCollection) serializer.Deserialize(ms);
                }

                plln.Points = pc;

                polylineList.Add(plln);
            }
            return polylineList;
        }

        public void SaveObjects()
        {
            _layersObjectsDataAdapter.Update(LayersObjectsDataTable);

            _rectanglesDataAdapter.Update(RectanglesDataTable);

            _polygonsDataAdapter.Update(PolygonsDataTable);

            _polylinesDataAdapter.Update(PolylinesDataTable);
        }

        public void AddResponsiblePerson(int layerId, int objectId, int workerId, int responsibleTypeId)
        {
            DataRow dr = ResponsiblePersonsDataTable.NewRow();

            dr["LayerID"] = layerId;
            dr["ObjectID"] = objectId;
            dr["WorkerID"] = workerId;
            dr["ResponsibleTypeID"] = responsibleTypeId;
            dr["IsEnabled"] = true;

            ResponsiblePersonsDataTable.Rows.Add(dr);

            SaveResponsiblePersons();
        }

        public void SaveResponsiblePersons()
        {
            _responsiblePersonsDataAdapter.Update(ResponsiblePersonsDataTable);
        }

        public void SaveLayerGroups()
        {
            _layerGroupsDataAdapter.Update(LayerGroupsDataTable);
            LayerGroupsDataTable.Rows.Clear();

            FillLayerGroups();
        }

        public void AddNewLayerGroup(string layerGroupName)
        {
            DataRow dr = LayerGroupsDataTable.NewRow();

            dr["LayerGroupName"] = layerGroupName;

            LayerGroupsDataTable.Rows.Add(dr);

            SaveLayerGroups();
        }
    }
}
