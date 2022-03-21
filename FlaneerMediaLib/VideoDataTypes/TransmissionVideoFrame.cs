using System.Text;

namespace FlaneerMediaLib;

public class TransmissionVideoFrame : VideoFrame
{
    //Int32 will always be sufficient since it will fit into UDP packet
    public bool IsPartial;
    public byte PacketIdx;
    public Int32 FrameDataSize;

    public byte[] ToUDPPacket()
    {
        using MemoryStream stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(IsPartial);
        writer.Write(PacketIdx);
        writer.Write(FrameDataSize);

        return stream.GetBuffer();
    }
}