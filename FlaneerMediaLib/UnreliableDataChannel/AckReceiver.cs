﻿using FlaneerMediaLib.Logging;
using FlaneerMediaLib.SmartStorage;

namespace FlaneerMediaLib.UnreliableDataChannel;

/// <summary>
/// Receives acks and parses them
/// </summary>
public class AckReceiver : IService
{
    private readonly Dictionary<int, bool> prevAcks = new ();
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

    private void OnAckReceived(SmartBuffer incomingAck)
    {
        OnAckReceivedImpl(incomingAck.Buffer, prevAcks);
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
