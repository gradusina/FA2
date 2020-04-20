using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using FA2.Converters;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class TimeControlClass
    {
        public struct WorkersStatValues
        {
            public decimal CommonTime;

            public decimal FVCommonTime;
            public decimal SVCommonTime;
            public decimal TVCommonTime;
            public decimal AllVCommonTime;
            public decimal OneOfVCommonTime;

            public decimal OperationTotalTime;
            public decimal WorkerCommonTime;
            public decimal MentorCommonTime;
            public decimal StudentCommonTime;
            public decimal TasksTotalTime;
        }

        public event ProgressChangedEventHandler ProgressChanged;

        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        protected virtual void CalculateStat_OnProgressChanged(int progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progress, null));
            }
        }

        protected virtual void CalculateStat_OnRunWorkerCompleted()
        {
            if (RunWorkerCompleted != null)
            {
                RunWorkerCompleted(this, null);
            }
        }


        private MySqlDataAdapter _shiftsDataAdapter;
        private DataTable _shiftsDataTable;

        private MySqlDataAdapter _timeTrackingDataAdapter;
        private DataTable _timeTrackingDataTable;

        public WorkersStatValues WorkersStatValuesStuct;

        public DataTable WorkersStatDataTable;
        public DataTable OperationCommonStatisticDataTable;

        private IdToMeasureUnitNameConverter _idToMeasureUnitNameConverter = new IdToMeasureUnitNameConverter();
        private readonly MeasureUnitNameFromOperationIdConverter _measureUnitNameFromOperationIdConverter =
            new MeasureUnitNameFromOperationIdConverter();

        private IdToNameConverter _idToNameConverter = new IdToNameConverter();

        private DateTime _fromDateTime;
        private DateTime _toDateTime;

        public DateTime GetDateFrom()
        {
            return _fromDateTime;
        }

        public DateTime GetDateTo()
        {
            return _toDateTime;
        }

        public TimeControlClass()
        {
            Initialize();
        }

        private void Initialize()
        {
            Create();

            CreateColumnsWorkersStatDataTable();
            CreateColumnsOperationCommonStatisticDataTable(OperationCommonStatisticDataTable);
        }

        private void Create()
        {
            _shiftsDataTable = new DataTable();
            _timeTrackingDataTable = new DataTable();

            WorkersStatDataTable = new DataTable();

            OperationCommonStatisticDataTable = new DataTable();

            WorkersStatValuesStuct = new WorkersStatValues
            {
                CommonTime = 0,
                FVCommonTime = 0,
                SVCommonTime = 0,
                TVCommonTime = 0,
                AllVCommonTime = 0,
                OneOfVCommonTime = 0,
                WorkerCommonTime = 0,
                MentorCommonTime = 0,
                StudentCommonTime = 0
            };
        }

        private void CreateColumnsWorkersStatDataTable()
        {
            WorkersStatDataTable.Columns.Add("Date", typeof(String));
            WorkersStatDataTable.Columns.Add("ShiftNumber", typeof(Int32));
            WorkersStatDataTable.Columns.Add("FactoryID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("WorkUnitID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("WorkSectionID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("WorkSubsectionID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("WorkOperationID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("TotalTime", typeof(String));
            WorkersStatDataTable.Columns.Add("FVTime", typeof(String));
            WorkersStatDataTable.Columns.Add("SVTime", typeof(String));
            WorkersStatDataTable.Columns.Add("TVTime", typeof(String));
            WorkersStatDataTable.Columns.Add("AllVTime", typeof(String));
            WorkersStatDataTable.Columns.Add("OneOfVTime", typeof(String));
            WorkersStatDataTable.Columns.Add("WorkScope", typeof(string));
            WorkersStatDataTable.Columns.Add("VCLP", typeof(decimal));
            WorkersStatDataTable.Columns.Add("OperationGroupID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("OperationTypeID", typeof(Int64));
            WorkersStatDataTable.Columns.Add("MeasureUnit", typeof(String));

            WorkersStatDataTable.Columns.Add("TaskID", typeof (Int64));
        }

        public void FillShifts(DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (_shiftsDataTable != null) _shiftsDataTable.Clear();

                //DataTable dt = new DataTable();

                const string commandText = @"SELECT TimeSpentAtWorkID, WorkerID, Date, WorkDayTimeStart, 
                                                    WorkDayTimeEnd, DinnerTimeStart, DinnerTimeEnd, Notes, 
                                                    ShiftNumber, MainWorkerID, MainWorkerNotes, VCLP, DayEnd 
                                                    FROM FAIITimeTracking.TimeSpentAtWork 
                                                    WHERE Date BETWEEN @FromDate AND @ToDate";

                var shiftsConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString);

                var command = new MySqlCommand(commandText, shiftsConnection);

                command.Parameters.Add("@FromDate", MySqlDbType.DateTime).Value = fromDate;
                command.Parameters.Add("@ToDate", MySqlDbType.DateTime).Value = toDate.AddDays(1);

                _shiftsDataAdapter = new MySqlDataAdapter(command);
// ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_shiftsDataAdapter);

                _shiftsDataAdapter.Fill(_shiftsDataTable);

                _fromDateTime = fromDate;
                _toDateTime = toDate;

                //_shiftsDataTable = dt.Clone();

                //foreach (DataRow dr in dt.Rows)
                //{
                //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                //        () =>
                //        {
                //            _shiftsDataTable.Rows.Add(dr.ItemArray);
                //        }));
                //}

            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[tcc0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FillTimeTracking(DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (_timeTrackingDataTable != null) _timeTrackingDataTable.Clear();

                const string commandText = @"SELECT WorkersTimeTrackingID, TimeSpentAtWorkID, WorkDayTimeStart,
                                                    WorkerID, WorkerNotes, WorkerGroupID, FactoryID, WorkUnitID, WorkSectionID,
                                                    WorkSubsectionID, WorkOperationID, TimeStart, TimeEnd, FirstStageVerification, 
                                                    FSVDate, FSVWorkerID, FSVWorkerNotes, SecondStageVerification, SSVDate, SSVWorkerID, 
                                                    SSVWorkerNotes, ThirdStageVerification, TSVDate, TSVWorkerID,
                                                    TSVWorkerNotes, DeleteRecord, MentorID, WorkStatusID, WorkScope, VCLP, OperationGroupID, OperationTypeID 
                                                    FROM FAIITimeTracking.WorkersTimeTracking 
                                                    WHERE DeleteRecord <> True AND  WorkDayTimeStart BETWEEN @FromDate AND @ToDate";

                var timeTrackingConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString);

                var command = new MySqlCommand(commandText, timeTrackingConnection);

                command.Parameters.Add("@FromDate", MySqlDbType.DateTime).Value = fromDate;
                command.Parameters.Add("@ToDate", MySqlDbType.DateTime).Value = toDate.AddDays(1);

                _timeTrackingDataAdapter = new MySqlDataAdapter(command);
                
                // ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_timeTrackingDataAdapter);
                //new MySqlCommandBuilder(_timeTrackingDataAdapter,);
                
                _timeTrackingDataAdapter.Fill(_timeTrackingDataTable);

                _fromDateTime = fromDate;
                _toDateTime = toDate;
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[tcc0002] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public DataView GetShifts()
        {
            var view = _shiftsDataTable.AsDataView();
            view.Sort = "WorkDayTimeStart";

            return view;
        }

        public DataView GetTimeTracking()
        {
            return _timeTrackingDataTable.AsDataView();
        }

        public DataView GetWorkersStat()
        {
            return WorkersStatDataTable.AsDataView();
        }

        public DataView GetCommonStat()
        {
            return OperationCommonStatisticDataTable.AsDataView();
        }

        #region time_control

        public void SaveTimeSpentAtWork(string mainWorkerNotes, decimal vclp, int shiftNumber, int timeSpentAtWorkID)
        {
            using (DataView dv = _shiftsDataTable.AsDataView())
            {
                dv.Sort = "TimeSpentAtWorkID";

                DataRowView[] dataRows = dv.FindRows(timeSpentAtWorkID.ToString(CultureInfo.InvariantCulture));

                if (dataRows.Count() != 0)
                {
                    dataRows[0]["ShiftNumber"] = shiftNumber;
                    dataRows[0]["MainWorkerID"] = AdministrationClass.CurrentWorkerId;
                    dataRows[0]["MainWorkerNotes"] = mainWorkerNotes;
                    dataRows[0]["VCLP"] = vclp;

                    _shiftsDataAdapter.Update(_shiftsDataTable);
                }
            }

        }

        public void SaveTimeTracking()
        {
            try
            {
                _timeTrackingDataAdapter.ContinueUpdateOnError = true;
                _timeTrackingDataAdapter.Update(_timeTrackingDataTable);

                FillTimeTracking(_fromDateTime, _toDateTime);

                //// First process deletes.
                //_timeTrackingDataAdapter.Update(_timeTrackingDataTable.Select(null, null, DataViewRowState.Deleted));

                //// Next process updates.
                //DataRow[] mdr = _timeTrackingDataTable.Select(null, null, DataViewRowState.ModifiedCurrent);
                //_timeTrackingDataAdapter.Update(mdr);

                //// Finally, process inserts.
                //_timeTrackingDataAdapter.Update(_timeTrackingDataTable.Select(null, null, DataViewRowState.Added));



                ////_timeTrackingDataTable.AcceptChanges();

                //int c = _timeTrackingDataTable.Rows.Count;
                //DataRow[] dr = _timeTrackingDataTable.Select(null, null, DataViewRowState.Unchanged);

                //DataTable changes = _timeTrackingDataTable.GetChanges();

                //if (changes != null)
                //{
                //    _timeTrackingDataAdapter.Update(changes);

                //    foreach (DataRow row in changes.Rows)
                //    {
                //        row.AcceptChanges();
                //    }
                //}

                //_timeTrackingDataTable.AcceptChanges();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        public TimeSpan CountingTotalTime(DataView timeTrackingDataView)
        {
            var result = new TimeSpan();

            if (_timeTrackingDataTable == null) return result;

            var totalTime = new TimeSpan();

            foreach (DataRowView drv in timeTrackingDataView)
            {
                var timeStart = (TimeSpan)drv.Row["TimeStart"];
                var timeEnd = (TimeSpan)drv.Row["TimeEnd"];

                if (timeEnd >= timeStart)
                {
                    totalTime = totalTime + new TimeSpan(timeEnd.Hours - timeStart.Hours,
                                                         timeEnd.Minutes - timeStart.Minutes, 0);
                }
                else
                {
                    totalTime = totalTime + new TimeSpan((24 - timeStart.Hours) + timeEnd.Hours,
                                                         timeEnd.Minutes - timeStart.Minutes, 0);
                }
            }

            result = totalTime;

            //if (totalTime.Days == 0)
            //    result = totalTime;
            //else
            //{
            //    string m = totalTime.Minutes.ToString(CultureInfo.InvariantCulture);
            //    if (totalTime.Minutes < 10)
            //    {
            //        m = "0" + totalTime.Minutes;
            //    }

            //    result = (totalTime.Days * 24) + totalTime.Hours + ":" + m;
            //}

            return result;
        }

        public void FirstStageVerification(DataRow dr)
        {
            dr["FSVDate"] = App.BaseClass.GetDateFromSqlServer();
            dr["FSVWorkerID"] = AdministrationClass.CurrentWorkerId;
            dr["FirstStageVerification"] = true;
        }

        public void SecondStageVerification(DataRow dr)
        {
            dr["SSVDate"] = App.BaseClass.GetDateFromSqlServer();
            dr["SSVWorkerID"] = AdministrationClass.CurrentWorkerId;
            dr["SecondStageVerification"] = true;
        }

        public void ThirdStageVerification(DataRow dr)
        {
            dr["TSVDate"] = App.BaseClass.GetDateFromSqlServer();
            dr["TSVWorkerID"] = AdministrationClass.CurrentWorkerId;
            dr["ThirdStageVerification"] = true;
        }

        private void UpdateTimeSpentAtWork()
        {
            try
            {
                _shiftsDataAdapter.Update(_shiftsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        #endregion


        public void ResetStatisticsObjects()
        {
            WorkersStatDataTable.Rows.Clear();

            WorkersStatValuesStuct.CommonTime = 0;

            WorkersStatValuesStuct.FVCommonTime = 0;
            WorkersStatValuesStuct.SVCommonTime = 0;
            WorkersStatValuesStuct.TVCommonTime = 0;
            WorkersStatValuesStuct.AllVCommonTime = 0;
            WorkersStatValuesStuct.OneOfVCommonTime = 0;

            WorkersStatValuesStuct.OperationTotalTime = 0;
            WorkersStatValuesStuct.WorkerCommonTime = 0;
            WorkersStatValuesStuct.MentorCommonTime = 0;
            WorkersStatValuesStuct.StudentCommonTime = 0;
            WorkersStatValuesStuct.TasksTotalTime = 0;
        }

        public decimal CalculateTotalTimeForShift(List<DataRow> statisticsDRs)
        {
            return statisticsDRs.Where(sdr => sdr["TotalTime"] != DBNull.Value)
                .Aggregate(Decimal.Zero,
                    (current, sdr) =>
                        current + Convert.ToDecimal(sdr["TotalTime"].ToString().Replace(".", ",")));
        }

        private decimal GetHoursFromTime(TimeSpan time)
        {
            return time != TimeSpan.Zero ? 24 * time.Days + time.Hours + Decimal.Round((decimal)time.Minutes / 60, 2) : Decimal.Zero;
        }

        public List<long> GetDistinctOperationIds(DataView timeTrackingDataView)
        {
            return
                (timeTrackingDataView.ToTable()
                    .AsEnumerable()
                    .Select(names => new { WorkOperationID = names.Field<Int64>("WorkOperationID") })).Distinct()
                    .Select(distinctOperationId => distinctOperationId.WorkOperationID)
                    .ToList();
        }

        public List<long> GetDistinctTaskIds(DataView taskTimeTrackingDataView)
        {
            return
                (taskTimeTrackingDataView.Cast<DataRowView>()
                    .Select(row => row.Row.Field<Int64>("TaskID"))
                    .Distinct()
                    .ToList());
        }

        public IEnumerable<DataRow> CalculateStatisticsForOneShift(DataRowView shiftDataRowView, int workerId,
            DataView timeTrackingDataView, DataView taskTimeTrackingDataView, string workerName)
        {
            var timeSpentAtWorkId = Convert.ToInt32(shiftDataRowView["TimeSpentAtWorkID"]);
            var dateFilterString =
                Convert.ToDateTime(shiftDataRowView["WorkDayTimeStart"]).ToString("yyyy-MM-dd HH:mm:ss.fff");

            timeTrackingDataView.RowFilter =
                String.Format("DeleteRecord <> 'True' AND WorkDayTimeStart= #{0}# AND WorkerID={1}", dateFilterString,
                    workerId);

            taskTimeTrackingDataView.RowFilter = string.Format("TimeSpentAtWorkID = {0}", timeSpentAtWorkId);

            var distinctOperationIDs = GetDistinctOperationIds(timeTrackingDataView);
            var distinctTaskIDs = GetDistinctTaskIds(taskTimeTrackingDataView);
            var statisticsDRs = new List<DataRow>();

            var workersStatDr = WorkersStatDataTable.NewRow();

            workersStatDr["Date"] = shiftDataRowView["WorkDayTimeStart"];
            workersStatDr["ShiftNumber"] = shiftDataRowView["ShiftNumber"];

            statisticsDRs.Add(workersStatDr);

            foreach (var operationId in distinctOperationIDs)
            {
                var timeTrackingDataRows =
                    timeTrackingDataView.Table.Select(String.Format("WorkDayTimeStart= #{0}#", dateFilterString));

                CalculateStatisticsForOneOperation(operationId, timeTrackingDataRows, workerName, statisticsDRs);
            }

            foreach (var taskId in distinctTaskIDs)
            {
                var taskTimeTrackingRows =
                    taskTimeTrackingDataView.Table.Select(string.Format("TimeSpentAtWorkID = {0}", timeSpentAtWorkId));

                CalculateStatisticsForOneTask(taskId, taskTimeTrackingRows, workerName, statisticsDRs);
            }

            decimal totalShiftTime = CalculateTotalTimeForShift(statisticsDRs);
            decimal commonFVShiftTime = CalculateFirstVerificationTimeForShift(statisticsDRs);
            decimal commonSVShiftTime = CalculateSecondVerificationTimeForShift(statisticsDRs);
            decimal commonTVShiftTime = CalculateThirdVerificationTimeForShift(statisticsDRs);
            decimal commonAllVShiftTime = CalculateAllVerificationTimeForShift(statisticsDRs);
            decimal commonOneOfVShiftTime = CalculateOneOffVerificationTimeForShift(statisticsDRs);

            WorkersStatValuesStuct.CommonTime = WorkersStatValuesStuct.CommonTime + totalShiftTime;
            WorkersStatValuesStuct.FVCommonTime = WorkersStatValuesStuct.FVCommonTime + commonFVShiftTime;
            WorkersStatValuesStuct.SVCommonTime = WorkersStatValuesStuct.SVCommonTime + commonSVShiftTime;
            WorkersStatValuesStuct.TVCommonTime = WorkersStatValuesStuct.TVCommonTime + commonTVShiftTime;
            WorkersStatValuesStuct.AllVCommonTime = WorkersStatValuesStuct.AllVCommonTime + commonAllVShiftTime;
            WorkersStatValuesStuct.OneOfVCommonTime = WorkersStatValuesStuct.OneOfVCommonTime + commonOneOfVShiftTime;

            statisticsDRs[0]["TotalTime"] = totalShiftTime;
            statisticsDRs[0]["FVTime"] = commonFVShiftTime;
            statisticsDRs[0]["SVTime"] = commonSVShiftTime;
            statisticsDRs[0]["TVTime"] = commonTVShiftTime;
            statisticsDRs[0]["AllVTime"] = commonAllVShiftTime;
            statisticsDRs[0]["OneOfVTime"] = commonOneOfVShiftTime;

            return statisticsDRs;
        }


        //public List<DataRow> CalculateStatisticsForTimetracking(DataView timeTrackingDataView, string workerName, List<long> distinctOperationIDs)
        //{
        //    List<DataRow> statisticsDRs = new List<DataRow>();

        //    foreach (var operationId in distinctOperationIDs)
        //    {
        //        DataRow[] timeTrackingDataRows =
        //            timeTrackingDataView.Table.Select();

        //        CalculateStatisticsForOneOperation(operationId, timeTrackingDataRows, workerName, statisticsDRs);
        //    }

        //    decimal totalShiftTime = CalculateTotalTimeForShift(statisticsDRs);
        //    decimal commonFVShiftTime = CalculateFirstVerificationTimeForShift(statisticsDRs);
        //    decimal commonSVShiftTime = CalculateSecondVerificationTimeForShift(statisticsDRs);
        //    decimal commonTVShiftTime = CalculateThirdVerificationTimeForShift(statisticsDRs);
        //    decimal commonAllVShiftTime = CalculateAllVerificationTimeForShift(statisticsDRs);
        //    decimal commonOneOfVShiftTime = CalculateOneOffVerificationTimeForShift(statisticsDRs);

        //    WorkersStatValuesStuct.CommonTime = WorkersStatValuesStuct.CommonTime + totalShiftTime;
        //    WorkersStatValuesStuct.FVCommonTime = WorkersStatValuesStuct.FVCommonTime + commonFVShiftTime;
        //    WorkersStatValuesStuct.SVCommonTime = WorkersStatValuesStuct.SVCommonTime + commonSVShiftTime;
        //    WorkersStatValuesStuct.TVCommonTime = WorkersStatValuesStuct.TVCommonTime + commonTVShiftTime;
        //    WorkersStatValuesStuct.AllVCommonTime = WorkersStatValuesStuct.AllVCommonTime + commonAllVShiftTime;
        //    WorkersStatValuesStuct.OneOfVCommonTime = WorkersStatValuesStuct.OneOfVCommonTime + commonOneOfVShiftTime;

        //    statisticsDRs[0]["TotalTime"] = totalShiftTime;
        //    statisticsDRs[0]["FVTime"] = commonFVShiftTime;
        //    statisticsDRs[0]["SVTime"] = commonSVShiftTime;
        //    statisticsDRs[0]["TVTime"] = commonTVShiftTime;
        //    statisticsDRs[0]["AllVTime"] = commonAllVShiftTime;
        //    statisticsDRs[0]["OneOfVTime"] = commonOneOfVShiftTime;

        //    return statisticsDRs;
        //}

        public decimal CalculateFirstVerificationTimeForShift(List<DataRow> statisticsDRs)
        {
            return
                statisticsDRs.Where(sdr => sdr["FVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
                    (current, sdr) => current + Convert.ToDecimal(sdr["FVTime"].ToString().Replace(".", ",")));
        }

        public decimal CalculateSecondVerificationTimeForShift(List<DataRow> statisticsDRs)
        {
            return
                statisticsDRs.Where(sdr => sdr["SVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
                    (current, sdr) => current + Convert.ToDecimal(sdr["SVTime"].ToString().Replace(".", ",")));
        }

        public decimal CalculateThirdVerificationTimeForShift(List<DataRow> statisticsDRs)
        {
            return
                statisticsDRs.Where(sdr => sdr["TVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
                    (current, sdr) => current + Convert.ToDecimal(sdr["TVTime"].ToString().Replace(".", ",")));
        }

        public decimal CalculateAllVerificationTimeForShift(List<DataRow> statisticsDRs)
        {
            return
                statisticsDRs.Where(sdr => sdr["AllVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
                    (current, sdr) => current + Convert.ToDecimal(sdr["AllVTime"].ToString().Replace(".", ",")));
        }

        public decimal CalculateOneOffVerificationTimeForShift(List<DataRow> statisticsDRs)
        {
            return
                statisticsDRs.Where(sdr => sdr["OneOfVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
                    (current, sdr) =>
                        current + Convert.ToDecimal(sdr["OneOfVTime"].ToString().Replace(".", ",")));
        }


        public void CalculateStatisticsForOneOperation(long operationId, IEnumerable<DataRow> timeTrackingDataRows, string workerName, List<DataRow> statisticsDRs)
        {
            List<DataRow> oneOparetionTimeTrackingDataRows =
                timeTrackingDataRows.Where(ids => ids.Field<Int64>("WorkOperationID") == operationId).ToList();

            DataRow workersStatDataRow = WorkersStatDataTable.NewRow();

            #region local variables

            var tempTime = new TimeSpan();
            var workScope = new decimal();
            //var vclp = new decimal();
            //int vclpCount = 0;

            var fvTime = new TimeSpan();
            var svTime = new TimeSpan();
            var tvTime = new TimeSpan();
            var allVTime = new TimeSpan();
            var oneOfVTime = new TimeSpan();
            #endregion

            foreach (DataRow trackDataRow in oneOparetionTimeTrackingDataRows)
            {
                TimeSpan timeFromTrack = GetTimeFromTrack(trackDataRow);

                tempTime = tempTime.Add(timeFromTrack);

                decimal tempWorkScope = GetWorkScopeFromTrack(trackDataRow);

                workScope = workScope + tempWorkScope;

                //if (tempWorkScope != 0)
                //{
                //    vclp = vclp + GetVCLPFromTrack(trackDataRow);
                //}
                //vclpCount++;

                if (HaveFirstStageVerification(trackDataRow)) fvTime = fvTime.Add(timeFromTrack);
                if (HaveSecondStageVerification(trackDataRow)) svTime = svTime.Add(timeFromTrack);
                if (HaveThirdStageVerification(trackDataRow)) tvTime = tvTime.Add(timeFromTrack);
                if (HaveAllStagesVerification(trackDataRow)) allVTime = allVTime.Add(timeFromTrack);
                if (HaveOneOfStagesVerification(trackDataRow)) oneOfVTime = oneOfVTime.Add(timeFromTrack);

                SetTimeByWorkStatus(trackDataRow, timeFromTrack);
            }

            workersStatDataRow["FactoryID"] = oneOparetionTimeTrackingDataRows[0]["FactoryID"];
            workersStatDataRow["WorkUnitID"] = oneOparetionTimeTrackingDataRows[0]["WorkUnitID"];
            workersStatDataRow["WorkSectionID"] = oneOparetionTimeTrackingDataRows[0]["WorkSectionID"];
            workersStatDataRow["WorkSubsectionID"] = oneOparetionTimeTrackingDataRows[0]["WorkSubsectionID"];
            workersStatDataRow["WorkOperationID"] = oneOparetionTimeTrackingDataRows[0]["WorkOperationID"];
            workersStatDataRow["OperationGroupID"] = oneOparetionTimeTrackingDataRows[0]["OperationGroupID"];
            workersStatDataRow["OperationTypeID"] = oneOparetionTimeTrackingDataRows[0]["OperationTypeID"];
            workersStatDataRow["Date"] = workerName;

            int tt = (24 * tempTime.Days + tempTime.Hours);
            decimal ttt = Decimal.Round((decimal)tempTime.Minutes / 60, 2);
            decimal T = tt + ttt;

            WorkersStatValuesStuct.OperationTotalTime += T;
            workersStatDataRow["TotalTime"] = T;
            
            workersStatDataRow["WorkScope"] = Decimal.Round(workScope, 2).ToString("0.##");
            workersStatDataRow["MeasureUnit"] = _idToMeasureUnitNameConverter.Convert(workersStatDataRow["WorkOperationID"].ToString());

            double productivity =
                Convert.ToDouble(_measureUnitNameFromOperationIdConverter.Convert(operationId, "Productivity"));
            var vclp = GetVCLP(Convert.ToDecimal(workScope), productivity, tempTime.TotalHours);
            workersStatDataRow["VCLP"] = vclp;
            workersStatDataRow["FVTime"] = GetHoursFromTime(fvTime);
            workersStatDataRow["SVTime"] = GetHoursFromTime(svTime);
            workersStatDataRow["TVTime"] = GetHoursFromTime(tvTime);
            workersStatDataRow["AllVTime"] = GetHoursFromTime(allVTime);
            workersStatDataRow["OneOfVTime"] = GetHoursFromTime(oneOfVTime);

            statisticsDRs.Add(workersStatDataRow);
        }

        private double GetVCLP(decimal workScope, double currentProductivity, double totalHours)
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

        public void CalculateStatisticsForOneTask(long taskId, IEnumerable<DataRow> timeTrackingDataRows,
            string workerName, List<DataRow> statisticsDRs)
        {
            var oneTaskTimeTrackingDataRows =
                timeTrackingDataRows.Where(ids => ids.Field<Int64>("TaskID") == taskId).ToList();

            var workersStatDataRow = WorkersStatDataTable.NewRow();

            #region local variables

            var tempTime = new TimeSpan();

            var fvTime = new TimeSpan();
            var svTime = new TimeSpan();
            var tvTime = new TimeSpan();
            var allVTime = new TimeSpan();
            var oneOfVTime = new TimeSpan();

            #endregion

            foreach (var trackDataRow in oneTaskTimeTrackingDataRows)
            {
                var timeFromTrack = GetTimeFromTrack(trackDataRow);

                tempTime = tempTime.Add(timeFromTrack);

                if (HaveMainWorkerStageVerification(trackDataRow))
                {
                    fvTime = fvTime.Add(timeFromTrack);
                    svTime = svTime.Add(timeFromTrack);
                    tvTime = tvTime.Add(timeFromTrack);
                    allVTime = allVTime.Add(timeFromTrack);
                    oneOfVTime = oneOfVTime.Add(timeFromTrack);
                }

                //SetTimeByWorkStatus(trackDataRow, timeFromTrack);
            }

            workersStatDataRow["TaskID"] = taskId;
            workersStatDataRow["Date"] = workerName;

            int tt = (24 * tempTime.Days + tempTime.Hours);
            decimal ttt = Decimal.Round((decimal)tempTime.Minutes / 60, 2);
            decimal T = tt + ttt;

            WorkersStatValuesStuct.TasksTotalTime += T;
            workersStatDataRow["TotalTime"] = T;

            workersStatDataRow["FVTime"] = GetHoursFromTime(fvTime);
            workersStatDataRow["SVTime"] = GetHoursFromTime(svTime);
            workersStatDataRow["TVTime"] = GetHoursFromTime(tvTime);
            workersStatDataRow["AllVTime"] = GetHoursFromTime(allVTime);
            workersStatDataRow["OneOfVTime"] = GetHoursFromTime(oneOfVTime);

            statisticsDRs.Add(workersStatDataRow);
        }

        private TimeSpan GetTimeFromTrack(DataRow trackDataRow)
        {
            var timeStart = (TimeSpan) trackDataRow["TimeStart"];
            var timeEnd = (TimeSpan) trackDataRow["TimeEnd"];

            return timeEnd >= timeStart
                ? timeEnd.Subtract(timeStart)
                : new TimeSpan(24, 0, 0).Subtract(timeEnd.Subtract(timeStart).Duration());
        }

        private decimal GetWorkScopeFromTrack(DataRow trackDataRow)
        {
            decimal tWorkScope;
            Decimal.TryParse(trackDataRow["WorkScope"].ToString(), out tWorkScope);

            return tWorkScope;
        }

        private decimal GetVCLPFromTrack(DataRow trackDataRow)
        {
            decimal tVCLP;
            Decimal.TryParse(trackDataRow["VCLP"].ToString(), out tVCLP);

            return tVCLP;
        }

        private bool HaveFirstStageVerification(DataRow trackDataRow)
        {
            return ((trackDataRow["FirstStageVerification"] != DBNull.Value) &&
                    (Convert.ToBoolean(trackDataRow["FirstStageVerification"])));
        }

        private bool HaveSecondStageVerification(DataRow trackDataRow)
        {
            return ((trackDataRow["SecondStageVerification"] != DBNull.Value) &&
                (Convert.ToBoolean(trackDataRow["SecondStageVerification"])));
        }

        private bool HaveThirdStageVerification(DataRow trackDataRow)
        {
            return ((trackDataRow["ThirdStageVerification"] != DBNull.Value) &&
                (Convert.ToBoolean(trackDataRow["ThirdStageVerification"])));
        }

        private bool HaveMainWorkerStageVerification(DataRow trackDataRow)
        {
            return ((trackDataRow["IsVerificated"] != DBNull.Value) &&
                (Convert.ToBoolean(trackDataRow["IsVerificated"])));
        }

        private bool HaveAllStagesVerification(DataRow trackDataRow)
        {
            return (((trackDataRow["FirstStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["FirstStageVerification"]))) &&
                ((trackDataRow["SecondStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["SecondStageVerification"]))) &&
                ((trackDataRow["ThirdStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["ThirdStageVerification"]))));
        }

        private bool HaveOneOfStagesVerification(DataRow trackDataRow)
        {
            return (((trackDataRow["FirstStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["FirstStageVerification"]))) ||
                ((trackDataRow["SecondStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["SecondStageVerification"]))) ||
                ((trackDataRow["ThirdStageVerification"] != DBNull.Value) &&
                 (Convert.ToBoolean(trackDataRow["ThirdStageVerification"]))));
        }


        private void SetTimeByWorkStatus(DataRow trackDataRow, TimeSpan timeFromTrack)
        {
            switch (Convert.ToInt32(trackDataRow["WorkStatusID"]))
            {
                case 1:
                {
                    WorkersStatValuesStuct.WorkerCommonTime = WorkersStatValuesStuct.WorkerCommonTime +
                                                              GetHoursFromTime(timeFromTrack);
                    break;
                }
                case 2:
                {
                    WorkersStatValuesStuct.MentorCommonTime = WorkersStatValuesStuct.MentorCommonTime +
                                                              GetHoursFromTime(timeFromTrack);
                    break;
                }
                case 3:
                {
                    WorkersStatValuesStuct.StudentCommonTime = WorkersStatValuesStuct.StudentCommonTime +
                                                               GetHoursFromTime(timeFromTrack);
                    break;
                }
            }
        }



        //public void CalculateWorkersStat(int[] workerIDs)
        //{
        //    #region reset_

        //    WorkersStatDataTable.Rows.Clear();

        //    WorkersStatValuesStuct.CommonTime = 0;

        //    WorkersStatValuesStuct.FVCommonTime = 0;
        //    WorkersStatValuesStuct.SVCommonTime = 0;
        //    WorkersStatValuesStuct.TVCommonTime = 0;
        //    WorkersStatValuesStuct.AllVCommonTime = 0;
        //    WorkersStatValuesStuct.OneOfVCommonTime = 0;

        //    WorkersStatValuesStuct.WorkerCommonTime = 0;
        //    WorkersStatValuesStuct.MentorCommonTime = 0;
        //    WorkersStatValuesStuct.StudentCommonTime = 0;

        //    #endregion

        //    #region init_

        //    DataView shiftsDv = GetShifts();
        //    DataView trackingDv = GetTimeTracking();

        //    var workerStatusTime = new TimeSpan();
        //    var mentorStatusTime = new TimeSpan();
        //    var studentStatusTime = new TimeSpan();

        //    decimal onePercent = shiftsDv.Count != 0 ? (decimal) 100/shiftsDv.Count : 100;
        //    decimal currentPercent = 0;



        //    #endregion

        //    foreach (int workerID in workerIDs)
        //    {
        //        shiftsDv.RowFilter = "WorkerID=" + workerID;

        //        foreach (DataRowView drv in shiftsDv)
        //        {
        //            string dateString = Convert.ToDateTime(drv["WorkDayTimeStart"]).ToString("yyyy-MM-dd HH:mm:ss.fff");

        //            trackingDv.RowFilter =
        //                "DeleteRecord <> 'True' AND WorkDayTimeStart= #" +
        //                dateString + "# AND WorkerID=" + workerID;

        //            var distinctOperationIDs =
        //                (trackingDv.ToTable().AsEnumerable().Select(names => new
        //                {
        //                    WorkOperationID
        //                        =
        //                        names.
        //                            Field
        //                            <Int64>(
        //                                "WorkOperationID")
        //                }))
        //                    .Distinct().ToArray();

        //            var statDRs = new List<DataRow>();

        //            DataRow workersStatDr = WorkersStatDataTable.NewRow();

        //            workersStatDr["Date"] = drv["WorkDayTimeStart"];
        //            workersStatDr["ShiftNumber"] = drv["ShiftNumber"];

        //            statDRs.Add(workersStatDr);

        //            foreach (var operationID in distinctOperationIDs)
        //            {
        //                string filterString = "WorkDayTimeStart= #" + dateString + "# AND WorkOperationID=" +
        //                                      operationID.WorkOperationID;

        //                trackingDv.RowFilter = filterString;

        //                DataRow workersStatDataRow = WorkersStatDataTable.NewRow();

        //                var tempTime = new TimeSpan();
        //                var workScope = new decimal();
        //                var VCLP = new decimal();
        //                int vclpC = 0;

        //                var fvTime = new TimeSpan();
        //                var svTime = new TimeSpan();
        //                var tvTime = new TimeSpan();
        //                var allVTime = new TimeSpan();
        //                var oneOfVTime = new TimeSpan();

        //                foreach (DataRowView trackingDRV in trackingDv)
        //                {
        //                    var timeStart = (TimeSpan) trackingDRV["TimeStart"];
        //                    var timeEnd = (TimeSpan) trackingDRV["TimeEnd"];

        //                    decimal tWorkScope;
        //                    Decimal.TryParse(trackingDRV["WorkScope"].ToString(), out tWorkScope);

        //                    Decimal tVCLP = 0;
        //                    if (trackingDRV["WorkScope"] != DBNull.Value && tWorkScope != 0)
        //                    {
        //                        Decimal.TryParse(trackingDRV["VCLP"].ToString(), out tVCLP);
        //                        vclpC++;
        //                    }
        //                    VCLP = VCLP + tVCLP;

        //                    var tTime = timeEnd >= timeStart
        //                        ? timeEnd.Subtract(timeStart)
        //                        : new TimeSpan(24, 0, 0).Subtract(timeEnd.Subtract(timeStart).Duration());

        //                    tempTime = tempTime.Add(tTime);

        //                    workScope = workScope + tWorkScope;


        //                    #region verifications_

        //                    if ((trackingDRV["FirstStageVerification"] != DBNull.Value) &&
        //                        (Convert.ToBoolean(trackingDRV["FirstStageVerification"])))
        //                    {
        //                        fvTime = fvTime.Add(tTime);
        //                    }

        //                    if ((trackingDRV["SecondStageVerification"] != DBNull.Value) &&
        //                        (Convert.ToBoolean(trackingDRV["SecondStageVerification"])))
        //                    {
        //                        svTime = svTime.Add(tTime);
        //                    }

        //                    if ((trackingDRV["ThirdStageVerification"] != DBNull.Value) &&
        //                        (Convert.ToBoolean(trackingDRV["ThirdStageVerification"])))
        //                    {
        //                        tvTime = tvTime.Add(tTime);
        //                    }

        //                    if (((trackingDRV["FirstStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["FirstStageVerification"]))) &&
        //                        ((trackingDRV["SecondStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["SecondStageVerification"]))) &&
        //                        ((trackingDRV["ThirdStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["ThirdStageVerification"]))))
        //                    {
        //                        allVTime = allVTime.Add(tTime);
        //                    }


        //                    if (((trackingDRV["FirstStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["FirstStageVerification"]))) ||
        //                        ((trackingDRV["SecondStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["SecondStageVerification"]))) ||
        //                        ((trackingDRV["ThirdStageVerification"] != DBNull.Value) &&
        //                         (Convert.ToBoolean(trackingDRV["ThirdStageVerification"]))))
        //                    {
        //                        oneOfVTime = oneOfVTime.Add(tTime);
        //                    }

        //                    #endregion

        //                    #region workStatuses_

        //                    switch (Convert.ToInt32(trackingDRV["WorkStatusID"]))
        //                    {
        //                        case 1:
        //                        {
        //                            workerStatusTime = workerStatusTime.Add(tTime);
        //                            break;
        //                        }
        //                        case 2:
        //                        {
        //                            mentorStatusTime = mentorStatusTime.Add(tTime);
        //                            break;
        //                        }
        //                        case 3:
        //                        {
        //                            studentStatusTime = studentStatusTime.Add(tTime);
        //                            break;
        //                        }
        //                    }

        //                    #endregion

        //                    //workScope

        //                    workersStatDataRow["FactoryID"] = trackingDRV["FactoryID"];
        //                    workersStatDataRow["WorkUnitID"] = trackingDRV["WorkUnitID"];
        //                    workersStatDataRow["WorkSectionID"] = trackingDRV["WorkSectionID"];
        //                    workersStatDataRow["WorkSubsectionID"] = trackingDRV["WorkSubsectionID"];
        //                    workersStatDataRow["WorkOperationID"] = trackingDRV["WorkOperationID"];
        //                    workersStatDataRow["OperationGroupID"] = trackingDRV["OperationGroupID"];
        //                    workersStatDataRow["OperationTypeID"] = trackingDRV["OperationTypeID"];

        //                }


        //                int tt = (24*tempTime.Days + tempTime.Hours);
        //                decimal ttt = Decimal.Round((decimal) tempTime.Minutes/60, 2);
        //                decimal T = tt + ttt;
        //                string tempTimeString = T.ToString(CultureInfo.InvariantCulture);
        //                workersStatDataRow["TotalTime"] = tempTimeString;

        //                workersStatDataRow["WorkScope"] = Decimal.Round(workScope, 2).ToString("0.##");


        //                workersStatDataRow["MeasureUnit"] = _idToMeasureUnitNameConverter.Convert(
        //                    workersStatDataRow["WorkOperationID"].ToString());

        //                workersStatDataRow["VCLP"] = vclpC != 0 ? Decimal.Round(VCLP/vclpC, 2) : 0;

        //                #region verifications_

        //                #region old_deleted

        //                //tt = (24 * fvTime.Days + fvTime.Hours);
        //                //ttt = Decimal.Round((decimal)fvTime.Minutes / 60, 2);
        //                //T = tt + ttt;
        //                //fvTimeString = T.ToString();
        //                //workersStatDr["FVTime"] = fvTimeString;

        //                //tt = (24 * svTime.Days + svTime.Hours);
        //                //ttt = Decimal.Round((decimal)svTime.Minutes / 60, 2);
        //                //T = tt + ttt;
        //                //svTimeString = T.ToString();
        //                //workersStatDr["SVTime"] = svTimeString;

        //                //tt = (24 * tvTime.Days + tvTime.Hours);
        //                //ttt = Decimal.Round((decimal)tvTime.Minutes / 60, 2);
        //                //T = tt + ttt;
        //                //tvTimeString = T.ToString();
        //                //workersStatDr["TVTime"] = tvTimeString;

        //                //tt = (24 * allVTime.Days + allVTime.Hours);
        //                //ttt = Decimal.Round((decimal)allVTime.Minutes / 60, 2);
        //                //T = tt + ttt;
        //                //allVTimeString = T.ToString();
        //                //workersStatDr["AllVTime"] = allVTimeString;

        //                //tt = (24 * oneOfVTime.Days + oneOfVTime.Hours);
        //                //ttt = Decimal.Round((decimal)oneOfVTime.Minutes / 60, 2);
        //                //T = tt + ttt;
        //                //oneOfVTimeString = T.ToString();
        //                //workersStatDr["OneOfVTime"] = oneOfVTimeString;

        //                #endregion

        //                workersStatDataRow["FVTime"] =
        //                    ((24*fvTime.Days + fvTime.Hours) + Decimal.Round((decimal) fvTime.Minutes/60, 2)).ToString(
        //                        CultureInfo.InvariantCulture);

        //                workersStatDataRow["SVTime"] =
        //                    ((24*svTime.Days + svTime.Hours) + Decimal.Round((decimal) svTime.Minutes/60, 2)).ToString(
        //                        CultureInfo.InvariantCulture);

        //                workersStatDataRow["TVTime"] =
        //                    ((24*tvTime.Days + tvTime.Hours) + Decimal.Round((decimal) tvTime.Minutes/60, 2)).ToString(
        //                        CultureInfo.InvariantCulture);

        //                workersStatDataRow["AllVTime"] =
        //                    ((24*allVTime.Days + allVTime.Hours) + Decimal.Round((decimal) allVTime.Minutes/60, 2))
        //                        .ToString
        //                        (CultureInfo.InvariantCulture);


        //                workersStatDataRow["OneOfVTime"] =
        //                    ((24*oneOfVTime.Days + oneOfVTime.Hours) + Decimal.Round((decimal) oneOfVTime.Minutes/60, 2))
        //                        .ToString(CultureInfo.InvariantCulture);

        //                #endregion

        //                statDRs.Add(workersStatDataRow);
        //            }

        //            decimal commonShiftTime =
        //                statDRs.Where(sdr => sdr["TotalTime"] != DBNull.Value)
        //                    .Aggregate(Decimal.Zero,
        //                        (current, sdr) =>
        //                            current + Convert.ToDecimal(sdr["TotalTime"].ToString().Replace(".", ",")));


        //            WorkersStatValuesStuct.CommonTime = WorkersStatValuesStuct.CommonTime + commonShiftTime;

        //            statDRs[0]["TotalTime"] = commonShiftTime;

        //            #region verifications_

        //            decimal commonFVShiftTime =
        //                statDRs.Where(sdr => sdr["FVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                    (current, sdr) => current + Convert.ToDecimal(sdr["FVTime"].ToString().Replace(".", ",")));

        //            WorkersStatValuesStuct.FVCommonTime = WorkersStatValuesStuct.FVCommonTime + commonFVShiftTime;

        //            statDRs[0]["FVTime"] = commonFVShiftTime;

        //            decimal commonSVShiftTime =
        //                statDRs.Where(sdr => sdr["SVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                    (current, sdr) => current + Convert.ToDecimal(sdr["SVTime"].ToString().Replace(".", ",")));
        //            WorkersStatValuesStuct.SVCommonTime = WorkersStatValuesStuct.SVCommonTime + commonSVShiftTime;
        //            statDRs[0]["SVTime"] = commonSVShiftTime;

        //            decimal commonTVShiftTime =
        //                statDRs.Where(sdr => sdr["TVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                    (current, sdr) => current + Convert.ToDecimal(sdr["TVTime"].ToString().Replace(".", ",")));

        //            WorkersStatValuesStuct.TVCommonTime = WorkersStatValuesStuct.TVCommonTime + commonTVShiftTime;
        //            statDRs[0]["TVTime"] = commonTVShiftTime;


        //            decimal commonAllVShiftTime =
        //                statDRs.Where(sdr => sdr["AllVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                    (current, sdr) => current + Convert.ToDecimal(sdr["AllVTime"].ToString().Replace(".", ",")));

        //            WorkersStatValuesStuct.AllVCommonTime = WorkersStatValuesStuct.AllVCommonTime + commonAllVShiftTime;
        //            statDRs[0]["AllVTime"] = commonAllVShiftTime;


        //            decimal commonOneOfVShiftTime =
        //                statDRs.Where(sdr => sdr["OneOfVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                    (current, sdr) =>
        //                        current + Convert.ToDecimal(sdr["OneOfVTime"].ToString().Replace(".", ",")));

        //            WorkersStatValuesStuct.OneOfVCommonTime = WorkersStatValuesStuct.OneOfVCommonTime +
        //                                                      commonOneOfVShiftTime;
        //            statDRs[0]["OneOfVTime"] = commonOneOfVShiftTime;

        //            #endregion

        //            foreach (DataRow sdr in statDRs.Where(sdr => sdr.RowState != DataRowState.Added))
        //            {
        //                WorkersStatDataTable.Rows.Add(sdr);
        //            }

        //            WorkersStatDataTable.Rows.Add();

        //            currentPercent = currentPercent + onePercent;

        //            CalculateStat_OnProgressChanged(Convert.ToInt32(currentPercent));

        //            DispatcherHelper.DoEvents();
        //        }
        //    }

        //    WorkersStatValuesStuct.WorkerCommonTime = 24 * workerStatusTime.Days + workerStatusTime.Hours +
        //                                              Decimal.Round((decimal)workerStatusTime.Minutes / 60, 2);
        //    WorkersStatValuesStuct.MentorCommonTime = 24 * mentorStatusTime.Days + mentorStatusTime.Hours +
        //                                              Decimal.Round((decimal)mentorStatusTime.Minutes / 60, 2);
        //    WorkersStatValuesStuct.StudentCommonTime = 24 * studentStatusTime.Days + studentStatusTime.Hours +
        //                                               Decimal.Round((decimal)studentStatusTime.Minutes / 60, 2);

        //    CalculateStat_OnRunWorkerCompleted();
        //}

        //public void CalculateOperationsStatistics(int wids)
        //{
        //    using (DataTable timeTrackingDataTableStat = TimeTrackingForControlDataTable.Copy())
        //    {
        //        using (DataTable timeSpentAtWorkDataTableStat = TimeSpentAtWorkForControlDataTable.Copy())
        //        {
        //            var timeSpentAtWorkViewSourceStat =
        //                new BindingListCollectionView(timeSpentAtWorkDataTableStat.DefaultView);

        //            //OperationDayStatisticDataTable.Rows.Clear();

        //            while (OperationDayStatisticDataTable.Rows.Count > 0)
        //            {
        //                OperationDayStatisticDataTable.Rows[0].Delete();
        //            }

        //            CommonTimeStatOp = 0;

        //            FVCommonTimeStatOp = 0;
        //            SVCommonTimeStatOp = 0;
        //            TVCommonTimeStatOp = 0;
        //            AllVCommonTimeStatOp = 0;
        //            OneOfVCommonTimeStatOp = 0;

        //            WorkerCommonTimeStatOp = 0;
        //            MentorCommonTimeStatOp = 0;
        //            StudentCommonTimeStatOp = 0;

        //            var workerStatusTime = new TimeSpan();
        //            var mentorStatusTime = new TimeSpan();
        //            var studentStatusTime = new TimeSpan();



        //            ICollection<int> wids = new Collection<int>() { 393, 248, 261, 275, 371, 291, 223, 429, 252, 259, 342, 369, 392 };

        //            foreach (int wid in wids)
        //            {
        //                #region
        //                timeSpentAtWorkViewSourceStat.CustomFilter = "WorkerID=" + wid;


        //                decimal onePercent = timeSpentAtWorkViewSourceStat.Count != 0
        //                    ? (decimal)100 / timeSpentAtWorkViewSourceStat.Count
        //                    : 100;
        //                decimal currentPercent = 0;




        //                foreach (DataRowView drv in timeSpentAtWorkViewSourceStat)
        //                {
        //                    DateTime dt = Convert.ToDateTime(drv["WorkDayTimeStart"]);
        //                    string dateString = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");

        //                    timeTrackingDataTableStat.DefaultView.RowFilter =
        //                        "DeleteRecord <> 'True' AND WorkDayTimeStart= #" +
        //                        dateString + "# AND WorkerID=" + wid;

        //                    var distinctOperationIDs =
        //                        (timeTrackingDataTableStat.DefaultView.ToTable().AsEnumerable().Select(names => new
        //                        {
        //                            WorkOperationID
        //                                =
        //                                names.
        //                                    Field
        //                                    <Int64>(
        //                                        "WorkOperationID")
        //                        }))
        //                            .Distinct().ToArray();

        //                    var statDRs = new List<DataRow>();

        //                    DataRow opdsdr = OperationDayStatisticDataTable.NewRow();

        //                    opdsdr["WorkerID"] = drv["WorkerID"];
        //                    opdsdr["Date"] = drv["WorkDayTimeStart"];
        //                    opdsdr["ShiftNumber"] = drv["ShiftNumber"];

        //                    statDRs.Add(opdsdr);

        //                    foreach (var operationID in distinctOperationIDs)
        //                    {
        //                        string filterString = "WorkDayTimeStart= #" + dateString +
        //                                              "# AND WorkOperationID=" + operationID.WorkOperationID;
        //                        timeTrackingDataTableStat.DefaultView.RowFilter = filterString;

        //                        string fvTimeString;
        //                        string svTimeString;
        //                        string tvTimeString;
        //                        string allVTimeString;
        //                        string oneOfVTimeString;

        //                        DataRow odsdr = OperationDayStatisticDataTable.NewRow();

        //                        var tempTime = new TimeSpan();
        //                        var fvTime = new TimeSpan();
        //                        var svTime = new TimeSpan();
        //                        var tvTime = new TimeSpan();
        //                        var allVTime = new TimeSpan();
        //                        var oneOfVTime = new TimeSpan();


        //                        var tempWorkScope = new decimal();
        //                        var tempVclp = new decimal();

        //                        String tempNotes = string.Empty;

        //                        foreach (DataRowView ttdrv in timeTrackingDataTableStat.DefaultView)
        //                        {
        //                            var timeStart = (TimeSpan)ttdrv["TimeStart"];
        //                            var timeEnd = (TimeSpan)ttdrv["TimeEnd"];


        //                            decimal ws;
        //                            Decimal.TryParse(ttdrv["WorkScope"].ToString(), out ws);
        //                            tempWorkScope = tempWorkScope + ws;

        //                            if (ttdrv["WorkScope"] != DBNull.Value)
        //                            {
        //                                decimal vclp;
        //                                Decimal.TryParse(ttdrv["VCLP"].ToString(), out vclp);
        //                                tempVclp = tempVclp + vclp;
        //                            }


        //                            if (ttdrv["WorkerNotes"].ToString().Trim() != string.Empty)
        //                                if (tempNotes == string.Empty)
        //                                    tempNotes = ttdrv["WorkerNotes"].ToString().Trim();
        //                                else
        //                                    tempNotes = tempNotes + ", " + ttdrv["WorkerNotes"].ToString().Trim();



        //                            if (timeEnd >= timeStart)
        //                            {
        //                                var tTime = timeEnd.Subtract(timeStart);

        //                                tempTime = tempTime.Add(tTime);

        //                                #region Verifications

        //                                if ((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["FirstStageVerification"])))
        //                                {
        //                                    fvTime = fvTime.Add(tTime);
        //                                }

        //                                if ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["SecondStageVerification"])))
        //                                {
        //                                    svTime = svTime.Add(tTime);
        //                                }

        //                                if ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["ThirdStageVerification"])))
        //                                {
        //                                    tvTime = tvTime.Add(tTime);
        //                                }

        //                                if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) &&
        //                                    ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) &&
        //                                    ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
        //                                {
        //                                    allVTime = allVTime.Add(tTime);
        //                                }


        //                                if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) ||
        //                                    ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) ||
        //                                    ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
        //                                {
        //                                    oneOfVTime = oneOfVTime.Add(tTime);
        //                                }

        //                                #endregion

        //                                #region WorkStatuses

        //                                switch (Convert.ToInt32(ttdrv["WorkStatusID"]))
        //                                {
        //                                    case 1:
        //                                        {
        //                                            workerStatusTime = workerStatusTime.Add(tTime);
        //                                            break;
        //                                        }
        //                                    case 2:
        //                                        {
        //                                            mentorStatusTime = mentorStatusTime.Add(tTime);
        //                                            break;
        //                                        }
        //                                    case 3:
        //                                        {
        //                                            studentStatusTime = studentStatusTime.Add(tTime);
        //                                            break;
        //                                        }
        //                                }

        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                var ttTime = new TimeSpan(24, 0, 0).Subtract(timeEnd.Subtract(timeStart).Duration());

        //                                tempTime = tempTime.Add(ttTime);

        //                                #region Verifications

        //                                if ((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["FirstStageVerification"])))
        //                                {
        //                                    fvTime = fvTime.Add(ttTime);
        //                                }

        //                                if ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["SecondStageVerification"])))
        //                                {
        //                                    svTime = svTime.Add(ttTime);
        //                                }

        //                                if ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                    (Convert.ToBoolean(ttdrv["ThirdStageVerification"])))
        //                                {
        //                                    tvTime = tvTime.Add(ttTime);
        //                                }

        //                                if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) &&
        //                                    ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) &&
        //                                    ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
        //                                {
        //                                    allVTime = allVTime.Add(ttTime);
        //                                }

        //                                if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) ||
        //                                    ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) ||
        //                                    ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
        //                                     (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
        //                                {
        //                                    oneOfVTime = oneOfVTime.Add(ttTime);
        //                                }

        //                                #endregion


        //                                #region WorkStatuses

        //                                switch (Convert.ToInt32(ttdrv["WorkStatusID"]))
        //                                {
        //                                    case 1:
        //                                        {
        //                                            workerStatusTime = workerStatusTime.Add(ttTime);
        //                                            break;
        //                                        }
        //                                    case 2:
        //                                        {
        //                                            mentorStatusTime = mentorStatusTime.Add(ttTime);
        //                                            break;
        //                                        }
        //                                    case 3:
        //                                        {
        //                                            studentStatusTime = studentStatusTime.Add(ttTime);
        //                                            break;
        //                                        }
        //                                }

        //                                #endregion
        //                            }


        //                            odsdr["FactoryID"] = ttdrv["FactoryID"];
        //                            odsdr["WorkUnitID"] = ttdrv["WorkUnitID"];
        //                            odsdr["WorkSectionID"] = ttdrv["WorkSectionID"];
        //                            odsdr["WorkSubsectionID"] = ttdrv["WorkSubsectionID"];
        //                            odsdr["WorkOperationID"] = ttdrv["WorkOperationID"];
        //                        }





        //                        int tt = (24 * tempTime.Days + tempTime.Hours);
        //                        decimal ttt = Decimal.Round((decimal)tempTime.Minutes / 60, 2);
        //                        decimal T = tt + ttt;
        //                        string tempTimeString = T.ToString(CultureInfo.InvariantCulture);
        //                        odsdr["TotalTime"] = tempTimeString;

        //                        odsdr["WorkerNotes"] = tempNotes;


        //                        odsdr["WorkScope"] = tempWorkScope + " " +
        //                                             _idToMeasureUnitNameConverter.Convert(
        //                                                 odsdr["WorkOperationID"].ToString());
        //                        odsdr["VCLP"] = tempVclp;

        //                        odsdr["WorkerID"] = drv["WorkerID"];


        //                        #region Verifications

        //                        tt = (24 * fvTime.Days + fvTime.Hours);
        //                        ttt = Decimal.Round((decimal)fvTime.Minutes / 60, 2);
        //                        T = tt + ttt;
        //                        fvTimeString = T.ToString();
        //                        odsdr["FVTime"] = fvTimeString;

        //                        tt = (24 * svTime.Days + svTime.Hours);
        //                        ttt = Decimal.Round((decimal)svTime.Minutes / 60, 2);
        //                        T = tt + ttt;
        //                        svTimeString = T.ToString();
        //                        odsdr["SVTime"] = svTimeString;

        //                        tt = (24 * tvTime.Days + tvTime.Hours);
        //                        ttt = Decimal.Round((decimal)tvTime.Minutes / 60, 2);
        //                        T = tt + ttt;
        //                        tvTimeString = T.ToString();
        //                        odsdr["TVTime"] = tvTimeString;

        //                        tt = (24 * allVTime.Days + allVTime.Hours);
        //                        ttt = Decimal.Round((decimal)allVTime.Minutes / 60, 2);
        //                        T = tt + ttt;
        //                        allVTimeString = T.ToString();
        //                        odsdr["AllVTime"] = allVTimeString;

        //                        tt = (24 * oneOfVTime.Days + oneOfVTime.Hours);
        //                        ttt = Decimal.Round((decimal)oneOfVTime.Minutes / 60, 2);
        //                        T = tt + ttt;
        //                        oneOfVTimeString = T.ToString();
        //                        odsdr["OneOfVTime"] = oneOfVTimeString;

        //                        #endregion

        //                        statDRs.Add(odsdr);
        //                    }

        //                    decimal commonShiftTime =
        //                        statDRs.Where(sdr => sdr["TotalTime"] != DBNull.Value)
        //                            .Aggregate(Decimal.Zero,
        //                                (current, sdr) =>
        //                                    current + Convert.ToDecimal(sdr["TotalTime"].ToString().Replace(".", ",")));

        //                    CommonTimeStatOp = CommonTimeStatOp + commonShiftTime;
        //                    statDRs[0]["TotalTime"] = commonShiftTime;

        //                    #region Verifications

        //                    decimal commonFVShiftTime =
        //                        statDRs.Where(sdr => sdr["FVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                            (current, sdr)
        //                                =>
        //                                current +
        //                                Convert.
        //                                    ToDecimal(
        //                                        sdr[
        //                                            "FVTime"
        //                                            ]));
        //                    FVCommonTimeStatOp = FVCommonTimeStatOp + commonFVShiftTime;
        //                    statDRs[0]["FVTime"] = commonFVShiftTime;

        //                    decimal commonSVShiftTime =
        //                        statDRs.Where(sdr => sdr["SVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                            (current, sdr)
        //                                =>
        //                                current +
        //                                Convert.
        //                                    ToDecimal(
        //                                        sdr[
        //                                            "SVTime"
        //                                            ]));
        //                    SVCommonTimeStatOp = SVCommonTimeStatOp + commonSVShiftTime;
        //                    statDRs[0]["SVTime"] = commonSVShiftTime;

        //                    decimal commonTVShiftTime =
        //                        statDRs.Where(sdr => sdr["TVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                            (current, sdr)
        //                                =>
        //                                current +
        //                                Convert.
        //                                    ToDecimal(
        //                                        sdr[
        //                                            "TVTime"
        //                                            ]));
        //                    TVCommonTimeStatOp = TVCommonTimeStatOp + commonTVShiftTime;
        //                    statDRs[0]["TVTime"] = commonTVShiftTime;


        //                    decimal commonAllVShiftTime =
        //                        statDRs.Where(sdr => sdr["AllVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                            (current, sdr)
        //                                =>
        //                                current +
        //                                Convert.
        //                                    ToDecimal(
        //                                        sdr[
        //                                            "AllVTime"
        //                                            ]));
        //                    AllVCommonTimeStatOp = AllVCommonTimeStatOp + commonAllVShiftTime;
        //                    statDRs[0]["AllVTime"] = commonAllVShiftTime;


        //                    decimal CommonOneOfVShiftTime =
        //                        statDRs.Where(SDR => SDR["OneOfVTime"] != DBNull.Value).Aggregate(Decimal.Zero,
        //                            (current, SDR)
        //                                =>
        //                                current +
        //                                Convert.
        //                                    ToDecimal(
        //                                        SDR[
        //                                            "OneOfVTime"
        //                                            ]));
        //                    OneOfVCommonTimeStatOp = OneOfVCommonTimeStatOp + CommonOneOfVShiftTime;
        //                    statDRs[0]["OneOfVTime"] = CommonOneOfVShiftTime;

        //                    #endregion

        //                    foreach (DataRow sdr in statDRs.Where(sdr => sdr.RowState != DataRowState.Added))
        //                    {
        //                        OperationDayStatisticDataTable.Rows.Add(sdr);
        //                    }

        //                    OperationDayStatisticDataTable.Rows.Add();

        //                    currentPercent = currentPercent + onePercent;
        //                    OnProgressChanged(Convert.ToInt32(currentPercent));
        //                    DispatcherHelper.DoEvents();


        //                }
        //                #endregion


        //                OperationDayStatisticDataTable.Rows.Add(OperationDayStatisticDataTable.NewRow());
        //            }

        //            WorkerCommonTimeStatOp = 24 * workerStatusTime.Days + workerStatusTime.Hours +
        //                                     Decimal.Round((decimal)workerStatusTime.Minutes / 60, 2);
        //            MentorCommonTimeStatOp = 24 * mentorStatusTime.Days + mentorStatusTime.Hours +
        //                                     Decimal.Round((decimal)mentorStatusTime.Minutes / 60, 2);
        //            StudentCommonTimeStatOp = 24 * studentStatusTime.Days + studentStatusTime.Hours +
        //                                      Decimal.Round((decimal)studentStatusTime.Minutes / 60, 2);
        //        }
        //    }

        //    OnRunWorkerCompleted();
        //}



        public void CalculateOperationsCommonStatistics(string filterCommonStatistics, DateTime fromDate, DateTime toDate, int shiftNumber = -1)
        {
            #region reset
            OperationCommonStatisticDataTable.Rows.Clear();

            WorkersStatValuesStuct.CommonTime = 0;

            WorkersStatValuesStuct.FVCommonTime = 0;
            WorkersStatValuesStuct.SVCommonTime = 0;
            WorkersStatValuesStuct.TVCommonTime = 0;
            WorkersStatValuesStuct.AllVCommonTime = 0;
            WorkersStatValuesStuct.OneOfVCommonTime = 0;

            WorkersStatValuesStuct.WorkerCommonTime = 0;
            WorkersStatValuesStuct.MentorCommonTime = 0;
            WorkersStatValuesStuct.StudentCommonTime = 0;

            #endregion

            DataTable timeTrackingDataTableComStat;

            string mainFilterString;
            if (filterCommonStatistics != string.Empty)
                mainFilterString = "DeleteRecord <> 'True' AND " + filterCommonStatistics;
            else
                mainFilterString = "DeleteRecord <> 'True'";

            if (shiftNumber != -1)
            {
                const string sqlCommandText = "SELECT " +
                                              "FAIITimeTracking.WorkersTimeTracking.TimeSpentAtWorkID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkDayTimeStart, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkerGroupID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.FactoryID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkUnitID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkSectionID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkSubsectionID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkOperationID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.TimeStart, " +
                                              "FAIITimeTracking.WorkersTimeTracking.TimeEnd , " +
                                              "FAIITimeTracking.WorkersTimeTracking.FirstStageVerification, " +
                                              "FAIITimeTracking.WorkersTimeTracking.SecondStageVerification, " +
                                              "FAIITimeTracking.WorkersTimeTracking.ThirdStageVerification , " +
                                              "FAIITimeTracking.WorkersTimeTracking.DeleteRecord, " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkScope, " +
                                              "FAIITimeTracking.WorkersTimeTracking.VCLP, " +
                                              "FAIITimeTracking.WorkersTimeTracking.OperationGroupID, " +
                                              "FAIITimeTracking.WorkersTimeTracking.OperationTypeID, " +
                                              "FAIITimeTracking.TimeSpentAtWork.ShiftNumber " +
                                              "FROM FAIITimeTracking.WorkersTimeTracking " +



                                              "INNER JOIN FAIITimeTracking.TimeSpentAtWork " +
                                              "ON FAIITimeTracking.WorkersTimeTracking.TimeSpentAtWorkID = FAIITimeTracking.TimeSpentAtWork.TimeSpentAtWorkID " +

                                              "WHERE FAIITimeTracking.TimeSpentAtWork.ShiftNumber = @ShiftNumber " +
                                              "AND FAIITimeTracking.WorkersTimeTracking.DeleteRecord <> True AND " +
                                              "FAIITimeTracking.WorkersTimeTracking.WorkDayTimeStart BETWEEN @FromDate AND @ToDate";

                using (var timeTrackingComStatDataAdapter = new MySqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString))
                {
                    MySqlCommand selectCommand = timeTrackingComStatDataAdapter.SelectCommand;
                    selectCommand.Parameters.Add("@ShiftNumber", MySqlDbType.Int64).Value = shiftNumber;
                    selectCommand.Parameters.Add("@FromDate", MySqlDbType.DateTime).Value = fromDate;
                    selectCommand.Parameters.Add("@ToDate", MySqlDbType.DateTime).Value = toDate.AddDays(1);

                    timeTrackingDataTableComStat = new DataTable();
                    timeTrackingComStatDataAdapter.Fill(timeTrackingDataTableComStat);
                }
            }
            else
            {
                timeTrackingDataTableComStat = _timeTrackingDataTable.Copy();
            }

            timeTrackingDataTableComStat.DefaultView.RowFilter = mainFilterString;

            var distinctOperationIDs =
                (timeTrackingDataTableComStat.DefaultView.ToTable().AsEnumerable().Select(names => new
                {
                    WorkOperationID = names.Field<Int64>("WorkOperationID")
                }))
                    .Distinct().ToArray();

            decimal onePercent = distinctOperationIDs.Count() != 0 ? (decimal)100 / distinctOperationIDs.Count() : 100;
            decimal currentPercent = 0;
            foreach (var operationID in distinctOperationIDs)
            {
                string operationFilterString = "WorkOperationID=" + operationID.WorkOperationID;
                timeTrackingDataTableComStat.DefaultView.RowFilter = mainFilterString + " AND " +
                                                                     operationFilterString;

                DataRow ocsdr = OperationCommonStatisticDataTable.NewRow();

                var tempTime = new TimeSpan();

                var workScope = new decimal();
                //var VCLP = new decimal();
                //int vclpC = 0;

                var fvTime = new TimeSpan();
                var svTime = new TimeSpan();
                var tvTime = new TimeSpan();
                var allVTime = new TimeSpan();
                var oneOfVTime = new TimeSpan();

                foreach (DataRowView ttdrv in timeTrackingDataTableComStat.DefaultView)
                {
                    var timeStart = (TimeSpan)ttdrv["TimeStart"];
                    var timeEnd = (TimeSpan)ttdrv["TimeEnd"];

                    decimal tWorkScope;
                    Decimal.TryParse(ttdrv["WorkScope"].ToString(), out tWorkScope);

                    //Decimal tVCLP = 0;
                    //if (ttdrv["WorkScope"] != DBNull.Value && tWorkScope != 0)
                    //{
                    //    Decimal.TryParse(ttdrv["VCLP"].ToString(), out tVCLP);
                    //    //vclpC++;
                    //}

                    //VCLP = VCLP + tVCLP;

                    tempTime =
                        tempTime.Add(timeEnd >= timeStart
                            ? timeEnd.Subtract(timeStart)
                            : new TimeSpan(24, 0, 0).Subtract(timeEnd.Subtract(timeStart).Duration()));


                    workScope = workScope + tWorkScope;

                    #region verifications_

                    if ((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                        (Convert.ToBoolean(ttdrv["FirstStageVerification"])))
                    {
                        fvTime = fvTime.Add(timeEnd.Subtract(timeStart));
                    }

                    if ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                        (Convert.ToBoolean(ttdrv["SecondStageVerification"])))
                    {
                        svTime = svTime.Add(timeEnd.Subtract(timeStart));
                    }

                    if ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                        (Convert.ToBoolean(ttdrv["ThirdStageVerification"])))
                    {
                        tvTime = tvTime.Add(timeEnd.Subtract(timeStart));
                    }

                    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) &&
                        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) &&
                        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    {
                        allVTime = allVTime.Add(timeEnd.Subtract(timeStart));
                    }


                    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) ||
                        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) ||
                        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    {
                        oneOfVTime = oneOfVTime.Add(timeEnd.Subtract(timeStart));
                    }

                    #endregion

                    #region old
                    //if (timeEnd >= timeStart)
                    //{
                    //    TempTime = TempTime.Add(timeEnd.Subtract(timeStart));

                    //    #region verifications_

                    //    if ((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["FirstStageVerification"])))
                    //    {
                    //        FVTime = FVTime.Add(timeEnd.Subtract(timeStart));
                    //    }

                    //    if ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["SecondStageVerification"])))
                    //    {
                    //        SVTime = SVTime.Add(timeEnd.Subtract(timeStart));
                    //    }

                    //    if ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["ThirdStageVerification"])))
                    //    {
                    //        TVTime = TVTime.Add(timeEnd.Subtract(timeStart));
                    //    }

                    //    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) &&
                    //        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) &&
                    //        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    //    {
                    //        AllVTime = AllVTime.Add(timeEnd.Subtract(timeStart));
                    //    }


                    //    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) ||
                    //        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) ||
                    //        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    //    {
                    //        OneOfVTime = OneOfVTime.Add(timeEnd.Subtract(timeStart));
                    //    }

                    //    #endregion
                    //}
                    //else
                    //{
                    //    TimeSpan ts = timeEnd.Subtract(timeStart).Duration();
                    //    TimeSpan tss = new TimeSpan(24, 0, 0).Subtract(ts);

                    //    TempTime = TempTime.Add(tss);

                    //    #region verifications_

                    //    if ((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["FirstStageVerification"])))
                    //    {
                    //        FVTime = FVTime.Add(tss);
                    //    }

                    //    if ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["SecondStageVerification"])))
                    //    {
                    //        SVTime = SVTime.Add(tss);
                    //    }

                    //    if ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //        (Convert.ToBoolean(ttdrv["ThirdStageVerification"])))
                    //    {
                    //        TVTime = TVTime.Add(tss);
                    //    }

                    //    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) &&
                    //        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) &&
                    //        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    //    {
                    //        AllVTime = AllVTime.Add(tss);
                    //    }

                    //    if (((ttdrv["FirstStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["FirstStageVerification"]))) ||
                    //        ((ttdrv["SecondStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["SecondStageVerification"]))) ||
                    //        ((ttdrv["ThirdStageVerification"] != DBNull.Value) &&
                    //         (Convert.ToBoolean(ttdrv["ThirdStageVerification"]))))
                    //    {
                    //        OneOfVTime = OneOfVTime.Add(tss);
                    //    }

                    //    #endregion
                    //}
                    #endregion

                    ocsdr["FactoryID"] = ttdrv["FactoryID"];
                    ocsdr["WorkUnitID"] = ttdrv["WorkUnitID"];
                    ocsdr["WorkSectionID"] = ttdrv["WorkSectionID"];
                    ocsdr["WorkSubsectionID"] = ttdrv["WorkSubsectionID"];
                    ocsdr["WorkOperationID"] = ttdrv["WorkOperationID"];
                    ocsdr["OperationGroupID"] = ttdrv["OperationGroupID"];
                    ocsdr["OperationTypeID"] = ttdrv["OperationTypeID"];
                }

                ocsdr["Time"] = ((24 * tempTime.Days + tempTime.Hours) + Decimal.Round((decimal)tempTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                ocsdr["WorkScope"] = Decimal.Round(workScope, 2).ToString("0.##") + " " + _idToMeasureUnitNameConverter.Convert(ocsdr["WorkOperationID"].ToString());

                double productivity = 
                    Convert.ToDouble(_measureUnitNameFromOperationIdConverter.Convert(operationID.WorkOperationID, "Productivity"));
                var vclp = GetVCLP(Convert.ToDecimal(workScope), productivity, tempTime.TotalHours);
                ocsdr["VCLP"] = vclp;

                #region Verifications
                ocsdr["FVTime"] = ((24 * fvTime.Days + fvTime.Hours) + Decimal.Round((decimal)fvTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                ocsdr["SVTime"] = ((24 * svTime.Days + svTime.Hours) + Decimal.Round((decimal)svTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                ocsdr["TVTime"] = ((24 * tvTime.Days + tvTime.Hours) + Decimal.Round((decimal)tvTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                ocsdr["AllVTime"] = ((24 * allVTime.Days + allVTime.Hours) + Decimal.Round((decimal)allVTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                ocsdr["OneOfVTime"] = ((24 * oneOfVTime.Days + oneOfVTime.Hours) + Decimal.Round((decimal)oneOfVTime.Minutes / 60, 2)).ToString(CultureInfo.InvariantCulture);
                #endregion

                OperationCommonStatisticDataTable.Rows.Add(ocsdr);

                currentPercent = currentPercent + onePercent;
                CalculateStat_OnProgressChanged(Convert.ToInt32(currentPercent));
                DispatcherHelper.DoEvents();
            }

            //OperationCommonStatisticDataTable = tempStatTable;
            CalculateStat_OnRunWorkerCompleted();
        }


        public void CalculateCommonStatTime()
        {
            WorkersStatValuesStuct.CommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["Time"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["Time"].ToString().Replace(".", ",")));

            WorkersStatValuesStuct.FVCommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["FVTime"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["FVTime"].ToString().Replace(".", ",")));

            WorkersStatValuesStuct.SVCommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["SVTime"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["SVTime"].ToString().Replace(".", ",")));

            WorkersStatValuesStuct.TVCommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["TVTime"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["TVTime"].ToString().Replace(".", ",")));

            WorkersStatValuesStuct.AllVCommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["AllVTime"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["AllVTime"].ToString().Replace(".", ",")));

            WorkersStatValuesStuct.OneOfVCommonTime =
                OperationCommonStatisticDataTable.DefaultView.ToTable()
                    .AsEnumerable()
                    .Where(sdr => sdr["OneOfVTime"] != DBNull.Value)
                    .Aggregate(Decimal.Zero, (current, sdr) => current + Convert.ToDecimal(sdr["OneOfVTime"].ToString().Replace(".", ",")));
        }

        private void CreateColumnsOperationCommonStatisticDataTable(DataTable dataTable)
        {
            dataTable.Columns.Add("FactoryID", typeof(Int64));
            dataTable.Columns.Add("WorkUnitID", typeof(Int64));
            dataTable.Columns.Add("WorkSectionID", typeof(Int64));
            dataTable.Columns.Add("WorkSubsectionID", typeof(Int64));
            dataTable.Columns.Add("WorkOperationID", typeof(Int64));
            dataTable.Columns.Add("OperationGroupID", typeof(Int64));
            dataTable.Columns.Add("OperationTypeID", typeof(Int64));
            dataTable.Columns.Add("Time", typeof(String));
            dataTable.Columns.Add("FVTime", typeof(String));
            dataTable.Columns.Add("SVTime", typeof(String));
            dataTable.Columns.Add("TVTime", typeof(String));
            dataTable.Columns.Add("AllVTime", typeof(String));
            dataTable.Columns.Add("OneOfVTime", typeof(String));

            dataTable.Columns.Add("WorkScope", typeof(string));
            dataTable.Columns.Add("VCLP", typeof(decimal));
        }



        public void AddNewTimeRecord(long timeSpentAtWorkId, DateTime workDayTimeStart, long workerId, int workerGroupID, int factoryID, int workUnitID, int workSectionID,
            int workSubsectionID, int workOperationID, int operationGroupID, int operationTypeID, TimeSpan timeStart, TimeSpan timeEnd, decimal workScope,
            double vclp,
            string workerNotes = null, int workStatusID = 1, int mentorID = -1)
        {
            var newRow = _timeTrackingDataTable.NewRow();
            newRow["TimeSpentAtWorkID"] = timeSpentAtWorkId;
            newRow["WorkDayTimeStart"] = workDayTimeStart;
            newRow["WorkerID"] = workerId;
            newRow["WorkScope"] = workScope;
            newRow["VCLP"] = vclp;
            newRow["WorkerNotes"] = workerNotes;
            newRow["WorkUnitID"] = workUnitID;
            newRow["WorkerGroupID"] = workerGroupID;
            newRow["FactoryID"] = factoryID;
            newRow["WorkSectionID"] = workSectionID;
            newRow["WorkSubsectionID"] = workSubsectionID;
            newRow["WorkOperationID"] = workOperationID;
            newRow["TimeStart"] = timeStart;
            newRow["TimeEnd"] = timeEnd;
            newRow["DeleteRecord"] = false;
            newRow["MentorID"] = mentorID;
            newRow["WorkStatusID"] = workStatusID;
            newRow["OperationGroupID"] = operationGroupID;
            newRow["OperationTypeID"] = operationTypeID;

            _timeTrackingDataTable.Rows.Add(newRow);
            UpdateTimeTracking();

            var timeRecordId = GetTimeRecordId(timeSpentAtWorkId, workDayTimeStart, workOperationID);
            newRow["WorkersTimeTrackingID"] = timeRecordId.HasValue
                ? timeRecordId.Value
                : 0;
            newRow.AcceptChanges();
        }

        private static long? GetTimeRecordId(long timeSpentAtWorkId, DateTime workDayTimeStart, int workOperationID)
        {
            long? timeRecordId = null;

            const string sqlComamndText = @"SELECT WorkersTimeTrackingID FROM FAIITimeTracking.WorkersTimeTracking 
                                            WHERE TimeSpentAtWorkID = @TimeSpentAtWorkID AND WorkDayTimeStart = @WorkDayTimeStart AND WorkOperationID = @WorkOperationID 
                                            ORDER BY WorkersTimeTrackingID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlComamndText, sqlConn);
                sqlCommand.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value = timeSpentAtWorkId;
                sqlCommand.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value = workDayTimeStart;
                sqlCommand.Parameters.Add("@WorkOperationID", MySqlDbType.Int64).Value = workOperationID;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        timeRecordId = Convert.ToInt64(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
            }

            return timeRecordId;
        }

        public void EditTimeRecord(long workerTimeTrackingId, TimeSpan timeStart, TimeSpan timeEnd, decimal workScope, double vclp)
        {
            var workerTimeTrackingRows = _timeTrackingDataTable.Select(string.Format("WorkersTimeTrackingID = {0}", workerTimeTrackingId));
            if (!workerTimeTrackingRows.Any()) return;

            var workerTimeTrackingRow = workerTimeTrackingRows.First();
            workerTimeTrackingRow["TimeStart"] = timeStart;
            workerTimeTrackingRow["TimeEnd"] = timeEnd;
            workerTimeTrackingRow["WorkScope"] = workScope;
            workerTimeTrackingRow["VCLP"] = vclp;

            UpdateTimeTracking();
        }

        public void DeleteRecord(DataRow timeTrackingDataRow)
        {
            if (timeTrackingDataRow == null) return;

            MessageBoxResult result = MessageBox.Show("Удалить текущую запись?", "Удаление",
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                timeTrackingDataRow["DeleteRecord"] = true;
                UpdateTimeTracking();
            }
        }

        public void DeleteWorkerShift(long timeSpentAtWorkId)
        {
            foreach(var timeTrackingRow in _timeTrackingDataTable.AsEnumerable().Where(r => r.Field<Int64>("TimeSpentAtWorkID") == timeSpentAtWorkId))
            {
                timeTrackingRow.Delete();
            }
            UpdateTimeTracking();

            var rows = _shiftsDataTable.Select(string.Format("TimeSpentAtWorkID = {0}", timeSpentAtWorkId));
            if (rows.Length == 0) return;

            var shift = rows.First();
            shift.Delete();
            UpdateTimeSpentAtWork();
        }

        public void AddWorkerShift(long workerId, DateTime dayStart, DateTime dayEnd)
        {
            int shiftNumber = dayStart.Hour >= 13
                ? 2 : 1;

            var newShift = _shiftsDataTable.NewRow();
            newShift["WorkerID"] = workerId;
            newShift["Date"] = dayStart;
            newShift["WorkDayTimeStart"] = dayStart;
            newShift["WorkDayTimeEnd"] = dayEnd;
            newShift["ShiftNumber"] = shiftNumber;
            newShift["DayEnd"] = true;

            _shiftsDataTable.Rows.Add(newShift);
            UpdateTimeSpentAtWork();

            var timeSpentAtWorkId = GetShiftId(workerId, dayStart);
            newShift["TimeSpentAtWorkID"] = timeSpentAtWorkId.HasValue
                ? timeSpentAtWorkId.Value : -1;
            newShift.AcceptChanges();
        }

        private long? GetShiftId(long workerId, DateTime dayStart)
        {
            long? timeSpentAtWorkId = null;
            const string sqlCommandText = @"SELECT TimeSpentAtWorkID 
                                            FROM FAIITimeTracking.TimeSpentAtWork 
                                            WHERE WorkerID = @WorkerID AND WorkDayTimeStart = @WorkDayTimeStart 
                                            ORDER BY TimeSpentAtWorkID DESC LIMIT 1";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value = dayStart;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                    {
                        timeSpentAtWorkId = Convert.ToInt64(sqlResult);
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    sqlConn.Close();
                }
            }

            return timeSpentAtWorkId;
        }

        private void UpdateTimeTracking()
        {
            try
            {
                _timeTrackingDataAdapter.Update(_timeTrackingDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[tcc00010] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
