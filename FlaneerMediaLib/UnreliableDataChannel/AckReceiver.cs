namespace FlaneerMediaLib;

/// <summary>
/// Receives acks and parses them
/// </summary>
public class AckReceiver : IService
{
    /// <summary>
    /// ctor
    /// </summary>
    public AckReceiver()
    {
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.Ack, OnAckReceived);
    }

    private void OnAckReceived(byte[] incomingAck)
    {
        var packetId = BitConverter.ToUInt32(incomingAck, 0);
    }
}
