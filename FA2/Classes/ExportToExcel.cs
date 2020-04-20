using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FA2.Converters;
using FAIIControlLibrary.CustomControls;
using Microsoft.Office.Interop.Excel;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using DataTable = System.Data.DataTable;
using Font = Microsoft.Office.Interop.Excel.Font;
using TimeSpanConverter = FA2.Converters.TimeSpanConverter;
using FA2.Classes.WorkerRequestsEnums;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class ExportToExcel
    {

        #region Variables_Declaration

        #region OperationStatisticsReport

        private static _Application _operationStatisticsApp;
        private static _Workbook _operationStatisticsWorkbook;
        private static _Worksheet _operationStatisticsWorksheet;
        private static Range _operationStatisticsRange;
        private static Font _operationStatisticsFont;

        private static DataGrid _operationStatisticsExportingDataGrid;
        //private string OperationStatistics_WorksheetName;

        private static int _operationStatisticsColumnCount;

        private static string _operationStatisticsWorkerName;
        private static string _operationStatisticsDate;
        private static ItemCollection _commonTimeOpStat;

        private const int StatOpTableIndent = 9;
        private static WaitWindow _operationStatisticsWaitWindow;

        #endregion

        #region ShiftStatisticsReport

        private static Microsoft.Office.Interop.Excel.Application _shiftStatisticsApp;
        private static Workbook _shiftStatisticsWorkbook;
        private static Worksheet _shiftStatisticsWorksheet;
        private static Range _shiftStatisticsRange;
        private static Font _shiftStatisticsFont;

        private static int _shiftStatisticsColumnCount;
        private static int _shiftStatisticsCurrentRow;

        private static bool _shiftStatisticsIsReporting;
        private static IEnumerable<int> _shiftStatisticsWorkerIds;

        private static TimeControlClass _shiftStatisticsTimeControlClass;
        private static CatalogClass _shiftStatisticsCatalogClass;

        private static IdToNameConverter _shiftStatisticsNameConverter;
        private static IdToFactoryConverter _shiftStatisticsFactoryConverter;
        private static IdToWorkUnitConverter _shiftStatisticsWorkUnitConverter;
        private static IdToWorkSectionConverter _shiftStatisticsSectionConverter;
        private static IdToWorkSubSectionConverter _shiftStatisticsSubSectionConverter;
        private static IdToWorkOperationConverter _shiftStatisticsWorkOperationConverter;
        private static MeasureUnitNameFromOperationIdConverter _shiftStatisticsMeasureNameConverter;

        private static WaitWindow _shiftStatisticsWaitWindow;

        #endregion

        #region CommonOperationStatisticsReport

        private static _Application _commonOperationStatisticsApp;
        private static _Workbook _commonOperationStatisticsWorkbook;
        private static _Worksheet _commonOperationStatisticsWorksheet;
        private static Range _commonOperationStatisticsRange;
        private static Font _commonOperationStatisticsFont;

        private static int _commonOperationStatisticsColumnCount;

        private static DataGrid _commonOperationStatisticsExportingDataGrid;
        //private string CommonOperationStatistics_WorksheetName;

        private static ItemCollection _commonTimeCommStat;

        private const int CommonStatTableIndent = 8;
        private static WaitWindow _commonOperationStatisticsWaitWindow;

        #endregion

        #region ProdRoomsScheduleReport

        private static _Application _scheduleApp;
        private static _Workbook _scheduleWorkbook;
        private static _Worksheet _scheduleWorksheet;
        private static Range _scheduleRange;
        private static Font _scheduleFont;

        private static DataGrid _scheduleOpeningDataGrid;
        private static DataGrid _scheduleClosingDataGrid;

        private static int _scheduleSelectedYear;
        private static int _scheduleSelectedMonth;
        private static string _scheduleSelectedMonthName;

        private static int _scheduleColumnsCount;
        private static int _scheduleStartOfClosingTable;

        #endregion

        #region ProdRoomsActualStatusReport

        private static bool _roomsActualStatusIsReporting;

        private static _Application _roomsActualStatusApp;
        private static _Workbook _roomsActualStatusWorkbook;
        private static _Worksheet _roomsActualStatusWorksheet;
        private static Range _roomsActualStatusRange;

        private static ProdRoomsClass _roomsActualStatusProdRoomsClass;
        private static DataTable _roomsActualStatusTable;

        private static WaitWindow _roomsActualStatusWaitWindow;
        private static IdToNameConverter _roomsActualStatusIdtoNameConverter;
        private static LockIDConverter _roomsActualStatuslockIdConverter;

        private const int RoomsActualStatusColumnsCount = 6;
        private static int _roomsActualStatusRowsCount;

        #endregion

        #region TimesheetReport

        private static _Application _timesheetApp;
        private static _Workbook _timesheetWorkbook;
        private static _Worksheet _timesheetWorksheet;
        private static Range _timesheetRange;

        private static Button _timesheetExportButton;
        private static Button _applyFilterTimeSheetButton;
        private static Button _calculateTimesheetStatButton;
        private static WrapPanel _absencesWrapPanel;
        private static int _selectedTimesheetYear;
        private static int _selectedTimesheetMonth;
        private static int _timeSheetColumnsCount;
        private static int _timeSheetItemsCount;
        private static ItemCollection _timeSheetDataGridItemsCollection;

        //private static DailyRateWorkingHoursConverter _dailyRateWorkingHoursConverter;
        private static IdToNameConverter _idToNameConverter;
        private static WorkerProfessionInfoConverter _workerProfessionInfoConverter;
        //private static AbsenceTypeConverter _absenceTypeConverter;

        private static WaitWindow _timesheetWaitWindow;

        #endregion

        #region ProductionScheduleReport

        private static _Application _productionScheduleApp;
        private static _Workbook _productionScheduleWorkbook;
        private static _Worksheet _productionScheduleWorksheet;
        private static Range _productionScheduleRange;

        private static DataView _prodScheduleView;
        private static int _prodScheduleVisibleColumnsCount;
        private static DateTime _prodScheduleSelectedDate;
        private static IdToWorkUnitConverter _workUnitConverter;

        #endregion

        #region ActionTypesReport

        private static Microsoft.Office.Interop.Excel.Application _actionTypesApp;
        private static Workbook _actionTypesWorkbook;
        private static Worksheet _actionTypesWorksheet;
        private static Range _actionTypesRange;

        private static BindingListCollectionView _actionTypesView;
        private const int ActionTypesColumns = 3;
        private const int ActionTypesStartRow = 2;
        private static int _actionTypesRowsCount;

        private static WaitWindow _actionTypesWaitWindow;

        #endregion

        #region MachineOperationsReport

        private static Microsoft.Office.Interop.Excel.Application _machineOperationsApp;
        private static Workbook _machineOperationsWorkbook;
        private static Worksheet _machineOperationsWorksheet;
        private static Range _machineOperationsRange;

        private static int _machineOperationsWorkerGroupId;
        private static int _machineOperationsFactoryId;
        private static bool _machineOperationsAvailableWorkUnits;
        private const int MachineOperationsStartRow = 4;
        private static int _machineOperationsRowsCount;

        private static CatalogClass _cc ;

        private static WaitWindow _machineOperationsWaitWindow;
        private static IdToWorkUnitConverter _machineOperationsWorkUnitConverter;
        private static IdToWorkSectionConverter _machineOperationsSectionConverter;
        private static IdToWorkSubSectionConverter _machineOperationsSubSectionConverter;
        private static IdToWorkOperationConverter _machineOperationsConverter;
        private static MeasureUnitNameFromOperationIdConverter _machineOperationsMeasureConverter;
        private static IdToWorkerGroupConverter _machineOperationsWorkerGroupConverter;
        private static IdToFactoryConverter _machineOperationsFactoryConverter;

        private static Control _machineOperationExportElement;

        #endregion

        #region AccessGroupReport

        private static Microsoft.Office.Interop.Excel.Application _accessGroupApp;
        private static Workbook _accessGroupWorkbook;
        private static Worksheet _accessGroupWorksheet;
        private static Range _accessGroupRange;

        private static AdministrationClass _accessGroupAdminClass;
        private static StaffClass _accessGroupSc;

        private static WaitWindow _accessGroupWaitWindow;
        private static IdToAccessGroupNameConverter _accessGroupNameConverter;

        private static int _accessGroupCurrentRow;
        private static int _accessGroupStartRow;

        private static Control _accessGroupExportedControl;

        #endregion

        #region ServiceEquipmentCrashStatisticsReport

        private static Microsoft.Office.Interop.Excel.Application _serviceEquipmentCrashStatisticsApp;
        private static Workbook _serviceEquipmentCrashStatisticsWorkbook;
        private static Worksheet _serviceEquipmentCrashStatisticsWorksheet;
        private static Range _serviceEquipmentCrashStatisticsRange;

        private static ServiceEquipmentClass _serviceEquipmentCrashStatisticsServiceEquipmentClass;
        private static TaskClass _serviceEquipmentCrashStatisticsTaskClass;

        private static IdToWorkSubSectionConverter _serviceEquipmentCrashStatisticsWorkSubSectionNameConverter;
        private static IdToFactoryConverter _serviceEquipmentCrashStatisticsFactoryNameConverter;
        private static IdToNameConverter _serviceEquipmentCrashStatisticsWorkerNameConverter;
        private static WaitWindow _serviceEquipmentCrashStatisticsWaitWindow;
        private static Control _serviceEquipmentCrashStatisticsExportedControl;
        private static int _serviceEquipmentCrashStatisticsWorkSubSectionId;

        private static int _serviceEquipmentCrashStatisticsCurrentRow;
        private static int _serviceEquipmentCrashStatisticsRowIndex;
        private static int _serviceEquipmentCrashStatisticsRowsCount;
        private static DateTime _serviceEquipmentCrashStattisticsCurrentDate;
        private const int ServiceEquipmentCrashStattisticsColumnsCount = 19;

        #endregion

        #region ServiceEquipmentDiagrammReport

        private static Microsoft.Office.Interop.Excel.Application _serviceEquipmentDiagrammApp;
        private static Workbook _serviceEquipmentDiagrammWorkbook;
        private static Worksheet _serviceEquipmentDiagrammWorksheet;
        private static Range _serviceEquipmentDiagrammRange;

        private static ServiceEquipmentClass _serviceEquipmentDiagrammServiceEquipmentClass;
        private static TaskClass _serviceEquipmentDiagrammTaskClass;

        private static IdToWorkSubSectionConverter _serviceEquipmentDiagrammWorkSubSectionNameConverter;
        private static IdToFactoryConverter _serviceEquipmentDiagrammFactoryNameConverter;
        private static IdToNameConverter _serviceEquipmentDiagrammWorkerNameConverter;
        private static WaitWindow _serviceEquipmentDiagrammWaitWindow;
        private static Control _serviceEquipmentDiagrammExportedControl;

        private static int _serviceEquipmentDiagrammCurrentRow;
        private static int _serviceEquipmentDiagrammRowIndex;
        private static int _serviceEquipmentDiagrammRowsCount;
        private const int ServiceEquipmentDiagrammTableColumnsCount = 8;
        private static int _serviceEquipmentDiagrammColumnsCount;

        #endregion

        #region TasksStatisticReport

        private static bool _tasksStatisticIsReporting;

        private static Microsoft.Office.Interop.Excel.Application _tasksStatisticApp;
        private static Workbook _tasksStatisticWorkbook;
        private static Worksheet _tasksStatisticWorksheet;
        private static Range _tasksStatisticRange;

        private static TaskClass _tasksStatisticTaskClass;

        private static IdToNameConverter _tasksStatisticWorkerNameConverter;
        private static TimeSpanConverter _tasksStatisticTimeSpanConverter;
        private static WaitWindow _tasksStatisticWaitWindow;

        private static int _tasksStatisticWorkerId;
        private static int _tasksStatisticCurrentRow;
        private static int _tasksStatisticRowIndex;
        private static int _tasksStatisticRowsCount;
        private const int TasksStatisticColumnsCount = 9;

        #endregion

        #region WorkerRequestsReport

        private static bool _workerRequestsIsReporting;

        private static Microsoft.Office.Interop.Excel.Application _workerRequestsApp;
        private static Workbook _workerRequestsWorkbook;
        private static Worksheet _workerRequestsWorksheet;
        private static Range _workerRequestsRange;

        private static WorkerRequestsClass _workerRequestsClass;
        private static WorkerRequestConverter _workerRequestsConverter;

        private static IdToNameConverter _workerRequestsWorkerNameConverter;
        private static WaitWindow _workerRequestsWaitWindow;

        private static DataView _workerRequestsView;
        private static int _workerRequestsCurrentRow;
        private static int _workerRequestsRowIndex;
        private static int _workerRequestsRowsCount;
        private const int WorkerRequestsColumnsCount = 12;

        #endregion

        #region WeekendResponsiblesReport

        private static bool _weekendResponsiblesIsReporting;

        private static Microsoft.Office.Interop.Excel.Application _weekendResponsiblesApp;
        private static Workbook _weekendResponsiblesWorkbook;
        private static Worksheet _weekendResponsiblesWorksheet;
        private static Range _weekendResponsiblesRange;

        private static ProdRoomsClass _weekendResponsiblesProdRoomsClass;
        private static DataView _weekendResponsiblesTimeSheetDataView;

        private static IdToNameConverter _weekendResponsiblesWorkerNameConverter;
        private static WaitWindow _weekendResponsiblesWaitWindow;

        private static int _weekendResponsiblesCurrentRow;
        private static int _weekendResponsiblesRowIndex;
        private static int _weekendResponsiblesRowsCount;
        private static int _weekendResponsiblesColumnsCount;
        private const int ResponsibleArrivesColumnsCount = 4;

        #endregion

        #region ProdRoomsResponsiblesReport

        private static bool _prodRoomsResponsiblesIsReporting;

        private static Microsoft.Office.Interop.Excel.Application _prodRoomsResponsiblesApp;
        private static Workbook _prodRoomsResponsiblesWorkbook;
        private static Worksheet _prodRoomsResponsiblesWorksheet;
        private static Range _prodRoomsResponsiblesRange;

        private static ProdRoomsClass _prodRoomsResponsiblesProdRoomsClass;

        private static IdToNameConverter _prodRoomsResponsiblesWorkerNameConverter;
        private static ActionsConverter _prodRoomsResponsiblesActionsConverter;
        private static WaitWindow _prodRoomsResponsiblesWaitWindow;

        private static int _prodRoomsResponsiblesCurrentRow;
        private static int _prodRoomsResponsiblesRowIndex;
        private static int _prodRoomsResponsiblesRowsCount;
        private const int ProdRoomsResponsiblesColumnsCount = 7;

        #endregion

        #region PlannedWorksReport

        private static bool _plannedWorksIsReporting;

        private static Microsoft.Office.Interop.Excel.Application _plannedWorksApp;
        private static Workbook _plannedWorksWorkbook;
        private static Worksheet _plannedWorksWorksheet;
        private static Range _plannedWorksRange;

        private static PlannedWorksClass _plannedWorksClass;
        private static TaskClass _plannedWorksTaskClass;

        private static IdToNameConverter _plannedWorksWorkerNameConverter;
        private static PlannedWorksConverter _plannedWorksConverter;
        private static TaskConverter _plannedWorksTaskConverter;
        private static TimeSpanConverter _plannedWorksTimeSpanConverter;
        private static WaitWindow _plannedWorksWaitWindow;

        private static int _plannedWorksCurrentRow;
        private static int _plannedWorksRowIndex;
        private static int _plannedWorksRowsCount;
        private const int PlannedWorksColumnsCount = 8;
        private const int StartedPlannedWorksColumnsCount = 9;

        #endregion

        #endregion



        #region OperationStatisticsReport

        public static void GenerateOperationStatisticsReport(ref DataGrid exportingDataGrid, string tWorkerName, string tDate,
            ItemCollection tCommonTimeOpStat)
        {
            _operationStatisticsWaitWindow = new WaitWindow();
            _operationStatisticsWaitWindow.Show(Application.Current.MainWindow);
            _operationStatisticsWaitWindow.Text = "Вывод информации в Excel";

            OperationStatistics_Initialize();

            _operationStatisticsExportingDataGrid = exportingDataGrid;

            _operationStatisticsWorkerName = tWorkerName;
            _operationStatisticsDate = tDate;
            _commonTimeOpStat = tCommonTimeOpStat;

            FillOperationStatisticsHeaders();

            SetOperationStatisticsStyle();

            FillOperationStatisticsData();

            OperationStatistics_AutoFitColumn();
            OperationStatistics_OpenReport();
            _operationStatisticsWaitWindow.Close(true);
        }

        private static void OperationStatistics_Initialize()
        {
            _operationStatisticsApp = new Microsoft.Office.Interop.Excel.Application();
            _operationStatisticsWorkbook = _operationStatisticsApp.Workbooks.Add(Type.Missing);
            _operationStatisticsWorksheet = _operationStatisticsWorkbook.ActiveSheet;
        }

        private static void OperationStatistics_AutoFitColumn()
        {
            _operationStatisticsRange = _operationStatisticsWorksheet.Rows;
            _operationStatisticsRange.Columns.AutoFit();

            Range er = _operationStatisticsWorksheet.get_Range("A:A", Type.Missing);
            er.EntireColumn.ColumnWidth = 15;

            er.NumberFormat = "MM-DD-YYYY HH:mm";
        }

        private static void OperationStatistics_OpenReport()
        {
            _operationStatisticsApp.Visible = true;

            Marshal.ReleaseComObject(_operationStatisticsWorkbook);
            Marshal.ReleaseComObject(_operationStatisticsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private static void FillOperationStatisticsHeaders()
        {
            _operationStatisticsWorksheet.Cells[1, 1] = _operationStatisticsWorkerName;
            _operationStatisticsWorksheet.Cells[1, 4] = _operationStatisticsDate;

            int i = 2;
            foreach (var item in _commonTimeOpStat)
            {
                _operationStatisticsWorksheet.Cells[i, 1] = item;
                i++;
            }

            int indexOfColumn = 1;

            foreach (
                DataGridColumn dataGridColumn in
                    _operationStatisticsExportingDataGrid.Columns.Where(
                        dataGridColumn => dataGridColumn.Visibility == Visibility.Visible))
            {
                _operationStatisticsWorksheet.Cells[StatOpTableIndent, dataGridColumn.DisplayIndex + 1] =
                    dataGridColumn.Header.ToString();

                _operationStatisticsColumnCount = indexOfColumn;
                indexOfColumn++;
            }

            SetOperationStatisticsHeadersStyle();
        }

        private static void FillOperationStatisticsData()
        {
            int visibleColumsCount = 0;

            for (var i = 0; i < _operationStatisticsExportingDataGrid.Items.Count; i++)
            {
                foreach (
                    DataGridColumn t in
                        _operationStatisticsExportingDataGrid.Columns.Where(t => t.Visibility == Visibility.Visible))
                {
                    visibleColumsCount++;

                    var cell = t.GetCellContent(_operationStatisticsExportingDataGrid.Items[i]);

                    var cellText = string.Empty;

                    var contentPresenter = cell as ContentPresenter;
                    if (contentPresenter != null)
                        if (contentPresenter.Content != null)
                        {
                            var txtblck =
                                contentPresenter.ContentTemplate.FindName("txtblck", contentPresenter) as TextBlock;
                            if (txtblck != null) cellText = txtblck.Text;
                        }

                    var indexIofCell = i + StatOpTableIndent + 1;

                    decimal cellNumbers;
                    var success = decimal.TryParse(cellText, out cellNumbers);

                    _operationStatisticsWorksheet.Cells[indexIofCell, t.DisplayIndex + 1] = success
                        ? (dynamic) cellNumbers
                        : cellText;
                }

                double percent = (double) i/_operationStatisticsExportingDataGrid.Items.Count;
                _operationStatisticsWaitWindow.Progress = percent*100;
                _operationStatisticsWaitWindow.Text = String.Format("Вывод данных в Excel {0}%", (int) (percent*100));
            }

            bool startRange = false;

            int rangeMinRow = 0;
            int k = StatOpTableIndent + 1;
            foreach (DataRowView drv in _operationStatisticsExportingDataGrid.Items)
            {
                int rangeMaxRow;
                if (drv["WorkUnitID"] == DBNull.Value && drv["TaskID"] == DBNull.Value && drv["Date"] != DBNull.Value)
                {
                    if (!startRange)
                    {
                        rangeMinRow = k;
                        startRange = true;
                    }
                    else
                    {
                        if (k != _operationStatisticsExportingDataGrid.Items.Count)
                            rangeMaxRow = k - 1;
                        else
                        {
                            rangeMaxRow = k;
                        }

                        _operationStatisticsRange =
                            _operationStatisticsWorksheet.Range[
                                _operationStatisticsWorksheet.Cells[rangeMinRow, 1],
                                _operationStatisticsWorksheet.Cells[rangeMaxRow, _operationStatisticsColumnCount]];
                        _operationStatisticsRange.BorderAround(XlLineStyle.xlContinuous,
                            XlBorderWeight.xlMedium,
                            XlColorIndex.xlColorIndexAutomatic,
                            ColorTranslator.ToOle(
                                Color.Black));

                        rangeMinRow = k;
                    }
                }

                if (drv["FactoryID"] == DBNull.Value)
                {
                    _operationStatisticsRange =
                        _operationStatisticsWorksheet.Range[
                            _operationStatisticsWorksheet.Cells[k, 1],
                            _operationStatisticsWorksheet.Cells[k, _operationStatisticsColumnCount]];
                    if (drv["TaskID"] != DBNull.Value)
                    {
                        _operationStatisticsWorksheet.Cells[k, 3] = "Задача";
                        _operationStatisticsRange.Font.Color = ColorTranslator.ToOle(Color.CornflowerBlue);
                    }
                    else if (drv["Date"] != DBNull.Value)
                    {
                        _operationStatisticsRange.Interior.Color = XlRgbColor.rgbLightGray;
                        _operationStatisticsFont = _operationStatisticsRange.Font;
                        _operationStatisticsFont.Bold = true;
                        _operationStatisticsFont.Italic = true;
                    }
                }

                k++;

                try
                {
                    if (k == _operationStatisticsExportingDataGrid.Items.Count)
                    {
                        rangeMaxRow = k + StatOpTableIndent;

                        object r2 = _operationStatisticsWorksheet.Cells[rangeMaxRow, _operationStatisticsColumnCount];

                        object r1 = _operationStatisticsWorksheet.Cells[rangeMinRow, 1];

                        _operationStatisticsRange = _operationStatisticsWorksheet.Range[r1, r2];

                        _operationStatisticsRange.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium,
                            XlColorIndex.xlColorIndexAutomatic, ColorTranslator.ToOle(Color.Black));
                    }
                }
                catch
                {
                    //MessageBox.Show(exp.Message);
                }
            }
        }

        private static void SetOperationStatisticsStyle()
        {
            _operationStatisticsRange =
                _operationStatisticsWorksheet.Range[
                    _operationStatisticsWorksheet.Cells[StatOpTableIndent, 1],
                    _operationStatisticsWorksheet.Cells[
                        _operationStatisticsExportingDataGrid.Items.Count + StatOpTableIndent,
                        _operationStatisticsColumnCount]];
            _operationStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
        }

        private static void SetOperationStatisticsHeadersStyle()
        {
            _operationStatisticsRange =
                _operationStatisticsWorksheet.Range[
                    _operationStatisticsWorksheet.Cells[1, 1],
                    _operationStatisticsWorksheet.Cells[1, _operationStatisticsColumnCount]];

            _operationStatisticsFont = _operationStatisticsRange.Font;
            _operationStatisticsFont.Bold = true;

            _operationStatisticsRange = _operationStatisticsWorksheet.Range[
                _operationStatisticsWorksheet.Cells[StatOpTableIndent, 1],
                _operationStatisticsWorksheet.Cells[StatOpTableIndent + 1, _operationStatisticsColumnCount]];

            _operationStatisticsFont = _operationStatisticsRange.Font;
            _operationStatisticsFont.Bold = true;

            _operationStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _operationStatisticsRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        #endregion

        #region ShiftStatisticsReport

        public static void GenerateShiftStatisticsReport(IEnumerable<int> workerIds)
        {
            if (_shiftStatisticsIsReporting) return;

            _shiftStatisticsIsReporting = true;
            _shiftStatisticsWorkerIds = workerIds;

            _shiftStatisticsWaitWindow = new WaitWindow();
            _shiftStatisticsWaitWindow.Show(Application.Current.MainWindow);
            _shiftStatisticsWaitWindow.Text = "Инициализация";

            ShiftStatisticsInitialize();
        }

        private static void ShiftStatisticsInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _shiftStatisticsNameConverter = new IdToNameConverter();
                _shiftStatisticsFactoryConverter = new IdToFactoryConverter();
                _shiftStatisticsWorkUnitConverter = new IdToWorkUnitConverter();
                _shiftStatisticsSectionConverter = new IdToWorkSectionConverter();
                _shiftStatisticsSubSectionConverter = new IdToWorkSubSectionConverter();
                _shiftStatisticsWorkOperationConverter = new IdToWorkOperationConverter();
                _shiftStatisticsMeasureNameConverter = new MeasureUnitNameFromOperationIdConverter();

                App.BaseClass.GetCatalogClass(ref _shiftStatisticsCatalogClass);
                App.BaseClass.GetTimeControlClass(ref _shiftStatisticsTimeControlClass);

                _shiftStatisticsApp = new Microsoft.Office.Interop.Excel.Application();
                _shiftStatisticsWorkbook = _shiftStatisticsApp.Workbooks.Add(Type.Missing);

                var factories = _shiftStatisticsCatalogClass.GetFactories();
                factories.Sort = "FactoryID DESC";
                foreach (DataRowView factory in factories)
                {
                    var factoryId = Convert.ToInt32(factory["FactoryID"]);
                    _shiftStatisticsCurrentRow = 1;
                    _shiftStatisticsWorksheet = _shiftStatisticsWorkbook.Sheets.Add();
                    _shiftStatisticsWorksheet.Name =
                        _shiftStatisticsFactoryConverter.Convert(factoryId, typeof(string), null, CultureInfo.InvariantCulture).ToString();

                    FillShiftStatisticsHeaders();
                    FillShiftStatisticsData(factoryId);

                    SetShiftStatisticsStyle();
                    SetShiftStatisticsColumnsWidth();
                    ShiftStatisticsFreezePanes();
                }

                ShiftStatisticsOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _shiftStatisticsIsReporting = false;
                _shiftStatisticsWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillShiftStatisticsHeaders()
        {
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 1, _shiftStatisticsCurrentRow, 7);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = 
                "с " + _shiftStatisticsTimeControlClass.GetDateFrom().ToString("dd.MM.yyyy") + " по " + _shiftStatisticsTimeControlClass.GetDateTo().ToString("dd.MM.yyyy");

            _shiftStatisticsCurrentRow++;

            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 1, _shiftStatisticsCurrentRow + 1, 1);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = "№";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 2, _shiftStatisticsCurrentRow + 1, 2);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 2] = "Участок";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 3, _shiftStatisticsCurrentRow + 1, 3);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 3] = "Подучасток";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 4, _shiftStatisticsCurrentRow + 1, 4);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 4] = "Станок";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 5, _shiftStatisticsCurrentRow + 1, 5);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 5] = "Выполняемые операции";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 6, _shiftStatisticsCurrentRow + 1, 6);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 6] = "Ед. изм.";
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 7, _shiftStatisticsCurrentRow + 1, 7);
            _shiftStatisticsRange.Merge();
            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 7] = "Норма";

            _shiftStatisticsColumnCount = 7;

            var fromDate = _shiftStatisticsTimeControlClass.GetDateFrom();
            var toDate = _shiftStatisticsTimeControlClass.GetDateTo();
            for(var startDate = fromDate; startDate <= toDate; startDate = startDate.AddDays(1))
            {
                _shiftStatisticsColumnCount++;
                _shiftStatisticsRange = 
                    GetShiftStatisticsRange(_shiftStatisticsCurrentRow, _shiftStatisticsColumnCount, _shiftStatisticsCurrentRow, _shiftStatisticsColumnCount + 2);
                _shiftStatisticsRange.Merge();
                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, _shiftStatisticsColumnCount] = startDate.ToString("dd MM");
                _shiftStatisticsRange.NumberFormat = "@";

                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow + 1, _shiftStatisticsColumnCount] = "V";
                _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[_shiftStatisticsColumnCount];
                _shiftStatisticsRange.ColumnWidth = 10;

                _shiftStatisticsColumnCount++;
                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow + 1, _shiftStatisticsColumnCount] = "Часы факт.";
                _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[_shiftStatisticsColumnCount];
                _shiftStatisticsRange.ColumnWidth = 10;

                _shiftStatisticsColumnCount++;
                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow + 1, _shiftStatisticsColumnCount] = "КТУ";
                _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[_shiftStatisticsColumnCount];
                _shiftStatisticsRange.ColumnWidth = 10;
            }

            SetShiftStatisticsHeadersStyle();

            _shiftStatisticsCurrentRow++;
        }

        private static void SetShiftStatisticsHeadersStyle()
        {
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 1, _shiftStatisticsCurrentRow + 1, _shiftStatisticsColumnCount);
            _shiftStatisticsFont = _shiftStatisticsRange.Font;
            _shiftStatisticsFont.Bold = true;
            _shiftStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _shiftStatisticsRange.Borders.Weight = XlBorderWeight.xlMedium;
            _shiftStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _shiftStatisticsRange.VerticalAlignment = Constants.xlCenter;
            
            _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 1, _shiftStatisticsCurrentRow + 1, 7);
            _shiftStatisticsRange.Interior.Color = XlRgbColor.rgbLightGray;
        }

        private static void FillShiftStatisticsData(int factoryId)
        {
            _shiftStatisticsCurrentRow++;
            var factoryName = _shiftStatisticsFactoryConverter.Convert(factoryId, typeof(string), null, CultureInfo.InvariantCulture).ToString();

            foreach(var workerId in _shiftStatisticsWorkerIds)
            {
                var workersTimeTrackingStatistics = GetShiftTimeTrackingStatistics(workerId);

                var workerName = _shiftStatisticsNameConverter.Convert(workerId, "FullName");
                _shiftStatisticsRange = GetShiftStatisticsRange(_shiftStatisticsCurrentRow, 1, _shiftStatisticsCurrentRow, 7);
                _shiftStatisticsRange.Interior.Color = XlRgbColor.rgbLightSkyBlue;
                _shiftStatisticsRange.Font.Bold = true;
                _shiftStatisticsRange.Merge();
                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = workerName;
                _shiftStatisticsCurrentRow++;

                var i = 0;

                var workUnitsView = GetShiftWorkUnits(2, factoryId);
                var workUnitIndex = 0;
                foreach (var workUnit in workUnitsView)
                {
                    workUnitIndex++;
                    var startWorkUnitRow = _shiftStatisticsCurrentRow;
                    var workUnitId = Convert.ToInt32(workUnit["WorkUnitID"]);
                    var workUnitName = _shiftStatisticsWorkUnitConverter.Convert(workUnitId, typeof(string), null, CultureInfo.InvariantCulture);

                    var sectionsView = GetShiftWorkSections(workUnitId);

                    foreach (var workSection in sectionsView)
                    {
                        var startWorkSectionRow = _shiftStatisticsCurrentRow;
                        var workSectionId = Convert.ToInt32(workSection["WorkSectionID"]);
                        var workSectionName = _shiftStatisticsSectionConverter.Convert(workSectionId, typeof(string), null, CultureInfo.InvariantCulture);

                        var subsectionsView = GetShiftWorkSubsections(workSectionId);
                        if(!subsectionsView.Any())
                        {
                            _shiftStatisticsCurrentRow++;
                        }

                        foreach (var workSubsection in subsectionsView)
                        {
                            var startWorkSubsectionRow = _shiftStatisticsCurrentRow;
                            var workSubsectionId = Convert.ToInt32(workSubsection["WorkSubsectionID"]);
                            var workSubsectionName = _shiftStatisticsSubSectionConverter.Convert(workSubsectionId, typeof(string), null, CultureInfo.InvariantCulture);

                            var workOperationsView = GetShiftWorkOperations(workSubsectionId);
                            if (!workOperationsView.Any())
                            {
                                i++;
                                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = i;
                                _shiftStatisticsCurrentRow++;
                            }

                            foreach (var operationRow in workOperationsView)
                            {
                                i++;
                                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = i;

                                var operationId = Convert.ToInt32(operationRow["WorkOperationID"]);
                                _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 5] =
                                    _shiftStatisticsWorkOperationConverter.Convert(operationId, typeof(string), null, new CultureInfo("ru-RU"));
                                var machineOperations =
                                    _shiftStatisticsCatalogClass.MachinesOperationsDataTable.AsEnumerable().Where(r => r.Field<Int64>("WorkOperationID") == operationId);
                                if (machineOperations.Any())
                                {
                                    var machineOperation = machineOperations.First();
                                    var productivity = machineOperation["Productivity"];
                                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 7] = productivity;

                                    var measureName = _shiftStatisticsMeasureNameConverter.Convert(operationId, typeof(string), "MeasureUnitName", new CultureInfo("ru-RU"));
                                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 6] = measureName;
                                }
                                else
                                {
                                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 6] = "";
                                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 7] = 0;
                                }

                                var fromDate = _shiftStatisticsTimeControlClass.GetDateFrom();
                                var toDate = _shiftStatisticsTimeControlClass.GetDateTo();

                                var j = 8;
                                for (var startDate = fromDate; startDate <= toDate; startDate = startDate.AddDays(1))
                                {
                                    var shiftInfoForOperationRows = 
                                        workersTimeTrackingStatistics.
                                            Where(r => r.Field<Int64>("WorkOperationID") == operationId && r.Field<DateTime>("WorkDayTimeStart").Date == startDate.Date);
                                    if(shiftInfoForOperationRows.Any())
                                    {
                                        var totalTime = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("TotalTime"));
                                        var totalWorkScope = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("WorkScope"));
                                        var totalVCLP = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("VCLP"));

                                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j] = totalWorkScope;
                                        _shiftStatisticsColumnCount++;
                                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j + 1] = totalTime;
                                        _shiftStatisticsColumnCount++;
                                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j + 2] = totalVCLP;
                                    }

                                    j += 3;
                                }

                                _shiftStatisticsCurrentRow++;
                            }

                            _shiftStatisticsRange = GetShiftStatisticsRange(startWorkSubsectionRow, 4, _shiftStatisticsCurrentRow - 1, 4);
                            _shiftStatisticsRange.Merge();
                            _shiftStatisticsWorksheet.Cells[startWorkSubsectionRow, 4] = workSubsectionName;
                        }

                        _shiftStatisticsRange = GetShiftStatisticsRange(startWorkSectionRow, 3, _shiftStatisticsCurrentRow - 1, 3);
                        _shiftStatisticsRange.Merge();
                        _shiftStatisticsWorksheet.Cells[startWorkSectionRow, 3] = workSectionName;
                        _shiftStatisticsRange.Orientation = workSectionName.ToString().Length > 10
                            ? subsectionsView.Count() > 5 
                                ? 90 : 0 
                            : 0;
                    }

                    _shiftStatisticsRange = GetShiftStatisticsRange(startWorkUnitRow, 2, _shiftStatisticsCurrentRow - 1, 2);
                    _shiftStatisticsRange.Merge();
                    _shiftStatisticsWorksheet.Cells[startWorkUnitRow, 2] = workUnitName;
                    _shiftStatisticsRange.Orientation = workUnitName.ToString().Length > 10
                        ? 90 : 0;

                    var percent = (double)workUnitIndex / workUnitsView.Count();
                    Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        _shiftStatisticsWaitWindow.Progress =
                            percent * 100;
                        _shiftStatisticsWaitWindow.Text =
                            string.Format(
                                "Вывод данных в Excel {0}% \n(Сотрудник: {1} Фабрика: {2})",
                                (int)(percent * 100), workerName, factoryName);
                    }));
                }

                var startAdditionalOperationsRow = _shiftStatisticsCurrentRow;                
                var additionalWorkOperationsView = GetShiftWorkOperations(-1);
                if(!additionalWorkOperationsView.Any())
                {
                    i++;
                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = i;
                    _shiftStatisticsCurrentRow++;
                }

                foreach(var additionalOperation in additionalWorkOperationsView)
                {
                    i++;
                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 1] = i;

                    var operationId = Convert.ToInt32(additionalOperation["WorkOperationID"]);
                    _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 5] =
                        _shiftStatisticsWorkOperationConverter.Convert(operationId, typeof(string), null, new CultureInfo("ru-RU"));
                    var machineOperations =
                        _shiftStatisticsCatalogClass.MachinesOperationsDataTable.AsEnumerable().Where(r => r.Field<Int64>("WorkOperationID") == operationId);
                    if (machineOperations.Any())
                    {
                        var machineOperation = machineOperations.First();
                        var productivity = machineOperation["Productivity"];
                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 7] = productivity;

                        var measureName = _shiftStatisticsMeasureNameConverter.Convert(operationId, typeof(string), "MeasureUnitName", new CultureInfo("ru-RU"));
                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 6] = measureName;
                    }
                    else
                    {
                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 6] = "";
                        _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, 7] = 0;
                    }

                    var fromDate = _shiftStatisticsTimeControlClass.GetDateFrom();
                    var toDate = _shiftStatisticsTimeControlClass.GetDateTo();

                    var j = 8;
                    for (var startDate = fromDate; startDate <= toDate; startDate = startDate.AddDays(1))
                    {
                        var shiftInfoForOperationRows =
                            workersTimeTrackingStatistics.
                                Where(r => r.Field<Int64>("WorkOperationID") == operationId && r.Field<DateTime>("WorkDayTimeStart").Date == startDate.Date);
                        if (shiftInfoForOperationRows.Any())
                        {
                            var totalTime = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("TotalTime"));
                            var totalWorkScope = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("WorkScope"));
                            var totalVCLP = shiftInfoForOperationRows.Sum(r => r.Field<decimal>("VCLP"));

                            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j] = totalWorkScope;
                            _shiftStatisticsColumnCount++;
                            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j + 1] = totalTime;
                            _shiftStatisticsColumnCount++;
                            _shiftStatisticsWorksheet.Cells[_shiftStatisticsCurrentRow, j + 2] = totalVCLP;
                        }

                        j += 3;
                    }

                    _shiftStatisticsCurrentRow++;
                }

                _shiftStatisticsRange = GetShiftStatisticsRange(startAdditionalOperationsRow, 2, _shiftStatisticsCurrentRow - 1, 2);
                _shiftStatisticsRange.Merge();
                _shiftStatisticsRange.Orientation = 90;
                _shiftStatisticsWorksheet.Cells[startAdditionalOperationsRow, 2] = "Общие операции";
            }
        }

        private static IEnumerable<DataRow> GetShiftTimeTrackingStatistics(int workerId)
        {
            var workerTimeTrackingStatisticsTable = new DataTable();
            workerTimeTrackingStatisticsTable.Columns.Add("WorkerID", typeof(long));
            workerTimeTrackingStatisticsTable.Columns.Add("TimeSpentAtWorkID", typeof(long));
            workerTimeTrackingStatisticsTable.Columns.Add("WorkDayTimeStart", typeof(DateTime));
            workerTimeTrackingStatisticsTable.Columns.Add("WorkOperationID", typeof(long));
            workerTimeTrackingStatisticsTable.Columns.Add("TotalTime", typeof(decimal));
            workerTimeTrackingStatisticsTable.Columns.Add("WorkScope", typeof(decimal));
            workerTimeTrackingStatisticsTable.Columns.Add("VCLP", typeof(decimal));

            var shiftsDataView = _shiftStatisticsTimeControlClass.GetShifts();
            var timeTrackingDataView = _shiftStatisticsTimeControlClass.GetTimeTracking();

            shiftsDataView.RowFilter = "WorkerID = " + workerId;

            foreach (DataRowView shiftDataRowView in shiftsDataView)
            {
                var timeSpentAtWorkId = Convert.ToInt32(shiftDataRowView["TimeSpentAtWorkID"]);
                var workDayTimeStart = Convert.ToDateTime(shiftDataRowView["WorkDayTimeStart"]);

                timeTrackingDataView.RowFilter =
                    string.Format("DeleteRecord <> 'True' AND TimeSpentAtWorkID = {0} AND WorkerID= {1}", timeSpentAtWorkId, workerId);

                var distinctOperationIDs = (timeTrackingDataView.ToTable().AsEnumerable()
                    .Select(names => new { WorkOperationID = names.Field<Int64>("WorkOperationID") })).Distinct()
                    .Select(distinctOperationId => distinctOperationId.WorkOperationID)
                    .ToList();

                foreach (var operationId in distinctOperationIDs)
                {
                    var operationRows = timeTrackingDataView.ToTable().AsEnumerable().Where(ids => ids.Field<Int64>("WorkOperationID") == operationId);

                    var statisticRow = workerTimeTrackingStatisticsTable.NewRow();
                    statisticRow["WorkerID"] = workerId;
                    statisticRow["TimeSpentAtWorkID"] = timeSpentAtWorkId;
                    statisticRow["WorkDayTimeStart"] = workDayTimeStart;
                    statisticRow["WorkOperationID"] = operationId;

                    var totalTime = GetTotalTime(operationRows, operationId);
                    int tt = (24 * totalTime.Days + totalTime.Hours);
                    decimal ttt = Decimal.Round((decimal)totalTime.Minutes / 60, 2);
                    decimal T = tt + ttt;
                    statisticRow["TotalTime"] = T;

                    var totalWorkScope = GetTotalWorkScope(operationRows, operationId);
                    statisticRow["WorkScope"] = Decimal.Round(totalWorkScope, 2).ToString("0.##");

                    double productivity = Convert.ToDouble(_shiftStatisticsMeasureNameConverter.Convert(operationId, "Productivity"));
                    var vclp = GetVCLP(Convert.ToDecimal(totalWorkScope), productivity, totalTime.TotalHours);
                    statisticRow["VCLP"] = vclp;

                    workerTimeTrackingStatisticsTable.Rows.Add(statisticRow);
                }
            }

            return workerTimeTrackingStatisticsTable.AsEnumerable();
        }

        private static TimeSpan GetTotalTime(IEnumerable<DataRow> timeTrackingRows, long operationId)
        {
            var tempTime = new TimeSpan();

            foreach (DataRow trackDataRow in timeTrackingRows)
            {
                var timeStart = (TimeSpan)trackDataRow["TimeStart"];
                var timeEnd = (TimeSpan)trackDataRow["TimeEnd"];

                TimeSpan timeFromTrack = timeEnd >= timeStart
                    ? timeEnd.Subtract(timeStart)
                    : new TimeSpan(24, 0, 0).Subtract(timeEnd.Subtract(timeStart).Duration());

                tempTime = tempTime.Add(timeFromTrack);
            }

            return tempTime;
        }

        private static decimal GetTotalWorkScope(IEnumerable<DataRow> timeTrackingRows, long operationId)
        {
            decimal workScope = 0;

            foreach (DataRow trackDataRow in timeTrackingRows)
            {
                decimal tempWorkScope;
                Decimal.TryParse(trackDataRow["WorkScope"].ToString(), out tempWorkScope);
                workScope += tempWorkScope;
            }

            return workScope;
        }

        private static double GetVCLP(decimal workScope, double currentProductivity, double totalHours)
        {
            double vclp;

            if (workScope == -1 || currentProductivity == -1)
            {
                return 0;
            }

            if (currentProductivity != 0 && totalHours != 0)
            {
                vclp = Convert.ToDouble(workScope) /
                       (currentProductivity * totalHours);
            }
            else
            {
                vclp = 0;
            }

            return Math.Round(vclp, 2);
        }

        private static IEnumerable<DataRow> GetShiftWorkUnits(int workerGroupId, int factoryId)
        {
            var workUnitsView = _shiftStatisticsCatalogClass.WorkUnitsDataTable.AsEnumerable().
                Where(wU => Convert.ToInt32(wU["WorkerGroupID"]) == workerGroupId &&
                    Convert.ToInt32(wU["FactoryID"]) == factoryId && Convert.ToBoolean(wU["Visible"]));
            return workUnitsView;
        }

        private static IEnumerable<DataRow> GetShiftWorkSections(int workUnitId)
        {
            var workSectionsView = _shiftStatisticsCatalogClass.WorkSectionsDataTable.AsEnumerable().
                    Where(r => Convert.ToInt32(r["WorkUnitID"]) == workUnitId && Convert.ToBoolean(r["Visible"]));
            return workSectionsView;
        }

        private static IEnumerable<DataRow> GetShiftWorkSubsections(int workSectionId)
        {
            var workSubsectionsView = _shiftStatisticsCatalogClass.WorkSubsectionsDataTable.AsEnumerable().
                        Where(r => Convert.ToInt32(r["WorkSectionID"]) == workSectionId && Convert.ToBoolean(r["Visible"]));
            return workSubsectionsView;
        }

        private static IEnumerable<DataRow> GetShiftWorkOperations(int workSubSectionId)
        {
            var workSubsectionsView = _shiftStatisticsCatalogClass.WorkOperationsDataTable.AsEnumerable().
                            Where(r => Convert.ToInt32(r["WorkSubsectionID"]) == workSubSectionId && Convert.ToBoolean(r["Visible"]));
            return workSubsectionsView;
        }

        private static Range GetShiftStatisticsRange(int y1, int x1, int y2, int x2)
        {
            return
                _shiftStatisticsWorksheet.Range[
                    _shiftStatisticsWorksheet.Cells[y1, x1],
                    _shiftStatisticsWorksheet.Cells[y2, x2]];
        }

        private static void SetShiftStatisticsStyle()
        {
            _shiftStatisticsRange = GetShiftStatisticsRange(4, 1, _shiftStatisticsCurrentRow - 1, _shiftStatisticsColumnCount);
            _shiftStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _shiftStatisticsRange.Borders.Weight = XlBorderWeight.xlMedium;

            _shiftStatisticsRange = GetShiftStatisticsRange(4, 1, _shiftStatisticsCurrentRow - 1, 7);
            _shiftStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _shiftStatisticsRange.VerticalAlignment = Constants.xlCenter;

            _shiftStatisticsRange = GetShiftStatisticsRange(4, 5, _shiftStatisticsCurrentRow - 1, 5);
            _shiftStatisticsRange.HorizontalAlignment = Constants.xlLeft;
            _shiftStatisticsRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void SetShiftStatisticsColumnsWidth()
        {
            _shiftStatisticsRange = GetShiftStatisticsRange(1, 1, _shiftStatisticsCurrentRow, 7);
            _shiftStatisticsRange.Columns.AutoFit();

            _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[1];
            _shiftStatisticsRange.ColumnWidth = 13;

            _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[2];
            _shiftStatisticsRange.ColumnWidth = 13;

            _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[3];
            _shiftStatisticsRange.ColumnWidth = 13;
            _shiftStatisticsRange.WrapText = true;

            _shiftStatisticsRange = _shiftStatisticsWorksheet.Columns[4];
            _shiftStatisticsRange.ColumnWidth = 24;
            _shiftStatisticsRange.WrapText = true;
        }

        private static void ShiftStatisticsFreezePanes()
        {
            _shiftStatisticsRange = _shiftStatisticsWorksheet.Rows[4];
            _shiftStatisticsRange.Activate();
            _shiftStatisticsRange.Select();
            _shiftStatisticsApp.ActiveWindow.FreezePanes = true;
        }

        private static void ShiftStatisticsOpenReport()
        {
            _shiftStatisticsApp.Visible = true;

            Marshal.ReleaseComObject(_shiftStatisticsWorkbook);
            Marshal.ReleaseComObject(_shiftStatisticsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion

        #region CommonOperationStatisticsReport

        public static void GenerateCommonOperationStatisticsReport(ref DataGrid exportingDataGrid,
            ItemCollection tCommonTimeCommStat)
        {
            _commonOperationStatisticsWaitWindow = new WaitWindow();
            _commonOperationStatisticsWaitWindow.Show(Application.Current.MainWindow);
            _commonOperationStatisticsWaitWindow.Text = "Вывод информации в Excel";

            _commonOperationStatisticsExportingDataGrid = exportingDataGrid;

            _commonTimeCommStat = tCommonTimeCommStat;

            CommonOperationStatistics_Initialize();

            FillCommonOperationStatisticsHeaders();

            SetCommonOperationStatisticsStyle();

            FillCommonOperationStatisticsData();

            CommonOperationStatistics_AutoFitColumn();
            CommonOperationStatistics_OpenReport();
            _commonOperationStatisticsWaitWindow.Close(true);
        }

        private static void CommonOperationStatistics_Initialize()
        {
            _commonOperationStatisticsApp = new Microsoft.Office.Interop.Excel.Application();
            _commonOperationStatisticsWorkbook = _commonOperationStatisticsApp.Workbooks.Add(Type.Missing);
            _commonOperationStatisticsWorksheet = _commonOperationStatisticsWorkbook.ActiveSheet;
        }

        private static void FillCommonOperationStatisticsHeaders()
        {
            int i = 1;
            foreach (var item in _commonTimeCommStat)
            {
                _commonOperationStatisticsWorksheet.Cells[i, 1] = item;
                i++;
            }

            int indexOfColumn = 1;

            foreach (
                DataGridColumn dataGridColumn in
                    _commonOperationStatisticsExportingDataGrid.Columns.Where(
                        dataGridColumn => dataGridColumn.Visibility == Visibility.Visible))
            {
                _commonOperationStatisticsWorksheet.Cells[CommonStatTableIndent, indexOfColumn] =
                    dataGridColumn.Header.ToString();

                _commonOperationStatisticsColumnCount = indexOfColumn;
                indexOfColumn++;
            }

            SetCommonOperationStatisticsHeadersStyle();
        }

        private static void SetCommonOperationStatisticsStyle()
        {
            _commonOperationStatisticsRange =
                _commonOperationStatisticsWorksheet.Range[
                    _commonOperationStatisticsWorksheet.Cells[CommonStatTableIndent + 1, 1],
                    _commonOperationStatisticsWorksheet.Cells[
                        _commonOperationStatisticsExportingDataGrid.Items.Count + CommonStatTableIndent,
                        _commonOperationStatisticsColumnCount]];
            _commonOperationStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
        }

        private static void SetCommonOperationStatisticsHeadersStyle()
        {
            _commonOperationStatisticsRange = _commonOperationStatisticsWorksheet.Range[
                _commonOperationStatisticsWorksheet.Cells[CommonStatTableIndent, 1],
                _commonOperationStatisticsWorksheet.Cells[CommonStatTableIndent, _commonOperationStatisticsColumnCount]];

            _commonOperationStatisticsFont = _commonOperationStatisticsRange.Font;

            _commonOperationStatisticsFont.Bold = true;

            _commonOperationStatisticsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _commonOperationStatisticsRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        private static void FillCommonOperationStatisticsData()
        {
            var visibleColumsCount = 0;

            for (var i = 0; i < _commonOperationStatisticsExportingDataGrid.Items.Count; i++)
            {
                var indexJofCell = 0;
                foreach (
                    DataGridColumn t in
                        _commonOperationStatisticsExportingDataGrid.Columns.Where(
                            t => t.Visibility == Visibility.Visible))
                {
                    visibleColumsCount++;

                    var cell = t.GetCellContent(_commonOperationStatisticsExportingDataGrid.Items[i]);

                    var cellText = string.Empty;

                    var contentPresenter = cell as ContentPresenter;
                    if (contentPresenter != null)
                        if (contentPresenter.Content != null)
                        {
                            var txtblck =
                                contentPresenter.ContentTemplate.FindName("txtblck", contentPresenter) as TextBlock;
                            if (txtblck != null) cellText = txtblck.Text;
                        }

                    var indexIofCell = i + CommonStatTableIndent + 1;

                    decimal cellNumbers;
                    var success = decimal.TryParse(cellText, out cellNumbers);

                    _commonOperationStatisticsWorksheet.Cells[indexIofCell, indexJofCell + 1] = success
                        ? (dynamic)
                            cellNumbers
                        : cellText;

                    indexJofCell++;
                }

                double percent = (double)i / _commonOperationStatisticsExportingDataGrid.Items.Count;
                _commonOperationStatisticsWaitWindow.Progress = percent * 100;
                _commonOperationStatisticsWaitWindow.Text = String.Format("Вывод данных в Excel {0}%", (int)(percent * 100));
            }
        }

        private static void CommonOperationStatistics_AutoFitColumn()
        {
            _commonOperationStatisticsRange = _commonOperationStatisticsWorksheet.Rows;
            _commonOperationStatisticsRange.Columns.AutoFit();

            Range er = _commonOperationStatisticsWorksheet.Range["A:A", Type.Missing];
            er.EntireColumn.ColumnWidth = 15;
        }

        private static void CommonOperationStatistics_OpenReport()
        {
            _commonOperationStatisticsApp.Visible = true;

            Marshal.ReleaseComObject(_commonOperationStatisticsWorkbook);
            Marshal.ReleaseComObject(_commonOperationStatisticsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region ProdRoomsScheduleReport

        public static void GenerateScheduleReport(ref DataGrid openingDataGrid, ref DataGrid closingDataGrid, int selectedYear,
            int selectedMonth, string selectedMonthName)
        {
            _scheduleOpeningDataGrid = openingDataGrid;
            _scheduleClosingDataGrid = closingDataGrid;

            _scheduleSelectedYear = selectedYear;
            _scheduleSelectedMonth = selectedMonth;
            _scheduleSelectedMonthName = selectedMonthName;

            _scheduleStartOfClosingTable = _scheduleOpeningDataGrid.Items.Count + 7;

            ScheduleInitialize();
        }

        private static void ScheduleInitialize()
        {
            _scheduleApp = new Microsoft.Office.Interop.Excel.Application();
            _scheduleWorkbook = _scheduleApp.Workbooks.Add(Type.Missing);
            _scheduleWorksheet = _scheduleWorkbook.ActiveSheet;

            FillScheduleHeader();

            FillScheduleOpeningTableHeaders();
            FillScheduleOpeningData();
            SetScheduleOpeningStyle();

            FillScheduleClosingTableHeaders();
            FillScheduleClosingData();
            SetScheduleClosingStyle();

            SetScheduleColumnsWidth();
            ScheduleOpenReport();
        }

        private static void FillScheduleHeader()
        {
            _scheduleRange = _scheduleWorksheet.Range[_scheduleWorksheet.Cells[4, 1], _scheduleWorksheet.Cells[4, 17]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[4, 1] = "График ответственных лиц за открытие цехов на " +
                                             _scheduleSelectedMonthName + " " +
                                             _scheduleSelectedYear + " г.";

            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 2, 1],
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 2, 17]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 2, 1] =
                "График ответственных лиц за закрытие цехов на " +
                _scheduleSelectedMonthName + " " + _scheduleSelectedYear + " г.";

            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[1, 19],
                    _scheduleWorksheet.Cells[1, 24]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[1, 19] = "УТВЕРЖДАЮ";

            _scheduleRange = _scheduleWorksheet.Range[_scheduleWorksheet.Cells[3, 19], _scheduleWorksheet.Cells[3, 24]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[3, 19] = " '       ' __________ " + _scheduleSelectedYear;

            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[3, _scheduleOpeningDataGrid.Columns.Count - 3],
                    _scheduleWorksheet.Cells[3, _scheduleOpeningDataGrid.Columns.Count]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[3, _scheduleOpeningDataGrid.Columns.Count - 3] = "Ф.А. Авдей";

            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + _scheduleClosingDataGrid.Items.Count + 7, 1],
                    _scheduleWorksheet.Cells[
                        _scheduleStartOfClosingTable + _scheduleClosingDataGrid.Items.Count + 7,
                        _scheduleOpeningDataGrid.Columns.Count]];
            _scheduleRange.Merge(Type.Missing);
            _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + _scheduleClosingDataGrid.Items.Count + 7, 1] =
                "Контроль осуществляет: заместитель директора Егорченко Р.П.";
        }

        #region OpeningData

        private static void FillScheduleOpeningTableHeaders()
        {
            int indexOfColumn = 1;

            foreach (
                DataGridColumn dataGridColumn in
                    _scheduleOpeningDataGrid.Columns.Where(
                        dataGridColumn => dataGridColumn.Visibility == Visibility.Visible))
            {
                _scheduleWorksheet.Cells[6, indexOfColumn] =
                    dataGridColumn.Header.ToString();
                _scheduleColumnsCount = indexOfColumn;
                indexOfColumn++;
            }

            SetScheduleOpeningTableHeadersStyle();
        }

        private static void SetScheduleOpeningTableHeadersStyle()
        {
            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[1, 1], _scheduleWorksheet.Cells[3, _scheduleColumnsCount]];

            _scheduleFont = _scheduleRange.Font;
            _scheduleFont.Bold = true;

            _scheduleRange = _scheduleWorksheet.Range[
                _scheduleWorksheet.Cells[6, 1], _scheduleWorksheet.Cells[6, _scheduleColumnsCount]];

            _scheduleFont = _scheduleRange.Font;
            _scheduleFont.Bold = true;

            _scheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _scheduleRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        private static void FillScheduleOpeningData()
        {
            for (var i = 0; i < _scheduleOpeningDataGrid.Items.Count; i++)
            {
                int visibleColumsCount = 0;
                foreach (
                    DataGridColumn t in
                        _scheduleOpeningDataGrid.Columns.Where(t => t.Visibility == Visibility.Visible))
                {
                    visibleColumsCount++;

                    var cell = t.GetCellContent(_scheduleOpeningDataGrid.Items[i]);

                    var cellText = string.Empty;

                    var checkBox = cell as CheckBox;
                    if (checkBox != null)
                        if (checkBox.IsChecked == true)
                            cellText = "О";

                    var textBlock = cell as TextBlock;
                    if (textBlock != null)
                        if (textBlock.Text != null)
                            cellText = textBlock.Text;

                    var indexIofCell = i + 7;

                    _scheduleWorksheet.Cells[indexIofCell, visibleColumsCount] = cellText;
                }
            }
        }

        private static void SetScheduleOpeningStyle()
        {
            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[7, 1],
                    _scheduleWorksheet.Cells[_scheduleOpeningDataGrid.Items.Count + 6, _scheduleColumnsCount]];
            _scheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;

            for (int i = 1; i < _scheduleOpeningDataGrid.Columns.Count() - 1; i++)
            {
                var date = new DateTime(_scheduleSelectedYear, _scheduleSelectedMonth, i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    _scheduleRange =
                        _scheduleWorksheet.Range[
                            _scheduleWorksheet.Cells[6, i + 1],
                            _scheduleWorksheet.Cells[_scheduleOpeningDataGrid.Items.Count + 6, i + 1]];
                    _scheduleRange.Interior.Color = XlRgbColor.rgbLightGray;
                }
            }
        }

        #endregion

        #region ClosingData

        private static void FillScheduleClosingTableHeaders()
        {
            int indexOfColumn = 1;

            foreach (
                DataGridColumn dataGridColumn in
                    _scheduleClosingDataGrid.Columns.Where(
                        dataGridColumn => dataGridColumn.Visibility == Visibility.Visible))
            {
                _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 4, indexOfColumn] =
                    dataGridColumn.Header.ToString();
                _scheduleColumnsCount = indexOfColumn;
                indexOfColumn++;
            }

            SetScheduleClosingTableHeadersStyle();
        }

        private static void SetScheduleClosingTableHeadersStyle()
        {
            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 4, 1],
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 4, _scheduleColumnsCount]];

            _scheduleFont = _scheduleRange.Font;
            _scheduleFont.Bold = true;

            _scheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _scheduleRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        private static void FillScheduleClosingData()
        {
            for (var i = 0; i < _scheduleClosingDataGrid.Items.Count; i++)
            {
                int visibleColumsCount = 0;
                foreach (
                    DataGridColumn t in
                        _scheduleClosingDataGrid.Columns.Where(t => t.Visibility == Visibility.Visible))
                {
                    visibleColumsCount++;

                    var cell = t.GetCellContent(_scheduleClosingDataGrid.Items[i]);

                    var cellText = string.Empty;

                    var checkBox = cell as CheckBox;
                    if (checkBox != null)
                        if (checkBox.IsChecked == true)
                            cellText = "З";

                    var textBlock = cell as TextBlock;
                    if (textBlock != null)
                        if (textBlock.Text != null)
                            cellText = textBlock.Text;

                    var indexIofCell = i + _scheduleStartOfClosingTable + 5;

                    _scheduleWorksheet.Cells[indexIofCell, visibleColumsCount] = cellText;
                }
            }
        }

        private static void SetScheduleClosingStyle()
        {
            _scheduleRange =
                _scheduleWorksheet.Range[
                    _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 5, 1],
                    _scheduleWorksheet.Cells[
                        _scheduleClosingDataGrid.Items.Count + 4 + _scheduleStartOfClosingTable, _scheduleColumnsCount]];
            _scheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;

            for (int i = 1; i < _scheduleClosingDataGrid.Columns.Count() - 1; i++)
            {
                var date = new DateTime(_scheduleSelectedYear, _scheduleSelectedMonth, i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    _scheduleRange =
                        _scheduleWorksheet.Range[
                            _scheduleWorksheet.Cells[_scheduleStartOfClosingTable + 4, i + 1],
                            _scheduleWorksheet.Cells[
                                _scheduleClosingDataGrid.Items.Count + _scheduleStartOfClosingTable + 4, i + 1]];
                    _scheduleRange.Interior.Color = XlRgbColor.rgbLightGray;
                }
            }
        }

        #endregion

        private static void SetScheduleColumnsWidth()
        {
            ((Range)_scheduleWorksheet.Cells[1, 1]).ColumnWidth = 20;
            _scheduleRange = _scheduleWorksheet.Range[_scheduleWorksheet.Cells[1, 2], _scheduleWorksheet.Cells[1, 34]];
            _scheduleRange.ColumnWidth = 2.7;
        }

        private static void ScheduleOpenReport()
        {
            _scheduleApp.Visible = true;

            Marshal.ReleaseComObject(_scheduleWorkbook);
            Marshal.ReleaseComObject(_scheduleApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion

        #region ProdRoomsActualStatusReport

        public static void GenerateProdRoomsActualStatusReport()
        {
            if (_roomsActualStatusIsReporting) return;

            _roomsActualStatusIsReporting = true;

            _roomsActualStatusWaitWindow = new WaitWindow();
            _roomsActualStatusWaitWindow.Show(Application.Current.MainWindow);
            _roomsActualStatusWaitWindow.Text = "Инициализация";

            ProdRoomsActualStatusInitialize();
        }

        private static void ProdRoomsActualStatusInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _roomsActualStatusRowsCount = 0;

                _roomsActualStatusIdtoNameConverter = new IdToNameConverter();
                _roomsActualStatuslockIdConverter = new LockIDConverter();
                App.BaseClass.GetProdRoomsClass(ref _roomsActualStatusProdRoomsClass);

                _roomsActualStatusApp = new Microsoft.Office.Interop.Excel.Application();
                _roomsActualStatusWorkbook = _roomsActualStatusApp.Workbooks.Add(Type.Missing);
                _roomsActualStatusWorksheet = _roomsActualStatusWorkbook.ActiveSheet;

                try
                {
                    _roomsActualStatusTable = ProdRoomsClass.GetDoorsActualStatus();

                    FillProdRoomsActualStatusHeaders();
                    FillProdRoomsActualStatusData();
                    SetProdRoomsActualStatusDataStyle();

                    SetProdRoomsActualStatusHorizontalAlignmentForCells();
                    SetProdRoomsActualStatusColumnsWidth();
                }
                catch (MySqlException exp)
                {
                    MessageBox.Show("Невозможно получить данные с сервера. Возможно отсутствует соединение. \n\n" + exp.Message);
                }
                finally
                {
                    JournalOpenReport();
                }
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _roomsActualStatusIsReporting = false;
                _roomsActualStatusWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillProdRoomsActualStatusHeaders()
        {
            _roomsActualStatusRange = GetProdRoomsActualStatusRange(2, 1, 2, 5);
            _roomsActualStatusRange.Merge(Type.Missing);
            _roomsActualStatusWorksheet.Cells[2, 1] = "Актуальное состояние производственных помещений";
            _roomsActualStatusRange.Font.Bold = true;

            _roomsActualStatusWorksheet.Cells[4, 1] = "Дверь";
            _roomsActualStatusWorksheet.Cells[4, 2] = "Состояние";
            _roomsActualStatusWorksheet.Cells[4, 3] = "Время";
            _roomsActualStatusWorksheet.Cells[4, 4] = "Номер пломбы";
            _roomsActualStatusWorksheet.Cells[4, 5] = "Ответственный за закрытие";
            _roomsActualStatusWorksheet.Cells[4, 6] = "Примечание";

            SetProdRoomsActualStatusHeaderStyle();
        }

        private static void SetProdRoomsActualStatusHeaderStyle()
        {
            _roomsActualStatusRange = GetProdRoomsActualStatusRange(4, 1, 4, RoomsActualStatusColumnsCount);
            _roomsActualStatusRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _roomsActualStatusRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        private static void FillProdRoomsActualStatusData()
        {
            var enableLocks = _roomsActualStatusProdRoomsClass.Locks.Table.AsEnumerable().
                Where(l => l.Field<Boolean>("IsEnable")).OrderBy(l => l.Field<string>("LockName")).ToList();

            var locksCount = enableLocks.Count();
            var lockIndex = 0;

            foreach (DataRow lockDr in enableLocks)
            {
                lockIndex++;
                var lockId = Convert.ToInt32(lockDr["LockID"]);

                var closingRow = _roomsActualStatusTable.AsEnumerable().FirstOrDefault(r => r.Field<Int64>("LockID") == lockId);
                if(closingRow != null)
                {
                    _roomsActualStatusRowsCount++;
                    _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 1] = _roomsActualStatuslockIdConverter.Convert(lockId);

                    var isClosed = Convert.ToBoolean(closingRow["IsClosed"]);
                    if (isClosed)
                    {
                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 2] = "Закрыта";
                        ((Range)_roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 2]).Font.Color = XlRgbColor.rgbIndianRed;

                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 3] = closingRow["Date"];
                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 4] = closingRow["SealNumber"];
                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 5] =
                            _roomsActualStatusIdtoNameConverter.Convert(closingRow["WorkerID"], "ShortName");
                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 6] = closingRow["WorkerNotes"];
                    }
                    else
                    {
                        _roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 2] = "Открыта";
                        ((Range)_roomsActualStatusWorksheet.Cells[4 + _roomsActualStatusRowsCount, 2]).Font.Color = XlRgbColor.rgbDarkGreen;
                    }
                }

                var percent = (double)lockIndex / locksCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _roomsActualStatusWaitWindow.Progress = percent * 100;
                    _roomsActualStatusWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void SetProdRoomsActualStatusDataStyle()
        {
            _roomsActualStatusRange = GetProdRoomsActualStatusRange(5, 1, _roomsActualStatusRowsCount + 4, RoomsActualStatusColumnsCount);
            _roomsActualStatusRange.Borders.LineStyle = XlLineStyle.xlContinuous;
        }

        private static void SetProdRoomsActualStatusHorizontalAlignmentForCells()
        {
            _roomsActualStatusRange = GetProdRoomsActualStatusRange(5, 1, _roomsActualStatusRowsCount + 5, 5);
            _roomsActualStatusRange.HorizontalAlignment = Constants.xlLeft;
        }

        private static void SetProdRoomsActualStatusColumnsWidth()
        {
            _roomsActualStatusRange = _roomsActualStatusWorksheet.Rows;
            _roomsActualStatusRange.Columns.AutoFit();
        }

        private static Range GetProdRoomsActualStatusRange(int y1, int x1, int y2, int x2)
        {
            return
                _roomsActualStatusWorksheet.Range[
                    _roomsActualStatusWorksheet.Cells[y1, x1],
                    _roomsActualStatusWorksheet.Cells[y2, x2]];
        }

        private static void JournalOpenReport()
        {
            _roomsActualStatusApp.Visible = true;

            Marshal.ReleaseComObject(_roomsActualStatusWorkbook);
            Marshal.ReleaseComObject(_roomsActualStatusApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region TimesheetReport

        public static void GenerateTimesheetReport(ref DataGrid timesheetDataGrid, int selectedYear, int selectedMonth,
                                            ref Button exportButton, ref Button applyFilterTimeSheetButton,
                                            ref Button calculateTimesheetStatButton, ref WrapPanel absencesWrapPanel)
        {
            _timesheetExportButton = exportButton;
            _applyFilterTimeSheetButton = applyFilterTimeSheetButton;
            _calculateTimesheetStatButton = calculateTimesheetStatButton;
            _absencesWrapPanel = absencesWrapPanel;

            _selectedTimesheetYear = selectedYear;
            _selectedTimesheetMonth = selectedMonth;

            _timeSheetColumnsCount = timesheetDataGrid.Columns.Count(c => c.Visibility == Visibility.Visible);
            _timeSheetItemsCount = timesheetDataGrid.Items.Count;
            _timeSheetDataGridItemsCollection = timesheetDataGrid.Items;

            _timesheetWaitWindow = new WaitWindow();
            _timesheetWaitWindow.Show(Application.Current.MainWindow);
            _timesheetWaitWindow.Text = "Вывод данных в Excel";

            TimeSheetInitialize();
        }

        private static void TimeSheetInitialize()
        {
            var bw = new BackgroundWorker();

            _timesheetExportButton.IsEnabled = false;
            _applyFilterTimeSheetButton.IsEnabled = false;
            _calculateTimesheetStatButton.IsEnabled = false;
            _absencesWrapPanel.IsEnabled = false;

            bw.DoWork += (sender, args) =>
            {
                _timesheetApp = new Microsoft.Office.Interop.Excel.Application();
                _timesheetWorkbook = _timesheetApp.Workbooks.Add(Type.Missing);
                _timesheetWorksheet = _timesheetWorkbook.ActiveSheet;

                //_dailyRateWorkingHoursConverter = new DailyRateWorkingHoursConverter();
                _idToNameConverter = new IdToNameConverter();
                _workerProfessionInfoConverter = new WorkerProfessionInfoConverter();
                //_absenceTypeConverter = new AbsenceTypeConverter();


                SetTimesheetStyle();
                FillTimeSheetHeaders();
                SetTimeSheetHeadersStyle();
                FillTimesheetData();

                SetTimesheetHorizontalAlignmentForCells();
                SetTimesheetFontSize();
                SetTimesheetColumnsWidth();
                TimeSheetFreezePanes();
                TimeSheetOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _timesheetExportButton.IsEnabled = true;
                _applyFilterTimeSheetButton.IsEnabled = true;
                _calculateTimesheetStatButton.IsEnabled = true;
                _absencesWrapPanel.IsEnabled = true;
                _timesheetWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void SetTimesheetStyle()
        {
            _timesheetRange = GetTimesheetRange(10, 1, 9 + (_timeSheetItemsCount + 1) * 3, _timeSheetColumnsCount + 11);
            _timesheetRange.Borders.LineStyle = XlLineStyle.xlContinuous;
        }

        private static void FillTimeSheetHeaders()
        {
            FillTimeSheetMask();

            _timesheetWorksheet.Cells[10, 1] = "№";
            _timesheetRange = GetTimesheetRange(11, 1, 12, 1);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[11, 1] = "п/п";

            _timesheetRange = GetTimesheetRange(10, 2, 12, 2);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[10, 2] = "ФИО";
            _timesheetRange = GetTimesheetRange(10, 3, 12, 3);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[10, 3] = "Разряд \nСтавка";

            for (int i = 1; i < _timeSheetColumnsCount + 1; i++)
            {
                _timesheetWorksheet.Cells[11, 3 + i] = i;
                //_timesheetWorksheet.Cells[12, 3 + i] =
                    //_dailyRateWorkingHoursConverter.Convert(
                    //    new[] { (object)i, _selectedTimesheetMonth - 1, _selectedTimesheetYear }, typeof(string),
                    //    "Hours", CultureInfo.InvariantCulture);
            }

            _timesheetRange = GetTimesheetRange(10, 4, 10, 3 + _timeSheetColumnsCount);
            _timesheetRange.Merge(Type.Missing);

            FillRightTimeSheetHeaders();
        }

        private static void FillRightTimeSheetHeaders()
        {
            _timesheetRange = GetTimesheetRange(10, 4 + _timeSheetColumnsCount, 10, 11 + _timeSheetColumnsCount);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[10, 4 + _timeSheetColumnsCount] = "факт отработ";

            int j = _timeSheetColumnsCount + 4;

            for (int i = 0; i < 8; i++)
            {
                _timesheetRange = GetTimesheetRange(11, j + i, 12, j + i);
                _timesheetRange.Merge(Type.Missing);
                switch (i)
                {
                    case 0:
                        _timesheetWorksheet.Cells[11, j + i] = "Дни";
                        break;
                    case 1:
                        _timesheetWorksheet.Cells[11, j + i] = "Часы";
                        break;
                    case 2:
                        _timesheetWorksheet.Cells[11, j + i] = "ч/н";
                        break;
                    case 3:
                        _timesheetWorksheet.Cells[11, j + i] = "б/л";
                        break;
                    case 4:
                        _timesheetWorksheet.Cells[11, j + i] = "По норме";
                        break;
                    case 5:
                        _timesheetWorksheet.Cells[11, j + i] = "А";
                        break;
                    case 6:
                        ((Range)_timesheetWorksheet.Cells[11, j + i]).WrapText = true;
                        _timesheetWorksheet.Cells[11, j + i] = "Переработка за месяц";
                        break;
                    case 7:
                        ((Range)_timesheetWorksheet.Cells[11, j + i]).WrapText = true;
                        _timesheetWorksheet.Cells[11, j + i] = "Готовая переработка";
                        break;
                }
            }
        }

        private static void FillTimeSheetMask()
        {
            _timesheetRange = GetTimesheetRange(1, 2, 2, 2);
            _timesheetRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _timesheetRange.Borders.Weight = XlBorderWeight.xlMedium;

            _timesheetRange = GetTimesheetRange(1, _timeSheetColumnsCount + 1, 1, _timeSheetColumnsCount + 3);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[1, _timeSheetColumnsCount + 1] = "УТВЕРЖДАЮ";

            _timesheetRange = GetTimesheetRange(2, _timeSheetColumnsCount - 4, 2, _timeSheetColumnsCount + 3);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[2, _timeSheetColumnsCount - 4] = "______________________    Ф.А. Авдей";

            _timesheetRange = GetTimesheetRange(3, _timeSheetColumnsCount - 4, 3, _timeSheetColumnsCount + 2);
            _timesheetRange.Merge(Type.Missing);
            _timesheetWorksheet.Cells[3, _timeSheetColumnsCount - 4] = "Директор  СООО ЗОВ-ПРОФИЛЬ";

            _timesheetRange = GetTimesheetRange(9, 8, 9, 27);
            _timesheetRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _timesheetRange.Borders.Weight = XlBorderWeight.xlMedium;
            _timesheetRange.Merge(Type.Missing);

            DateTimeFormatInfo dateTimeFormat = new CultureInfo("ru-RU").DateTimeFormat;
            _timesheetWorksheet.Cells[9, 8] = String.Format("ГРАФИК/ТАБЕЛЬ РАБОТЫ ЗА {0} {1}г.",
                                                             dateTimeFormat.GetMonthName(_selectedTimesheetMonth).
                                                                 ToUpper(), _selectedTimesheetYear);
        }

        private static void SetTimeSheetHeadersStyle()
        {
            _timesheetRange = GetTimesheetRange(10, 1, 12, _timeSheetColumnsCount + 11);
            _timesheetRange.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium,
                                             XlColorIndex.xlColorIndexNone, "FF000000");
        }


        private static void FillTimesheetData()
        {
            var j = 1;
            var indexOfRow = 13;
            var k = _timeSheetColumnsCount + 4;
            foreach (DataRowView row in _timeSheetDataGridItemsCollection)
            {
                #region FillWorkerProfInfo
                var parametrs = new[] { row["WorkerID"], row["FactoryID"] };

                _timesheetRange = GetTimesheetRange(indexOfRow, 1, indexOfRow + 2, 1);
                _timesheetRange.Merge(Type.Missing);
                _timesheetWorksheet.Cells[indexOfRow, 1] = j;

                _timesheetRange = GetTimesheetRange(indexOfRow, 2, indexOfRow + 1, 2);
                _timesheetRange.Merge(Type.Missing);
                _timesheetWorksheet.Cells[indexOfRow, 2] = _idToNameConverter.Convert(row["WorkerID"], typeof(string),
                                                                                      "ShortName", CultureInfo.InvariantCulture);

                _timesheetWorksheet.Cells[indexOfRow + 2, 2] =
                    _workerProfessionInfoConverter.Convert(parametrs, typeof(string), "ProfessionName",
                                                           new CultureInfo("ru-RU"));
                //((Excel.Range) _timesheetWorksheet.Cells[indexOfRow + 2, 2]).WrapText = true;

                _timesheetWorksheet.Cells[indexOfRow, 3] = _workerProfessionInfoConverter.Convert(parametrs, typeof(string),
                                                                                                      "Category",
                                                                                                      CultureInfo.InvariantCulture);
                _timesheetWorksheet.Cells[indexOfRow + 1, 3] = _workerProfessionInfoConverter.Convert(parametrs, typeof(string),
                                                                                                  "Rate", CultureInfo.InvariantCulture);
                _timesheetWorksheet.Cells[indexOfRow + 2, 3] = "Вредность";
                #endregion


                #region FillMainTimeSheetData
                for (int i = 1; i < _timeSheetColumnsCount + 1; i++)
                {
                    _timesheetWorksheet.Cells[indexOfRow, 3 + i] = row[String.Format("s{0}", i)];
                    //var value = _absenceTypeConverter.Convert(
                    //    new[] { row[String.Format("d{0}", i)], row[String.Format("t{0}", i)] }, typeof(string),
                    //    "Hours", CultureInfo.InvariantCulture);
                    //_timesheetWorksheet.Cells[indexOfRow + 1, 3 + i] = value;
                    //if (String.Equals(value.ToString(), "В"))
                    //{
                    //    _timesheetRange = GetTimesheetRange(indexOfRow, 3 + i, indexOfRow + 2, 3 + i);
                    //    _timesheetRange.Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbYellow;
                    //}

                    _timesheetWorksheet.Cells[indexOfRow + 2, 3 + i] = row[String.Format("i{0}", i)];
                }
                #endregion


                #region FillTimeSheetStatData

                for (int i = 0; i < 8; i++)
                {
                    _timesheetRange = GetTimesheetRange(indexOfRow, k + i, indexOfRow + 2, k + i);
                    _timesheetRange.Merge(Type.Missing);
                    switch (i)
                    {
                        case 0:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["WorkingDaysCount"];
                            break;
                        case 1:
                            _timesheetRange = GetTimesheetRange(indexOfRow, k + i, indexOfRow + 2, k + i);
                            _timesheetRange.UnMerge();
                            _timesheetRange = GetTimesheetRange(indexOfRow, k + i, indexOfRow + 1, k + i);
                            _timesheetRange.Merge(Type.Missing);
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["TimesheetSumm"];
                            _timesheetWorksheet.Cells[indexOfRow + 2, k + i] = row["InsalubrityTime"];
                            break;
                        case 2:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["NightTime"];
                            break;
                        case 3:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["SickTime"];
                            break;
                        case 4:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["TimeOnBasisNorms"];
                            break;
                        case 5:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = row["OwnExpenseTime"];
                            break;
                        case 6:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = Math.Round(Convert.ToDouble(row["ExceedingTime"]), 2);
                            break;
                        case 7:
                            _timesheetWorksheet.Cells[indexOfRow, k + i] = Math.Round(Convert.ToDouble(row["ExceedingAllTime"]), 2);
                            break;
                    }
                }

                #endregion

                _timesheetRange = GetTimesheetRange(indexOfRow, 1, indexOfRow + 2, k + 7);
                _timesheetRange.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium,
                                             XlColorIndex.xlColorIndexNone, "FF000000");

                double percent = (double)j / _timeSheetItemsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                                                                          {
                                                                              _timesheetWaitWindow.Progress =
                                                                                  percent * 100;
                                                                              _timesheetWaitWindow.Text =
                                                                                  String.Format(
                                                                                      "Вывод данных в Excel {0}%",
                                                                                      (int)(percent * 100));
                                                                          }));

                j++;
                indexOfRow += 3;
            }
        }



        //private static void FillMainTimesheetInfo()
        //{

        //    //_timesheetRange = GetTimesheetRange(indexOfRow, 3 + i, indexOfRow + 2, 3 + i);
        //    //var color = ((SolidColorBrush)_absenceTypeConverter.Convert(
        //    //    new[] { row[String.Format("d{0}", i)], row[String.Format("t{0}", i)] }, typeof(string),
        //    //    "Color", CultureInfo.InvariantCulture)).Color;
        //    //_timesheetRange.Interior.Color =
        //    //    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(color.A, color.R, color.G,
        //    //                                                                       color.B));
        //}

        private static void SetTimesheetHorizontalAlignmentForCells()
        {
            _timesheetRange = _timesheetWorksheet.Rows;
            _timesheetRange.HorizontalAlignment = Constants.xlCenter;
            _timesheetRange.VerticalAlignment = Constants.xlCenter;

            _timesheetRange = GetTimesheetRange(13, 2, _timeSheetItemsCount * 3 + 12, 2);
            _timesheetRange.HorizontalAlignment = Constants.xlLeft;
        }

        private static void SetTimesheetFontSize()
        {
            _timesheetRange = _timesheetWorksheet.Rows;
            _timesheetRange.Font.Size = 10;
        }

        private static void SetTimesheetColumnsWidth()
        {
            _timesheetRange = _timesheetWorksheet.Rows;
            _timesheetRange.Columns.AutoFit();

            ((Range)_timesheetWorksheet.Cells[1, 2]).ColumnWidth = 30;
            ((Range)_timesheetWorksheet.Cells[1, 3]).ColumnWidth = 8;
            _timesheetRange = GetTimesheetRange(1, 4, 1, 2 + _timeSheetColumnsCount);
            _timesheetRange.ColumnWidth = 3;

            var k = _timeSheetColumnsCount + 3;
            ((Range)_timesheetWorksheet.Cells[1, k + 6]).ColumnWidth = 3;
            ((Range)_timesheetWorksheet.Cells[1, k + 7]).ColumnWidth = 11;
            ((Range)_timesheetWorksheet.Cells[1, k + 8]).ColumnWidth = 11;
        }

        private static void TimeSheetFreezePanes()
        {
            ((Range)_timesheetWorksheet.Cells[13, 4]).Select();
            _timesheetApp.ActiveWindow.FreezePanes = true;
        }

        private static void TimeSheetOpenReport()
        {
            _timesheetApp.Visible = true;

            Marshal.ReleaseComObject(_timesheetWorkbook);
            Marshal.ReleaseComObject(_timesheetApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private static Range GetTimesheetRange(int y1, int x1, int y2, int x2)
        {
            return _timesheetWorksheet.Range[_timesheetWorksheet.Cells[y1, x1], _timesheetWorksheet.Cells[y2, x2]];
        }

        #endregion


        #region ProductionScheduleReport

        public static void GenerateProdScheduleReport(DataView prodScheduleView, int visibleColumnsCount, DateTime selectedDate)
        {
            _prodScheduleView = prodScheduleView;
            _prodScheduleVisibleColumnsCount = visibleColumnsCount;
            _prodScheduleSelectedDate = selectedDate;

            ProdScheduleInitialize();
        }

        private static void ProdScheduleInitialize()
        {
            _productionScheduleApp = new Microsoft.Office.Interop.Excel.Application();
            _productionScheduleWorkbook = _productionScheduleApp.Workbooks.Add(Type.Missing);
            _productionScheduleWorksheet = _productionScheduleWorkbook.ActiveSheet;

            _workUnitConverter = new IdToWorkUnitConverter();

            SetProdScheduleStyle();
            FillProdScheduleHeaders();
            FillProdScheduleData();
            SetProdScheduleColumnWidth();
            ProdSheduleOpenReport();
        }

        private static void SetProdScheduleStyle()
        {
            _productionScheduleRange = GetProdScheduleRange(3, 1, 3, _prodScheduleVisibleColumnsCount + 1);
            _productionScheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _productionScheduleRange.Borders.Weight = XlBorderWeight.xlMedium;

            _productionScheduleRange = GetProdScheduleRange(4, 1, _prodScheduleView.Count + 3, _prodScheduleVisibleColumnsCount + 1);
            _productionScheduleRange.Borders.LineStyle = XlLineStyle.xlContinuous;
        }

        private static void FillProdScheduleHeaders()
        {
            _productionScheduleRange = GetProdScheduleRange(1, 1, 1, 20);
            _productionScheduleRange.Merge();
            _productionScheduleWorksheet.Cells[1, 1] =
                string.Format("График выхода производственных участков на {0:MMMM yyyy}г.", _prodScheduleSelectedDate);

            _productionScheduleWorksheet.Cells[3, 1] = "Участок";

            for (int j = 0; j < _prodScheduleVisibleColumnsCount; j++)
            {
                _productionScheduleWorksheet.Cells[3, j + 2] = j + 1;
            }
        }

        private static void FillProdScheduleData()
        {
            int i = 4;

            foreach (DataRowView drv in _prodScheduleView)
            {
                _productionScheduleWorksheet.Cells[i, 1] =
                    _workUnitConverter.Convert(drv.Row[1], typeof(string), null, CultureInfo.CurrentCulture);

                for (int j = 0; j < _prodScheduleVisibleColumnsCount; j++)
                {
                    _productionScheduleWorksheet.Cells[i, j + 2] = drv.Row[j + 3];
                }

                i++;
            }
        }

        private static void SetProdScheduleColumnWidth()
        {
            _productionScheduleRange = _productionScheduleWorksheet.Rows;
            _productionScheduleRange.Columns.AutoFit();
            _productionScheduleRange = GetProdScheduleRange(1, 2, 1, _prodScheduleVisibleColumnsCount + 1);
            _productionScheduleRange.ColumnWidth = 2.1;
        }

        private static void ProdSheduleOpenReport()
        {
            _productionScheduleApp.Visible = true;

            Marshal.ReleaseComObject(_productionScheduleWorkbook);
            Marshal.ReleaseComObject(_productionScheduleApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private static Range GetProdScheduleRange(int y1, int x1, int y2, int x2)
        {
            return _productionScheduleWorksheet.Range[_productionScheduleWorksheet.Cells[y1, x1], _productionScheduleWorksheet.Cells[y2, x2]];
        }

        #endregion


        #region ActionTypesReport

        public static void GenerateProdScheduleReport(BindingListCollectionView actionTypesView)
        {
            _actionTypesView = actionTypesView;
            _actionTypesWaitWindow = new WaitWindow();
            _actionTypesWaitWindow.Show(Application.Current.MainWindow);
            _actionTypesWaitWindow.Text = "Вывод данных в Excel";

            ActionTypesInitialize();
        }

        private static void ActionTypesInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
                {
                    _actionTypesRowsCount = 0;

                    _actionTypesApp = new Microsoft.Office.Interop.Excel.Application();
                    _actionTypesWorkbook = _actionTypesApp.Workbooks.Add(Type.Missing);
                    _actionTypesWorksheet = _actionTypesWorkbook.ActiveSheet;

                    FillActionTypesHeaders();
                    FillActionTypesData();
                    SetActionTypesStyle();
                    SetActionTypesColumnWidth();
                    ActionTypesOpenReport();
                };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _actionTypesWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillActionTypesHeaders()
        {
            _actionTypesWorksheet.Cells[ActionTypesStartRow, 1] = "Модуль";
            _actionTypesWorksheet.Cells[ActionTypesStartRow, 2] = "Основные действия";
            _actionTypesWorksheet.Cells[ActionTypesStartRow, 3] = "ID";
        }

        private static void FillActionTypesData()
        {
            string moduleName = string.Empty;
            foreach (DataRowView item in _actionTypesView)
            {
                if (!string.Equals(item.Row["ModuleName"].ToString(), moduleName))
                {
                    moduleName = item.Row["ModuleName"].ToString();
                    _actionTypesWorksheet.Cells[ActionTypesStartRow + _actionTypesRowsCount, 1] = moduleName;
                    _actionTypesRange = _actionTypesWorksheet.Cells[ActionTypesStartRow + _actionTypesRowsCount, 1];
                    var font = _actionTypesRange.Font;
                    font.Bold = true;
                    _actionTypesRowsCount++;
                }
                _actionTypesWorksheet.Cells[ActionTypesStartRow + _actionTypesRowsCount, 2] = item.Row["ActionName"].ToString();
                _actionTypesWorksheet.Cells[ActionTypesStartRow + _actionTypesRowsCount, 3] = item.Row["ActionTypeID"].ToString();
                _actionTypesRowsCount++;
            }
        }

        private static void SetActionTypesStyle()
        {
            _actionTypesRange = GetActionTypesRange(ActionTypesStartRow, 1, ActionTypesStartRow, ActionTypesColumns);
            _actionTypesRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _actionTypesRange.Borders.Weight = XlBorderWeight.xlMedium;
            var font = _actionTypesRange.Font;
            font.Bold = true;

            _actionTypesRange = GetActionTypesRange(ActionTypesStartRow + 1, 1,
                _actionTypesRowsCount + ActionTypesStartRow - 1, ActionTypesColumns);
            _actionTypesRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _actionTypesRange.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium,
                XlColorIndex.xlColorIndexAutomatic, Type.Missing, Type.Missing);
        }

        private static void SetActionTypesColumnWidth()
        {
            _actionTypesRange = _actionTypesWorksheet.Rows;
            _actionTypesRange.Columns.AutoFit();
            //_productionScheduleRange = GetProdScheduleRange(1, 2, 1, _prodScheduleVisibleColumnsCount + 1);
            //_productionScheduleRange.ColumnWidth = 2.1;
        }

        private static void ActionTypesOpenReport()
        {
            _actionTypesApp.Visible = true;

            Marshal.ReleaseComObject(_actionTypesWorkbook);
            Marshal.ReleaseComObject(_actionTypesApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private static Range GetActionTypesRange(int y1, int x1, int y2, int x2)
        {
            return _actionTypesWorksheet.Range[_actionTypesWorksheet.Cells[y1, x1], _actionTypesWorksheet.Cells[y2, x2]];
        }

        #endregion


        #region MachineOperationsReport

        public static void GenerateMachineOperationsReport(int workerGroupId, int factoryId, bool availableWorkUnits,
            ref SwitchComboBox machineOperationExportElement)
        {
            _machineOperationsWorkerGroupId = workerGroupId;
            _machineOperationsFactoryId = factoryId;
            _machineOperationsAvailableWorkUnits = availableWorkUnits;

            _machineOperationsWaitWindow = new WaitWindow();
            _machineOperationsWaitWindow.Show(Application.Current.MainWindow);
            _machineOperationsWaitWindow.Text = "Вывод данных в Excel";

            _machineOperationExportElement = machineOperationExportElement;
            _machineOperationExportElement.IsEnabled = false;

            MachineOperationsInitialize();
        }

        private static void MachineOperationsInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _machineOperationsApp = new Microsoft.Office.Interop.Excel.Application();
                _machineOperationsWorkbook = _machineOperationsApp.Workbooks.Add(Type.Missing);
                _machineOperationsWorksheet = _machineOperationsWorkbook.ActiveSheet;
                if (_cc == null)
                    App.BaseClass.GetCatalogClass(ref _cc);

                _machineOperationsWorkUnitConverter = new IdToWorkUnitConverter();
                _machineOperationsSectionConverter = new IdToWorkSectionConverter();
                _machineOperationsSubSectionConverter = new IdToWorkSubSectionConverter();
                _machineOperationsConverter = new IdToWorkOperationConverter();
                _machineOperationsMeasureConverter = new MeasureUnitNameFromOperationIdConverter();
                _machineOperationsWorkerGroupConverter = new IdToWorkerGroupConverter();
                _machineOperationsFactoryConverter = new IdToFactoryConverter();

                FillMachineOperationsHeaders();
                FillMachineOperationsData();
                SetMachineOperationsStyle();
                SetMachineOperationsColumnWidth();
                MachineOperationsOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _machineOperationsWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillMachineOperationsHeaders()
        {
            _machineOperationsRange = GetMachineOperationsRange(1, 1, 1, 4);
            _machineOperationsRange.Merge();
            _machineOperationsRange.Font.Bold = true;
            _machineOperationsRange.Font.Size = 14;
            _machineOperationsWorksheet.Cells[1, 1] = "Список операций";

            _machineOperationsRange = GetMachineOperationsRange(2, 1, 2, 4);
            _machineOperationsRange.Merge();
            _machineOperationsRange.Font.Bold = true;
            _machineOperationsRange.Font.Size = 14;
            var headerText = string.Format("Группа: {0}, фабрика: {1}, дата {2:dd.MM.yyyy HH:mm}",
                _machineOperationsWorkerGroupConverter.Convert(_machineOperationsWorkerGroupId, typeof(string), null, new CultureInfo("ru-RU")),
                _machineOperationsFactoryConverter.Convert(_machineOperationsFactoryId, typeof(string), null, new CultureInfo("ru-RU")),
                App.BaseClass.GetDateFromSqlServer());
            _machineOperationsWorksheet.Cells[2, 1] = headerText;

            _machineOperationsWorksheet.Cells[MachineOperationsStartRow, 1] = "Станок";
            _machineOperationsWorksheet.Cells[MachineOperationsStartRow, 2] = "Операция";
            _machineOperationsWorksheet.Cells[MachineOperationsStartRow, 3] = "Норма в час";
            _machineOperationsWorksheet.Cells[MachineOperationsStartRow, 4] = "Ед. изм.";
        }

        private static void FillMachineOperationsData()
        {
            var view = GetWorkUnits(_machineOperationsWorkerGroupId, _machineOperationsFactoryId);
            int j = MachineOperationsStartRow + 1;
            var workUnitIndex = 1;
            foreach (var row in view)
            {
                var workUnitId = Convert.ToInt32(row["WorkUnitID"]);
                if (_machineOperationsAvailableWorkUnits)
                {
                    _machineOperationsRange = GetMachineOperationsRange(j, 1, j, 4);
                    _machineOperationsRange.Font.Bold = true;
                    _machineOperationsRange.Font.Size = 16;
                    _machineOperationsRange.Merge();
                    _machineOperationsWorksheet.Cells[j, 1] = _machineOperationsWorkUnitConverter.Convert(workUnitId, typeof(string), null, new CultureInfo("ru-RU"));
                    j++;
                }
                foreach (var workSectionRow in _cc.WorkSectionsDataTable.AsEnumerable().
                    Where(r => Convert.ToInt32(r["WorkUnitID"]) == workUnitId && Convert.ToBoolean(r["Visible"])))
                {
                    var workSectionId = Convert.ToInt32(workSectionRow["WorkSectionID"]);
                    if (_machineOperationsAvailableWorkUnits)
                    {
                        _machineOperationsRange = GetMachineOperationsRange(j, 1, j, 4);
                        _machineOperationsRange.Font.Bold = true;
                        _machineOperationsRange.Merge();
                        _machineOperationsRange.Interior.Color = XlRgbColor.rgbLightGray;
                        _machineOperationsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                        _machineOperationsWorksheet.Cells[j, 1] = _machineOperationsSectionConverter.Convert(workSectionId, typeof(string), null, new CultureInfo("ru-RU"));
                        j++;
                    }
                    foreach (var workSubsectionRow in _cc.WorkSubsectionsDataTable.AsEnumerable().
                        Where(r => Convert.ToInt32(r["WorkSectionID"]) == workSectionId && Convert.ToBoolean(r["Visible"])))
                    {
                        var workSubsectionId = Convert.ToInt32(workSubsectionRow["WorkSubsectionID"]);
                        var subSectionStart = j - 1;
                        _machineOperationsWorksheet.Cells[j, 1] = _machineOperationsSubSectionConverter.Convert(workSubsectionId, typeof(string), null, new CultureInfo("ru-RU"));
                        foreach (var operationRow in _cc.WorkOperationsDataTable.AsEnumerable().
                            Where(r => Convert.ToInt32(r["WorkSubsectionID"]) == workSubsectionId && Convert.ToBoolean(r["Visible"])))
                        {
                            var operationId = Convert.ToInt32(operationRow["WorkOperationID"]);
                            _machineOperationsWorksheet.Cells[j, 2] = _machineOperationsConverter.Convert(operationId, typeof(string), null, new CultureInfo("ru-RU"));
                            var machineOperations = _cc.MachinesOperationsDataTable.AsEnumerable().Where(r => r.Field<Int64>("WorkOperationID") == operationId);
                            if(machineOperations.Any())
                            {
                                var machineOperation = machineOperations.First();
                                var productivity = machineOperation["Productivity"];
                                _machineOperationsWorksheet.Cells[j, 3] = productivity;
                                if (productivity == DBNull.Value || productivity == null ||
                                    string.IsNullOrEmpty(productivity.ToString().Trim()) ||
                                    Convert.ToInt32(productivity) == 0)
                                {
                                    _machineOperationsRange = _machineOperationsWorksheet.Cells[j, 3];
                                    _machineOperationsRange.Interior.Color = XlRgbColor.rgbLightSalmon;
                                }
                                    
                                var measureName = _machineOperationsMeasureConverter.Convert(operationId, typeof(string), "MeasureUnitName", new CultureInfo("ru-RU"));
                                _machineOperationsWorksheet.Cells[j, 4] = measureName;
                                if(measureName == null || string.IsNullOrEmpty(measureName.ToString().Trim()))
                                {
                                    _machineOperationsRange = _machineOperationsWorksheet.Cells[j, 4];
                                    _machineOperationsRange.Interior.Color = XlRgbColor.rgbLightSalmon;
                                }
                            }
                            else
                            {
                                _machineOperationsRange = GetMachineOperationsRange(j, 3, j, 4);
                                _machineOperationsRange.Interior.Color = XlRgbColor.rgbLightSalmon;
                            }
                            _machineOperationsRange = GetMachineOperationsRange(j, 1, j, 4);
                            _machineOperationsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                            j++;
                        }

                        _machineOperationsRange = GetMachineOperationsRange(subSectionStart, 1, subSectionStart, 4);
                        _machineOperationsRange.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
                    }
                }
                double percent = (double)workUnitIndex / view.Count();
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _machineOperationsWaitWindow.Progress =
                        percent * 100;
                    _machineOperationsWaitWindow.Text =
                        String.Format(
                            "Вывод данных в Excel {0}%",
                            (int)(percent * 100));
                }));
                workUnitIndex++;
            }
            _machineOperationsRowsCount = j;
        }

        private static void SetMachineOperationsColumnWidth()
        {
            _machineOperationsRange = _machineOperationsWorksheet.Rows;
            _machineOperationsRange.Columns.AutoFit();
        }

        private static void SetMachineOperationsStyle()
        {
            _machineOperationsRange = GetMachineOperationsRange(MachineOperationsStartRow, 1, MachineOperationsStartRow, 4);
            _machineOperationsRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _machineOperationsRange.Borders.Weight = XlBorderWeight.xlMedium;
            var font = _machineOperationsRange.Font;
            font.Bold = true;

            _machineOperationsRange = GetMachineOperationsRange(MachineOperationsStartRow + 1, 1,
                _machineOperationsRowsCount - 1, 4);
            _machineOperationsRange.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium,
                XlColorIndex.xlColorIndexAutomatic, Type.Missing, Type.Missing);
            _machineOperationsRange = GetMachineOperationsRange(MachineOperationsStartRow + 1, 3, _machineOperationsRowsCount, 3);
            _machineOperationsRange.HorizontalAlignment = Constants.xlLeft;
        }

        private static void MachineOperationsOpenReport()
        {
            _machineOperationsApp.Visible = true;

            Marshal.ReleaseComObject(_machineOperationsWorkbook);
            Marshal.ReleaseComObject(_machineOperationsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                if (_machineOperationExportElement != null)
                    _machineOperationExportElement.IsEnabled = true;
            }));
        }

        private static IEnumerable<DataRow> GetWorkUnits(int workerGroupId, int factoryId)
        {
            var workUnitsView = _cc.WorkUnitsDataTable.AsEnumerable().
                Where(wU => Convert.ToInt32(wU["WorkerGroupID"]) == workerGroupId &&
                    Convert.ToInt32(wU["FactoryID"]) == factoryId && Convert.ToBoolean(wU["Visible"]));
            return workUnitsView;
        }

        private static Range GetMachineOperationsRange(int y1, int x1, int y2, int x2)
        {
            return _machineOperationsWorksheet.Range[_machineOperationsWorksheet.Cells[y1, x1], _machineOperationsWorksheet.Cells[y2, x2]];
        }

        #endregion


        #region AccessGroupReport

        public static void GenerateAccessGroupReport(ref Button exportedElement)
        {
            _accessGroupExportedControl = exportedElement;
            _accessGroupExportedControl.IsEnabled = false;

            _accessGroupWaitWindow = new WaitWindow();
            _accessGroupWaitWindow.Show(Application.Current.MainWindow);
            _accessGroupWaitWindow.Text = "Вывод данных в Excel";

            AccessGroupInitialize();
        }

        private static void AccessGroupInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _accessGroupCurrentRow = 1;

                _accessGroupApp = new Microsoft.Office.Interop.Excel.Application();
                _accessGroupWorkbook = _accessGroupApp.Workbooks.Add(Type.Missing);
                _accessGroupWorksheet = _accessGroupWorkbook.ActiveSheet;

                App.BaseClass.GetAdministrationClass(ref _accessGroupAdminClass);
                App.BaseClass.GetStaffClass(ref _accessGroupSc);

                _accessGroupNameConverter = new IdToAccessGroupNameConverter();

                FillAccessGroupData();
                SetAccessGroupColumnWidth();
                AccessGroupOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _accessGroupExportedControl.IsEnabled = true;
                _accessGroupWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillAccessGroupHeader(string factoryName)
        {
            _accessGroupCurrentRow++;
            _accessGroupRange = GetAccessGroupRange(_accessGroupCurrentRow, 1, _accessGroupCurrentRow, 3);
            _accessGroupRange.Merge();
            _accessGroupRange.Font.Bold = true;
            _accessGroupRange.Font.Size = 14;
            var headerText = string.Format("Таблица групп доступа по фабрике '{0}'", factoryName);
            _accessGroupWorksheet.Cells[_accessGroupCurrentRow, 1] = headerText;

            _accessGroupCurrentRow += 2;
            _accessGroupRange = GetAccessGroupRange(_accessGroupCurrentRow, 1, _accessGroupCurrentRow, 3);
            _accessGroupRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _accessGroupRange.Borders.Weight = XlBorderWeight.xlMedium;
            _accessGroupRange.Font.Bold = true;
            _accessGroupWorksheet.Cells[_accessGroupCurrentRow, 1] = "Группа доступа";
            _accessGroupWorksheet.Cells[_accessGroupCurrentRow, 2] = "Доступные модули";
            _accessGroupWorksheet.Cells[_accessGroupCurrentRow, 3] = "Работники";
        }

        private static void FillAccessGroupData()
        {
            foreach (var factory in _accessGroupSc.FactoriesDataTable.AsEnumerable())
            {
                var factoryId = Convert.ToInt32(factory["FactoryID"]);
                var factoryName = factory["FactoryName"].ToString();
                FillAccessGroupHeader(factoryName);

                var accessGroupView = GetAccessGroupView(factoryId);
                accessGroupView.Sort = "AccessGroupID, WorkerName";
                _accessGroupCurrentRow++;
                _accessGroupStartRow = _accessGroupCurrentRow;
                int currentAccessGroupId = 0;
                int workersRowNumber = _accessGroupCurrentRow;
                int accessGroupModulesRowNumber = _accessGroupCurrentRow;

                var separateRowIndexex = new List<int>();
                var idex = 1;

                foreach (DataRowView accessGroup in accessGroupView)
                {
                    var accessGroupId = Convert.ToInt32(accessGroup["AccessGroupID"]);
                    if (accessGroupId != currentAccessGroupId)
                    {
                        _accessGroupCurrentRow = workersRowNumber >= accessGroupModulesRowNumber
                            ? workersRowNumber
                            : accessGroupModulesRowNumber;

                        separateRowIndexex.Add(_accessGroupCurrentRow);

                        workersRowNumber = _accessGroupCurrentRow;
                        accessGroupModulesRowNumber = _accessGroupCurrentRow;

                        currentAccessGroupId = accessGroupId;
                        _accessGroupWorksheet.Cells[_accessGroupCurrentRow, 1] =
                            _accessGroupNameConverter.Convert(currentAccessGroupId, typeof(string), null, new CultureInfo("ru-RU"));

                        var accessModules = _accessGroupAdminClass.
                            GetAvailableModulesForAccessGroup(currentAccessGroupId).AsDataView();
                        accessModules.Sort = "ModuleName";
                        foreach (DataRowView module in accessModules)
                        {
                            var moduleName = module["ModuleName"].ToString();
                            _accessGroupWorksheet.Cells[accessGroupModulesRowNumber, 2] = moduleName;
                            accessGroupModulesRowNumber++;
                        }

                        double percent = (double)idex / accessGroupView.Count;
                        Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                        {
                            _accessGroupWaitWindow.Progress =
                                percent * 100;
                            _accessGroupWaitWindow.Text =
                                String.Format(
                                    "Вывод данных в Excel по фабрике '{0}' {1}%", factoryName, (int)(percent * 100));
                        }));
                    }

                    _accessGroupWorksheet.Cells[workersRowNumber, 3] = accessGroup["WorkerName"].ToString();
                    workersRowNumber++;
                    idex++;
                }

                _accessGroupCurrentRow = workersRowNumber >= accessGroupModulesRowNumber
                            ? workersRowNumber
                            : accessGroupModulesRowNumber;

                _accessGroupRange = GetAccessGroupRange(_accessGroupStartRow, 1, _accessGroupCurrentRow - 1, 3);
                _accessGroupRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                _accessGroupRange.Borders.Weight = XlBorderWeight.xlThin;

                foreach (var rowIndex in separateRowIndexex)
                {
                    _accessGroupRange = GetAccessGroupRange(rowIndex, 1, rowIndex, 3);
                    _accessGroupRange.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                    _accessGroupRange.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;
                }

                _accessGroupRange = GetAccessGroupRange(_accessGroupStartRow - 1, 1, _accessGroupCurrentRow - 1, 3);
                _accessGroupRange.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, 
                    XlColorIndex.xlColorIndexAutomatic, Type.Missing, Type.Missing);
            }
        }

        private static void SetAccessGroupColumnWidth()
        {
            _accessGroupRange = _accessGroupWorksheet.Rows;
            _accessGroupRange.Columns.AutoFit();
        }

        private static void AccessGroupOpenReport()
        {
            _accessGroupApp.Visible = true;

            Marshal.ReleaseComObject(_accessGroupWorkbook);
            Marshal.ReleaseComObject(_accessGroupApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        private static DataView GetAccessGroupView(int factoryId)
        {
            var view = _accessGroupAdminClass.AccessGroupStructureTable.AsEnumerable().
                Where(aG => _accessGroupSc.WorkerProfessionsDataTable.AsEnumerable().
                    Any(wP => wP.Field<object>("FactoryID") != null && wP.Field<Int64>("FactoryID") == factoryId &&
                        wP.Field<Int64>("WorkerID") == aG.Field<Int64>("WorkerID")));

            //Result table
            var table = new DataTable();
            table.Columns.Add("AccessGroupID", typeof(Int64));
            table.Columns.Add("WorkerName", typeof(string));

            var accessGroupView = view.CopyToDataTable().AsEnumerable().
                Join(_accessGroupSc.StaffPersonalInfoDataTable.AsEnumerable(), outer => outer["WorkerID"], inner => inner["WorkerID"],
                (outer, inner) =>
                {
                    var newRow = table.NewRow();
                    newRow["AccessGroupID"] = outer["AccessGroupID"];
                    newRow["WorkerName"] = inner["Name"];
                    return newRow;
                });

            return accessGroupView.CopyToDataTable().AsDataView();
        }

        private static Range GetAccessGroupRange(int y1, int x1, int y2, int x2)
        {
            return _accessGroupWorksheet.Range[_accessGroupWorksheet.Cells[y1, x1], _accessGroupWorksheet.Cells[y2, x2]];
        }

        #endregion


        #region ServiceEquipmentCrashStatisticsReport

        public static void GenerateServiceEquipmentCrashStatisticsReport(ref SwitchComboBox exportedElement)
        {
            _serviceEquipmentCrashStatisticsWorkSubSectionId = -1;
            _serviceEquipmentCrashStatisticsExportedControl = exportedElement;
            _serviceEquipmentCrashStatisticsExportedControl.IsEnabled = false;

            _serviceEquipmentCrashStatisticsWaitWindow = new WaitWindow();
            _serviceEquipmentCrashStatisticsWaitWindow.Show(Application.Current.MainWindow);
            _serviceEquipmentCrashStatisticsWaitWindow.Text = "Инициализация";

            ServiceEquipmentCrashStatisticsInitialize();
        }

        public static void GenerateServiceEquipmentCrashStatisticsReport(int workSubSectionId, ref SwitchComboBox exportedElement)
        {
            _serviceEquipmentCrashStatisticsWorkSubSectionId = workSubSectionId;
            _serviceEquipmentCrashStatisticsExportedControl = exportedElement;
            _serviceEquipmentCrashStatisticsExportedControl.IsEnabled = false;

            _serviceEquipmentCrashStatisticsWaitWindow = new WaitWindow();
            _serviceEquipmentCrashStatisticsWaitWindow.Show(Application.Current.MainWindow);
            _serviceEquipmentCrashStatisticsWaitWindow.Text = "Инициализация";

            ServiceEquipmentCrashStatisticsInitialize();
        }

        private static void ServiceEquipmentCrashStatisticsInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
                         {
                             _serviceEquipmentCrashStatisticsCurrentRow = 1;
                             _serviceEquipmentCrashStatisticsRowIndex = 0;
                             _serviceEquipmentCrashStattisticsCurrentDate = App.BaseClass.GetDateFromSqlServer();

                             _serviceEquipmentCrashStatisticsApp = new Microsoft.Office.Interop.Excel.Application();
                             _serviceEquipmentCrashStatisticsWorkbook =
                                 _serviceEquipmentCrashStatisticsApp.Workbooks.Add(Type.Missing);
                             _serviceEquipmentCrashStatisticsWorksheet =
                                 _serviceEquipmentCrashStatisticsWorkbook.ActiveSheet;

                             App.BaseClass.GetServiceEquipmentClass(
                                 ref _serviceEquipmentCrashStatisticsServiceEquipmentClass);
                             App.BaseClass.GetTaskClass(ref _serviceEquipmentCrashStatisticsTaskClass);
                             _serviceEquipmentCrashStatisticsTaskClass.Fill(
                                 _serviceEquipmentCrashStatisticsServiceEquipmentClass.DateFrom,
                                 _serviceEquipmentCrashStatisticsServiceEquipmentClass.DateTo);

                             _serviceEquipmentCrashStatisticsWorkSubSectionNameConverter = new IdToWorkSubSectionConverter();
                             _serviceEquipmentCrashStatisticsFactoryNameConverter = new IdToFactoryConverter();
                             _serviceEquipmentCrashStatisticsWorkerNameConverter = new IdToNameConverter();

                             SetServiceEquipmentCrashStatisticsDefaultStyle();
                             FillServiceEquipmentCrashStatisticsData();
                             ServiceEquipmentCrashStatisticsFreezePanes();
                             SetServiceEquipmentCrashStatisticsFormat();
                             SetServiceEquipmentCrashStatisticsColumnWidth();
                             ServiceEquipmentCrashStatisticsOpenReport();
                         };

            bw.RunWorkerCompleted += (sender, args) =>
                                     {
                                         _serviceEquipmentCrashStatisticsExportedControl.IsEnabled = true;
                                         _serviceEquipmentCrashStatisticsWaitWindow.Close(true);
                                         bw.Dispose();
                                     };

            bw.RunWorkerAsync();
        }

        private static void SetServiceEquipmentCrashStatisticsDefaultStyle()
        {
            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Rows;
            _serviceEquipmentCrashStatisticsRange.Font.Name = "Arial";
            _serviceEquipmentCrashStatisticsRange.Font.Size = 10;
        }

        private static void FillServiceEquipmentCrashStatisticsData()
        {
            FillServiceEquipmentCrashStatisticsHeader();

            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _serviceEquipmentCrashStatisticsWaitWindow.Text =
                    "Формирование таблиц";
            }));

            var crashTable = _serviceEquipmentCrashStatisticsWorkSubSectionId != -1
                ? _serviceEquipmentCrashStatisticsServiceEquipmentClass.Table.AsEnumerable()
                    .Where(r => r.Field<Int64>("WorkSubSectionID") == _serviceEquipmentCrashStatisticsWorkSubSectionId)
                : _serviceEquipmentCrashStatisticsServiceEquipmentClass.Table.AsEnumerable();

            var notClosedRequestsQuery = crashTable.Where(r => !r.Field<bool>("RequestClose"));

            var closedRequestsQuery = crashTable.Where(r => r.Field<bool>("RequestClose"));

            _serviceEquipmentCrashStatisticsRowsCount = notClosedRequestsQuery.Count() + closedRequestsQuery.Count();

            _serviceEquipmentCrashStatisticsCurrentRow++;
            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
            _serviceEquipmentCrashStatisticsRange.Interior.Color = XlRgbColor.rgbOrangeRed;
            _serviceEquipmentCrashStatisticsRange.Font.Color = XlRgbColor.rgbWhite;
            _serviceEquipmentCrashStatisticsRange.Font.Size = 14;
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] = "В работе";

            if (!notClosedRequestsQuery.Any())
            {
                _serviceEquipmentCrashStatisticsCurrentRow ++;
                _serviceEquipmentCrashStatisticsRange =
                    GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                        _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
                _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] =
                    "Отсутствуют";
            }
            else
            {
                FillServiceEquipmentCrashStatistics(notClosedRequestsQuery.CopyToDataTable());
            }

            _serviceEquipmentCrashStatisticsCurrentRow++;
            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
            _serviceEquipmentCrashStatisticsRange.Interior.Color = XlRgbColor.rgbGreen;
            _serviceEquipmentCrashStatisticsRange.Font.Color = XlRgbColor.rgbWhite;
            _serviceEquipmentCrashStatisticsRange.Font.Size = 14;
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] = "Завершены";

            if (!closedRequestsQuery.Any())
            {
                _serviceEquipmentCrashStatisticsCurrentRow++;
                _serviceEquipmentCrashStatisticsRange =
                    GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                        _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
                _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] =
                    "Отсутствуют";
            }
            else
            {
                FillServiceEquipmentCrashStatistics(closedRequestsQuery.CopyToDataTable());
            }
        }

        private static void FillServiceEquipmentCrashStatistics(DataTable crashTable)
        {
            var index = 0;
            _serviceEquipmentCrashStatisticsCurrentRow++;
            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
            _serviceEquipmentCrashStatisticsRange.Interior.Color =
                XlRgbColor.rgbLightSteelBlue;
            _serviceEquipmentCrashStatisticsRange.Font.Size = 12;
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] =
                "Поломки";

            foreach (
                var crashRow in
                    crashTable.AsEnumerable()
                        .Where(r => r.Field<Int64>("RequestTypeID") == 1))
            {
                index++;
                FillServiceEquipmentCrashStatiscticsInfo(crashRow, index);
            }


            _serviceEquipmentCrashStatisticsCurrentRow++;
            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
            _serviceEquipmentCrashStatisticsRange.Interior.Color =
                XlRgbColor.rgbLightSteelBlue;
            _serviceEquipmentCrashStatisticsRange.Font.Size = 12;
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] =
                "Замечания";

            foreach (
                var crashRow in
                    crashTable.AsEnumerable()
                        .Where(r => r.Field<Int64>("RequestTypeID") == 2))
            {
                index++;
                FillServiceEquipmentCrashStatiscticsInfo(crashRow, index);
            }
        }

        private static void FillServiceEquipmentCrashStatiscticsInfo(DataRow infoRow, int index)
        {
            _serviceEquipmentCrashStatisticsCurrentRow++;
            _serviceEquipmentCrashStatisticsRowIndex++;

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] = index;

            FillServiceEquipmentCrashStatisticsRequestInfo(infoRow);

            if (infoRow["ReceivedDate"] != DBNull.Value)
            {
                FillServiceEquipmentCrashStatisticsReceivedInfo(infoRow);
                FillServiceEquipmentCrashStatisticsWorkersInProcess(infoRow);

                if (infoRow["CompletionDate"] != DBNull.Value)
                {
                    FillServiceEquipmentCrashStatisticsCompletionInfo(infoRow);

                    if (infoRow["LaunchDate"] != DBNull.Value)
                    {
                        FillServiceEquipmentCrashStatisticsLaunchInfo(infoRow);
                    }
                }
            }

            if (!Convert.ToBoolean(infoRow["RequestClose"]))
            {
                _serviceEquipmentCrashStatisticsRange =
                    _serviceEquipmentCrashStatisticsWorksheet.Rows[_serviceEquipmentCrashStatisticsCurrentRow];
                _serviceEquipmentCrashStatisticsRange.Font.Color = XlRgbColor.rgbOrangeRed;
            }
            else
            {
                _serviceEquipmentCrashStatisticsRange =
                    _serviceEquipmentCrashStatisticsWorksheet.Rows[_serviceEquipmentCrashStatisticsCurrentRow];
                _serviceEquipmentCrashStatisticsRange.Font.Color = XlRgbColor.rgbGreen;
            }

            var percent = (double)_serviceEquipmentCrashStatisticsRowIndex /
                              _serviceEquipmentCrashStatisticsRowsCount;
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _serviceEquipmentCrashStatisticsWaitWindow.Progress =
                    percent * 100;
                _serviceEquipmentCrashStatisticsWaitWindow.Text =
                    String.Format(
                        "Вывод данных в Excel {0}%", (int)(percent * 100));
            }));
        }

        private static void FillServiceEquipmentCrashStatisticsRequestInfo(DataRow requestRow)
        {
            var factoryId = Convert.ToInt32(requestRow["FactoryID"]);
            var workSubSectionId = Convert.ToInt32(requestRow["WorkSubSectionID"]);
            var requestDate = Convert.ToDateTime(requestRow["RequestDate"]);
            var requestWorkerId = Convert.ToInt32(requestRow["RequestWorkerID"]);

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 2] = requestDate.Date;
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 3] = requestDate.ToString("HH:mm");

            var span = TimeSpan.Zero;

            if (requestRow["LaunchDate"] != DBNull.Value)
            {
                DateTime stopTime;
                var success = DateTime.TryParse(requestRow["LaunchDate"].ToString(), out stopTime);
                if (success)
                {
                    span = stopTime.Subtract(requestDate);
                }
            }
            else
            {
                span = _serviceEquipmentCrashStattisticsCurrentDate.Subtract(requestDate);
            }

            if (span.Days > 0)
            {
                _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 10] =
                    Math.Round(span.TotalDays, 1);
            }

            var totalHours = span.TotalHours;
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 11] =
                Math.Round(totalHours, 1);

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 13] =
                _serviceEquipmentCrashStatisticsWorkSubSectionNameConverter.Convert(workSubSectionId, typeof(string),
                    null, CultureInfo.InvariantCulture);
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 12] =
                _serviceEquipmentCrashStatisticsFactoryNameConverter.Convert(factoryId, typeof (string), null,
                    CultureInfo.InvariantCulture);
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 15] =
                _serviceEquipmentCrashStatisticsWorkerNameConverter.Convert(requestWorkerId, "ShortName");
        }

        private static void FillServiceEquipmentCrashStatisticsReceivedInfo(DataRow receivedRow)
        {
            var receivedDate = Convert.ToDateTime(receivedRow["ReceivedDate"]);
            var receivedWorkerId = Convert.ToInt32(receivedRow["ReceivedWorkerID"]);
            var receivedNotes = receivedRow["ReceivedNotes"].ToString();

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 14] = receivedNotes;
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 4] = receivedDate.Date;
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 5] = receivedDate.ToString("HH:mm");
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 16] =
                _serviceEquipmentCrashStatisticsWorkerNameConverter.Convert(receivedWorkerId, "ShortName");
        }

        private static void FillServiceEquipmentCrashStatisticsCompletionInfo(DataRow completionRow)
        {
            var completionDate = Convert.ToDateTime(completionRow["CompletionDate"]);
            //var completionNotes = completionRow["CompletionNotes"].ToString();

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 6] = completionDate.Date;
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 7] = completionDate.ToString("HH:mm");

            //_serviceEquipmentCrashStatisticsWorksheet.
            //    Cells[_serviceEquipmentCrashStatisticsCurrentRow, 15] = completionNotes;
        }

        private static void FillServiceEquipmentCrashStatisticsWorkersInProcess(DataRow row)
        {
            var globalId = row["GlobalID"].ToString();
            var tasks =
                _serviceEquipmentCrashStatisticsTaskClass.Tasks.Table.Select(string.Format("GlobalID = {0}", globalId));
            if (tasks.Any())
            {
                var task = tasks.First();
                var taskId = Convert.ToInt32(task["TaskID"]);

                var workerIds =
                    from performer in
                        _serviceEquipmentCrashStatisticsTaskClass.Performers.Table.AsEnumerable()
                            .Where(p => p.Field<Int64>("TaskID") == taskId)
                    select Convert.ToInt32(performer["WorkerID"]);

                if (workerIds.Any())
                {
                    var completionWorkers = workerIds.Aggregate(string.Empty,
                    (current, workerId) =>
                        current +
                        string.Format("{0}\n",
                            _serviceEquipmentCrashStatisticsWorkerNameConverter.Convert(workerId, "ShortName")));
                    completionWorkers = completionWorkers.Remove(completionWorkers.Length - 2);
                    _serviceEquipmentCrashStatisticsWorksheet.
                        Cells[_serviceEquipmentCrashStatisticsCurrentRow, 17] = completionWorkers;
                }
            }
        }

        private static void FillServiceEquipmentCrashStatisticsLaunchInfo(DataRow launchRow)
        {
            var launchDate = Convert.ToDateTime(launchRow["LaunchDate"]);
            var launchWorkerId = Convert.ToInt32(launchRow["LaunchWorkerID"]);
            var launchNotes = launchRow["LaunchNotes"].ToString();

            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 8] = launchDate.Date;
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 9] = launchDate.ToString("HH:mm");
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 18] =
                _serviceEquipmentCrashStatisticsWorkerNameConverter.Convert(launchWorkerId,
                    "ShortName");
            _serviceEquipmentCrashStatisticsWorksheet.
                Cells[_serviceEquipmentCrashStatisticsCurrentRow, 19] = launchNotes;
        }

        private static void FillServiceEquipmentCrashStatisticsHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                                                                  {
                                                                      _serviceEquipmentCrashStatisticsWaitWindow.Text =
                                                                          "Формирование шапки";
                                                                  }));

            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, 10);
            _serviceEquipmentCrashStatisticsRange.Merge();
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;
            _serviceEquipmentCrashStatisticsRange.Font.Size = 12;
            var headerText = string.Format("Статистика за период с {0:dd.MM.yyyy} по {1:dd.MM.yyyy}",
                _serviceEquipmentCrashStatisticsServiceEquipmentClass.DateFrom,
                _serviceEquipmentCrashStatisticsServiceEquipmentClass.DateTo);
            if (_serviceEquipmentCrashStatisticsWorkSubSectionId != -1)
            {
                headerText += string.Format(" по оборудованию: {0}",
                    _serviceEquipmentCrashStatisticsWorkSubSectionNameConverter.Convert(
                        _serviceEquipmentCrashStatisticsWorkSubSectionId, typeof (string), null,
                        CultureInfo.InvariantCulture));
            }
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] = headerText;

            _serviceEquipmentCrashStatisticsCurrentRow += 2;
            _serviceEquipmentCrashStatisticsRange =
                GetServiceEquipmentCrashStatisticsRange(_serviceEquipmentCrashStatisticsCurrentRow, 1,
                    _serviceEquipmentCrashStatisticsCurrentRow, ServiceEquipmentCrashStattisticsColumnsCount);
            _serviceEquipmentCrashStatisticsRange.Interior.Color =
                XlRgbColor.rgbSteelBlue;
            _serviceEquipmentCrashStatisticsRange.Font.Color = XlRgbColor.rgbWhite;
            _serviceEquipmentCrashStatisticsRange.Font.Bold = true;

            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 1] = "№ п/п";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 2] =
                "Заявка поступила";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 3] = "время";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 4] =
                "Заявка принята";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 5] =
                "время принятия";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 6] =
                "Работа выполнена";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 7] =
                "время вып-ия";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 8] =
                "Оборудование запущено";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 9] =
                "время зав-ия";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 10] =
                "Простой (дни)";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 11] =
                "Простой (часы)";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 12] =
                "Фирма";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 13] =
                "Оборудование";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 14] =
                "Описание вопроса";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 15] =
                "Постановщик";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 16] =
                "Принял заявку";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 17] =
                "Исполнитель(и)";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 18] =
                "Произвёл запуск оборудования";
            _serviceEquipmentCrashStatisticsWorksheet.Cells[_serviceEquipmentCrashStatisticsCurrentRow, 19] =
                "Результат, заключение";
        }

        private static void SetServiceEquipmentCrashStatisticsFormat()
        {
            var range = GetServiceEquipmentCrashStatisticsRange(3, 1, _serviceEquipmentCrashStatisticsCurrentRow,
                ServiceEquipmentCrashStattisticsColumnsCount);
            FormatServiceEquipmentCrashStatisticsAsTable(range, "MyTable", "TableStyleMedium2");
            range.VerticalAlignment = Constants.xlTop;
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;

            range = GetServiceEquipmentCrashStatisticsRange(3, 4, _serviceEquipmentCrashStatisticsCurrentRow, 4);
            var border = range.Borders[XlBordersIndex.xlEdgeLeft];
            border.LineStyle = XlLineStyle.xlContinuous;
            border.Weight = XlBorderWeight.xlMedium;
            border.Color = Color.Black;

            range = GetServiceEquipmentCrashStatisticsRange(3, 6, _serviceEquipmentCrashStatisticsCurrentRow, 6);
            border = range.Borders[XlBordersIndex.xlEdgeLeft];
            border.LineStyle = XlLineStyle.xlContinuous;
            border.Weight = XlBorderWeight.xlMedium;
            border.Color = Color.Black;
        }

        private static void SetServiceEquipmentCrashStatisticsColumnWidth()
        {
            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Rows;
            _serviceEquipmentCrashStatisticsRange.Columns.AutoFit();

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[1];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 3;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[2];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 10;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[3];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 5.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[4];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 10;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[5];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 5.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[6];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 10;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[7];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 5.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[8];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 10;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[9];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 5.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[10];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 7.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[11];
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 7.5;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[12];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 8;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[13];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 19;
            _serviceEquipmentCrashStatisticsRange.WrapText = true;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[14];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 19;
            _serviceEquipmentCrashStatisticsRange.WrapText = true;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[17];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 20;
            _serviceEquipmentCrashStatisticsRange.WrapText = true;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Columns[19];
            _serviceEquipmentCrashStatisticsRange.ColumnWidth = 19;
            _serviceEquipmentCrashStatisticsRange.WrapText = true;

            _serviceEquipmentCrashStatisticsRange = _serviceEquipmentCrashStatisticsWorksheet.Rows[3];
            _serviceEquipmentCrashStatisticsRange.RowHeight = 40;
            _serviceEquipmentCrashStatisticsRange.HorizontalAlignment = Constants.xlCenter;
            _serviceEquipmentCrashStatisticsRange.WrapText = true;
        }

        private static void ServiceEquipmentCrashStatisticsFreezePanes()
        {
            ((Range)_serviceEquipmentCrashStatisticsWorksheet.Cells[4, 14]).Select();
            _serviceEquipmentCrashStatisticsApp.ActiveWindow.FreezePanes = true;
        }

        private static void FormatServiceEquipmentCrashStatisticsAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static Range GetServiceEquipmentCrashStatisticsRange(int y1, int x1, int y2, int x2)
        {
            return
                _serviceEquipmentCrashStatisticsWorksheet.Range[
                    _serviceEquipmentCrashStatisticsWorksheet.Cells[y1, x1],
                    _serviceEquipmentCrashStatisticsWorksheet.Cells[y2, x2]];
        }

        private static void ServiceEquipmentCrashStatisticsOpenReport()
        {
            _serviceEquipmentCrashStatisticsApp.Visible = true;

            Marshal.ReleaseComObject(_serviceEquipmentCrashStatisticsWorkbook);
            Marshal.ReleaseComObject(_serviceEquipmentCrashStatisticsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region ServiceEquipmentDiagrammReport

        public static void GenerateServiceEquipmentDiagrammReport(ref SwitchComboBox exportedElement)
        {
            _serviceEquipmentDiagrammExportedControl = exportedElement;
            _serviceEquipmentDiagrammExportedControl.IsEnabled = false;

            _serviceEquipmentDiagrammWaitWindow = new WaitWindow();
            _serviceEquipmentDiagrammWaitWindow.Show(Application.Current.MainWindow);
            Thread.Sleep(500);
            _serviceEquipmentDiagrammWaitWindow.Text = "Инициализация";

            ServiceEquipmentDiagrammInitialize();
        }

        private static void ServiceEquipmentDiagrammInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _serviceEquipmentDiagrammCurrentRow = 1;
                _serviceEquipmentDiagrammRowIndex = 0;

                _serviceEquipmentDiagrammApp = new Microsoft.Office.Interop.Excel.Application();
                _serviceEquipmentDiagrammWorkbook =
                    _serviceEquipmentDiagrammApp.Workbooks.Add(Type.Missing);
                _serviceEquipmentDiagrammWorksheet =
                    _serviceEquipmentDiagrammWorkbook.ActiveSheet;

                App.BaseClass.GetServiceEquipmentClass(
                    ref _serviceEquipmentDiagrammServiceEquipmentClass);
                var date = _serviceEquipmentDiagrammServiceEquipmentClass.DateTo;
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                _serviceEquipmentDiagrammColumnsCount = ServiceEquipmentDiagrammTableColumnsCount + daysInMonth;

                App.BaseClass.GetTaskClass(ref _serviceEquipmentDiagrammTaskClass);
                _serviceEquipmentDiagrammTaskClass.Fill(new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, daysInMonth));

                _serviceEquipmentDiagrammWorkSubSectionNameConverter = new IdToWorkSubSectionConverter();
                _serviceEquipmentDiagrammFactoryNameConverter = new IdToFactoryConverter();
                _serviceEquipmentDiagrammWorkerNameConverter = new IdToNameConverter();

                SetServiceEquipmentDiagrammDefaultStyle();
                FillServiceEquipmentDiagrammData();
                ServiceEquipmentDiagrammFreezePanes();
                SetServiceEquipmentDiagrammFormat();
                SetServiceEquipmentDiagrammColumnWidth();
                ServiceEquipmentDiagrammOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _serviceEquipmentDiagrammExportedControl.IsEnabled = true;
                _serviceEquipmentDiagrammWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void SetServiceEquipmentDiagrammDefaultStyle()
        {
            _serviceEquipmentDiagrammRange = _serviceEquipmentDiagrammWorksheet.Rows;
            _serviceEquipmentDiagrammRange.Font.Name = "Arial";
            _serviceEquipmentDiagrammRange.Font.Size = 10;
        }

        private static void FillServiceEquipmentDiagrammData()
        {
            FillServiceEquipmentDiagrammHeader();

            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _serviceEquipmentDiagrammWaitWindow.Text =
                    "Формирование таблиц";
            }));

            try
            {
                var date = _serviceEquipmentDiagrammServiceEquipmentClass.DateTo;
                var statisticTable = _serviceEquipmentDiagrammServiceEquipmentClass.GetServiceEquipmentDurringTheMonth(date.Year, date.Month);
                _serviceEquipmentDiagrammRowsCount = statisticTable.Rows.Count;

                var index = 0;
                foreach (var row in statisticTable.Select())
                {
                    index++;
                    _serviceEquipmentDiagrammRowIndex++;

                    FillServiceEquipmentDiagrammRow(row, index);

                    var percent = (double)_serviceEquipmentDiagrammRowIndex /
                              _serviceEquipmentDiagrammRowsCount;

                    Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        _serviceEquipmentDiagrammWaitWindow.Progress =
                            percent * 100;
                        _serviceEquipmentDiagrammWaitWindow.Text =
                            string.Format(
                                "Вывод данных в Excel {0}%", (int)(percent * 100));
                    }));
                }
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message + "\nДанные не обработаны корректно!");
                return;
            }
        }

        private static void FillServiceEquipmentDiagrammHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _serviceEquipmentDiagrammWaitWindow.Text =
                    "Формирование шапки";
            }));

            var date = _serviceEquipmentDiagrammServiceEquipmentClass.DateTo;

            _serviceEquipmentDiagrammRange =
                GetServiceEquipmentDiagrammRange(_serviceEquipmentDiagrammCurrentRow, 1,
                    _serviceEquipmentDiagrammCurrentRow, 3);
            _serviceEquipmentDiagrammRange.Merge();
            _serviceEquipmentDiagrammRange.Font.Bold = true;
            _serviceEquipmentDiagrammRange.Font.Size = 12;
            var headerText = string.Format("Отчёт за {0:MMMM} {0:yyyy}г.", date);
            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 1] = headerText;

            for (var i = 1; i <= DateTime.DaysInMonth(date.Year, date.Month); i++)
            {
                var day = new DateTime(date.Year, date.Month, i);
                var weekDay = day.ToString("ddd", new CultureInfo("ru-RU"));

                var index = ServiceEquipmentDiagrammTableColumnsCount + i;
                _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, index] = weekDay;
            }

            _serviceEquipmentDiagrammCurrentRow++;
            _serviceEquipmentDiagrammRange =
                GetServiceEquipmentDiagrammRange(_serviceEquipmentDiagrammCurrentRow, 1,
                    _serviceEquipmentDiagrammCurrentRow, 3);
            _serviceEquipmentDiagrammRange.Merge();
            _serviceEquipmentDiagrammRange.Font.Bold = true;
            _serviceEquipmentDiagrammRange.Font.Size = 12;
            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 1] = "ДИАГРАММА РЕМОНТОВ И АВАРИЙ";

            _serviceEquipmentDiagrammCurrentRow++;
            _serviceEquipmentDiagrammRange =
                GetServiceEquipmentDiagrammRange(_serviceEquipmentDiagrammCurrentRow, 2,
                    _serviceEquipmentDiagrammCurrentRow, 3);
            _serviceEquipmentDiagrammRange.Merge();
            _serviceEquipmentDiagrammRange.Interior.Color = XlRgbColor.rgbYellow;
            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 2] = "Неисправности";
            _serviceEquipmentDiagrammCurrentRow++;
            _serviceEquipmentDiagrammRange =
                GetServiceEquipmentDiagrammRange(_serviceEquipmentDiagrammCurrentRow, 2,
                    _serviceEquipmentDiagrammCurrentRow, 3);
            _serviceEquipmentDiagrammRange.Merge();
            _serviceEquipmentDiagrammRange.Interior.Color = XlRgbColor.rgbOrangeRed;
            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 2] = "Поломки";

            _serviceEquipmentDiagrammCurrentRow ++;
            _serviceEquipmentDiagrammRange =
                GetServiceEquipmentDiagrammRange(_serviceEquipmentDiagrammCurrentRow, 1,
                    _serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount);
            _serviceEquipmentDiagrammRange.Font.Bold = true;

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 1] = "№ п/п";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 2] = "Оборудование";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 3] = "Фирма";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 4] = "Участники";
            ((Range)_serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 4]).ColumnWidth = 50;

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 5] = "начало";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 6] = "время";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 7] = "завершение";

            _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, 8] = "время";

            for (var i = 1; i <= DateTime.DaysInMonth(date.Year, date.Month); i++)
            {
                var day = new DateTime(date.Year, date.Month, i);
                var weekDay = day.ToString("ddd", new CultureInfo("ru-RU"));

                var index = ServiceEquipmentDiagrammTableColumnsCount + i;
                _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow - 1, index] = i;
                _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, index] = weekDay;
            }
        }

        private static void FillServiceEquipmentDiagrammRow(DataRow serviceEquipmentRow, int index)
        {
            _serviceEquipmentDiagrammCurrentRow ++;         

            FillServiceEquipmentDiagrammRowInfo(serviceEquipmentRow, index);

            FillServiceEquipmentDiagrammInfo(serviceEquipmentRow);
        }

        private static void FillServiceEquipmentDiagrammRowInfo(DataRow infoRow, int index)
        {
            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 1] = index;

            FillServiceEquipmentDiagrammRequestInfo(infoRow);

            if (infoRow["ReceivedDate"] != DBNull.Value)
            {
                FillServiceEquipmentDiagrammWorkersInProcess(infoRow);

                if (infoRow["LaunchDate"] != DBNull.Value)
                {
                    FillServiceEquipmentDiagrammLaunchInfo(infoRow);
                }
            }
        }

        private static void FillServiceEquipmentDiagrammRequestInfo(DataRow requestRow)
        {
            var factoryId = Convert.ToInt32(requestRow["FactoryID"]);
            var workSubSectionId = Convert.ToInt32(requestRow["WorkSubSectionID"]);
            var requestDate = Convert.ToDateTime(requestRow["RequestDate"]);
            
            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 2] = 
                _serviceEquipmentDiagrammWorkSubSectionNameConverter.Convert(workSubSectionId, typeof(string),
                    null, CultureInfo.InvariantCulture);

            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 3] =
                _serviceEquipmentDiagrammFactoryNameConverter.Convert(factoryId, typeof(string), null,
                    CultureInfo.InvariantCulture);

            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 5] = requestDate.Date;
            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 6] = requestDate.ToString("HH:mm");
        }

        private static void FillServiceEquipmentDiagrammWorkersInProcess(DataRow row)
        {
            var globalId = row["GlobalID"].ToString();
            var tasks =
                _serviceEquipmentDiagrammTaskClass.Tasks.Table.Select(string.Format("GlobalID = {0}", globalId));
            if (tasks.Any())
            {
                var task = tasks.First();
                var taskId = Convert.ToInt32(task["TaskID"]);

                var workerIds =
                    from performer in
                        _serviceEquipmentDiagrammTaskClass.Performers.Table.AsEnumerable()
                            .Where(p => p.Field<Int64>("TaskID") == taskId)
                    select Convert.ToInt32(performer["WorkerID"]);

                if (workerIds.Any())
                {
                    var completionWorkers = workerIds.Aggregate(string.Empty,
                    (current, workerId) =>
                        current +
                        string.Format("{0} ",
                            _serviceEquipmentDiagrammWorkerNameConverter.Convert(workerId, "ShortName")));
                    completionWorkers = completionWorkers.Remove(completionWorkers.Length - 1);
                    _serviceEquipmentDiagrammWorksheet.
                        Cells[_serviceEquipmentDiagrammCurrentRow, 4] = completionWorkers;
                }
            }
        }

        private static void FillServiceEquipmentDiagrammLaunchInfo(DataRow launchRow)
        {
            var launchDate = Convert.ToDateTime(launchRow["LaunchDate"]);
            var launchWorkerId = Convert.ToInt32(launchRow["LaunchWorkerID"]);
            var launchNotes = launchRow["LaunchNotes"].ToString();

            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 7] = launchDate.Date;
            _serviceEquipmentDiagrammWorksheet.
                Cells[_serviceEquipmentDiagrammCurrentRow, 8] = launchDate.ToString("HH:mm");
        }

        private static void FillServiceEquipmentDiagrammInfo(DataRow row)
        {
            var date = _serviceEquipmentDiagrammServiceEquipmentClass.DateTo;
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var requestTypeId = Convert.ToInt32(row["RequestTypeID"]);
            var requestDate = Convert.ToDateTime(row["RequestDate"]);
            var launchDate = row["LaunchDate"] != DBNull.Value 
                ? Convert.ToDateTime(row["LaunchDate"])
                : DateTime.MinValue;

            if(requestTypeId == 1)
            {
                var startDate = requestDate.Year == date.Year && requestDate.Month == date.Month
                    ? requestDate : new DateTime(date.Year, date.Month, 1);
                var endDate = launchDate.Year == date.Year && launchDate.Month == date.Month
                    ? launchDate : new DateTime(date.Year, date.Month, daysInMonth);
                var range =
                    GetServiceEquipmentDiagrammRange(
                        _serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + startDate.Day, 
                        _serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + endDate.Day);
                range.Interior.Color = XlRgbColor.rgbOrangeRed;

                if(requestDate.Date == launchDate.Date)
                {
                    _serviceEquipmentDiagrammWorksheet.
                        Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + endDate.Day] = "Н/К";
                }
                else
                {
                    if(requestDate == startDate)
                        _serviceEquipmentDiagrammWorksheet.
                            Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + startDate.Day] = "Н";

                    if (launchDate == endDate)
                        _serviceEquipmentDiagrammWorksheet.
                            Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + endDate.Day] = "К";
                }
            }
            else
            {
                if(requestDate.Year == date.Year && requestDate.Month == date.Month)
                {
                    Range range = 
                        _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + requestDate.Day];
                    range.Interior.Color = XlRgbColor.rgbYellow;

                    _serviceEquipmentDiagrammWorksheet.
                            Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + requestDate.Day] = "Н";
                }

                if (launchDate.Year == date.Year && launchDate.Month == date.Month)
                {
                    Range range =
                        _serviceEquipmentDiagrammWorksheet.Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + launchDate.Day];
                    range.Interior.Color = XlRgbColor.rgbYellow;

                    _serviceEquipmentDiagrammWorksheet.
                            Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + launchDate.Day] = "К";
                }

                if (requestDate.Date == launchDate.Date)
                {
                    _serviceEquipmentDiagrammWorksheet.
                        Cells[_serviceEquipmentDiagrammCurrentRow, ServiceEquipmentDiagrammTableColumnsCount + launchDate.Day] = "Н/К";
                }
            }
        }

        private static void SetServiceEquipmentDiagrammFormat()
        {
            var range = GetServiceEquipmentDiagrammRange(5, 1, _serviceEquipmentDiagrammCurrentRow,
                ServiceEquipmentDiagrammTableColumnsCount);
            FormatServiceEquipmentDiagrammAsTable(range, "MyTable", "TableStyleMedium2");
            range.VerticalAlignment = Constants.xlTop;
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;

            var date = _serviceEquipmentDiagrammServiceEquipmentClass.DateTo;
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var startDiagramm = ServiceEquipmentDiagrammTableColumnsCount + 1;
            range = GetServiceEquipmentDiagrammRange(4, startDiagramm, _serviceEquipmentDiagrammCurrentRow, (startDiagramm - 1) + daysInMonth);
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void SetServiceEquipmentDiagrammColumnWidth()
        {
            _serviceEquipmentDiagrammRange = _serviceEquipmentDiagrammWorksheet.Rows;
            _serviceEquipmentDiagrammRange.Columns.AutoFit();
        }

        private static void ServiceEquipmentDiagrammFreezePanes()
        {
            ((Range)_serviceEquipmentDiagrammWorksheet.Cells[6, 9]).Select();
            _serviceEquipmentDiagrammApp.ActiveWindow.FreezePanes = true;
        }

        private static void FormatServiceEquipmentDiagrammAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static Range GetServiceEquipmentDiagrammRange(int y1, int x1, int y2, int x2)
        {
            return
                _serviceEquipmentDiagrammWorksheet.Range[
                    _serviceEquipmentDiagrammWorksheet.Cells[y1, x1],
                    _serviceEquipmentDiagrammWorksheet.Cells[y2, x2]];
        }

        private static void ServiceEquipmentDiagrammOpenReport()
        {
            _serviceEquipmentDiagrammApp.Visible = true;

            Marshal.ReleaseComObject(_serviceEquipmentDiagrammWorkbook);
            Marshal.ReleaseComObject(_serviceEquipmentDiagrammApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region TasksStatisticReport

        public static void GenerateTasksStatisticReport(int workerId)
        {
            if (_tasksStatisticIsReporting) return;

            _tasksStatisticIsReporting = true;

            _tasksStatisticWorkerId = workerId;

            _tasksStatisticWaitWindow = new WaitWindow();
            _tasksStatisticWaitWindow.Show(Application.Current.MainWindow);
            _tasksStatisticWaitWindow.Text = "Инициализация";

            TasksStatisticInitialize();
        }

        private static void TasksStatisticInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _tasksStatisticCurrentRow = 1;
                _tasksStatisticRowIndex = 0;

                _tasksStatisticApp = new Microsoft.Office.Interop.Excel.Application();
                _tasksStatisticWorkbook =
                    _tasksStatisticApp.Workbooks.Add(Type.Missing);
                _tasksStatisticWorksheet =
                    _tasksStatisticWorkbook.ActiveSheet;

                App.BaseClass.GetTaskClass(ref _tasksStatisticTaskClass);

                _tasksStatisticWorkerNameConverter = new IdToNameConverter();
                _tasksStatisticTimeSpanConverter = new TimeSpanConverter();

                SetTasksStatisticDefaultStyle();
                FillTasksStatisticData();
                TasksStatisticFreezePanes();
                SetTasksStatisticFormat();
                SetTasksStatisticColumnWidth();
                TasksStatisticOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _tasksStatisticIsReporting = false;
                _tasksStatisticWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void SetTasksStatisticDefaultStyle()
        {
            _tasksStatisticRange = _tasksStatisticWorksheet.Rows;
            _tasksStatisticRange.Select();
            _tasksStatisticRange.Font.Name = "Arial";
            _tasksStatisticRange.Font.Size = 10;
            _tasksStatisticRange.HorizontalAlignment = Constants.xlCenter;
        }

        private static void FillTasksStatisticData()
        {
            FillTasksStatisticHeader();

            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _tasksStatisticWaitWindow.Text =
                    "Формирование таблиц";
            }));

            var dateFrom = _tasksStatisticTaskClass.GetDateFrom();
            var dateTo = _tasksStatisticTaskClass.GetDateTo();

            var notClosedTasks =
                _tasksStatisticTaskClass.Tasks.Table.AsEnumerable()
                    .Where(
                        t =>
                            _tasksStatisticTaskClass.Performers.Table.AsEnumerable()
                                .Any(
                                    p =>
                                        p.Field<Int64>("TaskID") == t.Field<Int64>("TaskID") &&
                                        p.Field<Int64>("WorkerID") == _tasksStatisticWorkerId &&
                                        !p.Field<bool>("IsComplete")));

            var closedTasks =
                _tasksStatisticTaskClass.Tasks.Table.AsEnumerable()
                    .Where(
                        t =>
                            ((t.Field<DateTime>("CreationDate") >= dateFrom &&
                              t.Field<DateTime>("CreationDate") <= dateTo)
                             ||
                             (t.Field<object>("CompletionDate") != null &&
                              t.Field<DateTime>("CompletionDate") >= dateFrom &&
                              t.Field<DateTime>("CompletionDate") <= dateTo))
                            &&
                            _tasksStatisticTaskClass.Performers.Table.AsEnumerable()
                                .Any(
                                    p =>
                                        p.Field<Int64>("TaskID") == t.Field<Int64>("TaskID") &&
                                        p.Field<Int64>("WorkerID") == _tasksStatisticWorkerId &&
                                        p.Field<bool>("IsComplete")));

             _tasksStatisticRowsCount = notClosedTasks.Count() + closedTasks.Count();


            #region Not closed tasks

             _tasksStatisticCurrentRow++;
            _tasksStatisticRange =
                GetTasksStatisticRange(_tasksStatisticCurrentRow, 1, _tasksStatisticCurrentRow,
                    TasksStatisticColumnsCount);
            _tasksStatisticRange.Interior.Color = XlRgbColor.rgbOrangeRed;
            _tasksStatisticRange.Font.Color = XlRgbColor.rgbWhite;
            _tasksStatisticRange.Font.Bold = true;
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] = "В работе";

            if (!notClosedTasks.Any())
            {
                _tasksStatisticCurrentRow++;
                _tasksStatisticRange =
                    GetTasksStatisticRange(_tasksStatisticCurrentRow, 1,
                        _tasksStatisticCurrentRow, TasksStatisticColumnsCount);
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] =
                    "Задачи отсутствуют";
            }
            else
            {
                FillTasksStatisticRows(notClosedTasks.CopyToDataTable());
            }

             #endregion


            #region Closed tasks

            _tasksStatisticCurrentRow++;
            _tasksStatisticRange =
                GetTasksStatisticRange(_tasksStatisticCurrentRow, 1, _tasksStatisticCurrentRow,
                    TasksStatisticColumnsCount);
            _tasksStatisticRange.Interior.Color = XlRgbColor.rgbGreen;
            _tasksStatisticRange.Font.Color = XlRgbColor.rgbWhite;
            _tasksStatisticRange.Font.Bold = true;
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] = "Завершённые";

            if (!closedTasks.Any())
            {
                _tasksStatisticCurrentRow++;
                _tasksStatisticRange =
                    GetTasksStatisticRange(_tasksStatisticCurrentRow, 1,
                        _tasksStatisticCurrentRow, TasksStatisticColumnsCount);
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] =
                    "Задачи отсутствуют";
            }
            else
            {
                FillTasksStatisticRows(closedTasks.CopyToDataTable());
            }

            #endregion
        }

        private static void FillTasksStatisticHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _tasksStatisticWaitWindow.Text = "Формирование шапки";
            }));

            _tasksStatisticRange =
                GetTasksStatisticRange(_tasksStatisticCurrentRow, 1, _tasksStatisticCurrentRow, 10);
            _tasksStatisticRange.Merge();
            _tasksStatisticRange.Font.Bold = true;
            _tasksStatisticRange.Font.Size = 12;
            var headerText = string.Format("Статистика за период с {0:dd.MM.yyyy} по {1:dd.MM.yyyy} по работнику: {2}",
                _tasksStatisticTaskClass.GetDateFrom(),
                _tasksStatisticTaskClass.GetDateTo(),
                _tasksStatisticWorkerNameConverter.Convert(_tasksStatisticWorkerId, "FullName"));

            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 1] = headerText;

            _tasksStatisticCurrentRow += 2;
            _tasksStatisticRange = GetTasksStatisticRange(_tasksStatisticCurrentRow, 1, _tasksStatisticCurrentRow,
                TasksStatisticColumnsCount);
            _tasksStatisticRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _tasksStatisticRange.Font.Color = XlRgbColor.rgbWhite;
            _tasksStatisticRange.Font.Bold = true;

            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 1] = "№ п/п";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] = "Задача";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 3] = "Описание задачи";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 4] = "Дата создания";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 5] = "Крайний срок";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 6] = "Постановщик";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 7] = "Приступил(а)";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 8] = "Завершил(а)";
            _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 9] = "Затрачено времени";
        }

        private static void FillTasksStatisticRows(DataTable taskTable)
        {
            var index = 0;

            foreach ( var taskRow in taskTable.AsEnumerable())
            {
                index++;
                _tasksStatisticCurrentRow++;
                _tasksStatisticRowIndex++;

                var taskId = Convert.ToInt32(taskRow["TaskID"]);

                var performers =
                    _tasksStatisticTaskClass.Performers.Table.AsEnumerable()
                        .Where(
                            p =>
                                p.Field<Int64>("TaskID") == taskId &&
                                p.Field<Int64>("WorkerID") == _tasksStatisticWorkerId);
                if(!performers.Any()) continue;

                var performer = performers.First();

                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 1] = index;
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2] = taskRow["TaskName"].ToString();
                _tasksStatisticRange = _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 2];
                _tasksStatisticRange.Font.Color = XlRgbColor.rgbRoyalBlue;

                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 3] = taskRow["Description"].ToString();
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 4] = taskRow["CreationDate"];

                var hasDeadLine = Convert.ToBoolean(taskRow["IsDeadLine"]);
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 5] = hasDeadLine ? taskRow["DeadLine"] : "Отсутствует";
                
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 6] =
                    _tasksStatisticWorkerNameConverter.Convert(taskRow["MainWorkerID"], "ShortName");
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 7] = performer["StartDate"];
                _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 8] = performer["CompletionDate"];
                
                var result = _tasksStatisticTimeSpanConverter.Convert(performer["SpendTime"], typeof (TimeSpan),
                    "FromMinutes", CultureInfo.InvariantCulture);
                if (result != null)
                {
                    TimeSpan spendTime;
                    TimeSpan.TryParse(result.ToString(), out spendTime);
                    _tasksStatisticWorksheet.Cells[_tasksStatisticCurrentRow, 9] = Math.Round(spendTime.TotalHours, 2);
                }

                var percent = (double)_tasksStatisticRowIndex / _tasksStatisticRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _tasksStatisticWaitWindow.Progress = percent * 100;
                    _tasksStatisticWaitWindow.Text =
                        String.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void SetTasksStatisticFormat()
        {
            var range = GetTasksStatisticRange(3, 1, _tasksStatisticCurrentRow, TasksStatisticColumnsCount);
            FormatTasksStatisticAsTable(range, "MyTable", "TableStyleMedium16");
            range.VerticalAlignment = Constants.xlTop;
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void SetTasksStatisticColumnWidth()
        {
            _tasksStatisticRange = _tasksStatisticWorksheet.Rows;
            _tasksStatisticRange.Columns.AutoFit();

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[1];
            _tasksStatisticRange.ColumnWidth = 5;
            _tasksStatisticRange.WrapText = true;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[2];
            _tasksStatisticRange.ColumnWidth = 35;
            _tasksStatisticRange.WrapText = true;
            _tasksStatisticRange.HorizontalAlignment = Constants.xlLeft;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[3];
            _tasksStatisticRange.ColumnWidth = 30;
            _tasksStatisticRange.WrapText = true;
            _tasksStatisticRange.HorizontalAlignment = Constants.xlLeft;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[4];
            _tasksStatisticRange.HorizontalAlignment = Constants.xlLeft;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[6];
            _tasksStatisticRange.HorizontalAlignment = Constants.xlLeft;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[7];
            _tasksStatisticRange.HorizontalAlignment = Constants.xlLeft;

            _tasksStatisticRange = _tasksStatisticWorksheet.Columns[8];
            _tasksStatisticRange.ColumnWidth = 14;
            _tasksStatisticRange.WrapText = true;
            _tasksStatisticRange.HorizontalAlignment = Constants.xlRight;
        }

        private static void TasksStatisticFreezePanes()
        {
            ((Range) _tasksStatisticWorksheet.Cells[4, 2]).Select();
            _tasksStatisticApp.ActiveWindow.FreezePanes = true;
        }

        private static void FormatTasksStatisticAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static Range GetTasksStatisticRange(int y1, int x1, int y2, int x2)
        {
            return
                _tasksStatisticWorksheet.Range[
                    _tasksStatisticWorksheet.Cells[y1, x1],
                    _tasksStatisticWorksheet.Cells[y2, x2]];
        }

        private static void TasksStatisticOpenReport()
        {
            _tasksStatisticApp.Visible = true;

            Marshal.ReleaseComObject(_tasksStatisticWorkbook);
            Marshal.ReleaseComObject(_tasksStatisticApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region WorkerRequestsReport

        public static void GenerateWorkerRequestsReport(DataView workerRequestsView)
        {
            if (_workerRequestsIsReporting) return;

            _workerRequestsIsReporting = true;

            _workerRequestsView = workerRequestsView;
            _workerRequestsWaitWindow = new WaitWindow();
            _workerRequestsWaitWindow.Show(Application.Current.MainWindow);
            _workerRequestsWaitWindow.Text = "Инициализация";

            WorkerRequestsInitialize();
        }

        private static void WorkerRequestsInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                _workerRequestsCurrentRow = 1;
                _workerRequestsRowIndex = 0;

                _workerRequestsApp = new Microsoft.Office.Interop.Excel.Application();
                _workerRequestsWorkbook = _workerRequestsApp.Workbooks.Add(Type.Missing);
                _workerRequestsWorksheet = _workerRequestsWorkbook.ActiveSheet;

                App.BaseClass.GetWorkerRequestsClass(ref _workerRequestsClass);

                _workerRequestsConverter = new WorkerRequestConverter();
                _workerRequestsWorkerNameConverter = new IdToNameConverter();

                SetWorkerRequestsDefaultStyle();
                FillWorkerRequestsData();
                WorkerRequestsFreezePanes();
                SetWorkerRequestsFormat();
                SetWorkerRequestsColumnWidth();
                WorkerRequestsOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _workerRequestsIsReporting = false;
                _workerRequestsWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void SetWorkerRequestsDefaultStyle()
        {
            _workerRequestsRange = _workerRequestsWorksheet.Rows;
            _workerRequestsRange.Select();
            _workerRequestsRange.Font.Name = "Arial";
            _workerRequestsRange.Font.Size = 10;
            _workerRequestsRange.HorizontalAlignment = Constants.xlCenter;
        }

        private static void FillWorkerRequestsData()
        {
            FillWorkerRequestsHeader();

            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _workerRequestsWaitWindow.Text = "Формирование таблиц";
            }));

            var index = 0;

            if(_workerRequestsView == null || _workerRequestsView.Count == 0)
            {
                _workerRequestsCurrentRow++;
                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 2] =
                    "Заявки отсутствуют";
                return;
            }

            _workerRequestsRowsCount = _workerRequestsView.Count;

            foreach (DataRowView workerRequest in _workerRequestsView)
            {
                index++;
                _workerRequestsCurrentRow++;
                _workerRequestsRowIndex++;

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 1] = index;

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 2] = 
                    _workerRequestsWorkerNameConverter.Convert(workerRequest["WorkerID"], "ShortName");

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 3] = workerRequest["CreationDate"];

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 4] = 
                    _workerRequestsConverter.GetRequestTypeName(Convert.ToInt32(workerRequest["RequestTypeID"]));

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 5] =
                    _workerRequestsConverter.GetSalarySaveTypeName(Convert.ToInt32(workerRequest["SalarySaveTypeID"]));

                var intervalType = (IntervalType)Convert.ToInt32(workerRequest["IntervalTypeID"]);
                var requestDate = Convert.ToDateTime(workerRequest["RequestDate"]);
                var requestFinishDate = Convert.ToDateTime(workerRequest["RequestFinishDate"]);
                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 6] = 
                    WorkerRequestDurationConverter.GetDateDuration(intervalType, requestDate, requestFinishDate).ToString() + "\n" +
                    WorkerRequestDurationConverter.GetTimeDuration(intervalType, requestDate, requestFinishDate).ToString();

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 7] = workerRequest["RequestNotes"];

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 8] =
                    _workerRequestsConverter.GetInitiativeTypeName(Convert.ToInt32(workerRequest["InitiativeTypeID"]));

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 9] =
                    _workerRequestsConverter.GetWorkingOffTypeName(Convert.ToInt32(workerRequest["WorkingOffTypeID"]));

                _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 10] =
                    _workerRequestsWorkerNameConverter.Convert(Convert.ToInt32(workerRequest["MainWorkerID"]), "ShortName");

                if(workerRequest["IsConfirmed"] != DBNull.Value)
                {
                    _workerRequestsRange = GetWorkerRequestsRange(_workerRequestsCurrentRow, 11, _workerRequestsCurrentRow, 12);
                    _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 12] = workerRequest["ConfirmationDate"];

                    if (Convert.ToBoolean(workerRequest["IsConfirmed"]))
                    {
                        _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 11] = "Согласовано";                        
                        _workerRequestsRange.Font.Color = XlRgbColor.rgbDarkGreen;
                    }
                    else
                    {
                        _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 11] = "Отказано";
                        _workerRequestsRange.Font.Color = XlRgbColor.rgbIndianRed;
                    }
                }
                else
                {
                    _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 11] = "Подлежит согласованию";
                    _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 12] = "Отсутствует";
                }


                var percent = (double)_workerRequestsRowIndex / _workerRequestsRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _workerRequestsWaitWindow.Progress = percent * 100;
                    _workerRequestsWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void FillWorkerRequestsHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _workerRequestsWaitWindow.Text = "Формирование шапки";
            }));

            _workerRequestsRange =
                GetWorkerRequestsRange(_workerRequestsCurrentRow, 1, _workerRequestsCurrentRow, 4);
            _workerRequestsRange.Merge();
            _workerRequestsRange.Font.Bold = true;
            _workerRequestsRange.Font.Size = 12;

            var headerText = string.Format("Статистика за период с {0:dd.MM.yyyy} по {1:dd.MM.yyyy}",
                _workerRequestsClass.DateFrom.HasValue ? _workerRequestsClass.DateFrom.Value : DateTime.MinValue,
                _workerRequestsClass.DateTo.HasValue ? _workerRequestsClass.DateTo.Value : DateTime.MinValue);

            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 1] = headerText;

            _workerRequestsCurrentRow += 2;
            _workerRequestsRange = GetWorkerRequestsRange(_workerRequestsCurrentRow, 1, _workerRequestsCurrentRow,
                WorkerRequestsColumnsCount);
            _workerRequestsRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _workerRequestsRange.Font.Color = XlRgbColor.rgbWhite;
            _workerRequestsRange.Font.Bold = true;

            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 1] = "№ п/п";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 2] = "Работник";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 3] = "Дата создания";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 4] = "Тип заявки";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 5] = "Заработная плата";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 6] = "На дату";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 7] = "Причина";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 8] = "Инициатива";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 9] = "Отработка";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 10] = "На согласование";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 11] = "Статус";
            _workerRequestsWorksheet.Cells[_workerRequestsCurrentRow, 12] = "Дата согласования";
        }

        private static void WorkerRequestsFreezePanes()
        {
            ((Range)_workerRequestsWorksheet.Cells[4, 7]).Select();
            _workerRequestsApp.ActiveWindow.FreezePanes = true;
        }

        private static void SetWorkerRequestsFormat()
        {
            var range = GetWorkerRequestsRange(3, 1, _workerRequestsCurrentRow, WorkerRequestsColumnsCount);
            FormatWorkerRequestsAsTable(range, "MyTable", "TableStyleMedium16");
            range.VerticalAlignment = Constants.xlTop;
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
            range.VerticalAlignment = Constants.xlCenter;
        }

        private static void FormatWorkerRequestsAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static void SetWorkerRequestsColumnWidth()
        {
            _workerRequestsRange = _workerRequestsWorksheet.Rows;
            _workerRequestsRange.Columns.AutoFit();

            _workerRequestsRange = _workerRequestsWorksheet.Columns[1];
            _workerRequestsRange.ColumnWidth = 5;
            _workerRequestsRange.WrapText = true;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[2];
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[3];
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[4];
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[5];
            _workerRequestsRange.ColumnWidth = 18;
            _workerRequestsRange.WrapText = true;
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[6];
            _workerRequestsRange.ColumnWidth = 22;
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[7];
            _workerRequestsRange.ColumnWidth = 30;
            _workerRequestsRange.WrapText = true;
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[8];
            _workerRequestsRange.ColumnWidth = 23;
            _workerRequestsRange.WrapText = true;
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[10];
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;

            _workerRequestsRange = _workerRequestsWorksheet.Columns[12];
            _workerRequestsRange.HorizontalAlignment = Constants.xlLeft;
        }

        private static Range GetWorkerRequestsRange(int y1, int x1, int y2, int x2)
        {
            return
                _workerRequestsWorksheet.Range[
                    _workerRequestsWorksheet.Cells[y1, x1],
                    _workerRequestsWorksheet.Cells[y2, x2]];
        }

        private static void WorkerRequestsOpenReport()
        {
            _workerRequestsApp.Visible = true;

            Marshal.ReleaseComObject(_workerRequestsWorkbook);
            Marshal.ReleaseComObject(_workerRequestsApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region WeekendResponsiblesReport

        public static void GenerateWeekendResponsiblesReport(DataView weekendResponsiblesTimeSheetDataView)
        {
            if (_weekendResponsiblesIsReporting) return;

            _weekendResponsiblesIsReporting = true;
            _weekendResponsiblesWaitWindow = new WaitWindow();
            _weekendResponsiblesWaitWindow.Show(Application.Current.MainWindow);
            _weekendResponsiblesWaitWindow.Text = "Инициализация";

            _weekendResponsiblesTimeSheetDataView = weekendResponsiblesTimeSheetDataView;
            WeekendResponsiblesInitialize();
        }

        private static void WeekendResponsiblesInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                App.BaseClass.GetProdRoomsClass(ref _weekendResponsiblesProdRoomsClass);
                _weekendResponsiblesWorkerNameConverter = new IdToNameConverter();

                _weekendResponsiblesApp = new Microsoft.Office.Interop.Excel.Application();
                _weekendResponsiblesWorkbook = _weekendResponsiblesApp.Workbooks.Add(Type.Missing);

                _weekendResponsiblesWorksheet = _weekendResponsiblesWorkbook.ActiveSheet;
                _weekendResponsiblesWorksheet.Name = "Учёт выхода на работу";
                _weekendResponsiblesCurrentRow = 1;
                _weekendResponsiblesRowIndex = 0;
                SetResponsibleArrivesDefaultStyle();
                FillResponsibleArrivesData();
                SetResponsibleArrivesFormat();
                ResponsibleArrivesFreezePanes();
                SetResponsibleArriveColumnWidth();


                _weekendResponsiblesWorksheet = _weekendResponsiblesWorkbook.Worksheets.Add();
                _weekendResponsiblesWorksheet.Name = "График";
                _weekendResponsiblesCurrentRow = 1;
                _weekendResponsiblesRowIndex = 0;

                FillWeekendResponsiblesData();
                SetWeekendResponsiblesFormat();
                SetWeekendResponsiblesColumnWidth();

                WeekendResponsiblesOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _weekendResponsiblesIsReporting = false;
                _weekendResponsiblesWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillWeekendResponsiblesData()
        {
            FillWeekendResponsiblesHeader();

            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _weekendResponsiblesWaitWindow.Text = "Формирование таблиц";
            }));

            var index = 0;

            _weekendResponsiblesRowsCount = _weekendResponsiblesTimeSheetDataView.Count;

            foreach (DataRowView weekendResponsibleRowView in _weekendResponsiblesTimeSheetDataView)
            {
                index++;
                _weekendResponsiblesCurrentRow++;
                _weekendResponsiblesRowIndex++;

                var workerId = Convert.ToInt64(weekendResponsibleRowView["WorkerID"]);
                var workerName = _weekendResponsiblesWorkerNameConverter.Convert(workerId, "ShortName");

                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = workerName;

                for(var i = 1; i < weekendResponsibleRowView.Row.ItemArray.Length; i++)
                {
                    if(weekendResponsibleRowView.Row.ItemArray[i] != DBNull.Value && Convert.ToBoolean(weekendResponsibleRowView.Row.ItemArray[i]))
                        _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, i + 1] = "X";
                }

                var percent = (double)_weekendResponsiblesRowIndex / _weekendResponsiblesRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _weekendResponsiblesWaitWindow.Progress = percent * 100;
                    _weekendResponsiblesWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void FillWeekendResponsiblesHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _weekendResponsiblesWaitWindow.Text = "Формирование шапки";
            }));

            _weekendResponsiblesRange = GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 19, _weekendResponsiblesCurrentRow, 24);
            _weekendResponsiblesRange.Merge(Type.Missing);
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 19] = "УТВЕРЖДАЮ";

            _weekendResponsiblesCurrentRow += 2;
            _weekendResponsiblesRange = GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 19, _weekendResponsiblesCurrentRow, 24);
            _weekendResponsiblesRange.Merge(Type.Missing);
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 19] = 
                " '       ' __________ " + _weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetYear;

            _weekendResponsiblesRange = 
                GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, _weekendResponsiblesTimeSheetDataView.Table.Columns.Count - 3, 
                _weekendResponsiblesCurrentRow, _weekendResponsiblesTimeSheetDataView.Table.Columns.Count);
            _weekendResponsiblesRange.Merge(Type.Missing);
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, _weekendResponsiblesTimeSheetDataView.Table.Columns.Count - 3] = "Ф.А. Авдей";

            _weekendResponsiblesCurrentRow++;
            _weekendResponsiblesRange =
                GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 1, _weekendResponsiblesCurrentRow, 25);
            _weekendResponsiblesRange.Merge();

            var headerText = string.Format("График ответственных на выходные и праздничные дни за {0} месяц {1}г.",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetMonth),
                _weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetYear);

            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = headerText;

            _weekendResponsiblesCurrentRow += 2;
            _weekendResponsiblesColumnsCount = 1;

            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = "ФИО";

            for (var i = 1; i < _weekendResponsiblesTimeSheetDataView.Table.Columns.Count; i++)
            {
                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, i + 1] = i.ToString();

                var date = new DateTime(_weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetYear, 
                    _weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetMonth, i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    _weekendResponsiblesRange = GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, i + 1,
                        _weekendResponsiblesCurrentRow + _weekendResponsiblesTimeSheetDataView.Count, i + 1);
                    _weekendResponsiblesRange.Interior.Color = XlRgbColor.rgbLightGray;
                }

                _weekendResponsiblesColumnsCount++;
            }

            _weekendResponsiblesRange = GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 1, 
                _weekendResponsiblesCurrentRow, _weekendResponsiblesColumnsCount);
            _weekendResponsiblesRange.Font.Bold = true;
            _weekendResponsiblesRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _weekendResponsiblesRange.Borders.Weight = XlBorderWeight.xlMedium;
        }

        private static void SetWeekendResponsiblesFormat()
        {
            var range = GetWeekendResponsiblesRange(7, 1, _weekendResponsiblesCurrentRow, _weekendResponsiblesColumnsCount);
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void SetWeekendResponsiblesColumnWidth()
        {
            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Columns[1];
            _weekendResponsiblesRange.ColumnWidth = 20;
            _weekendResponsiblesRange.HorizontalAlignment = Constants.xlLeft;

            _weekendResponsiblesRange = GetWeekendResponsiblesRange(1, 2, 1, _weekendResponsiblesColumnsCount);
            _weekendResponsiblesRange.ColumnWidth = 2.7;
        }


        private static void SetResponsibleArrivesDefaultStyle()
        {
            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Rows;
            _weekendResponsiblesRange.Font.Name = "Arial";
            _weekendResponsiblesRange.Font.Size = 10;
            _weekendResponsiblesRange.HorizontalAlignment = Constants.xlLeft;
            _weekendResponsiblesRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void FillResponsibleArrivesData()
        {
            FillResponsibleArrivesHeader();

            _weekendResponsiblesRowsCount = _weekendResponsiblesProdRoomsClass.ResponsibleArrivesTable.Rows.Count;
            foreach (var responsibleArrive in _weekendResponsiblesProdRoomsClass.ResponsibleArrivesTable.AsEnumerable())
            {
                _weekendResponsiblesCurrentRow++;
                _weekendResponsiblesRowIndex++;

                var workerId = Convert.ToInt64(responsibleArrive["WorkerID"]);
                var workerName = _weekendResponsiblesWorkerNameConverter.Convert(workerId, "ShortName");
                var date = Convert.ToDateTime(responsibleArrive["Date"]);
                var addInfo = responsibleArrive["AdditionalInfo"] != DBNull.Value
                    ? responsibleArrive["AdditionalInfo"].ToString()
                    : string.Empty;

                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = workerName;
                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 2] = date.ToString("dd.MM.yyyy");
                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 3] = date.ToString("H:mm");
                _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 4] = addInfo;

                var percent = (double)_weekendResponsiblesRowIndex / _weekendResponsiblesRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _weekendResponsiblesWaitWindow.Progress = percent * 100;
                    _weekendResponsiblesWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void FillResponsibleArrivesHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _weekendResponsiblesWaitWindow.Text = "Формирование шапки";
            }));

            _weekendResponsiblesRange =
                GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 1, _weekendResponsiblesCurrentRow, 7);
            _weekendResponsiblesRange.Merge();
            _weekendResponsiblesRange.Font.Bold = true;
            _weekendResponsiblesRange.Font.Size = 12;

            var headerText = string.Format("Статистика за {0} месяц {1}г.",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetMonth),
                _weekendResponsiblesProdRoomsClass.SelectedWeekendTimeSheetYear);

            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = headerText;

            _weekendResponsiblesCurrentRow += 2;
            _weekendResponsiblesRange = GetWeekendResponsiblesRange(_weekendResponsiblesCurrentRow, 1, _weekendResponsiblesCurrentRow,
                ResponsibleArrivesColumnsCount);
            _weekendResponsiblesRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _weekendResponsiblesRange.Font.Color = XlRgbColor.rgbWhite;
            _weekendResponsiblesRange.Font.Bold = true;

            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 1] = "ФИО";
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 2] = "Дата";
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 3] = "Время";
            _weekendResponsiblesWorksheet.Cells[_weekendResponsiblesCurrentRow, 4] = "Примечание";
        }

        private static void SetResponsibleArrivesFormat()
        {
            var range = GetWeekendResponsiblesRange(3, 1, _weekendResponsiblesCurrentRow, ResponsibleArrivesColumnsCount);
            FormatResponsibleArrivesAsTable(range, "MyTable", "TableStyleMedium16");
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void FormatResponsibleArrivesAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static void ResponsibleArrivesFreezePanes()
        {
            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Rows[4];
            _weekendResponsiblesRange.Activate();
            _weekendResponsiblesRange.Select();
            _weekendResponsiblesApp.ActiveWindow.FreezePanes = true;
        }

        private static void SetResponsibleArriveColumnWidth()
        {
            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Columns[1];
            _weekendResponsiblesRange.ColumnWidth = 20;

            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Columns[2];
            _weekendResponsiblesRange.ColumnWidth = 10;

            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Columns[3];
            _weekendResponsiblesRange.ColumnWidth = 8;

            _weekendResponsiblesRange = _weekendResponsiblesWorksheet.Columns[4];
            _weekendResponsiblesRange.ColumnWidth = 50;
            _weekendResponsiblesRange.WrapText = true;
        }


        private static Range GetWeekendResponsiblesRange(int y1, int x1, int y2, int x2)
        {
            return
                _weekendResponsiblesWorksheet.Range[
                    _weekendResponsiblesWorksheet.Cells[y1, x1],
                    _weekendResponsiblesWorksheet.Cells[y2, x2]];
        }

        private static void WeekendResponsiblesOpenReport()
        {
            _weekendResponsiblesApp.Visible = true;

            Marshal.ReleaseComObject(_weekendResponsiblesWorkbook);
            Marshal.ReleaseComObject(_weekendResponsiblesApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region ProdRoomsResponsiblesReport

        public static void GenerateProdRoomsResponsiblesReport(int year, int month)
        {
            if (_prodRoomsResponsiblesIsReporting) return;

            _prodRoomsResponsiblesIsReporting = true;
            _prodRoomsResponsiblesWaitWindow = new WaitWindow();
            _prodRoomsResponsiblesWaitWindow.Show(Application.Current.MainWindow);
            _prodRoomsResponsiblesWaitWindow.Text = "Инициализация";

            ProdRoomsResponsiblesInitialize(year, month);
        }

        private static void ProdRoomsResponsiblesInitialize(int year, int month)
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                App.BaseClass.GetProdRoomsClass(ref _prodRoomsResponsiblesProdRoomsClass);
                _prodRoomsResponsiblesProdRoomsClass.FillWorkerReports(year, month);
                _prodRoomsResponsiblesWorkerNameConverter = new IdToNameConverter();
                _prodRoomsResponsiblesActionsConverter = new ActionsConverter();

                _prodRoomsResponsiblesApp = new Microsoft.Office.Interop.Excel.Application();
                _prodRoomsResponsiblesWorkbook = _prodRoomsResponsiblesApp.Workbooks.Add(Type.Missing);

                _prodRoomsResponsiblesWorksheet = _prodRoomsResponsiblesWorkbook.ActiveSheet;
                _prodRoomsResponsiblesWorksheet.Name = "Инструкции";
                _prodRoomsResponsiblesCurrentRow = 1;
                _prodRoomsResponsiblesRowIndex = 0;

                foreach (var actionStatusId in _prodRoomsResponsiblesProdRoomsClass.Actions.Status.AsEnumerable().Select(r => r.Field<Int64>("ActionStatusID")))
                {
                    FillProdRoomsActionsData(actionStatusId);
                }
                SetProdRoomsActionsColumnsWidth();

                _prodRoomsResponsiblesWorksheet = _prodRoomsResponsiblesWorkbook.Worksheets.Add();
                _prodRoomsResponsiblesWorksheet.Name = "Рапорты";
                _prodRoomsResponsiblesCurrentRow = 1;
                _prodRoomsResponsiblesRowIndex = 0;

                SetProdRoomsResponsiblesDefaultStyle();
                FillProdRoomsResponsiblesData();
                SetProdRoomsResponsiblesFormat();
                ProdRoomsResponsiblesFreezePanes();
                SetProdRoomsResponsiblesColumnWidth();
                ProdRoomsResponsiblesOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _prodRoomsResponsiblesIsReporting = false;
                _prodRoomsResponsiblesWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void FillProdRoomsActionsData(long actionStatusId)
        {
            FillProdRoomsActionsHeaders(actionStatusId);

            _prodRoomsResponsiblesCurrentRow++;

            foreach (var action in _prodRoomsResponsiblesProdRoomsClass.Actions.Table.AsEnumerable().
                Where(a => a.Field<bool>("Visible") && a.Field<Int64>("ActionStatus") == actionStatusId).OrderBy(a => a.Field<Int64>("ActionNumber")))
            {
                _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = action["ActionNumber"];
                _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 2] = action["ActionText"];

                _prodRoomsResponsiblesCurrentRow++;
            }
        }

        private static void FillProdRoomsActionsHeaders(long actionStatusId)
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _prodRoomsResponsiblesWaitWindow.Text = "Экспорт инструкций";
            }));

            _prodRoomsResponsiblesCurrentRow += 2;
            _prodRoomsResponsiblesRange =
                GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow, 2);
            _prodRoomsResponsiblesRange.Merge();
            _prodRoomsResponsiblesRange.Font.Bold = true;
            _prodRoomsResponsiblesRange.Font.Size = 12;
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = 
                new ActionGroupConverter().Convert(actionStatusId, typeof(string), null, CultureInfo.InvariantCulture);

            _prodRoomsResponsiblesCurrentRow++;
            _prodRoomsResponsiblesRange = GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow, 2);
            _prodRoomsResponsiblesRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _prodRoomsResponsiblesRange.Font.Color = XlRgbColor.rgbWhite;
            _prodRoomsResponsiblesRange.Font.Bold = true;

            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = "№";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 2] = "Содержание";
        }

        private static void SetProdRoomsActionsColumnsWidth()
        {
            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[1];
            _prodRoomsResponsiblesRange.ColumnWidth = 5;
            _prodRoomsResponsiblesRange.VerticalAlignment = Constants.xlTop;
            _prodRoomsResponsiblesRange.HorizontalAlignment = Constants.xlCenter;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[2];
            _prodRoomsResponsiblesRange.ColumnWidth = 150;
            _prodRoomsResponsiblesRange.VerticalAlignment = Constants.xlTop;
            _prodRoomsResponsiblesRange.WrapText = true;
        }


        private static void SetProdRoomsResponsiblesDefaultStyle()
        {
            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Rows;
            _prodRoomsResponsiblesRange.Font.Name = "Arial";
            _prodRoomsResponsiblesRange.Font.Size = 10;
            _prodRoomsResponsiblesRange.HorizontalAlignment = Constants.xlLeft;
            _prodRoomsResponsiblesRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void FillProdRoomsResponsiblesData()
        {
            FillProdRoomsResponsiblesHeader();

            var closingWorkers = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                Where(r => r.Field<Int64>("ActionStatusID") == 2).Select(r => r.Field<Int64>("WorkerID")).Distinct();

            var openingWorkers = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                Where(r => r.Field<Int64>("ActionStatusID") == 1).Select(r => r.Field<Int64>("WorkerID")).Distinct();

            _prodRoomsResponsiblesRowsCount = closingWorkers.Count() + openingWorkers.Count();

            foreach (var workerId in closingWorkers)
            {
                _prodRoomsResponsiblesRowIndex++;
                var workerName = _prodRoomsResponsiblesWorkerNameConverter.Convert(workerId, "ShortName");
                var closingDates = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                    Where(r => r.Field<Int64>("ActionStatusID") == 2 && r.Field<Int64>("WorkerID") == workerId).Select(r => r.Field<DateTime>("ReportDate")).Distinct();

                foreach (var closingDate in closingDates)
                {
                    var closingRaports = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                        Where(r => r.Field<Int64>("ActionStatusID") == 2 && r.Field<Int64>("WorkerID") == workerId && r.Field<DateTime>("ReportDate") == closingDate);
                    if (!closingRaports.Any()) continue;

                    _prodRoomsResponsiblesCurrentRow++;

                    var mainRaport = closingRaports.First();
                    var addInfo = mainRaport["AdditionalInfo"] != DBNull.Value
                        ? mainRaport["AdditionalInfo"].ToString()
                        : string.Empty;

                    var hasWorkerCheduleOnDate = _prodRoomsResponsiblesProdRoomsClass.HasWorkerDateClosingSchedule(workerId, closingDate);
                    if (!hasWorkerCheduleOnDate)
                    {
                        _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = string.Format("{0} (не указан в графике)", workerName);
                        _prodRoomsResponsiblesRange = GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow, 1);
                        _prodRoomsResponsiblesRange.Font.Color = XlRgbColor.rgbOrangeRed;
                    }
                    else
                    {
                        _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = workerName;
                    }
                    
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 2] = "Закрытие";
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 3] = closingDate.ToString("dd.MM.yyyy");
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 4] = closingDate.ToString("HH:mm");

                    var doneActions = closingRaports.Where(r => r.Field<bool>("IsDoneAction")).
                        Select(r => r.Field<Int64>("ActionID")).
                        Aggregate(string.Empty, (current, i) => current += ", " + _prodRoomsResponsiblesActionsConverter.GetActionNumber(i));

                    var doesNotDoneActions = closingRaports.Where(r => !r.Field<bool>("IsDoneAction")).
                        Select(r => r.Field<Int64>("ActionID")).
                        Aggregate(string.Empty, (current, i) => current += ", " + _prodRoomsResponsiblesActionsConverter.GetActionNumber(i));

                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 5] = doneActions.Length > 2 
                        ? doneActions.Substring(2)
                        : string.Empty;
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 6] = doesNotDoneActions.Length > 2
                        ? doesNotDoneActions.Substring(2)
                        : string.Empty;

                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 7] = addInfo;
                }

                var percent = (double)_prodRoomsResponsiblesRowIndex / _prodRoomsResponsiblesRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _prodRoomsResponsiblesWaitWindow.Progress = percent * 100;
                    _prodRoomsResponsiblesWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }


            foreach (var workerId in openingWorkers)
            {
                _prodRoomsResponsiblesRowIndex++;
                var workerName = _prodRoomsResponsiblesWorkerNameConverter.Convert(workerId, "ShortName");
                var openingDates = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                    Where(r => r.Field<Int64>("ActionStatusID") == 1 && r.Field<Int64>("WorkerID") == workerId).Select(r => r.Field<DateTime>("ReportDate")).Distinct();

                foreach (var openingDate in openingDates)
                {
                    var openingRaports = _prodRoomsResponsiblesProdRoomsClass.ReportTable.AsEnumerable().
                        Where(r => r.Field<Int64>("ActionStatusID") == 1 && r.Field<Int64>("WorkerID") == workerId && r.Field<DateTime>("ReportDate") == openingDate);
                    if (!openingRaports.Any()) continue;

                    _prodRoomsResponsiblesCurrentRow++;

                    var mainRaport = openingRaports.First();
                    var addInfo = mainRaport["AdditionalInfo"] != DBNull.Value
                        ? mainRaport["AdditionalInfo"].ToString()
                        : string.Empty;

                    var hasWorkerCheduleOnDate = _prodRoomsResponsiblesProdRoomsClass.HasWorkerDateOpeningSchedule(workerId, openingDate);
                    if (!hasWorkerCheduleOnDate)
                    {
                        _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = string.Format("{0} (не указан в графике)", workerName);
                        _prodRoomsResponsiblesRange = GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow, 1);
                        _prodRoomsResponsiblesRange.Font.Color = XlRgbColor.rgbOrangeRed;
                    }
                    else
                    {
                        _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = workerName;
                    }

                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 2] = "Открытие";
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 3] = openingDate.ToString("dd.MM.yyyy");
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 4] = openingDate.ToString("HH:mm");

                    var doneActions = openingRaports.Where(r => r.Field<bool>("IsDoneAction")).
                        Select(r => r.Field<Int64>("ActionID")).
                        Aggregate(string.Empty, (current, i) => current += ", " + _prodRoomsResponsiblesActionsConverter.GetActionNumber(i));

                    var doesNotDoneActions = openingRaports.Where(r => !r.Field<bool>("IsDoneAction")).
                        Select(r => r.Field<Int64>("ActionID")).
                        Aggregate(string.Empty, (current, i) => current += ", " + _prodRoomsResponsiblesActionsConverter.GetActionNumber(i));

                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 5] = doneActions.Length > 2
                        ? doneActions.Substring(2)
                        : string.Empty;
                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 6] = doesNotDoneActions.Length > 2
                        ? doesNotDoneActions.Substring(2)
                        : string.Empty;

                    _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 7] = addInfo;
                }

                var percent = (double)_prodRoomsResponsiblesRowIndex / _prodRoomsResponsiblesRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _prodRoomsResponsiblesWaitWindow.Progress = percent * 100;
                    _prodRoomsResponsiblesWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void FillProdRoomsResponsiblesHeader()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _prodRoomsResponsiblesWaitWindow.Text = "Формирование шапки";
            }));

            _prodRoomsResponsiblesRange =
                GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow, 7);
            _prodRoomsResponsiblesRange.Merge();
            _prodRoomsResponsiblesRange.Font.Bold = true;
            _prodRoomsResponsiblesRange.Font.Size = 12;

            var headerText = string.Format("Статистика за {0} месяц {1}г.",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_prodRoomsResponsiblesProdRoomsClass.SelectedWorkerReportMonth),
                _prodRoomsResponsiblesProdRoomsClass.SelectedWorkerReportYear);

            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = headerText;

            _prodRoomsResponsiblesCurrentRow += 2;
            _prodRoomsResponsiblesRange = GetProdRoomsResponsiblesRange(_prodRoomsResponsiblesCurrentRow, 1, _prodRoomsResponsiblesCurrentRow,
                ProdRoomsResponsiblesColumnsCount);
            _prodRoomsResponsiblesRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _prodRoomsResponsiblesRange.Font.Color = XlRgbColor.rgbWhite;
            _prodRoomsResponsiblesRange.Font.Bold = true;

            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 1] = "ФИО";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 2] = "Действие";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 3] = "Дата";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 4] = "Время";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 5] = "Выполненые пункты";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 6] = "Невыполненые пункты";
            _prodRoomsResponsiblesWorksheet.Cells[_prodRoomsResponsiblesCurrentRow, 7] = "Примечание";
        }

        private static void SetProdRoomsResponsiblesFormat()
        {
            var range = GetProdRoomsResponsiblesRange(3, 1, _prodRoomsResponsiblesCurrentRow, ProdRoomsResponsiblesColumnsCount);
            FormatProdRoomsResponsiblesAsTable(range, "MyTable", "TableStyleMedium16");
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void FormatProdRoomsResponsiblesAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static void ProdRoomsResponsiblesFreezePanes()
        {
            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Rows[4];
            _prodRoomsResponsiblesRange.Activate();
            _prodRoomsResponsiblesRange.Select();
            _prodRoomsResponsiblesApp.ActiveWindow.FreezePanes = true;
        }

        private static void SetProdRoomsResponsiblesColumnWidth()
        {
            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[1];
            _prodRoomsResponsiblesRange.ColumnWidth = 30;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[2];
            _prodRoomsResponsiblesRange.ColumnWidth = 15;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[3];
            _prodRoomsResponsiblesRange.ColumnWidth = 10;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[4];
            _prodRoomsResponsiblesRange.ColumnWidth = 8;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[5];
            _prodRoomsResponsiblesRange.ColumnWidth = 25;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[6];
            _prodRoomsResponsiblesRange.ColumnWidth = 25;

            _prodRoomsResponsiblesRange = _prodRoomsResponsiblesWorksheet.Columns[7];
            _prodRoomsResponsiblesRange.ColumnWidth = 50;
            _prodRoomsResponsiblesRange.WrapText = true;
        }


        private static Range GetProdRoomsResponsiblesRange(int y1, int x1, int y2, int x2)
        {
            return
                _prodRoomsResponsiblesWorksheet.Range[
                    _prodRoomsResponsiblesWorksheet.Cells[y1, x1],
                    _prodRoomsResponsiblesWorksheet.Cells[y2, x2]];
        }

        private static void ProdRoomsResponsiblesOpenReport()
        {
            _prodRoomsResponsiblesApp.Visible = true;

            Marshal.ReleaseComObject(_prodRoomsResponsiblesWorkbook);
            Marshal.ReleaseComObject(_prodRoomsResponsiblesApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion


        #region PlannedWorksReport

        public static void GeneratePlannedWorksReport()
        {
            if (_plannedWorksIsReporting) return;

            _plannedWorksIsReporting = true;
            _plannedWorksWaitWindow = new WaitWindow();
            _plannedWorksWaitWindow.Show(Application.Current.MainWindow);
            _plannedWorksWaitWindow.Text = "Инициализация";

            PlannedWorksInitialize();
        }

        private static void PlannedWorksInitialize()
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (sender, args) =>
            {
                App.BaseClass.GetPlannedWorksClass(ref _plannedWorksClass);
                App.BaseClass.GetTaskClass(ref _plannedWorksTaskClass);
                _plannedWorksWorkerNameConverter = new IdToNameConverter();
                _plannedWorksConverter = new PlannedWorksConverter();
                _plannedWorksTaskConverter = new TaskConverter();
                _plannedWorksTimeSpanConverter = new TimeSpanConverter();

                _plannedWorksApp = new Microsoft.Office.Interop.Excel.Application();
                _plannedWorksWorkbook = _plannedWorksApp.Workbooks.Add(Type.Missing);

                _plannedWorksWorksheet = _plannedWorksWorkbook.ActiveSheet;
                _plannedWorksWorksheet.Name = "Список работ";
                _plannedWorksCurrentRow = 1;
                _plannedWorksRowIndex = 0;

                SetPlannedWorksDefaultStyle();
                FillPlannedWorksData();
                SetPlannedWorksColumnsWidth();
                SetPlannedWorksFormat();

                _plannedWorksWorksheet = _plannedWorksWorkbook.Worksheets.Add();
                _plannedWorksWorksheet.Name = "Статистика выполнения работ";
                _plannedWorksCurrentRow = 1;
                _plannedWorksRowIndex = 0;

                SetStartedPlannedWorksDefaultStyle();
                FillStartedPlannedWorksData();
                SetStartedPlannedWorksFormat();
                PlannedWorksFreezePanes();
                SetStartedPlannedWorksColumnWidth();
                PlannedWorksOpenReport();
            };

            bw.RunWorkerCompleted += (sender, args) =>
            {
                _plannedWorksIsReporting = false;
                _plannedWorksWaitWindow.Close(true);
                bw.Dispose();
            };

            bw.RunWorkerAsync();
        }

        private static void SetPlannedWorksDefaultStyle()
        {
            _plannedWorksRange = _plannedWorksWorksheet.Rows;
            _plannedWorksRange.Font.Name = "Arial";
            _plannedWorksRange.Font.Size = 10;
            _plannedWorksRange.HorizontalAlignment = Constants.xlLeft;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void FillPlannedWorksData()
        {
            FillPlannedWorksHeaders();

            var plannedWorksView = _plannedWorksClass.PlannedWorksTable.AsDataView();
            plannedWorksView.RowFilter = "IsEnable = 'True'";
            plannedWorksView.Sort = "ConfirmationStatusID, PlannedWorksName";

            var i = 0;
            foreach (DataRowView plannedWorks in plannedWorksView)
            {
                i++;
                _plannedWorksCurrentRow++;

                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = i;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 2] = plannedWorks["PlannedWorksName"];
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 3] = plannedWorks["Description"];
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 4] = 
                    _plannedWorksConverter.GetPlannedWorksTypeName(Convert.ToInt32(plannedWorks["PlannedWorksTypeID"]));
                var isActive = Convert.ToBoolean(plannedWorks["IsActive"]);
                if(isActive)
                {
                    _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5] = "Активна";
                    ((Range)_plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5]).Font.Color = XlRgbColor.rgbGreen;
                }
                else
                {
                    _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5] = "Неактивна";
                    ((Range)_plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5]).Font.Color = XlRgbColor.rgbGray;
                }
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6] = 
                    _plannedWorksConverter.GetConfirmationStatus(Convert.ToInt32(plannedWorks["ConfirmationStatusID"]));
                switch ((ConfirmationStatus)Convert.ToInt32(plannedWorks["ConfirmationStatusID"]))
                {
                    case ConfirmationStatus.Confirmed:
                        ((Range)_plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6]).Font.Color = XlRgbColor.rgbGreen;
                        break;
                    case ConfirmationStatus.Rejected:
                        ((Range)_plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6]).Font.Color = XlRgbColor.rgbOrangeRed;
                        break;
                    case ConfirmationStatus.WaitingConfirmation:
                        ((Range)_plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6]).Font.Color = XlRgbColor.rgbGray;
                        break;
                }

                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 7] = 
                    _plannedWorksWorkerNameConverter.Convert(Convert.ToInt64(plannedWorks["CreatedWorkerID"]), "ShortName");
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 8] = Convert.ToDateTime(plannedWorks["CreationDate"]);
            }
        }

        private static void FillPlannedWorksHeaders()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _plannedWorksWaitWindow.Text = "Экспорт списка плановых работ";
            }));
            _plannedWorksRange =
                GetPlannedWorksRange(_plannedWorksCurrentRow, 1, _plannedWorksCurrentRow, PlannedWorksColumnsCount);
            _plannedWorksRange.Merge();
            _plannedWorksRange.Font.Bold = true;
            _plannedWorksRange.Font.Size = 12;
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = "Список плановых работ";

            _plannedWorksCurrentRow += 2;
            _plannedWorksRange = GetPlannedWorksRange(_plannedWorksCurrentRow, 1, _plannedWorksCurrentRow, PlannedWorksColumnsCount);
            _plannedWorksRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _plannedWorksRange.Font.Color = XlRgbColor.rgbWhite;
            _plannedWorksRange.Font.Bold = true;
            _plannedWorksRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _plannedWorksRange.Borders.Weight = XlBorderWeight.xlMedium;

            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = "№ п/п";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 2] = "Название";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 3] = "Описание";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 4] = "Характер работ";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5] = "Статус";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6] = "Подтверждение";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 7] = "Составитель";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 8] = "Дата создания";
        }

        private static void SetPlannedWorksColumnsWidth()
        {
            _plannedWorksRange = _plannedWorksWorksheet.Columns[1];
            _plannedWorksRange.ColumnWidth = 4;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[2];
            _plannedWorksRange.ColumnWidth = 40;
            _plannedWorksRange.VerticalAlignment = Constants.xlTop;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[3];
            _plannedWorksRange.ColumnWidth = 40;
            _plannedWorksRange.VerticalAlignment = Constants.xlTop;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[4];
            _plannedWorksRange.ColumnWidth = 25;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[5];
            _plannedWorksRange.ColumnWidth = 10;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[6];
            _plannedWorksRange.ColumnWidth = 15;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[7];
            _plannedWorksRange.ColumnWidth = 20;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[8];
            _plannedWorksRange.ColumnWidth = 15;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;

            _plannedWorksRange = _plannedWorksWorksheet.Rows[3];
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void SetPlannedWorksFormat()
        {
            _plannedWorksRange = GetPlannedWorksRange(4, 1, _plannedWorksCurrentRow, PlannedWorksColumnsCount);
            _plannedWorksRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            _plannedWorksRange.Borders.Weight = XlBorderWeight.xlThin;
        }


        private static void SetStartedPlannedWorksDefaultStyle()
        {
            _plannedWorksRange = _plannedWorksWorksheet.Rows;
            _plannedWorksRange.Font.Name = "Arial";
            _plannedWorksRange.Font.Size = 10;
            _plannedWorksRange.HorizontalAlignment = Constants.xlLeft;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;
        }

        private static void FillStartedPlannedWorksData()
        {
            FillStartedPlannedWorksHeaders();

            var startedPlannedWorksView = _plannedWorksClass.StartedPlannedWorksTable.AsDataView();

            _plannedWorksRowsCount = startedPlannedWorksView.Count;

            foreach (DataRowView startedPlannedWorks in startedPlannedWorksView)
            {
                var plannedWorksId = Convert.ToInt64(startedPlannedWorks["PlannedWorksID"]);
                var taskId = Convert.ToInt64(startedPlannedWorks["TaskID"]);
                var emptyWorkReasonId = Convert.ToInt32(startedPlannedWorks["EmptyWorkReasonID"]);

                var plannedWorksRows = _plannedWorksClass.PlannedWorksTable.Select(string.Format("PlannedWorksID = {0}", plannedWorksId));
                if (!plannedWorksRows.Any()) continue;

                var tasksRows = _plannedWorksTaskClass.Tasks.Table.Select(string.Format("TaskID = {0}", taskId));
                if (!tasksRows.Any()) continue;

                var plannedWorksRow = plannedWorksRows.First();
                var taskRow = tasksRows.First();

                _plannedWorksRowIndex++;
                _plannedWorksCurrentRow++;

                var plannedWorksName = plannedWorksRow["PlannedWorksName"].ToString();
                var plannedWorksDescription = plannedWorksRow["Description"].ToString();
                var plannedWorksTypeName = _plannedWorksConverter.GetPlannedWorksTypeName(Convert.ToInt32(plannedWorksRow["PlannedWorksTypeID"]));
                var mainWorkerName = _plannedWorksWorkerNameConverter.Convert(taskRow["MainWorkerID"], "ShortName");
                var creationDate = Convert.ToDateTime(taskRow["CreationDate"]);
                var emptyWorkReasonName = _plannedWorksConverter.GetEmptyWorkReasonName(emptyWorkReasonId);
                var taskStatusName = _plannedWorksTaskConverter.Convert(taskRow["TaskStatusID"], typeof(string), "TaskStatusName", CultureInfo.InvariantCulture);
                string performersString = string.Empty;

                var performers = _plannedWorksTaskClass.Performers.Table.AsEnumerable().Where(p => p.Field<Int64>("TaskID") == taskId).ToList();
                var workerIds = from performer in performers
                                select Convert.ToInt32(performer["WorkerID"]);

                if (workerIds.Any())
                {
                    var completionWorkers = workerIds.Aggregate(string.Empty,
                    (current, workerId) =>
                        current +
                        string.Format("{0}\n",
                            _plannedWorksWorkerNameConverter.Convert(workerId, "ShortName")));
                    completionWorkers = completionWorkers.Remove(completionWorkers.Length - 2);
                    performersString = completionWorkers;
                }

                TimeSpan totalTime = TimeSpan.Zero;
                foreach(var performer in performers)
                {
                    var result = _plannedWorksTimeSpanConverter.Convert(performer["SpendTime"], typeof(TimeSpan), "FromMinutes", CultureInfo.InvariantCulture);
                    if (result != null)
                    {
                        TimeSpan spendTime;
                        TimeSpan.TryParse(result.ToString(), out spendTime);
                        totalTime = totalTime.Add(spendTime);
                    }
                }
                
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = plannedWorksName;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 2] = plannedWorksDescription;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 3] = plannedWorksTypeName;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 4] = mainWorkerName;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5] = creationDate;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6] = emptyWorkReasonName;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 7] = taskStatusName;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 8] = performersString;
                _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 9] = Math.Round(totalTime.TotalHours, 2);

                var percent = (double)_plannedWorksRowIndex / _plannedWorksRowsCount;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    _plannedWorksWaitWindow.Progress = percent * 100;
                    _plannedWorksWaitWindow.Text =
                        string.Format(
                            "Вывод данных в Excel {0}%", (int)(percent * 100));
                }));
            }
        }

        private static void FillStartedPlannedWorksHeaders()
        {
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                _plannedWorksWaitWindow.Text = "Формирование шапки";
            }));

            _plannedWorksRange =
                GetPlannedWorksRange(_plannedWorksCurrentRow, 1, _plannedWorksCurrentRow, StartedPlannedWorksColumnsCount);
            _plannedWorksRange.Merge();
            _plannedWorksRange.Font.Bold = true;
            _plannedWorksRange.Font.Size = 12;

            var headerText = string.Format("Статистика за период с {0:dd.MM.yyyy} по {1:dd.MM.yyyy}г.", 
                _plannedWorksTaskClass.GetDateFrom(), _plannedWorksTaskClass.GetDateTo());

            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = headerText;

            _plannedWorksCurrentRow += 2;
            _plannedWorksRange = GetPlannedWorksRange(_plannedWorksCurrentRow, 1, _plannedWorksCurrentRow,
                StartedPlannedWorksColumnsCount);
            _plannedWorksRange.Interior.Color = XlRgbColor.rgbSteelBlue;
            _plannedWorksRange.Font.Color = XlRgbColor.rgbWhite;
            _plannedWorksRange.Font.Bold = true;

            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 1] = "Название работ";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 2] = "Описание";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 3] = "Характер работ";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 4] = "Принял на выполнение";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 5] = "Дата принятия";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 6] = "Причина выполнения";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 7] = "Статус";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 8] = "Ответственные";
            _plannedWorksWorksheet.Cells[_plannedWorksCurrentRow, 9] = "Затраченное время";
        }

        private static void SetStartedPlannedWorksFormat()
        {
            var range = GetPlannedWorksRange(3, 1, _plannedWorksCurrentRow, StartedPlannedWorksColumnsCount);
            FormatPlannedWorksAsTable(range, "MyTable", "TableStyleMedium16");
            range.Borders.LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Weight = XlBorderWeight.xlThin;
        }

        private static void FormatPlannedWorksAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
            sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private static void PlannedWorksFreezePanes()
        {
            _plannedWorksRange = _plannedWorksWorksheet.Rows[4];
            _plannedWorksRange.Activate();
            _plannedWorksRange.Select();
            _plannedWorksApp.ActiveWindow.FreezePanes = true;
        }

        private static void SetStartedPlannedWorksColumnWidth()
        {
            _plannedWorksRange = _plannedWorksWorksheet.Columns[1];
            _plannedWorksRange.ColumnWidth = 40;
            _plannedWorksRange.VerticalAlignment = Constants.xlTop;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[2];
            _plannedWorksRange.ColumnWidth = 40;
            _plannedWorksRange.VerticalAlignment = Constants.xlTop;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[3];
            _plannedWorksRange.ColumnWidth = 17;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[4];
            _plannedWorksRange.ColumnWidth = 20;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[5];
            _plannedWorksRange.ColumnWidth = 15;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[6];
            _plannedWorksRange.ColumnWidth = 20;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[7];
            _plannedWorksRange.ColumnWidth = 15;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[8];
            _plannedWorksRange.ColumnWidth = 20;

            _plannedWorksRange = _plannedWorksWorksheet.Columns[9];
            _plannedWorksRange.ColumnWidth = 13;
            _plannedWorksRange.WrapText = true;

            _plannedWorksRange = _plannedWorksWorksheet.Rows[3];
            _plannedWorksRange.RowHeight = 30;
            _plannedWorksRange.VerticalAlignment = Constants.xlCenter;
        }


        private static Range GetPlannedWorksRange(int y1, int x1, int y2, int x2)
        {
            return
                _plannedWorksWorksheet.Range[
                    _plannedWorksWorksheet.Cells[y1, x1],
                    _plannedWorksWorksheet.Cells[y2, x2]];
        }

        private static void PlannedWorksOpenReport()
        {
            _plannedWorksApp.Visible = true;

            Marshal.ReleaseComObject(_plannedWorksWorkbook);
            Marshal.ReleaseComObject(_plannedWorksApp);
            GC.Collect(0);
            GC.GetTotalMemory(true);
        }

        #endregion
    }
}
