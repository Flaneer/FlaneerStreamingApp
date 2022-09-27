using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.UnreliableDataChannel;

namespace UDPHolePunchServer;

internal class Program
{
    static void Main(string[] args)
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var ipMe = new IPEndPoint(IPAddress.Any, 11000);
        s.Bind(ipMe);

        List<HolePunchInfoPacket> clients = new List<HolePunchInfoPacket>();
        
        bool loop = true;
        while (loop)
        {
            byte[] inBuf = new byte[HolePunchInfoPacket.HeaderSize];
            EndPoint inEP = new IPEndPoint(IPAddress.Any, 11000);
            s.ReceiveFrom(inBuf, ref inEP);
            var inIp = inEP as IPEndPoint ?? throw new InvalidOperationException();
            Console.WriteLine($"Contact from {inIp}");
            
            HolePunchInfoPacket newClient = HolePunchInfoPacket.FromIpEndpoint(inIp);
            Console.WriteLine($"Added new client {newClient}");

            foreach (var client in clients)
            {
                Console.WriteLine($"Sending {newClient} to {client}");
                s.SendTo(newClient.ToUDPPacket(), client.ToEndPoint() ?? throw new InvalidOperationException());
                Console.WriteLine($"Sending {client} to {newClient}");
                s.SendTo(client.ToUDPPacket(), newClient.ToEndPoint() ?? throw new InvalidOperationException());
            }
            clients.Add(newClient);
        }
    }
}
