using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib;

/// <summary>
/// Sends UDP packets
/// </summary>
public class UDPSender
{
    private readonly Socket s = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private readonly IPEndPoint endPoint;

    private readonly Dictionary<PacketType, int> packetCount = new();

    /// <summary>
    /// ctor
    /// </summary>
    public UDPSender(string broadcast, int port)
    {
        var ip = IPAddress.Parse(broadcast);
        endPoint = new IPEndPoint(ip, port);
        
        var packetTypes = Enum.GetValues(typeof(PacketType));
        foreach (var enumType in packetTypes)
        {
            var packetType = (PacketType) enumType;
            packetCount[packetType] = 0;
        }
    }

    /// <summary>
    /// Sends bytes to the address
    /// </summary>
    public void Send(byte[] bytes)
    {
        var outboundType = PacketInfoParser.PacketType(bytes);
        var packetCountBytes = BitConverter.GetBytes(++packetCount[outboundType]);
        for (int i = 0; i < sizeof(Int32); i++)
        {
            bytes[PacketInfoParser.PacketIdIdx + i] = packetCountBytes[i];
        }
        s.SendTo(bytes, endPoint);
    }
}
