

using ChatCommon;
using System.Net;

namespace ChatNetwork
{
    public record ReceiveResult(IPEndPoint EndPoint, Message? Message);    
}
