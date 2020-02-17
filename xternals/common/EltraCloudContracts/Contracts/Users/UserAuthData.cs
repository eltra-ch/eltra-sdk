using System;
using System.Runtime.Serialization;
using System.Text;
using EltraCommon.Helpers;

namespace EltraCloudContracts.Contracts.Users
{
    [DataContract]
    public class UserAuthData
    {
        #region Private fields

        private string _plainPassword;

        #endregion

        #region Properties

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string PlainPassword
        {
            get
            {
                const int maxPassLength = 6;

                if (string.IsNullOrEmpty(_plainPassword))
                {
                    PlainPassword = GeneratePlainPassword(maxPassLength);
                }

                return _plainPassword;
            }
            set
            {
                if (_plainPassword != value && !string.IsNullOrEmpty(value))
                {
                    _plainPassword = value;
                    OnPlainPasswordChanged();
                }
            }
        }

        [DataMember]
        public string Password { get; set; }

        #endregion

        #region Methods

        private void OnPlainPasswordChanged()
        {
            Password = CryptHelpers.ToSha256(_plainPassword);
        }

        private string GeneratePlainPassword(int size)
        {
            var result = new StringBuilder();
            Random random = new Random();
            
            for (int i = 0; i < size; i++)
            {
                var r = random.Next(0,9);

                result.Append($"{r}");
            }
            
            return result.ToString();
        }

        #endregion
    }
}
