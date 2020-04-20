using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using FA2.Annotations;

namespace FA2.Classes
{
    public class NewsFeedClass
    {
        private readonly string _connectionString;

        private readonly TimeSpan _updateTimeLimit = TimeSpan.FromMinutes(30);
        private DateTime _newsStatusesLastUpdate;
        private DateTime _newsStatusGroupsLastUpdate;


        #region Data Variables


        private DataTable _news;
        private MySqlDataAdapter _newsAdapter;
        public DataTable News
        {
            get { return _news; }
            set { _news = value; }
        }


        private DataTable _comments;
        private MySqlDataAdapter _commentsAdapter;
        public DataTable Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }


        private DataTable _attachments;
        private MySqlDataAdapter _attachmentsAdapter;
        public DataTable Attachments
        {
            get { return _attachments; }
            set { _attachments = value; }
        }


        private DataTable _commentsAttachments;
        private MySqlDataAdapter _commentsAttachmentsAdapter;
        public DataTable CommentsAttachments
        {
            get { return _commentsAttachments; }
            set { _commentsAttachments = value; }
        }


        private List<NewsStatus> _newsStatuses;
        public List<NewsStatus> NewsStatuses
        {
            get
            {
                var currentTime = DateTime.Now;

                if (currentTime.Subtract(_newsStatusesLastUpdate) <= _updateTimeLimit)
                    return _newsStatuses;

                _newsStatusesLastUpdate = currentTime;
                FillNewsStatuses();

                return _newsStatuses;
            }
            private set { _newsStatuses = value; }
        }


        private List<NewsStatusGroup> _newsStatusGroups;
        public List<NewsStatusGroup> NewsStatusGroups
        {
            get
            {
                var currentTime = DateTime.Now;

                if (currentTime.Subtract(_newsStatusGroupsLastUpdate) <= _updateTimeLimit)
                    return _newsStatusGroups;

                _newsStatusGroupsLastUpdate = currentTime;
                FillNewsStatusGroups();

                return _newsStatusGroups;
            }
            private set { _newsStatusGroups = value; }
        }


        #endregion


        public NewsFeedClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;
            News = new DataTable();
            Comments = new DataTable();
            Attachments = new DataTable();
            NewsStatuses = new List<NewsStatus>();
            NewsStatusGroups = new List<NewsStatusGroup>();

            FillNewsStatuses();
            FillNewsStatusGroups();
            _newsStatusesLastUpdate = DateTime.Now;
            _newsStatusGroupsLastUpdate = DateTime.Now;
        }

        public void Fill(int indexFrom, int limiCount, NewsStatus newsStatus, int? prodStatusId)
        {
            FillNews(indexFrom, limiCount, newsStatus, prodStatusId);
            var newsIdsArray = GetNewsIds();

            FillComments(newsIdsArray);
            var commentsIdsArray = GetCommentsIds();

            FillAttachments(newsIdsArray);
            FillCommentsAttachments(commentsIdsArray);
        }

        public void Fill(int indexFrom, int limiCount, NewsStatusGroup newsStatusGroup)
        {
            FillNews(indexFrom, limiCount, newsStatusGroup);
            var newsIdsArray = GetNewsIds();

            FillComments(newsIdsArray);
            var commentsIdsArray = GetCommentsIds();

            FillAttachments(newsIdsArray);
            FillCommentsAttachments(commentsIdsArray);
        }

        public void Fill(string globalId)
        {
            FillNews(globalId);
            var newsIdsArray = GetNewsIds();

            FillComments(newsIdsArray);
            var commentsIdsArray = GetCommentsIds();

            FillAttachments(newsIdsArray);
            FillCommentsAttachments(commentsIdsArray);
        }

        private void FillNewsStatuses()
        {
            _newsStatuses.Clear();

            const string sqlCommandText = @"SELECT NewsStatusID, NewsStatusName, NewsStatusColor, NewsStatusGroupID 
                                            FROM FAIINewsFeed.NewsStatus";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);

                try
                {
                    sqlConn.Open();
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            int newsStatusId;
                            Int32.TryParse(sqlReader["NewsStatusID"].ToString(), out newsStatusId);
                            var newsStatusName = sqlReader["NewsStatusName"].ToString();
                            var newsStatusColor = sqlReader["NewsStatusColor"].ToString();
                            int newsStatusGroupId;
                            Int32.TryParse(sqlReader["NewsStatusGroupID"].ToString(), out newsStatusGroupId);
                            var newsStatus = new NewsStatus(newsStatusId, newsStatusName, newsStatusColor,
                                newsStatusGroupId);
                            _newsStatuses.Add(newsStatus);
                        }
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }
        }

        private void FillNewsStatusGroups()
        {
            _newsStatusGroups.Clear();

            const string sqlCommandText = @"SELECT NewsStatusGroupID, NewsStatusGroupName 
                                            FROM FAIINewsFeed.NewsStatusGroups";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);

                try
                {
                    sqlConn.Open();
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            int newsStatusGroupId;
                            Int32.TryParse(sqlReader["NewsStatusGroupID"].ToString(), out newsStatusGroupId);
                            var newsStatusGroupName = sqlReader["NewsStatusGroupName"].ToString();

                            var newsStatusesForGroup =
                                NewsStatuses.Where(ns => ns.NewsStatusGroupId == newsStatusGroupId);
                            var statusesForGroup = newsStatusesForGroup as List<NewsStatus> ??
                                                   newsStatusesForGroup.ToList();

                            var newsStatusGroup = new NewsStatusGroup(newsStatusGroupId, newsStatusGroupName)
                            {
                                NewsStatuses = statusesForGroup
                            };
                            _newsStatusGroups.Add(newsStatusGroup);
                        }
                    }
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }
        }

        private void FillNews(int indexFrom, int limitCount, NewsStatus newsStatus, int? prodStatusId)
        {
            var filter = GetFilterString(newsStatus, prodStatusId);

            var sqlCommandText =
                @"SELECT NewsID, NewsDate, NewsText, NewsStatus, WorkerID, GlobalID, LastEditing, ProdStatusID 
                  FROM FAIINewsFeed.NewsFeed " + filter +
                 " ORDER BY LastEditing DESC, NewsDate DESC limit " + indexFrom + "," + limitCount;
            _newsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_newsAdapter);
            try
            {
                _news = new DataTable();
                _newsAdapter.Fill(_news);
            }
            catch (MySqlException)
            {
            }
        }

        private void FillNews(int indexFrom, int limitCount, NewsStatusGroup newsStatusGroup)
        {
            var filter = GetFilterString(newsStatusGroup);

            var sqlCommandText =
                @"SELECT NewsID, NewsDate, NewsText, NewsStatus, WorkerID, GlobalID, LastEditing, ProdStatusID 
                  FROM FAIINewsFeed.NewsFeed " + filter +
                 " ORDER BY LastEditing DESC, NewsDate DESC limit " + indexFrom + "," + limitCount;
            _newsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_newsAdapter);
            try
            {
                _news = new DataTable();
                _newsAdapter.Fill(_news);
            }
            catch (MySqlException)
            {
            }
        }

        private void FillNews(string globalId)
        {
            const string sqlCommandText = @"SELECT NewsID, NewsDate, NewsText, NewsStatus, 
                                            WorkerID, GlobalID, LastEditing, ProdStatusID 
                                            FROM FAIINewsFeed.NewsFeed Where GlobalID = @GlobalID";
            var sqlConn = new MySqlConnection(_connectionString);
            var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
            sqlCommand.Parameters.Add("GlobalID", MySqlDbType.VarChar).Value = globalId;

            _newsAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_newsAdapter);
            try
            {
                _news = new DataTable();
                _newsAdapter.Fill(_news);
            }
            catch (MySqlException)
            {
            }
        }

        private void FillComments(IEnumerable newsIdsArray)
        {
            var newsIdsString = string.Empty;

            newsIdsString = newsIdsArray != null
                ? (newsIdsArray.Cast<object>()
                    .Aggregate(newsIdsString, (current, newsId) => current + ", " + newsId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText =
                @"SELECT CommentID, NewsID, WorkerID, CommentDate, CommentText 
                  FROM FAIINewsFeed.NewsComments 
                  WHERE NewsID IN (" + newsIdsString + ")";
            _commentsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_commentsAdapter);

            try
            {
                _comments = new DataTable();
                _commentsAdapter.Fill(_comments);
            }
            catch (MySqlException)
            {
            }
        }

        private void FillAttachments(IEnumerable newsIdsArray)
        {
            var newsIdsString = string.Empty;
            newsIdsString = newsIdsArray != null
                ? (newsIdsArray.Cast<object>()
                    .Aggregate(newsIdsString, (current, newsId) => current + ", " + newsId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText =
                @"SELECT AttachmentID, NewsID, AttachmentName 
                  FROM FAIINewsFeed.Attachments 
                  WHERE NewsID IN (" + newsIdsString + ")";
            _attachmentsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_attachmentsAdapter);
            try
            {
                _attachments = new DataTable();
                _attachmentsAdapter.Fill(_attachments);
            }
            catch (MySqlException)
            {
            }
        }

        private void FillCommentsAttachments(IEnumerable commentsIdsArray)
        {
            var commentIdsString = string.Empty;
            commentIdsString = commentsIdsArray != null
                ? (commentsIdsArray.Cast<object>()
                    .Aggregate(commentIdsString, (current, commentId) => current + ", " + commentId))
                    .Remove(0, 2)
                : "-1";

            var sqlCommandText =
                @"SELECT CommentAttachmentID, CommentID, CommentAttachmentName 
                  FROM FAIINewsFeed.CommentAttachments 
                  WHERE CommentID IN (" + commentIdsString + ")";
            _commentsAttachmentsAdapter = new MySqlDataAdapter(sqlCommandText, _connectionString);
            new MySqlCommandBuilder(_commentsAttachmentsAdapter);
            try
            {
                _commentsAttachments = new DataTable();
                _commentsAttachmentsAdapter.Fill(_commentsAttachments);
            }
            catch (MySqlException)
            {
            }
        }




        #region News

        public int AddNews(string newsText, DateTime newsDate, int newsStatusId, 
            int workerId, string globalId, int? prodStatusId)
        {
            var newRow = News.NewRow();
            newRow["NewsText"] = newsText;
            newRow["NewsDate"] = newsDate;
            newRow["NewsStatus"] = newsStatusId;
            newRow["WorkerID"] = workerId;
            newRow["GlobalID"] = globalId;
            newRow["LastEditing"] = newsDate;

            if (prodStatusId.HasValue)
                newRow["ProdStatusID"] = prodStatusId.Value;

            News.Rows.Add(newRow);

            UpdateNews();

            var newsId = GetNewsId(newsStatusId, workerId, newsDate);
            newRow["NewsID"] = newsId;
            newRow.AcceptChanges();
            return newsId;
        }

        private int GetNewsId(int newsStatusId, int workerId, DateTime newsDate)
        {
            var newsId = -1;

            const string sqlCommandText = @"SELECT NewsID FROM FAIINewsFeed.NewsFeed 
                                            WHERE NewsStatus = @NewsStatus AND WorkerID = @WorkerID 
                                            AND NewsDate = @NewsDate";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@NewsStatus", MySqlDbType.Int64).Value = newsStatusId;
                    sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                    sqlCommand.Parameters.Add("@NewsDate", MySqlDbType.DateTime).Value = newsDate;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            newsId = Convert.ToInt32(result);
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

            return newsId;
        }

        public void ChangeNewsText(int newsId, string newsText)
        {
            var rows = News.Select(string.Format("NewsID = {0}", newsId));
            if (!rows.Any()) return;

            var changingNews = rows.First();
            changingNews["NewsText"] = newsText;

            UpdateNews();
        }

        public void DeleteNews(int newsId)
        {
            var rows = News.Select(string.Format("NewsID = {0}", newsId));
            if (!rows.Any()) return;

            var deletingNews = rows.First();
            deletingNews.Delete();

            UpdateNews();
        }

        public void UpdateNewsLastEditing(int newsId, DateTime lastEditing)
        {
            var news = News.Select(string.Format("NewsID = {0}", newsId));
            if (!news.Any()) return;

            var row = news[0];
            row["LastEditing"] = lastEditing;
            UpdateNews();
        }

        private void UpdateNews()
        {
            try
            {
                _newsAdapter.Update(News);
            }
            catch (MySqlException)
            {
            }
        }

        public int GetNewsCount(NewsStatus newsStatus, int? prodStatusId)
        {
            var rowsCount = 0;
            var filter = GetFilterString(newsStatus, prodStatusId);

            var sql = "SELECT COUNT(*) FROM FAIINewsFeed.NewsFeed " + filter;

            using (var conn = new MySqlConnection(_connectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                try
                {
                    conn.Open();
                    rowsCount = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }
                catch (MySqlException)
                {
                }
            }
            return rowsCount;
        }

        public int GetNewsCount(NewsStatusGroup showAllGroup)
        {
            var rowsCount = 0;
            var filter = GetFilterString(showAllGroup);

            var sql = "SELECT COUNT(*) FROM FAIINewsFeed.NewsFeed " + filter;

            using (var conn = new MySqlConnection(_connectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                try
                {
                    conn.Open();
                    rowsCount = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }
                catch (MySqlException)
                {
                }
            }
            return rowsCount;
        }

        private static string GetPersonalTasksGlobalIds()
        {
            var globalIds = new List<string>();
            const string sqlCommandText = @"SELECT DISTINCT ot.GlobalID 
                                            FROM 
	                                            (SELECT t.MainWorkerID, t.GlobalID, t.TaskID, o.WorkerID 
                                                 FROM FAIITasks.Tasks as t 
	                                             LEFT JOIN FAIITasks.Observers as o 
                                                 ON t.TaskID = o.TaskID) ot 
                                            JOIN (FAIITasks.Performers as p) 
                                            ON ot.TaskID = p.TaskID 
                                            WHERE ot.MainWorkerID = @WorkerID OR p.WorkerID = @WorkerID 
                                                  OR ot.WorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                    AdministrationClass.CurrentWorkerId;
                try
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            globalIds.Add(reader[0].ToString());
                        }
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

            var globalIdsString = string.Empty;

            globalIdsString = globalIds.Any()
                ? (globalIds.Aggregate(globalIdsString, (current, newsId) => current + ", " + newsId)).Remove(0, 2)
                : "-1";
            return globalIdsString;
        }

        private static string GetPersonalWorkerRequestsGlobalIds()
        {
            var globalIds = new List<string>();
            const string sqlCommandText = @"SELECT DISTINCT GlobalID 
                                            FROM FAIIVacations.WorkerRequests 
                                            WHERE WorkerID = @WorkerID OR MainWorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value =
                    AdministrationClass.CurrentWorkerId;
                try
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            globalIds.Add(reader[0].ToString());
                        }
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

            var globalIdsString = string.Empty;

            globalIdsString = globalIds.Any()
                ? (globalIds.Aggregate(globalIdsString, (current, newsId) => current + ", " + newsId)).Remove(0, 2)
                : "-1";
            return globalIdsString;
        }

        private static string GetFilterString(NewsStatus newsStatus, int? prodStatusId)
        {
            var filterString = string.Format("WHERE NewsStatus = {0}", newsStatus.NewsStatusId);

            // Geting available production statuses for worker
            if (newsStatus.NewsStatusId == 6 || newsStatus.NewsStatusId == 7)
            {
                if (prodStatusId.HasValue)
                    filterString += string.Format(" AND ProdStatusID = {0}", prodStatusId.Value);

                // Return all messages if user is administrator
                else if (!AdministrationClass.IsAdministrator)
                {
                    //var workerGroups = GetWorkerGroupsIds(AdministrationClass.CurrentWorkerId);
                    //if (workerGroups.All(g => g != 1))
                    //{
                        var workerProdStatuses = GetWorkerProdStatuses(AdministrationClass.CurrentWorkerId);
                        if (workerProdStatuses.Any())
                        {
                            var workerProdStatusesString = workerProdStatuses.Aggregate(string.Empty, (current, prodStatus) => current + ", " + prodStatus).Remove(0, 2);
                            filterString += " AND (ProdStatusID IN (" + workerProdStatusesString + ") OR ProdStatusID IS NULL)";
                        }
                        else
                        {
                            filterString += " AND ProdStatusID IS NULL";
                        }
                    //}
                }
            }

            // Worker requests
            if (newsStatus.NewsStatusId == 9 && !AdministrationClass.IsAdministrator)
            {
                var fullAccess = AdministrationClass.HasFullAccess(AdministrationClass.Modules.WorkerRequests);
                if (!fullAccess)
                {
                    // Get personal worker requests
                    var workerRequestsGlobalIdsString = GetPersonalWorkerRequestsGlobalIds();
                    filterString += " AND GlobalID IN (" + workerRequestsGlobalIdsString + ")";
                }
            }

            // Worker tasks
            if (newsStatus.NewsStatusId == 11 && !AdministrationClass.IsAdministrator)
            {
                // Get GlobalIds of personal messages

                var globalIdsString = GetPersonalTasksGlobalIds();

                filterString += " AND GlobalID IN (" + globalIdsString + ")";
            }

            return filterString;
        }

        private string GetFilterString(NewsStatusGroup newsStatusGroup)
        {
            var newsStatusIds = newsStatusGroup.NewsStatuses.Any()
                ? (newsStatusGroup.NewsStatuses
                    .Aggregate(string.Empty, (current, newsStatus) => current + ", " + newsStatus.NewsStatusId))
                    .Remove(0, 2)
                : "-1";
            var filterString = string.Format("WHERE NewsStatus IN ({0})", newsStatusIds);

            // Return all messages if user is administrator
            if (AdministrationClass.IsAdministrator) return filterString;

            if (newsStatusGroup.NewsStatusGroupId == 2)
            {
                var fullAccess = AdministrationClass.HasFullAccess(AdministrationClass.Modules.WorkerRequests);
                if (!fullAccess)
                {
                    // Get news statuses without worker requests
                    newsStatusIds = newsStatusGroup.NewsStatuses.Where(newsStatus => newsStatus.NewsStatusId != 9).Any()
                    ? (newsStatusGroup.NewsStatuses.Where(newsStatus => newsStatus.NewsStatusId != 9)
                        .Aggregate(string.Empty, (current, newsStatus) => current + ", " + newsStatus.NewsStatusId))
                        .Remove(0, 2)
                    : "-1";

                    filterString = string.Format("WHERE (NewsStatus IN ({0})", newsStatusIds);

                    // Get personal worker requests
                    var workerRequestsGlobalIdsString = GetPersonalWorkerRequestsGlobalIds();
                    filterString += " OR GlobalID IN (" + workerRequestsGlobalIdsString + "))";
                }

                // Geting available production statuses for worker
                //var workerGroups = GetWorkerGroupsIds(AdministrationClass.CurrentWorkerId);
                //if(workerGroups.All(g => g != 1))
                //{
                    var workerProdStatuses = GetWorkerProdStatuses(AdministrationClass.CurrentWorkerId);
                    if(workerProdStatuses.Any())
                    {
                        var workerProdStatusesString = workerProdStatuses.Aggregate(string.Empty, (current, prodStatus) => current + ", " + prodStatus).Remove(0, 2);
                        filterString += " AND (ProdStatusID IN (" + workerProdStatusesString + ") OR ProdStatusID IS NULL)";
                    }
                    else
                    {
                        filterString += " AND ProdStatusID IS NULL";
                    }
                //}
            }

            if (newsStatusGroup.NewsStatusGroupId == 3)
            {
                // Get GlobalIds of personal messages

                var globalIdsString = GetPersonalTasksGlobalIds();

                filterString += " AND GlobalID IN (" + globalIdsString + ")";
            }

            return filterString;
        }

        public static IEnumerable<int> GetWorkerProdStatuses(int workerId)
        {
            var workerProdStatuses = new List<int>();
            const string sqlCommandText = @"SELECT ProdStatusID FROM FAIIStaff.WorkerProdStatuses
                                            WHERE WorkerID = @WorkerID";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

                try
                {
                    sqlConn.Open();
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            if (sqlReader.FieldCount != 0 && sqlReader[0] != DBNull.Value)
                            {
                                var prodStatusId = Convert.ToInt32(sqlReader[0]);
                                workerProdStatuses.Add(prodStatusId);
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    sqlConn.Close();
                }
            }

            return workerProdStatuses;
        }

        //public static IEnumerable<int> GetWorkerGroupsIds(int workerId)
        //{
        //    var workerGroups = new List<int>();
        //    const string sqlCommandText = @"SELECT DISTINCT WorkerGroupID FROM FAIIStaff.WorkerProfessions
        //                                    WHERE WorkerID = @WorkerID";
        //    using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
        //    {
        //        var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
        //        sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;

        //        try
        //        {
        //            sqlConn.Open();
        //            using (var sqlReader = sqlCommand.ExecuteReader())
        //            {
        //                while (sqlReader.Read())
        //                {
        //                    if (sqlReader.FieldCount != 0 && sqlReader[0] != DBNull.Value)
        //                    {
        //                        var workerGroupId = Convert.ToInt32(sqlReader[0]);
        //                        workerGroups.Add(workerGroupId);
        //                    }
        //                }
        //            }
        //        }
        //        catch { }
        //        finally
        //        {
        //            sqlConn.Close();
        //        }
        //    }

        //    return workerGroups;
        //}

        public static int GetNewMessagesCount(NewsStatus newsStatus, int workerId, DateTime lastUpdate)
        {
            var rowsCount = 0;
            var filter = GetFilterString(newsStatus, null);
            var latestNewsSqlCommandText = @"SELECT NewsID, WorkerID, NewsDate FROM FAIINewsFeed.NewsFeed as news "
                                           + filter + " AND news.LastEditing >= @LastUpdate";
            var latestNewsTable = new DataTable();

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(latestNewsSqlCommandText, conn);
                sqlCommand.Parameters.Add("@LastUpdate", MySqlDbType.DateTime).Value = lastUpdate;
                var adapter = new MySqlDataAdapter(sqlCommand);

                try
                {
                    adapter.Fill(latestNewsTable);
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    adapter.Dispose();
                    sqlCommand.Dispose();
                }
            }

            if (latestNewsTable.Rows.Count == 0) return 0;

            var latestNewsIds = from newsRow in latestNewsTable.AsEnumerable()
                                 select Convert.ToInt32(newsRow["NewsID"]);

            if (!latestNewsIds.Any()) return 0;

            var newsIdsString = string.Empty;
            newsIdsString = (latestNewsIds.Cast<object>()
                    .Aggregate(newsIdsString, (current, newsId) => current + ", " + newsId))
                    .Remove(0, 2);      

            var latestCommentsSqlCommandText = @"SELECT COUNT(*) FROM FAIINewsFeed.NewsComments 
                                                 WHERE NewsID IN (" + newsIdsString + ") " +
                                               "AND CommentDate >= @LastUpdate AND WorkerID != @WorkerID";
            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(latestCommentsSqlCommandText, conn);
                sqlCommand.Parameters.Add("@LastUpdate", MySqlDbType.DateTime).Value = lastUpdate;
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                try
                {
                    conn.Open();
                    rowsCount = Convert.ToInt32(sqlCommand.ExecuteScalar());
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    conn.Close();
                    sqlCommand.Dispose();
                }
            }

            var latestNewsCount =
                latestNewsTable.AsEnumerable()
                    .Count(n => n.Field<Int64>("WorkerID") != workerId &&
                    n.Field<DateTime>("NewsDate") > lastUpdate);
            rowsCount += latestNewsCount;

            return rowsCount;
        }

        private Array GetNewsIds()
        {
            var newsIds =
                from newsView in News.AsEnumerable()
                select Convert.ToInt32(newsView["NewsID"]);

            var newsIdsArray = newsIds.ToArray();

            return newsIdsArray.Length == 0 ? null : newsIdsArray;
        }

        #endregion



        #region Comments

        public int AddComment(string commentText, DateTime commentDate, int newsId, int workerId)
        {
            var newComment = Comments.NewRow();
            newComment["CommentText"] = commentText;
            newComment["CommentDate"] = commentDate;
            newComment["NewsID"] = newsId;
            newComment["WorkerID"] = workerId;
            Comments.Rows.Add(newComment);

            UpdateComments();

            var commentId = GetCommentId(newsId, commentDate);
            newComment["CommentID"] = commentId;
            newComment.AcceptChanges();
            return commentId;
        }

        private int GetCommentId(int newsId, DateTime commentDate)
        {
            var commentId = -1;

            const string sqlCommandText = @"SELECT CommentID FROM FAIINewsFeed.NewsComments 
                                            WHERE NewsID = @NewsID AND CommentDate = @CommentDate";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@NewsID", MySqlDbType.Int64).Value = newsId;
                    sqlCommand.Parameters.Add("@CommentDate", MySqlDbType.DateTime).Value = commentDate;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            commentId = Convert.ToInt32(result);
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

            return commentId;
        }

        public void ChangeCommentText(int commentId, string commentText)
        {
            var rows = Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!rows.Any()) return;

            var changingComment = rows.First();
            changingComment["CommentText"] = commentText;

            UpdateComments();
        }

        public void DeleteComment(int commentId)
        {
            var rows = Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!rows.Any()) return;

            var deletingComment = rows.First();
            deletingComment.Delete();

            UpdateComments();
        }

        public void DeleteComments(int newsId)
        {
            var rows = Comments.Select(string.Format("NewsID = {0}", newsId));
            if (!rows.Any()) return;

            foreach (var deletingComment in rows)
            {
                deletingComment.Delete();
            }

            UpdateComments();
        }

        private void UpdateComments()
        {
            try
            {
                _commentsAdapter.Update(Comments);
            }
            catch (MySqlException)
            {
            }
        }

        private Array GetCommentsIds()
        {
            var filePaths =
                from commetsView in Comments.AsEnumerable()
                select Convert.ToInt32(commetsView["CommentID"]);

            var commentsIdsArray = filePaths.ToArray();

            return commentsIdsArray.Length == 0 ? null : commentsIdsArray;
        }

        #endregion



        #region NewsAttachments

        public void AddAttachment(int newsId, string attachmentName)
        {
            var newAttachment = Attachments.NewRow();
            newAttachment["NewsID"] = newsId;
            newAttachment["AttachmentName"] = attachmentName;
            Attachments.Rows.Add(newAttachment);

            UpdateAttachments();

            var attachmentId = GetAttachmentId(newsId, attachmentName);
            newAttachment["AttachmentID"] = attachmentId;
            newAttachment.AcceptChanges();
        }

        private int GetAttachmentId(int newsId, string attachmentName)
        {
            var attachmentId = -1;

            const string sqlCommandText = @"SELECT AttachmentID FROM FAIINewsFeed.Attachments 
                                            WHERE NewsID = @NewsID AND AttachmentName LIKE @AttachmentName";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@NewsID", MySqlDbType.Int64).Value = newsId;
                    sqlCommand.Parameters.Add("@AttachmentName", MySqlDbType.Text).Value = attachmentName;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            attachmentId = Convert.ToInt32(result);
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

            return attachmentId;
        }

        public void DeleteAttachment(int attachmentId)
        {
            var rows = Attachments.Select(string.Format("AttachmentID = {0}", attachmentId));
            if (!rows.Any()) return;

            var deletingAttachment = rows.First();
            deletingAttachment.Delete();

            UpdateAttachments();
        }

        public void DeleteAttachments(int newsId)
        {
            var rows = Attachments.Select(string.Format("NewsID = {0}", newsId));
            if (!rows.Any()) return;

            foreach (var deletingAttachment in rows)
            {
                deletingAttachment.Delete();
            }

            UpdateAttachments();
        }

        private void UpdateAttachments()
        {
            try
            {
                _attachmentsAdapter.Update(Attachments);
            }
            catch (MySqlException)
            {
            }
        }

        #endregion



        #region CommentsAttachments

        public void AddCommentAttachment(int commentId, string commentAttachmentName)
        {
            var newCommentAttachment = CommentsAttachments.NewRow();
            newCommentAttachment["CommentID"] = commentId;
            newCommentAttachment["CommentAttachmentName"] = commentAttachmentName;
            CommentsAttachments.Rows.Add(newCommentAttachment);

            UpdateCommentsAttachments();

            var commentAttachmentId = GetCommentAttachmentId(commentId, commentAttachmentName);
            newCommentAttachment["CommentAttachmentID"] = commentAttachmentId;
            newCommentAttachment.AcceptChanges();
        }

        private int GetCommentAttachmentId(int commentId, string commentAttachmentName)
        {
            var commentAttachmentId = -1;

            const string sqlCommandText = @"SELECT CommentAttachmentID FROM FAIINewsFeed.CommentAttachments 
                                            WHERE CommentID = @CommentID AND 
                                            CommentAttachmentName LIKE @CommentAttachmentName";
            using (var sqlConn = new MySqlConnection(_connectionString))
            {
                using (var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn))
                {
                    sqlCommand.Parameters.Add("@CommentID", MySqlDbType.Int64).Value = commentId;
                    sqlCommand.Parameters.Add("@CommentAttachmentName", MySqlDbType.Text).Value = commentAttachmentName;

                    try
                    {
                        sqlConn.Open();

                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            commentAttachmentId = Convert.ToInt32(result);
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

            return commentAttachmentId;
        }

        public void DeleteCommentAttachment(int commentAttachmentId)
        {
            var rows = CommentsAttachments.Select(string.Format("CommentAttachmentID = {0}", commentAttachmentId));
            if (!rows.Any()) return;

            var deletingCommentAttachment = rows.First();
            deletingCommentAttachment.Delete();

            UpdateCommentsAttachments();
        }

        public void DeleteCommentAttachments(int commentId)
        {
            var rows = CommentsAttachments.Select(string.Format("CommentID = {0}", commentId));
            if (!rows.Any()) return;

            foreach (var deletingCommentAttachment in rows)
            {
                deletingCommentAttachment.Delete();
            }

            UpdateCommentsAttachments();
        }

        private void UpdateCommentsAttachments()
        {
            try
            {
                _commentsAttachmentsAdapter.Update(CommentsAttachments);
            }
            catch (MySqlException)
            {
            }
        }

        #endregion
    }


    public class NewsStatus : ICloneable, INotifyPropertyChanged
    {
        private int _newsStatusId;
        private string _newsStatusName;
        private string _newsStatusColor;
        private int _newsStatusGroupId;
        private bool _isSelected;
        private int _newEventsCount;
        private DateTime _lastUpdate;

        public int NewsStatusId
        {
            get { return _newsStatusId; }
            set
            {
                _newsStatusId = value;
                OnPropertyChanged("NewsStatusId");
            }
        }

        public string NewsStatusName
        {
            get { return _newsStatusName; }
            set
            {
                _newsStatusName = value;
                OnPropertyChanged("NewsStatusName");
            }
        }

        public string NewsStatusColor
        {
            get { return _newsStatusColor; }
            set
            {
                _newsStatusColor = value;
                OnPropertyChanged("NewsStatusColor");
            }
        }

        public int NewsStatusGroupId
        {
            get { return _newsStatusGroupId; }
            set
            {
                _newsStatusGroupId = value;
                OnPropertyChanged("NewsStatusGroupId");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public int NewEventsCount
        {
            get { return _newEventsCount; }
            set
            {
                _newEventsCount = value;
                OnPropertyChanged("NewEventsCount");
            }
        }

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                OnPropertyChanged("LastUpdate");
            }
        }

        public NewsStatus(int newsStatusId, string newsStatusName, string newsStatusColor, int newsStatusGroupId)
        {
            _newsStatusId = newsStatusId;
            NewsStatusName = newsStatusName;
            _newsStatusColor = newsStatusColor;
            _newsStatusGroupId = newsStatusGroupId;

            IsSelected = false;
            NewEventsCount = 0;
            LastUpdate = DateTime.MinValue;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class NewsStatusGroup : ICloneable, INotifyPropertyChanged
    {
        private int _newsStatusGroupId;
        private string _newsStatusGroupName;
        private bool _isSelected;
        private int _newEventsCount;
        private DateTime _lastUpdate;
        private List<NewsStatus> _newsStatuses;

        public int NewsStatusGroupId
        {
            get { return _newsStatusGroupId; }
            set
            {
                _newsStatusGroupId = value;
                OnPropertyChanged("NewsStatusGroupId");
            }
        }

        public string NewsStatusGroupName
        {
            get { return _newsStatusGroupName; }
            set
            {
                _newsStatusGroupName = value;
                OnPropertyChanged("NewsStatusGroupName");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public int NewEventsCount
        {
            get { return _newEventsCount; }
            set
            {
                _newEventsCount = value;
                OnPropertyChanged("NewEventsCount");
            }
        }

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                OnPropertyChanged("LastUpdate");
            }
        }

        public List<NewsStatus> NewsStatuses
        {
            get { return _newsStatuses; }
            set
            {
                _newsStatuses = value;
                OnPropertyChanged("NewsStatuses");
            }
        }

        

        public NewsStatusGroup(int newsStatusGroupId, string newsStatusGroupName)
        {
            _newsStatusGroupId = newsStatusGroupId;
            _newsStatusGroupName = newsStatusGroupName;

            IsSelected = false;
            NewEventsCount = 0;
            LastUpdate = DateTime.MinValue;
        }

        public object Clone()
        {
            var newStatusGroup = (NewsStatusGroup) MemberwiseClone();
            newStatusGroup.NewsStatuses = new List<NewsStatus>();
            NewsStatuses.ForEach(ns => newStatusGroup.NewsStatuses.Add((NewsStatus) ns.Clone()));
            return newStatusGroup;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
