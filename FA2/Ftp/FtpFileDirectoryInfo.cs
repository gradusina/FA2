namespace FA2.Ftp
{
    internal class FtpFileDirectoryInfo
    {
        public string DirInfo { get; set; }

        public string Permissions { get; set; }

        public string FileSize { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Name { get; set; }

        public string Adress { get; set; }

        public FtpFileDirectoryInfo()
        {
        }

        public FtpFileDirectoryInfo(string dirInfo, string permissions, string fileSize, 
            string date, string time, string name, string adress)
        {
            DirInfo = dirInfo;
            Permissions = permissions;
            FileSize = fileSize;
            Date = date;
            Time = time;
            Name = name;
            Adress = adress;
        }

        public bool IsDirectory
        {
            get
            {
                return !string.IsNullOrWhiteSpace(DirInfo) && DirInfo.ToLower().Equals("d");
            }
        }

        public bool IsFolderUpAction { get; set; }
    }
}
