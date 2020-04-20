using System;
using System.Linq;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Reflection;
using System.Windows;
using FA2.Classes.WorkerRequestsEnums;
using FA2.Converters;
using FA2.Ftp;
using System.Net;
using System.ComponentModel;
using System.Globalization;

namespace FA2.Classes
{
    public struct WorkerRequestMicrosoftWordReportStruct
    {
        public int FactoryId { get; set; }
        public string DirectoryName { get; set; }
        public long ProfessionId { get; set; }
        public int WorkerId { get; set; }
        public SalarySaveType SalarySaveType { get; set; }
        public IntervalType IntervalType { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime RequestFinishDate { get; set; }
        public WorkingOffType WorkingOffType { get; set; }
        public string WorkerNotes { get; set; }
        public long MainWorkerProfessionId { get; set; }
        public int MainWorkerId { get; set; }
    }

    static class ExportToWord
    {
        private const string TemplatesDirectory = @"FtpFaII/Templates/";
        private const string WorkerRequestTemplate = "WorkerRequestTemplate.docx";

        private static FtpClient _ftpClient;
        private static string _tempWorkerRequestDocumentPath;
        private static WaitWindow _processWindow;
        private static long _fileSize;
        private static WorkerRequestsClass _workerRequestsClass;
        private static WorkerRequestMicrosoftWordReportStruct _workerRequestReportStruct;

        public static void CreateWorkerRequestReport(WorkerRequestMicrosoftWordReportStruct reportStruct)
        {
            _workerRequestReportStruct = reportStruct;
            _tempWorkerRequestDocumentPath = Path.Combine(App.TempFolder, "WorkerRequestTemplate.docx");

            if (File.Exists(_tempWorkerRequestDocumentPath))
            {
                ReplaceWorkerRequestText();
                return;
            }

            // Downloading template word file from ftp

            var ftpFilePath = string.Concat(App.GetFtpUrl, TemplatesDirectory, WorkerRequestTemplate);
            _ftpClient = new FtpClient(string.Concat(App.GetFtpUrl, TemplatesDirectory), "fa2app", "Franc1961");

            if (_ftpClient.IsBusy)
            {
                MessageBox.Show("В данный момент невозможно выполнить загрузку файла-шаблона. Попробуйте позже");
                return;
            }

            if (!Directory.Exists(App.TempFolder))
            {
                Directory.CreateDirectory(App.TempFolder);
            }
            
            if (_ftpClient.FileExist(ftpFilePath))
            {
                var uri = new Uri(ftpFilePath);

                if (_processWindow == null)
                {
                    _processWindow = new WaitWindow { Text = "Загрузка файла-шаблона..." };
                    _processWindow.Show(Application.Current.MainWindow, true);
                }

                _fileSize = _ftpClient.GetFileSize(ftpFilePath);
                _ftpClient.DownloadProgressChanged += OnFtpClientDownloadProgressChanged;
                _ftpClient.DownloadFileCompleted += OnFtpClientDownloadFileCompleted;
                _ftpClient.DownloadFileAsync(uri, _tempWorkerRequestDocumentPath);
            }
            else
            {
                MessageBox.Show("Не удаётся найти файл-шаблон на сервере. \nПопробуйте выполнить операцию позже или обратитесь к администратору.");
            }
        }

        private static void OnFtpClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.BytesReceived / (double)_fileSize;
            if (_processWindow == null) return;

            _processWindow.Progress = progress * 100;
            _processWindow.Text = string.Format("Загрузка файла-шаблона... \n{0} кБ", e.BytesReceived / 1024);
        }

        private static void OnFtpClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            _ftpClient.DownloadFileCompleted -= OnFtpClientDownloadFileCompleted;
            _ftpClient.DownloadProgressChanged -= OnFtpClientDownloadProgressChanged;

            if (_processWindow != null)
                _processWindow.Text = "Формирование заявления...";

            ReplaceWorkerRequestText();

            if (_processWindow != null)
                _processWindow.Close(true);
            _processWindow = null;
        }

        private static void ReplaceWorkerRequestText()
        {
            #region Variables initializing

            App.BaseClass.GetWorkerRequestsClass(ref _workerRequestsClass);

            var factoryName = _workerRequestReportStruct.FactoryId == 1 
                ? "СООО «ЗОВ-ПРОФИЛЬ»"
                : _workerRequestReportStruct.FactoryId == 2 
                    ? "СООО «ЗОВ-ТермоПрофильСистемы»"
                    : string.Empty;
            var directoryName = _workerRequestReportStruct.DirectoryName;
            var professionName = new IdToProfessionConverter().
                Convert(_workerRequestReportStruct.ProfessionId, typeof(string), null, CultureInfo.InvariantCulture).ToString().ToLower();
            var workerName = new IdToNameConverter().Convert(_workerRequestReportStruct.WorkerId, "FullName");

            var salarySaveType = string.Empty;
            var rows = _workerRequestsClass.SalatySaveTypesTable.Select(string.Format("SalarySaveTypeID = {0}", (int)_workerRequestReportStruct.SalarySaveType));
            if (rows.Any())
            {
                salarySaveType = rows.First()["SalarySaveTypeName"].ToString().ToLower();
            }

            var requestDate = string.Empty;
            switch (_workerRequestReportStruct.IntervalType)
            {
                case IntervalType.DurringSomeHours:
                    requestDate = string.Format("на {0:dd.MM.yyyy} с {0:HH:mm} по {1:HH:mm}",
                        _workerRequestReportStruct.RequestDate, _workerRequestReportStruct.RequestFinishDate);
                    break;
                case IntervalType.DurringWorkingDay:
                    requestDate = string.Format("на {0:dd.MM.yyyy}", _workerRequestReportStruct.RequestDate);
                    break;
                case IntervalType.DurringSomeDays:
                    requestDate = _workerRequestReportStruct.RequestDate.Year == _workerRequestReportStruct.RequestFinishDate.Year
                    ? string.Format("с {0:dd.MM} по {1:dd.MM.yyyy}", _workerRequestReportStruct.RequestDate, _workerRequestReportStruct.RequestFinishDate)
                    : string.Format("с {0:dd.MM.yyyy} по {1:dd.MM.yyyy}", _workerRequestReportStruct.RequestDate, _workerRequestReportStruct.RequestFinishDate);
                    break;
            }

            var workingOffType = string.Empty;
            var workingOffRows = _workerRequestsClass.WorkingOffTypesTable.
                Select(string.Format("WorkingOffTypeID = {0}", (int)_workerRequestReportStruct.WorkingOffType));
            if (rows.Any())
            {
                workingOffType = workingOffRows.First()["WorkingOffTypeName"].ToString().ToLower();
            }

            var workerNotes = _workerRequestReportStruct.WorkerNotes;

            var mainWorkerName = new IdToNameConverter().Convert(_workerRequestReportStruct.MainWorkerId, "FullName");
            var mainWorkerProfession = new IdToProfessionConverter().
                Convert(_workerRequestReportStruct.MainWorkerProfessionId, typeof(string), null, CultureInfo.InvariantCulture).ToString().ToLower();

            #endregion

            try
            {
                //  create missing object
                object missing = Missing.Value;
                //  create Word application object
                Word.Application wordApp = new Word.Application();
                //  create Word document object
                Word.Document aDoc = null;
                //  if temp.doc available
                if (File.Exists(_tempWorkerRequestDocumentPath))
                {
                    object readOnly = false;
                    object isVisible = true;

                    wordApp.Visible = false;
                    object filePath = _tempWorkerRequestDocumentPath;
                    //aDoc = wordApp.Documents.Open(ref filePath, ref missing,
                    //                            ref readOnly, ref missing, ref missing, ref missing,
                    //                            ref missing, ref missing, ref missing, ref missing,
                    //                            ref missing, ref isVisible, ref missing, ref missing,
                    //                            ref missing, ref missing);

                    aDoc = wordApp.Documents.Add(ref filePath, ref missing, ref missing, ref isVisible);

                    aDoc.Activate();
                    FindAndReplace(wordApp, "<CompanyName/>", factoryName);
                    FindAndReplace(wordApp, "<Director/>", directoryName);
                    FindAndReplace(wordApp, "<WorkerProfession/>", professionName);
                    FindAndReplace(wordApp, "<WorkerName/>", workerName);
                    FindAndReplace(wordApp, "<SalarySaveType/>", salarySaveType);
                    FindAndReplace(wordApp, "<RequestDate/>", requestDate);
                    //FindAndReplace(wordApp, "<WorkingOffType/>", workingOffType);
                    FindAndReplace(wordApp, "<RequestNotes/>", workerNotes);
                    FindAndReplace(wordApp, "<MainWorkerName/>", mainWorkerName);
                    FindAndReplace(wordApp, "<MainWorkerProfession/>", mainWorkerProfession);

                    wordApp.Visible = true;
                }
                else
                {
                    MessageBox.Show("Не удаётся найти файл-шаблон.", "Файл отсутствует", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка в процессе." + "\n" + exp.Message + "\nПопробуйте закрыть открытые Word документы.", "Internal Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void FindAndReplace(Word.Application wordApp, object findText, object replaceText)
        {
            object matchCase = true;
            object matchWholeWord = true;
            object matchWildCards = false;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object format = false;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = 2;
            object wrap = 1;
            wordApp.Selection.Find.Execute(ref findText, ref matchCase,
                ref matchWholeWord, ref matchWildCards, ref matchSoundsLike,
                ref matchAllWordForms, ref forward, ref wrap, ref format,
                ref replaceText, ref replace, ref matchKashida,
                        ref matchDiacritics,
                ref matchAlefHamza, ref matchControl);
        }

    }
}
