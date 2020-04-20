using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using FA2.Converters;
using MySql.Data.MySqlClient;
using System.Data;

namespace FA2.Classes
{
    public class TimeTrackingClass
    {
        public DateTime CurrentWorkerStartDate = new DateTime();
        public int CurrentTimeSpentAtWorkID = 0;
        private IdToNameConverter _idToNameConverter = new IdToNameConverter();

        public struct ButtonVisStruct
        {
            public bool StartWorkingDayButtonVis;
            public bool DinnerButtonVis;
            public bool EndWorkingDayButtonVis;
            public bool EndDinnerButtonVis;
        }

        public ButtonVisStruct WorkingDayButtonsVis = new ButtonVisStruct {StartWorkingDayButtonVis = true};

        public bool EnableDinnerButton = true;
        public bool StartWorkinDayTimeTimer = true;

        private MySqlDataAdapter _timeTrackingDataAdapter;
        private DataTable _timeTrackingDataTable;

        #region TimeTracking
        
        public void StartWorkingDay()
        {
            int shiftNumber = 1;

            CurrentWorkerStartDate = App.BaseClass.GetDateFromSqlServer();

            if (CurrentWorkerStartDate.Hour >= 13)
                shiftNumber = 2;

            const string commandText = @"INSERT INTO FAIITimeTracking.TimeSpentAtWork
                                            (WorkerID,
                                            Date,
                                            WorkDayTimeStart,
                                            ShiftNumber,
                                            DayEnd, DayStageID) VALUES 
                                            (@WorkerID, @Date, @WorkDayTimeStart, @ShiftNumber, @DayEnd, @DayStageID)";

            const string getIdCommand = @"SELECT TimeSpentAtWorkID FROM FAIITimeTracking.TimeSpentAtWork WHERE Date = @Date AND WorkerID = @WorkerID";


            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    using (var command = new MySqlCommand(commandText, sqlConn))
                    {
                        command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                            AdministrationClass.CurrentWorkerId;
                        command.Parameters.Add("@Date", MySqlDbType.DateTime).Value = CurrentWorkerStartDate;
                        command.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value = CurrentWorkerStartDate;
                        command.Parameters.Add("@ShiftNumber", MySqlDbType.Int64).Value = shiftNumber;
                        command.Parameters.Add("@DayEnd", MySqlDbType.Bit).Value = false;
                        command.Parameters.Add("@DayStageID", MySqlDbType.Int16).Value = 2;
                        
                        command.ExecuteScalar();
                    }
                    
                    using (var command = new MySqlCommand(getIdCommand, sqlConn))
                    {
                        command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                            AdministrationClass.CurrentWorkerId;
                        command.Parameters.Add("@Date", MySqlDbType.DateTime).Value = CurrentWorkerStartDate;

                        int.TryParse(command.ExecuteScalar().ToString(), out CurrentTimeSpentAtWorkID);
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            SetWorkingDayButtonsVis(2);
            EnableDinnerButton = true;
        }
        
        public void EndWorkingDay()
        {
            const string endWorkingDayCommandText = "UPDATE FAIITimeTracking.TimeSpentAtWork SET WorkDayTimeEnd = @WorkDayTimeEnd, DayEnd = True, DayStageID = 1 WHERE TimeSpentAtWorkID = @TimeSpentAtWorkID";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    using (var command = new MySqlCommand(endWorkingDayCommandText, sqlConn))
                    {
                        command.Parameters.Add("@WorkDayTimeEnd", MySqlDbType.DateTime).Value = App.BaseClass.GetDateFromSqlServer();

                        command.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value = CurrentTimeSpentAtWorkID;

                        command.ExecuteScalar();
                    }
                    
                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }
            CurrentWorkerStartDate = new DateTime();
            CurrentTimeSpentAtWorkID = -1;
            
            EnableDinnerButton = true;
            SetWorkingDayButtonsVis(1);
        }

        public void StartDinner()
        {
            const string endWorkingDayCommandText = "UPDATE FAIITimeTracking.TimeSpentAtWork SET DinnerTimeStart = @DinnerTimeStart, DayStageID = 3  WHERE TimeSpentAtWorkID = @TimeSpentAtWorkID";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    using (var command = new MySqlCommand(endWorkingDayCommandText, sqlConn))
                    {
                        command.Parameters.Add("@DinnerTimeStart", MySqlDbType.DateTime).Value = App.BaseClass.GetDateFromSqlServer();
                        command.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value = CurrentTimeSpentAtWorkID;
                        command.ExecuteScalar();
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            EnableDinnerButton = false;

            SetWorkingDayButtonsVis(3);
        }

        public void EndDinner()
        {
            const string endWorkingDayCommandText =
                "UPDATE FAIITimeTracking.TimeSpentAtWork SET DinnerTimeEnd = @DinnerTimeEnd, DayStageID = 4  WHERE TimeSpentAtWorkID = @TimeSpentAtWorkID";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                try
                {
                    sqlConn.Open();

                    using (var command = new MySqlCommand(endWorkingDayCommandText, sqlConn))
                    {
                        command.Parameters.Add("@DinnerTimeEnd", MySqlDbType.DateTime).Value =
                            App.BaseClass.GetDateFromSqlServer();
                        command.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value = CurrentTimeSpentAtWorkID;
                        command.ExecuteScalar();
                    }

                    sqlConn.Close();
                }
                catch
                {
                    sqlConn.Close();
                }
            }

            SetWorkingDayButtonsVis(2);

            EnableDinnerButton = false;
        }

        /// <summary>
        /// LengthParametr:
        /// Show_EndWorkingDay/StartDinner  -- 2
        /// Show_EndDinner  -- 3
        /// Show_StartWorkingDay  -- 1
        /// </summary>
        /// <param name="lengthParametr"></param>
        /// <returns></returns>
        private void SetWorkingDayButtonsVis(int lengthParametr)
        {
            switch (lengthParametr)
            {
                case 1:
                    WorkingDayButtonsVis.StartWorkingDayButtonVis = true;
                    WorkingDayButtonsVis.EndWorkingDayButtonVis = false;
                    WorkingDayButtonsVis.DinnerButtonVis = false;
                    WorkingDayButtonsVis.EndDinnerButtonVis = false;
                    break;
                case 2:
                    WorkingDayButtonsVis.StartWorkingDayButtonVis = false;
                    WorkingDayButtonsVis.EndWorkingDayButtonVis = true;
                    WorkingDayButtonsVis.DinnerButtonVis = true;
                    WorkingDayButtonsVis.EndDinnerButtonVis = false;
                    break;
                case 3:
                    WorkingDayButtonsVis.StartWorkingDayButtonVis = false;
                    WorkingDayButtonsVis.EndWorkingDayButtonVis = false;
                    WorkingDayButtonsVis.DinnerButtonVis = false;
                    WorkingDayButtonsVis.EndDinnerButtonVis = true;
                    break;
            }
        }

        public TimeSpan CurrentWorkerTimeAtWork()
        {
            var result = new TimeSpan(-1, 0, 0);

            const string getRowCommand =
                @"SELECT TimeSpentAtWorkID, WorkDayTimeStart, WorkDayTimeEnd, DinnerTimeStart, DinnerTimeEnd 
                            FROM 
                            ( SELECT * FROM FAIITimeTracking.TimeSpentAtWork WHERE WorkerID = @WorkerID ORDER BY  TimeSpentAtWorkID DESC LIMIT 1) as resTabl 
                            WHERE DayEnd = False";

            var timeSpentAtWorkCollection = new List<object>();

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                sqlConn.Open();

                using (var command = new MySqlCommand(getRowCommand, sqlConn))
                {
                    command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                        AdministrationClass.CurrentWorkerId;

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            timeSpentAtWorkCollection.Add(reader[i]);
                        }
                        break;
                    }
                }

                sqlConn.Close();
            }

            if (timeSpentAtWorkCollection.Count == 0) return result;

            //TimeSpentAtWorkID [0], WorkDayTimeStart [1], WorkDayTimeEnd [2], DinnerTimeStart [3], DinnerTimeEnd [4] 

            if (timeSpentAtWorkCollection[1] == DBNull.Value) return result;

            CurrentWorkerStartDate = (DateTime) timeSpentAtWorkCollection[1];

            if ((timeSpentAtWorkCollection[3] != DBNull.Value) &&
                (timeSpentAtWorkCollection[4] != DBNull.Value))
            {
                TimeSpan dinnerTime =
                    ((DateTime) timeSpentAtWorkCollection[4]).Subtract(
                        (DateTime) timeSpentAtWorkCollection[3]);

                return
                    (App.BaseClass.GetDateFromSqlServer().Subtract(CurrentWorkerStartDate)).Subtract(dinnerTime)
                        .Duration();
            }

            if ((timeSpentAtWorkCollection[3] != DBNull.Value) &&
                (timeSpentAtWorkCollection[4] == DBNull.Value))
            {
                DateTime curDateTime = App.BaseClass.GetDateFromSqlServer();

                TimeSpan dinnerTime =
                    (curDateTime).Subtract(
                        (DateTime) timeSpentAtWorkCollection[3]);

                return
                    (curDateTime.Subtract(CurrentWorkerStartDate)).Subtract(dinnerTime)
                        .Duration();
            }

            return (App.BaseClass.GetDateFromSqlServer().Subtract(CurrentWorkerStartDate)).Duration();
        }

        public void GetWorkingDayStage()
        {
            int dayStageID;

            DateTime t;
            GetShiftParams(AdministrationClass.CurrentWorkerId, out dayStageID, out CurrentTimeSpentAtWorkID, out t);
            
            switch (dayStageID)
            {
                case 1:
                    SetWorkingDayButtonsVis(1);
                    break;
                
                case 2:
                    SetWorkingDayButtonsVis(2);
                    break;

                case 3:
                    SetWorkingDayButtonsVis(3);
                    StartWorkinDayTimeTimer = false;
                    break;

                case 4:
                    SetWorkingDayButtonsVis(2);
                    EnableDinnerButton = false;
                    break;
            }
        }


        public bool GetShiftParams(int workerId, out int dayStageID, out int timeSpentAtWorkID,
            out DateTime workDayTimeStart)
        {
            bool isDayEnd = true;

            const string getIdCommand =
                @"SELECT TimeSpentAtWorkID, DayStageID, WorkDayTimeStart, DayEnd FROM FAIITimeTracking.TimeSpentAtWork WHERE WorkerID = @WorkerID ORDER BY WorkDayTimeStart DESC LIMIT 1";

            var collection = new List<object>();

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                sqlConn.Open();

                using (var command = new MySqlCommand(getIdCommand, sqlConn))
                {
                    command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            collection.Add(reader[i]);
                        }
                        break;
                    }
                }

                sqlConn.Close();
            }

            if (collection.Count == 4)
            {
                int.TryParse(collection[0].ToString(), out timeSpentAtWorkID);
                int.TryParse(collection[1].ToString(), out dayStageID);
                DateTime.TryParse(collection[2].ToString(), out workDayTimeStart);
                bool.TryParse(collection[3].ToString(), out isDayEnd);
            }
            else
            {
                timeSpentAtWorkID = -1;
                dayStageID = -1;
                workDayTimeStart = new DateTime();
            }

            return isDayEnd;
        }

        public bool GetIsDayEnd(int workerId)
        {
            bool isDayEnd;

            const string getIdCommand =
                @"SELECT DayEnd FROM FAIITimeTracking.TimeSpentAtWork WHERE WorkerID = @WorkerID ORDER BY WorkDayTimeStart DESC LIMIT 1";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                sqlConn.Open();

                using (var command = new MySqlCommand(getIdCommand, sqlConn))
                {
                    command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                    bool.TryParse(command.ExecuteScalar().ToString(), out isDayEnd);
                }

                sqlConn.Close();
            }


            return isDayEnd;
        }

        #endregion


        #region TimeTracking_Page

        private void FillTimeForUserAndDate(int workerID, DateTime workerStartDate)
        {
            try
            {
                _timeTrackingDataTable.Clear();

                var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString);

                var dt = Convert.ToDateTime(workerStartDate);

                if (dt.Year == 1) dt = dt.AddYears(2000);

                const string commandText = "SELECT WorkersTimeTrackingID, TimeSpentAtWorkID, WorkDayTimeStart," +
                                           "WorkerID, WorkerNotes, WorkerGroupID, FactoryID, WorkUnitID, WorkSectionID," +
                                           "WorkSubsectionID, WorkOperationID, TimeStart, TimeEnd, DeleteRecord, MentorID, WorkStatusID, WorkScope, VCLP " +
                                           "FROM FAIITimeTracking.WorkersTimeTracking " +
                                           "WHERE DeleteRecord<>True AND WorkerID= @CurrentWorkerId AND WorkDayTimeStart= @WorkDayTimeStart";
                
                var command = new MySqlCommand(commandText, sqlConn);

                command.Parameters.Add("@CurrentWorkerId", MySqlDbType.Int64).Value = workerID;
                command.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value = dt;

                _timeTrackingDataAdapter = new MySqlDataAdapter(command);

// ReSharper disable once ObjectCreationAsStatement
                new MySqlCommandBuilder(_timeTrackingDataAdapter);

                _timeTrackingDataAdapter.Fill(_timeTrackingDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [TTC0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public DataView GetTimeTracking(int workerID, DateTime workerStartDate)
        {
            if (_timeTrackingDataTable == null) _timeTrackingDataTable = new DataTable();
            _timeTrackingDataTable.Clear();

            FillTimeForUserAndDate(workerID, workerStartDate);

            var dv = _timeTrackingDataTable.AsDataView();

            dv.RowFilter = "DeleteRecord = 'False'";

            return dv;
        }



        public void AddNewTimeRecord(int workerGroupID, int factoryID, int workUnitID, int workSectionID,
            int workSubsectionID, int workOperationID, int operationGroupID, int operationTypeID, TimeSpan timeStart, TimeSpan timeEnd, decimal workScope,
            double vclp,
            string workerNotes = null, int workStatusID = 1, int workerID = -1, int mentorID = -1)
        {
            const string commandText = @"INSERT INTO FAIITimeTracking.WorkersTimeTracking
                                            (TimeSpentAtWorkID, WorkDayTimeStart, WorkerID, WorkScope, VCLP, WorkerNotes, WorkUnitID,
                                            WorkerGroupID, FactoryID, WorkSectionID, WorkSubsectionID, WorkOperationID, TimeStart,
                                            TimeEnd, DeleteRecord, MentorID, WorkStatusID, OperationGroupID, OperationTypeID) 
                                            VALUES 
                                            (@TimeSpentAtWorkID, @WorkDayTimeStart, @WorkerID, @WorkScope, @VCLP, @WorkerNotes,
                                            @WorkUnitID, @WorkerGroupID, @FactoryID, @WorkSectionID, @WorkSubsectionID,  @WorkOperationID,
                                            @TimeStart, @TimeEnd, @DeleteRecord, @MentorID, @WorkStatusID, @OperationGroupID, @OperationTypeID)";

            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                sqlConn.Open();

                using (var command = new MySqlCommand(commandText, sqlConn))
                {
                    if (workerID == -1)
                    {
                        command.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value =
                            CurrentTimeSpentAtWorkID;
                        command.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value =
                            CurrentWorkerStartDate;
                        command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                            AdministrationClass.CurrentWorkerId;
                    }
                    else
                    {
                        int timeSpentAtWorkID;
                        int dayStageID;
                        DateTime workDayTimeStart;

                        if (GetShiftParams(workerID, out dayStageID, out timeSpentAtWorkID, out workDayTimeStart))
                        {
                            MessageBox.Show(
                                _idToNameConverter.Convert(workerID, "ShortName") +
                                " не начал(а) рабочий день!\nЗапись не будет добавлена!", "Информация",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            return;
                        }

                        command.Parameters.Add("@TimeSpentAtWorkID", MySqlDbType.Int64).Value =
                            timeSpentAtWorkID;
                        command.Parameters.Add("@WorkDayTimeStart", MySqlDbType.DateTime).Value =
                            workDayTimeStart;
                        command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerID;
                    }

                    command.Parameters.Add("@WorkScope", MySqlDbType.Decimal).Value = workScope;
                    command.Parameters.Add("@VCLP", MySqlDbType.Decimal).Value = vclp;

                    command.Parameters.Add("@WorkerNotes", MySqlDbType.LongText).Value = workerNotes;
                    command.Parameters.Add("@WorkUnitID", MySqlDbType.Int64).Value = workUnitID;
                    command.Parameters.Add("@WorkerGroupID", MySqlDbType.Int64).Value = workerGroupID;

                    command.Parameters.Add("@FactoryID", MySqlDbType.Int64).Value = factoryID;
                    command.Parameters.Add("@WorkSectionID", MySqlDbType.Int64).Value = workSectionID;
                    command.Parameters.Add("@WorkSubsectionID", MySqlDbType.Int64).Value = workSubsectionID;

                    command.Parameters.Add("@WorkOperationID", MySqlDbType.Int64).Value = workOperationID;
                    command.Parameters.Add("@TimeStart", MySqlDbType.Time).Value = timeStart;
                    command.Parameters.Add("@TimeEnd", MySqlDbType.Time).Value = timeEnd;

                    command.Parameters.Add("@DeleteRecord", MySqlDbType.Bit).Value = false;
                    command.Parameters.Add("@MentorID", MySqlDbType.Int64).Value = mentorID;
                    command.Parameters.Add("@WorkStatusID", MySqlDbType.Int64).Value = workStatusID;

                    command.Parameters.Add("@OperationGroupID", MySqlDbType.Int64).Value = operationGroupID;
                    command.Parameters.Add("@OperationTypeID", MySqlDbType.Int64).Value = operationTypeID;

                    command.ExecuteScalar();
                }

                sqlConn.Close();
            }

            if (workerID == -1)
            {
                _timeTrackingDataTable.AcceptChanges();
                _timeTrackingDataTable.Clear();

                FillTimeForUserAndDate(AdministrationClass.CurrentWorkerId, CurrentWorkerStartDate);
            }
        }

        public void DeleteRecord(DataRow timeTrackingDataRow)
        {
            if (timeTrackingDataRow == null) return;

            MessageBoxResult result = MessageBox.Show("Удалить текущую запись?", "Удаление",
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                timeTrackingDataRow["DeleteRecord"] = true;
                _timeTrackingDataAdapter.Update(_timeTrackingDataTable);
            }
        }

        public string CountingTotalTime()
        {
            string result = "00:00";

            if (_timeTrackingDataTable == null) return result;

            var totalTime = new TimeSpan();

            _timeTrackingDataTable.DefaultView.RowFilter = "DeleteRecord = False";

            foreach (DataRowView drv in _timeTrackingDataTable.DefaultView)
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
            
            if (totalTime.Days == 0)
                result = string.Format("{0:hh\\:mm}", totalTime);
            else
            {
                string m = totalTime.Minutes.ToString(CultureInfo.InvariantCulture);
                if (totalTime.Minutes < 10)
                {
                    m = "0" + totalTime.Minutes;
                }

                result = (totalTime.Days * 24) + totalTime.Hours + ":" + m;
            }

            return result;
        }

        #endregion TimeTrackingPage
    }
}
