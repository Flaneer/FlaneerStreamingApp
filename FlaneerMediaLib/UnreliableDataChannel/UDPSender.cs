using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Sends UDP packets
/// </summary>
public class UDPSender : IService
{
    private readonly Socket s = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    internal IPEndPoint? peerEndPoint = null;
    private readonly IPEndPoint serverEndPoint;

    private UInt32 packetCount;
    private readonly Logger logger;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPSender()
    {
        logger = Logger.GetLogger(this);
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.BroadcastAddress);
        
        var ip = IPAddress.Parse(frameSettings[0]);
        serverEndPoint = new IPEndPoint(ip, Int32.Parse(frameSettings[1]));
    }
    
    /// <summary>
    /// Sends bytes to the address
    /// </summary>
    public void SendToServer(byte[] bytes)
    {
        if (PacketInfoParser.PacketType(bytes) != PacketType.Ack)
        {
            var packetCountBytes = BitConverter.GetBytes(++packetCount);
            for (int i = 0; i < sizeof(Int32); i++)
            {
                bytes[PacketInfoParser.PacketIdIdx + i] = packetCountBytes[i];
            }
        }
        s.SendTo(bytes, serverEndPoint);
    }
    
    /// <summary>
    /// Sends bytes to the address
    /// </summary>
    public void SendToPeer(byte[] bytes)
    {
        if (peerEndPoint == null)
        {
            logger.Error("Can't send packet, no peer registered!");
        }
        
        if (PacketInfoParser.PacketType(bytes) != PacketType.Ack)
        {
            var packetCountBytes = BitConverter.GetBytes(++packetCount);
            for (int i = 0; i < sizeof(Int32); i++)
            {
                bytes[PacketInfoParser.PacketIdIdx + i] = packetCountBytes[i];
            }
        }
        s.SendTo(bytes, peerEndPoint);
    }
}
