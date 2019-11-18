using System.Collections.Generic;
using System.Threading.Tasks;

namespace Twino.Core.Protocols
{
    public interface IProtocolConnectionHandler<in TMessage>
    {
        Task<SocketBase> Connected(ITwinoServer server, IConnectionInfo connection, Dictionary<string, string> properties);

        Task Received(ITwinoServer server, IConnectionInfo info, SocketBase client, TMessage message);

        Task Disconnected(ITwinoServer server, SocketBase client);
    }
}