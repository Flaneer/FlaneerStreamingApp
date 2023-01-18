using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;

namespace UDPHolePunchClient;

internal class Program
{
    static void Main(string[] args)
    {
        IPAddress serverAddr = IPAddress.Parse(args[0]);
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        byte[] bytes = new HolePunchInfoPacket(NodeType.StreamingClient, 0).ToUDPPacket();
        IPEndPoint serverEndPoint = new IPEndPoint(serverAddr, 11000);
        
        s.SendTo(bytes, serverEndPoint);

        var loop = false;

        IPEndPoint? peer = null;

        Task? task = null;
        var packetsToSend = 50;
        var receivedPackets = 0;
        
        do
        {
            byte[] inBuf = new byte[Int16.MaxValue];
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
                loop = ++receivedPackets < packetsToSend;
            }
            
            if (peer != null && task == null)
            {
                var peerLocal = peer;
                task = Task.Run(() =>
                {
                    for (int i = 0; i < packetsToSend; i++)
                    {
                        byte[] buffer = new byte[Int16.MaxValue];
                        Random random = Random.Shared;
                        random.NextBytes(buffer);
                        s.SendTo(buffer, peerLocal);
                        Thread.Sleep(2);
                    }
                });
            }
        } while (loop);
    }
}
