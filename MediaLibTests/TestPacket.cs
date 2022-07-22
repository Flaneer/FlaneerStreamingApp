using System;
using System.IO;
using System.Text;
using FlaneerMediaLib;

namespace MediaLibTests;

public class TestPacket : IPacketInfo
{
    public PacketType PacketType => PacketType.TestPacket;
    public ushort PacketSize { get; set; }
    public long TimeStamp { get; init; }
    public uint PacketId { get; init; }

    private readonly int HeaderSize = 15;
    
    public byte[] ToUDPPacket()
    {
        byte[] ret = new byte[HeaderSize];
        using MemoryStream stream = new MemoryStream(ret);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write((byte) PacketType);
        writer.Write(PacketSize);
        writer.Write(DateTime.UtcNow.Ticks);
        writer.Write(PacketId);
        
        return ret;
    }
}
