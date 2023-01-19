using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;

namespace UDPHolePunchServer;

internal class Program
{
    static void Main(string[] args)
    {
        HolePunchServer server = new HolePunchServer();
        server.InitHolePunching();
        
    }
}
