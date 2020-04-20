using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using FA2.ChildPages.NewsFeedPage;
using FA2.ChildPages.TaskPage;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FA2.XamlFiles;
using Microsoft.Win32;

namespace FA2.ViewModels
{
    public class TasksViewModel : INotifyPropertyChanged
    {
        private readonly TaskClass _taskClass;
        private readonly StaffClass _staffClass;
        private readonly TimeTrackingClass _timeTrackingClass;
        private readonly NewsFeedClass _newsFeedClass;

        private readonly int _currentWorkerId;
        private DateTime _currentDateFrom;
        private DateTime _currentDateTo;

        private FtpClient _ftpClient;
        private string _basicDirectory;
        private string _tempDirectory;
        private readonly Queue<string> _uploadingFilesQueue = new Queue<string>();
        private WaitWindow _processWindow;
        private long _fileSize;
        private string _neededOpeningFilePath;

        private enum FilteringType
        {
            IsCharged,
            IsPerformed,
            IsObserved
        }

        private FilteringType _filteringType;

        public struct RenamedFile
        {
            public readonly string OriginalName;
            public readonly string Renamed;

            public RenamedFile(string originalName, string renamed)
            {
                OriginalName = originalName;
                Renamed = renamed;
            }

            public override string ToString()
            {
                return OriginalName;
            }
        }

        public TasksViewModel(bool fullAccess)
        {
            App.BaseClass.GetTaskClass(ref _taskClass);
            App.BaseClass.GetStaffClass(ref _staffClass);
            App.BaseClass.GetTimeTrackingClass(ref _timeTrackingClass);
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);

            SetFtpData();

            _currentWorkerId = AdministrationClass.CurrentWorkerId;

            TasksView = _taskClass.Tasks.Table.AsDataView();
            PerformerView = _taskClass.Performers.Table.AsDataView();
            ObserversView = _taskClass.Observers.Table.AsDataView();
            CommentAttachmentsView = new ObservableCollection<RenamedFile>();
            FullAccess = fullAccess;

            DateInitializing();

            FiltersInitializing();

            PreviousInitializing();

            CommandInitializing();

            FillTasksData();
        }

        private void SetFtpData()
        {
            _basicDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Файлы живой ленты/";
            _tempDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Temp/";
            _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
            _ftpClient.UploadProgressChanged += OnFtpClientUploadProgressChanged;
            _ftpClient.DownloadProgressChanged += OnFtpClientDownloadProgressChanged;

            //можно выкл
            //----------------------------------------------------
            if (!_ftpClient.DirectoryExist(_basicDirectory))
                _ftpClient.MakeDirectory(_basicDirectory);
            if (!_ftpClient.DirectoryExist(_tempDirectory))
                _ftpClient.MakeDirectory(_tempDirectory);
            //----------------------------------------------------
        }

        private void DateInitializing()
        {
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            var dateFrom = currentDate.Month > 2
                ? new DateTime(currentDate.Year, currentDate.Month - 2, 1)
                : new DateTime(currentDate.Year - 1, 12 + currentDate.Month - 2, 1);
            DateFrom = dateFrom;
            DateTo = new DateTime(currentDate.Year, currentDate.Month, daysInMonth);
        }

        private void FiltersInitializing()
        {
            // Geting worker groups data
            var workerGroupsView = _staffClass.GetWorkerGroups();
            WorkerGroupsView = workerGroupsView;

            // Geting factiries data
            var factoryViews = _staffClass.GetFactories();
            FactoriesView = factoryViews;

            // Seting id of selected worker group
            SelectedWorkerGroupId = WorkerGroupsView.Count != 0
                ? Convert.ToInt64(WorkerGroupsView[0]["WorkerGroupID"])
                : 0;

            // Seting id of selected factory
            SelectedFactoryId = FactoriesView.Count != 0
                ? Convert.ToInt64(FactoriesView[0]["FactoryID"])
                : 0;
        }

        private void PreviousInitializing()
        {
            IsPerformed = true;
            IsCharged = false;
            IsObserved = false;
            ShowCompleted = true;
            ShowTaskInfoDetailEnable = false;
            AddingCommentEnable = false;
        }

        private void CommandInitializing()
        {
            NewTask = new ActionCommand(ShowAddNewTaskWindow) {IsExecutable = true};
            Fill = new ActionCommand(FillTasksData) {IsExecutable = true};
            StartTask = new ActionCommand(BeginTask) {IsExecutable = true};
            EndTask = new ActionCommand(CompleteTask) {IsExecutable = true};
            DeleteTask = new ActionCommand(DeleteSelectedTask) {IsExecutable = true};
            EditTask = new ActionCommand(ShowEditTaskWindow) {IsExecutable = true};
            FillTimeTracking = new ActionCommand(ShowFillTaskTimeTracking) {IsExecutable = true};
            TaskTimeTrackingInfo = new ActionCommand(ShowTaskTimeTrackingInfo) {IsExecutable = true};
            ShowTaskDetails = new ActionCommand(ShowTaskInfoDetails) {IsExecutable = true};
            BackToTasksListCommand = new ActionCommand(BackToTasksList) {IsExecutable = true};
            ShowAddingCommentPanelCommand = new ActionCommand(ShowAddingCommentPanel) {IsExecutable = true};
            HideAddingCommentPanelCommand = new ActionCommand(HideAddingCommentPanel) {IsExecutable = true};
            AddNewCommentCommand = new ActionCommand(AddNewComment) {IsExecutable = true};
            SelectAttachmentsCommand = new ActionCommand(SelectAttachments) {IsExecutable = true};
            DeleteCommentCommand = new ActionCommand(DeleteComment) {IsExecutable = true};
            EditCommentCommand = new ActionCommand(EditComment) {IsExecutable = true};
            DownloadAndOpenCommentAttachmentCommand = new ActionCommand(DownloadAndOpenCommentAttachment)
            {
                IsExecutable = true
            };
            OpenNewCommentAttachmentCommand = new ActionCommand(OpenNewCommentAttachment) {IsExecutable = true};
            DeleteNewCommentAttachmentCommand = new ActionCommand(DeleteNewCommentAttachment) {IsExecutable = true};
            ExportToExcelCommand = new ActionCommand(ExportToExcel) {IsExecutable = true};
            AddToObserversCommand = new ActionCommand(AddToObservers) {IsExecutable = true};
        }

        private void FilterWorkers()
        {
            var workersView = _staffClass.FilterWorkers(true, (int) SelectedWorkerGroupId, true, (int) SelectedFactoryId,
                false, -1);

            WorkersView = workersView != null
                ? workersView.AsDataView()
                : null;

            if (!WorkerFilteringEnable)
            {
                SelectedWorkerId = 0;
                return;
            }

            SelectedWorkerId = WorkersView != null && WorkersView.Count != 0
                ? Convert.ToInt64(WorkersView[0]["WorkerID"])
                : 0;
        }

        private void ShowAddNewTaskWindow()
        {
            if (WorkerFilteringEnable)
                WorkerFilteringEnable = false;
            IsCharged = true;
            BackToTasksList();

            var addNewTask = new AddNewTask(TaskClass.SenderApplications.Tasks);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewTask, "Новая задача");
            }
        }

        private void ShowEditTaskWindow()
        {
            if(SelectedTaskRow == null) return;

            if (Convert.ToInt32(SelectedTaskRow["MainWorkerID"]) != _currentWorkerId ||
                Convert.ToBoolean(SelectedTaskRow["IsComplete"]))
                return;

            var addNewTask = new AddNewTask(SelectedTaskRow, true);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(addNewTask, string.Format("Задача №{0}", SelectedTaskRow["GlobalID"]));
            }
        }

        public void FillTasksData()
        {
            _currentDateFrom = DateFrom;
            _currentDateTo = DateTo;
            if (WorkerFilteringEnable)
                _taskClass.Fill(DateFrom, DateTo, (int) SelectedWorkerId);
            else
                _taskClass.Fill(DateFrom, DateTo, _currentWorkerId);

            if (WorkerFilteringEnable)
                FilterTaskView((int) SelectedWorkerId, ShowCompleted);
            else
                FilterTaskView(_currentWorkerId, ShowCompleted);
        }

        private void FilterTaskView(int workerId, bool showCompleted)
        {
            if (TasksView == null)
            {
                SelectedTaskId = 0;
                return;
            }

            switch (_filteringType)
            {
                case FilteringType.IsCharged:
                    TasksView.RowFilter =
                        string.Format(
                            showCompleted ? "MainWorkerID = {0}" : "MainWorkerID = {0} AND IsComplete = 'FALSE'",
                            workerId);
                    break;
                case FilteringType.IsPerformed:
                {
                    var filteredPerformerIds = showCompleted
                        ? from performers in
                            _taskClass.Performers.Table.Select(string.Format("WorkerID = {0}", workerId))
                            select performers.Field<Int64>("TaskID")
                        : from performers in
                            _taskClass.Performers.Table.Select(string.Format("WorkerID = {0} AND IsComplete = 'FALSE'",
                                workerId))
                            select performers.Field<Int64>("TaskID");

                    var taskIdsString = string.Empty;
                    //If returned task ids is empty, set filter to -1
                    var performerIds = filteredPerformerIds as IList<long> ?? filteredPerformerIds.ToList();
                    taskIdsString = performerIds.Count() != 0
                        ? (performerIds.Cast<object>()
                            .Aggregate(taskIdsString, (current, serviceHistoryId) => current + ", " + serviceHistoryId))
                            .Remove(0, 2)
                        : "-1";

                    TasksView.RowFilter = string.Format("TaskID IN ({0})", taskIdsString);
                }
                    break;
                case FilteringType.IsObserved:
                {
                    var filteredObserverIds = from observer in
                        _taskClass.Observers.Table.Select(string.Format("WorkerID = {0}", workerId))
                        select observer.Field<Int64>("TaskID");

                    var taskIdsString = string.Empty;
                    //If returned task ids is empty, set filter to -1
                    var taskIds = filteredObserverIds as IList<long> ?? filteredObserverIds.ToList();
                    taskIdsString = taskIds.Count() != 0
                        ? (taskIds.Cast<object>()
                            .Aggregate(taskIdsString, (current, serviceHistoryId) => current + ", " + serviceHistoryId))
                            .Remove(0, 2)
                        : "-1";
                    var filterString = string.Format("TaskID IN ({0})", taskIdsString);
                    if (!showCompleted)
                        filterString += " AND IsComplete = 'FALSE'";

                    TasksView.RowFilter = filterString;
                }
                    break;
            }

            SelectedTaskId = TasksView != null && TasksView.Count != 0
                ? Convert.ToInt64(TasksView[0]["TaskID"])
                : 0;
        }

        private bool GetStartTaskEnable(int taskId, int workerId)
        {
            var rows =
                _taskClass.Performers.Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == taskId && p.Field<Int64>("WorkerID") == workerId);
            if (!rows.Any()) return false;
            var performer = rows.First();
            var enable = performer["StartDate"] == DBNull.Value;

            return enable;
        }

        private bool GetFinishTaskEnable(int taskId, int workerId)
        {
            var rows =
                _taskClass.Performers.Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == taskId && p.Field<Int64>("WorkerID") == workerId);
            if (!rows.Any()) return false;
            var performer = rows.First();
            var enable = performer["StartDate"] != DBNull.Value && performer["CompletionDate"] == DBNull.Value;

            return enable;
        }

        private bool GetFillTimeTrackingEnable(int taskId, int workerId)
        {
            var rows =
                _taskClass.Performers.Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == taskId && p.Field<Int64>("WorkerID") == workerId);
            if (!rows.Any()) return false;
            var performer = rows.First();
            var enable = performer["StartDate"] != DBNull.Value;

            return enable;
        }

        private bool GetAddNewCommentEnable(int taskId, int workerId)
        {
            var performers =
                _taskClass.Performers.Table.Select(string.Format("TaskID = {0} AND WorkerID = {1}", taskId, workerId));
            if (performers.Any()) return true;

            var observers =
                _taskClass.Observers.Table.Select(string.Format("TaskID = {0} AND WorkerID = {1}", taskId, workerId));
            if (observers.Any()) return true;

            var tasks =
                _taskClass.Tasks.Table.Select(string.Format("TaskID = {0} AND MainWorkerID = {1}", taskId, workerId));
            return tasks.Any();
        }

        private bool GetAddingToObserversEnable(int taskId, int workerId)
        {
            var observers =
                _taskClass.Observers.Table.Select(string.Format("TaskID = {0} AND WorkerID = {1}", taskId, workerId));
            if (observers.Any())
            {
                return false;
            }

            var performers =
                _taskClass.Performers.Table.Select(string.Format("TaskID = {0} AND WorkerID = {1}", taskId, workerId));
            if (performers.Any())
            {
                return false;
            }

            var tasks =
                _taskClass.Tasks.Table.Select(string.Format("TaskID = {0} AND MainWorkerID = {1}", taskId, workerId));
            return !tasks.Any();
        }

        private void SetButtonsEnable()
        {
            SetTasksButtonsEnable();
            
            SetCommentButtonEnable();
            SetAddToObserverButtonEnable();
        }

        private void SetTasksButtonsEnable()
        {
            StartTaskEnable = GetStartTaskEnable((int)SelectedTaskId, _currentWorkerId);
            FinishTaskEnable = GetFinishTaskEnable((int)SelectedTaskId, _currentWorkerId);
            DeleteTaskEnable = SelectedTaskRow != null &&
                               Convert.ToInt64(SelectedTaskRow["MainWorkerID"]) == _currentWorkerId &&
                               Convert.ToBoolean(SelectedTaskRow["IsComplete"]) == false;
            FillTimeTrackingEnable = GetFillTimeTrackingEnable((int)SelectedTaskId, _currentWorkerId);
        }

        private void SetCommentButtonEnable()
        {
            AddNewCommentEnable = GetAddNewCommentEnable((int)SelectedTaskId, _currentWorkerId);
        }

        private void SetAddToObserverButtonEnable()
        {
            AddingToObserversEnable = SelectedTaskRow != null && FullAccess &&
                                      GetAddingToObserversEnable((int)SelectedTaskId, _currentWorkerId);
        }

        private void BeginTask()
        {
            if(SelectedTaskId < 1) return;

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _taskClass.Performers.StartTask(SelectedTaskId, _currentWorkerId, currentDate);
            _taskClass.Tasks.StartTask(SelectedTaskId);

            var globalId = SelectedTaskRow["GlobalID"].ToString();
            var senderAppId = Convert.ToInt32(SelectedTaskRow["SenderAppID"]);
            switch ((TaskClass.SenderApplications) senderAppId)
            {
                case TaskClass.SenderApplications.ServiceJournal:
                    ServiceEquipmentClass.ServiceResponsibilitiesClass.AcceptAndStart(globalId, _currentWorkerId,
                        currentDate);
                    break;
            }

            SetButtonsEnable();
        }

        private void CompleteTask()
        {
            if (SelectedTaskId < 1) return;

            var selectedTaskId = SelectedTaskId;

            var currentDate = App.BaseClass.GetDateFromSqlServer();
            _taskClass.Performers.EndTask(selectedTaskId, _currentWorkerId, currentDate);

            var globalId = SelectedTaskRow["GlobalID"].ToString();
            var senderAppId = Convert.ToInt32(SelectedTaskRow["SenderAppID"]);
            switch ((TaskClass.SenderApplications) senderAppId)
            {
                // If sender application is service journal, than complete journal row
                case TaskClass.SenderApplications.ServiceJournal:
                    ServiceEquipmentClass.ServiceResponsibilitiesClass.Complete(globalId, _currentWorkerId,
                        currentDate);
                    break;
            }

            // Complete task if every performers is allready completed
            if (_taskClass.Performers.Table.AsEnumerable().Where(p => p.Field<Int64>("TaskID") == selectedTaskId)
                .All(p => p.Field<bool>("IsComplete")))
            {
                _taskClass.Tasks.EndTask(selectedTaskId, currentDate);

                switch ((TaskClass.SenderApplications)senderAppId)
                {
                    // If sender application is service equipment, program will complete crash request
                    case TaskClass.SenderApplications.ServiceDamage:
                        {
                            ServiceEquipmentClass servEquipClass = null;
                            App.BaseClass.GetServiceEquipmentClass(ref servEquipClass);

                            if (servEquipClass != null)
                            {
                                servEquipClass.FillCompletionInfo(globalId, currentDate, _currentWorkerId, null);
                                AdministrationClass.AddNewAction(10);
                            }

                            break;
                        }
                    case TaskClass.SenderApplications.TechnologyProblem:
                        {
                            TechnologyProblemClass techProblemClass = null;
                            App.BaseClass.GetTechnologyProblemClass(ref techProblemClass);

                            if (techProblemClass != null)
                            {
                                techProblemClass.FillCompletionInfo(globalId, currentDate, _currentWorkerId);
                            }

                            break;
                        }
                    case TaskClass.SenderApplications.PlannedWorks:
                        {
                            PlannedWorksClass plannedWorksClass = null;
                            App.BaseClass.GetPlannedWorksClass(ref plannedWorksClass);
                            if (plannedWorksClass != null)
                            {
                                plannedWorksClass.FinishPlannedWorks(selectedTaskId);
                            }
                            break;
                        }
                }
            }


            // Hide completed tasks
            if (!ShowCompleted)
                FilterTaskView(_currentWorkerId, false);

            SetButtonsEnable();
        }

        private void DeleteSelectedTask()
        {
            if(SelectedTaskId < 1 || SelectedTaskRow == null) return;

            if (
                MessageBox.Show("Вы действительно хотите удалить выбранную задачу?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var mainWorkerId = Convert.ToInt32(SelectedTaskRow["MainWorkerID"]);
            var creationDate = Convert.ToDateTime(SelectedTaskRow["CreationDate"]);

            var senderAppId = Convert.ToInt32(SelectedTaskRow["SenderAppID"]);
            switch ((TaskClass.SenderApplications)senderAppId)
            {
                case TaskClass.SenderApplications.PlannedWorks:
                    {
                        PlannedWorksClass plannedWorksClass = null;
                        App.BaseClass.GetPlannedWorksClass(ref plannedWorksClass);
                        if (plannedWorksClass != null)
                        {
                            plannedWorksClass.DeleteStartedPlannedWorks(SelectedTaskId);
                            AdministrationClass.AddNewAction(57);
                        }
                        break;
                    }
            }

            _taskClass.DeleteTask((int) SelectedTaskId);
            NewsHelper.DeleteNews(creationDate, mainWorkerId);

            SetButtonsEnable();
        }

        public void UpdateTaskView()
        {
            if (WorkerFilteringEnable)
                FilterTaskView((int) SelectedWorkerId, ShowCompleted);
            else
                FilterTaskView(_currentWorkerId, ShowCompleted);
        }

        public void SelectTask(long taskId)
        {
            var custView = new DataView(_taskClass.Tasks.Table, "", "TaskID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(taskId);

            var selectedRow = foundRows.Any()
                ? foundRows.First()
                : null;

            SelectedTaskRow = selectedRow;
        }

        private void ShowFillTaskTimeTracking()
        {
            if (SelectedTaskId < 1) return;

            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (_timeTrackingClass.GetIsDayEnd(AdministrationClass.CurrentWorkerId))
            {
                if (mainWindow != null)
                {
                    mainWindow.BlinkWorkingDayButton();
                }
                return;
            }

            var rows =
                _taskClass.Performers.Table.AsEnumerable()
                    .Where(p => p.Field<Int64>("TaskID") == SelectedTaskId &&
                                p.Field<Int64>("WorkerID") == _currentWorkerId);
            if (!rows.Any()) return;
            var performer = rows.First();
            var performerId = Convert.ToInt32(performer["PerformerID"]);
            var startDate = Convert.ToDateTime(performer["StartDate"]);
            var completionDate = Convert.ToBoolean(performer["IsComplete"])
                ? Convert.ToDateTime(performer["CompletionDate"])
                : App.BaseClass.GetDateFromSqlServer();

            var fillTaskTimeTracking = new FillTaskTimeTracking(SelectedTaskId, performerId);
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(fillTaskTimeTracking,
                    string.Format("Задача №{0} ({1:dd.MM} - {2:dd.MM})", SelectedTaskRow["GlobalID"],
                        startDate,
                        completionDate));
            }
        }

        private void ShowTaskTimeTrackingInfo(object dataContext)
        {
            if (SelectedTaskRow == null || dataContext == null) return;

            var performer = (DataRowView) dataContext;

            var performerId = Convert.ToInt32(performer["PerformerID"]);
            var workerId = Convert.ToInt32(performer["WorkerID"]);
            var mainWorkerId = Convert.ToInt32(SelectedTaskRow["MainWorkerID"]);
            var fullAccess = mainWorkerId == _currentWorkerId;

            var taskTimeTrackingInfo = new TaskTimeTrackingInfo(performerId, fullAccess);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(taskTimeTrackingInfo,
                    string.Format("{0}  Задача №{1}",
                        new IdToNameConverter().Convert(workerId, typeof (string), "ShortName",
                            CultureInfo.InvariantCulture),
                        SelectedTaskRow["GlobalID"]));
            }
        }

        private void ShowTaskInfoDetails()
        {
            ShowTaskInfoDetailEnable = true;
        }

        private void BackToTasksList()
        {
            ShowTaskInfoDetailEnable = false;
            HideAddingCommentPanel();
        }

        private void FillNewsFeed()
        {
            if (SelectedTaskRow == null || SelectedTaskRow["GlobalID"] == DBNull.Value)
            {
                CommentsView = null;
                return;
            }

            var globalId = SelectedTaskRow["GlobalID"].ToString();
            _newsFeedClass.Fill(globalId);
            CommentsView = _newsFeedClass.Comments.AsDataView();
        }

        private void ExportToExcel()
        {
            Classes.ExportToExcel.GenerateTasksStatisticReport(!WorkerFilteringEnable
                ? _currentWorkerId
                : (int) SelectedWorkerId);
        }

        private void AddToObservers()
        {
            if (SelectedTaskId < 1) return;

            // Don't let add to observer if allready exist
            var observers =
                _taskClass.Observers.Table.Select(string.Format("TaskID = {0} AND WorkerID = {1}", SelectedTaskId,
                    _currentWorkerId));
            if (observers.Any()) return;

            _taskClass.AddNewObserver(SelectedTaskId, _currentWorkerId);
            SetAddToObserverButtonEnable();
            SetCommentButtonEnable();
        }



        #region Comments

        private void ShowAddingCommentPanel()
        {
            AddingCommentEnable = true;
        }

        private void HideAddingCommentPanel()
        {
            AddingCommentEnable = false;
            CommentText = string.Empty;

            // Delete attached files in ftp Temp directory
            foreach (var renamedFile in CommentAttachmentsView)
            {
                _ftpClient.DeleteFile(renamedFile.Renamed);
            }
            CommentAttachmentsView.Clear();
        }

        private void SelectAttachments()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            var ofd = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };

            if (!ofd.ShowDialog().Value) return;

            _uploadingFilesQueue.Clear();
            foreach (var fileName in ofd.FileNames)
            {
                _uploadingFilesQueue.Enqueue(fileName);
            }

            UploadQueueFile();
        }

        private static void OpenNewCommentAttachment(object dataContext)
        {
            if (dataContext == null) return;
            if (!(dataContext is RenamedFile)) return;

            var renamedFile = (RenamedFile)dataContext;

            try
            {
                Process.Start(renamedFile.OriginalName);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void DeleteNewCommentAttachment(object dataContext)
        {
            if (dataContext == null) return;
            if (!(dataContext is RenamedFile)) return;

            var renamedFile = (RenamedFile)dataContext;

            _ftpClient.DeleteFile(renamedFile.Renamed);
            CommentAttachmentsView.Remove(renamedFile);
        }

        private void AddNewComment()
        {
            if (SelectedTaskRow == null || SelectedTaskRow["GlobalID"] == DBNull.Value) return;

            if (string.IsNullOrEmpty(CommentText)) return;

            var globalId = SelectedTaskRow["GlobalID"].ToString();
            var rows = _newsFeedClass.News.Select(string.Format("GlobalID = '{0}'", globalId));
            if (!rows.Any()) return;

            var news = rows.First();
            var newsId = Convert.ToInt32(news["NewsID"]);
            var commentDate = App.BaseClass.GetDateFromSqlServer();

            var commentId =
                _newsFeedClass.AddComment(CommentText, commentDate, newsId, _currentWorkerId);
            _newsFeedClass.UpdateNewsLastEditing(newsId, commentDate);

            if (CommentAttachmentsView.Any())
            {
                AddCommentAttachments(commentId);
            }


            HideAddingCommentPanel();
        }

        private void AddCommentAttachments(int commentId)
        {
            if (commentId < 1) return;

            var commentsRows = _newsFeedClass.Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!commentsRows.Any()) return;

            var commentRow = commentsRows.First();
            var newsId = Convert.ToInt32(commentRow["NewsID"]);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if (commentPath == null) return;

            if (!_ftpClient.DirectoryExist(commentPath))
                _ftpClient.MakeDirectory(commentPath);

            foreach (var renamedCommentAttachment in CommentAttachmentsView)
            {
                var adress = GetDistinctFileName(renamedCommentAttachment.OriginalName, commentPath);

                var commentAttachmentName = Path.GetFileName(adress);
                _newsFeedClass.AddCommentAttachment(commentId, commentAttachmentName);

                var sourceUri = new Uri(renamedCommentAttachment.Renamed, UriKind.Absolute);
                var targetUri = new Uri(adress, UriKind.Absolute);
                var targetUriRelative = sourceUri.MakeRelativeUri(targetUri);

                _ftpClient.Rename(renamedCommentAttachment.Renamed,
                    Uri.UnescapeDataString(targetUriRelative.OriginalString));
            }

            CommentAttachmentsView.Clear();
        }

        private void DeleteComment(object dataContext)
        {
            if (SelectedTaskId < 1 || dataContext == null) return;

            var comment = dataContext as DataRowView;
            if (comment == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить комментарий?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            int commentId;
            Int32.TryParse(comment["CommentID"].ToString(), out commentId);
            int newsId;
            Int32.TryParse(comment["NewsID"].ToString(), out newsId);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if (commentPath != null)
            {
                if (_ftpClient.DirectoryExist(commentPath))
                {
                    _ftpClient.CurrentPath = commentPath;
                    var filesList = _ftpClient.ListDirectory();
                    foreach (var file in filesList)
                    {
                        // Delete all comment attachments on ftp-server
                        _ftpClient.DeleteFile(string.Concat(commentPath, file));
                    }
                }
            }

            // Delete all attachments rows, that comment includes
            _newsFeedClass.DeleteCommentAttachments(commentId);

            _newsFeedClass.DeleteComment(commentId);
        }

        private void EditComment(object dataContext)
        {
            if (SelectedTaskId < 1 || dataContext == null) return;

            var comment = dataContext as DataRowView;
            if (comment == null) return;

            var editCommentPage = new EditNewsAndCommentPage(comment, EditNewsAndCommentPage.EditMode.Comment);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowCatalogGrid(editCommentPage, "Редактировать комментарий");
            }
        }

        private void DownloadAndOpenCommentAttachment(object dataContext)
        {
            if (SelectedTaskId < 1 || dataContext == null) return;

            var attachment = dataContext as DataRowView;
            if(attachment == null) return;

            var commentAttachmentName = attachment["CommentAttachmentName"].ToString();

            int commentId;
            Int32.TryParse(attachment["CommentID"].ToString(), out commentId);

            var commentRows = _newsFeedClass.Comments.Select(string.Format("CommentID = {0}", commentId));
            if (!commentRows.Any()) return;

            var commentRow = commentRows.First();
            var newsId = Convert.ToInt32(commentRow["NewsID"]);

            var commentPath = GetCommentAttachmentPath(newsId, commentId);
            if (commentPath == null) return;

            var tempPath = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId, "Комментарий_[id]" + commentId,
                commentAttachmentName);
            var tempDirectory = Path.Combine(App.TempFolder, "Сообщение_[id]" + newsId, "Комментарий_[id]" + commentId);
            if (File.Exists(tempPath))
            {
                try
                {
                    Process.Start(tempPath);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
                return;
            }

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            var filePath = string.Concat(commentPath, "/", commentAttachmentName);
            if (_ftpClient.FileExist(filePath))
            {
                _neededOpeningFilePath = tempPath;
                var uri = new Uri(filePath);

                if (_processWindow != null)
                    _processWindow.Close(true);

                _processWindow = new WaitWindow { Text = "Загрузка файла..." };
                _processWindow.Show(Window.GetWindow(Application.Current.MainWindow), true);

                _fileSize = _ftpClient.GetFileSize(filePath);
                _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
                _ftpClient.DownloadFileAsync(uri, tempPath);
            }
            else
            {
                MessageBox.Show("Невозможно найти указанный файл, возможно он был удалён ранее.");
            }
        }

        #endregion



        #region Ftp

        private void UploadQueueFile()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            while (true)
            {
                CommentFileUploadingProgress = 0;

                if (!_uploadingFilesQueue.Any()) return;

                var filePath = _uploadingFilesQueue.Dequeue();

                var fileUploadingEnable = GetFileUploadingEnable(filePath);
                if (!fileUploadingEnable)
                {
                    UploadQueueFile();
                    return;
                }

                // Get distinct file name in Temp directory
                var adress = GetDistinctFileName(filePath, _tempDirectory);

                CommentAttachmentsView.Add(new RenamedFile(filePath, adress));
                var uri = new Uri(adress);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadFileCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }
        }

        private string GetDistinctFileName(string filePath, string parrentDirectory)
        {
            var j = 1;
            var originalFileName = Path.GetFileNameWithoutExtension(filePath);
            var fileName = originalFileName;
            var fileExtension = Path.GetExtension(filePath);

            var adress = string.Concat(parrentDirectory, fileName + fileExtension);
            while (_ftpClient.FileExist(adress))
            {
                fileName = originalFileName;
                fileName += "(" + j + ")";
                j++;
                adress = string.Concat(parrentDirectory, fileName + fileExtension);
            }

            return adress;
        }

        private static bool GetFileUploadingEnable(string filePath)
        {
            var fileLocked = AdministrationClass.IsFileLocked(new FileInfo(filePath));
            while (fileLocked)
            {
                var message =
                    string.Format(
                        "Файл {0} невозможно прикрепить. " +
                        "\nФайл заблокирован другим процессом." +
                        "\nПродолжить отправку без данного файла?",
                        filePath);
                if (MessageBox.Show(message, "Файл заблокирован", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    return false;
                }

                fileLocked = AdministrationClass.IsFileLocked(new FileInfo(filePath));
            }

            return true;
        }

        private string GetCommentAttachmentPath(int newsId, int commentId)
        {
            var newsRows = _newsFeedClass.News.Select(string.Format("NewsID = {0}", newsId));
            if (!newsRows.Any()) return null;

            // Get news status name, and make basic parrent directory path
            var newsStatusId = Convert.ToInt32(newsRows.First()["NewsStatus"]);
            var newsStatusName = new IdToNewsStatusConverter().Convert(newsStatusId, typeof(string),
                "NewsStatusName", CultureInfo.InvariantCulture).ToString();

            var newsDir = string.Format("Сообщение_[id]{0}", newsId);
            var commentDir = string.Format("Комментарий_[id]{0}", commentId);

            var commentPath = string.Concat(_basicDirectory, newsStatusName, "/", newsDir,
                "/", commentDir, "/");

            return commentPath;
        }

        private void OnFtpClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var progress = (e.BytesSent / (double)e.TotalBytesToSend) * 100;

            CommentFileUploadingProgress = progress;
        }

        private void OnFtpClientUploadFileCompleted(object sender,
            UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadFileCompleted;

            UploadQueueFile();
        }

        private void OnFtpClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.BytesReceived/(double) _fileSize;
            if (_processWindow == null) return;

            _processWindow.Progress = progress*100;
            _processWindow.Text = string.Format("Загрузка файла... \n{0} кБ", e.BytesReceived/1024);
        }

        private void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (_processWindow != null)
                _processWindow.Text = "Открытие файла...";

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
            _processWindow = null;

            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;
        }

        

        #endregion



        #region Variables


        #region Views and lists


        private DataView _workerGroupsView;
        public DataView WorkerGroupsView
        {
            get { return _workerGroupsView; }
            set
            {
                _workerGroupsView = value;
                RaisePropertyChanged("WorkerGroupsView");
            }
        }


        private DataView _factoriesView;
        public DataView FactoriesView
        {
            get { return _factoriesView; }
            set
            {
                _factoriesView = value;
                RaisePropertyChanged("FactoriesView");
            }
        }


        private DataView _workersView;
        public DataView WorkersView
        {
            get { return _workersView; }
            set
            {
                _workersView = value;
                RaisePropertyChanged("WorkersView");
            }
        }


        private DataView _tasksView;
        public DataView TasksView
        {
            get { return _tasksView; }
            set
            {
                _tasksView = value;
                RaisePropertyChanged("TasksView");

                if (value != null)
                {
                    _tasksView.Sort = "IsComplete ASC, TaskStatusID ASC, CreationDate ASC";
                }
            }
        }


        private DataView _performersView;
        public DataView PerformerView
        {
            get { return _performersView; }
            set
            {
                _performersView = value;
                RaisePropertyChanged("PerformerView");
            }
        }


        private DataView _observersView;
        public DataView ObserversView
        {
            get { return _observersView; }
            set
            {
                _observersView = value;
                RaisePropertyChanged("ObserversView");
            }
        }


        private DataView _commentsView;
        public DataView CommentsView
        {
            get { return _commentsView; }
            set
            {
                _commentsView = value;
                RaisePropertyChanged("CommentsView");
            }
        }


        private ObservableCollection<RenamedFile> _commentAttachmentsView;
        public ObservableCollection<RenamedFile> CommentAttachmentsView
        {
            get { return _commentAttachmentsView; }
            set
            {
                _commentAttachmentsView = value;
                RaisePropertyChanged("CommentAttachmentsView");
            }
        }


        #endregion




        #region Selected items and values


        private Int64 _selectedWorkerGroupId;
        public Int64 SelectedWorkerGroupId
        {
            get { return _selectedWorkerGroupId; }
            set
            {
                _selectedWorkerGroupId = value;
                RaisePropertyChanged("SelectedWorkerGroupId");

                FilterWorkers();
            }
        }


        private Int64 _selectedFactoryId;
        public Int64 SelectedFactoryId
        {
            get { return _selectedFactoryId; }
            set
            {
                _selectedFactoryId = value;
                RaisePropertyChanged("SelectedFactoryId");

                FilterWorkers();
            }
        }


        private Int64 _selectedWorkerId;
        public Int64 SelectedWorkerId
        {
            get { return _selectedWorkerId; }
            set
            {
                _selectedWorkerId = value;
                RaisePropertyChanged("SelectedWorkerId");

                if (!WorkerFilteringEnable) return;

                _taskClass.Fill(_currentDateFrom, _currentDateTo, (int) value);
                FilterTaskView((int) value, ShowCompleted);
                BackToTasksList();
            }
        }


        private Int64 _selectedTaskId;
        public Int64 SelectedTaskId
        {
            get { return _selectedTaskId; }
            set
            {
                _selectedTaskId = value;
                RaisePropertyChanged("SelectedTaskId");

                if (PerformerView != null)
                    PerformerView.RowFilter = string.Format("TaskID = {0}", value);

                if (ObserversView != null)
                    ObserversView.RowFilter = string.Format("TaskID = {0}", value);
            }
        }


        private DataRowView _selectedTaskRow;
        public DataRowView SelectedTaskRow
        {
            get { return _selectedTaskRow; }
            set
            {
                _selectedTaskRow = value;
                RaisePropertyChanged("SelectedTaskRow");

                SelectedTaskId = value != null
                    ? Convert.ToInt64(value["TaskID"])
                    : 0;

                SetButtonsEnable();
                FillNewsFeed();
            }
        }


        #endregion




        #region Enables and states


        private bool _workerFilteringEnable;
        public bool WorkerFilteringEnable
        {
            get { return _workerFilteringEnable; }
            set
            {
                _workerFilteringEnable = value;
                RaisePropertyChanged("WorkerFilteringEnable");

                if (!value)
                {
                    SelectedWorkerId = 0;
                    _taskClass.Fill(_currentDateFrom, _currentDateTo, _currentWorkerId);
                    FilterTaskView(_currentWorkerId, ShowCompleted);
                    BackToTasksList();
                }
                else
                {
                    SelectedWorkerId = WorkersView != null && WorkersView.Count != 0
                        ? Convert.ToInt64(WorkersView[0]["WorkerID"])
                        : 0;
                }
            }
        }


        private bool _isCharged;
        public bool IsCharged
        {
            get { return _isCharged; }
            set
            {
                _isCharged = value;
                RaisePropertyChanged("IsCharged");

                if (value)
                {
                    _filteringType = FilteringType.IsCharged;

                    if (WorkerFilteringEnable)
                        FilterTaskView((int) SelectedWorkerId, ShowCompleted);
                    else
                        FilterTaskView(_currentWorkerId, ShowCompleted);

                    BackToTasksList();
                }
            }
        }


        private bool _isPerformed;
        public bool IsPerformed
        {
            get { return _isPerformed; }
            set
            {
                _isPerformed = value;
                RaisePropertyChanged("IsPerformed");

                if (value)
                {
                    _filteringType = FilteringType.IsPerformed;

                    if (WorkerFilteringEnable)
                        FilterTaskView((int) SelectedWorkerId, ShowCompleted);
                    else
                        FilterTaskView( _currentWorkerId, ShowCompleted);

                    BackToTasksList();
                }
            }
        }


        private bool _isObserved;
        public bool IsObserved
        {
            get { return _isObserved; }
            set
            {
                _isObserved = value;
                RaisePropertyChanged("IsObserved");

                if (value)
                {
                    _filteringType = FilteringType.IsObserved;

                    if (WorkerFilteringEnable)
                        FilterTaskView((int)SelectedWorkerId, ShowCompleted);
                    else
                        FilterTaskView(_currentWorkerId, ShowCompleted);

                    BackToTasksList();
                }
            }
        }


        private bool _showCompleted;
        public bool ShowCompleted
        {
            get { return _showCompleted; }
            set
            {
                _showCompleted = value;
                RaisePropertyChanged("ShowCompleted");

                if (WorkerFilteringEnable)
                    FilterTaskView((int) SelectedWorkerId, value);
                else
                    FilterTaskView(_currentWorkerId, value);
            }
        }


        private bool _startTaskEnable;
        public bool StartTaskEnable
        {
            get { return _startTaskEnable; }
            set
            {
                _startTaskEnable = value;
                RaisePropertyChanged("StartTaskEnable");
            }
        }


        private bool _finishTaskEnable;
        public bool FinishTaskEnable
        {
            get { return _finishTaskEnable; }
            set
            {
                _finishTaskEnable = value;
                RaisePropertyChanged("FinishTaskEnable");
            }
        }


        private bool _deleteTaskEnable;
        public bool DeleteTaskEnable
        {
            get { return _deleteTaskEnable; }
            set
            {
                _deleteTaskEnable = value;
                RaisePropertyChanged("DeleteTaskEnable");
            }
        }


        private bool _fillTimeTrackingEnable;
        public bool FillTimeTrackingEnable
        {
            get { return _fillTimeTrackingEnable; }
            set
            {
                _fillTimeTrackingEnable = value;
                RaisePropertyChanged("FillTimeTrackingEnable");
            }
        }


        private bool _fullAccess;
        public bool FullAccess
        {
            get { return _fullAccess; }
            set
            {
                _fullAccess = value;
                RaisePropertyChanged("FullAccess");
            }
        }


        private bool _showTaskDetailEnable;
        public bool ShowTaskInfoDetailEnable
        {
            get { return _showTaskDetailEnable; }
            set
            {
                _showTaskDetailEnable = value;
                RaisePropertyChanged("ShowTaskInfoDetailEnable");
            }
        }


        private bool _addNewCommentEnable;
        public bool AddNewCommentEnable
        {
            get { return _addNewCommentEnable; }
            set
            {
                _addNewCommentEnable = value;
                RaisePropertyChanged("AddNewCommentEnable");
            }
        }


        private bool _addingCommentEnable;
        public bool AddingCommentEnable
        {
            get { return _addingCommentEnable; }
            set
            {
                _addingCommentEnable = value;
                RaisePropertyChanged("AddingCommentEnable");
            }
        }


        private bool _commentFileIsUploading;
        public bool CommentFileIsUploading
        {
            get { return _commentFileIsUploading; }
            set
            {
                _commentFileIsUploading = value;
                RaisePropertyChanged("CommentFileIsUploading");
            }
        }


        private bool _addingToObserversEnable;
        public bool AddingToObserversEnable
        {
            get { return _addingToObserversEnable; }
            set
            {
                _addingToObserversEnable = value;
                RaisePropertyChanged("AddingToObserversEnable");
            }
        }


        #endregion




        #region Additional


        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                _dateFrom = value;
                RaisePropertyChanged("DateFrom");
            }
        }


        private DateTime _dateTo;
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                _dateTo = value;
                RaisePropertyChanged("DateTo");
            }
        }


        private string _commentText;
        public string CommentText
        {
            get { return _commentText; }
            set
            {
                _commentText = value;
                RaisePropertyChanged("CommentText");
            }
        }


        private double _commentFileUploadingProgress;
        public double CommentFileUploadingProgress
        {
            get { return _commentFileUploadingProgress; }
            set
            {
                _commentFileUploadingProgress = value;
                RaisePropertyChanged("CommentFileUploadingProgress");

                CommentFileIsUploading = Math.Abs(value) > 0;
            }
        }


        #endregion


        #endregion



        #region Commands

        public ActionCommand Fill { get; set; }

        public ActionCommand NewTask { get; set; }

        public ActionCommand StartTask { get; set; }

        public ActionCommand EndTask { get; set; }

        public ActionCommand DeleteTask { get; set; }

        public ActionCommand EditTask { get; set; }

        public ActionCommand FillTimeTracking { get; set; }

        public ActionCommand TaskTimeTrackingInfo { get; set; }

        public ActionCommand ShowTaskDetails { get; set; }

        public ActionCommand BackToTasksListCommand { get; set; }

        public ActionCommand ShowAddingCommentPanelCommand { get; set; }

        public ActionCommand HideAddingCommentPanelCommand { get; set; }

        public ActionCommand AddNewCommentCommand { get; set; }

        public ActionCommand SelectAttachmentsCommand { get; set; }

        public ActionCommand DeleteCommentCommand { get; set; }

        public ActionCommand EditCommentCommand { get; set; }

        public ActionCommand DownloadAndOpenCommentAttachmentCommand { get; set; }

        public ActionCommand OpenNewCommentAttachmentCommand { get; set; }

        public ActionCommand DeleteNewCommentAttachmentCommand { get; set; }

        public ActionCommand ExportToExcelCommand { get; set; }

        public ActionCommand AddToObserversCommand { get; set; }

        #endregion



        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
