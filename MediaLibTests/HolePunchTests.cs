using System;
using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using Xunit;

namespace MediaLibTests;

public class HolePunchTests
{
    [Fact]
    public void TestHeaderSize()
    {
        HolePunchInfoPacket holePunchInfoPacket = new HolePunchInfoPacket(NodeType.StreamingClient, UInt16.MinValue);
        var packet = holePunchInfoPacket.ToUDPPacket();
        Assert.Equal(HolePunchInfoPacket.HeaderSize, packet.Length);
    }

    [Fact]
    public void TestToFromUDPPacket()
    {
        HolePunchInfoPacket holePunchInfoPacket = new HolePunchInfoPacket(NodeType.StreamingClient, UInt16.MinValue);
        var packetIn = holePunchInfoPacket.ToUDPPacket();
        var packetOut = HolePunchInfoPacket.FromBytes(packetIn);
        
        Assert.Equal(holePunchInfoPacket.PacketType, packetOut.PacketType);
        Assert.Equal(holePunchInfoPacket.PacketSize, packetOut.PacketSize);
        Assert.Equal(holePunchInfoPacket.PacketId, packetOut.PacketId);
        Assert.Equal(holePunchInfoPacket.ConnectionId, packetOut.ConnectionId);
        Assert.Equal(holePunchInfoPacket.NodeType, packetOut.NodeType);
        //Check the timestamp is within 1 second of the current time, it is encoded when ToUDPPacket is called so is not in the original packet
        Assert.True(DateTime.UtcNow.Ticks - packetOut.TimeStamp < TimeSpan.TicksPerSecond);
    }
}
