using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.Results;

namespace EltraCloudContracts.Contracts.Users
{
    public class AuthRequestResult : RequestResult
    {
        [DataMember]
        public string Token { get; set; }
    }
}
