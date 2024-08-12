using ChatCommon;
using ChatCommon.Extentions;
using System.Net;
using System.Net.Sockets;

namespace ChatNetwork
{
    public class MessageSource : IMessageSource
    {
        private readonly UdpClient _udpClient;

        public MessageSource(UdpClient udpClient)
        {
            _udpClient = udpClient;                
        }

        public IPEndPoint CreateEndPoint(string address, int port)
        {
            throw new NotImplementedException();
        }

        public async Task<ReceiveResult> Receive(CancellationToken token)
        {
            var data = await _udpClient.ReceiveAsync(token);
            return new(data.RemoteEndPoint, data.Buffer.ToMessage());
        }

        public async Task Send(Message message, IPEndPoint endPoint, CancellationToken token)
        {
            await _udpClient.SendAsync(message.ToBytes(), endPoint, token);
        }
    }
}
