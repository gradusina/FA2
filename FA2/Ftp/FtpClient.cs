using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace FA2.Ftp
{
    internal class FtpClient : WebClient
    {
        //protected override WebRequest GetWebRequest(Uri address)
        //{
        //    var request = (FtpWebRequest) base.GetWebRequest(address);
        //    if (request == null) return base.GetWebRequest(address);

        //    request.UsePassive = false;
        //    return request;
        //}

        //private FtpWebRequest _ftpWebRequest;

        private string _path;
        private const int BufferSize = 2048;

        public bool Passive { get; set; }
        public bool Binary { get; set; }
        public bool EnableSsl { get; set; }

        public string CurrentPath
        {
            get { return _path; }
            set { _path = value; }
        }

        public string ParentUri
        {
            get
            {
                //var index = _path.LastIndexOf("/", StringComparison.Ordinal);
                //return _path.Remove(index);
                return GetParrentDictionary(_path);
            }
        }

        public FtpClient(string path, string userName, string password)
        {
            _path = path;

            Passive = true;
            Binary = true;
            EnableSsl = false;
            Credentials = new NetworkCredential(userName, password);
        }

        public string DeleteFile(string filePath)
        {
            var ftpWebRequest = CreateRequest(filePath, WebRequestMethods.Ftp.DeleteFile);

            try
            {
                return GetStatusDescription(ftpWebRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string FtpDownloadFile(string fileName, string dest)
        {
            var ftpWebRequest = CreateRequest(Combine(_path, fileName), WebRequestMethods.Ftp.DownloadFile);

            var buffer = new byte[BufferSize];

            try
            {
                var response = (FtpWebResponse) ftpWebRequest.GetResponse();
                var stream = response.GetResponseStream();

                using (var fs = new FileStream(dest, FileMode.OpenOrCreate))
                {
                    if (stream == null) return response.StatusDescription;

                    var readCount = stream.Read(buffer, 0, BufferSize);

                    while (readCount > 0)
                    {
                        fs.Write(buffer, 0, readCount);
                        readCount = stream.Read(buffer, 0, BufferSize);
                    }
                }

                var status = response.StatusDescription;

                ftpWebRequest.Abort();
                if (response != null) response.Close();

                return status;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DateTime GetDateTimestamp(string filePath)
        {
            var ftpWebRequest = CreateRequest(filePath, WebRequestMethods.Ftp.GetDateTimestamp);

            try
            {
                var response = (FtpWebResponse) ftpWebRequest.GetResponse();

                DateTime lastMod = response.LastModified;

                ftpWebRequest.Abort();
                if (response != null) response.Close();

                return lastMod;
            }
            catch (Exception)
            {
                return new DateTime();
            }
        }

        public long GetFileSize(string filePath)
        {
            var ftpWebRequest = CreateRequest(filePath, WebRequestMethods.Ftp.GetFileSize);

            try
            {
                var response = (FtpWebResponse) ftpWebRequest.GetResponse();
                var length = response.ContentLength;

                ftpWebRequest.Abort();
                if (response != null) response.Close();

                return length;

            }
            catch (Exception)
            {
                return 0;
            }
        }

        public IEnumerable<string> ListDirectory()
        {
            var list = new List<string>();
            var ftpWebRequest = CreateRequest(WebRequestMethods.Ftp.ListDirectory);

            try
            {
                var response = (FtpWebResponse) ftpWebRequest.GetResponse();
                var stream = response.GetResponseStream();

                if (stream == null) return list.ToArray();

                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }

                ftpWebRequest.Abort();
                if (response != null) response.Close();
            }
            catch (WebException)
            {
            }

            return list.ToArray();
        }

        public IEnumerable<FtpFileDirectoryInfo> ListDirectoryDetails()
        {
            var stringsList = new List<string>();
            var list = new List<FtpFileDirectoryInfo>();

            var ftpWebRequest = CreateRequest(WebRequestMethods.Ftp.ListDirectoryDetails);
            var regex =
                new Regex(
                    @"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase |
                    RegexOptions.IgnorePatternWhitespace);

            try
            {
                var response = (FtpWebResponse) ftpWebRequest.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null)
                    //using (var reader = new StreamReader(stream, Encoding.GetEncoding(1251), true))
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            stringsList.Add(reader.ReadLine());
                        }
                    }

                ftpWebRequest.Abort();
                if (response != null) response.Close();

                if (stringsList.Count == 0) return list;

                list = stringsList.AsEnumerable().Select(s =>
                                                         {
                                                             var match = regex.Match(s);
                                                             if (match.Length > 5)
                                                             {
                                                                 var dirInfo = match.Groups[1].Value;

                                                                 var fileSize = string.Empty;
                                                                 if (dirInfo != "d")
                                                                     fileSize = match.Groups[3].Value.Trim();

                                                                 var permissions = match.Groups[2].Value;


                                                                 var date = match.Groups[4].Value;

                                                                 var convertedDate = DateTime.Parse(date,
                                                                     new CultureInfo("en-US"));
                                                                 date = convertedDate.ToString("dd.MM.yyyy");

                                                                 var time = string.IsNullOrEmpty(match.Groups[5].Value)
                                                                     ? "00:00"
                                                                     : match.Groups[5].Value;

                                                                 var fileName = match.Groups[6].Value;

                                                                 return new FtpFileDirectoryInfo(dirInfo, permissions,
                                                                     fileSize, date, time, fileName,
                                                                     Combine(_path, fileName));
                                                             }
                                                             return new FtpFileDirectoryInfo();
                                                         }).ToList();
            }
            catch (WebException)
            {
            }

            return list;
        }

        public string MakeDirectory(string directoryPath)
        {
            var parrentPath = directoryPath;
            string description;

            // Find first, exist parrent directory
            while (!DirectoryExist(parrentPath))
            {
                parrentPath = GetParrentDictionary(parrentPath);
            }

            do
            {
                var childDirectory = directoryPath;

                // Get child directory
                while (parrentPath != GetParrentDictionary(childDirectory))
                {
                    childDirectory = GetParrentDictionary(childDirectory);
                }

                parrentPath = childDirectory;

                // Creating directory
                var ftpWebRequest = CreateRequest(parrentPath, WebRequestMethods.Ftp.MakeDirectory);
                try
                {
                    description = GetStatusDescription(ftpWebRequest);
                }
                catch (WebException)
                {
                    return null;
                }
            } while (parrentPath != directoryPath);

            return description;
        }

        public string PrintWorkingDirectory()
        {
            var ftpWebRequest = CreateRequest(WebRequestMethods.Ftp.PrintWorkingDirectory);

            try
            {
                return GetStatusDescription(ftpWebRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string RemoveDirectory(string directoryPath)
        {
            var ftpWebRequest = CreateRequest(directoryPath, WebRequestMethods.Ftp.RemoveDirectory);

            try
            {
                return GetStatusDescription(ftpWebRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string Rename(string currentPath, string newName)
        {
            var ftpWebRequest = CreateRequest(currentPath, WebRequestMethods.Ftp.Rename);
            ftpWebRequest.RenameTo = newName;

            try
            {
                return GetStatusDescription(ftpWebRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string FtpUploadFile(string source, string destination)
        {
            var ftpWebRequest = CreateRequest(Combine(_path, destination), WebRequestMethods.Ftp.UploadFile);

            try
            {
                using (var stream = ftpWebRequest.GetRequestStream())
                {
                    using (var fileStream = File.Open(source, FileMode.Open))
                    {
                        int num;

                        var buffer = new byte[BufferSize];

                        while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, num);
                        }
                    }
                }

                return GetStatusDescription(ftpWebRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string UploadFileWithUniqueName(string source)
        {
            var ftpWebRequest = CreateRequest(WebRequestMethods.Ftp.UploadFileWithUniqueName);

            try
            {
                using (var stream = ftpWebRequest.GetRequestStream())
                {
                    using (var fileStream = File.Open(source, FileMode.Open))
                    {
                        int num;

                        var buffer = new byte[BufferSize];

                        while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, num);
                        }
                    }
                }

                var response = (FtpWebResponse) ftpWebRequest.GetResponse();
                var fileName = Path.GetFileName(response.ResponseUri.ToString());
                ftpWebRequest.Abort();
                if (response != null)
                    response.Close();
                return fileName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private FtpWebRequest CreateRequest(string method)
        {
            return CreateRequest(_path, method);
        }

        private FtpWebRequest CreateRequest(string path, string method)
        {
            var r = (FtpWebRequest) WebRequest.Create(path);

            r.Credentials = Credentials;
            r.Method = method;
            r.UseBinary = Binary;
            r.EnableSsl = EnableSsl;
            r.UsePassive = Passive;
            r.KeepAlive = false;
            r.Proxy = null;

            return r;
        }

        private static string GetStatusDescription(WebRequest request)
        {
            var response = (FtpWebResponse) request.GetResponse();
            var description = response.StatusDescription;

            request.Abort();
            if (response != null) response.Close();

            return description;
        }

        private static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2).Replace("\\", "/");
        }

        public bool DirectoryExist(string directoryPath)
        {
            var exist = false;
            var ftpWebRequest = CreateRequest(directoryPath, WebRequestMethods.Ftp.ListDirectory);

            try
            {
                var response = ftpWebRequest.GetResponse();
                exist = true;
                ftpWebRequest.Abort();
                
                if (response != null) response.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response == null) return exist;
                var response = (FtpWebResponse) ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    exist = false;
                }
            }

            return exist;
        }

        public bool FileExist(string filePath)
        {
            var exist = false;
            var ftpWebRequest = CreateRequest(filePath, WebRequestMethods.Ftp.GetFileSize);

            try
            {
                var response = ftpWebRequest.GetResponse();
                exist = true;
                ftpWebRequest.Abort();
                if (response != null) response.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var response = (FtpWebResponse) ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        exist = false;
                    }
                }
            }

            return exist;
        }

        private static string GetParrentDictionary(string directoryPath)
        {
            var uri = new Uri(Path.Combine(directoryPath, @"..\"));
            return uri.ToString();
        }
    }
}
