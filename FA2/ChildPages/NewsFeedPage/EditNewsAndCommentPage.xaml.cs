using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using FA2.Converters;
using FA2.Ftp;
using FA2.XamlFiles;
using Microsoft.Win32;

namespace FA2.ChildPages.NewsFeedPage
{
    /// <summary>
    /// Логика взаимодействия для EditComment.xaml
    /// </summary>
    public partial class EditNewsAndCommentPage
    {
        public enum EditMode
        {
            News,
            Comment
        }

        private readonly EditMode _editMode;
        private readonly int _commentId;
        private readonly int _newsId;

        private NewsFeedClass _newsFeedClass;

        private FtpClient _ftpClient;
        private string _basicDirectory;
        private string _tempDirectory;

        private readonly Queue<string> _uploadNewNewsAttachments = new Queue<string>();
        private readonly List<RenamedFile> _renamedNewNewsAttachments = new List<RenamedFile>();
        private readonly Queue<string> _uploadNewCommentAttachments = new Queue<string>();
        private readonly List<RenamedFile> _renamedNewCommentAttachments = new List<RenamedFile>();

        private struct RenamedFile
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

        public EditNewsAndCommentPage(object editedData, EditMode editMode)
        {
            InitializeComponent();

            if (!(editedData is DataRowView) && !(editedData is DataRow))
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                EditTextBox.IsReadOnly = true;
                AddNewAttachmentsButton.Visibility = Visibility.Collapsed;
                AttachmentsItemsControl.IsEnabled = false;
                return;
            }

            _editMode = editMode;
            FillData();
            DataContext = editedData;

            var view = editedData as DataRowView;

            if (_editMode == EditMode.News)
            {
                HeaderTextBlock.Text = "Текст сообщения";
                EditTextBox.Text = new NewsTextConverter().Convert(view != null
                    ? view["NewsText"]
                    : ((DataRow) editedData)["NewsText"], typeof (string), "JustText",
                    CultureInfo.InvariantCulture).ToString();

                _newsId = Convert.ToInt32(view != null
                    ? view["NewsID"]
                    : ((DataRow) editedData)["NewsID"]);

                var newsAttachmentTemplate = Resources["EditAttachmenTemplate"] as DataTemplate;
                if (newsAttachmentTemplate != null)
                    AttachmentsItemsControl.ItemTemplate = newsAttachmentTemplate;

                SetNewsAttachmentsItemsSource();
            }
            else
            {
                HeaderTextBlock.Text = "Текст комментария";
                EditTextBox.Text = new NewsTextConverter().Convert(view != null
                    ? view["CommentText"]
                    : ((DataRow) editedData)["CommentText"], typeof (string), "JustText",
                    CultureInfo.InvariantCulture).ToString();

                _commentId = Convert.ToInt32(view != null
                    ? view["CommentID"]
                    : ((DataRow) editedData)["CommentID"]);

                _newsId = Convert.ToInt32(view != null
                    ? view["NewsID"]
                    : ((DataRow) editedData)["NewsID"]);

                var commentAttachmentTemplate = Resources["EditCommentAttachmenTemplate"] as DataTemplate;
                if (commentAttachmentTemplate != null)
                    AttachmentsItemsControl.ItemTemplate = commentAttachmentTemplate;

                SetCommentAttachmentsItemsSource();
            }

            SetFtpData();
        }

        private void FillData()
        {
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);
        }

        private void SetNewsAttachmentsItemsSource()
        {
            var attachments =
                _newsFeedClass.Attachments.AsEnumerable().Where(a => a.Field<Int64>("NewsID") == _newsId);
            var attachmentsTable = attachments.Any()
                ? attachments.CopyToDataTable()
                : _newsFeedClass.Attachments.Clone();

            var deleteNeededColumn = new DataColumn("DeleteNeeded", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = false
            };
            var isEditedColumn = new DataColumn("IsEdited", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = true
            };
            var addNeededColumn = new DataColumn("AddNeeded", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = false
            };

            attachmentsTable.Columns.Add(deleteNeededColumn);
            attachmentsTable.Columns.Add(isEditedColumn);
            attachmentsTable.Columns.Add(addNeededColumn);
            AttachmentsItemsControl.ItemsSource = attachmentsTable.AsDataView();
        }

        private void SetCommentAttachmentsItemsSource()
        {
            var commentAttachments =
                _newsFeedClass.CommentsAttachments.AsEnumerable().
                Where(a => a.Field<Int64>("CommentID") == _commentId);

            var commentAttachmentsTable = commentAttachments.Any()
                ? commentAttachments.CopyToDataTable()
                : _newsFeedClass.CommentsAttachments.Clone();

            var deleteNeededColumn = new DataColumn("DeleteNeeded", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = false
            };
            var isEditedColumn = new DataColumn("IsEdited", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = true
            };
            var addNeededColumn = new DataColumn("AddNeeded", typeof(bool))
            {
                AllowDBNull = false,
                DefaultValue = false
            };

            commentAttachmentsTable.Columns.Add(deleteNeededColumn);
            commentAttachmentsTable.Columns.Add(isEditedColumn);
            commentAttachmentsTable.Columns.Add(addNeededColumn);
            AttachmentsItemsControl.ItemsSource = commentAttachmentsTable.AsDataView();
        }

        private void OnAddNewAttachmentsButtonClick(object sender, RoutedEventArgs e)
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

            if (_editMode == EditMode.News)
            {
                _uploadNewNewsAttachments.Clear();
                foreach (var fileName in ofd.FileNames)
                {
                    _uploadNewNewsAttachments.Enqueue(fileName);
                }

                UploadNewAttachmentsInQueue();
            }
            else
            {
                _uploadNewCommentAttachments.Clear();
                foreach (var fileName in ofd.FileNames)
                {
                    _uploadNewCommentAttachments.Enqueue(fileName);
                }

                UploadNewCommentAttachmentsInQueue();
            }
        }

        private void OnSaveCommentButtonClick(object sender, RoutedEventArgs e)
        {
            var currentTime = App.BaseClass.GetDateFromSqlServer();
            var editInfoText = !string.IsNullOrEmpty(EditTextBox.Text)
                ? string.Format("Последнее редактирование {0:dd.MM.yyyy HH:mm}",
                    currentTime)
                : string.Format("Текст сообщения удалён отправителем {0:dd.MM.yyyy HH:mm}",
                    currentTime);
            var editText = EditTextBox.Text + "[Edit]" + editInfoText;

            if (_editMode == EditMode.News)
            {
                _newsFeedClass.ChangeNewsText(_newsId, editText);
                AdministrationClass.AddNewAction(95);

                var newsAttachmentsView = AttachmentsItemsControl.ItemsSource as DataView;

                #region NewsAttachments

                // Working with attachments if attachment list is not empty
                if (newsAttachmentsView != null && newsAttachmentsView.Count != 0)
                {
                    var newsPath = GetNewsPath(_newsId);
                    if (newsPath == null) return;

                    // Parrent directory doesnt exist
                    if (!_ftpClient.DirectoryExist(newsPath))
                    {
                        // Create news directory
                        _ftpClient.MakeDirectory(newsPath);
                    }

                    // Delete all marked attachments
                    foreach (var deleteNeededAttachment in
                        newsAttachmentsView.Table.AsEnumerable().Where(a => a.Field<bool>("DeleteNeeded")))
                    {
                        int attachmentId;
                        Int32.TryParse(deleteNeededAttachment["AttachmentID"].ToString(), out attachmentId);
                        var attachmentName = deleteNeededAttachment["AttachmentName"].ToString();

                        _newsFeedClass.DeleteAttachment(attachmentId);
                        _ftpClient.DeleteFile(string.Concat(newsPath, attachmentName));
                    }

                    foreach (
                        var addNeededAttachment in
                            newsAttachmentsView.Table.AsEnumerable().Where(a => a.Field<bool>("AddNeeded")))
                    {
                        var attachmentName = addNeededAttachment["AttachmentName"].ToString();
                        if (_renamedNewNewsAttachments.Any(a => a.OriginalName == attachmentName))
                        {
                            var renamedNewAttachment =
                                _renamedNewNewsAttachments.First(a => a.OriginalName == attachmentName);
                            var adress = GetDistinctFileName(renamedNewAttachment.OriginalName, newsPath);

                            _newsFeedClass.AddAttachment(_newsId, Path.GetFileName(adress));

                            var sourceUri = new Uri(renamedNewAttachment.Renamed, UriKind.Absolute);
                            var targetUri = new Uri(adress, UriKind.Absolute);
                            var targetUriRelative = sourceUri.MakeRelativeUri(targetUri);

                            _ftpClient.Rename(renamedNewAttachment.Renamed,
                                Uri.UnescapeDataString(targetUriRelative.OriginalString));

                            _renamedNewNewsAttachments.Remove(renamedNewAttachment);
                        }
                    }

                    _renamedNewNewsAttachments.Clear();
                }

                #endregion
            }
            else
            {
                _newsFeedClass.ChangeCommentText(_commentId, editText);
                AdministrationClass.AddNewAction(98);

                var commentAttachmentsView = AttachmentsItemsControl.ItemsSource as DataView;

                #region CommentAttachments

                // Working with attachments if attachment list is not empty
                if (commentAttachmentsView != null && commentAttachmentsView.Count != 0)
                {
                    var commentPath = GetCommentAttachmentPath(_newsId, _commentId);
                    if (commentPath == null) return;

                    // Parrent directory doesnt exist
                    if (!_ftpClient.DirectoryExist(commentPath))
                    {
                        // Create news directory
                        _ftpClient.MakeDirectory(commentPath);
                    }

                    // Delete all marked attachments
                    foreach (var deleteNeededAttachment in
                        commentAttachmentsView.Table.AsEnumerable().Where(a => a.Field<bool>("DeleteNeeded")))
                    {
                        int commentAttachmentId;
                        Int32.TryParse(deleteNeededAttachment["CommentAttachmentID"].ToString(), out commentAttachmentId);
                        var commentAttachmentName = deleteNeededAttachment["CommentAttachmentName"].ToString();

                        _newsFeedClass.DeleteCommentAttachment(commentAttachmentId);
                        _ftpClient.DeleteFile(string.Concat(commentPath, commentAttachmentName));
                    }

                    foreach (
                        var addNeededAttachment in
                            commentAttachmentsView.Table.AsEnumerable().Where(a => a.Field<bool>("AddNeeded")))
                    {
                        var commentAttachmentName = addNeededAttachment["CommentAttachmentName"].ToString();
                        if (_renamedNewCommentAttachments.Any(a => a.OriginalName == commentAttachmentName))
                        {
                            var renamedNewAttachment =
                                _renamedNewCommentAttachments.First(a => a.OriginalName == commentAttachmentName);
                            var adress = GetDistinctFileName(renamedNewAttachment.OriginalName, commentPath);

                            _newsFeedClass.AddCommentAttachment(_commentId, Path.GetFileName(adress));

                            var sourceUri = new Uri(renamedNewAttachment.Renamed, UriKind.Absolute);
                            var targetUri = new Uri(adress, UriKind.Absolute);
                            var targetUriRelative = sourceUri.MakeRelativeUri(targetUri);

                            _ftpClient.Rename(renamedNewAttachment.Renamed,
                                Uri.UnescapeDataString(targetUriRelative.OriginalString));

                            _renamedNewCommentAttachments.Remove(renamedNewAttachment);
                        }
                    }

                    _renamedNewCommentAttachments.Clear();
                }

                #endregion
            }

            OnClosePageButtonClick(null, null);
        }

        private void OnClosePageButtonClick(object sender, RoutedEventArgs e)
        {
            // Delete attached files in ftp Temp directory
            if (_editMode == EditMode.News)
            {
                foreach (var renamedFile in _renamedNewNewsAttachments)
                {
                    _ftpClient.DeleteFile(renamedFile.Renamed);
                }
                _renamedNewNewsAttachments.Clear();
            }
            else
            {
                
                foreach (var renamedFile in _renamedNewCommentAttachments)
                {
                    _ftpClient.DeleteFile(renamedFile.Renamed);
                }
                _renamedNewCommentAttachments.Clear();
            }


            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }



        #region AttachmentsEvents

        private void OnCancelAddingCommentAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var cancelAddingCommentAttachmentButton = sender as Button;
            if (cancelAddingCommentAttachmentButton == null) return;

            var newCommentAttachment = cancelAddingCommentAttachmentButton.DataContext as DataRowView;
            if (newCommentAttachment == null) return;

            var attachmentName = newCommentAttachment["CommentAttachmentName"].ToString();
            if (_renamedNewCommentAttachments.Any(a => a.OriginalName == attachmentName))
            {
                var renamedNewCommentAttachment =
                    _renamedNewCommentAttachments.First(a => a.OriginalName == attachmentName);
                _ftpClient.DeleteFile(renamedNewCommentAttachment.Renamed);
                _renamedNewCommentAttachments.Remove(renamedNewCommentAttachment);
            }

            ((DataView) AttachmentsItemsControl.ItemsSource).Table.Rows.Remove(newCommentAttachment.Row);
        }

        private void OnDeleteCommentAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteCommentAttachmentButton = sender as Button;
            if (deleteCommentAttachmentButton == null) return;

            var commentAttachment = deleteCommentAttachmentButton.DataContext as DataRowView;
            if (commentAttachment == null) return;

            commentAttachment["DeleteNeeded"] = true;
            commentAttachment["IsEdited"] = false;
            commentAttachment["AddNeeded"] = false;
        }

        private void OnCancelDeletingCommentAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var cancelDeletingCommentAttachmentButton = sender as Button;
            if (cancelDeletingCommentAttachmentButton == null) return;

            var commentAttachment = cancelDeletingCommentAttachmentButton.DataContext as DataRowView;
            if (commentAttachment == null) return;

            commentAttachment["DeleteNeeded"] = false;
            commentAttachment["IsEdited"] = true;
            commentAttachment["AddNeeded"] = false;
        }

        private void OnDeleteFileButtonClick(object sender, RoutedEventArgs e)
        {
            var deleteAttachmentButton = sender as Button;
            if (deleteAttachmentButton == null) return;

            var attachment = deleteAttachmentButton.DataContext as DataRowView;
            if (attachment == null) return;

            attachment["DeleteNeeded"] = true;
            attachment["IsEdited"] = false;
            attachment["AddNeeded"] = false;
        }

        private void OnCancelDeletingAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var cancelDeletingAttachmentButton = sender as Button;
            if (cancelDeletingAttachmentButton == null) return;

            var attachment = cancelDeletingAttachmentButton.DataContext as DataRowView;
            if (attachment == null) return;

            attachment["DeleteNeeded"] = false;
            attachment["IsEdited"] = true;
            attachment["AddNeeded"] = false;
        }

        private void OnCancelAddingAttachmentButtonClick(object sender, RoutedEventArgs e)
        {
            var cancelAddingAttachmentButton = sender as Button;
            if (cancelAddingAttachmentButton == null) return;

            var newAttachment = cancelAddingAttachmentButton.DataContext as DataRowView;
            if (newAttachment == null) return;

            var attachmentName = newAttachment["AttachmentName"].ToString();
            if (_renamedNewNewsAttachments.Any(a => a.OriginalName == attachmentName))
            {
                var renamedNewAttachment = _renamedNewNewsAttachments.First(a => a.OriginalName == attachmentName);
                _ftpClient.DeleteFile(renamedNewAttachment.Renamed);
                _renamedNewNewsAttachments.Remove(renamedNewAttachment);
            }

            ((DataView)AttachmentsItemsControl.ItemsSource).Table.Rows.Remove(newAttachment.Row);
        }

        #endregion



        #region Ftp

        private void SetFtpData()
        {
            _basicDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Файлы живой ленты/";
            _tempDirectory = App.GetFtpUrl + @"FtpFaII/FAIIFileStorage/Temp/";

            _ftpClient = new FtpClient(_basicDirectory, "fa2app", "Franc1961");
            _ftpClient.UploadProgressChanged += OnFtpClientUploadProgressChanged;

            //можно выкл
            //----------------------------------------------------
            if (!_ftpClient.DirectoryExist(_basicDirectory))
                _ftpClient.MakeDirectory(_basicDirectory);
            if (!_ftpClient.DirectoryExist(_tempDirectory))
                _ftpClient.MakeDirectory(_tempDirectory);
            //----------------------------------------------------
        }

        private void UploadNewAttachmentsInQueue()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            while (true)
            {
                if (!_uploadNewNewsAttachments.Any()) return;

                var filePath = _uploadNewNewsAttachments.Dequeue();

                var fileUploadingEnable = GetFileUploadingEnable(filePath);
                if (!fileUploadingEnable)
                {
                    UploadNewAttachmentsInQueue();
                    return;
                }

                SaveButton.IsEnabled = false;
                ClosePageButton.IsEnabled = false;

                EditAttachmentsProgressBar.Value = 0;
                EditUploadingStatusPanel.Visibility = Visibility.Visible;

                // Get distinct file name in Temp directory
                var adress = GetDistinctFileName(filePath, _tempDirectory);

                _renamedNewNewsAttachments.Add(new RenamedFile(filePath, adress));
                var uri = new Uri(adress);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }
        }

        private void UploadNewCommentAttachmentsInQueue()
        {
            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку. Попробуйте позже");
                return;
            }

            while (true)
            {
                if (!_uploadNewCommentAttachments.Any()) return;

                var filePath = _uploadNewCommentAttachments.Dequeue();

                var fileUploadingEnable = GetFileUploadingEnable(filePath);
                if (!fileUploadingEnable)
                {
                    UploadNewCommentAttachmentsInQueue();
                    return;
                }

                SaveButton.IsEnabled = false;
                ClosePageButton.IsEnabled = false;

                EditAttachmentsProgressBar.Value = 0;
                EditUploadingStatusPanel.Visibility = Visibility.Visible;

                // Get distinct file name in Temp directory
                var adress = GetDistinctFileName(filePath, _tempDirectory);

                _renamedNewCommentAttachments.Add(new RenamedFile(filePath, adress));
                var uri = new Uri(adress);

                _ftpClient.UploadFileCompleted += OnFtpClientUploadCompleted;
                _ftpClient.UploadFileAsync(uri, "STOR", filePath);
                break;
            }
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

        private void OnFtpClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var progress = (e.BytesSent / (double)e.TotalBytesToSend) * 100;
            EditAttachmentsProgressBar.Value = progress;
        }

        private void OnFtpClientUploadCompleted(object sender,
            UploadFileCompletedEventArgs uploadFileCompletedEventArgs)
        {
            _ftpClient.UploadFileCompleted -= OnFtpClientUploadCompleted;

            SaveButton.IsEnabled = true;
            ClosePageButton.IsEnabled = true;
            EditUploadingStatusPanel.Visibility = Visibility.Collapsed;

            if (_editMode == EditMode.News)
            {
                var attachmetnsTable = ((DataView)AttachmentsItemsControl.ItemsSource).Table;
                var newRow = attachmetnsTable.NewRow();
                newRow["AttachmentName"] = _renamedNewNewsAttachments.Last().OriginalName;
                newRow["IsEdited"] = false;
                newRow["AddNeeded"] = true;
                attachmetnsTable.Rows.Add(newRow);

                UploadNewAttachmentsInQueue();
            }
            else
            {
                var commentAttachmetnsTable = ((DataView)AttachmentsItemsControl.ItemsSource).Table;
                var newRow = commentAttachmetnsTable.NewRow();
                newRow["CommentAttachmentName"] = _renamedNewCommentAttachments.Last().OriginalName;
                newRow["IsEdited"] = false;
                newRow["AddNeeded"] = true;
                commentAttachmetnsTable.Rows.Add(newRow);

                UploadNewCommentAttachmentsInQueue();
            }
        }

        private string GetNewsPath(int newsId)
        {
            var newsRows = _newsFeedClass.News.Select(string.Format("NewsID = {0}", newsId));
            if (!newsRows.Any()) return null;

            // Get news status name, and make basic parrent directory path
            var newsStatusId = Convert.ToInt32(newsRows.First()["NewsStatus"]);
            var newsStatusName = new IdToNewsStatusConverter().Convert(newsStatusId, typeof(string),
                "NewsStatusName", CultureInfo.InvariantCulture).ToString();

            var newsDir = string.Format("Сообщение_[id]{0}", newsId);

            var newsPath = string.Concat(_basicDirectory, newsStatusName, "/", newsDir, "/");

            return newsPath;
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

        #endregion
    }
}
