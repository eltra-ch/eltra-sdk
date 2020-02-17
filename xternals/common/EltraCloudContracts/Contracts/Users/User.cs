using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Users
{
    [DataContract]
    public class User
    {
        public User()
        {
            Modified = DateTime.Now;
            Created = DateTime.Now;
            Status = UserStatus.Unlocked;
        }

        public User(UserAuthData authData)
        {
            AuthData = authData;
            Modified = DateTime.Now;
            Created = DateTime.Now;
            Status = UserStatus.Unlocked;
        }

        [DataMember]
        public UserAuthData AuthData { get; set; }

        [DataMember]
        public UserStatus Status { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public DateTime Created { get; set; }
    }
}
