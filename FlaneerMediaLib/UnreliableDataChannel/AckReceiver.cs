namespace FlaneerMediaLib;

/// <summary>
/// Receives acks and parses them
/// </summary>
public class AckReceiver : IService
{
    private Dictionary<int, bool> prevAcks = new ();
    /// <summary>
    /// ctor
    /// </summary>
    public AckReceiver()
    {
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.Ack, OnAckReceived);
    }

    private void OnAckReceived(byte[] incomingAck) => OnAckReceivedImpl(incomingAck, prevAcks);

    internal static void OnAckReceivedImpl(byte[] incomingAck, Dictionary<int, bool> prevAckBuffer)
    {
        Ack ack = Ack.FromUDPPacket(incomingAck);
        var prev32 = ack.GetPrevious32();
        var ackBuffer = ack.PreviousAcksToBuffer();
        for (int i = 0; i < prev32.Count; i++)
        {
            
        }
    }
}
