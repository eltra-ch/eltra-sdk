using EltraCommon.Contracts.Users;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws.Interfaces
{
    interface IConnection
    {
        bool IsConnected { get; }
        bool IsDisconnecting { get; }
        Task<bool> Connect(string url);

        Task<bool> Disconnect();

        Task<bool> Send(UserIdentity identity, string typeName, string data);

        Task<bool> Send<T>(UserIdentity identity, T obj);

        Task<string> Receive();

        Task<T> Receive<T>();
    }
}
