using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using FA2.ChildPages.ServiceEquipmentPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FA2.Notifications;
using FAIIControlLibrary;
using FAIIControlLibrary.UserControls;
using Microsoft.Win32;

namespace FA2.XamlFiles
{
    /// <summary>
    /// Логика взаимодействия для WorkshopMapPage.xaml
    /// </summary>
    public partial class WorkshopMapPage
    {
        private Point? _lastCenterPositionOnTarget;
        private Point? _lastMousePositionOnTarget;
        private Point? _lastDragPoint;

        private bool _firstRun = true;

        private WorkshopMapClass _wmc;
        private StaffClass _sc;
        private CatalogClass _catalogClass;

        private Border _selectedBorder;
        private Polygon _selectedPolygon;

        private bool _isLockLayers = true;

        private bool _isCanMoveMap = true;

        private bool _editMode;

        private bool _editLock;

        private bool _only90 = true;

        public struct LayersInfo
        {
            public int LayerId;
            public int LayerIndex;
        }

        private List<LayersInfo> _layers = new List<LayersInfo>();

        private PointCollection newObjectPoints = new PointCollection();
        private Polygon _tempPolygon;
        private Polyline _tempPolyline;
        private Canvas _currentLayerCanvas;

        private TextBlock _tempTextBlock;

        private readonly IdToNameConverter _idToNameConverter = new IdToNameConverter();

        private FtpClient _ftpClient;
        private string _basicDirectory;
        private WaitWindow _processWindow;
        private Thread _listLoadingThread;
        private long _fileSize;
        private string _neededOpeningFilePath;

        private DataTable _availableWorkersOnMachinesTable;

        public WorkshopMapPage(bool fullAccess)
        {
            InitializeComponent();

            EditLayersButton.Visibility = fullAccess ? Visibility.Visible : Visibility.Hidden;

            RefreshView();

            ViewToggleButton.IsChecked = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdministrationClass.OpenNewModuleEntry(AdministrationClass.Modules.WorkshopMap);

            NotificationManager.ClearNotifications(AdministrationClass.Modules.WorkshopMap);

            if (_firstRun)
            {

                _basicDirectory = App.GetFtpUrl + @"FtpFaII/WorkShopMapFiles/";

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (o, args) =>
                    {
                        GetClasses();
                        _availableWorkersOnMachinesTable = AdmissionsClass.GetAvailableWorkersForWorkSubsections();
                    };

                backgroundWorker.RunWorkerCompleted += (o, args) =>
                {
                    SetBindings();

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null) mainWindow.HideWaitAnnimation();

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
                    _ftpClient.UploadProgressChanged += OnUploadProgressChanged;
                    _ftpClient.DownloadProgressChanged += OnDownloadProgressChanged;
                    if (!_ftpClient.DirectoryExist(_basicDirectory))
                        _ftpClient.MakeDirectory(_basicDirectory);
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    ShowLayersFilesToggleButton.IsChecked = true;
                };

                backgroundWorker.RunWorkerAsync();

                _firstRun = false;

            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null) mainWindow.HideWaitAnnimation();
            }


            MapScrollViewer.ScrollChanged += MapScrollViewer_ScrollViewerScrollChanged;
            MapScrollViewer.PreviewMouseWheel += MapScrollViewer_PreviewMouseWheel;

            MapScrollViewer.MouseMove += MapScrollViewer_MouseMove;

            MapScrollViewer.PreviewMouseLeftButtonDown += MapScrollViewer_MouseLeftButtonDown;
            MapScrollViewer.PreviewMouseLeftButtonUp += MapScrollViewer_MouseLeftButtonUp;

            MapSlider.ValueChanged += MapSlider_ValueChanged;


            EditModeOnOff(false);
            HideMenuGrid();

            LayersCheckBox.IsChecked = true;
        }

        private void GetClasses()
        {
            App.BaseClass.GetStaffClass(ref _sc);
            App.BaseClass.GetWorkshopMapClass(ref _wmc);
            App.BaseClass.GetCatalogClass(ref _catalogClass);
        }

        private void SetBindings()
        {
            AccessFactoriesComboBox.SelectionChanged -= AccessFactoriesComboBox_SelectionChanged;
            AccessFactoriesComboBox.ItemsSource = _sc.GetFactories();
            AccessFactoriesComboBox.SelectionChanged += AccessFactoriesComboBox_SelectionChanged;

            AccessWorkersGroupsComboBox.SelectionChanged -= AccessWorkersGroupsComboBox_SelectionChanged;
            AccessWorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            AccessWorkersGroupsComboBox.SelectedValue = 2;
            AccessWorkersGroupsComboBox.SelectionChanged += AccessWorkersGroupsComboBox_SelectionChanged;
            AccessWorkersGroupsComboBox_SelectionChanged(null, null);
            
            AccessListBox.ItemsSource = _wmc.GetAccessList();
            
            FactoriesComboBox.SelectionChanged -= FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.DisplayMemberPath = "FactoryName";
            FactoriesComboBox.SelectedValuePath = "FactoryID";
            FactoriesComboBox.ItemsSource = _sc.GetFactories();
            FactoriesComboBox.SelectionChanged += FactoriesComboBox_SelectionChanged;
            FactoriesComboBox.Items.MoveCurrentToFirst();

            WorkersNamesListBox.SelectedValuePath = "WorkerID";
            WorkersNamesListBox.DisplayMemberPath = "Name";

            WorkersNamesListBox.Items.MoveCurrentToFirst();

            LayerGroupsListBox.ItemsSource = _wmc.GetLayerGroups();
            LayerGroupsListBox.SelectedIndex = 0;

            WorkersGroupsComboBox.SelectionChanged -= WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkersGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            WorkersGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkersGroupsComboBox.SelectedValue = 2;
            WorkersGroupsComboBox.SelectionChanged += WorkersGroupsComboBox_SelectionChanged;
            WorkersGroupsComboBox_SelectionChanged(null, null);
            
            NewObjectTypeComboBox.ItemsSource = _wmc.GetObjectTypes();

            ObjectTypeComboBox.ItemsSource = _wmc.GetObjectTypes();

            ResponsiblePersonsListBox.ItemsSource = _wmc.GetResponsiblePersons();
            ResponsibleTypesComboBox.ItemsSource = _wmc.GetResponsibleTypes();
            ResponsibleTypesComboBox.SelectedIndex = 0;

            LayersListBox.ItemsSource = _wmc.GetLayers();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(LayersListBox.ItemsSource);
            if (view.CanGroup)
                if (view.GroupDescriptions != null)
                    view.GroupDescriptions.Add(new PropertyGroupDescription("LayerGroupID"));

            LayersListBox_SelectionChanged(null, null);
        }

        #region Move_Rect

        private bool _canMove;
        private Point _dragPoint;

        private void MachineBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_editLock) return;
            
            if (!_isLockLayers)
            {
                MapScrollViewer.MouseMove -= MapScrollViewer_MouseMove;

                MapScrollViewer.PreviewMouseLeftButtonDown -= MapScrollViewer_MouseLeftButtonDown;
                MapScrollViewer.PreviewMouseLeftButtonUp -= MapScrollViewer_MouseLeftButtonUp;

                var curBrd = sender as Border;
                if (curBrd == null) return;

                Panel.SetZIndex(curBrd, 20);

                Mouse.Capture(curBrd);

                _dragPoint = Mouse.GetPosition(curBrd);
                _canMove = true;
            }

            if (sender as Border != null)
            {
                LayersObjectsListBox.SelectionChanged -= LayersObjectsListBox_SelectionChanged;

                LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

                LayersListBox.SelectedValue = ((MachineInfo) ((Border) sender).Tag).LayerId;

                LayersListBox.ScrollIntoView(LayersListBox.SelectedItem);

                LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;
                LayersListBox_SelectionChanged(null, null);

                LayersObjectsListBox.SelectedValue = ((MachineInfo) ((Border) sender).Tag).MachineId;

                LayersObjectsListBox.ScrollIntoView(LayersObjectsListBox.SelectedItem);

                LayersObjectsListBox.SelectionChanged += LayersObjectsListBox_SelectionChanged;

                LayersObjectsListBox_SelectionChanged(null, null);
            }
        }

        private void MachineBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_canMove) return;

            var brd = sender as Border;

            if (brd == null) return;

            double x = e.GetPosition(MapCanvas).X - _dragPoint.X;
            double y = e.GetPosition(MapCanvas).Y - _dragPoint.Y;

            if (e.GetPosition(MapCanvas).X < 0)
                x = 0;

            if (e.GetPosition(MapCanvas).X > MapCanvas.ActualWidth)
                x = MapCanvas.ActualWidth - brd.ActualWidth;

            if (e.GetPosition(MapCanvas).Y < 0)
                y = 0;

            if (e.GetPosition(MapCanvas).Y > MapCanvas.ActualHeight)
                y = MapCanvas.ActualHeight - brd.ActualHeight;

            brd.SetValue(Canvas.LeftProperty, x);
            brd.SetValue(Canvas.TopProperty, y);
        }

        private void MachineBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_canMove) return;

            MapScrollViewer.MouseMove += MapScrollViewer_MouseMove;

            MapScrollViewer.PreviewMouseLeftButtonDown += MapScrollViewer_MouseLeftButtonDown;
            MapScrollViewer.PreviewMouseLeftButtonUp += MapScrollViewer_MouseLeftButtonUp;

            var brd = sender as Border;
            if (brd == null) return;

            Panel.SetZIndex(brd, 10);

            Mouse.Capture(null);
            _canMove = false;
        }

        
        private void LockBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_editLock) return;

            if (!_isLockLayers)
            {
                MapScrollViewer.MouseMove -= MapScrollViewer_MouseMove;

                MapScrollViewer.PreviewMouseLeftButtonDown -= MapScrollViewer_MouseLeftButtonDown;
                MapScrollViewer.PreviewMouseLeftButtonUp -= MapScrollViewer_MouseLeftButtonUp;

                var curBrd = sender as Border;
                if (curBrd == null) return;

                Panel.SetZIndex(curBrd, 20);

                Mouse.Capture(curBrd);

                _dragPoint = Mouse.GetPosition(curBrd);
                _canMove = true;
            }

            if (sender as Border != null)
            {
                LayersObjectsListBox.SelectionChanged -= LayersObjectsListBox_SelectionChanged;

                LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

                LayersListBox.SelectedValue = ((LockInfo)((Border)sender).Tag).LayerId;

                LayersListBox.ScrollIntoView(LayersListBox.SelectedItem);

                LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;
                LayersListBox_SelectionChanged(null, null);

                LayersObjectsListBox.SelectedValue = ((LockInfo)((Border)sender).Tag).LockId;

                LayersObjectsListBox.ScrollIntoView(LayersObjectsListBox.SelectedItem);

                LayersObjectsListBox.SelectionChanged += LayersObjectsListBox_SelectionChanged;

                LayersObjectsListBox_SelectionChanged(null, null);
            }
        }

        //private void LockBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!_canMove) return;

        //    var brd = sender as Border;

        //    if (brd == null) return;

        //    double x = e.GetPosition(MapCanvas).X - _dragPoint.X;
        //    double y = e.GetPosition(MapCanvas).Y - _dragPoint.Y;

        //    if (e.GetPosition(MapCanvas).X < 0)
        //        x = 0;

        //    if (e.GetPosition(MapCanvas).X > MapCanvas.ActualWidth)
        //        x = MapCanvas.ActualWidth - brd.ActualWidth;

        //    if (e.GetPosition(MapCanvas).Y < 0)
        //        y = 0;

        //    if (e.GetPosition(MapCanvas).Y > MapCanvas.ActualHeight)
        //        y = MapCanvas.ActualHeight - brd.ActualHeight;

        //    brd.SetValue(Canvas.LeftProperty, x);
        //    brd.SetValue(Canvas.TopProperty, y);
        //}

        //private void LockBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (!_canMove) return;

        //    MapScrollViewer.MouseMove += MapScrollViewer_MouseMove;

        //    MapScrollViewer.PreviewMouseLeftButtonDown += MapScrollViewer_MouseLeftButtonDown;
        //    MapScrollViewer.PreviewMouseLeftButtonUp += MapScrollViewer_MouseLeftButtonUp;

        //    var brd = sender as Border;
        //    if (brd == null) return;

        //    Panel.SetZIndex(brd, 10);

        //    Mouse.Capture(null);
        //    _canMove = false;
        //}

        #endregion

        private void MapScrollViewer_ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePositionOnTarget.HasValue)
                {
                    if (_lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(MapScrollViewer.ViewportWidth/2,
                            MapScrollViewer.ViewportHeight/2);
                        Point centerOfTargetNow = MapScrollViewer.TranslatePoint(centerOfViewport, MapGrid);

                        targetBefore = _lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(MapGrid);

                    _lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth/MapGrid.Width;
                    double multiplicatorY = e.ExtentHeight/MapGrid.Height;

                    double newOffsetX = MapScrollViewer.HorizontalOffset - dXInTargetPixels*multiplicatorX;
                    double newOffsetY = MapScrollViewer.VerticalOffset - dYInTargetPixels*multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    MapScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    MapScrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _lastMousePositionOnTarget = Mouse.GetPosition(MapGrid);

            if (e.Delta > 0)
            {
                MapSlider.Value += 1;
            }
            if (e.Delta < 0)
            {
                MapSlider.Value -= 1;
            }

            e.Handled = true;
        }

        private void MapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           // PrintMapButton.Content = e.NewValue;

            MapGridScaleTransform.ScaleX = e.NewValue;
            MapGridScaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(MapScrollViewer.ViewportWidth/2, MapScrollViewer.ViewportHeight/2);

            _lastCenterPositionOnTarget = MapScrollViewer.TranslatePoint(centerOfViewport, MapGrid);
        }

        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCanMoveMap) return;

            if (_lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(MapScrollViewer);

                double dX = posNow.X - _lastDragPoint.Value.X;
                double dY = posNow.Y - _lastDragPoint.Value.Y;

                _lastDragPoint = posNow;

                MapScrollViewer.ScrollToHorizontalOffset(MapScrollViewer.HorizontalOffset - dX);
                MapScrollViewer.ScrollToVerticalOffset(MapScrollViewer.VerticalOffset - dY);
            }
        }

        private void MapScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isCanMoveMap) return;

            if (e.Source.GetType() != typeof (ZoomableCanvas) && e.Source.GetType() != typeof (ScrollViewer)) return;

            var mousePos = e.GetPosition(MapScrollViewer);
            if (mousePos.X <= MapScrollViewer.ViewportWidth && mousePos.Y < MapScrollViewer.ViewportHeight)
            {
                MapScrollViewer.Cursor = Cursors.SizeAll;
                _lastDragPoint = mousePos;
                Mouse.Capture(MapScrollViewer);
            }
        }

        private void MapScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isCanMoveMap) return;

            MapScrollViewer.Cursor = Cursors.Arrow;
            MapScrollViewer.ReleaseMouseCapture();
            _lastDragPoint = null;
        }

        private void EditLayersButton_Click(object sender, RoutedEventArgs e)
        {
            EditModeOnOff(!_editMode);
        }

        private void EditModeOnOff(bool editModeOn)
        {

            if (editModeOn)
            {
                EditLayersGrid.Children.Add(EditLayerButtonsGrid);

                EditObjectButtonsGrid.Visibility = Visibility.Visible;
                EditResponsiblePersonsButtonsGrid.Visibility = Visibility.Visible;

                NewFileButton.Visibility = Visibility.Visible;
                DeleteFileButton.Visibility = Visibility.Visible;

                ObjectDescriptionTextBox.IsReadOnly = false;
            }
            else
            {
                EditLayersGrid.Children.Remove(EditLayerButtonsGrid);

                EditObjectButtonsGrid.Visibility = Visibility.Hidden;
                EditResponsiblePersonsButtonsGrid.Visibility = Visibility.Hidden;

                NewFileButton.Visibility = Visibility.Hidden;
                DeleteFileButton.Visibility = Visibility.Hidden;

                ObjectDescriptionTextBox.IsReadOnly = true;
            }
            _isLockLayers = !editModeOn;
            _editMode = editModeOn;
        }
        
        private void NewLayersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuGrid(NewLayerBorder);

            NewLayerNameTextBox.Focus();
            PrivateLayerCheckBox.IsChecked = false;
            LockLayerCheckBox.IsChecked = false;
        }

        private void LayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdditionalPropertiesGrid.Children.Clear();
            AdditionalPropertiesBorder.Visibility = Visibility.Hidden;
            
            if (LayersListBox.SelectedItem == null || LayersListBox.SelectedValue == null)
            {
                if (LayersObjectsListBox.ItemsSource != null)
                    ((DataView) LayersObjectsListBox.ItemsSource).RowFilter = "LayerID='-1'";
                LayersObjectsListBox_SelectionChanged(null, null);

                ExplorerListBox.ItemsSource = null;

                return;
            }

            var lck = Convert.ToBoolean((((DataRowView) LayersListBox.SelectedItem)["IsLocked"]));

            if (lck) LockLayer();
            else UnLockLayer();

            var editLck = Convert.ToBoolean((((DataRowView)LayersListBox.SelectedItem)["IsEditLocked"]));

            if (editLck)
            {
                if (AdministrationClass.IsAdministrator) EditUnLockedLayer();
                else
                {
                    if (Convert.ToInt32((((DataRowView) LayersListBox.SelectedItem)["EditingWorkerID"])) ==
                        AdministrationClass.CurrentWorkerId)
                        EditUnLockedLayer();
                    else
                        EditLockedLayer();
                }
            }
            else EditUnLockedLayer();



            var layerId = Convert.ToInt32((((DataRowView) LayersListBox.SelectedItem)["LayerId"]));

            if (layerId == 1)
            {
                LayersObjectsListBox.ItemsSource = _wmc.GetMachines();
                LayersObjectsListBox.SelectedValuePath = "MachineID";

                ((DataView) LayersObjectsListBox.ItemsSource).Sort = "MachineName ASC";

            }
            else if (layerId == 2)
            {
                LayersObjectsListBox.ItemsSource = _wmc.GetLocks();
                LayersObjectsListBox.SelectedValuePath = "LockID";

                ((DataView) LayersObjectsListBox.ItemsSource).Sort = "LockName ASC";
            }
            else
            {
                LayersObjectsListBox.ItemsSource = _wmc.GetLayersObjects();
                LayersObjectsListBox.SelectedValuePath = "ObjectID";

                ((DataView) LayersObjectsListBox.ItemsSource).RowFilter = "LayerID=" + layerId +
                                                                          " AND IsEnabled = 'True'";

                ((DataView) LayersObjectsListBox.ItemsSource).Sort = "Name ASC";
            }


            //string s = LayersObjectsListBox.DisplayMemberPath.ToString();

            _currentLayerCanvas = null;
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                var canvas = MapGrid.Children[i] as Canvas;
                if (canvas != null)
                {
                    if (Convert.ToInt32(canvas.Tag) == layerId)
                    {
                        _currentLayerCanvas = canvas;
                    }
                }
            }

            LayersObjectsListBox_SelectionChanged(null, null);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            #region FileStorage

            ShowLayersFilesToggleButton.Checked -= ShowLayersFilesToggleButton_Checked;
            ShowLayersFilesToggleButton.IsChecked = true;
            ShowLayersFilesToggleButton.Checked += ShowLayersFilesToggleButton_Checked;

            ShowLayersFilesToggleButton_Checked(null, null);

            #endregion

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        private void LockLayer()
        {
            DeleteLayerButton.IsEnabled = false;
            NewObjectButton.IsEnabled = false;
            DeleteObjectButton.IsEnabled = false;

            VisabilityObjectCheckBox.IsEnabled = false;
        }

        private void UnLockLayer()
        {
            DeleteLayerButton.IsEnabled = true;
            NewObjectButton.IsEnabled = true;
            DeleteObjectButton.IsEnabled = true;

            VisabilityObjectCheckBox.IsEnabled = true;
        }

        private void EditLockedLayer()
        {
            DeleteLayerButton.IsEnabled = false;
            NewObjectButton.IsEnabled = false;
            DeleteObjectButton.IsEnabled = false;

            VisabilityObjectCheckBox.IsEnabled = false;

            NewFileButton.IsEnabled = false;
            DeleteFileButton.IsEnabled = false;

            SaveLayersButton.IsEnabled = false;

            NewResponsiblePersonButton.IsEnabled = false;
            DeleteResponsiblePersonButton.IsEnabled = false;

            ObjectsEditorBorder.IsEnabled = false;
        }

        private void EditUnLockedLayer()
        {
            DeleteLayerButton.IsEnabled = true;
            NewObjectButton.IsEnabled = true;
            DeleteObjectButton.IsEnabled = true;

            VisabilityObjectCheckBox.IsEnabled = true;

            NewFileButton.IsEnabled = true;
            DeleteFileButton.IsEnabled = true;

            SaveLayersButton.IsEnabled = true;

            NewResponsiblePersonButton.IsEnabled = true;
            DeleteResponsiblePersonButton.IsEnabled = true;

            ObjectsEditorBorder.IsEnabled = true;
        }

        private void LayerCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var curCheckBox = sender as CheckBox;

            if (curCheckBox == null) return;

            int layerId;

            if (!Int32.TryParse(curCheckBox.Tag.ToString(), out layerId)) return;

            LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

            LayersListBox.SelectedValue = layerId;

            LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;


            switch (layerId)
            {
                case 1:
                    ShowMachinesLayerObjects(layerId);
                    LayersListBox_SelectionChanged(null, null);
                    return;
                case 2:
                    ShowLockLayerObjects(layerId);
                    LayersListBox_SelectionChanged(null, null);
                    return;
            }

            ShowLayerObjects(layerId);

            _currentLayerCanvas = null;
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                var canvas = MapGrid.Children[i] as Canvas;
                if (canvas != null)
                {
                    if (Convert.ToInt32(canvas.Tag) == layerId)
                    {
                        _currentLayerCanvas = canvas;
                    }
                }
            }

            LayersListBox_SelectionChanged(null, null);
        }

        private void LayerCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var curCheckBox = sender as CheckBox;

            if (curCheckBox == null) return;

            int layerId;

            if (!Int32.TryParse(curCheckBox.Tag.ToString(), out layerId)) return;

            LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

            LayersListBox.SelectedValue = layerId;

            LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;



            HideLayerObjects(layerId);

            _currentLayerCanvas = null;
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                if (MapGrid.Children[i] is Canvas)
                {
                    if (Convert.ToInt32(((Canvas) MapGrid.Children[i]).Tag) == layerId)
                    {
                        _currentLayerCanvas = (Canvas) MapGrid.Children[i];
                    }
                }
            }
        }

        private void RedrawLayer(int layerId)
        {
            HideLayerObjects(layerId);

            if (layerId == 1)
            {
                ShowMachinesLayerObjects(layerId);
            }
            else
            {
                ShowLayerObjects(layerId);
            }
        }

        private void ShowMachinesLayerObjects(int layerId)
        {
            var machineLayerCanv = new Canvas();

            Panel.SetZIndex(machineLayerCanv, 999);

            AddMachineObjectsOnLayer(machineLayerCanv);

            machineLayerCanv.Tag = layerId;

            MapGrid.Children.Add(machineLayerCanv);

            int index = MapGrid.Children.IndexOf(machineLayerCanv);
            var li = new LayersInfo {LayerId = layerId, LayerIndex = index};

            _layers.Add(li);
        }

        private void ShowLockLayerObjects(int layerId)
        {
            var lockLayerCanv = new Canvas();

            Panel.SetZIndex(lockLayerCanv, 999);

            AddLockObjectsOnLayer(lockLayerCanv);

            lockLayerCanv.Tag = layerId;

            MapGrid.Children.Add(lockLayerCanv);

            int index = MapGrid.Children.IndexOf(lockLayerCanv);
            var li = new LayersInfo { LayerId = layerId, LayerIndex = index };

            _layers.Add(li);
        }
        
        private void HideLayerObjects(int layerId)
        {
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                var canvas = MapGrid.Children[i] as Canvas;
                if (canvas != null)
                    if (Convert.ToInt32(canvas.Tag) == layerId)
                    {
                        MapGrid.Children.Remove(canvas);
                        return;
                    }
            }
        }

        private void AddMachineObjectsOnLayer(Canvas machineLayerCanv)
        {
            #region ContextMenu

            var cntMenu = new ContextMenu();

            var cntMenuItem = new MenuItem
            {
                Header = "Повернуть на 90°",
                FontSize = 14,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            cntMenuItem.Click += TurnMachineMenuItem_Click;

            var cntMenuItem2 = new MenuItem
            {
                Header = "Заполнить заявку о поломке",
                FontSize = 14,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            cntMenuItem2.Click += (s, e) => AddMachineRemark(ServiceEquipmentClass.RequestType.Crash);

            var cntMenuItem3 = new MenuItem
            {
                Header = "Добавить замечание по станку",
                FontSize = 14,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            cntMenuItem3.Click += (s, e) => AddMachineRemark(ServiceEquipmentClass.RequestType.Truble);
           
            cntMenu.Items.Add(cntMenuItem);
            cntMenu.Items.Add(new Separator());
            cntMenu.Items.Add(cntMenuItem2);
            cntMenu.Items.Add(cntMenuItem3);

            cntMenu.Loaded += (s, e) => { cntMenuItem.IsEnabled = _editMode; };

            #endregion

            List<Border> brdList = _wmc.GetMachinesIcons();

            List<CrashMachineInfo> crasheMachineIds = ServiceEquipmentClass.GetCrashesMachines(ServiceEquipmentClass.RequestType.Crash);

            List<CrashMachineInfo> trubleMachineIds = ServiceEquipmentClass.GetCrashesMachines(ServiceEquipmentClass.RequestType.Truble);

            foreach (Border brd in brdList)
            {
                machineLayerCanv.Children.Add(brd);

                int machineId = ((MachineInfo) brd.Tag).MachineId;

                int sitId;
                var backColor = GetMachineColor(crasheMachineIds, trubleMachineIds, machineId, out sitId);

                var lighten = Color.FromArgb(255, (byte) (backColor.Color.R + ((255 - backColor.Color.R)/3)),
                    (byte) (backColor.Color.G + ((255 - backColor.Color.G)/3)),
                    (byte) (backColor.Color.B + ((255 - backColor.Color.B)/3)));

                var machineInfo = new MachineInfo
                {
                    MachineId = ((MachineInfo)brd.Tag).MachineId,
                    MachineName = ((MachineInfo)brd.Tag).MachineName,
                    LayerId = ((MachineInfo)brd.Tag).LayerId,
                    Color = backColor
                };

                brd.Tag = machineInfo;

                brd.Background = backColor;
                brd.BorderBrush = backColor;

                if (sitId != 0)
                {
                    SetMachineTooltip(crasheMachineIds, trubleMachineIds, brd);
                }
                SetAvailableWorkersForMachineToolTip(_availableWorkersOnMachinesTable, brd);

                brd.MouseEnter += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Hand;
                    brd.BorderBrush = new SolidColorBrush(lighten);

                    //MachineInfo mi = brd.Tag is MachineInfo ? (MachineInfo) brd.Tag : new MachineInfo();

                    //CurrentMachineNameLabel.Content = mi.MachineName;

                    brd.BorderThickness = new Thickness(0.3);
                };

                brd.MouseLeave += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Arrow;
                    brd.BorderBrush = backColor;
                    //CurrentMachineNameLabel.Content = "";

                    brd.BorderThickness = new Thickness(0.05);
                };

                brd.PreviewMouseDown += MachineBorder_PreviewMouseDown;
                brd.PreviewMouseUp += MachineBorder_PreviewMouseUp;
                brd.PreviewMouseMove += MachineBorder_PreviewMouseMove;

                brd.ContextMenu = cntMenu;
            }
        }

        private void AddLockObjectsOnLayer(Canvas lockLayerCanv)
        {
            #region ContextMenu

            var cntMenu = new ContextMenu();

            var cntMenuItem = new MenuItem
            {
                Header = "Повернуть на 90°",
                FontSize = 14,
                Height = 25,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            cntMenuItem.Click += TurnLockMenuItem_Click;


            cntMenu.Items.Add(cntMenuItem);

            cntMenu.Loaded += (s, e) => { cntMenuItem.IsEnabled = _editMode; };

            #endregion

            List<Border> brdList = _wmc.GetLocksIcons();

            DataTable closedDoorsDataTable = ProdRoomsClass.GetClosedDoors();

            foreach (Border brd in brdList)
            {
                lockLayerCanv.Children.Add(brd);

                int lockId = ((LockInfo) brd.Tag).LockId;

                var backColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#4CAF50");

                brd.ToolTip = "Состояние замка: Открыт" + "\n" +
                              ((LockInfo) brd.Tag).LockName;

                var custView = new DataView(closedDoorsDataTable, "", "LockID",
                DataViewRowState.CurrentRows);

                var foundRows = custView.FindRows(lockId);

                if (foundRows.Length != 0)
                {
                    backColor = (SolidColorBrush) new BrushConverter().ConvertFrom("#F44336");

                    if (foundRows[0]["ConfirmWorkerID"] == DBNull.Value)

                    brd.ToolTip = "Состояние замка: Закрыт" + "\n" +
                                  ((LockInfo)brd.Tag).LockName + "\n\n"
                                  + "Дата закрытия: " + foundRows[0]["Date"] + "\n"
                                  + "Закрыл: " +
                                  _idToNameConverter.Convert(foundRows[0]["WorkerID"], "ShortName") + "\n"
                                  + "Номер пломбы: " + foundRows[0]["SealNumber"];
                    else
                        brd.ToolTip = "Состояние замка: Закрыт" + "\n" +
                                     ((LockInfo)brd.Tag).LockName + "\n\n"
                                     + "Дата последнего подтверждения: " + foundRows[0]["ConfirmDate"] + "\n"
                                     + "Подтвердил: " +
                                     _idToNameConverter.Convert(foundRows[0]["ConfirmWorkerID"], "ShortName") + "\n"
                                     + "Номер пломбы: " + foundRows[0]["ConfirmSealNumber"];
                }

                var lighten = Color.FromArgb(255, (byte)(backColor.Color.R + ((255 - backColor.Color.R) / 3)),
                    (byte)(backColor.Color.G + ((255 - backColor.Color.G) / 3)),
                    (byte)(backColor.Color.B + ((255 - backColor.Color.B) / 3)));

                var lockInfo = new LockInfo
                {
                    LockId = lockId,
                    LockName = ((LockInfo)brd.Tag).LockName,
                    LayerId = ((LockInfo)brd.Tag).LayerId,
                    Color = backColor
                };

                brd.Tag = lockInfo;

                brd.Background = backColor;
                brd.BorderBrush = backColor;

                brd.MouseEnter += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Hand;
                    brd.BorderBrush = new SolidColorBrush(lighten);

                    //LockInfo li = brd.Tag is LockInfo ? (LockInfo)brd.Tag : new LockInfo();

                    //CurrentMachineNameLabel.Content = li.LockName;

                    brd.BorderThickness = new Thickness(0.1);
                };

                brd.MouseLeave += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Arrow;
                    brd.BorderBrush = backColor;
                    //CurrentMachineNameLabel.Content = "";

                    brd.BorderThickness = new Thickness(0.05);
                };

                //
                brd.PreviewMouseDown += LockBorder_PreviewMouseDown;
                brd.PreviewMouseUp += MachineBorder_PreviewMouseUp;
                brd.PreviewMouseMove += MachineBorder_PreviewMouseMove;
                //
                
                brd.ContextMenu = cntMenu;
            }
        }

        private void SetMachineTooltip(List<CrashMachineInfo> crasheMachineIds, List<CrashMachineInfo> trubleMachineIds, Border machineBorder)
        {
            var machineId = ((MachineInfo) machineBorder.Tag).MachineId;

            if (crasheMachineIds.Any(cmi => cmi.MachineId == machineId))
            {
                var reason = string.Empty;
                var rd = string.Empty;

                foreach (
                    var crashMachineInfo in
                        crasheMachineIds.Where(crashMachineInfo => crashMachineInfo.MachineId == machineId))
                {
                    reason = crashMachineInfo.CrashReason;
                    rd = crashMachineInfo.RequestDate.ToShortDateString();
                }

                if (reason != string.Empty)
                {
                    var sp = (StackPanel) (machineBorder.ToolTip);


                    var toolBrd = new Border
                    {
                        Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFF59790"),
                        MaxWidth = 350,
                        BorderThickness = new  Thickness(0,1,0,0),
                        BorderBrush = Brushes.Gray
                    };

                    var tb = new TextBlock
                    {
                        Text = reason,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Margin = new Thickness(5, 5, 5, 2),
                        Foreground = Brushes.White
                    };

                    var tb1 = new TextBlock
                    {
                        Text = "Дата: " + rd,
                        Margin = new Thickness(5, 2, 5, 5),
                        Foreground = Brushes.Gray
                    };

                    var borderSp = new StackPanel();

                    borderSp.Children.Add(tb);
                    borderSp.Children.Add(tb1);

                    toolBrd.Child = borderSp;

                    sp.Children.Add(toolBrd);
                }
            }

            if (trubleMachineIds.Any(tmi => tmi.MachineId == machineId))
            {
                var reason = string.Empty;
                var rd = string.Empty;

                foreach (
                    var crashMachineInfo in
                        trubleMachineIds.Where(crashMachineInfo => crashMachineInfo.MachineId == machineId))
                {
                    reason = crashMachineInfo.CrashReason;
                    rd = crashMachineInfo.RequestDate.ToShortDateString();
                }

                if (reason != string.Empty)
                {
                    var sp = (StackPanel) (machineBorder.ToolTip);


                    var toolBrd = new Border
                    {
                        Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFE595"),
                        MaxWidth = 350,
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        BorderBrush = Brushes.Gray
                    };

                    var tb = new TextBlock
                    {
                        Text = reason,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Margin = new Thickness(5,5,5,2)
                    };

                    var tb1 = new TextBlock
                    {
                        Text = "Дата: "+ rd,
                        Margin = new Thickness(5, 2, 5, 5)
                    };

                    var borderSp = new StackPanel();

                    borderSp.Children.Add(tb);
                    borderSp.Children.Add(tb1);

                    toolBrd.Child = borderSp;

                    sp.Children.Add(toolBrd);
                }
            }
        }

        private void SetAvailableWorkersForMachineToolTip(DataTable availableWorkersTable, Border machineBorder)
        {
            var machineId = ((MachineInfo)machineBorder.Tag).MachineId;

            if (availableWorkersTable != null && availableWorkersTable.Rows.Count != 0)
            {
                var machines = _catalogClass.MachinesDataTable.Select(string.Format("MachineID = {0}", machineId));
                if (machines.Any())
                {
                    var machine = machines.First();
                    long subSectionId = 0;
                    long.TryParse(machine["WorkSubSectionID"].ToString(), out subSectionId);
                    var workers =
                        availableWorkersTable.AsEnumerable().
                            Where(r => r.Field<Int64>("WorkSubsectionID") == subSectionId).Select(r => r.Field<Int64>("WorkerID"));
                    if (workers.Any())
                    {
                        var sp = (StackPanel)(machineBorder.ToolTip);

                        var toolBrd = new Border
                        {
                            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA5D6A7"),
                            MaxWidth = 350,
                            BorderThickness = new Thickness(0, 1, 0, 0),
                            BorderBrush = Brushes.Gray
                        };

                        var borderSp = new StackPanel();
                        toolBrd.Child = borderSp;

                        var headTextBlock = new TextBlock
                        {
                            FontSize = 14,
                            Margin = new Thickness(5, 5, 5, 2),
                            TextWrapping = TextWrapping.Wrap,
                            Text = "Список работников имеющих доступ:"
                        };
                        borderSp.Children.Add(headTextBlock);

                        foreach (var workerId in workers)
                        {
                            var textBlock = new TextBlock
                            {
                                Text = _idToNameConverter.Convert(workerId, "ShortName"),
                                TextTrimming = TextTrimming.CharacterEllipsis,
                                Margin = new Thickness(5, 0, 5, 2)
                            };

                            borderSp.Children.Add(textBlock);
                        }

                        sp.Children.Add(toolBrd);
                    }
                }
            }
        }
        
        private SolidColorBrush GetMachineColor(List<CrashMachineInfo> crasheMachineIds, List<CrashMachineInfo> trubleMachineIds,
            int machineId, out int sitId)
        {
            var result = (SolidColorBrush)new BrushConverter().ConvertFrom("#3575E0");

            sitId = 0;

            if (crasheMachineIds.Any(cmi => cmi.MachineId == machineId))
            {
                sitId = 1;
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#F44336");
            }

            if (trubleMachineIds.Any(tmi => tmi.MachineId == machineId))
            {
                sitId = 2;
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFC107");
            }

            return result;
        }

        private void AddMachineRemark(ServiceEquipmentClass.RequestType remarkType)
        {
            if (LayersObjectsListBox.SelectedItem == null) return;

            var workSubSectionId = Convert.ToInt32(((DataRowView)LayersObjectsListBox.SelectedItem)["WorkSubSectionID"]);

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var addServEquip = new AddServiceEquipment(remarkType, workSubSectionId);
                mainWindow.ShowCatalogGrid(addServEquip, "Добавить заявку");
            }
        }

        private void TurnMachineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //string s = sender.ToString();

            double tempWidth = _selectedBorder.Width;
            _selectedBorder.Width = _selectedBorder.Height;
            _selectedBorder.Height = tempWidth;
            //_wmc.MachinesInfo
            int machineId = ((MachineInfo) _selectedBorder.Tag).MachineId;

            _wmc.SetMachineValue(machineId, "pxlWidth", _selectedBorder.Width);
            _wmc.SetMachineValue(machineId, "pxlLength", _selectedBorder.Height);

            _wmc.SetMachineValue(machineId, "IsTurned", !Convert.ToBoolean(_wmc.GetMachineValue(machineId, "IsTurned")));
        }

        private void TurnLockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            double tempWidth = _selectedBorder.Width;
            _selectedBorder.Width = _selectedBorder.Height;
            _selectedBorder.Height = tempWidth;

            int lockId = ((LockInfo)_selectedBorder.Tag).LockId;

            _wmc.SetLockValue(lockId, "IsTurned", !Convert.ToBoolean(_wmc.GetLockValue(lockId, "IsTurned")));
        }

        private void ShowLayerObjects(int layerId)
        {
            var layerCanv = new Canvas();

            Panel.SetZIndex(layerCanv, 999);

            AddObjectsOnLayer(layerCanv, layerId);

            layerCanv.Tag = layerId;

            MapGrid.Children.Add(layerCanv);

            int index = MapGrid.Children.IndexOf(layerCanv);
            var li = new LayersInfo {LayerId = layerId, LayerIndex = index};
            _layers.Add(li);
            _currentLayerCanvas = layerCanv;
        }

        private void AddObjectsOnLayer(Canvas layerCanv, int layerId)
        {
            foreach (Polygon plgn in _wmc.GetPolygonsObjects(layerId))
            {
                plgn.PreviewMouseDown += PolygonBorder_PreviewMouseDown;
                layerCanv.Children.Add(plgn);
            }

            foreach (Polyline plln in _wmc.GetPolylinesObjects(layerId))
            {
                plln.PreviewMouseDown += PolyLine_PreviewMouseDown;
                layerCanv.Children.Add(plln);
            }

            foreach (Border brd in _wmc.GetBordersObjects(layerId))
            {
                layerCanv.Children.Add(brd);

                brd.MouseEnter += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Hand;

                    //ObjectInfo oi = brd.Tag is ObjectInfo ? (ObjectInfo) brd.Tag : new ObjectInfo();

                    //CurrentMachineNameLabel.Content = oi.ObjectName;

                    brd.BorderThickness = new Thickness(0.3);
                };

                brd.MouseLeave += (obj, ea) =>
                {
                    brd.Cursor = Cursors.Arrow;
                    //CurrentMachineNameLabel.Content = "";

                    brd.BorderThickness = new Thickness(0.3);
                };

                brd.PreviewMouseDown += ObjectBorder_PreviewMouseDown;
                brd.PreviewMouseUp += ObjectBorder_PreviewMouseUp;
                brd.PreviewMouseMove += ObjectBorder_PreviewMouseMove;
            }
        }

        private void PolyLine_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_editLock) return;

            var polyLine = sender as Polyline;
            if (polyLine == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                SelectedMachineNameLabel.Content = ((ObjectInfo)polyLine.Tag).ObjectName;

                LayersListBox.SelectedValue = ((ObjectInfo)polyLine.Tag).LayerId;

                LayersObjectsListBox.SelectedValue = ((ObjectInfo)polyLine.Tag).ObjectId;
                LayersObjectsListBox.ScrollIntoView(LayersObjectsListBox.SelectedItem);
            }
        }

        private void PolygonBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_editLock) return;

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                if (_selectedBorder != null)
                    if (_selectedBorder.Tag is MachineInfo)
                    {
                        _selectedBorder.Background = ((MachineInfo) _selectedBorder.Tag).Color;
                            //(Brush) new BrushConverter().ConvertFrom("#DD738ffe");
                    }
                    else if (_selectedBorder.Tag is ObjectInfo)
                    {
                        _selectedBorder.Background = ((ObjectInfo) _selectedBorder.Tag).Color;
                    }

                if (_selectedPolygon != null)
                    if (_selectedPolygon.Tag is ObjectInfo)
                    {
                        _selectedPolygon.Fill.Opacity = 0.2;
                        _selectedPolygon = null;
                    }

                _selectedPolygon = sender as Polygon;

                if (_selectedPolygon != null)
                {
                    _selectedPolygon.Fill.Opacity = 0.8;

                    SelectedMachineNameLabel.Content = ((ObjectInfo) _selectedPolygon.Tag).ObjectName;

                    LayersListBox.SelectedValue = ((ObjectInfo) _selectedPolygon.Tag).LayerId;

                    LayersObjectsListBox.SelectedValue = ((ObjectInfo) _selectedPolygon.Tag).ObjectId;
                    LayersObjectsListBox.ScrollIntoView(LayersObjectsListBox.SelectedItem);
                }
            }
        }

        #region Move_Obj

        private bool _canMoveObj;
        private Point _dragPointObj;

        private void ObjectBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_editLock) return;

            if (!_isLockLayers)
            {
                MapScrollViewer.MouseMove -= MapScrollViewer_MouseMove;

                MapScrollViewer.PreviewMouseLeftButtonDown -= MapScrollViewer_MouseLeftButtonDown;
                MapScrollViewer.PreviewMouseLeftButtonUp -= MapScrollViewer_MouseLeftButtonUp;

                var curBrd = sender as Border;
                if (curBrd == null) return;

                Panel.SetZIndex(curBrd, 20);

                Mouse.Capture(curBrd);

                _dragPointObj = Mouse.GetPosition(curBrd);
                _canMoveObj = true;
            }

            if (sender as Border != null)
            {
                LayersObjectsListBox.SelectionChanged -= LayersObjectsListBox_SelectionChanged;

                LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

                LayersListBox.SelectedValue = ((ObjectInfo) ((Border) sender).Tag).LayerId;

                LayersListBox.ScrollIntoView(LayersListBox.SelectedItem);

                LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;
                LayersListBox_SelectionChanged(null, null);

                LayersObjectsListBox.SelectedValue = ((ObjectInfo) ((Border) sender).Tag).ObjectId;

                LayersObjectsListBox.ScrollIntoView(LayersObjectsListBox.SelectedItem);

                LayersObjectsListBox.SelectionChanged += LayersObjectsListBox_SelectionChanged;

                LayersObjectsListBox_SelectionChanged(null, null);
            }
        }

        private void ObjectBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_canMoveObj) return;

            var brd = sender as Border;

            if (brd == null) return;

            double x = e.GetPosition(MapCanvas).X - _dragPointObj.X;
            double y = e.GetPosition(MapCanvas).Y - _dragPointObj.Y;

            if (e.GetPosition(MapCanvas).X < 0)
                x = 0;

            if (e.GetPosition(MapCanvas).X > MapCanvas.ActualWidth)
                x = MapCanvas.ActualWidth - brd.ActualWidth;

            if (e.GetPosition(MapCanvas).Y < 0)
                y = 0;

            if (e.GetPosition(MapCanvas).Y > MapCanvas.ActualHeight)
                y = MapCanvas.ActualHeight - brd.ActualHeight;

            brd.SetValue(Canvas.LeftProperty, x);
            brd.SetValue(Canvas.TopProperty, y);
        }

        private void ObjectBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_canMoveObj) return;

            MapScrollViewer.MouseMove += MapScrollViewer_MouseMove;

            MapScrollViewer.PreviewMouseLeftButtonDown += MapScrollViewer_MouseLeftButtonDown;
            MapScrollViewer.PreviewMouseLeftButtonUp += MapScrollViewer_MouseLeftButtonUp;

            var brd = sender as Border;
            if (brd == null) return;

            //brd.SetBinding()

            Panel.SetZIndex(brd, 10);

            Mouse.Capture(null);
            _canMoveObj = false;
        }

        #endregion

        private void SaveLayersButton_Click(object sender, RoutedEventArgs e)
        {
            _wmc.SaveMachines();
            _wmc.SaveObjects();
            _wmc.SaveLocks();
        }

        private void AddNewLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewLayerNameTextBox.Text.Trim() == string.Empty) return;
            if (LayerGroupsListBox.SelectedValue == null && LayerGroupsListBox.SelectedItem == null) return;

            LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

            _wmc.AddNewLayer(Convert.ToInt32(LayerGroupsListBox.SelectedValue), NewLayerNameTextBox.Text, PrivateLayerCheckBox.IsChecked == true,
                LockLayerCheckBox.IsChecked == true);
            AdministrationClass.AddNewAction(85);

            _wmc.ClearAccessList();

            CancelNewLayerButton_Click(null, null);

            NewLayerNameTextBox.Text = string.Empty;
            PrivateLayerCheckBox.IsChecked = false;

            LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;
            LayersListBox.SelectedIndex = LayersListBox.Items.Count - 1;
            LayersListBox.ScrollIntoView(LayersListBox.SelectedItem);
            LayersListBox_SelectionChanged(null, null);
        }

        private void CancelNewLayerButton_Click(object sender, RoutedEventArgs e)
        {
            HideMenuGrid();
            _wmc.ClearAccessList();
        }

        private void ShowMenuGrid(UIElement shownElement)
        {
            MenuGrid.Visibility = Visibility.Visible;

            shownElement.Visibility = Visibility.Visible;

            NewObjectColorColorPicker.SelectedIndex = 1;

            NewObjectNameTextBox.Text = string.Empty;
            NewObjectDescriptionTextBox.Text = string.Empty;
            IsFillCheckBox.IsChecked = true;
            NewRectWidthTextBox.Text = 1000.ToString(CultureInfo.InvariantCulture);
            NewRectHeightTextBox.Text = 1000.ToString(CultureInfo.InvariantCulture);

            MenuContentGrid.Children.Clear();

            MenuContentGrid.Children.Add(shownElement);
        }

        private void HideMenuGrid()
        {
            MenuGrid.Visibility = Visibility.Collapsed;
            MenuContentGrid.Children.Clear();
        }

        private void DeleteLayerButton_Click(object sender, RoutedEventArgs e)
        {
            int layerId = Convert.ToInt32(LayersListBox.SelectedValue);

            HideLayerObjects(layerId);

            _wmc.DeleteLayer(layerId, ((DataRowView) LayersListBox.SelectedItem)["LayerName"].ToString());
        }

        private void LayersObjectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayersObjectsListBox.SelectedItem == null)
            {
                //ObjectDescriptionTextBox.Text = string.Empty;

                ObjectsEditorBorder.DataContext = null;

                ((DataView) ResponsiblePersonsListBox.ItemsSource).RowFilter = "LayerID='-1'";

                if (Convert.ToBoolean(ShowObjectsFilesToggleButton.IsChecked))
                    ExplorerListBox.ItemsSource = null;

                return;
            }

            var layerId = Convert.ToInt32((((DataRowView) LayersListBox.SelectedItem)["LayerId"]));

            if (layerId == 1 || layerId == 2)
            {
                ObjectsEditorBorder.DataContext = null;
                ObjectsEditorBorder.IsEnabled = false;
            }
            else
            {
                ObjectsEditorBorder.IsEnabled = true;
                ObjectsEditorBorder.DataContext = LayersObjectsListBox.SelectedItem;
            }

            int objectId;

            switch (layerId)
            {
                case 1:
                    objectId = Convert.ToInt32(((DataRowView)LayersObjectsListBox.SelectedItem)["MachineId"]);
                    break;
                case 2:
                    objectId = Convert.ToInt32(((DataRowView)LayersObjectsListBox.SelectedItem)["LockID"]);
                    break;
                default:
                    objectId = Convert.ToInt32(((DataRowView)LayersObjectsListBox.SelectedItem)["ObjectID"]);
                    break;
            }


            ((DataView) ResponsiblePersonsListBox.ItemsSource).RowFilter = "LayerID='" + layerId + "' AND ObjectID='" +
                                                                           objectId + "' AND IsEnabled = 'True'";

            SelectObjectOnMap(objectId, layerId);

            #region FileStorage

            ShowObjectsFilesToggleButton.Checked -= ShowObjectsFilesToggleButton_Checked;
            ShowObjectsFilesToggleButton.IsChecked = true;
            ShowObjectsFilesToggleButton.Checked += ShowObjectsFilesToggleButton_Checked;

            ShowObjectsFilesToggleButton_Checked(null, null);

            #endregion
        }

        private void SelectObjectOnMap(int objectId, int layerId)
        {
            //_currentObject = null;

            if (layerId == 1)
            {
                #region layerId_1
                if (_selectedBorder != null)
                    if (_selectedBorder.Tag is MachineInfo)
                    {
                        _selectedBorder.Background = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }
                    else if (_selectedBorder.Tag is ObjectInfo)
                    {
                        _selectedBorder.Background = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }

                for (int i = 0; i < MapGrid.Children.Count; i++)
                {
                    if (MapGrid.Children[i] is Canvas)
                        if (Convert.ToInt32(((Canvas) MapGrid.Children[i]).Tag) == layerId)
                        {
                            foreach (Border brd in ((Canvas) MapGrid.Children[i]).Children)
                            {
                                if (((MachineInfo) brd.Tag).MachineId == objectId)
                                {
                                    _selectedBorder = brd;

                                    if (_selectedBorder != null)
                                    {
                                        var backColor = (SolidColorBrush) _selectedBorder.Background;

                                        var darken = Color.FromArgb(255, (byte) (backColor.Color.R/1.7),
                                            (byte) (backColor.Color.G/1.7),
                                            (byte) (backColor.Color.B/1.7));

                                        _selectedBorder.Background = new SolidColorBrush(darken);

                                        _selectedBorder.BorderBrush =
                                            (Brush)new BrushConverter().ConvertFrom("#F44336");
                                        _selectedBorder.BorderThickness = new Thickness(0.5);

                                        SelectedMachineNameLabel.Content =
                                            ((MachineInfo) _selectedBorder.Tag).MachineName;

                                        //_currentObject = brd;
                                    }
                                }
                            }
                        }
                }
                #endregion
            }

            else if (layerId == 2)
            {
                #region layerId_2

                if (_selectedBorder != null)
                    if (_selectedBorder.Tag is MachineInfo)
                    {
                        _selectedBorder.Background = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }
                    else if (_selectedBorder.Tag is ObjectInfo)
                    {
                        _selectedBorder.Background = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }
                    else if (_selectedBorder.Tag is LockInfo)
                    {
                        _selectedBorder.Background = ((LockInfo)_selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((LockInfo)_selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }

                for (int i = 0; i < MapGrid.Children.Count; i++)
                {
                    if (MapGrid.Children[i] is Canvas)
                        if (Convert.ToInt32(((Canvas) MapGrid.Children[i]).Tag) == layerId)
                        {
                            foreach (Border brd in ((Canvas) MapGrid.Children[i]).Children)
                            {
                                if (((LockInfo)brd.Tag).LockId == objectId)
                                {
                                    _selectedBorder = brd;

                                    if (_selectedBorder != null)
                                    {
                                        var backColor = (SolidColorBrush) _selectedBorder.Background;

                                        var darken = Color.FromArgb(255, (byte) (backColor.Color.R/1.5),
                                            (byte) (backColor.Color.G/1.5),
                                            (byte) (backColor.Color.B/1.5));

                                        _selectedBorder.Background = new SolidColorBrush(darken);

                                        _selectedBorder.BorderBrush =
                                            (Brush) new BrushConverter().ConvertFrom("#e51c23");
                                        _selectedBorder.BorderThickness = new Thickness(0.5);

                                        SelectedMachineNameLabel.Content =
                                            ((LockInfo)_selectedBorder.Tag).LockName;

                                        //_currentObject = brd;
                                    }
                                }
                            }
                        }
                }

                #endregion
            }

            else
            {
                #region else

                if (_selectedBorder != null)
                {
                    if (_selectedBorder.Tag is MachineInfo)
                    {
                        _selectedBorder.Background = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((MachineInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }
                    else if (_selectedBorder.Tag is ObjectInfo)
                    {
                        _selectedBorder.Background = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderBrush = ((ObjectInfo) _selectedBorder.Tag).Color;
                        _selectedBorder.BorderThickness = new Thickness(0.1);
                    }
                }

                if (_selectedPolygon != null)
                {
                    _selectedPolygon.Fill.Opacity = 0.2;

                    _selectedPolygon = null;
                }

                for (int i = 0; i < MapGrid.Children.Count; i++)
                {
                    if (MapGrid.Children[i] is Canvas)
                        if (Convert.ToInt32(((Canvas) MapGrid.Children[i]).Tag) == layerId)
                        {
                            for (int j = 0; j < ((Canvas) MapGrid.Children[i]).Children.Count; j++)
                            {
                                object obj = ((Canvas) MapGrid.Children[i]).Children[j];

                                //_currentObject = obj;
                                if (obj is Border)
                                {
                                    var brd = obj as Border;
                                    if (((ObjectInfo) brd.Tag).ObjectId == objectId)
                                    {
                                        _selectedBorder = brd;

                                        if (_selectedBorder != null)
                                        {
                                            var backColor = (SolidColorBrush) _selectedBorder.Background;

                                            var darken = Color.FromArgb(255, (byte) (backColor.Color.R/1.4),
                                                (byte) (backColor.Color.G/1.4),
                                                (byte) (backColor.Color.B/1.4));

                                            _selectedBorder.Background = new SolidColorBrush(darken);

                                            _selectedBorder.BorderBrush =
                                                (Brush) new BrushConverter().ConvertFrom("#e51c23");

                                            _selectedBorder.BorderThickness = new Thickness(0.5);

                                            SelectedMachineNameLabel.Content =
                                                ((ObjectInfo) _selectedBorder.Tag).ObjectName;
                                        }

                                        AdditionalPropertiesGrid.Children.Clear();

                                        var lbl1 = new Label
                                        {
                                            Foreground = Brushes.DimGray,
                                            Content = "Габариты",
                                            Width = 100,
                                            HorizontalContentAlignment = HorizontalAlignment.Center
                                        };
                                        Grid.SetRow(lbl1, 0);
                                        AdditionalPropertiesGrid.Children.Add(lbl1);

                                        var lbl2 = new Label
                                        {
                                            Foreground = Brushes.DimGray,
                                            Content =
                                                ((ObjectInfo) brd.Tag).Width + " м x " + ((ObjectInfo) brd.Tag).Height +
                                                " м",
                                            HorizontalContentAlignment = HorizontalAlignment.Center
                                        };
                                        Grid.SetRow(lbl2, 1);
                                        AdditionalPropertiesGrid.Children.Add(lbl2);

                                        AdditionalPropertiesBorder.Visibility = Visibility.Visible;
                                    }
                                }

                                if (obj is Polygon)
                                {
                                    var plgn = obj as Polygon;

                                    if (((ObjectInfo) plgn.Tag).ObjectId == objectId)
                                    {
                                        plgn.Fill.Opacity = 0.8;
                                        _selectedPolygon = plgn;
                                    }

                                    AdditionalPropertiesGrid.Children.Clear();
                                    AdditionalPropertiesBorder.Visibility = Visibility.Hidden;
                                }

                                if (obj is Polyline)
                                {
                                    var pln = obj as Polyline;
                                    if (((ObjectInfo) pln.Tag).ObjectId == objectId)
                                    {
                                        AdditionalPropertiesGrid.Children.Clear();
                                        var lbl1 = new Label
                                        {
                                            Foreground = Brushes.DimGray,
                                            Content = "Длина",
                                            Width = 100,
                                            HorizontalContentAlignment = HorizontalAlignment.Center
                                        };
                                        Grid.SetRow(lbl1, 0);
                                        AdditionalPropertiesGrid.Children.Add(lbl1);

                                        var lbl2 = new Label
                                        {
                                            Foreground = Brushes.DimGray,
                                            Content = Math.Round(GetPolylineLength(pln.Points)/_wmc.ScaleVar, 2) + " м",
                                            HorizontalContentAlignment = HorizontalAlignment.Center
                                        };
                                        Grid.SetRow(lbl2, 1);
                                        AdditionalPropertiesGrid.Children.Add(lbl2);

                                        AdditionalPropertiesBorder.Visibility = Visibility.Visible;
                                        SelectedMachineNameLabel.Content = ((ObjectInfo)pln.Tag).ObjectName;
                                    }
                                }
                            }
                        }
                }

                #endregion
            }
        }

        private void NewObjectTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObjectPropetiesGrid.Children.Count != 0)
                ObjectPropetiesGrid.Children.Clear();

            int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

            switch (objectTypeId)
            {
                case 1:
                    if (RectanglePropertiesGrid.Parent == null)
                        ObjectPropetiesGrid.Children.Add(RectanglePropertiesGrid);
                    AddNewObjectButton.Content = "Добавить";
                    break;
                case 2:
                    if (PolygonPropertiesGrid.Parent == null)
                        ObjectPropetiesGrid.Children.Add(PolygonPropertiesGrid);
                    AddNewObjectButton.Content = "Указать на карте";
                    break;
                case 3:
                    AddNewObjectButton.Content = "Указать на карте";
                    break;
            }
        }

        private void NewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLayerCanvas == null) return;

            ShowMenuGrid(NewObjecttBorder);

            AddNewObjectButton.IsEnabled = true;

            NewObjectTypeComboBox_SelectionChanged(null, null);
        }

        private void CancelNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            HideMenuGrid();
        }

        private void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedValue == null) return;

            if (NewObjectNameTextBox.Text.Trim() == string.Empty) return;

            AddNewLayerButton.IsEnabled = false;

            int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

            int layerId = Convert.ToInt32(LayersListBox.SelectedValue);

             if (!_wmc.CheckNewObjectName(layerId, NewObjectNameTextBox.Text.Trim())) return;
                

            if (objectTypeId == 1)
            {
                double rectWidth;
                if (!Double.TryParse(NewRectWidthTextBox.Text.Trim().Replace(".", ","), out rectWidth)) return;

                double rectHeight;
                if (!Double.TryParse(NewRectHeightTextBox.Text.Trim().Replace(".", ","), out rectHeight)) return;

                _wmc.AddNewObject(layerId, objectTypeId, NewObjectColorColorPicker.SelectedValue,
                    NewObjectNameTextBox.Text, NewObjectDescriptionTextBox.Text, rectWidth, rectHeight);
                AdministrationClass.AddNewAction(86);

                HideMenuGrid();

                RedrawLayer(layerId);
            }
            else if (objectTypeId == 2 || objectTypeId == 3)
            {
                MenuGrid.IsEnabled = false;
                NewObjectControlGrid.Visibility = Visibility.Visible;
                _isLockLayers = true;
                _isCanMoveMap = false;

                _editLock = true;

                _currentLayerCanvas.BringToFront();

                _currentLayerCanvas.Background = Brushes.Transparent;

                _currentLayerCanvas.PreviewMouseLeftButtonDown += CurrentLayerCanvas_MouseLeftButtonDown;
                _currentLayerCanvas.PreviewMouseRightButtonDown += CurrentLayerCanvas_MouseRightButtonDown;
                _currentLayerCanvas.PreviewMouseMove += CurrentLayerCanvas_MouseMove;
            }

            //else if (objectTypeId == 3)
            //{
            //    MenuGrid.IsEnabled = false;
            //    NewObjectControlGrid.Visibility = Visibility.Visible;
            //    _isLockLayers = true;
            //    _isCanMoveMap = false;
            //}
        }

        private void CancelDrawNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            _editLock = false;
            MenuGrid.IsEnabled = true;
            NewObjectControlGrid.Visibility = Visibility.Hidden;
            _isLockLayers = false;
            _isCanMoveMap = true;

            if (_currentLayerCanvas != null)
            {
                _currentLayerCanvas.Background = null;

                _currentLayerCanvas.Children.Remove(_tempPolygon);
                _currentLayerCanvas.Children.Remove(_tempPolyline);
                _currentLayerCanvas.Children.Remove(_tempTextBlock);

                newObjectPoints.Clear();

                //if (objectTypeId == 2)
                //{
                _currentLayerCanvas.PreviewMouseLeftButtonDown -= CurrentLayerCanvas_MouseLeftButtonDown;
                _currentLayerCanvas.PreviewMouseRightButtonDown -= CurrentLayerCanvas_MouseRightButtonDown;
                _currentLayerCanvas.PreviewMouseMove -= CurrentLayerCanvas_MouseMove;
            }
            //}
        }

        private void ClearDrawNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

            switch (objectTypeId)
            {
                case 2:
                {
                    if (_tempPolygon != null)
                    {
                        _currentLayerCanvas.Children.Remove(_tempPolygon);

                        newObjectPoints.Clear();
                    }
                }
                    break;
                case 3:
                {
                    if (_tempPolyline != null)
                    {
                        _currentLayerCanvas.Children.Remove(_tempPolyline);
                        _currentLayerCanvas.Children.Remove(_tempTextBlock);
                        newObjectPoints.Clear();
                    }
                }
                    break;
            }
        }

        private void SaveNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            _editLock = false;
            int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);
            int layerId = Convert.ToInt32(LayersListBox.SelectedValue);

            string colorObjStr = NewObjectColorColorPicker.SelectedValue.ToString();
            var colorObj = (Brush) new BrushConverter().ConvertFrom(colorObjStr);
            colorObj.Opacity = 0.9;

            if (objectTypeId == 2)
            {
                if (! _wmc.AddNewObject(layerId, objectTypeId, colorObjStr, NewObjectNameTextBox.Text,
                    NewObjectDescriptionTextBox.Text, GetPolygonPoints(newObjectPoints),
                    IsFillCheckBox.IsChecked == true)) return;
            }
            else if (objectTypeId == 3)
            {
                if (!
                    _wmc.AddNewObject(layerId, objectTypeId, colorObjStr, NewObjectNameTextBox.Text,
                        NewObjectDescriptionTextBox.Text, GetPolygonPoints(newObjectPoints))) return;
            }

            _currentLayerCanvas.Background = null;

            MenuGrid.IsEnabled = true;
            NewObjectControlGrid.Visibility = Visibility.Hidden;
            _isLockLayers = false;
            _isCanMoveMap = true;

            RedrawLayer(layerId);

            HideMenuGrid();

            if (newObjectPoints != null) newObjectPoints.Clear();
        }

        private byte[] GetPolygonPoints(PointCollection pc)
        {
            var serializer = new XmlSerializer(typeof (PointCollection));
            byte[] raw;

            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, pc);
                raw = ms.ToArray();
            }

            return raw;
        }

        private void CurrentLayerCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = Mouse.GetPosition(_currentLayerCanvas);

                int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

                if (newObjectPoints.Count != 0)
                {
                    Point p1 = newObjectPoints[newObjectPoints.Count - 1];

                    if (objectTypeId == 2)
                    {
                        if (_only90)
                        {
                            if (Math.Abs(p.X - p1.X) < Math.Abs(p.Y - p1.Y))
                                p.X = p1.X;
                            else
                                p.Y = p1.Y;
                        }
                    }
                    else
                    {
                        if (Math.Abs(p.X - p1.X) < Math.Abs(p.Y - p1.Y))
                            p.X = p1.X;
                        else
                            p.Y = p1.Y;
                    }
                }

                newObjectPoints.Add(p);

                string colorObjStr = NewObjectColorColorPicker.SelectedValue.ToString();



                switch (objectTypeId)
                {
                    case 2:
                        DrawPolygon(newObjectPoints, false, colorObjStr);
                        break;
                    case 3:
                        DrawPolyline(newObjectPoints, false, colorObjStr);
                        break;
                }
            }
        }

        private void CurrentLayerCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {

                if (newObjectPoints.Count > 0)
                {
                    int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

                    switch (objectTypeId)
                    {
                        case 2:
                        {
                            if (_tempPolygon != null)
                            {
                                _currentLayerCanvas.Children.Remove(_tempPolygon);

                                newObjectPoints.Clear();
                            }
                        }
                            break;
                        case 3:
                        {
                            if (_tempPolyline != null)
                            {
                                _currentLayerCanvas.Children.Remove(_tempPolyline);
                                _currentLayerCanvas.Children.Remove(_tempTextBlock);
                                newObjectPoints.Clear();
                            }
                        }
                            break;
                    }
                }
            }
        }

        private void CurrentLayerCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = Mouse.GetPosition(_currentLayerCanvas);

                int objectTypeId = Convert.ToInt32(NewObjectTypeComboBox.SelectedValue);

                if (newObjectPoints.Count > 1)
                {
                    Point p1 = newObjectPoints[newObjectPoints.Count - 1];

                    if (objectTypeId == 2)
                    {
                        if (_only90)
                        {
                            if (Math.Abs(p.X - p1.X) < Math.Abs(p.Y - p1.Y))
                                p.X = p1.X;
                            else
                                p.Y = p1.Y;
                        }
                    }
                    else
                    {
                        if (Math.Abs(p.X - p1.X) < Math.Abs(p.Y - p1.Y))
                            p.X = p1.X;
                        else
                            p.Y = p1.Y;
                    }

                    newObjectPoints[newObjectPoints.Count - 1] = p;
                }

                string colorObjStr = NewObjectColorColorPicker.SelectedValue.ToString();

                switch (objectTypeId)
                {
                    case 2:
                        DrawPolygon(newObjectPoints, false, colorObjStr);
                        break;
                    case 3:
                        DrawPolyline(newObjectPoints, false, colorObjStr);
                        break;
                }
            }
        }

        private void DrawPolygon(PointCollection pc, bool showPoints, string colorObjStr, object tag = null)
        {
            if (_currentLayerCanvas == null) return;

            if (_tempPolygon != null) _currentLayerCanvas.Children.Remove(_tempPolygon);

            _tempPolygon = new Polygon
            {
                SnapsToDevicePixels = true,
                Stroke = (Brush) new BrushConverter().ConvertFrom(colorObjStr),
                StrokeThickness = 0.3,
                Points = pc
            };

            _currentLayerCanvas.Children.Add(_tempPolygon);

            if (IsFillCheckBox.IsChecked == true)
            {
                _tempPolygon.Fill = (Brush) new BrushConverter().ConvertFrom(colorObjStr);
                _tempPolygon.Fill.Opacity = 0.5;
            }

            _tempPolygon.Tag = tag;

            if (!showPoints) return;

            foreach (Point rPoint in newObjectPoints)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Black);
                rect.Fill = new SolidColorBrush(Colors.Black);
                rect.Width = 1.2;
                rect.Height = 1.2;

                rect.Tag = newObjectPoints.IndexOf(rPoint);
                Canvas.SetLeft(rect, rPoint.X - 0.6);
                Canvas.SetTop(rect, rPoint.Y - 0.6);
                _currentLayerCanvas.Children.Add(rect);
            }
        }

        private void DrawPolyline(PointCollection pc, bool showPoints, string colorObjStr)
        {
            if (_currentLayerCanvas == null) return;

            if (_tempPolyline != null)
            {
                //_currentLayerCanvas.Children.Remove(_vb);

                _currentLayerCanvas.Children.Remove(_tempPolyline);
                _currentLayerCanvas.Children.Remove(_tempTextBlock);

            }

            _tempPolyline = new Polyline
            {
                SnapsToDevicePixels = true,
                Stroke = (Brush) new BrushConverter().ConvertFrom(colorObjStr),
                StrokeThickness = 0.2,
                Points = pc
            };

            _currentLayerCanvas.Children.Add(_tempPolyline);

            double length = Math.Round(GetPolylineLength(pc)/_wmc.ScaleVar, 2);

            if (pc.Count != 0)
            {

                _tempTextBlock = new TextBlock
                {
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    SnapsToDevicePixels = true,
                    Text = length + " м",
                    Foreground = (Brush) new BrushConverter().ConvertFrom(colorObjStr),
                    FontWeight = FontWeights.Normal,
                    FontSize = 8,
                };

                Canvas.SetLeft(_tempTextBlock, pc[0].X - 5);
                Canvas.SetTop(_tempTextBlock, pc[0].Y - 5);

                _currentLayerCanvas.Children.Add(_tempTextBlock);
            }

            if (!showPoints) return;

            foreach (Point rPoint in newObjectPoints)
            {
                var rect = new Rectangle
                {
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = new SolidColorBrush(Colors.Black),
                    Width = 1.2,
                    Height = 1.2,
                    Tag = newObjectPoints.IndexOf(rPoint)
                };

                Canvas.SetLeft(rect, rPoint.X - 0.6);
                Canvas.SetTop(rect, rPoint.Y - 0.6);

                _currentLayerCanvas.Children.Add(rect);
            }
        }

        private double GetPolylineLength(PointCollection pc)
        {
            double length = 0;
            for (int i = 0; i < pc.Count - 1; i++)
            {
                length += Math.Sqrt(Math.Pow((pc[i + 1].X - pc[i].X), 2) + Math.Pow((pc[i + 1].Y - pc[i].Y), 2));
            }
            return length;
        }

        private void DeleteObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayersObjectsListBox.SelectedItem == null) return;

            int layerId = Convert.ToInt32(LayersListBox.SelectedValue);
            int objectId = Convert.ToInt32(LayersObjectsListBox.SelectedValue);

            if (_wmc.DeleteLayerObject(objectId, ((DataRowView) LayersObjectsListBox.SelectedItem)["Name"].ToString()))
            {
                AdministrationClass.AddNewAction(87);

                RedrawLayer(layerId);

                LayersObjectsListBox.Items.MoveCurrentToFirst();
            }
        }
        
        private void NewResponsiblePersonButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuGrid(NewResponsiblePersonBorder);
        }

        private void CancelResponsiblePersonsButton_Click(object sender, RoutedEventArgs e)
        {
            HideMenuGrid();
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
            var cv = new BindingListCollectionView(staffDataTable.DefaultView);

            //if (staffDataTable != null)
            //{
            //    cv = new BindingListCollectionView(staffDataTable.DefaultView);

            //    if (cv.GroupDescriptions != null)
            //    {
            //        cv.GroupDescriptions.Add(new PropertyGroupDescription("Name", new FirstLetterConverter()));
            //        cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            //    }
            //}

            WorkersNamesListBox.ItemsSource = cv;
            WorkersNamesListBox.SelectedIndex = 0;
            //WorkersNamesListBox_SelectionChanged(null, null);
        }

        private void AddResponsiblePersonButton_Click(object sender, RoutedEventArgs e)
        {
            int layerId = Convert.ToInt32(LayersListBox.SelectedValue);
            int objectId = Convert.ToInt32(LayersObjectsListBox.SelectedValue);
            int workerId = Convert.ToInt32(WorkersNamesListBox.SelectedValue);
            int responsibleTypeId = Convert.ToInt32(ResponsibleTypesComboBox.SelectedValue);


            _wmc.AddResponsiblePerson(layerId, objectId, workerId, responsibleTypeId);

            HideMenuGrid();
        }

        private void DeleteResponsiblePersonButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResponsiblePersonsListBox.SelectedItem != null)
            {
                ((DataRowView) ResponsiblePersonsListBox.SelectedItem)["IsEnabled"] = false;
                _wmc.SaveResponsiblePersons();
            }
        }

        private void Only90CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _only90 = (Only90CheckBox.IsChecked == true);
        }

        private void Only90CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _only90 = (Only90CheckBox.IsChecked == true);
        }

        //private void VisabilityLayersEditorButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var da = new DoubleAnimation {Duration = new Duration(TimeSpan.FromSeconds(0.2))};

        //    if (EditorGrid.Width == 1)
        //    {
        //        da.From = 1;
        //        da.To = 450;

        //        VisabilityLayersEditorButton.Content = ">";
        //    }
        //    else
        //    {
        //        da.From = 450;
        //        da.To = 1;

        //        VisabilityLayersEditorButton.Content = "<";
        //    }

        //    EditorGrid.BeginAnimation(WidthProperty, da);
        //}


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        #region FileStorage

        #region Uploading

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem == null ||
                (!Convert.ToBoolean(ShowLayersFilesToggleButton.IsChecked) &&
                 !Convert.ToBoolean(ShowObjectsFilesToggleButton.IsChecked)) ||
                (Convert.ToBoolean(ShowObjectsFilesToggleButton.IsChecked) && LayersObjectsListBox.SelectedItem == null))
                return;

            var ofd = new OpenFileDialog
                      {
                          Filter = "All files (*.*)|*.*",
                          FilterIndex = 1,
                          RestoreDirectory = true,
                          Multiselect = true
                      };

            if (ofd.ShowDialog().Value)
            {
                try
                {
                    UploadFiles(ofd.FileNames);
                }
                catch (Exception ex)
                {
                    MetroMessageBox.Show("Невозможно найти файл: " + ex.Message, "Ошибка");
                }
            }
        }

        private void UploadFiles(IEnumerable<string> filePaths)
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            if (!_ftpClient.DirectoryExist(_ftpClient.CurrentPath))
                _ftpClient.MakeDirectory(_ftpClient.CurrentPath);

            foreach (var filePath in filePaths)
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                var adress = string.Concat(_ftpClient.CurrentPath, "/", fileName);

                if (_ftpClient.FileExist(adress))
                {
                    if (MetroMessageBox.Show("Файл '" + fileName + "' уже существует в данном каталоге. \n\n" +
                                             "Заменить существующий файл?", string.Empty, MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                }

                var uri = new Uri(adress);

                if (_processWindow != null)
                    _processWindow.Close(true);

                _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                _processWindow.Show(Window.GetWindow(this), true);
                _ftpClient.UploadFileCompleted += OnFtpClientUploadFileCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
            }

            FillViews(_ftpClient.CurrentPath);

            AdministrationClass.AddNewAction(88);
        }

        private void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (_processWindow != null)
            {
                _processWindow.Progress = (e.BytesSent / (double)e.TotalBytesToSend) * 100;
                _processWindow.Text = string.Format("Загрузка файла... \n{0} кБ", e.BytesSent / 1024);
            }
        }

        private void OnFtpClientUploadFileCompleted(object sender, UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadFileCompleted;

            if (_processWindow != null)
                _processWindow.Close(true);
        }

        #endregion

        #region Opening

        private void OpenFile(FtpFileDirectoryInfo fileDirectoryInfo)
        {
            if (fileDirectoryInfo.IsDirectory) return;

            var filePath = System.IO.Path.Combine(App.TempFolder, fileDirectoryInfo.Name);

            if (File.Exists(filePath))
            {
                Process.Start(filePath);
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            _neededOpeningFilePath = filePath;
            var path = fileDirectoryInfo.Adress;
            var uri = new Uri(path);

            if (_processWindow != null)
                _processWindow.Close(true);

            _processWindow = new WaitWindow { Text = "Загрузка файла..." };
            _processWindow.Show(Window.GetWindow(this), true);

            _fileSize = _ftpClient.GetFileSize(fileDirectoryInfo.Adress);
            _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
            _ftpClient.DownloadFileAsync(uri, filePath);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.BytesReceived / (double)_fileSize;

            if (_processWindow != null)
            {
                _processWindow.Progress = progress * 100;
                _processWindow.Text = string.Format("Загрузка файла... \n{0} кБ", e.BytesReceived / 1024);
            }
        }

        private void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
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

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;
        }

        #endregion

        private void FillViews(string directoryPath)
        {
            ShowWaitAnnimation();

            if (_listLoadingThread != null && _listLoadingThread.IsAlive)
            {
                _listLoadingThread.Abort();
                _listLoadingThread.Join();
            }

            _ftpClient.CurrentPath = directoryPath;

            _listLoadingThread = new Thread(() =>
            {
                Thread.Sleep(300);
                var dirDetails = _ftpClient.ListDirectoryDetails().ToList();

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(
                    () =>
                    {
                        ExplorerListBox.ItemsSource = dirDetails;

                        HideWaitAnnimation();
                    }));
            });
            _listLoadingThread.SetApartmentState(ApartmentState.STA);
            _listLoadingThread.IsBackground = true;
            _listLoadingThread.Start();
        }

        private void ShowWaitAnnimation()
        {
            ShadowGrid.Children.Clear();
            ShadowGrid.Visibility = Visibility.Visible;
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ShadowGrid.Children.Add(stackPanel);
            var circularFadingLine = new CircularFadingLine();
            stackPanel.Children.Add(circularFadingLine);
            var textBlock = new TextBlock();
            stackPanel.Children.Add(textBlock);
        }

        private void HideWaitAnnimation()
        {
            ShadowGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Children.Clear();
        }

        private void FileButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LayersListBox.SelectedItem == null ||
                (Convert.ToBoolean(ShowObjectsFilesToggleButton.IsChecked) && LayersObjectsListBox.SelectedItem == null))
                return;

            var fileInfo = ((FrameworkElement) sender).DataContext as FtpFileDirectoryInfo;
            if (fileInfo == null) return;

            OpenFile(fileInfo);
        }

        private void FileButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FileButton_MouseDoubleClick(sender, null);
            }

            if (e.Key == Key.Delete)
            {
                DeleteFileButton_Click(null, null);
            }
        }

        private void DeleteSelectedRows(ICollection ftpFileInfoCollection)
        {
            if (ftpFileInfoCollection.Count <= 0) return;

            if (MetroMessageBox.Show(
                "Вы действительно хотите удалить эти обьекты (" + ftpFileInfoCollection.Count + "шт.)", string.Empty,
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            foreach (
                var fullPath in
                    from FtpFileDirectoryInfo ftpFileDirectoryInfo in ftpFileInfoCollection
                    select ftpFileDirectoryInfo.Adress)
            {
                _ftpClient.DeleteFile(fullPath);
            }

            FillViews(_ftpClient.CurrentPath);
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode) return;

            if (LayersListBox.SelectedItem == null || ExplorerListBox.SelectedItem == null ||
                (Convert.ToBoolean(ShowObjectsFilesToggleButton.IsChecked) && LayersObjectsListBox.SelectedItem == null))
                return;

            DeleteSelectedRows(ExplorerListBox.SelectedItems);
        }

        private void ShowLayersFilesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem == null)
            {
                ExplorerListBox.ItemsSource = null;
                return;
            }

            var renamedFileName = GetRenamedFileName(((DataRowView) LayersListBox.SelectedItem)["LayerName"].ToString());
            var path = string.Concat(_basicDirectory, renamedFileName, "/", "Файлы слоя", "/");
            _ftpClient.CurrentPath = path;
            FillViews(_ftpClient.CurrentPath);
        }

        private void ShowObjectsFilesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem == null || LayersObjectsListBox.SelectedItem == null)
            {
                ExplorerListBox.ItemsSource = null;
                return;
            }

            var layerId = Convert.ToInt32((((DataRowView) LayersListBox.SelectedItem)["LayerId"]));

            string fieldName;

            switch (layerId)
            {
                case 1:
                    fieldName = "MachineName";
                    break;
                case 2:
                    fieldName = "LockName";
                    break;
                default:
                    fieldName = "Name";
                    break;
            }

            var renamedLayerName = GetRenamedFileName(((DataRowView) LayersListBox.SelectedItem)["LayerName"].ToString());
            var renamedObjectName =
                GetRenamedFileName(((DataRowView) LayersObjectsListBox.SelectedItem)[fieldName].ToString());
            var path = string.Concat(_basicDirectory, renamedLayerName, "/", "Файлы объектов", "/", renamedObjectName, "/");
            _ftpClient.CurrentPath = path;
            FillViews(_ftpClient.CurrentPath);
        }

        private static string GetRenamedFileName(string fileName)
        {
            var renamedFileName = fileName;
            const string str = @"?:<>/|\""";
            foreach (var symbol in renamedFileName.Where(symbol => str.Any(s => s == symbol)))
            {
                renamedFileName = renamedFileName.Replace(symbol, '_');
            }
            return renamedFileName.Trim();
        }

        #endregion


        private void LayersObjectsTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                //if (_currentObject == null) return;

                //MapSlider.Value = MapSlider.Maximum;

                //if (_currentObject is Border)
                //{
                //    var brd = _currentObject as Border;

                //    MapScrollViewer.ScrollToVerticalOffset(brd.TranslatePoint(new Point(), MapCanvas).Y);// + (brd.ActualHeight / 2));
                //    MapScrollViewer.ScrollToHorizontalOffset(brd.TranslatePoint(new Point(), MapCanvas).X);// + (brd.ActualWidth / 2));
                //}

                //if (_currentObject is Polygon)
                //{
                //    var plgn = _currentObject as Polygon;

                //    MapScrollViewer.ScrollToVerticalOffset(plgn.Points[0].Y + (plgn.ActualHeight / 2));
                //    MapScrollViewer.ScrollToHorizontalOffset(plgn.Points[0].X + (plgn.ActualWidth / 2));
                //}

                //if (_currentObject is Polyline)
                //{
                //    var pln = _currentObject as Polyline;

                //    MapScrollViewer.ScrollToVerticalOffset(pln.Points[0].Y);
                //    MapScrollViewer.ScrollToHorizontalOffset(pln.Points[0].X);
                //}
            }
        }

        private void AddNewLayerGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayerGroupNameTextBox.Text.Trim() == string.Empty) return;

            _wmc.AddNewLayerGroup(LayerGroupNameTextBox.Text);

            LayerGroupNameTextBox.Text = string.Empty;

            LayerGroupsListBox.SelectedIndex = 0;
        }

        private void DeleteLayerGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if(LayerGroupsListBox.SelectedValue == null && LayerGroupsListBox.SelectedItem == null) return;

            var drv = (DataRowView) LayerGroupsListBox.SelectedItem;

            if (MetroMessageBox.Show(
                   "Вы действительно хотите удалить '" + drv["LayerGroupName"] + "'", string.Empty,
                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                drv["IsEnabled"] = false;
            }
        }

        private void Path_UpLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var curButton = sender as Button;

            if (curButton == null) return;

            int layerId;

            if (!Int32.TryParse(curButton.Tag.ToString(), out layerId)) return;

            //LayersListBox.SelectionChanged -= LayersListBox_SelectionChanged;

            //LayersListBox.SelectedValue = layerId;

            //LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;



            HideLayerObjects(layerId);

            _currentLayerCanvas = null;
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                if (MapGrid.Children[i] is Canvas)
                {
                    if (Convert.ToInt32(((Canvas)MapGrid.Children[i]).Tag) == layerId)
                    {
                        _currentLayerCanvas = (Canvas)MapGrid.Children[i];
                    }
                }
            }



            if (layerId == 1)
            {
                ShowMachinesLayerObjects(layerId);

                LayersListBox_SelectionChanged(null, null);
                return;
            }

            if (layerId == 2)
            {
                ShowLockLayerObjects(layerId);

                LayersListBox_SelectionChanged(null, null);
                return;
            }

            ShowLayerObjects(layerId);

            _currentLayerCanvas = null;
            for (int i = 0; i < MapGrid.Children.Count; i++)
            {
                if (MapGrid.Children[i] is Canvas)
                {
                    if (Convert.ToInt32(((Canvas)MapGrid.Children[i]).Tag) == layerId)
                    {
                        _currentLayerCanvas = (Canvas)MapGrid.Children[i];
                    }
                }
            }

            LayersListBox_SelectionChanged(null, null);
        }

        //private void PrintMapButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveCanvas(MapCanvas, 96, "d:\\canvas.png");
        //}

        public static void SaveCanvas(ZoomableCanvas canvas, int dpi, string filename)
        {
            Size size = new Size(canvas.ActualWidth * 4, canvas.ActualHeight * 4);
            canvas.Measure(size);
            
            canvas.Arrange(new Rect(size));

            var c = new Canvas
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = Brushes.White
            };

            var rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth, //width 
                (int)canvas.ActualHeight, //height 
                dpi, //dpi x 
                dpi, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );

            rtb.Render(c);
            rtb.Render(canvas);

            SaveRTBAsPNG(rtb, filename);
        }

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (var stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        #region View

        private void AllCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            FilesCheckBox.IsChecked = AllCheckBox.IsChecked;
            LayersCheckBox.IsChecked = AllCheckBox.IsChecked;
            ObjectsCheckBox.IsChecked = AllCheckBox.IsChecked;
            ObjPropCheckBox.IsChecked = AllCheckBox.IsChecked;
            RespPersCheckBox.IsChecked = AllCheckBox.IsChecked;
        }

        private void FilesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (FilesCheckBox.IsChecked == true)
            {
                FilesPropGrid.RowDefinitions[2].Height = new GridLength(60);
                FilesGrid.Visibility = Visibility.Visible;
            }
            else
            {
                FilesPropGrid.RowDefinitions[2].Height = new GridLength(60, GridUnitType.Auto);
                FilesGrid.Visibility = Visibility.Collapsed;

                AllCheckBox.Unchecked -= AllCheckBox_CheckedChanged;
                AllCheckBox.IsChecked = false;
                AllCheckBox.Unchecked += AllCheckBox_CheckedChanged;
            }
        }

        private void LayersCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (LayersCheckBox.IsChecked == true)
            {
                PropGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                LayersBorder.Visibility = Visibility.Visible;
            }
            else
            {
                PropGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
                LayersBorder.Visibility = Visibility.Collapsed;

                AllCheckBox.Unchecked -= AllCheckBox_CheckedChanged;
                AllCheckBox.IsChecked = false;
                AllCheckBox.Unchecked += AllCheckBox_CheckedChanged;
            }
        }

        private void ObjectsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ObjectsCheckBox.IsChecked == true)
            {
                PropGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                LayersObjectsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                PropGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                LayersObjectsBorder.Visibility = Visibility.Collapsed;

                AllCheckBox.Unchecked -= AllCheckBox_CheckedChanged;
                AllCheckBox.IsChecked = false;
                AllCheckBox.Unchecked += AllCheckBox_CheckedChanged;
            }
        }



        private void ObjPropCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ObjPropCheckBox.IsChecked == true)
            {

                ObjectsEditorBorder.Visibility = Visibility.Visible;
            }
            else
            {

                ObjectsEditorBorder.Visibility = Visibility.Collapsed;

                AllCheckBox.Unchecked -= AllCheckBox_CheckedChanged;
                AllCheckBox.IsChecked = false;
                AllCheckBox.Unchecked += AllCheckBox_CheckedChanged;
            }
        }

        private void RespPersCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (RespPersCheckBox.IsChecked == true)
            {
                
                ResponsiblePersonsBorder.Visibility = Visibility.Visible;
            }
            else
            {
               
                ResponsiblePersonsBorder.Visibility = Visibility.Collapsed;

                AllCheckBox.Unchecked -= AllCheckBox_CheckedChanged;
                AllCheckBox.IsChecked = false;
                AllCheckBox.Unchecked += AllCheckBox_CheckedChanged;
            }
        }

        private void RefreshView()
        {
            FilesCheckBox_CheckedChanged(null, null);
            LayersCheckBox_CheckedChanged(null, null);
            ObjectsCheckBox_CheckedChanged(null, null);
            ObjPropCheckBox_CheckedChanged(null, null);
            RespPersCheckBox_CheckedChanged(null, null);
        }

        #endregion

        private void AddAccessListButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccessWorkersNamesListBox.SelectedItem == null) return;

            int workerId = Convert.ToInt32(AccessWorkersNamesListBox.SelectedValue);


            bool contains = false;
            foreach (DataRowView drv in ((DataView) AccessListBox.ItemsSource))
            {
                contains = Convert.ToInt32(drv["WorkerID"]) == workerId;
                if(contains) return;
            }

            if(!contains)  _wmc.AddWorkerToAccessList(workerId);
        }

        private void OKAccessButton_Click(object sender, RoutedEventArgs e)
        {
            AccessListGrid.Visibility = Visibility.Collapsed;
        }

        private void AccessWorkersGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (AccessWorkersGroupsComboBox.Items.Count == 0) return;

            AccessFactoriesComboBox.SelectionChanged -= AccessFactoriesComboBox_SelectionChanged;
            AccessFactoriesComboBox.SelectedIndex = 0;
            AccessFactoriesComboBox.SelectionChanged += AccessFactoriesComboBox_SelectionChanged;

            AccessFilterWorkers();
        }

        private void AccessFactoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccessFilterWorkers();
        }

        private void AccessFilterWorkers()
        {
            if (AccessWorkersGroupsComboBox.SelectedItem == null) return;
            if (AccessFactoriesComboBox.SelectedItem == null) return;

            var staffDataTable = _sc.FilterWorkers(Convert.ToInt32(AccessWorkersGroupsComboBox.SelectedValue),
                Convert.ToInt32(AccessFactoriesComboBox.SelectedValue));
            var cv = new BindingListCollectionView(staffDataTable.DefaultView);

            AccessWorkersNamesListBox.ItemsSource = cv;
            AccessWorkersNamesListBox.SelectedIndex = 0;
        }

        private void EditAccessList_Click(object sender, RoutedEventArgs e)
        {
            ((DataView) (AccessListBox.ItemsSource)).RowFilter = "LayerID = -1";
            AccessWorkersNamesListBox.Items.MoveCurrentToFirst();

            AccessListGrid.Visibility = Visibility.Visible;
        }
    }
}