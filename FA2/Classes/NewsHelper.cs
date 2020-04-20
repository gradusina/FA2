using System;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    internal static class NewsHelper
    {
        public static void AddNews(DateTime newsDate, string newsText, int newsStatus, int workerId)
        {
            using (var sqlConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var adapter = new MySqlDataAdapter();
                var insertCommand = new MySqlCommand(@"INSERT INTO FAIINewsFeed.NewsFeed 
                                                       (NewsDate, NewsText, NewsStatus, WorkerID, LastEditing) 
                                                       VALUES (@NewsDate, @NewsText, @NewsStatus, @WorkerID, @LastEditing)",
                    sqlConnection);

                insertCommand.Parameters.Add("@NewsDate", MySqlDbType.DateTime).Value = newsDate;
                insertCommand.Parameters.Add("@NewsText", MySqlDbType.Text).Value = newsText;
                insertCommand.Parameters.Add("@NewsStatus", MySqlDbType.Int32).Value = newsStatus;
                insertCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                insertCommand.Parameters.Add("@LastEditing", MySqlDbType.DateTime).Value = newsDate;
                adapter.InsertCommand = insertCommand;
                try
                {
                    sqlConnection.Open();
                    adapter.InsertCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (MySqlException)
                {
                }
            }
        }

        public static void AddNews(DateTime newsDate, string newsText, int newsStatus, int workerId, string globalId)
        {
            using (var sqlConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var adapter = new MySqlDataAdapter();
                var insertCommand = new MySqlCommand(@"INSERT INTO FAIINewsFeed.NewsFeed 
                                                       (NewsDate, NewsText, GlobalID, NewsStatus, WorkerID, LastEditing) 
                                                       VALUES (@NewsDate, @NewsText, @GlobalID, @NewsStatus, @WorkerID, @LastEditing)",
                    sqlConnection);

                insertCommand.Parameters.Add("@NewsDate", MySqlDbType.DateTime).Value = newsDate;
                insertCommand.Parameters.Add("@NewsText", MySqlDbType.Text).Value = newsText;
                insertCommand.Parameters.Add("@GlobalID", MySqlDbType.VarChar).Value = string.IsNullOrEmpty(globalId) ? null : globalId;
                insertCommand.Parameters.Add("@NewsStatus", MySqlDbType.Int32).Value = newsStatus;
                insertCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                insertCommand.Parameters.Add("@LastEditing", MySqlDbType.DateTime).Value = newsDate;
                adapter.InsertCommand = insertCommand;
                try
                {
                    sqlConnection.Open();
                    adapter.InsertCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (MySqlException)
                {
                }
            }
        }

        public static void AddTextToNews(DateTime newsDate, int workerId, string receivedText)
        {
            using (var sqlConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var adapter = new MySqlDataAdapter();
                var updateCommand = new MySqlCommand(@"UPDATE FAIINewsFeed.NewsFeed 
                                                       SET NewsText = concat(NewsText, @AddText) 
                                                       WHERE NewsDate = @NewsDate AND WorkerID = @WorkerID",
                    sqlConnection);
                updateCommand.Parameters.Add("@AddText", MySqlDbType.VarChar, 200).Value = receivedText;
                updateCommand.Parameters.Add("@NewsDate", MySqlDbType.DateTime).Value = newsDate;
                updateCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                adapter.UpdateCommand = updateCommand;
                try
                {
                    sqlConnection.Open();
                    adapter.UpdateCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (MySqlException)
                {
                }
            }
        }

        public static void DeleteNews(DateTime newsDate, int workerId)
        {
            using (var sqlConnection = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var adapter = new MySqlDataAdapter();
                var deleteCommand = new MySqlCommand(@"DELETE FROM FAIINewsFeed.NewsFeed 
                                                       WHERE NewsDate = @NewsDate AND WorkerID = @WorkerID",
                    sqlConnection);
                deleteCommand.Parameters.Add("@NewsDate", MySqlDbType.DateTime).Value = newsDate;
                deleteCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                adapter.DeleteCommand = deleteCommand;
                try
                {
                    sqlConnection.Open();
                    adapter.DeleteCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (MySqlException)
                {
                }
            }
        }
    }
}
