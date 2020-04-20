using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using FA2.Classes;
using FAII.Notifications;
using MySql.Data.MySqlClient;

namespace FA2.Notifications
{
    public static class NotificationManager
    {
        private static int _workerId;
        private static DataTable _notificationsTable;
        private static MySqlDataAdapter _notificationsAdapter;

        private static GrowlNotifiactions _growNotifications;
        private const double TopOffset = 60;
        private const double LeftOffset = 320;
        private static Timer _notificationsUpdateTimer;
        private const int UpdateTimerInterval = 300000;

        private static AdministrationClass _administrationClass;

        private static DataTable NotificationsTable
        {
            get { return _notificationsTable ?? (_notificationsTable = new DataTable()); }
            set { _notificationsTable = value; }
        }

        public static readonly List<TileNotification> TileNotifications = new List<TileNotification>();

        /// <summary>
        /// Fill notification info for worker
        /// </summary>
        private static void FillNotifications(int workerId)
        {
            const string sqlCommandText = "SELECT NotificationID, WorkerID, ModuleID, " +
                                          "OriginalID, ShowCount, ShowNotification " +
                                          "FROM FAIIAdministration.Notifications " +
                                          "WHERE WorkerID = @WorkerID AND ShowCount = TRUE";
            var sqlCommand = new MySqlCommand(sqlCommandText, new MySqlConnection(App.ConnectionInfo.ConnectionString));
            sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
            _notificationsAdapter = new MySqlDataAdapter(sqlCommand);
            new MySqlCommandBuilder(_notificationsAdapter);
            try
            {
                NotificationsTable.Clear();
                _notificationsAdapter.Fill(NotificationsTable);
            }
            catch (MySqlException)
            {
            }
        }

        /// <summary>
        /// Add new notification
        /// </summary>
        /// <param name="workerId">Worker, that should see this notification</param>
        /// <param name="module">Module, for wich notification is added</param>
        /// <param name="originalId">Id of the new row</param>
        public static void AddNotification(int workerId, AdministrationClass.Modules module, int originalId)
        {
            //if(workerId == AdministrationClass.CurrentWorkerId) return;

            const string sqlCommandText = "INSERT INTO FAIIAdministration.Notifications " +
                                          "(WorkerID, ModuleID, OriginalID, ShowCount, ShowNotification) " +
                                          "VALUES (@WorkerID, @ModuleID, @OriginalID, @ShowCount, @ShowNotification)";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ModuleID", MySqlDbType.Int64).Value = (int)module;
                sqlCommand.Parameters.Add("@OriginalID", MySqlDbType.Int64).Value = originalId;
                sqlCommand.Parameters.Add("@ShowCount", MySqlDbType.Bit).Value = true;
                sqlCommand.Parameters.Add("@ShowNotification", MySqlDbType.Bit).Value = true;

                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteScalar();
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

        /// <summary>
        /// Delete all notifications from module for current worker
        /// </summary>
        /// <param name="module">Id of selected module</param>
        public static void ClearNotifications(AdministrationClass.Modules module)
        {
            if (_notificationsAdapter == null) return;

            foreach (var dataRow in NotificationsTable.AsEnumerable().
                Where(r => r.Field<Int64>("ModuleID") == (long)module))
            {
                dataRow.Delete();
            }

            try
            {
                _notificationsAdapter.Update(NotificationsTable);
            }
            catch (MySqlException)
            {
            }
        }

        /// <summary>
        /// Returns notifications count for module
        /// </summary>
        /// <param name="module">Id of selected module</param>
        /// <returns>Notifications count</returns>
        public static int GetNotificationsCount(AdministrationClass.Modules module)
        {
            var notificationCount =
                NotificationsTable.AsEnumerable().Count(r => r.Field<Int64>("ModuleID") == (long) module);
            return notificationCount;
        }

        /// <summary>
        /// Clear ShowNotification value for every notification in module
        /// </summary>
        /// <param name="module">Id of selected module</param>
        private static void ClearShowNotifications(AdministrationClass.Modules module)
        {
            if (_notificationsAdapter == null) return;

            foreach (var dataRow in NotificationsTable.AsEnumerable().
                Where(r => r.Field<Int64>("ModuleID") == (long)module))
            {
                dataRow["ShowNotification"] = false;
            }

            try
            {
                _notificationsAdapter.Update(NotificationsTable);
            }
            catch (MySqlException)
            {
            }
        }

        public static void ShowNotifications(int workerId)
        {
            _workerId = workerId;

            if (_growNotifications == null)
                GrowNotificationsInitialize();

            if (_notificationsUpdateTimer == null)
                TimerInitialize();

            NotificationsUpdateTimerOnTick(null, null);
        }

        private static void TimerInitialize()
        {
            _notificationsUpdateTimer = new Timer {Interval = UpdateTimerInterval};
            _notificationsUpdateTimer.Tick -= NotificationsUpdateTimerOnTick;
            _notificationsUpdateTimer.Tick += NotificationsUpdateTimerOnTick;
            _notificationsUpdateTimer.Start();
        }

        private static void NotificationsUpdateTimerOnTick(object sender, EventArgs eventArgs)
        {
            // Fill notification info for current worker
            FillNotifications(_workerId);

            // Fill module information
            if (_administrationClass == null)
                App.BaseClass.GetAdministrationClass(ref _administrationClass);

            // Calculate notifications for available modules
            foreach (var tileNotification in TileNotifications)
            {
                var module = tileNotification.Module;
                tileNotification.NotificationsCount = GetNotificationsCount(module);
            }

            // Show new notifications
            var modulesWithNotifications =
                    (from notifications in NotificationsTable.AsEnumerable()
                     where notifications.Field<Boolean>("ShowNotification")
                     select notifications.Field<Int64>("ModuleID")).Distinct().ToList();
            foreach (var module in modulesWithNotifications)
            {
                var moduleId = module;
                var moduleRows =
                    _administrationClass.ModulesTable.AsEnumerable().Where(m => m.Field<Int64>("ModuleID") == moduleId);
                if (!moduleRows.Any()) continue;

                // Creating notification properties
                var moduleRow = moduleRows.First();
                var notificationIcon = moduleRow["ModuleIcon"] != DBNull.Value
                    ? AdministrationClass.ObjectToBitmapImage(moduleRow["ModuleIcon"])
                    : null;
                var notificationTitle = string.Format("{0}", moduleRow["ModuleName"]);
                var notificationCount = GetNotificationsCount((AdministrationClass.Modules)module);
                var notificationText = string.Format("Новых записей: {0}", notificationCount);
                var convertedBrush = new BrushConverter().ConvertFrom(moduleRow["ModuleColor"]) as Brush;
                var notificationBrush = convertedBrush ?? Brushes.Transparent;

                // Create Notification
                var notification = new Notification
                {
                    Title = notificationTitle,
                    Message = notificationText,
                    Image = notificationIcon,
                    Brush = notificationBrush
                };
                _growNotifications.AddNotification(notification);

                // Clear 'ShowNotification' for module
                ClearShowNotifications((AdministrationClass.Modules)module);
            }
        }

        private static void GrowNotificationsInitialize()
        {
            _growNotifications = new GrowlNotifiactions
            {
                Top = SystemParameters.WorkArea.Top + TopOffset,
                Left =
                    SystemParameters.WorkArea.Left +
                    SystemParameters.WorkArea.Width - LeftOffset
            };
        }

        public static void StopUpdate()
        {
            if (_notificationsUpdateTimer != null)
            {
                _notificationsUpdateTimer.Stop();
            }
        }
    }

    public sealed class TileNotification : INotifyPropertyChanged
    {
        private AdministrationClass.Modules _module;
        public AdministrationClass.Modules Module
        {
            get { return _module; }
            set
            {
                _module = value;
                OnPropertyChanged("Module");
            }
        }

        private int _notificationsCount;

        public int NotificationsCount
        {
            get { return _notificationsCount; }
            set
            {
                _notificationsCount = value;
                OnPropertyChanged("NotificationsCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
