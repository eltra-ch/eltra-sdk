using System;
using System.Runtime.Serialization;

using EltraCloudContracts.Contracts.Users;

namespace EltraCloudContracts.Contracts.Sessions
{
    [DataContract]
    public class Session
    {
        #region Private fields

        private const uint DefaultUpdateInterval = 30;

        private User _user;
        private IpLocation _location;

        #endregion

        #region Constructors

        public Session()
        {
            Modified = DateTime.Now;
            Created = DateTime.Now;
            Status = SessionStatus.Offline;
            Timeout = uint.MaxValue;
            UpdateInterval = DefaultUpdateInterval;
        }

        #endregion

        #region Properties

        [DataMember]
        public string Uuid { get; set; }

        [DataMember]
        public User User
        {
            get => _user ?? (_user = new User());
            set => _user = value;
        }
        
        [DataMember]
        public SessionStatus Status { get; set; }

        [DataMember]
        public IpLocation IpLocation
        {
            get => _location ?? (_location = new IpLocation());
            set => _location = value;
        }

        [DataMember]
        public uint Timeout { get; set; }

        [IgnoreDataMember]
        public uint UpdateInterval { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public DateTime Created { get; set; }
        
        #endregion
    }
}
