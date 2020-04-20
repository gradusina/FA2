using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class TechnologyProblemClass
    {
        private readonly string _connectionString;

        private DataTable _technologyProblemsTable;
        private MySqlDataAdapter _technologyProblemsAdapter;

        public DataTable TechnologyProblemsTable
        {
            get
            {
                if (_technologyProblemsTable.Columns.Count == 0)
                {
                    FillTechnologyProblems();
                }

                return _technologyProblemsTable;
            }
        }

        private DataTable _technologyProblemsNotesTable;
        private MySqlDataAdapter _technologyProblemsNotesAdapter;

        public DataTable TechnologyProblemsNotesTable
        {
            get
            {
                if (_technologyProblemsNotesTable.Columns.Count == 0)
                {
                    FillTechnologyProblemsNotes();
                }

                return _technologyProblemsNotesTable;
            }
        }

        private DataTable _technologyProblemNotesResponsibilitiesTable;
        private MySqlDataAdapter _technologyProblemNotesResponsibilitiesAdapter;

        public DataTable TechnologyProblemNotesResponsibilitiesTable
        {
            get
            {
                if (_technologyProblemNotesResponsibilitiesTable.Columns.Count == 0)
                {
                    FillTechnologyProblemNotesResponsibilities();
                }

                return _technologyProblemNotesResponsibilitiesTable;
            }
        }


        public TechnologyProblemClass(string connectionString)
        {
            _connectionString = connectionString;
            Create();
        }

        private void Create()
        {
            _technologyProblemsTable = new DataTable();
            _technologyProblemsNotesTable = new DataTable();
            _technologyProblemNotesResponsibilitiesTable = new DataTable();
        }


        #region Fillings

        public void Fill(DateTime dateFrom, DateTime dateTo)
        {
            FillTechnologyProblems(dateFrom, dateTo);

            var techProblemIds = GetTechnologyProblemIds();
            var technologyProblemIds = techProblemIds as IList<long> ?? techProblemIds.ToList();
            FillTechnologyProblemsNotes(technologyProblemIds);
            FillTechnologyProblemNotesResponsibilities(technologyProblemIds);
        }

        private void FillTechnologyProblems()
        {
            const string commandText =
                @"SELECT TechnologyProblemID, GlobalID, FactoryID, WorkUnitID, WorkSectionID, 
                  RequestDate, RequestWorkerID, RequestNotes, ReceivedDate, ReceivedWorkerID, 
                  ReceivedNotes, CompletionDate, CompletionWorkerID, CompletionNotes, 
                  PlanedCompletionDate, EditingDate, EditingWorkerID, RequestClose, Enable 
                  FROM FAIITechnologyProblem.TechnologyProblems 
                  WHERE Enable = True 
                  LIMIT 0";

            _technologyProblemsAdapter = new MySqlDataAdapter(commandText, _connectionString);
            new MySqlCommandBuilder(_technologyProblemsAdapter);

            try
            {
                _technologyProblemsTable.Clear();
                _technologyProblemsAdapter.Fill(_technologyProblemsTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TP0001] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTechnologyProblems(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText =
                @"SELECT TechnologyProblemID, GlobalID, FactoryID, WorkUnitID, WorkSectionID, 
                  RequestDate, RequestWorkerID, RequestNotes, ReceivedDate, ReceivedWorkerID, 
                  ReceivedNotes, CompletionDate, CompletionWorkerID, CompletionNotes, 
                  PlanedCompletionDate, EditingDate, EditingWorkerID, RequestClose, Enable 
                  FROM FAIITechnologyProblem.TechnologyProblems WHERE 
                  RequestDate >= @DateFrom AND RequestDate < @DateTo AND Enable = True 
                  ORDER BY RequestClose, RequestDate";

            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(commandText, sqlConn);
                command.Parameters.Add("@DateFrom", MySqlDbType.DateTime).Value = dateFrom;
                command.Parameters.Add("@DateTo", MySqlDbType.DateTime).Value = dateTo.AddDays(1);

                _technologyProblemsAdapter = new MySqlDataAdapter(command);
                new MySqlCommandBuilder(_technologyProblemsAdapter);

                try
                {
                    _technologyProblemsTable.Clear();
                    _technologyProblemsAdapter.Fill(_technologyProblemsTable);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        exp.Message +
                        "\n\n[TP0002] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FillTechnologyProblemsNotes()
        {
            const string sqlCommandText = @"SELECT TechnologyProblemNoteID, TechnologyProblemID, 
                                            TechnologyProblemNoteText, TechnologyProblemNoteDate 
                                            FROM FAIITechnologyProblem.TechnologyProblemsNotes 
                                            LIMIT 0";


            _technologyProblemsNotesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_technologyProblemsNotesAdapter);
            try
            {
                _technologyProblemsNotesTable.Clear();
                _technologyProblemsNotesAdapter.Fill(_technologyProblemsNotesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TP0003] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTechnologyProblemsNotes(IEnumerable<long> technologyProblemIds)
        {
            var ids = technologyProblemIds as IList<long> ?? technologyProblemIds.ToList();

            var technologyProblemIdsString = string.Empty;
            //If returned technology problem ids is empty, set filter to -1
            technologyProblemIdsString = ids.Count() != 0
                ? (ids.Cast<object>()
                    .Aggregate(technologyProblemIdsString,
                        (current, technologyProblemId) => current + ", " + technologyProblemId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText = @"SELECT TechnologyProblemNoteID, TechnologyProblemID, 
                                   TechnologyProblemNoteText, TechnologyProblemNoteDate 
                                   FROM FAIITechnologyProblem.TechnologyProblemsNotes 
                                   WHERE TechnologyProblemID IN (" + technologyProblemIdsString + ")";


            _technologyProblemsNotesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_technologyProblemsNotesAdapter);
            try
            {
                _technologyProblemsNotesTable.Clear();
                _technologyProblemsNotesAdapter.Fill(_technologyProblemsNotesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TP0004] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTechnologyProblemNotesResponsibilities()
        {
            const string sqlCommandText = @"SELECT TechnologyProblemNotesResponsibilityID, 
                                            TechnologyProblemNoteID, TechnologyProblemID, WorkerID 
                                            FROM FAIITechnologyProblem.TechnologyProblemNotesResponsibilities 
                                            LIMIT 0";


            _technologyProblemNotesResponsibilitiesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_technologyProblemNotesResponsibilitiesAdapter);
            try
            {
                _technologyProblemNotesResponsibilitiesTable.Clear();
                _technologyProblemNotesResponsibilitiesAdapter.Fill(_technologyProblemNotesResponsibilitiesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TP0005] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTechnologyProblemNotesResponsibilities(IEnumerable<long> technologyProblemIds)
        {
            var ids = technologyProblemIds as IList<long> ?? technologyProblemIds.ToList();

            var technologyProblemIdsString = string.Empty;
            //If returned technology problem ids is empty, set filter to -1
            technologyProblemIdsString = ids.Count() != 0
                ? (ids.Cast<object>()
                    .Aggregate(technologyProblemIdsString,
                        (current, technologyProblemId) => current + ", " + technologyProblemId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText = @"SELECT TechnologyProblemNotesResponsibilityID, 
                                   TechnologyProblemNoteID, TechnologyProblemID, WorkerID 
                                   FROM FAIITechnologyProblem.TechnologyProblemNotesResponsibilities 
                                   WHERE TechnologyProblemID IN (" + technologyProblemIdsString + ")";


            _technologyProblemNotesResponsibilitiesAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_technologyProblemNotesResponsibilitiesAdapter);
            try
            {
                _technologyProblemNotesResponsibilitiesTable.Clear();
                _technologyProblemNotesResponsibilitiesAdapter.Fill(_technologyProblemNotesResponsibilitiesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message +
                                "\n\n[TP0006] Попробуйте перезапустить приложение. " +
                                "В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region Updates

        private void UpdateTechnologyProblems()
        {
            try
            {
                _technologyProblemsAdapter.Update(TechnologyProblemsTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TP0007] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTechnologyProblemNotes()
        {
            try
            {
                _technologyProblemsNotesAdapter.Update(TechnologyProblemsNotesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TP0008] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTechnologyProblemNoteResponsibilities()
        {
            try
            {
                _technologyProblemNotesResponsibilitiesAdapter.
                    Update(TechnologyProblemNotesResponsibilitiesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[TP0009] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region Technology problems

        private IEnumerable<long> GetTechnologyProblemIds()
        {
            var ids = from technologyProblemsView in TechnologyProblemsTable.AsEnumerable()
                      select technologyProblemsView.Field<Int64>("TechnologyProblemID");
            return ids.ToList();
        }

        public int AddNewRequest(int factoryId, int workUnitId, int workSectionId, DateTime requestDate,
            int workerId, string requestNote)
        {
            var newDr = TechnologyProblemsTable.NewRow();
            newDr["GlobalID"] = "0200000";
            newDr["FactoryID"] = factoryId;
            newDr["WorkUnitID"] = workUnitId;
            newDr["WorkSectionID"] = workSectionId;
            newDr["RequestDate"] = requestDate;
            newDr["RequestWorkerID"] = workerId;
            newDr["RequestNotes"] = requestNote;
            newDr["RequestClose"] = false;
            newDr["Enable"] = true;
            TechnologyProblemsTable.Rows.Add(newDr);

            UpdateTechnologyProblems();

            var techProblemId = GetTechnologyProblemId(workSectionId, workerId, requestDate);
            if (techProblemId != -1)
            {
                newDr["TechnologyProblemID"] = techProblemId;
                newDr.AcceptChanges();

                newDr["GlobalID"] = "02" + techProblemId.ToString("00000");
                UpdateTechnologyProblems();
            }

            return techProblemId;
        }

        private int GetTechnologyProblemId(int workSectionId, int requestWorkerId, DateTime requestDate)
        {
            var techProblemId = -1;

            const string sqlCommandText = @"SELECT TechnologyProblemID 
                                            FROM FAIITechnologyProblem.TechnologyProblems
                                            WHERE WorkSectionID = @WorkSectionID 
                                            AND RequestWorkerID = @RequestWorkerID 
                                            AND RequestDate = @RequestDate";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@WorkSectionID", MySqlDbType.Int64).Value = workSectionId;
                    sqlCommand.Parameters.Add("@RequestWorkerID", MySqlDbType.Int64).Value = requestWorkerId;
                    sqlCommand.Parameters.Add("@RequestDate", MySqlDbType.DateTime).Value = requestDate;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            techProblemId = Convert.ToInt32(result);
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
            }

            return techProblemId;
        }

        public void FillReceivedInfo(int techProblemId, DateTime receivedDate, int receivedWorkerId, string receivedNote)
        {
            var rows = TechnologyProblemsTable.Select("TechnologyProblemID = " + techProblemId);
            if (!rows.Any()) return;

            var receivedRow = rows.First();
            receivedRow["ReceivedDate"] = receivedDate;
            receivedRow["ReceivedWorkerID"] = receivedWorkerId;
            receivedRow["ReceivedNotes"] = receivedNote;
            UpdateTechnologyProblems();
        }

        public void CompleteRequest(int techProblemId, DateTime completionDate, int completionWorkerId,
            string completionNote)
        {
            var rows = TechnologyProblemsTable.Select("TechnologyProblemID = " + techProblemId);
            if (!rows.Any()) return;

            var completionRow = rows.First();
            completionRow["CompletionDate"] = completionDate;
            completionRow["CompletionWorkerID"] = completionWorkerId;
            completionRow["CompletionNotes"] = completionNote;
            completionRow["RequestClose"] = true;

            UpdateTechnologyProblems();
        }

        public void FillCompletionInfo(string globalId, DateTime completionDate, int completionWorkerId)
        {
            var rows = TechnologyProblemsTable.Select("GlobalID = " + globalId);
            if (!rows.Any())
            {
                CompleteRequest(globalId, completionDate, completionWorkerId);
                return;
            }

            var completionRow = rows.First();
            completionRow["CompletionDate"] = completionDate;
            completionRow["CompletionWorkerID"] = completionWorkerId;

            UpdateTechnologyProblems();
        }

        private static void CompleteRequest(string globalId, DateTime completionDate, int completionWorkerId)
        {
            const string sqlCommandText = @"UPDATE FAIITechnologyProblem.TechnologyProblems 
                                            SET CompletionDate = @CompletionDate, 
                                                CompletionWorkerID = @CompletionWorkerID 
                                            WHERE GlobalID = @GlobalID";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, conn);
                sqlCommand.Parameters.Add("@CompletionDate", MySqlDbType.DateTime).Value = completionDate;
                sqlCommand.Parameters.Add("@CompletionWorkerID", MySqlDbType.Int64).Value = completionWorkerId;
                sqlCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = globalId;

                try
                {
                    conn.Open();
                    sqlCommand.ExecuteScalar();
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public void FillPlannedCompletionDate(int techProblemId, object plannedCompleteDate, DateTime editingDate,
            int editingWorkerId)
        {
            var rows = TechnologyProblemsTable.Select("TechnologyProblemID = " + techProblemId);
            if (!rows.Any()) return;

            var additionalRow = rows.First();
            additionalRow["PlanedCompletionDate"] = plannedCompleteDate;
            additionalRow["EditingDate"] = editingDate;
            additionalRow["EditingWorkerID"] = editingWorkerId;
            UpdateTechnologyProblems();
        }

        public void DeleteTechnologyProblem(int techProblemId)
        {
            var rows = TechnologyProblemsTable.Select("TechnologyProblemID = " + techProblemId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow["Enable"] = false;

            UpdateTechnologyProblems();
        }

        public void ChangeReceivedNotes(int techProblemId, string receivedNotes)
        {
            var rows = TechnologyProblemsTable.Select("TechnologyProblemID = " + techProblemId);
            if (!rows.Any()) return;

            var techProblemRow = rows.First();
            techProblemRow["ReceivedNotes"] = receivedNotes;

            UpdateTechnologyProblems();
        }

        #endregion


        #region Technology problem notes

        public int AddNewTechnologyProblemNote(int techProblemId, string techProblemNoteText, DateTime techProblemNoteDate)
        {
            var newDr = TechnologyProblemsNotesTable.NewRow();
            newDr["TechnologyProblemID"] = techProblemId;
            newDr["TechnologyProblemNoteText"] = techProblemNoteText;
            newDr["TechnologyProblemNoteDate"] = techProblemNoteDate;
            TechnologyProblemsNotesTable.Rows.Add(newDr);

            UpdateTechnologyProblemNotes();

            var techProblemNoteId = GetTechnologyProblemNoteId(techProblemId, techProblemNoteDate);
            if (techProblemNoteId != -1)
            {
                newDr["TechnologyProblemNoteID"] = techProblemNoteId;
                newDr.AcceptChanges();
            }

            return techProblemNoteId;
        }

        private int GetTechnologyProblemNoteId(int techProblemId, DateTime techProblemNoteDate)
        {
            var techProblemNoteId = -1;

            const string sqlCommandText = @"SELECT TechnologyProblemNoteID 
                                            FROM FAIITechnologyProblem.TechnologyProblemsNotes
                                            WHERE TechnologyProblemID = @TechnologyProblemID 
                                            AND TechnologyProblemNoteDate = @TechnologyProblemNoteDate";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@TechnologyProblemID", MySqlDbType.Int64).Value = techProblemId;
                    sqlCommand.Parameters.Add("@TechnologyProblemNoteDate", MySqlDbType.DateTime).Value =
                        techProblemNoteDate;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            techProblemNoteId = Convert.ToInt32(result);
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
            }

            return techProblemNoteId;
        }

        public void DeleteTechnologyProblemNote(int techProblemNoteId)
        {
            DeleteTechnologyProblemNoteResponsibilities(techProblemNoteId);

            var rows =
                TechnologyProblemsNotesTable.Select(string.Format("TechnologyProblemNoteID = {0}", techProblemNoteId));
            if(!rows.Any()) return;

            var techProblemNote = rows.First();
            techProblemNote.Delete();

            UpdateTechnologyProblemNotes();
        }

        #endregion


        #region Technology problem note responsibilities

        public void AddNewTechnologyProblemNoteResponsible(int technologyProblemNoteId, int technologyProblemId, int workerId)
        {
            var newDr = TechnologyProblemNotesResponsibilitiesTable.NewRow();
            newDr["TechnologyProblemNoteID"] = technologyProblemNoteId;
            newDr["TechnologyProblemID"] = technologyProblemId;
            newDr["WorkerID"] = workerId;
            TechnologyProblemNotesResponsibilitiesTable.Rows.Add(newDr);

            UpdateTechnologyProblemNoteResponsibilities();

            var techProblemNoteResponsibleId = GetTechnologyProblemNoteResponsibleId(technologyProblemNoteId, workerId);
            if (techProblemNoteResponsibleId != -1)
            {
                newDr["TechnologyProblemNotesResponsibilityID"] = techProblemNoteResponsibleId;
                newDr.AcceptChanges();
            }
        }

        private int GetTechnologyProblemNoteResponsibleId(int technologyProblemNoteId, int workerId)
        {
            var techProblemNoteResponsibilityId = -1;

            const string sqlCommandText = @"SELECT TechnologyProblemNotesResponsibilityID 
                                            FROM FAIITechnologyProblem.TechnologyProblemNotesResponsibilities
                                            WHERE TechnologyProblemNoteID = @TechnologyProblemNoteID 
                                            AND WorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@TechnologyProblemNoteID", MySqlDbType.Int64).Value =
                        technologyProblemNoteId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            techProblemNoteResponsibilityId = Convert.ToInt32(result);
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
            }

            return techProblemNoteResponsibilityId;
        }

        private void DeleteTechnologyProblemNoteResponsibilities(int techProblemNoteId)
        {
            foreach (
                var techProblemNoteRespons in
                    TechnologyProblemNotesResponsibilitiesTable.AsEnumerable()
                        .Where(r => r.Field<Int64>("TechnologyProblemNoteID") == techProblemNoteId))
            {
                techProblemNoteRespons.Delete();
            }

            UpdateTechnologyProblemNoteResponsibilities();
        }

        #endregion

    }
}
