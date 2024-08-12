
using ChatCommon;
using System.Net;

namespace ChatNetwork
{
    public interface IMessageSource
    {
        Task<ReceiveResult> Receive(CancellationToken token);
        Task Send(Message message, IPEndPoint endPoint, CancellationToken token);
        // IPEndPoint CreateEndPoint(string address, int port);   
    }
}
