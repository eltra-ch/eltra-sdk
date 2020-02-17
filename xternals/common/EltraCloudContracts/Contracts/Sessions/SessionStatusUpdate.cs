using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.Users;

namespace EltraCloudContracts.Contracts.Sessions
{
    [DataContract]
    public class SessionStatusUpdate
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public UserAuthData AuthData { get; set; }

        [DataMember]
        public SessionStatus Status { get; set; }
    }
}
