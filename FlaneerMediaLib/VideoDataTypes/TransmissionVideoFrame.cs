using System.Text;

namespace FlaneerMediaLib;

public class TransmissionVideoFrame : VideoFrame
{
    //Int32 will always be sufficient since it will fit into UDP packet
    public byte SequenceIDX;
    public byte NumberOfPackets;
    public byte PacketIdx;
    public Int32 FrameDataSize;

    public const int HeaderSize = 11;
    
    public byte[] ToUDPPacket()
    {
        byte[] ret = new byte[HeaderSize];
        using MemoryStream stream = new MemoryStream(ret);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(SequenceIDX);
        writer.Write(NumberOfPackets);
        writer.Write(PacketIdx);
        writer.Write(FrameDataSize);
        
        return ret;
    }
    
    public static Tuple<TransmissionVideoFrame, byte[]> FromUDPPacket(byte[] packet)
    {
        using var stream = new MemoryStream(packet);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var frame = new TransmissionVideoFrame()
        {
            Width = reader.ReadInt16(),
            Height = reader.ReadInt16(),
            SequenceIDX = reader.ReadByte(),
            NumberOfPackets = reader.ReadByte(),
            PacketIdx = reader.ReadByte(),
            FrameDataSize = reader.ReadInt32()
        };
        return new Tuple<TransmissionVideoFrame, byte[]>(frame, packet);
    }
}