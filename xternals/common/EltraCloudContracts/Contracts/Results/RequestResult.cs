using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Results
{
    [DataContract]
    public class RequestResult
    {
        public RequestResult()
        {
            Result = true;
            Message = "Success";
            ErrorCode = ErrorCodes.Success;
        }

        [DataMember]
        public bool Result { get; set; }
        
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public ErrorCodes ErrorCode { get; set; }
    }
}
