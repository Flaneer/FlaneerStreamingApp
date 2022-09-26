using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.UnreliableDataChannel;

namespace UDPHolePunchClient;

internal class Program
{
    static void Main(string[] args)
    {
        IPAddress serverAddr = IPAddress.Parse(args[0]);
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        byte[] bytes = new byte[] {128, 64};
        IPEndPoint serverEndPoint = new IPEndPoint(serverAddr, 11000);
        
        s.SendTo(bytes, serverEndPoint);

        var loop = false;

        IPEndPoint? peer = null;

        do
        {
            byte[] inBuf = new byte[HolePunchInfoPacket.HeaderSize];
            EndPoint inEP = new IPEndPoint(IPAddress.Any, 11000);
            s.ReceiveFrom(inBuf, ref inEP);

            var inIp = inEP as IPEndPoint ?? throw new InvalidOperationException();
            Console.WriteLine($"Contact from {inIp}");
            //Message from server
            if (Equals(inIp?.Address, serverEndPoint.Address) && inIp.Port == serverEndPoint.Port)
            {
                HolePunchInfoPacket client = HolePunchInfoPacket.FromBytes(inBuf);
                peer = client.ToEndPoint();
                Console.WriteLine($"Peer registered at: {client}");
                loop = true;
            }
            //Message from peer
            else
            {
                string message = System.Text.Encoding.UTF8.GetString(inBuf, 0, inBuf.Length);
                Console.WriteLine($"Received message from peer: {message}");
                loop = false;
            }
            
            if (peer != null)
            {
                string convert = "Hello there!";
                Console.WriteLine($"Sending message to peer: {convert}");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(convert);
                s.SendTo(buffer, peer);
            }
        } while (loop);
    }
}
