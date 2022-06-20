using System.Diagnostics;
using FlaneerMediaLib.Logging;

namespace FlaneerMediaLib;

/// <summary>
/// Receives acks and parses them
/// </summary>
public class AckReceiver : IService
{
    private Dictionary<int, bool> prevAcks = new ();
    private readonly Logger logger;

    /// <summary>
    /// ctor
    /// </summary>
    public AckReceiver()
    {
        logger = Logger.GetLogger(this);
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.Ack, OnAckReceived);
    }

    private void OnAckReceived(byte[] incomingAck)
    {
        OnAckReceivedImpl(incomingAck, prevAcks);
        logger.Trace(prevAcks.Last().Key.ToString());
    }

    internal static void OnAckReceivedImpl(byte[] incomingAck, Dictionary<int, bool> prevAckBuffer)
    {
        Ack ack = Ack.FromUDPPacket(incomingAck);
        var prev32 = ack.GetPrevious32();
        var ackBuffer = ack.PreviousAcksToBuffer();
        for (int i = ackBuffer.Length-1; i >= ackBuffer.Length - prev32.Count; i--)
        {
            prevAckBuffer[(int) prev32.Pop()] = ackBuffer[i];
        }
    }
}
