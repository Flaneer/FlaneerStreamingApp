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
    public HolePunchClient(NodeType nodeType)
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

        ushort connectionId = 0;
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        if(clas.HasArgument(CommandLineArgs.SessionId))
            connectionId = ushort.Parse(clas.GetParams(CommandLineArgs.SessionId).First());
        
        udpSender.SendToServer(new HolePunchInfoPacket(nodeType, connectionId).ToUDPPacket());
    }
    
    private void OnInfoReceived(SmartBuffer smartBuffer)
    {
        HolePunchInfoPacket packet = HolePunchInfoPacket.FromBytes(smartBuffer.Buffer);
        logger.Info($"Peer info received: {packet}");
        udpSender.PeerEndPoint = packet.ToEndPoint();
    }
}
