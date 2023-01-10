using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Sends UDP packets
/// </summary>
public class UDPSender : IService
{
    /// <summary>
    /// Whether or not a hole punched peer is registered
    /// </summary>
    public bool PeerRegistered = false;
    
    private readonly Socket s;

    internal IPEndPoint? PeerEndPoint
    {
        get => peerEndPoint;
        set
        {
            if (value != null)
                PeerRegistered = true;
            //Ping the peer
            byte[] buf = {128, 128};
            s.SendTo(buf, value);
            peerEndPoint = value;
        }
    }
    private IPEndPoint? peerEndPoint = null;
    private readonly IPEndPoint serverEndPoint;

    private UInt32 packetCount;
    private readonly Logger logger;
    private readonly CommandLineArgumentStore clas;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPSender(Socket s)
    {
        this.s = s;
        logger = Logger.GetLogger(this);
        ServiceRegistry.TryGetService(out clas);
        
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        
        var frameSettings = clas.GetParams(CommandLineArgs.BroadcastAddress);
        
        var ip = IPAddress.Parse(frameSettings[0]);
        serverEndPoint = new IPEndPoint(ip, Int32.Parse(frameSettings[1]));
    }
    
    /// <summary>
    /// Sends bytes to the address
    /// </summary>
    public void SendToServer(byte[] bytes)
    {
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
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
        if(clas.HasArgument(CommandLineArgs.NoNet))
            return;
        if (peerEndPoint == null)
        {
            logger.Error("Can't send packet, no peer registered!");
            return;
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
