namespace FlaneerMediaLib;

/// <summary>
/// Sends Acks
/// </summary>
public class AckSender : IService
{
    private UDPSender udpSender;

    private List<UInt32> ackBuffer = new(){};

    /// <summary>
    /// ctor
    /// </summary>
    public AckSender()
    {
        ServiceRegistry.TryGetService(out udpSender);
        ServiceRegistry.TryGetService(out UDPReceiver receiver);
        receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, OnPacketReceived);
    }

    private static Stack<UInt32> GetPrevious32(UInt32 packetId)
    {
        var ret = new Stack<UInt32>();
        var size = Math.Min(32, packetId);
        
        for (long i = packetId; i > packetId-size; i--)
        {
            ret.Push((UInt32)i-1);
        }
        return ret;
    }

    private void OnPacketReceived(byte[] incomingPacket)
    {
        Ack ack = AckFromReceivedPacket(ackBuffer, incomingPacket);
        udpSender.Send(ack.ToUDPPacket());
    }

    internal static Ack AckFromReceivedPacket(List<UInt32> ackBuffer, byte[] incomingPacket)
    {
        Ack ack = new Ack();
        ack.PacketId = PacketInfoParser.PacketId(incomingPacket);
        
        var prev32 = GetPrevious32(ack.PacketId);
        int[] binary = new int[32];
        for (int i = 31; i >= 0; i--)
        {
            if (prev32.Count == 0)
            {
                binary[i] = 0;
            }
            else if (ackBuffer.Contains(prev32.Pop()))
            {
                binary[i] = 1;
            }
            else
            { 
                binary[i] = 0;
            }
        }

        var prevAck = Ack.AcksFromBinary(binary);
        ack.PreviousAcks = prevAck;

        if(ackBuffer.Count == 32)
        {
            ackBuffer.RemoveAt(0);
        }
        ackBuffer.Add(ack.PacketId);
        return ack;
    }
}
