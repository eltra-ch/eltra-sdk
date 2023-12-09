using EltraCommon.Helpers;
using EltraCommon.Logger;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace EltraXamCommon.Plugins
{
    [DataContract]
    internal class PluginStoreItemFile
    {
        #region Private fields

        private readonly bool _calculateHash;
        private bool _purged;
        private string _fileName;

        #endregion

        #region Constructors

        public PluginStoreItemFile()
        {
        }

        public PluginStoreItemFile(bool calculateHash)
        {
            _calculateHash = calculateHash;
        }

        #endregion

        #region Properties

        [DataMember]
        public string FileName
        {
            get
            {
                return _fileName;                
            }
            set
            {
                _fileName = value;
                OnFileNameChanged();
            }
        }

        [DataMember]
        public string HashCode { get; set; }

        public bool Purged => _purged;

        #endregion

        #region Methods

        #region Events

        protected virtual void OnFileNameChanged()
        {
            try
            {
                if (File.Exists(FileName) && _calculateHash)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] inputBytes = File.ReadAllBytes(FileName);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);

                        var sb = new StringBuilder();

                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("X2"));
                        }

                        HashCode = sb.ToString();
                    }
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnFileNameChanged", e);
            }            
        }

        #endregion

        internal void Purge()
        {
            if (File.Exists(FileName))
            {
                try
                {
                    File.Delete(FileName);

                    _purged = true;
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Purge", e);
                }
            }
            else
            {
                _purged = true;
            }
        }

        public bool Validate()
        {
            bool result = false;

            if (File.Exists(FileName))
            {
                var inputBytes = File.ReadAllBytes(FileName);

                var md5 = CryptHelpers.ToMD5(inputBytes);

                if (md5 == HashCode)
                {
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}
