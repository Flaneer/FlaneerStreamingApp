namespace FlaneerMediaLib;

/// <summary>
/// Sends Acks
/// </summary>
public class AckSender : IService
{
    private UDPSender udpSender;
    
    /// <summary>
    /// ctor
    /// </summary>
    public AckSender()
    {
        ServiceRegistry.TryGetService(out udpSender);
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, OnPacketReceived);
    }

    private void OnPacketReceived(byte[] incomingPacket)
    {
        Ack ack = new Ack();
        ack.PacketId = PacketInfoParser.PacketId(incomingPacket);
        
        
        
        udpSender.Send();
    }
}
