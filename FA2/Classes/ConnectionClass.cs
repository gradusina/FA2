// An INI file handling class By BLaZiNiX

using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FA2.Classes
{
    public class ConnectionClass
    {
        private readonly string _connectionTimeOut = " Connection Timeout="+2;

        public struct ConnectionStruct
        {
            public string ConnectionString;
            public string ConnectionName;
        }

        private ConnectionStruct _connStr;

        public ConnectionClass(string param)
        {
            CreateConnection(param);
        }

        private static string Decrypt(string cipherText, string password,
                                      string salt = "Kosher", string hashAlgorithm = "MD5",
                                      int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY",
                                      int keySize = 256)
        {
            if (string.IsNullOrEmpty(cipherText))
                return "";

            byte[] initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm,
                                                                          passwordIterations);
            // ReSharper disable once CSharpWarnings::CS0618
            byte[] keyBytes = derivedPassword.GetBytes(keySize/8);

            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};

            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount;

            using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream(cipherTextBytes))
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {
                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            symmetricKey.Clear();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        public ConnectionStruct ConnectionString
        {
            get { return _connStr; }
        }

        private void CreateConnection(string param)
        {
            _connStr = new ConnectionStruct();

            switch (param)
            {
                case "-ic":
                    _connStr.ConnectionString =
                        Decrypt(ConfigurationManager.AppSettings["Internet_Connection"], "24Denis03") + _connectionTimeOut;
                    _connStr.ConnectionName = "Интернет";
                    break;
                //case "-lc":
                //    _connStr.ConnectionString =
                //        Decrypt(ConfigurationManager.AppSettings["Local_Connection"], "24Denis03") + _connectionTimeOut;
                //    _connStr.ConnectionName = "Локальная сеть";
                //    break;
                case "-lc":
                    _connStr.ConnectionString = @"Server = 192.168.1.145; Port = 3306;  Uid = test; Pwd = Franc1961;" + _connectionTimeOut;
                    //_connStr.ConnectionString = Decrypt(ConfigurationManager.AppSettings["MyServer_Connection"], "24Denis03") + _connectionTimeOut;
                    _connStr.ConnectionName = "MyServerConnection";
                    break;
            }
        }
    }
}
