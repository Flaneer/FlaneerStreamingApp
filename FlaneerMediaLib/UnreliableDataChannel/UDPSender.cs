using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Sends UDP packets
/// </summary>
public class UDPSender : IService
{
    private readonly Socket s = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private readonly IPEndPoint endPoint;

    private UInt32 packetCount;

    /// <summary>
    /// ctor
    /// </summary>
    public UDPSender()
    {
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        var frameSettings = clas.GetParams(CommandLineArgs.BroadcastAddress);
        
        var ip = IPAddress.Parse(frameSettings[0]);
        endPoint = new IPEndPoint(ip, Int32.Parse(frameSettings[1]));
    }

    /// <summary>
    /// Sends bytes to the address
    /// </summary>
    public void Send(byte[] bytes)
    {
        if (PacketInfoParser.PacketType(bytes) != PacketType.Ack)
        {
            var packetCountBytes = BitConverter.GetBytes(++packetCount);
            for (int i = 0; i < sizeof(Int32); i++)
            {
                bytes[PacketInfoParser.PacketIdIdx + i] = packetCountBytes[i];
            }
        }
        s.SendTo(bytes, endPoint);
    }
}
