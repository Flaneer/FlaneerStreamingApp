using System.Text;

namespace FlaneerMediaLib.VideoDataTypes;

/// <summary>
/// Video frame for transmission
/// <remarks>This does not contain the actual frame data it only creates the header to avoid moving the larger frame data</remarks>
/// </summary>
public class TransmissionVideoFrame : IPacketInfo, IVideoFrame
{
    /// <inheritdoc/>
    public PacketType PacketType => PacketType.VideoStreamPacket;
    /// <inheritdoc/>
    public ushort PacketSize { get; set; }
    /// <inheritdoc/>
    public long TimeStamp { get; init; }
    /// <inheritdoc/>
    public uint PacketId { get; init; }
    /// <inheritdoc/>
    public VideoCodec Codec { get; set; }
    /// <inheritdoc/>
    public short Width { get; set; }
    /// <inheritdoc/>
    public short Height { get; set; }
    /// <summary>
    /// The index of the frame in the sequence of frames
    /// </summary>
    public UInt32 SequenceIDX = UInt32.MaxValue;
    /// <summary>
    /// The total number of packets this frame has been split into
    /// </summary>
    public byte NumberOfPackets;
    /// <summary>
    /// The index of the packet in the group of packets
    /// </summary>
    public byte PacketIdx;
    /// <summary>
    /// Size of the frame data
    /// <remarks>Int32 will always be sufficient since it will fit into UDP packet</remarks>
    /// </summary>
    public Int32 FrameDataSize;
    
    /// <summary>
    /// Stores 8 boolean values
    /// </summary>
    /// <remarks>0: IsIFrame 1-7: Unreserved</remarks>
    private byte flagsByte;

    private bool[] flags = Array.Empty<bool>();

    private const byte IsIFrameIdx = 0;
    
    /// <summary>
    /// Is this frame an I frame
    /// </summary>
    public bool IsIFrame
    {
        get => flags.Length > IsIFrameIdx && flags[IsIFrameIdx];
        set => flagsByte = BooleanArrayUtils.SetSingleBit(flagsByte, value, IsIFrameIdx);
    } 
    
    /// <summary>
    /// The size of the header in bytes
    /// <remarks>This is manually calculated</remarks>
    /// </summary>
    public const int HeaderSize = 30;
    
    /// <inheritdoc/>
    public byte[] ToUDPPacket()
    {
        byte[] ret = new byte[HeaderSize];
        using MemoryStream stream = new MemoryStream(ret);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write((byte) PacketType);
        writer.Write(PacketSize);
        writer.Write(DateTime.UtcNow.Ticks);
        writer.Write(PacketId);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(SequenceIDX);
        writer.Write(NumberOfPackets);
        writer.Write(PacketIdx);
        writer.Write(FrameDataSize);
        writer.Write(flagsByte);
        
        return ret;
    }
    
    /// <summary>
    /// Helper method for turning UDP packet into transmission frame
    /// </summary>
    public static TransmissionVideoFrame FromUDPPacket(byte[] packet)
    {
        using var stream = new MemoryStream(packet, 0, HeaderSize);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        //NOTE: this check also moves the reader beyond the type byte
        var packetType = reader.ReadByte();
        if (packetType != (byte)PacketType.VideoStreamPacket)
        {
            throw new Exception($"Trying to decode a {(PacketType) packetType} as a {PacketType.VideoStreamPacket}");
        }

        var ret = new TransmissionVideoFrame()
        {
            PacketSize = reader.ReadUInt16(),
            TimeStamp = reader.ReadInt64(),
            PacketId = reader.ReadUInt32(),
            Width = reader.ReadInt16(),
            Height = reader.ReadInt16(),
            SequenceIDX = reader.ReadUInt32(),
            NumberOfPackets = reader.ReadByte(),
            PacketIdx = reader.ReadByte(),
            FrameDataSize = reader.ReadInt32(),
            flagsByte = reader.ReadByte(),
        };

        ret.flags = BooleanArrayUtils.ConvertByteToBoolArray(ret.flagsByte);
        
        return ret;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Partial Frame: SeqIDX: {SequenceIDX} | Packet{PacketIdx+1}/{NumberOfPackets}";
    }
}
