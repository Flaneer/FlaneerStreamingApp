using System.Text;

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

    /// <summary>
    /// The size in bytes of an Ack
    /// </summary>
    public const int ACKSIZE = 9;
    
    internal byte[] ToUDPPacket()
    {
        byte[] ret = new byte[ACKSIZE];
        using MemoryStream stream = new MemoryStream(ret);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write((byte) PacketType);
        writer.Write(PacketId);
        writer.Write(PreviousAcks);
        
        return ret;
    }

    internal static Ack FromUDPPacket(byte[] ackPacket)
    {
        using var stream = new MemoryStream(ackPacket, 0, ACKSIZE);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        //NOTE: this check also moves the reader beyond the type byte
        var packetType = reader.ReadByte();
        if (packetType != (byte)PacketType.Ack)
        {
            throw new Exception($"Trying to decode a {(PacketType) packetType} as a {PacketType.Ack}");
        }

        return new Ack()
        {
            PacketId = reader.ReadUInt32(),
            PreviousAcks = reader.ReadUInt32(),
        };
    }
    
    
    internal static UInt32 AcksFromBinary(int[] buffer)
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
