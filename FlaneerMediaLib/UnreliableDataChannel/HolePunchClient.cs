using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Listens for the address of a punched hole
/// </summary>
public class HolePunchClient : IService
{
    private readonly Logger logger;
    private UDPSender udpSender;

    /// <summary>
    /// ctor
    /// </summary>
    public HolePunchClient()
    {
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService(out udpSender);
        
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.HolePunchInfo, OnInfoReceived);
        
        udpSender.SendToServer(new HolePunchInfoPacket().ToUDPPacket());
    }

    private void OnInfoReceived(byte[] obj)
    {
        HolePunchInfoPacket packet = HolePunchInfoPacket.FromBytes(obj);
        logger.Info($"Peer info received: {packet}");
        udpSender.PeerEndPoint = packet.ToEndPoint();
    }
}
