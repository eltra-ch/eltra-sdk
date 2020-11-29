using EltraCommon.Contracts.Users;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws.Interfaces
{
    interface IConnectionManager
    {
        Task<bool> Connect(string uniqueId, string channelName);
        bool IsConnected(string uniqueId);
        bool CanConnect(string uniqueId);
        
        bool IsDisconnecting(string uniqueId);

        Task<bool> Disconnect(string uniqueId);
        
        Task<bool> DisconnectAll();
        
        Task<bool> Send<T>(string uniqueId, UserIdentity identity, T obj);
        
        Task<string> Receive(string uniqueId);

        Task<T> Receive<T>(string uniqueId);
    }
}
