using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;

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
        
        ServiceRegistry.ServiceAdded += service =>
        {
            if (service is UDPReceiver receiver)
            {
                receiver.SubscribeToReceptionTraffic(PacketType.HolePunchInfo, OnInfoReceived);      
            }
        };

        udpSender.SendToServer(new HolePunchInfoPacket().ToUDPPacket());
    }
    
    private void OnInfoReceived(SmartBuffer smartBuffer)
    {
        HolePunchInfoPacket packet = HolePunchInfoPacket.FromBytes(smartBuffer.Buffer);
        logger.Info($"Peer info received: {packet}");
        udpSender.PeerEndPoint = packet.ToEndPoint();
    }
}
