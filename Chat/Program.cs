using ChatApp;
using ChatNetwork;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000);
            UdpClient udpClient;
            IMessageSource source;

            if (args.Length == 0 || args[0] == "--start-server") 
            // if (args.Length > 0) 
            { 
                // Server
                udpClient = new UdpClient(serverEndPoint);
                source = new MessageSource(udpClient);
                

                var chat = new ChatServer(source);
                await chat.Start();
            }
            else
            {
                // Client
                var rand = new Random();
                udpClient = new UdpClient(rand.Next(12500, 62000));
                source = new MessageSource(udpClient);

                var client = new ChatClient(args[0], serverEndPoint, source);
                await client.Start();
            }
        }
    }
}
