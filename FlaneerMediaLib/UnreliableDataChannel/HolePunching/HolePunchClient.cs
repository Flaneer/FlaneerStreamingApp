using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;

namespace FlaneerMediaLib.UnreliableDataChannel.HolePunching;

/// <summary>
/// Listens for the address of a punched hole
/// </summary>
public class HolePunchClient : IService
{
    private readonly HolePunchMessageType holePunchMessageType;
    private readonly Logger logger;
    private UDPSender udpSender;
    private readonly ushort connectionId;
    private bool connected;

    /// <summary>
    /// ctor
    /// </summary>
    public HolePunchClient(HolePunchMessageType holePunchMessageType)
    {
        this.holePunchMessageType = holePunchMessageType;
        logger = Logger.GetLogger(this);
        
        ServiceRegistry.TryGetService(out udpSender);
        
        ServiceRegistry.ServiceAdded += service =>
        {
            if (service is UDPReceiver receiver)
            {
                receiver.SubscribeToReceptionTraffic(PacketType.HolePunchInfo, OnInfoReceived);      
            }
        };

        connectionId = 0;
        
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
        if(clas.HasArgument(CommandLineArgs.SessionId))
            connectionId = ushort.Parse(clas.GetParams(CommandLineArgs.SessionId).First());

        connected = true;
        Task.Run(HolePunchHeartBeat);
    }

    private void HolePunchHeartBeat()
    {
        while (connected)
        {
            udpSender.SendToServer(new HolePunchInfoPacket(holePunchMessageType, connectionId).ToUDPPacket());
            Thread.Sleep(HolePunchServer.HeartbeatInterval);
        }
    }


    private void OnInfoReceived(SmartBuffer smartBuffer)
    {
        HolePunchInfoPacket packet = HolePunchInfoPacket.FromBytes(smartBuffer.Buffer);
        if (packet.HolePunchMessageType == HolePunchMessageType.PartnerDisconnected)
        {
            udpSender.PeerEndPoint = null;
        }
        else
        {
            logger.Info($"Peer info received: {packet}");
            udpSender.PeerEndPoint = packet.ToEndPoint();
        }
        
    }
}
