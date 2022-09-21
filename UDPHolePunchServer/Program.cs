using System.Net;
using System.Net.Sockets;

namespace UDPHolePunchServer;

internal class Program
{
    static void Main(string[] args)
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var ipMe = new IPEndPoint(IPAddress.Any, 11000);
        s.Bind(ipMe);

        List<Client> clients = new List<Client>();
        
        bool loop = true;
        while (loop)
        {
            byte[] inBuf = new byte[2];
            EndPoint inEP = new IPEndPoint(IPAddress.Any, 11000);
            s.ReceiveFrom(inBuf, ref inEP);
            var inIp = inEP as IPEndPoint ?? throw new InvalidOperationException();
            Client newClient = Client.FromIpEndpoint(inIp);
            Console.WriteLine($"Added new client {newClient}");

            foreach (var client in clients)
            {
                s.SendTo(newClient.ToBytes(), client.ToEndPoint());
            }
            
            clients.Add(newClient);
        }
    }
}
