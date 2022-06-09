namespace FlaneerMediaLib;

/// <summary>
/// This class is sent in response to a packet being received
/// </summary>
public class Ack
{
    /// <summary>
    /// The type of this packet
    /// </summary>
    public PacketType PacketType => PacketType.Ack;
    
    /// <summary>
    /// The Id of the current packet
    /// </summary>
    public UInt32 PacketId;

    /// <summary>
    /// 
    /// </summary>
    public UInt32 PreviousAcks;

    internal byte[] ToPacket()
    {
        
    }

    internal static Ack FromPacket(byte[] ackPacket)
    {
        
    }
    
    
    internal static UInt32 AcksFromBuffer(int[] buffer)
    {
        if (buffer.Length != 32)
        {
            throw new Exception("Acks buffer not full");
        }

        var bufferAsString = "";
        foreach (var b in buffer)
        {
            bufferAsString += b.ToString();
        }
        return Convert.ToUInt32(bufferAsString, 2);
    }

    internal static int[] BufferFromAck(UInt32 ack)
    {
        var binary = Convert.ToString(ack, 2);
        var paddedBinary = binary.PadLeft(32, '0');
        var buffer = new int[32];
        for (int i = 0; i < 32; i++)
        {
            buffer[i] = Int32.Parse(paddedBinary.Substring(i, 1));
        }
        return buffer;
    }
}
